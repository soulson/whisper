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
using Whisper.Game.Units;

namespace Whisper.Game.Characters
{
    public class CharacterTemplate
    {
        public CharacterTemplate(byte race, byte @class, int mapId, int zoneId, float x, float y, float z, float orientation, IList<ActionButton> actionButtons, IList<int> spellIds)
        {
            Race raceEnum;
            if (!Enum.TryParse(race.ToString(), out raceEnum))
                throw new CharacterException("attempt to create character template with invalid race [{0}]", race);

            Class classEnum;
            if (!Enum.TryParse(@class.ToString(), out classEnum))
                throw new CharacterException("attempt to create character template with invalid class [{0}]", @class);

            if(mapId < 0)
                throw new CharacterException("attempt to create character template with invalid map id [{0}]", mapId);

            if (zoneId < 0)
                throw new CharacterException("attempt to create character template with invalid zone id [{0}]", zoneId);

            Race = raceEnum;
            Class = classEnum;
            MapID = mapId;
            ZoneID = zoneId;
            PositionX = x;
            PositionY = y;
            PositionZ = z;
            Orientation = orientation;

            ActionButtons = actionButtons;
            SpellIDs = spellIds;
        }

        public Race Race
        {
            get;
            private set;
        }

        public Class Class
        {
            get;
            private set;
        }

        public int MapID
        {
            get;
            private set;
        }

        public int ZoneID
        {
            get;
            private set;
        }

        public float PositionX
        {
            get;
            private set;
        }

        public float PositionY
        {
            get;
            private set;
        }

        public float PositionZ
        {
            get;
            private set;
        }

        public float Orientation
        {
            get;
            private set;
        }

        public IList<ActionButton> ActionButtons
        {
            get;
            private set;
        }

        public IList<int> SpellIDs
        {
            get;
            private set;
        }
    }
}
