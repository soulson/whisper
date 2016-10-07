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
using Whisper.Game.Units;

namespace Whisper.Game.Characters
{
    public class CharacterBaseStats
    {
        public CharacterBaseStats(Race race, Class @class, byte level, int health, int mana, int strength, int agility, int stamina, int intellect, int spirit)
        {
            Race = race;
            Class = @class;
            Level = level;
            Health = health;
            Mana = mana;
            Strength = strength;
            Agility = agility;
            Stamina = stamina;
            Intellect = intellect;
            Spirit = spirit;
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

        public byte Level
        {
            get;
            private set;
        }

        public int Health
        {
            get;
            private set;
        }

        public int Mana
        {
            get;
            private set;
        }

        public int Strength
        {
            get;
            private set;
        }

        public int Agility
        {
            get;
            private set;
        }

        public int Stamina
        {
            get;
            private set;
        }

        public int Intellect
        {
            get;
            private set;
        }

        public int Spirit
        {
            get;
            private set;
        }
    }
}
