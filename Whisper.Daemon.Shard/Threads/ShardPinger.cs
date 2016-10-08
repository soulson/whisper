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
using Whisper.Shared.Database;

namespace Whisper.Daemon.Shard.Threads
{
    public class ShardPinger
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ShardPinger));

        private readonly ShardConfig config;
        private readonly IWhisperDatasource authDatasource;

        public ShardPinger(IWhisperDatasource wauth, ShardConfig config)
        {
            this.config = config;
            authDatasource = wauth;
        }

        public void Run()
        {
            log.Info("starting shard pinger");
            
            try
            {
                while (true)
                {
                    int result = authDatasource.ExecuteNonQuery("update shard set last_ping = ? where id = ?", DateTime.Now, config.ShardID);
                    if (result != 1)
                        log.ErrorFormat("expected to update 1 row with ping but updated {0}. this is likely due to a mismatch between ShardID in shardd.config and wauth.shard.id", result);

                    Thread.Sleep(config.ShardPingMilliseconds);
                }
            }
            catch (ThreadInterruptedException)
            {
                // nothing to do here. this is normal behavior for stopping this thread when it's asleep
            }

            log.Info("stopping shard pinger");
        }
    }
}
