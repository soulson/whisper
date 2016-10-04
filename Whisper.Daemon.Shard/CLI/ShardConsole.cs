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
using Whisper.Shared.CLI;

namespace Whisper.Daemon.Shard.CLI
{
    public class ShardConsole : ConsoleBase<ShardServer, ShardSession, ShardRequest>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ShardConsole));

        private volatile bool running;

        public ShardConsole(ShardServer server) : base(server)
        {
            
        }

        public override void Run()
        {
            Thread shardPinger = new Thread(() => PingShardsTable());
            shardPinger.Name = "shard pinger";

            running = true;
            shardPinger.Start();

            base.Run();
            running = false;

            shardPinger.Interrupt();
            shardPinger.Join();
        }

        private void PingShardsTable()
        {
            log.Info("starting shard pinger thread");

            try
            {
                while (running)
                {
                    int result = Server.AuthDB.ExecuteNonQuery("update shard set last_ping = ? where id = ?", DateTime.Now, Server.AppConfig.ShardID);
                    if (result != 1)
                        log.ErrorFormat("expected to update 1 row with ping but updated {0}. this is likely due to a mismatch between ShardID in shardd.config and wauth.shard.id", result);

                    Thread.Sleep(Server.AppConfig.ShardPingMilliseconds);
                }
            }
            catch (ThreadInterruptedException)
            {
                // nothing to do here. this is normal behavior for stopping this thread when it's asleep
            }

            log.Info("stopping shard pinger thread");
        }
    }
}
