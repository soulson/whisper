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
using Whisper.Game.Objects;
using Whisper.Shared.Utility;

namespace Whisper.Game.Units
{
    public class Unit : GameObject
    {
        private bool movementFlagsUpdated;
        private MovementFlags movementFlags;

        public Unit(ObjectID id, ObjectTypeID typeId) : this(id, typeId, (ushort)UnitFields.END)
        {
        }

        protected Unit(ObjectID id, ObjectTypeID typeId, ushort fieldsCount) : base(id, typeId, fieldsCount)
        {
            movementFlags = MovementFlags.None;
            movementFlagsUpdated = false;
            MovementSpeed = new MovementSpeed();

            // initialize default values
            CastSpeed = 1.0f;
            Energy = 100;
            EnergyMax = 100;
            RageMax = 1000;
            AttackTimeMainhandMilliseconds = 2000;
            AttackTimeOffhandMilliseconds = 2000;
            AttackTimeRangedMilliseconds = 2000;
        }

        #region Field Virtualizers
        protected virtual void OnRaceChanged()
        {
        }

        protected virtual void OnSexChanged()
        {
        }

        protected virtual void OnClassChanged()
        {
        }

        protected virtual void OnStatsChanged()
        {
        }
        #endregion

        #region Unit Fields
        public int Health
        {
            get
            {
                return GetFieldSigned(UnitFields.Health);
            }
            set
            {
                SetField(UnitFields.Health, value);
            }
        }

        public int HealthMax
        {
            get
            {
                return GetFieldSigned(UnitFields.MaxHealth);
            }
            set
            {
                SetField(UnitFields.MaxHealth, value);
            }
        }

        public int HealthBase
        {
            get
            {
                return GetFieldSigned(UnitFields.BaseHealth);
            }
            set
            {
                SetField(UnitFields.BaseHealth, value);
                OnStatsChanged();
            }
        }

        public int Mana
        {
            get
            {
                return GetFieldSigned(UnitFields.Mana);
            }
            set
            {
                SetField(UnitFields.Mana, value);
            }
        }

        public int ManaMax
        {
            get
            {
                return GetFieldSigned(UnitFields.MaxMana);
            }
            set
            {
                SetField(UnitFields.MaxMana, value);
            }
        }

        public int ManaBase
        {
            get
            {
                return GetFieldSigned(UnitFields.BaseMana);
            }
            set
            {
                SetField(UnitFields.BaseMana, value);
                OnStatsChanged();
            }
        }

        public int Rage
        {
            get
            {
                return GetFieldSigned(UnitFields.Rage);
            }
            set
            {
                SetField(UnitFields.Rage, value);
            }
        }

        public int RageMax
        {
            get
            {
                return GetFieldSigned(UnitFields.MaxRage);
            }
            set
            {
                SetField(UnitFields.MaxRage, value);
            }
        }

        public int Energy
        {
            get
            {
                return GetFieldSigned(UnitFields.Energy);
            }
            set
            {
                SetField(UnitFields.Energy, value);
            }
        }

        public int EnergyMax
        {
            get
            {
                return GetFieldSigned(UnitFields.MaxEnergy);
            }
            set
            {
                SetField(UnitFields.MaxEnergy, value);
            }
        }

        public int Focus
        {
            get
            {
                return GetFieldSigned(UnitFields.Focus);
            }
            set
            {
                SetField(UnitFields.Focus, value);
            }
        }

        public int FocusMax
        {
            get
            {
                return GetFieldSigned(UnitFields.MaxFocus);
            }
            set
            {
                SetField(UnitFields.MaxFocus, value);
            }
        }

        public int Happiness
        {
            get
            {
                return GetFieldSigned(UnitFields.Happiness);
            }
            set
            {
                SetField(UnitFields.Happiness, value);
            }
        }

        public int HappinessMax
        {
            get
            {
                return GetFieldSigned(UnitFields.MaxHappiness);
            }
            set
            {
                SetField(UnitFields.MaxHappiness, value);
            }
        }

        public UnitFlags UnitFlags
        {
            get
            {
                return (UnitFlags)GetFieldUnsigned(UnitFields.Flags);
            }
            set
            {
                SetField(UnitFields.Flags, (uint)value);
            }
        }

        public byte Level
        {
            get
            {
                return (byte)GetFieldSigned(UnitFields.Level);
            }
            set
            {
                SetField(UnitFields.Level, (int)value);
            }
        }

        public int Strength
        {
            get
            {
                return GetFieldSigned(UnitFields.Strength);
            }
            set
            {
                SetField(UnitFields.Strength, value);
                OnStatsChanged();
            }
        }

