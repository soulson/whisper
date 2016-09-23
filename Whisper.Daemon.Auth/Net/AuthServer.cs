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
using Whisper.Daemon.Auth.Config;
using Whisper.Shared.Database;
using Whisper.Shared.Net;
using Whisper.Shared.Security;

namespace Whisper.Daemon.Auth.Net
{
    public class AuthServer : ServerBase<AuthSession, AuthRequest, AuthStatus>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(AuthServer));

        public AuthServer(IWhisperComposerFactory<AuthRequest> composerFactory, IRandomSource randomSource, IWhisperDatasource wauth, AuthConfig configuration) : base(composerFactory)
        {
            AppConfig = configuration;
            SecureRandom = randomSource;
            AuthDB = wauth;
        }

        public IRandomSource SecureRandom
        {
            get;
            private set;
        }

        public IWhisperDatasource AuthDB
        {
            get;
            private set;
        }

        public AuthConfig AppConfig
        {
            get;
            private set;
        }
    }
}
