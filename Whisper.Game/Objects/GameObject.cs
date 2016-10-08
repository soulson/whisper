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
using Whisper.Shared.Math;
using Whisper.Shared.Utility;
using log4net;
using System.IO;

namespace Whisper.Game.Objects
{
    public class GameObject
    {
        private readonly ILog log = LogManager.GetLogger(typeof(GameObject));
        
        private uint[] fields;
        private UpdateMask updateMask;
        private ObjectTypeID typeId;

        private bool positionUpdated;
        private OrientedVector3 position;

        public GameObject(ObjectID id, ObjectTypeID typeId) : this(id, typeId, (ushort)ObjectFields.END)
        {
        }

        protected GameObject(ObjectID id, ObjectTypeID typeId, ushort fieldsCount)
        {
            if (fieldsCount < (ushort)ObjectFields.END)
                throw new ArgumentOutOfRangeException(nameof(fieldsCount), "field count must be at least the minimum for ObjectFields");

            // must go first. other properties may actually be backed by this array
            fields = new uint[fieldsCount];
            updateMask = new UpdateMask(fieldsCount);

            ID = id;
            TypeID = typeId;
        }

        /// <summary>
        /// Used to set the initial values for a newly created instance of GameObject.
        /// </summary>
        /// <param name="world">World instance that provides static data about the game world</param>
        public virtual void Initialize(World.World world)
        {
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
            get
            {
                return position;
            }
            set
            {
                positionUpdated = true;
                position = value;
            }
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

        public byte[] GetRawFields()
        {
            byte[] buffer = new byte[sizeof(uint) * FieldCount];

            using (MemoryStream ms = new MemoryStream(buffer, true))
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                for (int i = 0; i < FieldCount; ++i)
                    bw.Write(fields[i]);
            }

            return buffer;
        }

        public void SetRawFields(byte[] buffer)
        {
            if (buffer.Length != sizeof(uint) * FieldCount)
                throw new ArgumentException($"buffer size must equal size of internal fields buffer ({sizeof(uint) * FieldCount})", nameof(buffer));

            for (int i = 0; i < FieldCount; ++i)
                fields[i] = BitConverter.ToUInt32(buffer, sizeof(uint) * i);
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
            if (fields[index] != value)
            {
                updateMask.SetBit(index);
                fields[index] = value;
            }
        }

        protected void SetField(ushort index, ulong value)
        {
            updateMask.SetBit(index);
            updateMask.SetBit(index + 1);

            fields[index] = (uint)(value & 0xffffffff);
            fields[index + 1] = (uint)(value >> 32);
        }

        protected unsafe void SetField(ushort index, int value)
        {
            if (fields[index] != *(uint*)&value)
            {
                updateMask.SetBit(index);
                fields[index] = *(uint*)&value;
            }
        }

        protected void SetField(ushort fieldIndex, byte byteIndex, byte value)
        {
            updateMask.SetBit(fieldIndex);

            uint bytes = GetFieldUnsigned(fieldIndex);
            bytes &= ~((uint)0x000000ff << (byteIndex * 8));
            SetField(fieldIndex, bytes | ((uint)value << (byteIndex * 8)));
        }

        protected unsafe void SetField(ushort index, float value)
        {
            if (fields[index] != *(uint*)&value)
            {
                updateMask.SetBit(index);
                fields[index] = *(uint*)&value;
            }
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

        protected virtual void AppendCreationUpdate(ByteBuffer buffer, ObjectUpdateType updateType, ObjectUpdateFlags updateFlags)
        {
            buffer.Append((byte)updateType);
            buffer.Append(ID.GetPacked());
            buffer.Append((byte)TypeID);
            buffer.Append((byte)updateFlags);
        }

        protected virtual void AppendMovementUpdate(ByteBuffer buffer, ObjectUpdateType updateType, ObjectUpdateFlags updateFlags)
        {
            if ((updateFlags & ObjectUpdateFlags.HasPosition) != 0)
                buffer.Append(Position);

            if ((updateFlags & ObjectUpdateFlags.MaskID) != 0)
                buffer.Append(ID.MaskID);

            if ((updateFlags & ObjectUpdateFlags.All) != 0)
                buffer.Append(1); // ?
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
            ObjectUpdateType updateType = ObjectUpdateType.CreateObject;
            ObjectUpdateFlags updateFlags = UpdateFlags;
            
            if (ID.ObjectType == ObjectID.Type.Corpse
             || ID.ObjectType == ObjectID.Type.DynamicObject
             || ID.ObjectType == ObjectID.Type.GameObject
             || ID.ObjectType == ObjectID.Type.Player
             || ID.ObjectType == ObjectID.Type.Unit)
                updateType = ObjectUpdateType.CreateObject2;

            if (character == this)
                updateFlags |= ObjectUpdateFlags.Self;

            using (ByteBuffer buffer = new ByteBuffer())
            {
                // creation update
                AppendCreationUpdate(buffer, updateType, updateFlags);

                // movement update
                AppendMovementUpdate(buffer, updateType, updateFlags);

                // values update
                UpdateMask updateMask = GetCreationUpdateMask();
                AppendValuesUpdate(buffer, updateType, updateFlags, updateMask);

                // append the update
                data.AddUpdateBlock(buffer);
            }
        }

        public void BuildTargetedUpdate(UpdateData data, Character character)
        {
            ObjectUpdateFlags updateFlags = UpdateFlags;

            if (character == this)
                updateFlags |= ObjectUpdateFlags.Self;

            if ((ChangeState & ObjectChangeState.Movement) != 0)
            {
                using (ByteBuffer buffer = new ByteBuffer())
                {
                    buffer.Append((byte)ObjectUpdateType.Movement);
                    buffer.Append(ID);

                    AppendMovementUpdate(buffer, ObjectUpdateType.Movement, updateFlags);

                    data.AddUpdateBlock(buffer);
                }
            }
            if ((ChangeState & ObjectChangeState.Values) != 0)
            {
                using (ByteBuffer buffer = new ByteBuffer())
                {
                    buffer.Append((byte)ObjectUpdateType.Values);
                    buffer.Append(ID.GetPacked()); // why is movement a full id and values is packed? no clue

                    AppendValuesUpdate(buffer, ObjectUpdateType.Values, updateFlags, updateMask);

                    data.AddUpdateBlock(buffer);
                }
            }
        }

        protected UpdateMask GetCreationUpdateMask()
        {
            UpdateMask updateMask = new UpdateMask(FieldCount);
            for (ushort i = 0; i < FieldCount; ++i)
            {
                if (GetFieldUnsigned(i) != 0)
                    updateMask.SetBit(i);
            }
            return updateMask;
        }

        protected UpdateMask GetUpdateMask()
        {
            return updateMask;
        }

        public virtual ObjectChangeState ChangeState
        {
            get
            {
                ObjectChangeState state = ObjectChangeState.None;

                if (!updateMask.IsEmpty)
                    state |= ObjectChangeState.Values;

                if (positionUpdated)
                    state |= ObjectChangeState.Movement;

                return state;
            }
        }

        public virtual void ClearChangeState()
        {
            updateMask.Clear();
            positionUpdated = false;
        }
        #endregion
    }
}
