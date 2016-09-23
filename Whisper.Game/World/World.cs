﻿/* 
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
using Whisper.Game.Characters;
using Whisper.Shared.Database;
using log4net;

namespace Whisper.Game.World
{
    /// <summary>
    /// Instances of this class permanently store static data about the game world.
    /// It is expected that clients will make use of it as a singleton, although this is not enforced.
    /// Individual instances of dynamic shards using this World as a template are represented by the Shard class.
    /// </summary>
    /// <seealso cref="Whisper.Game.World.Shard"/>
    public class World
    {
        private readonly ILog log = LogManager.GetLogger(typeof(World));

        /// <summary>
        /// Initializes a new instance of the World class, loading and caching its contents from the provided datasource.
        /// </summary>
        /// <param name="wworld">A datasource to a static world database. Needs read access only. The SQL used to initialize this database is located under Whisper.Daemon.Shard.SQL.World.</param>
        public World(IWhisperDatasource wworld)
        {
            LoadCharacterTemplates(wworld);
        }

        private void LoadCharacterTemplates(IWhisperDatasource datasource)
        {
            log.Info("loading character templates...");
            datasource.ExecuteQuery("select race, class, map_id, zone_id, position_x, position_y, position_z, orientation from character_template", ctReader =>
            {
                var templates = new Dictionary<CharacterRace, IDictionary<CharacterClass, CharacterTemplate>>();

                while (ctReader.Read())
                {
                    byte race = ctReader.GetByte(0);
                    byte @class = ctReader.GetByte(1);

                    int buttonCount = 0;
                    ActionButton[] actionButtons = new ActionButton[Character.MaxActionButtons];
                    datasource.ExecuteQuery("select button, action, type from character_template_action_button where race = ? and class = ? limit ?", new object[] { race, @class, Character.MaxActionButtons }, abReader =>
                    {
                        while (abReader.Read())
                        {
                            actionButtons[abReader.GetInt16(0)] = new ActionButton(abReader.GetInt32(1), (ActionButton.Type)abReader.GetByte(2));
                            ++buttonCount;
                        }
                    });

                    IList<int> spells = new List<int>();
                    datasource.ExecuteQuery("select spell_id from character_template_spell where race = ? and class = ?", new object[] { race, @class }, sReader =>
                    {
                        while (sReader.Read())
                            spells.Add(sReader.GetInt32(0));
                    });

                    CharacterTemplate template = new CharacterTemplate(race, @class, ctReader.GetInt32(2), ctReader.GetInt32(3), ctReader.GetFloat(4), ctReader.GetFloat(5), ctReader.GetFloat(6), ctReader.GetFloat(7), actionButtons, spells);

                    if (!templates.ContainsKey(template.Race))
                        templates.Add(template.Race, new Dictionary<CharacterClass, CharacterTemplate>());

                    templates[template.Race].Add(template.Class, template);
                    log.DebugFormat("loaded character template for {0} {1} with {2} spells and {3} buttons", template.Race, template.Class, template.SpellIDs.Count, buttonCount);
                }

                CharacterTemplates = templates;
            });
        }
        
        /// <summary>
        /// Gets a mapping from CharacterRace and CharacterClass to a CharacterTemplate describing the starting state for characters of that race and class.
        /// </summary>
        public IDictionary<CharacterRace, IDictionary<CharacterClass, CharacterTemplate>> CharacterTemplates
        {
            get;
            private set;
        }
    }
}