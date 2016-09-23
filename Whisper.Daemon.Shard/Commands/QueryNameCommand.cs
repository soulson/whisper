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
using System.Runtime.InteropServices;
using Whisper.Daemon.Shard.Database;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Net;
using Whisper.Game.Characters;
using Whisper.Game.Objects;
using log4net;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Commands
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct QueryName
    {
        private ClientPacketHeader Header;
        public ObjectID CharacterID;
    }

    public sealed class QueryNameCommand : ShardCommandBase<QueryName>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(QueryNameCommand));

        public override string Name
        {
            get
            {
                return ShardClientOpcode.QueryName.ToString();
            }
        }

        public override SessionStatus PermissibleStatus
        {
            get
            {
                return SessionStatus.Ingame;
            }
        }

        public override CommandThreadSafety ThreadSafety
        {
            get
            {
                return CommandThreadSafety.ThreadSafe;
            }
        }

        public override void ExecuteCommand(ShardSession session, ShardRequest request, QueryName header)
        {
            Character character = session.Server.Shard.GetCharacter(header.CharacterID);

            if(character == null)
            {
                // not online
                character = new CharacterDao().GetCharacterByID(session.Server.ShardDB, header.CharacterID);
            }

            if (character == null)
                log.WarnFormat("received {0} for non-existing object id {1}", ShardClientOpcode.QueryName, header.CharacterID);
            else
            {
                using (ByteBuffer response = new ByteBuffer())
                {
                    response.Append(character.ID);
                    response.Append(character.Name);
                    response.Append((byte)0); // used to identify shard name in cross-shard bg
                    response.Append((int)character.Race);
                    response.Append((int)character.Sex);
                    response.Append((int)character.Class);

                    session.Send(ShardServerOpcode.QueryName, response);
                }
            }
        }
    }
}
