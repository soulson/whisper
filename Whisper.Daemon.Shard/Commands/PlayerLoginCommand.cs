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

using log4net;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Whisper.Daemon.Shard.Database;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Net;
using Whisper.Game.Objects;
using Whisper.Game.Characters;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Commands
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerLogin
    {
        private ClientPacketHeader Header;
        public ObjectID CharacterID;
    }

    public sealed class PlayerLoginCommand : ShardCommandBase<PlayerLogin>
    {
        private const int LoginEffectSpellID = 836;
        private const int FactionCount = 64;
        private const float GameSpeed = 0.01666667f;

        private readonly ILog log = LogManager.GetLogger(typeof(PlayerLoginCommand));

        public override string Name
        {
            get
            {
                return ShardClientOpcode.PlayerLogin.ToString();
            }
        }

        public override SessionStatus PermissibleStatus
        {
            get
            {
                return SessionStatus.Authenticated;
            }
        }

        public override CommandThreadSafety ThreadSafety
        {
            get
            {
                return CommandThreadSafety.NotThreadSafe;
            }
        }

        public override void ExecuteCommand(ShardSession session, ShardRequest request, PlayerLogin header)
        {
            log.DebugFormat("player logging in. loading character id = {0}", header.CharacterID);
            Character player = new CharacterDao().GetCharacterByID(session.Server.ShardDB, header.CharacterID);
            log.DebugFormat("character '{0}' loaded successfully", player.Name);

            // set proficiencies (hack)
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append((byte)2);
                packet.Append(0x00084400);

                session.Send(ShardServerOpcode.SetProficiency, packet);
            }
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append((byte)4);
                packet.Append(0x00000003);

                session.Send(ShardServerOpcode.SetProficiency, packet);
            }

            // send logon player response packet
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append(player.MapID);
                packet.Append(player.Position);

                session.Send(ShardServerOpcode.LoginVerifyWorld, packet);
            }

            // not sure what this does
            session.Send(ShardServerOpcode.AccountDataTimes, new byte[128]);

            // not sure what this does, either
            session.Send(ShardServerOpcode.LoginSetRestStart, BitConverter.GetBytes(0));

            // update bind point
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append(-8949.95f); // x
                packet.Append(-132.493f); // y
                packet.Append(-83.5312f); // z
                packet.Append(0); // map id
                packet.Append(12); // zone id

                session.Send(ShardServerOpcode.BindPointUpdate, packet);
            }

            // set tutorial state
            using (ByteBuffer buffer = new ByteBuffer())
            {
                for (int i = 0; i < 8; ++i)
                    buffer.Append(-1);

                session.Send(ShardServerOpcode.LoginTutorialFlags, buffer);
            }

            // set initial spells
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append((byte)0);
                packet.Append((ushort)player.Spells.Where(spell => spell.Enabled).Count());

                foreach (Character.Spell spell in player.Spells.Where(spell => spell.Enabled))
                {
                    packet.Append((ushort)spell.SpellID);
                    packet.Append((short)0);
                }

                packet.Append((ushort)0); // spell cooldown count

                session.Send(ShardServerOpcode.LoginInitializeSpells, packet);
            }

            // set initial action buttons
            using (ByteBuffer packet = new ByteBuffer())
            {
                for(int i = 0; i < Character.MaxActionButtons; ++i)
                {
                    if (i < player.ActionButtons.Count)
                        packet.Append(player.ActionButtons[i]);
                    else
                        packet.Append(0);
                }

                session.Send(ShardServerOpcode.LoginInitializeActionButtons, packet);
            }

            // set initial faction standing
            /*using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append(FactionCount);

                for(int i = 0; i < FactionCount; ++i)
                {
                    packet.Append((byte)0); // faction flags
                    packet.Append(0); // faction standing
                }

                session.Send(ShardServerOpcode.LoginInitializeFactions, packet);
            }*/

            // set initial time and speed
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append(DateTimes.GetBitfield(DateTime.Now));
                packet.Append(GameSpeed);

                session.Send(ShardServerOpcode.LoginSetTimeAndSpeed, packet);
            }

            // trigger cinematic
            session.Send(ShardServerOpcode.TriggerCinematic, BitConverter.GetBytes(0x00000051));

            // send object state
            using (ByteBuffer packet = new ByteBuffer())
            {
                using (UpdateData updateData = new UpdateData())
                {
                    player.BuildTargetedCreationUpdate(updateData, player);
                    updateData.Append(packet);
                    //packet.Append(Strings.HexDumpToBytes(MagicUpdateObject2));

                    session.Send(ShardServerOpcode.ObjectUpdate, packet);
                }
            }

            // send friend list and ignore list (empty for now)
            session.Send(ShardServerOpcode.FriendList, (byte)0);
            session.Send(ShardServerOpcode.IgnoreList, (byte)0);

            // set session status to ingame
            session.Status = SessionStatus.Ingame;

            // initialize world state
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append(player.MapID);
                packet.Append(player.ZoneID);

                packet.Append((ushort)0); // special map int64s (for battleground, outdoor pvp areas)

                session.Send(ShardServerOpcode.InitializeWorldState, packet);
            }

            // send login effect spell
            /*byte[] loginEffectSpellGo = new byte[] { 01, 02, 01, 02, 0x44, 03, 00, 00, 00, 01, 01, 02, 00, 00, 00, 00, 00, 00, 00, 00, 02, 00, 01, 02 };
            session.Send(ShardServerOpcode.SpellGo, loginEffectSpellGo);*/
        }

        // magic oracle packet
        private const string MagicUpdateObject =
