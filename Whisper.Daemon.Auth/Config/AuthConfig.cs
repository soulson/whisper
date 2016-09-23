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

using System;
using Westwind.Utilities.Configuration;

namespace Whisper.Daemon.Auth.Config
{
    [Serializable]
    public class AuthConfig : AppConfiguration
    {
        public AuthConfig() : base()
        {
            AuthDaemonBindAddress = "0.0.0.0";
            AuthDaemonBindPort = 3724;
            ShardOfflineTimeoutMilliseconds = 10000;
        }

        public string AuthDaemonBindAddress
        {
            get;
            set;
        }

        public int AuthDaemonBindPort
        {
            get;
            set;
        }

        public string AuthDatabaseAddress
        {
            get;
            set;
        }

        public int AuthDatabasePort
        {
            get;
            set;
        }

        public string AuthDatabaseName
        {
            get;
            set;
        }

        public string AuthDatabaseUser
        {
            get;
            set;
        }

        public string AuthDatabasePassword
        {
            get;
            set;
        }

        public int ShardOfflineTimeoutMilliseconds
        {
            get;
            set;
        }
    }
}
