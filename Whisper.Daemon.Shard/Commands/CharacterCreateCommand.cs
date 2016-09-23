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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Net;
using Whisper.Game.Characters;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Commands
{
    public sealed class CharacterCreateCommand : ShardCommandBase<ClientPacketHeader>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CharacterCreateCommand));

        public override string Name
        {
            get
            {
                return ShardClientOpcode.CharacterCreate.ToString();
            }
        }

        public override SessionStatus PermissibleStatus
        {
            get
            {
                return SessionStatus.Authenticated;
            }
        }

        public override CommandThreadSafety ThreadSafety
        {
            get
            {
                return CommandThreadSafety.Immediate;
            }
        }

        public override void ExecuteCommand(ShardSession session, ShardRequest request, ClientPacketHeader header)
        {
            int offset = Marshal.SizeOf<ClientPacketHeader>();

            string name = Strings.FromNullTerminated(request.Packet, ref offset);
            byte race = request.Packet[offset++];
            byte @class = request.Packet[offset++];
            byte sex = request.Packet[offset++];
            byte skin = request.Packet[offset++];
            byte face = request.Packet[offset++];
            byte hairStyle = request.Packet[offset++];
            byte hairColor = request.Packet[offset++];
            byte facialHair = request.Packet[offset++];
            byte outfitId = request.Packet[offset++];

            CharacterRace raceEnum;
            if(!Enum.TryParse(race.ToString(), out raceEnum))
            {
                log.WarnFormat("client attempted to create character with invalid race [{0}]", race);
                session.Send(ShardServerOpcode.CharacterCreate, (byte)CharacterCreateResponse.Failed);
                return;
            }

            CharacterClass classEnum;
            if (!Enum.TryParse(@class.ToString(), out classEnum))
            {
                log.WarnFormat("client attempted to create character with invalid class [{0}]", @class);
                session.Send(ShardServerOpcode.CharacterCreate, (byte)CharacterCreateResponse.Failed);
                return;
            }

            if(sex != (byte)CharacterSex.Male && sex != (byte)CharacterSex.Female)
            {
                log.WarnFormat("client attempted to create character with invalid sex [{0}]", sex);
                session.Send(ShardServerOpcode.CharacterCreate, (byte)CharacterCreateResponse.Failed);
                return;
            }
            
            CharacterTemplate template;

            try
            {
                template = session.Server.World.CharacterTemplates[raceEnum][classEnum];
            }
            catch (KeyNotFoundException)
            {
                // no template for race/class combination
                log.WarnFormat("cannot create character with race {0} and class {1} because that combination is not valid", raceEnum, classEnum);
                session.Send(ShardServerOpcode.CharacterCreate, (byte)CharacterCreateResponse.Failed);
                return;
            }

            if(session.Server.ShardDB.ExecuteQuery("select id from `character` where name = ?", name, result =>
            {
                return result.Read();
            }))
            {
                // character already exists
                log.DebugFormat("cannot create character [{0}] because that name already exists", name);
                session.Send(ShardServerOpcode.CharacterCreate, (byte)CharacterCreateResponse.NameTaken);
                return;
            }

            int updated = session.Server.ShardDB.ExecuteNonQuery("insert into `character` (account_id, name, race, class, sex, skin, face, hair_style, hair_color, facial_hair, position_x, position_y, position_z, orientation, map_id, zone_id) values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                session.AccountID, name, race, @class, sex, skin, face, hairStyle, hairColor, facialHair, template.PositionX, template.PositionY, template.PositionZ, template.Orientation, template.MapID, template.ZoneID);

            if(updated != 1)
            {
                log.ErrorFormat("expected 1 row updated when inserting new character but got {0}", updated);
                session.Send(ShardServerOpcode.CharacterCreate, (byte)CharacterCreateResponse.Failed);
                return;
            }

            int characterId = session.Server.ShardDB.ExecuteQuery("select id from `character` where account_id = ? and name = ?", new object[] { session.AccountID, name }, result =>
            {
                if (result.Read())
                    return result.GetInt32(0);
                else
                    return -1;
            });

            if(characterId < 0)
            {
                log.ErrorFormat("unable to locate recently inserted character with account id [{0}] and name '{1}'", session.AccountID, name);
                session.Send(ShardServerOpcode.CharacterCreate, (byte)CharacterCreateResponse.Failed);
                return;
            }

            foreach(var spellId in template.SpellIDs)
            {
                if (session.Server.ShardDB.ExecuteNonQuery("insert into character_spell (character_id, spell_id) values (?, ?)", characterId, spellId) != 1)
                    log.Error("insert into character_spell did not affect 1 row");
            }

            for(int i = 0; i < template.ActionButtons.Count; ++i)
            {
                if(session.Server.ShardDB.ExecuteNonQuery("insert into character_action_button (character_id, button, action, type) values (?, ?, ?, ?)", characterId, i, template.ActionButtons[i].Action, template.ActionButtons[i].ButtonType) != 1)
                    log.Error("insert into character_action_button did not affect 1 row");
            }

            // success case
            log.InfoFormat("account [{0}] successfully created new {1} {2} named '{3}'", session.AccountID, raceEnum, classEnum, name);
            session.Send(ShardServerOpcode.CharacterCreate, (byte)CharacterCreateResponse.Success);
        }
    }
}
