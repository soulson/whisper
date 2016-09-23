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
using System.Linq;
using System.Text;
using Whisper.Game.Characters;
using Whisper.Game.Objects;
using Whisper.Game.Units;

namespace Whisper.Game.World
{
    /// <summary>
    /// Instances of this class represent the dynamic state of a shard instance; that is, an individual copy of the world whose static state is represented by the World class.
    /// </summary>
    /// <seealso cref="Whisper.Game.World.World"/>
    public class Shard
    {
        private readonly ILog log = LogManager.GetLogger(typeof(Shard));

        /// <summary>
        /// Initializes a new instance of the Shard class.
        /// </summary>
        public Shard()
        {
            GameObjects = new List<GameObject>();
            Units = new List<Unit>();
            Characters = new List<Character>();
        }

        protected IList<GameObject> GameObjects
        {
            get;
            private set;
        }

        protected IList<Unit> Units
        {
            get;
            private set;
        }

        protected IList<Character> Characters
        {
            get;
            private set;
        }

        /// <summary>
        /// Adds a Character to the Shard.
        /// </summary>
        /// <param name="character">character to add</param>
        public void AddCharacter(Character character)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character), nameof(AddCharacter));

            Characters.Add(character);
            log.DebugFormat("added character {0} to shard", character);
        }

        /// <summary>
        /// Gets a Character by name, or null if that Character is not online.
        /// </summary>
        /// <param name="name">character name</param>
        /// <returns>character with given name or null</returns>
        public Character GetCharacter(string name)
        {
            foreach(Character c in Characters)
            {
                if (c.Name == name)
                    return c;
            }

            return null;
        }

        /// <summary>
        /// Removes a Character from the Shard.
        /// </summary>
        /// <param name="character">character to remove</param>
        public void RemoveCharacter(Character character)
        {
            if(character == null)
            {
                log.Warn("Shard.RemoveCharacter called with null argument");
                return;
            }

            if (Characters.Remove(character))
                log.DebugFormat("character {0} removed from shard", character);
            else
                log.WarnFormat("Shard.RemoveCharacter cannot find character {0} to remove it", character);
        }
    }
}
