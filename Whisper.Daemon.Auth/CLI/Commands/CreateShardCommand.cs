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
using Whisper.Daemon.Auth.Lookup;
using Whisper.Daemon.Auth.Net;

namespace Whisper.Daemon.Auth.CLI.Commands
{
    public sealed class CreateShardCommand : AuthConsoleCommandBase
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CreateShardCommand));

        public override string Name
        {
            get
            {
                return "shard.create";
            }
        }

        public override string Help
        {
            get
            {
                return "name address port type\n\tCreates a new shard with the specified name and IP information.\n\tType must be one of: Normal, PvP, RP, RPPvP";
            }
        }

        public override void ExecuteCommand(AuthServer server, string[] arguments)
        {
            if (arguments.Length < 5)
                log.ErrorFormat("{0} requires 4 arguments", Name);
            else
            {
                string name = arguments[1];
                string address = arguments[2];
                int port = int.Parse(arguments[3]);
                string type = arguments[4];

                log.InfoFormat("creating new shard '{0}'", name);
                
                if (server.AuthDB.ExecuteQuery("select 1 from shard where name = ?", name, reader =>
                {
                    if (reader.Read())
                    {
                        log.ErrorFormat("cannot create shard '{0}': a shard with this name already exists", name);
                        return false;
                    }
                    return true;
                }))
                {
                    // they should really add a type param overload of this method
                    ShardType typeOrdinal = (ShardType)Enum.Parse(typeof(ShardType), type, true);

                    int result = server.AuthDB.ExecuteNonQuery("insert into shard (name, address, port, type) values (?, ?, ?, ?)", new object[] { name, address, port, (byte)typeOrdinal });
                    if (result == 1)
                        log.InfoFormat("successfully created shard '{0}'", name);
                    else
                        log.ErrorFormat("failed to create account '{0}'. expected 1 update, got {1}", name, result);
                }
            }
        }
    }
}
