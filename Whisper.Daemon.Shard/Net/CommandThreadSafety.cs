﻿/* 
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

namespace Whisper.Daemon.Shard.Net
{
    public enum CommandThreadSafety
    {
        /// <summary>
        /// Declares that a Command does not read or write any non-static data that it does not own.
        /// </summary>
        Immediate,

        /// <summary>
        /// Declares that a Command reads, but does not write, non-static data that it does not own.
        /// </summary>
        ThreadSafe,

        /// <summary>
        /// Declares that a Command writes to non-static data that it does not own.
        /// </summary>
        NotThreadSafe,
    }
}
