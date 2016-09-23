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
using System.Runtime.InteropServices;

namespace Whisper.Shared.Net
{
    public abstract class CommandBase<THeader, TSession, TRequest, TStatus> : IWhisperCommand<TSession, TRequest>
        where THeader : struct
        where TSession : SessionBase<TSession, TRequest, TStatus>, new()
        where TRequest : class, IWhisperRequest
    {
        public abstract string Name
        {
            get;
        }

        public abstract TStatus PermissibleStatus
        {
            get;
        }

        public void ExecuteCommand(TSession session, TRequest requestInfo)
        {
            if (Marshal.SizeOf<THeader>() > requestInfo.Size)
                throw new ArgumentException("requestInfo has payload smaller than opcode expects");

            if (!IsMatchingStatus(session.Status, PermissibleStatus))
                throw new InvalidOperationException(string.Format("received {0} packet for session in unexpected {1} = {2}. should be {3}", Name, typeof(TStatus).Name, session.Status, PermissibleStatus));

            THeader payload;
            byte[] packet = requestInfo.Packet;

            GCHandle handle = GCHandle.Alloc(packet, GCHandleType.Pinned);
            try
            {
                payload = Marshal.PtrToStructure<THeader>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }

            ExecuteCommand(session, requestInfo, payload);
        }

        protected virtual bool IsMatchingStatus(TStatus sessionStatus, TStatus permissibleStatus)
        {
            return sessionStatus.Equals(permissibleStatus);
        }

        public abstract void ExecuteCommand(TSession session, TRequest request, THeader header);

        public virtual int GetPacketSize(ArraySegment<byte> buffer)
        {
            return Marshal.SizeOf<THeader>();
        }
    }
}
