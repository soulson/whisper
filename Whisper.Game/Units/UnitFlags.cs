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

using System;

namespace Whisper.Game.Units
{
    /// <remarks>
    /// UnitFlags.Flags
    /// </remarks>
    [Flags]
    public enum UnitFlags : uint
    {
        None = 0x00000000,
        NotAttackable = 0x00000002,
        DisableMovement = 0x00000004,
        PvPUnit = 0x00000008,
        Rename = 0x00000010,
        Resting = 0x00000020,
        NotAttackableWhenOutOfCombat = 0x00000100,
        CannotAttack = 0x00000200,
        PvPFlagged = 0x00001000,
        Pacified = 0x00020000,
        DisableRotation = 0x00040000,
        InCombat = 0x00080000,
        NotSelectable = 0x02000000,
        Skinnable = 0x04000000,
        DetectMagic = 0x08000000,
        Sheath = 0x40000000,
    }

    /// <remarks>
    /// UnitFlags.NpcFlags
    /// </remarks>
    [Flags]
    public enum NpcFlags : uint
    {
        None = 0x00000000,
        Gossip = 0x00000001,
        QuestGiver = 0x00000002,
        Vendor = 0x00000004,
        FlightMaster = 0x00000008,
        Trainer = 0x00000010,
        SpiritHealer = 0x00000020,
        SpiritGuide = 0x00000040,
        Innkeeper = 0x00000080,
        Banker = 0x00000100,
        Petitioner = 0x00000200,
        TabardDesigner = 0x00000400,
        Battlemaster = 0x00000800,
        Auctioneer = 0x00001000,
        StableMaster = 0x00002000,
        Repairman = 0x00004000,
    }

    /// <remarks>
    /// UnitFlags.Bytes1[3]
    /// </remarks>
    [Flags]
    public enum UnitFlags1 : byte
    {
        AlwaysStand = 0x01,
        Creep = 0x02,
        Untrackable = 0x04,
    }

    /// <remarks>
    /// UnitFlags.Bytes2[1]
    /// </remarks>
    [Flags]
    public enum UnitFlags2 : byte
    {
        Supportable = 0x08,
        CanHaveAuras = 0x10,
    }
}
