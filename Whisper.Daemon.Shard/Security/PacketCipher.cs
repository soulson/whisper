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
using System.Numerics;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Security
{
    public sealed class PacketCipher
    {
        private const int KeySize = 40;
        private const int SendLength = 4;
        private const int ReceiveLength = 6;

        private readonly ILog log = LogManager.GetLogger(typeof(PacketCipher));

        private bool initialized;
        private byte[] sessionKey;

        private byte sendI, sendJ;
        private byte recvI, recvJ;

        public PacketCipher()
        {
            initialized = false;
        }

        public void Initialize(BigInteger sessionKey)
        {
            this.sessionKey = Arrays.Left(sessionKey.ToByteArray(), KeySize);
            sendI = sendJ = 0;
            recvI = recvJ = 0;
            initialized = true;
        }

        public void EncryptHeader(ArraySegment<byte> packet)
        {
            if (!initialized)
                return;

            if (packet.Count < SendLength)
                throw new ArgumentException(string.Format("packet to encrypt must be at least {0} bytes in length", SendLength));

            for (int t = 0; t < SendLength; ++t)
            {
                sendI %= KeySize;
                byte x = unchecked((byte)((packet.Array[t + packet.Offset] ^ sessionKey[sendI]) + sendJ));
                ++sendI;
                packet.Array[t + packet.Offset] = sendJ = x;
            }
        }

        public void DecryptHeader(ArraySegment<byte> packet)
        {
            if (!initialized)
                return;

            if (packet.Count < ReceiveLength)
                throw new ArgumentException(string.Format("packet to decrypt must be at least {0} bytes in length", ReceiveLength));

            for (int t = 0; t < ReceiveLength; ++t)
            {
                recvI %= KeySize;
                byte x = unchecked((byte)((packet.Array[t + packet.Offset] - recvJ) ^ sessionKey[recvI]));
                ++recvI;
                recvJ = packet.Array[t + packet.Offset];
                packet.Array[t + packet.Offset] = x;
            }
        }
    }
}
