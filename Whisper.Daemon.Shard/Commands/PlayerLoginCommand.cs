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
using Whisper.Game.Units;

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

            // initialize the character if it has never logged in before now
            if (player.FirstLogin)
            {
                log.DebugFormat("character '{0}' first login, initializing", player.Name);
                player.Initialize(session.Server.World);
            }

            // set session status to ingame and add the player to the Shard
            session.Player = player;
            session.Status = SessionStatus.Ingame;
            session.Server.Shard.AddCharacter(player);

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
                // TODO: implement bind point other than initial world position
                CharacterTemplate ct = session.Server.World.CharacterTemplates[player.Race][player.Class];

                packet.Append(ct.PositionX); // x
                packet.Append(ct.PositionY); // y
                packet.Append(ct.PositionZ); // z
                packet.Append(ct.MapID); // map id
                packet.Append(ct.ZoneID); // zone id

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
                for (int i = 0; i < Character.MaxActionButtons; ++i)
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

            // trigger cinematic if this is the character's first login
            if (player.FirstLogin)
            {
                RaceDefinition rd;
                if (session.Server.World.RaceDefinitions.TryGetValue(player.Race, out rd))
                    session.Send(ShardServerOpcode.TriggerCinematic, BitConverter.GetBytes(rd.FirstLoginCinematicID));
                else
                    log.WarnFormat("cannot send first login cinematic for undefined race '{0}'", player.Race);
            }

            // send object state
            using (ByteBuffer packet = new ByteBuffer())
            {
                using (UpdateData updateData = new UpdateData())
                {
                    player.BuildTargetedCreationUpdate(updateData, player);
                    updateData.Append(packet);

                    session.Send(ShardServerOpcode.ObjectUpdate, packet);
                }
            }

            // send friend list and ignore list (empty for now)
            session.Send(ShardServerOpcode.FriendList, (byte)0);
            session.Send(ShardServerOpcode.IgnoreList, (byte)0);

            // initialize world state
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append(player.MapID);
                packet.Append(player.ZoneID);

                packet.Append((ushort)0); // special map int64s (for battleground, outdoor pvp areas)

                session.Send(ShardServerOpcode.InitializeWorldState, packet);
            }
        }
    }
}
