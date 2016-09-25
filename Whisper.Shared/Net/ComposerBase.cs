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
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whisper.Shared.Net
{
    public abstract class ComposerBase<TSession, TRequest, TOpcode, TStatus> : ReceiveFilterBase<TRequest>, IWhisperComposer<TRequest>
        where TSession : SessionBase<TSession, TRequest, TStatus>, new()
        where TRequest : class, IWhisperRequest
        where TOpcode : struct
    {
        private readonly ILog log = LogManager.GetLogger("Whisper.Shared.Net.ComposerBase");

        protected readonly IWhisperRequestFactory<TRequest, TOpcode> requestFactory;
        protected readonly IDictionary<string, IWhisperCommand<TSession, TRequest>> commandDictionary;

        public ComposerBase(IWhisperRequestFactory<TRequest, TOpcode> requestFactory, IDictionary<string, IWhisperCommand<TSession, TRequest>> commandDictionary) : base()
        {
            this.requestFactory = requestFactory;

            if (commandDictionary.IsReadOnly)
                this.commandDictionary = commandDictionary;
            else
                this.commandDictionary = new ReadOnlyDictionary<string, IWhisperCommand<TSession, TRequest>>(commandDictionary);
        }

        public override TRequest Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            // two notes on this piece of mess:
            //  1. the double cast is necessary because C# does not know that TOpcode will be an enum, and that cannot be enforced. it is an unsafe cast
            //  2. it is not determined if cases 2, 4, and 8 should use signed or unsigned ints. correct if later determined to be wrong
            TOpcode opcode;
            switch (OpcodeSize)
            {
                case 1:
                    opcode = (TOpcode)(object)readBuffer[offset + OpcodeOffset];
                    break;
                case 2:
                    opcode = (TOpcode)(object)BitConverter.ToUInt16(readBuffer, offset + OpcodeOffset);
                    break;
                case 4:
                    opcode = (TOpcode)(object)BitConverter.ToUInt32(readBuffer, offset + OpcodeOffset);
                    break;
                case 8:
                    opcode = (TOpcode)(object)BitConverter.ToUInt64(readBuffer, offset + OpcodeOffset);
                    break;
                default:
                    throw new ArgumentException("size of TOpcode must be 1, 2, 4, or 8 bytes");
            }

            IWhisperCommand<TSession, TRequest> command;

            try
            {
                command = commandDictionary[opcode.ToString()];
            }
            catch (KeyNotFoundException)
            {
                command = commandDictionary[GetUnimplementedOpcodeSubstitute(opcode).ToString()];
            }

            int size = command.GetPacketSize(new ArraySegment<byte>(readBuffer, offset, length));
            rest = System.Math.Max(0, length - size);

            if (size > length)
                return null;

            log.DebugFormat("composing {0} packet of length {1}. rest = {2}", opcode, size, rest);

            byte[] packet = new byte[size];
            Array.Copy(readBuffer, offset, packet, 0, size);
            return requestFactory.CreateRequest(opcode, packet);
        }

        protected abstract TOpcode GetUnimplementedOpcodeSubstitute(TOpcode unimplementedOpcode);

        // in bytes
        public abstract int OpcodeSize
        {
            get;
        }

        public abstract int OpcodeOffset
        {
            get;
        }
    }
}
