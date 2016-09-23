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
using Whisper.Shared.Utility;

namespace Whisper.Game.Objects
{
    public class UpdateMask
    {
        private byte[] blocks;

        public UpdateMask(int valueCount)
        {
            ValueCount = valueCount;
            BlockCount = (byte)((valueCount + 31) / 32);
            blocks = new byte[BlockCount * 4];
        }

        public UpdateMask(int valueCount, string hexDump)
        {
            blocks = Strings.HexDumpToBytes(hexDump);
            ValueCount = valueCount;
            BlockCount = (byte)((valueCount + 31) / 32);

            if (BlockCount * 4 != blocks.Length)
                throw new ArgumentException("valueCount did not match the size of hexDump", nameof(hexDump));
        }

        public int ValueCount
        {
            get;
            private set;
        }

        public byte BlockCount
        {
            get;
            private set;
        }

        public byte[] Data
        {
            get
            {
                return blocks;
            }
        }

        public void SetBit(int index)
        {
            blocks[index >> 3] |= (byte)(1 << (index & 0x7));
        }

        public void UnsetBit(int index)
        {
            blocks[index >> 3] &= (byte)~(1 << (index & 0x7));
        }

        public bool GetBit(int index)
        {
            return (blocks[index >> 3] & (1 << (index & 0x7))) != 0;
        }
    }
}
