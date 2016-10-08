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

using SuperSocket.SocketBase;
using System;

namespace Whisper.Shared.Net
{
    public abstract class SessionBase<TSession, TRequest, TStatus> : AppSession<TSession, TRequest>, IWhisperSession<TSession, TRequest>
        where TSession : SessionBase<TSession, TRequest, TStatus>, new()
        where TRequest : class, IWhisperRequest
    {
        private TStatus status;
        private object statusLock;

        public SessionBase(TStatus initialStatus)
        {
            statusLock = new object();
            status = initialStatus;
        }

        public TStatus Status
        {
            get
            {
                lock (statusLock)
                    return status;
            }
            set
            {
                lock (statusLock)
                    status = value;
            }
        }
    }
}
