/* 
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
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Whisper.Daemon.Auth.Lookup;
using Whisper.Daemon.Auth.Net;
using Whisper.Shared.Net;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Auth.Commands
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LogonChallenge
    {
        public AuthRequestOpcode Opcode;
        private byte unk0;
        public ushort Size;
        public fixed byte GameName[4];
        public byte Version1;
        public byte Version2;
        public byte Version3;
        public short Build;
        public fixed byte Platform[4];
        public fixed byte OS[4];
        public fixed byte Country[4];
        public int TimezoneBias;
        public int IP;
        public byte ILength;
    }

    public sealed class LogonChallengeCommand : CommandBase<LogonChallenge, AuthSession, AuthRequest, AuthStatus>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(LogonChallengeCommand));

        public override string Name
        {
            get
            {
                return AuthRequestOpcode.LogonChallenge.ToString();
            }
        }

        public override AuthStatus PermissibleStatus
        {
            get
            {
                return AuthStatus.Connected;
            }
        }

        public override void ExecuteCommand(AuthSession session, AuthRequest requestInfo, LogonChallenge header)
        {
            log.DebugFormat("handling {0} opcode. build = {1}", header.Opcode, header.Build);
            
            // identity is in a UTF8 string immediately following the packet header
            string I = Encoding.UTF8.GetString(requestInfo.Packet, Marshal.SizeOf<LogonChallenge>(), header.ILength);
            string s = null;
            string v = null;

            // get account srp information from database. doubles as account existence check
            if (session.Server.AuthDB.ExecuteQuery("select name, salt, verifier from account where name = ? and enabled = true", I, reader =>
            {
                if (reader.Read())
                {
                    I = reader.GetString(0);
                    s = reader.GetString(1);
                    v = reader.GetString(2);
                    return true; // account found
                }
                return false; // account not found
            }))
            {
                // account found case
                byte gLength = 1;
                byte NLength = 32;
                BigInteger unk3 = session.Server.SecureRandom.GetRandomBigInteger(16);
                byte securityFlags = 0;

                session.SRP.I = I;
                session.SRP.s = BigInteger.Parse(s, NumberStyles.AllowHexSpecifier);
                session.SRP.v = BigInteger.Parse(v, NumberStyles.AllowHexSpecifier);
                session.SRP.b = session.Server.SecureRandom.GetRandomBigInteger(19);
                session.SRP.B = ((session.SRP.v * session.SRP.k) + BigInteger.ModPow(session.SRP.g, session.SRP.b, session.SRP.N)) % session.SRP.N;

                using (ByteBuffer response = new ByteBuffer())
                {
                    response.Append((byte)AuthRequestOpcode.LogonChallenge);
                    response.Append((byte)0);
                    response.Append((byte)AuthResponseOpcode.Success);
                    response.Append(Arrays.Left(session.SRP.B.ToByteArray(), 32));
                    response.Append(gLength);
                    response.Append(Arrays.Left(session.SRP.g.ToByteArray(), gLength));
                    response.Append(NLength);
                    response.Append(Arrays.Left(session.SRP.N.ToByteArray(), NLength));
                    response.Append(Arrays.Left(session.SRP.s.ToByteArray(), 32));
                    response.Append(Arrays.Left(unk3.ToByteArray(), 16));
                    response.Append(securityFlags);

                    session.Send(response.GetArraySegment());
                }
                log.DebugFormat("user '{0}' handshake sent", session.SRP.I);
            }
            else
            {
                // account not found case
                using (ByteBuffer response = new ByteBuffer())
                {
                    response.Append((byte)AuthRequestOpcode.LogonChallenge);
                    response.Append((byte)0);
                    response.Append((byte)AuthResponseOpcode.FailBadCredentials);

                    session.Send(response.GetArraySegment());
                }
                log.InfoFormat("unknown or inactive user '{0}' logon attempt", I);
            }
        }

        public override int GetPacketSize(ArraySegment<byte> buffer)
        {
            return BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2) + 4;
        }
    }
}
