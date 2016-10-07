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
using Whisper.Game.Characters;
using Whisper.Game.Units;
using Whisper.Shared.Database;
using log4net;
using Whisper.Game.Objects;

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
            LoadRaceDefinitions(wworld);
            LoadCharacterTemplates(wworld);
            LoadModelDefinitions(wworld);
        }

        private void LoadCharacterTemplates(IWhisperDatasource datasource)
        {
            log.Info("loading character templates...");
            datasource.ExecuteQuery("select race, class, map_id, zone_id, position_x, position_y, position_z, orientation from character_template", ctReader =>
            {
                var templates = new Dictionary<Race, IDictionary<Class, CharacterTemplate>>();

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
                        templates.Add(template.Race, new Dictionary<Class, CharacterTemplate>());

                    templates[template.Race].Add(template.Class, template);
                    log.DebugFormat("loaded character template for {0} {1} with {2} spells and {3} buttons", template.Race, template.Class, template.SpellIDs.Count, buttonCount);
                }

                CharacterTemplates = templates;
            });
        }

        private void LoadRaceDefinitions(IWhisperDatasource datasource)
        {
            log.Info("loading race definitions...");
            datasource.ExecuteQuery("select id, name, flags, display_id_male, display_id_female, first_login_cinematic_id, faction_id from race_definition", reader =>
            {
                var defs = new Dictionary<Race, RaceDefinition>();

                while (reader.Read())
                {
                    RaceDefinition rd = new RaceDefinition((Race)reader.GetByte(0), reader.GetString(1), reader.GetInt32(6), (RaceFlags)reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5));
                    defs.Add(rd.Race, rd);

                    log.DebugFormat("loaded race definition for {0}", rd.Name);
                }

                RaceDefinitions = defs;
            });
        }

        private void LoadModelDefinitions(IWhisperDatasource datasource)
        {
            log.Info("loading model definitions...");
            datasource.ExecuteQuery("select id, bounding_radius, combat_reach from model_definition", reader =>
            {
                var defs = new Dictionary<int, ModelDefinition>();

                while (reader.Read())
                {
                    ModelDefinition md = new ModelDefinition(reader.GetInt32(0), reader.GetFloat(1), reader.GetFloat(2));
                    defs.Add(md.ModelID, md);
                }

                ModelDefinitions = defs;
                log.DebugFormat("loaded {0} model definitions", defs.Count);
            });
        }

        /// <summary>
        /// Gets a mapping from Race and Class to a CharacterTemplate describing the starting state for characters of that race and class.
        /// </summary>
        public IDictionary<Race, IDictionary<Class, CharacterTemplate>> CharacterTemplates
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a mapping from Race to a RaceDefinition describing properties of that race.
        /// </summary>
        public IDictionary<Race, RaceDefinition> RaceDefinitions
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a mapping from model ID to a ModelDefinition object describing the properties of that model.
        /// </summary>
        public IDictionary<int, ModelDefinition> ModelDefinitions
        {
            get;
            private set;
        }
    }
}
