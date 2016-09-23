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
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Security;
using Whisper.Shared.Net;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Net
{
    public class ShardSession : SessionBase<ShardSession, ShardRequest, SessionStatus>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ShardSession));
        private readonly ILog packetLog = LogManager.GetLogger("PacketLog");

        public ShardSession() : base(SessionStatus.Connected)
        {
            Cipher = new PacketCipher();
            AccountID = -1;
        }

        public ShardServer Server
        {
            get
            {
                return (ShardServer)AppServer;
            }
        }

        public PacketCipher Cipher
        {
            get;
            private set;
        }

        public int Seed
        {
            get;
            private set;
        }

        public int AccountID
        {
            get;
            set;
        }

        public override void Send(ArraySegment<byte> segment)
        {
            if (packetLog.IsInfoEnabled && segment.Count >= 4)
            {
                ShardServerOpcode opcode = (ShardServerOpcode)BitConverter.ToUInt16(segment.Array, segment.Offset + 2);
                ushort size = BitConverter.ToUInt16(new byte[] { segment.Array[segment.Offset + 1], segment.Array[segment.Offset] }, 0); // big endian
                string hexDump = Strings.HexDump(segment.Array, segment.Offset + 4, size - sizeof(ushort)); // sizeof the opcode

                packetLog.InfoFormat("Server [{0}] -> Client [{1}]", LocalEndPoint.ToString(), RemoteEndPoint.ToString());
                packetLog.InfoFormat("Opcode: {0} [0x{1:x4}]", opcode, (ushort)opcode);
                packetLog.InfoFormat("Length: {0}", size - sizeof(ushort));
                packetLog.InfoFormat("{1}{0}{1}", hexDump, Environment.NewLine);
            }

            Cipher.EncryptHeader(segment);
            base.Send(segment);
        }

        public override void Send(byte[] data, int offset, int length)
        {
            Send(new ArraySegment<byte>(data, offset, length));
        }

        public virtual void Send(ShardServerOpcode opcode, ByteBuffer packet)
        {
            Send(opcode, packet.GetBytes());
        }

        public virtual void Send(ShardServerOpcode opcode, byte responseCode)
        {
            Send(opcode, new byte[] { responseCode });
        }

        public virtual void Send(ShardServerOpcode opcode, byte[] packet)
        {
            using (ByteBuffer output = new ByteBuffer())
            {
                // the extra ushort is the opcode
                output.Append(Arrays.Reverse(BitConverter.GetBytes((ushort)(packet.Length + sizeof(ushort)))));
                output.Append((ushort)opcode);
                output.Append(packet);

                log.DebugFormat("sending {0} packet to client. size is {1} bytes including header", opcode, output.Size);

                Send(output.GetArraySegment());
            }
        }

        protected override void OnSessionStarted()
        {
            base.OnSessionStarted();

            // can't initialize this in constructor because Server reference is null at that time
            Seed = Server.SecureRandom.GetRandomInt();

            // start the session by challenging the client for authentication
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append(Seed);
                packet.Append(Server.SecureRandom.GetRandomBytes(32));
                
                Send(ShardServerOpcode.AuthChallenge, packet);
            }
        }
    }
}