@"09 00 00 00 00 02 81 16 40 01 10 01 00 00 00 02
5F 41 00 00 00 00 00 00 16 00 00 00 00 00 00 40
03 00 00 00 D0 17 00 00 00 00 80 3F 02 00 00 00
02 00 00 00 01 00 00 00 02 81 0E 40 01 10 01 00
00 00 02 5F 41 00 00 00 C0 00 00 0E 00 00 00 00
00 00 40 03 00 00 00 38 00 00 00 00 00 80 3F 02
00 00 00 02 00 00 00 01 00 00 00 23 00 00 00 23
00 00 00 02 81 10 40 01 10 01 00 00 00 02 5F 41
00 00 00 C0 00 00 10 00 00 00 00 00 00 40 03 00
00 00 73 05 00 00 00 00 80 3F 02 00 00 00 02 00
00 00 01 00 00 00 19 00 00 00 19 00 00 00 02 81
12 40 01 10 01 00 00 00 02 5F 41 00 00 00 00 00
00 12 00 00 00 00 00 00 40 03 00 00 00 37 00 00
00 00 00 80 3F 02 00 00 00 02 00 00 00 01 00 00
00 02 81 14 40 01 10 01 00 00 00 02 5F 41 00 00
00 C0 00 00 14 00 00 00 00 00 00 40 03 00 00 00
23 00 00 00 00 00 80 3F 02 00 00 00 02 00 00 00
01 00 00 00 19 00 00 00 19 00 00 00 02 81 18 40
01 10 01 00 00 00 02 5F 41 01 00 00 00 00 00 18
00 00 00 00 00 00 40 03 00 00 00 16 08 00 00 00
00 80 3F 02 00 00 00 02 00 00 00 05 00 00 00 FF
FF FF FF 02 81 1A 40 01 10 01 00 00 00 02 5F 41
01 00 00 00 00 00 1A 00 00 00 00 00 00 40 03 00
00 00 9F 00 00 00 00 00 80 3F 02 00 00 00 02 00
00 00 05 00 00 00 FF FF FF FF 02 81 1C 40 01 10
01 00 00 00 02 5F 41 20 00 00 00 00 00 1C 00 00
00 00 00 00 40 03 00 00 00 24 1B 00 00 00 00 80
3F 02 00 00 00 02 00 00 00 01 00 00 00 01 00 00
00 03 01 02 04 71 00 00 00 00 00 00 00 00 CD D7
0B C6 35 7E 04 C3 F9 0F A7 42 FD 97 56 3B 00 00
00 00 00 00 20 40 00 00 E0 40 00 00 90 40 71 1C
97 40 00 00 20 40 E0 0F 49 40 01 00 00 00 29 15
00 C0 74 1D 40 00 00 00 00 00 00 00 00 00 C0 DF
04 C2 0F 3C 19 00 00 0E 00 00 00 00 00 00 00 00
00 00 00 00 01 10 00 00 10 00 01 00 00 00 00 00
00 00 00 00 00 00 01 00 00 00 00 00 F0 3C 00 30
00 F0 03 00 00 00 00 00 00 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00 E0 B6 6D 1B 00 00 00 00
00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00 80 68 04 00 00 00 00 00
00 80 00 00 00 00 80 3F 00 00 00 00 20 00 00 00
00 00 00 02 00 00 00 19 00 00 00 00 00 80 3F 33
00 00 00 A5 00 00 00 64 00 00 00 33 00 00 00 A5
00 00 00 E8 03 00 00 64 00 00 00 01 00 00 00 01
00 00 00 01 08 01 00 08 00 00 00 54 0B 00 00 D0
07 00 00 D0 07 00 00 F4 FD 54 3E 00 00 C0 3F 32
00 00 00 32 00 00 00 25 49 A2 40 25 49 E2 40 00
EE 00 00 00 00 80 3F 14 00 00 00 14 00 00 00 14
00 00 00 17 00 00 00 17 00 00 00 2D 00 00 00 64
00 00 00 1F 00 00 00 00 28 00 00 0A 00 00 00 0A
00 00 00 B7 6D 1B 40 B7 6D 5B 40 01 05 05 05 00
00 00 02 01 00 00 00 D0 17 00 00 38 00 00 00 73
05 00 00 37 00 00 00 23 00 00 00 16 00 00 00 00
00 00 40 0E 00 00 00 00 00 00 40 10 00 00 00 00
00 00 40 12 00 00 00 00 00 00 40 14 00 00 00 00
00 00 40 18 00 00 00 00 00 00 40 1A 00 00 00 00
00 00 40 1C 00 00 00 00 00 00 40 90 01 00 00 06
00 00 00 01 00 01 00 08 00 00 00 01 00 01 00 5F
00 00 00 01 00 05 00 62 00 00 00 2C 01 2C 01 88
00 00 00 01 00 05 00 A2 00 00 00 01 00 05 00 E4
00 00 00 01 00 05 00 9F 01 00 00 01 00 01 00 02
00 00 00 3E 0A 57 3F 1C 6A 5E 3F AB 2C 54 3F 00
00 00 20 07 00 00 00 00 00 80 3F 00 00 80 3F 00
00 80 3F 00 00 80 3F 00 00 80 3F 00 00 80 3F 00
00 80 3F FF FF FF FF";

        private const string MagicUpdateObject2 =
 @"01 00 00 00 00
   03 01 02 04 71 00 00 00 00 00 00 00 00 CD D7
