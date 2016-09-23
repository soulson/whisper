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
using Whisper.Game.Units;
using Whisper.Shared.Math;

namespace Whisper.Game.Characters
{
    public class Character : Unit
    {
        public const int MaxActionButtons = 120;

        public Character(ObjectID id, string name, IList<ActionButton> actionButtons, IList<Spell> spells) : base(id, ObjectTypeID.Player, (ushort)CharacterFields.END)
        {
            ActionButtons = actionButtons;
            Spells = spells;
            Name = name;
        }

        public string Name
        {
            get;
            private set;
        }

        public CharacterRace Race
        {
            get;
            set;
        }

        public CharacterClass Class
        {
            get;
            set;
        }

        public CharacterSex Sex
        {
            get;
            set;
        }

        public int ZoneID
        {
            get;
            set;
        }

        public OrientedVector3 BindPoint
        {
            get;
            set;
        }

        public IList<ActionButton> ActionButtons
        {
            get;
            private set;
        }

        public IList<Spell> Spells
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Name;
        }

        public uint GetFieldUnsigned(CharacterFields field)
        {
            return GetFieldUnsigned((ushort)field);
        }

        public int GetFieldSigned(CharacterFields field)
        {
            return GetFieldSigned((ushort)field);
        }

        public float GetFieldFloat(CharacterFields field)
        {
            return GetFieldFloat((ushort)field);
        }

        public void SetField(CharacterFields field, uint value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(CharacterFields field, int value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(CharacterFields field, float value)
        {
            SetField((ushort)field, value);
        }

        public class Spell
        {
            public int SpellID
            {
                get;
                set;
            }

            public bool Enabled
            {
                get;
                set;
            }
        }
    }
}
