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

namespace Whisper.Daemon.Shard.Net
{
    public enum CommandThreadSafety
    {
        /// <summary>
        /// Declares that a Command does not read or write any dynamic data.
        /// Immediate commands are executed in parallel immediately by whichever thread received them.
        /// </summary>
        Immediate,

        /// <summary>
        /// Declares that a Command writes dynamic data owned by its session or reads dynamic data that its session does not own.
        /// ThreadSafe commands are executed in serial on a thread owned by the session that received the command.
        /// </summary>
        ThreadSafe,

        /// <summary>
        /// Declares that a Command writes to non-static data that its session does not own.
        /// NotThreadSafe commands are executed in serial on a thread owned by the shard.
        /// </summary>
        NotThreadSafe,
    }
}
