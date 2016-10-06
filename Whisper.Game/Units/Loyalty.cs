﻿/*
 * This file is part of the whisper project.
 * Copyright(C) 2016  soulson(a.k.a.foxic)
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

using System;

namespace Whisper.Game.Units
{
    public enum Loyalty : byte
    {
        None = 0,
        Rebellious = 1,
        Unruly = 2,
        Submissive = 3,
        Dependable = 4,
        Faithful = 5,
        BestFriend = 6,
    }
}