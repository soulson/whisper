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
using System.Numerics;
using log4net;

namespace Whisper.Shared.Utility
{
    public static class BigIntegers
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BigIntegers));

        public static BigInteger FromUnsignedByteArray(byte[] input)
        {
            // sign bit
            if ((input[input.Length - 1] & 0x80) != 0)
            {
                byte[] padded = new byte[input.Length + 1];
                Array.Copy(input, padded, input.Length);
                padded[input.Length] = 0;
                return new BigInteger(padded);
            }
            else
                return new BigInteger(input);
        }
    }
}
