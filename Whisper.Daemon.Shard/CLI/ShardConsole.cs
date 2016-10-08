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
using System.Threading;
using Whisper.Daemon.Shard.Net;
using Whisper.Daemon.Shard.Threads;
using Whisper.Shared.CLI;

namespace Whisper.Daemon.Shard.CLI
{
    public class ShardConsole : ConsoleBase<ShardServer, ShardSession, ShardRequest>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ShardConsole));

        public ShardConsole(ShardServer server) : base(server)
        {
        }

        public override void Run()
        {
            Thread shardPinger = new Thread(new ShardPinger(Server.AuthDB, Server.AppConfig).Run);
            shardPinger.Name = "shard pinger";

            Thread shardUpdater = new Thread(new ShardUpdater(Server, Server.AppConfig).Run);
            shardUpdater.Name = "shard updater";

            shardUpdater.Start();
            shardPinger.Start();

            base.Run();

            shardPinger.Interrupt();
            shardUpdater.Interrupt();

            shardPinger.Join();
            shardUpdater.Join();
        }
    }
}
