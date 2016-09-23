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

namespace Whisper.Daemon.Shard.Lookup
{
    /// <summary>
    /// Represents the opcodes that a client can send to the server.
    /// </summary>
    public enum ShardClientOpcode : uint
    {
        Noop = 0x0000,
        
        WorldTeleport = 0x0008,
        TeleportToUnit = 0x0009,
        ZoneMap = 0x000a,
        CharacterCreate = 0x0036,
        CharacterList = 0x0037,
        CharacterDelete = 0x0038,
        PlayerLogin = 0x003d,
        PlayerLogout = 0x004a,
        PlayerLogoutRequest = 0x004b,
        PlayerLogoutCancel = 0x004e,
        QueryName = 0x0050,
        QueryPetName = 0x0052,
        QueryGuild = 0x0054,
        QueryItem = 0x0056,
        QueryPageTest = 0x005a,
        QueryQuest = 0x005c,
        QueryGameObject = 0x005e,
        QueryCreature = 0x0060,
        Who = 0x0062,
        WhoIs = 0x0064,
        FriendList = 0x0066,
        FriendAdd = 0x0069,
        FriendDelete = 0x006a,
        QueryTime = 0x01ce,
        Ping = 0x01dc,
        AuthSession = 0x01ed,
        AccountDataUpdate = 0x020b,
        SupportTicketQuery = 0x0211,
        SetControlledUnit = 0x026a,
        QueryRaidInfo = 0x02cd,
    }
}
