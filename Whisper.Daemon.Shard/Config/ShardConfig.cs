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

namespace Whisper.Daemon.Shard.Config
{
    [Serializable]
    public class ShardConfig : AppConfiguration
    {
        public ShardConfig() : base()
        {
            ShardDaemonBindAddress = "0.0.0.0";
            ShardDaemonBindPort = 8085;
            ShardID = 1;
            ShardPingMilliseconds = 5000;
        }

        public string ShardDaemonBindAddress
        {
            get;
            set;
        }

        public int ShardDaemonBindPort
        {
            get;
            set;
        }

        public int ShardID
        {
            get;
            set;
        }

        public int ShardPingMilliseconds
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

        public string ShardDatabaseAddress
        {
            get;
            set;
        }

        public int ShardDatabasePort
        {
            get;
            set;
        }

        public string ShardDatabaseName
        {
            get;
            set;
        }

        public string ShardDatabaseUser
        {
            get;
            set;
        }

        public string ShardDatabasePassword
        {
            get;
            set;
        }

        public string WorldDatabaseAddress
        {
            get;
            set;
        }

        public int WorldDatabasePort
        {
            get;
            set;
        }

        public string WorldDatabaseName
        {
            get;
            set;
        }

        public string WorldDatabaseUser
        {
            get;
            set;
        }

        public string WorldDatabasePassword
        {
            get;
            set;
        }
    }
}
