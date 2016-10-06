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
using Whisper.Game.Characters;
using Whisper.Game.Units;
using Whisper.Shared.Math;
using Whisper.Shared.Utility;
using log4net;

namespace Whisper.Game.Objects
{
    public class GameObject
    {
        private readonly ILog log = LogManager.GetLogger(typeof(GameObject));

        private uint[] fields;
        private ObjectTypeID typeId;

        public GameObject(ObjectID id, ObjectTypeID typeId) : this(id, typeId, (ushort)ObjectFields.END)
        {
        }

        protected GameObject(ObjectID id, ObjectTypeID typeId, ushort fieldsCount)
        {
            if (fieldsCount < (ushort)ObjectFields.END)
                throw new ArgumentOutOfRangeException(nameof(fieldsCount), "field count must be at least the minimum for ObjectFields");

            // must go first. other properties may actually be backed by this array
            fields = new uint[fieldsCount];

            ID = id;
            TypeID = typeId;
            Scale = 1.0f;
        }

        #region Object Fields
        public ObjectID ID
        {
            get
            {
                return new ObjectID(GetFieldLong(ObjectFields.ID));
            }
            private set
            {
                SetField(ObjectFields.ID, value.LongForm);
            }
        }

        public float Scale
        {
            get
            {
                return GetFieldFloat(ObjectFields.Scale);
            }
            set
            {
                SetField(ObjectFields.Scale, value);
            }
        }

        public ObjectTypeID TypeID
        {
            get
            {
                return typeId;
            }
            private set
            {
                ObjectTypeMask typeMask = ObjectTypeMask.None;
                switch(value)
                {
                    case ObjectTypeID.Player:
                        typeMask |= ObjectTypeMask.Player;
                        goto case ObjectTypeID.Unit;

                    case ObjectTypeID.Unit:
                        typeMask |= ObjectTypeMask.Unit;
                        goto case ObjectTypeID.Object;

                    case ObjectTypeID.Object:
                        typeMask |= ObjectTypeMask.Object;
                        break;

                    default:
                        throw new ArgumentException($"illegal ObjectTypeID value '{value}'");
                }

                SetField(ObjectFields.Type, (uint)typeMask);
                typeId = value;
            }
        }
        #endregion

        #region Object Properties
        public int MapID
        {
            get;
            set;
        }

        public OrientedVector3 Position
        {
            get;
            set;
        }
        #endregion

        #region Fields Management
        public int FieldCount
        {
            get
            {
                return fields.Length;
            }
        }

        protected uint GetFieldUnsigned(ushort index)
        {
            return fields[index];
        }

        protected unsafe int GetFieldSigned(ushort index)
        {
            uint field = fields[index];
            return *(int*)&field;
        }

        protected ulong GetFieldLong(ushort index)
        {
            return fields[index] | ((ulong)fields[index + 1] << 32);
        }

        protected byte GetFieldByte(ushort fieldIndex, byte byteIndex)
        {
            uint bytes = GetFieldUnsigned(fieldIndex);
            bytes &= (uint)0x000000ff << (byteIndex * 8);
            return (byte)(bytes >> (byteIndex * 8));
        }

        protected unsafe float GetFieldFloat(ushort index)
        {
            uint field = fields[index];
            return *(float*)&field;
        }

        protected void SetField(ushort index, uint value)
        {
            fields[index] = value;
        }

        protected void SetField(ushort index, ulong value)
        {
            fields[index] = (uint)(value & 0xffffffff);
            fields[index + 1] = (uint)(value >> 32);
        }

        protected unsafe void SetField(ushort index, int value)
        {
            fields[index] = *(uint*)&value;
        }

        protected void SetField(ushort fieldIndex, byte byteIndex, byte value)
        {
            uint bytes = GetFieldUnsigned(fieldIndex);
            bytes &= ~((uint)0x000000ff << (byteIndex * 8));
            SetField(fieldIndex, bytes | ((uint)value << (byteIndex * 8)));
        }

        protected unsafe void SetField(ushort index, float value)
        {
            fields[index] = *(uint*)&value;
        }

        public uint GetFieldUnsigned(ObjectFields field)
        {
            return GetFieldUnsigned((ushort)field);
        }

        public int GetFieldSigned(ObjectFields field)
        {
            return GetFieldSigned((ushort)field);
        }

        public ulong GetFieldLong(ObjectFields field)
        {
            return GetFieldLong((ushort)field);
        }

        public float GetFieldFloat(ObjectFields field)
        {
            return GetFieldFloat((ushort)field);
        }

        public void SetField(ObjectFields field, uint value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(ObjectFields field, int value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(ObjectFields field, ulong value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(ObjectFields field, float value)
        {
            SetField((ushort)field, value);
        }
        #endregion

        #region Update Management
        protected virtual ObjectUpdateFlags UpdateFlags
        {
            get
            {
                return ObjectUpdateFlags.HasPosition | ObjectUpdateFlags.All;
            }
        }

        protected virtual void AppendBasicUpdate(ByteBuffer buffer, ObjectUpdateType updateType, ObjectUpdateFlags updateFlags)
        {
            buffer.Append((byte)updateType);
            buffer.Append(ID.GetPacked());
            buffer.Append((byte)TypeID);
            buffer.Append((byte)updateFlags);
        }

        protected virtual void AppendMovementUpdate(ByteBuffer buffer, ObjectUpdateType updateType, ObjectUpdateFlags updateFlags)
        {

        }

        protected virtual void AppendValuesUpdate(ByteBuffer buffer, ObjectUpdateType updateType, ObjectUpdateFlags updateFlags, UpdateMask updateMask)
        {
            buffer.Append(updateMask.BlockCount);
            buffer.Append(updateMask.Data);

            // write the value set
            for (ushort i = 0; i < FieldCount; ++i)
            {
                if (updateMask.GetBit(i))
                    buffer.Append(GetFieldUnsigned(i));
            }
        }

        public void BuildTargetedCreationUpdate(UpdateData data, Character character)
        {
            // TODO: review how to pick which CreateObject to use
            ObjectUpdateType updateType = ObjectUpdateType.CreateObject2;
            ObjectUpdateFlags updateFlags = UpdateFlags;

            if (character == this)
                updateFlags |= ObjectUpdateFlags.Self;

            using (ByteBuffer buffer = new ByteBuffer())
            {
                // basic update
                AppendBasicUpdate(buffer, updateType, updateFlags);

                // movement update
                if((updateFlags & ObjectUpdateFlags.Living) != 0)
                    AppendMovementUpdate(buffer, updateType, updateFlags);
                else if((updateFlags & ObjectUpdateFlags.HasPosition) != 0)
                    buffer.Append(Position);

                if ((updateFlags & ObjectUpdateFlags.MaskID) != 0)
                    buffer.Append(ID.MaskID);

                if ((updateFlags & ObjectUpdateFlags.All) != 0)
                    buffer.Append(1); // ?

                // values update
                UpdateMask updateMask = BuildCreationUpdateMask();
                AppendValuesUpdate(buffer, updateType, updateFlags, updateMask);

                // append the update
                data.AddUpdateBlock(buffer);
            }
        }

        protected UpdateMask BuildCreationUpdateMask()
        {
            UpdateMask updateMask = new UpdateMask(FieldCount);
            for (ushort i = 0; i < FieldCount; ++i)
            {
                if (GetFieldUnsigned(i) != 0)
                    updateMask.SetBit(i);
            }
            return updateMask;
        }
        #endregion
    }
}
