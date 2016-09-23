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
using System.Runtime.InteropServices;
using Whisper.Shared.Utility;

namespace Whisper.Game.Objects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ObjectID
    {
        private const ulong TypeMask = 0xffff000000000000;
    
        private ulong id;

        public ObjectID(ulong id, Type type)
        {
            this.id = (id & ~TypeMask) | ((ulong)type & TypeMask);
        }

        public unsafe ObjectID(int id, Type type)
        {
            // this is just a silly reinterpret cast. keeps the bits in order, doesn't overflow
            this.id = *(uint*)(&id) | (ulong)type;
        }

        public int ID
        {
            get
            {
                return checked((int)(id & ~TypeMask));
            }
            set
            {
                id = (id & TypeMask) | (checked((ulong)value) & ~TypeMask);
            }
        }

        public Type ObjectType
        {
            get
            {
                return (Type)(id & TypeMask);
            }
            set
            {
                id = id & ~TypeMask;
                id |= (ulong)value & TypeMask;
            }
        }

        public uint MaskID
        {
            get
            {
                return (uint)((id & TypeMask) >> 48);
            }
        }

        public ulong LongForm
        {
            get
            {
                return id;
            }
        }

        public unsafe byte[] GetPacked()
        {
            ulong id = this.id;
            byte* ptr = stackalloc byte[9];

            ptr[0] = 0;
            byte size = 1;

            for (byte i = 0; id != 0 && size < 9; ++i)
            {
                if ((id & 0xff) != 0)
                {
                    ptr[0] |= (byte)(1 << i);
                    ptr[size] = (byte)(id & 0xff);
                    ++size;
                }

                id >>= 8;
            }

            return FixedBuffers.ToArray(ptr, size);
        }

        public override string ToString()
        {
            return String.Format("0x{0:x16}", id);
        }

        public enum Type : ulong
        {
            Item = 0x4000000000000000,
            Container = 0x4000000000000000,
            Player = 0x0000000000000000,
            GameObject = 0xf110000000000000,
            Transport = 0xf120000000000000,
            Unit = 0xf130000000000000,
            Pet = 0xf140000000000000,
            DynamicObject = 0xf100000000000000,
            Corpse = 0xf101000000000000,
            MoTransport = 0x1fc0000000000000,
        }
    }
}
