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

namespace Whisper.Shared.Math
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OrientedVector3
    {
        public float X, Y, Z;
        public float Orientation;

        public OrientedVector3(float x, float y, float z, float orientation)
        {
            X = x;
            Y = y;
            Z = z;
            Orientation = orientation;
        }

        public float Distance(OrientedVector3 other)
        {
            return (float)System.Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y) + (Z - other.Z) * (Z - other.Z));
        }
    }
}
