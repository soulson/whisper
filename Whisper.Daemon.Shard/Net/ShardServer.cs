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

        public ShardServer(IWhisperComposerFactory<ShardRequest> composerFactory, IRandomSource randomSource, IWhisperDatasource wauth, IWhisperDatasource wshard, WorldStore worldStore, ShardConfig configuration) : base(composerFactory)
        {
            AppConfig = configuration;
            SecureRandom = randomSource;
            AuthDB = wauth;
            ShardDB = wshard;
            WorldStore = worldStore;
        }

        public IWhisperDatasource AuthDB
        {
            get;
            private set;
        }

        public IWhisperDatasource ShardDB
        {
            get;
            private set;
        }

        public ShardConfig AppConfig
        {
            get;
            private set;
        }

        public IRandomSource SecureRandom
        {
            get;
            private set;
        }

        public WorldStore WorldStore
        {
            get;
            private set;
        }
    }
}
