﻿/* 
 * This file is part of the whisper project.
 * Copyright (C) 2016  soulson (a.k.a. foxic)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using log4net;
using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Net;
using Whisper.Shared.Security;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Commands
{
    public sealed class AuthSessionCommand : ShardCommandBase<ClientPacketHeader>
    {
        private const int StandardAddonCRC = 0x1c776d01;

        private readonly ILog log = LogManager.GetLogger(typeof(AuthSessionCommand));

        public override string Name
        {
            get
            {
                return ShardClientOpcode.AuthSession.ToString();
            }
        }

        public override SessionStatus PermissibleStatus
        {
            get
            {
                return SessionStatus.Connected;
            }
        }

        public override CommandThreadSafety ThreadSafety
        {
            get
            {
                return CommandThreadSafety.Immediate;
            }
        }

        public override void ExecuteCommand(ShardSession session, ShardRequest request, ClientPacketHeader header)
        {
            int clientBuild = BitConverter.ToInt32(request.Packet, 6);
            int identityLength;
            string identity = Strings.FromNullTerminated(request.Packet, 14, Encoding.UTF8, out identityLength);
            int clientSeed = BitConverter.ToInt32(request.Packet, 14 + identityLength + 1); // + 1 for null terminator
            byte[] clientDigest = new byte[20];
            Array.Copy(request.Packet, 14 + identityLength + 1 + 4, clientDigest, 0, 20);

            log.DebugFormat("read {0} packet. client build = {1}, identity = {2}, client seed = {3:X}, packet size = {4}", header.Opcode, clientBuild, identity, clientSeed, request.Size);

            int accountId = -1;
            string sessionKey = null;
            if(!session.Server.AuthDB.ExecuteQuery("select name, session_key, id from account where name = ? and enabled = true", identity, result =>
            {
                if (result.Read())
                {
                    identity = result.GetString(0);
                    sessionKey = result.GetString(1);
                    accountId = result.GetInt32(2);
                    return true;
                }
                return false;
            }))
            {
                // no identity match
                log.DebugFormat("account {0} not found. sending {1} response", identity, AuthResponse.UnknownAccount);
                session.Send(ShardServerOpcode.AuthResponse, new byte[] { (byte)AuthResponse.UnknownAccount });
                return;
            }

            BigInteger sessionKeyInt = BigInteger.Parse(sessionKey, NumberStyles.AllowHexSpecifier);

            // verify client/server identity and session key match
            using (Digester sha1 = new Digester(new SHA1CryptoServiceProvider()))
            {
                byte[] serverDigest = sha1.CalculateDigest(new byte[][]
                {
                    Encoding.UTF8.GetBytes(identity),
                    new byte[4],
                    BitConverter.GetBytes(clientSeed),
                    BitConverter.GetBytes(session.Seed),
                    Arrays.Left(sessionKeyInt.ToByteArray(), 40)
                });

                log.DebugFormat("client digest = {0}", Strings.HexOf(clientDigest));
                log.DebugFormat("server digest = {0}", Strings.HexOf(serverDigest));

                if (Arrays.AreEqual(clientDigest, serverDigest))
                {
                    log.InfoFormat("client {0} successfully authenticated from {1}", identity, session.RemoteEndPoint.Address.ToString());

                    session.AccountID = accountId;
                    session.Status = SessionStatus.Authenticated;
                    session.Cipher.Initialize(sessionKeyInt);

                    using (ByteBuffer authPacket = new ByteBuffer())
                    {
                        authPacket.Append((byte)AuthResponse.Success);
                        authPacket.Append(0);
                        authPacket.Append((byte)0);
                        authPacket.Append(0);

                        session.Send(ShardServerOpcode.AuthResponse, authPacket);
                    }

                    using (ByteBuffer addonPacket = BuildAddonPacket(new ArraySegment<byte>(request.Packet, 14 + identityLength + 1 + 4 + 20, request.Size - (14 + identityLength + 1 + 4 + 20))))
                        session.Send(ShardServerOpcode.AddonInfo, addonPacket);
                }
                else
                {
                    // digest mismatch
                    log.DebugFormat("client digest did not match server digest for account {0}. sending {1} response", identity, AuthResponse.Failed);
                    session.Send(ShardServerOpcode.AuthResponse, new byte[] { (byte)AuthResponse.Failed });
                    return;
                }
            }
        }

        private ByteBuffer BuildAddonPacket(ArraySegment<byte> clientData)
        {
            if (clientData.Count < sizeof(int))
                throw new ArgumentException("client sent invalid addon packet. no actualSize field");

            int actualSize = BitConverter.ToInt32(clientData.Array, clientData.Offset);
            if (actualSize <= 0 || actualSize > 0xfffff)
                throw new ArgumentException(string.Format("client sent addon packet with bad actualSize [{0}]", actualSize));

            byte[] data = new byte[actualSize];

            // the +2/-2 is for the zlib header (which is not part of deflate, but they are otherwise equivalent)
            using (DeflateStream stream = new DeflateStream(new MemoryStream(clientData.Array, clientData.Offset + sizeof(int) + 2, clientData.Count - sizeof(int) - 2, false), CompressionMode.Decompress, false))
            {
                int result = stream.Read(data, 0, actualSize);

                if (result != actualSize)
                    throw new Exception(string.Format("number of bytes decompressed [{0}] did not equal expected value [{1}]", result, actualSize));
            }

            ByteBuffer addonPacket = new ByteBuffer();
            
            int offset = 0;
            while(offset < actualSize)
            {
                int len;
                string addonName = Strings.FromNullTerminated(data, offset, Encoding.UTF8, out len);
                offset += len + 1; // + 1 for null terminator
                int crc = BitConverter.ToInt32(data, offset);
                offset += sizeof(int) + 5;

                bool addonIsStandard = crc == StandardAddonCRC;
                log.DebugFormat("building addon packet block for '{0}'. addonIsStandard = {1}", addonName, addonIsStandard);

                addonPacket.Append((byte)2);
                addonPacket.Append((byte)1);

                addonPacket.Append(addonIsStandard ? (byte)0 : (byte)1);
                if (!addonIsStandard)
                    addonPacket.Append(MysteryAddonArray);

                addonPacket.Append(0);
                addonPacket.Append((byte)0);
            }

            return addonPacket;
        }

        private static readonly byte[] MysteryAddonArray =
        {
            0xC3, 0x5B, 0x50, 0x84, 0xB9, 0x3E, 0x32, 0x42, 0x8C, 0xD0, 0xC7, 0x48, 0xFA, 0x0E, 0x5D, 0x54,
            0x5A, 0xA3, 0x0E, 0x14, 0xBA, 0x9E, 0x0D, 0xB9, 0x5D, 0x8B, 0xEE, 0xB6, 0x84, 0x93, 0x45, 0x75,
            0xFF, 0x31, 0xFE, 0x2F, 0x64, 0x3F, 0x3D, 0x6D, 0x07, 0xD9, 0x44, 0x9B, 0x40, 0x85, 0x59, 0x34,
            0x4E, 0x10, 0xE1, 0xE7, 0x43, 0x69, 0xEF, 0x7C, 0x16, 0xFC, 0xB4, 0xED, 0x1B, 0x95, 0x28, 0xA8,
            0x23, 0x76, 0x51, 0x31, 0x57, 0x30, 0x2B, 0x79, 0x08, 0x50, 0x10, 0x1C, 0x4A, 0x1A, 0x2C, 0xC8,
            0x8B, 0x8F, 0x05, 0x2D, 0x22, 0x3D, 0xDB, 0x5A, 0x24, 0x7A, 0x0F, 0x13, 0x50, 0x37, 0x8F, 0x5A,
            0xCC, 0x9E, 0x04, 0x44, 0x0E, 0x87, 0x01, 0xD4, 0xA3, 0x15, 0x94, 0x16, 0x34, 0xC6, 0xC2, 0xC3,
            0xFB, 0x49, 0xFE, 0xE1, 0xF9, 0xDA, 0x8C, 0x50, 0x3C, 0xBE, 0x2C, 0xBB, 0x57, 0xED, 0x46, 0xB9,
            0xAD, 0x8B, 0xC6, 0xDF, 0x0E, 0xD6, 0x0F, 0xBE, 0x80, 0xB3, 0x8B, 0x1E, 0x77, 0xCF, 0xAD, 0x22,
            0xCF, 0xB7, 0x4B, 0xCF, 0xFB, 0xF0, 0x6B, 0x11, 0x45, 0x2D, 0x7A, 0x81, 0x18, 0xF2, 0x92, 0x7E,
            0x98, 0x56, 0x5D, 0x5E, 0x69, 0x72, 0x0A, 0x0D, 0x03, 0x0A, 0x85, 0xA2, 0x85, 0x9C, 0xCB, 0xFB,
            0x56, 0x6E, 0x8F, 0x44, 0xBB, 0x8F, 0x02, 0x22, 0x68, 0x63, 0x97, 0xBC, 0x85, 0xBA, 0xA8, 0xF7,
            0xB5, 0x40, 0x68, 0x3C, 0x77, 0x86, 0x6F, 0x4B, 0xD7, 0x88, 0xCA, 0x8A, 0xD7, 0xCE, 0x36, 0xF0,
            0x45, 0x6E, 0xD5, 0x64, 0x79, 0x0F, 0x17, 0xFC, 0x64, 0xDD, 0x10, 0x6F, 0xF3, 0xF5, 0xE0, 0xA6,
            0xC3, 0xFB, 0x1B, 0x8C, 0x29, 0xEF, 0x8E, 0xE5, 0x34, 0xCB, 0xD1, 0x2A, 0xCE, 0x79, 0xC3, 0x9A,
            0x0D, 0x36, 0xEA, 0x01, 0xE0, 0xAA, 0x91, 0x20, 0x54, 0xF0, 0x72, 0xD8, 0x1E, 0xC7, 0x89, 0xD2
        };
    }
}
