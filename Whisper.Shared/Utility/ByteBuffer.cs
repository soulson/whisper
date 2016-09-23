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
using System.Text;

namespace Whisper.Shared.Utility
{
    public class ByteBuffer : IDisposable
    {
        private MemoryStream stream;
        private BinaryWriter writer;

        public ByteBuffer()
        {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
        }

        public long Size
        {
            get
            {
                return stream.Length;
            }
        }

        public byte[] GetBytes()
        {
            return stream.ToArray();
        }

        public ArraySegment<byte> GetArraySegment()
        {
            return new ArraySegment<byte>(GetBytes());
        }

        public void Append<T>(T value) where T : struct
        {
            byte[] arr = new byte[Marshal.SizeOf<T>()];
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            try
            {
                Marshal.StructureToPtr(value, ptr, false);
                Marshal.Copy(ptr, arr, 0, arr.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            Append(arr);
        }

        public void Append(long value)
        {
            writer.Write(value);
        }

        public void Append(ulong value)
        {
            writer.Write(value);
        }

        public void Append(int value)
        {
            writer.Write(value);
        }

        public void Append(uint value)
        {
            writer.Write(value);
        }

        public void Append(short value)
        {
            writer.Write(value);
        }

        public void Append(ushort value)
        {
            writer.Write(value);
        }

        public void Append(byte value)
        {
            writer.Write(value);
        }

        public void Append(float value)
        {
            writer.Write(value);
        }

        public void Append(string value)
        {
            Append(value, Encoding.UTF8);
        }

        public void Append(string value, Encoding encoding)
        {
            writer.Write(encoding.GetBytes(value));
            writer.Write((byte)0); // null terminator
        }

        public void Append(byte[] value)
        {
            writer.Write(value);
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
                    writer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ByteBuffer() {
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