        public int Agility
        {
            get
            {
                return GetFieldSigned(UnitFields.Agility);
            }
            set
            {
                SetField(UnitFields.Agility, value);
                OnStatsChanged();
            }
        }

        public int Stamina
        {
            get
            {
                return GetFieldSigned(UnitFields.Stamina);
            }
            set
            {
                SetField(UnitFields.Stamina, value);
                OnStatsChanged();
            }
        }

        public int Intellect
        {
            get
            {
                return GetFieldSigned(UnitFields.Intellect);
            }
            set
            {
                SetField(UnitFields.Intellect, value);
                OnStatsChanged();
            }
        }

        public int Spirit
        {
            get
            {
                return GetFieldSigned(UnitFields.Spirit);
            }
            set
            {
                SetField(UnitFields.Spirit, value);
                OnStatsChanged();
            }
        }

        public int DisplayID
        {
            get
            {
                return GetFieldSigned(UnitFields.DisplayID);
            }
            set
            {
                SetField(UnitFields.DisplayID, value);
            }
        }

        public int NativeDisplayID
        {
            get
            {
                return GetFieldSigned(UnitFields.NativeDisplayID);
            }
            set
            {
                SetField(UnitFields.NativeDisplayID, value);
            }
        }

        public int FactionTemplate
        {
            get
            {
                return GetFieldSigned(UnitFields.FactionTemplate);
            }
            set
            {
                SetField(UnitFields.FactionTemplate, value);
            }
        }

        public int AttackTimeMainhandMilliseconds
        {
            get
            {
                return GetFieldSigned(UnitFields.AttackTimeBase);
            }
            set
            {
                SetField(UnitFields.AttackTimeBase, value);
            }
        }

        public int AttackTimeRangedMilliseconds
        {
            get
            {
                return GetFieldSigned(UnitFields.AttackTimeRanged);
            }
            set
            {
                SetField(UnitFields.AttackTimeRanged, value);
            }
        }

        public int AttackTimeOffhandMilliseconds
        {
            get
            {
                return GetFieldSigned(UnitFields.AttackTimeOffhand);
            }
            set
            {
                SetField(UnitFields.AttackTimeOffhand, value);
            }
        }

        public int AttackPower
        {
            get
            {
                return GetFieldSigned(UnitFields.AttackPower);
            }
            set
            {
                SetField(UnitFields.AttackPower, value);
            }
        }

        public int AttackPowerRanged
        {
            get
            {
                return GetFieldSigned(UnitFields.AttackPowerRanged);
            }
            set
            {
                SetField(UnitFields.AttackPowerRanged, value);
            }
        }

        public float BoundingRadius
        {
            get
            {
                return GetFieldFloat(UnitFields.BoundingRadius);
            }
            set
            {
                SetField(UnitFields.BoundingRadius, value);
            }
        }

        public float CombatReach
        {
            get
            {
                return GetFieldFloat(UnitFields.CombatReach);
            }
            set
            {
                SetField(UnitFields.CombatReach, value);
            }
        }

        public float AttackDamageMainhandMin
        {
            get
            {
                return GetFieldFloat(UnitFields.DamageMin);
            }
            set
            {
                SetField(UnitFields.DamageMin, value);
            }
        }

        public float AttackDamageMainhandMax
        {
            get
            {
                return GetFieldFloat(UnitFields.DamageMax);
            }
            set
            {
                SetField(UnitFields.DamageMax, value);
            }
        }

        public float AttackDamageRangedMin
        {
            get
            {
                return GetFieldFloat(UnitFields.DamageRangedMin);
            }
            set
            {
                SetField(UnitFields.DamageRangedMin, value);
            }
        }

        public float AttackDamageRangedMax
        {
            get
            {
                return GetFieldFloat(UnitFields.DamageRangedMax);
            }
            set
            {
                SetField(UnitFields.DamageRangedMax, value);
            }
        }

        public float CastSpeed
        {
            get
            {
                return GetFieldFloat(UnitFields.ModCastSpeed);
            }
            set
            {
                SetField(UnitFields.ModCastSpeed, value);
            }
        }

        public int Armor
        {
            get
            {
                return GetFieldSigned(UnitFields.Armor);
            }
            set
            {
                SetField(UnitFields.Armor, value);
            }
        }

        public Race Race
        {
            get
            {
                return (Race)GetFieldByte((ushort)UnitFields.Bytes0, 0);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes0, 0, (byte)value);
                OnRaceChanged();
            }
        }

