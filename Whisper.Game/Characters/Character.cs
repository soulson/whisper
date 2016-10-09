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
using Whisper.Game.Objects;
using Whisper.Game.Units;
using Whisper.Game.World;
using Whisper.Shared.Math;

namespace Whisper.Game.Characters
{
    public class Character : Unit
    {
        public const int MaxActionButtons = 120;

        private readonly ILog log = LogManager.GetLogger(typeof(Character));

        /// <summary>
        /// Creates a new instance of the Character class.
        /// </summary>
        /// <param name="id">object ID of this Character</param>
        /// <param name="name">this Character's name</param>
        /// <param name="actionButtons">a list of ActionButton objects for this Character</param>
        /// <param name="spells">a list of Spells known to this Character</param>
        public Character(ObjectID id, string name, IList<ActionButton> actionButtons, IList<Spell> spells) : base(id, ObjectTypeID.Player, (ushort)CharacterFields.END)
        {
            ActionButtons = actionButtons;
            Spells = spells;
            Name = name;
            FirstLogin = false;
        }

        /// <summary>
        /// Gets the name of the Character.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the current Zone ID of the Character.
        /// </summary>
        public int ZoneID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the bind point for this Character.
        /// </summary>
        public OrientedVector3 BindPoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a list of this Character's ActionButtons.
        /// </summary>
        public IList<ActionButton> ActionButtons
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a list of Spells known to this Character.
        /// </summary>
        public IList<Spell> Spells
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets whether this is the first time this Character has logged into the Shard.
        /// </summary>
        public bool FirstLogin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the unit that this Character is controlling. This is typically the same instance as the Character itself.
        /// </summary>
        public Unit Control
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Initializes this Character before it is added to the Shard for the first time.
        /// </summary>
        /// <param name="world">static world store</param>
        public override void Initialize(World.World world)
        {
            base.Initialize(world);

            // initialize default values
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
            
            RestState = RestState.Normal;
            WatchedFactionIndex = -1;

            // get race definition for this character and assign related unit values
            RaceDefinition rd = world.RaceDefinitions[Race];
            DisplayID = NativeDisplayID = rd.GetDisplayID(Sex);
            FactionTemplate = rd.FactionID;

            // get model definition for this character and assign related unit values
            ModelDefinition md = ModelDefinition.Default;
            if (world.ModelDefinitions.ContainsKey(DisplayID))
                md = world.ModelDefinitions[DisplayID];
            else
                log.WarnFormat("model bounding info not found for player {0} with display id {1}", Name, DisplayID);

            BoundingRadius = md.BoundingRadius * Scale;
            CombatReach = md.CombatReach * Scale;

            // get player base stats and assign. probably not the best spot for this
            // TODO: get healthmax and manamax out of here
            CharacterBaseStats cbs = world.CharacterBaseStats[Race][Class][(byte)Level];
            HealthMax = Health = HealthBase = cbs.Health;
            ManaMax = Mana = ManaBase = cbs.Mana;
            Strength = cbs.Strength;
            Agility = cbs.Agility;
            Stamina = cbs.Stamina;
            Intellect = cbs.Intellect;
            Spirit = cbs.Spirit;
        }

        /// <summary>
        /// Returns the name of this Character.
        /// </summary>
        /// <returns>the name of this Character</returns>
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
