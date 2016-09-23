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
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whisper.Shared.Net
{
    public abstract class ServerBase<TSession, TRequest, TStatus> : AppServer<TSession, TRequest>, IWhisperServer<TSession, TRequest>
        where TSession : SessionBase<TSession, TRequest, TStatus>, new()
        where TRequest : class, IWhisperRequest
    {
        private readonly IDictionary<string, IWhisperCommand<TSession, TRequest>> commandDictionary;

        public IDictionary<string, IWhisperCommand<TSession, TRequest>> RegisteredCommands
        {
            get;
            private set;
        }

        public ServerBase(IReceiveFilterFactory<TRequest> receiveFilterFactory) : base(receiveFilterFactory)
        {
            commandDictionary = new Dictionary<string, IWhisperCommand<TSession, TRequest>>();
        }

        protected override bool SetupCommands(Dictionary<string, ICommand<TSession, TRequest>> discoveredCommands)
        {
            if (base.SetupCommands(discoveredCommands))
            {
                foreach (var entry in discoveredCommands)
                {
                    if (entry.Value is IWhisperCommand<TSession, TRequest>)
                        commandDictionary.Add(entry.Key, entry.Value as IWhisperCommand<TSession, TRequest>);
                }

                RegisteredCommands = new ReadOnlyDictionary<string, IWhisperCommand<TSession, TRequest>>(commandDictionary);
                return true;
            }

            return false;
        }
    }
}
