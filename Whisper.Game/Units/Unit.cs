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
using Whisper.Game.Objects;
using Whisper.Shared.Utility;

namespace Whisper.Game.Units
{
    public class Unit : GameObject
    {
        public Unit(ObjectID id, ObjectTypeID typeId) : this(id, typeId, (ushort)UnitFields.END)
        {
        }

        protected Unit(ObjectID id, ObjectTypeID typeId, ushort fieldsCount) : base(id, typeId, fieldsCount)
        {
            MovementFlags = MovementFlags.None;
            MovementSpeed = new MovementSpeed();
        }

        public MovementFlags MovementFlags
        {
            get;
            protected set;
        }

        public MovementSpeed MovementSpeed
        {
            get;
            private set;
        }

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

        protected override ObjectUpdateFlags UpdateFlags
        {
            get
            {
                return base.UpdateFlags | ObjectUpdateFlags.Living;
            }
        }

        protected override void AppendMovementUpdate(ByteBuffer buffer, ObjectUpdateType updateType, ObjectUpdateFlags updateFlags)
        {
            base.AppendMovementUpdate(buffer, updateType, updateFlags);

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
        }
    }
}
