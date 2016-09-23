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
using System.Security.Cryptography;
using Whisper.Shared.Utility;

namespace Whisper.Shared.Security
{
    public class SecureRandom : IRandomSource
    {
        // specifically naming RNGCryptoSeerviceProvider because it is guaranteed to be thread-safe
        protected readonly RNGCryptoServiceProvider random;

        public SecureRandom()
        {
            random = new RNGCryptoServiceProvider();
        }

        public virtual byte[] GetRandomBytes(int size)
        {
            byte[] array = new byte[size];
            random.GetBytes(array);
            return array;
        }

        public virtual BigInteger GetRandomBigInteger(int sizeInBytes)
        {
            return BigIntegers.FromUnsignedByteArray(GetRandomBytes(sizeInBytes));
        }

        public virtual int GetRandomInt()
        {
            return BitConverter.ToInt32(GetRandomBytes(sizeof(int)), 0);
        }

        public virtual int GetRandomInt(int exclusiveUpperBound)
        {
            if (exclusiveUpperBound < 1)
                throw new ArgumentException("exclusiveUpperBound must be at least 1", "exclusiveUpperBound");

            // the cast to long here is to make sure Abs doesn't crash if GetRandomInt returns int.MinValue
            return (int)(System.Math.Abs((long)GetRandomInt()) % exclusiveUpperBound);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    random.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SecureRandom() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
