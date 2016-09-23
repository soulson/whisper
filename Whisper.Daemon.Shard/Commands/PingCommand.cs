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
using System.Runtime.InteropServices;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Net;

namespace Whisper.Daemon.Shard.Commands
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ping
    {
        private ClientPacketHeader Header;
        public int Cookie;
        public int Latency;
    }

    public sealed class PingCommand : ShardCommandBase<Ping>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(PingCommand));

        public override string Name
        {
            get
            {
                return ShardClientOpcode.Ping.ToString();
            }
        }

        public override SessionStatus PermissibleStatus
        {
            get
            {
                return SessionStatus.Connected | SessionStatus.Authenticated | SessionStatus.Ingame;
            }
        }

        public override CommandThreadSafety ThreadSafety
        {
            get
            {
                return CommandThreadSafety.Immediate;
            }
        }

        public override void ExecuteCommand(ShardSession session, ShardRequest request, Ping header)
        {
            log.DebugFormat("client sends ping with latency = {0}", header.Latency);
            session.Send(ShardServerOpcode.Pong, BitConverter.GetBytes(header.Cookie));
        }
    }
}
