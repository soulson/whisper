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
using Whisper.Game.Units;
using Whisper.Shared.Database;
using Whisper.Shared.Math;
using log4net;
using System.IO;

namespace Whisper.Daemon.Shard.Database
{
    public class CharacterDao
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CharacterDao));

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

            //                                 0     1     2      3    4     5     6           7           8            9      10          11          12          13           14      15       16            17
            return wshard.ExecuteQuery("select name, race, class, sex, skin, face, hair_style, hair_color, facial_hair, level, position_x, position_y, position_z, orientation, map_id, zone_id, player_flags, fields from `character` where id = ?", characterId.ID, result =>
            {
                if (result.Read())
                {
                    Character character = new Character(characterId, result.GetString(0), actionButtons, spells);

                    // set fields first. following updates could clobber information in that array
                    if (result.IsDBNull(17))
                    {
                        // new characters don't have field arrays yet
                        character.FirstLogin = true;
                    }
                    else
                    {
                        int size = character.FieldCount * sizeof(uint);
                        byte[] buffer = new byte[size];
                        long fieldsResult = result.GetBytes(17, 0, buffer, 0, size);

                        if (fieldsResult == size)
                            character.SetRawFields(buffer);
                        else
                        {
                            // did not read the correct amount of bytes. corruption?
                            throw new InvalidDataException($"character.fields had incorrect number of bytes. expected {size}, but got {fieldsResult}");
                        }
                    }

                    character.Position = new OrientedVector3(result.GetFloat(10), result.GetFloat(11), result.GetFloat(12), result.GetFloat(13));
                    character.MapID = result.GetInt32(14);
                    character.ZoneID = result.GetInt32(15);

                    Race raceEnum;
                    if (Enum.TryParse(result.GetByte(1).ToString(), out raceEnum))
                        character.Race = raceEnum;
                    else
                        throw new ArgumentException($"cannot load character with invalid race {result.GetByte(1)}");

                    Class classEnum;
                    if (Enum.TryParse(result.GetByte(2).ToString(), out classEnum))
                        character.Class = classEnum;
                    else
                        throw new ArgumentException($"cannot load character with invalid class {result.GetByte(2)}");

                    Sex sexEnum;
                    if (Enum.TryParse(result.GetByte(3).ToString(), out sexEnum))
                        character.Sex = sexEnum;
                    else
                        throw new ArgumentException($"cannot load character with invalid sex {result.GetByte(3)}");

                    character.Skin = result.GetByte(4);
                    character.Face = result.GetByte(5);
                    character.HairStyle = result.GetByte(6);
                    character.HairColor = result.GetByte(7);
                    character.FaceExtra = result.GetByte(8);
                    character.Level = result.GetInt32(9);

                    return character;
                }
                else
                    throw new ArgumentException("GetCharacterByID called on ObjectID that does not exist", nameof(characterId));
            });
        }
    }
}
