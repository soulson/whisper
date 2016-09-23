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
using System.Collections.Generic;
using Whisper.Shared.Utility;

namespace Whisper.Game.Objects
{
    public class UpdateData : IDisposable
    {
        protected int blockCount;
        protected IList<ObjectID> outOfRangeIDs;
        protected ByteBuffer buffer;

        public UpdateData()
        {
            blockCount = 0;
            outOfRangeIDs = new List<ObjectID>();
            buffer = new ByteBuffer();
        }

        public void AddOutOfRangeIDs(params ObjectID[] ids)
        {
            throw new NotImplementedException();
        }

        public void AddUpdateBlock(ByteBuffer block)
        {
            buffer.Append(block.GetBytes());
            ++blockCount;
        }

        public void Append(ByteBuffer to)
        {
            int writeBlockCount = blockCount;
            if (outOfRangeIDs.Count > 0)
                ++writeBlockCount;

            to.Append(writeBlockCount);
            to.Append((byte)0); // hasTransport

            to.Append(buffer.GetBytes());
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
                    buffer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UpdateData() {
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
