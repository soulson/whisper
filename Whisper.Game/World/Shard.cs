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
        /// <param name="world">the World that this Shard is to be an instance of</param>
        public Shard(World world)
        {
            World = world;
            GameObjects = new List<GameObject>();
            Units = new List<Unit>();
            Characters = new List<Character>();
        }

        /// <summary>
        /// Initializes the state of the Shard. Must be called before any Update calls.
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// Updates the state of the Shard to represent that the given amount of game time has passed.
        /// </summary>
        /// <param name="diff">amount of game time passed since the last call to Update</param>
        public void Update(TimeSpan diff)
        {

        }

        /// <summary>
        /// Clears the UpdateMask on all GameObjects in the Shard.
        /// </summary>
        public void ClearUpdateMasks()
        {
            foreach (GameObject go in GameObjects)
                go.ClearChangeState();
            foreach (GameObject go in Units)
                go.ClearChangeState();
            foreach (GameObject go in Characters)
                go.ClearChangeState();
        }

        /// <summary>
        /// Returns true if and only if the given GameObject exists in this Shard.
        /// </summary>
        /// <param name="gameObject">GameObject to test</param>
        /// <returns>as described</returns>
        public bool Exists(GameObject gameObject)
        {
            return GameObjects.Contains(gameObject)
                || Units.Contains(gameObject)
                || Characters.Contains(gameObject);
        }

        /// <summary>
        /// Gets the World that this Shard is an instance of.
        /// </summary>
        public World World
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a List of all GameObjects currently in the Shard.
        /// </summary>
        protected IList<GameObject> GameObjects
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a List of all Units currently in the Shard.
        /// </summary>
        protected IList<Unit> Units
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a List of all Characters currently in the Shard.
        /// </summary>
        protected IList<Character> Characters
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets an enumeration of all GameObject instances existing in the given map.
        /// </summary>
        /// <param name="mapId">map id for which to list GameObjects</param>
        /// <returns>enumeration of GameObjects or an empty enumeration if none found</returns>
        public IEnumerable<GameObject> GetObjects(int mapId)
        {
            return GameObjects.Where((go) => go.MapID == mapId).Union(Units.Where((u) => u.MapID == mapId)).Union(Characters.Where((c) => c.MapID == mapId)).AsEnumerable();
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
        /// Gets a Character by ObjectID, or null if that Character is not online.
        /// </summary>
        /// <param name="id">character id</param>
        /// <returns>character with given id or null</returns>
        public Character GetCharacter(ObjectID id)
        {
            foreach (Character c in Characters)
            {
                if (c.ID == id)
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
                log.WarnFormat("{0} called with null argument", nameof(RemoveCharacter));
                return;
            }

            if (Characters.Remove(character))
                log.DebugFormat("character {0} removed from shard", character);
            else
                log.WarnFormat("{1} cannot find character {0} to remove it", character, nameof(RemoveCharacter));
        }
    }
}
