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

using System;
using Whisper.Shared.Net;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Net
{
    public abstract class ShardCommandBase<THeader> : CommandBase<THeader, ShardSession, ShardRequest, SessionStatus>
        where THeader : struct
    {
        public abstract CommandThreadSafety ThreadSafety
        {
            get;
        }

        protected override bool IsMatchingStatus(SessionStatus sessionStatus, SessionStatus permissibleStatus)
        {
            // SessionStatus is a bitmask, so use masking instead of equality comparison
            return (sessionStatus & permissibleStatus) != 0;
        }

        public override int GetPacketSize(ArraySegment<byte> buffer)
        {
            // the ushort is the size field itself. reverse because the size is written in big-endian for some reason
            return sizeof(ushort) + BitConverter.ToUInt16(Arrays.Reverse(BitConverter.GetBytes(BitConverter.ToUInt16(buffer.Array, buffer.Offset))), 0);
        }
    }
}
