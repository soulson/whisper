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

using System.Globalization;
using System.Numerics;

namespace Whisper.Daemon.Auth.Net
{
    public sealed class SecureRemotePasswordArgs
    {
        public SecureRemotePasswordArgs()
        {
            N = BigInteger.Parse("0894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", NumberStyles.AllowHexSpecifier);
            g = 7;
            k = 3;
        }

        public BigInteger N
        {
            get;
            private set;
        }

        public BigInteger g
        {
            get;
            private set;
        }

        public BigInteger k
        {
            get;
            private set;
        }

        public BigInteger v
        {
            get;
            set;
        }

        public BigInteger u
        {
            get;
            set;
        }

        public BigInteger s
        {
            get;
            set;
        }

        public BigInteger S
        {
            get;
            set;
        }

        public BigInteger K
        {
            get;
            set;
        }

        public BigInteger B
        {
            get;
            set;
        }

        public BigInteger b
        {
            get;
            set;
        }

        public BigInteger A
        {
            get;
            set;
        }

        public string I
        {
            get;
            set;
        }
    }
}
