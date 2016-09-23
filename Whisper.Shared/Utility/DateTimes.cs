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

namespace Whisper.Shared.Utility
{
    public static class DateTimes
    {
        public static int GetBitfield(DateTime time)
        {
            return unchecked((time.Year - 100) << 24 | time.Month << 20 | (time.Day - 1) << 14 | ((byte)time.DayOfWeek) << 11 | time.Hour << 6 | time.Minute);
        }
    }
}
