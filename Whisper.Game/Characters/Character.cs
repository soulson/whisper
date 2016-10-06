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

            // initialize default values
            CombatReach = 1.5f;
            DamageDoneArcaneMultiplier = 1.0f;
            DamageDoneFrostMultiplier = 1.0f;
            DamageDoneFireMultiplier = 1.0f;
            DamageDoneNatureMultiplier = 1.0f;
            DamageDoneShadowMultiplier = 1.0f;
            DamageDoneHolyMultiplier = 1.0f;
            DamageDonePhysicalMultiplier = 1.0f;

            UnitFlags |= UnitFlags.PvPUnit;

            // TODO: instead of 0x18 as described here, this field has value 0x28 for players. need to determine the difference
            UnitFlags2 |= UnitFlags2.Supportable;
            UnitFlags2 |= UnitFlags2.CanHaveAuras;

            // TODO: these need to be updated as implemented
            RestState = RestState.Normal;
            WatchedFactionIndex = -1;

            // TODO: remove magic values
            SetField(UnitFields.Health, 51);
            SetField(UnitFields.Mana, 165);
            SetField(UnitFields.MaxHealth, 51);
            SetField(UnitFields.MaxMana, 165);
            SetField(UnitFields.Strength, 20);
            SetField(UnitFields.Agility, 20);
            SetField(UnitFields.Stamina, 20);
            SetField(UnitFields.Intellect, 23);
            SetField(UnitFields.Spirit, 23);
            SetField(UnitFields.BoundingRadius, 0.208f);
            SetField(UnitFields.BaseMana, 100);
            SetField(UnitFields.BaseHealth, 31);
            SetField(CharacterFields.XPNextLevel, 400);
        }

        public string Name
        {
            get;
            private set;
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

        protected override void OnSexChanged()
        {
            base.OnSexChanged();
            SetField((ushort)CharacterFields.Bytes3, 0, (byte)Sex);
        }

        #region Character Fields
        public byte Skin
        {
            get
            {
                return GetFieldByte((ushort)CharacterFields.Bytes1, 0);
            }
            set
            {
                SetField((ushort)CharacterFields.Bytes1, 0, value);
            }
        }

        public byte Face
        {
            get
            {
                return GetFieldByte((ushort)CharacterFields.Bytes1, 1);
            }
            set
            {
                SetField((ushort)CharacterFields.Bytes1, 1, value);
            }
        }

        public byte HairStyle
        {
            get
            {
                return GetFieldByte((ushort)CharacterFields.Bytes1, 2);
            }
            set
            {
                SetField((ushort)CharacterFields.Bytes1, 2, value);
            }
        }

        public byte HairColor
        {
            get
            {
                return GetFieldByte((ushort)CharacterFields.Bytes1, 3);
            }
            set
            {
                SetField((ushort)CharacterFields.Bytes1, 3, value);
            }
        }

        public byte FaceExtra
        {
            get
            {
                return GetFieldByte((ushort)CharacterFields.Bytes2, 0);
            }
            set
            {
                SetField((ushort)CharacterFields.Bytes2, 0, value);
            }
        }

        public RestState RestState
        {
            get
            {
                return (RestState)GetFieldByte((ushort)CharacterFields.Bytes2, 3);
            }
            set
            {
                SetField((ushort)CharacterFields.Bytes2, 3, (byte)value);
            }
        }

        public float DamageDonePhysicalMultiplier
        {
            get
            {
                return GetFieldFloat(CharacterFields.DamageDonePhysicalModPercent);
            }
            set
            {
                SetField(CharacterFields.DamageDonePhysicalModPercent, value);
            }
        }

        public float DamageDoneHolyMultiplier
        {
            get
            {
                return GetFieldFloat(CharacterFields.DamageDoneHolyModPercent);
            }
            set
            {
                SetField(CharacterFields.DamageDoneHolyModPercent, value);
            }
        }

        public float DamageDoneFireMultiplier
        {
            get
            {
                return GetFieldFloat(CharacterFields.DamageDoneFireModPercent);
            }
            set
            {
                SetField(CharacterFields.DamageDoneFireModPercent, value);
            }
        }

        public float DamageDoneNatureMultiplier
        {
            get
            {
                return GetFieldFloat(CharacterFields.DamageDoneNatureModPercent);
            }
            set
            {
                SetField(CharacterFields.DamageDoneNatureModPercent, value);
            }
        }

        public float DamageDoneFrostMultiplier
        {
            get
            {
                return GetFieldFloat(CharacterFields.DamageDoneFrostModPercent);
            }
            set
            {
                SetField(CharacterFields.DamageDoneFrostModPercent, value);
            }
        }

        public float DamageDoneShadowMultiplier
        {
            get
            {
                return GetFieldFloat(CharacterFields.DamageDoneShadowModPercent);
            }
            set
            {
                SetField(CharacterFields.DamageDoneShadowModPercent, value);
            }
        }

        public float DamageDoneArcaneMultiplier
        {
            get
            {
                return GetFieldFloat(CharacterFields.DamageDoneArcaneModPercent);
            }
            set
            {
                SetField(CharacterFields.DamageDoneArcaneModPercent, value);
            }
        }

        public int WatchedFactionIndex
        {
            get
            {
                return GetFieldSigned(CharacterFields.WatchedFactionIndex);
            }
            set
            {
                SetField(CharacterFields.WatchedFactionIndex, value);
            }
        }

        public int XPNextLevel
        {
            get
            {
                return GetFieldSigned(CharacterFields.XPNextLevel);
            }
            set
            {
                SetField(CharacterFields.XPNextLevel, value);
            }
        }

        public float CritChanceMelee
        {
            get
            {
                return GetFieldFloat(CharacterFields.CritPercent);
            }
            set
            {
                SetField(CharacterFields.CritPercent, value);
            }
        }

        public float CritChanceRanged
        {
            get
            {
                return GetFieldFloat(CharacterFields.CritPercentRanged);
            }
            set
            {
                SetField(CharacterFields.CritPercentRanged, value);
            }
        }

        public float DodgeChance
        {
            get
            {
                return GetFieldFloat(CharacterFields.DodgePercent);
            }
            set
            {
                SetField(CharacterFields.DodgePercent, value);
            }
        }
        #endregion

        #region Fields Management
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
        #endregion

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
