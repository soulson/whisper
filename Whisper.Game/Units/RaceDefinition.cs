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

namespace Whisper.Game.Units
{
    public class RaceDefinition
    {
        public RaceDefinition(Race race, string name, int factionId, RaceFlags flags, int displayIdMale, int displayIdFemale, int firstLoginCinematicId)
        {
            Race = race;
            Name = name;
            FactionID = factionId;
            Flags = flags;
            DisplayIDMale = displayIdMale;
            DisplayIDFemale = displayIdFemale;
            FirstLoginCinematicID = firstLoginCinematicId;
        }

        public Race Race
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public int FactionID
        {
            get;
            private set;
        }

        public RaceFlags Flags
        {
            get;
            private set;
        }

        protected int DisplayIDMale
        {
            get;
            private set;
        }

        protected int DisplayIDFemale
        {
            get;
            private set;
        }

        public int GetDisplayID(Sex sex)
        {
            if (sex == Sex.Female)
                return DisplayIDFemale;
            else if (sex == Sex.Male)
                return DisplayIDMale;
            else
                throw new ArgumentException(nameof(sex));
        }

        public int FirstLoginCinematicID
        {
            get;
            private set;
        }
    }
}
