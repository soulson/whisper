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
using System.Collections.Generic;
using Whisper.Game.Objects;
using Whisper.Game.Characters;
using Whisper.Shared.Database;
using Whisper.Shared.Math;

namespace Whisper.Daemon.Shard.Database
{
    public class CharacterDao
    {
        public Character GetCharacterByID(IWhisperDatasource wshard, ObjectID characterId)
        {
            if (characterId.ObjectType != ObjectID.Type.Player)
                throw new ArgumentException("GetCharacterByID called on ObjectID that is not a Player", "characterId");

            IList<ActionButton> actionButtons = new List<ActionButton>();
            wshard.ExecuteQuery("select action, type from character_action_button where character_id = ? order by button asc", characterId.ID, result =>
            {
                while (result.Read())
                    actionButtons.Add(new ActionButton(result.GetInt32(0), (ActionButton.Type)result.GetByte(1)));
            });

            IList<Character.Spell> spells = new List<Character.Spell>();
            wshard.ExecuteQuery("select spell_id, enabled from character_spell where character_id = ?", characterId.ID, result =>
            {
                while (result.Read())
                    spells.Add(new Character.Spell() { SpellID = result.GetInt32(0), Enabled = result.GetBoolean(1) });
            });

            //                                 0     1     2      3    4     5     6           7           8            9      10  11     12          13          14          15           16      17       18
            return wshard.ExecuteQuery("select name, race, class, sex, skin, face, hair_style, hair_color, facial_hair, level, xp, money, position_x, position_y, position_z, orientation, map_id, zone_id, player_flags from `character` where id = ?", characterId.ID, result =>
            {
                if (result.Read())
                {
                    Character character = new Character(characterId, result.GetString(0), actionButtons, spells);
                    character.Position = new OrientedVector3(result.GetFloat(12), result.GetFloat(13), result.GetFloat(14), result.GetFloat(15));
                    character.MapID = result.GetInt32(16);
                    character.ZoneID = result.GetInt32(17);

                    return character;
                }
                else
                    throw new ArgumentException("GetCharacterByID called on ObjectID that does not exist", "characterId");
            });
        }
    }
}
