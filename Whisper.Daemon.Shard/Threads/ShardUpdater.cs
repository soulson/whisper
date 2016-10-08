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
using Whisper.Daemon.Shard.Config;
using Whisper.Daemon.Shard.Net;

namespace Whisper.Daemon.Shard.Threads
{
    public class ShardUpdater
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ShardUpdater));

        private readonly ShardConfig config;
        private readonly ShardServer server;

        public ShardUpdater(ShardServer server, ShardConfig config)
        {
            this.server = server;
            this.config = config;
        }

        public void Run()
        {
            log.Info("starting shard updater");

            try
            {
                // initialize the shard
                DateTime lastUpdate = DateTime.Now;
                server.Shard.Initialize();

                while (true)
                {
                    // sleep first since initial update is done outside the loop
                    Thread.Sleep(config.ShardUpdateMilliseconds);

                    // calculate diff
                    DateTime updateTime = DateTime.Now;
                    TimeSpan diff = updateTime - lastUpdate;

                    // process all thread-unsafe queued commands
                    server.ProcessQueuedCommands();

                    // update the game world
                    server.Shard.Update(diff);

                    // update each session whose status is Ingame
                    foreach (ShardSession session in server.GetSessions((ss) => ss.Status == SessionStatus.Ingame))
                        session.Update(diff);

                    // clear update masks, so any new GameObject updates will be sent during the next set of session updates and we don't repeat any from this tick
                    server.Shard.ClearUpdateMasks();
                    lastUpdate = updateTime;
                }
            }
            catch (ThreadInterruptedException)
            {
                // nothing to do here. this is normal behavior for stopping this thread when it's asleep
            }

            log.Info("stopping shard updater");

            // TODO: save shard state @ shutdown
        }
    }
}
