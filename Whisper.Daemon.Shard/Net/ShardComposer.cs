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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Security;
using Whisper.Shared.Net;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Net
{
    public class ShardComposer : ComposerBase<ShardSession, ShardRequest, ShardClientOpcode, SessionStatus>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ShardComposer));

        protected readonly PacketCipher cipher;

        public ShardComposer(IWhisperRequestFactory<ShardRequest, ShardClientOpcode> requestFactory, IDictionary<string, IWhisperCommand<ShardSession, ShardRequest>> commandDictionary, PacketCipher cipher) : base(requestFactory, commandDictionary)
        {
            this.cipher = cipher;
        }

        public override ShardRequest Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            if (length < Marshal.SizeOf<ClientPacketHeader>())
            {
                if (log.IsWarnEnabled)
                {
                    string contents = Strings.HexOf(readBuffer, offset, length);
                    log.WarnFormat("Filter received packet size {0} less than minimum of {1}. packet contents = 0x{2}", length, Marshal.SizeOf<ClientPacketHeader>(), contents);
                }

                rest = 0;
                return null;
            }

            cipher.DecryptHeader(new ArraySegment<byte>(readBuffer, offset, length));
            return base.Filter(readBuffer, offset, length, toBeCopied, out rest);
        }

        protected override ShardClientOpcode GetUnimplementedOpcodeSubstitute(ShardClientOpcode unimplementedOpcode)
        {
            // log it and return a no-op
            log.WarnFormat("received unimplemented opcode 0x{0:x4}", (ushort)unimplementedOpcode);
            return ShardClientOpcode.Noop;
        }

        public override int OpcodeSize
        {
            get
            {
                return sizeof(uint);
            }
        }

        public override int OpcodeOffset
        {
            get
            {
                return 2;
            }
        }
    }
}
