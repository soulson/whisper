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
using System.Collections.Concurrent;
using Whisper.Daemon.Shard.Config;
using Whisper.Game.World;
using Whisper.Shared.Database;
using Whisper.Shared.Net;
using Whisper.Shared.Security;

namespace Whisper.Daemon.Shard.Net
{
    public class ShardServer : ServerBase<ShardSession, ShardRequest, SessionStatus>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ShardServer));
        private readonly ConcurrentQueue<CommandClosure> commandQueue = new ConcurrentQueue<CommandClosure>();

        public ShardServer(IWhisperComposerFactory<ShardRequest> composerFactory, IRandomSource randomSource, IWhisperDatasource wauth, IWhisperDatasource wshard, World world, Game.World.Shard shard, ShardConfig configuration) : base(composerFactory)
        {
            AppConfig = configuration;
            SecureRandom = randomSource;
            AuthDB = wauth;
            ShardDB = wshard;
            World = world;
            Shard = shard;
        }

        /// <summary>
        /// Gets a datasource for the authentication database.
        /// </summary>
        public IWhisperDatasource AuthDB
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a datasource for the shard database.
        /// </summary>
        public IWhisperDatasource ShardDB
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the configuration for this server.
        /// </summary>
        public ShardConfig AppConfig
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a cryptographically secure random number generator.
        /// </summary>
        public IRandomSource SecureRandom
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the World associated with this server.
        /// </summary>
        public World World
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the Shard associated with this server.
        /// </summary>
        public Game.World.Shard Shard
        {
            get;
            private set;
        }

        /// <summary>
        /// Executes Immediate commands and enqueues unsafe commands.
        /// </summary>
        /// <param name="session">session that sent the command</param>
        /// <param name="requestInfo">request that tells us which command to execute</param>
        /// <seealso cref="Whisper.Daemon.Shard.Net.ShardServer.ExecuteQueuedCommand"/>
        protected override void ExecuteCommand(ShardSession session, ShardRequest requestInfo)
        {
            IWhisperCommand<ShardSession, ShardRequest> command;
            if (RegisteredCommands.TryGetValue(requestInfo.Key, out command))
            {
                // best to assume the worst, right?
                CommandThreadSafety safety = CommandThreadSafety.NotThreadSafe;

                if (command is IThreadAwareCommand)
                    safety = (command as IThreadAwareCommand).ThreadSafety;

                if(safety == CommandThreadSafety.Immediate)
                {
                    // execute Immediate commands immediately
                    base.ExecuteCommand(session, requestInfo);
                }
                else
                {
                    // split remaining commands by thread safety. thread-safe commands are executed on a thread owned by the session that received them; thread-unsafe commands are executed on the server update thread
                    CommandClosure closure = () => base.ExecuteCommand(session, requestInfo);

                    if (safety == CommandThreadSafety.ThreadSafe)
                        session.EnqueueCommand(closure);
                    else
                        EnqueueCommand(closure);
                }
            }
            // no need to log the 'else' case here. unknown packet notifications are handled by the Composer
        }

        /// <summary>
        /// Executes all queued commands synchronously on the currently executing Thread.
        /// </summary>
        public void ProcessQueuedCommands()
        {
            CommandClosure command;
            while (commandQueue.TryDequeue(out command))
            {
                try
                {
                    ExecuteQueuedCommand(command);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("{0} uncaught during ProcessQueuedCommands{1}{2}", ex.GetType().Name, Environment.NewLine, ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Executes a command from the command queue.
        /// </summary>
        /// <param name="command">closure for the command to execute</param>
        protected virtual void ExecuteQueuedCommand(CommandClosure command)
        {
            command();
        }

        /// <summary>
        /// Enqueues a command to be executed on the next update tick.
        /// </summary>
        /// <param name="command">command to execute</param>
        public void EnqueueCommand(CommandClosure command)
        {
            commandQueue.Enqueue(command);
        }
    }
}