        public Class Class
        {
            get
            {
                return (Class)GetFieldByte((ushort)UnitFields.Bytes0, 1);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes0, 1, (byte)value);
                OnClassChanged();
            }
        }
        
        public Sex Sex
        {
            get
            {
                return (Sex)GetFieldByte((ushort)UnitFields.Bytes0, 2);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes0, 2, (byte)value);
                OnSexChanged();
            }
        }

        public Resource ActiveResource
        {
            get
            {
                return (Resource)GetFieldByte((ushort)UnitFields.Bytes0, 3);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes0, 3, (byte)value);
            }
        }

        public StandState StandState
        {
            get
            {
                return (StandState)GetFieldByte((ushort)UnitFields.Bytes1, 0);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes1, 0, (byte)value);
            }
        }

        /// <remarks>
        /// For some reason, this is set to 0xee for players, but doesn't seem to be necessary.
        /// </remarks>
        public Loyalty Loyalty
        {
            get
            {
                return (Loyalty)GetFieldByte((ushort)UnitFields.Bytes1, 1);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes1, 1, (byte)value);
            }
        }

        public Form Form
        {
            get
            {
                return (Form)GetFieldByte((ushort)UnitFields.Bytes1, 2);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes1, 2, (byte)value);
            }
        }

        public UnitFlags1 UnitFlags1
        {
            get
            {
                return (UnitFlags1)GetFieldByte((ushort)UnitFields.Bytes1, 3);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes1, 3, (byte)value);
            }
        }

        public SheathState SheathState
        {
            get
            {
                return (SheathState)GetFieldByte((ushort)UnitFields.Bytes2, 0);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes2, 0, (byte)value);
            }
        }
        
        public UnitFlags2 UnitFlags2
        {
            get
            {
                return (UnitFlags2)GetFieldByte((ushort)UnitFields.Bytes2, 1);
            }
            set
            {
                SetField((ushort)UnitFields.Bytes2, 1, (byte)value);
            }
        }
        #endregion

        #region Unit Properties
        public MovementFlags MovementFlags
        {
            get
            {
                return movementFlags;
            }
            set
            {
                movementFlagsUpdated |= value != movementFlags;
                movementFlags = value;
            }
        }

        public MovementSpeed MovementSpeed
        {
            get;
            private set;
        }
        #endregion

        #region Fields Management
        public uint GetFieldUnsigned(UnitFields field)
        {
            return GetFieldUnsigned((ushort)field);
        }

        public int GetFieldSigned(UnitFields field)
        {
            return GetFieldSigned((ushort)field);
        }

        public float GetFieldFloat(UnitFields field)
        {
            return GetFieldFloat((ushort)field);
        }

        public void SetField(UnitFields field, uint value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(UnitFields field, int value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(UnitFields field, float value)
        {
            SetField((ushort)field, value);
        }
        #endregion

        #region Update Management
        protected override ObjectUpdateFlags UpdateFlags
        {
            get
            {
                return base.UpdateFlags | ObjectUpdateFlags.Living;
            }
        }

        public override ObjectChangeState ChangeState
        {
            get
            {
                if (movementFlagsUpdated || MovementSpeed.IsChanged)
                    return base.ChangeState | ObjectChangeState.Movement;
                else
                    return base.ChangeState;
            }
        }

        public override void ClearChangeState()
        {
            base.ClearChangeState();

            MovementSpeed.ClearChangeState();
            movementFlagsUpdated = false;
        }

        protected override void AppendMovementUpdate(ByteBuffer buffer, ObjectUpdateType updateType, ObjectUpdateFlags updateFlags)
        {

            if ((updateFlags & ObjectUpdateFlags.Living) != 0)
            {
                buffer.Append((int)MovementFlags);
                buffer.Append(0); // time
                buffer.Append(Position);
                buffer.Append(0); // fallingTime

                buffer.Append(MovementSpeed.Walking);
                buffer.Append(MovementSpeed.Running);
                buffer.Append(MovementSpeed.RunningBack);
                buffer.Append(MovementSpeed.Swimming);
                buffer.Append(MovementSpeed.SwimmingBack);
                buffer.Append(MovementSpeed.Turning);

                // remove HasPosition from the updateFlags when passing to GameObject, since Living already wrote the position
                base.AppendMovementUpdate(buffer, updateType, updateFlags & ~ObjectUpdateFlags.HasPosition);
            }
            else
                base.AppendMovementUpdate(buffer, updateType, updateFlags);
        }
        #endregion
    }
}
