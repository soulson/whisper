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
using System.IO;
using System.Runtime.InteropServices;

namespace Whisper.Game.Maps
{
    public class MapTile
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MapTileHeader
        {
            public uint MapMagic;
            public uint VersionMagic;
            public uint AreaMapOffset;
            public uint AreaMapSize;
            public uint HeightMapOffset;
            public uint HeightMapSize;
            public uint LiquidMapOffset;
            public uint LiquidMapSize;
            public uint HolesOffset;
            public uint HolesSize;
        }

        [Flags]
        private enum AreaFlags : ushort
        {
            None = 0x00,
            NoArea = 0x01,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MapTileAreaHeader
        {
            public uint AreaMagic;
            public AreaFlags Flags;
            public ushort GridArea;
        }

        [Flags]
        private enum HeightFlags : uint
        {
            None = 0x00,
            NoHeight = 0x01,
            UShortHeight = 0x02,
            ByteHeight = 0x04,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MapTileHeightHeader
        {
            public uint HeightMagic;
            public HeightFlags Flags;
            public float GridHeight;
            public float GridMaxHeight;
        }

        [Flags]
        public enum LiquidFlags : ushort
        {
            None = 0x00,
            NoType = 0x01,
            NoHeight = 0x02,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MapTileLiquidHeader
        {
            public uint LiquidMagic;
            public LiquidFlags Flags;
            public ushort LiquidType;
            public byte OffsetX;
            public byte OffsetY;
            public byte Width;
            public byte Height;
            public float LiquidLevel;
        }
    }
}