0B C6 35 7E 04 C3 F9 0F A7 42 FD 97 56 3B 00 00
00 00 00 00 20 40 00 00 E0 40 00 00 90 40 71 1C
97 40 00 00 20 40 E0 0F 49 40 01 00 00 00 29 15
00 C0 74 1D 40 00 00 00 00 00 00 00 00 00 C0 DF
04 C2 0F 3C 19 00 00 0E 00 00 00 00 00 00 00 00
00 00 00 00 01 10 00 00 10 00 01 00 00 00 00 00
00 00 00 00 00 00 01 00 00 00 00 00 F0 3C 00 30
00 F0 03 00 00 00 00 00 00 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00 E0 B6 6D 1B 00 00 00 00
00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
00 00 00 00 00 00 00 00 80 68 04 00 00 00 00 00
00 80 00 00 00 00 80 3F 00 00 00 00 20 00 00 00
00 00 00 02 00 00 00 19 00 00 00 00 00 80 3F 33
00 00 00 A5 00 00 00 64 00 00 00 33 00 00 00 A5
00 00 00 E8 03 00 00 64 00 00 00 01 00 00 00 01
00 00 00 01 08 01 00 08 00 00 00 54 0B 00 00 D0
07 00 00 D0 07 00 00 F4 FD 54 3E 00 00 C0 3F 32
00 00 00 32 00 00 00 25 49 A2 40 25 49 E2 40 00
EE 00 00 00 00 80 3F 14 00 00 00 14 00 00 00 14
00 00 00 17 00 00 00 17 00 00 00 2D 00 00 00 64
00 00 00 1F 00 00 00 00 28 00 00 0A 00 00 00 0A
00 00 00 B7 6D 1B 40 B7 6D 5B 40 01 05 05 05 00
00 00 02 01 00 00 00 D0 17 00 00 38 00 00 00 73
05 00 00 37 00 00 00 23 00 00 00 16 00 00 00 00
00 00 40 0E 00 00 00 00 00 00 40 10 00 00 00 00
00 00 40 12 00 00 00 00 00 00 40 14 00 00 00 00
00 00 40 18 00 00 00 00 00 00 40 1A 00 00 00 00
00 00 40 1C 00 00 00 00 00 00 40 90 01 00 00 06
00 00 00 01 00 01 00 08 00 00 00 01 00 01 00 5F
00 00 00 01 00 05 00 62 00 00 00 2C 01 2C 01 88
00 00 00 01 00 05 00 A2 00 00 00 01 00 05 00 E4
00 00 00 01 00 05 00 9F 01 00 00 01 00 01 00 02
00 00 00 3E 0A 57 3F 1C 6A 5E 3F AB 2C 54 3F 00
00 00 20 07 00 00 00 00 00 80 3F 00 00 80 3F 00
00 80 3F 00 00 80 3F 00 00 80 3F 00 00 80 3F 00
00 80 3F FF FF FF FF";
    }
}
