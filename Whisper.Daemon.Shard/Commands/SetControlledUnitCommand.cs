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
using System.Runtime.InteropServices;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Net;
using Whisper.Game.Objects;
using log4net;

namespace Whisper.Daemon.Shard.Commands
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SetControlledUnit
    {
        private ClientPacketHeader Header;
        public ObjectID UnitID;
    }

    public sealed class SetControlledUnitCommand : ShardCommandBase<SetControlledUnit>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(SetControlledUnitCommand));

        public override string Name
        {
            get
            {
                return ShardClientOpcode.SetControlledUnit.ToString();
            }
        }

        public override SessionStatus PermissibleStatus
        {
            get
            {
                return SessionStatus.Ingame;
            }
        }

        public override CommandThreadSafety ThreadSafety
        {
            get
            {
                return CommandThreadSafety.Immediate;
            }
        }

        public override void ExecuteCommand(ShardSession session, ShardRequest request, SetControlledUnit header)
        {
            if (header.UnitID != session.Player.ID)
                log.WarnFormat("received {0} with incorrect unit id {1}. it should be {2}", ShardClientOpcode.SetControlledUnit, header.UnitID, session.Player.ID);
        }
    }
}
