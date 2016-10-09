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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Net;
using Whisper.Game.Objects;
using Whisper.Game.Units;
using Whisper.Shared.Math;

namespace Whisper.Daemon.Shard.Commands.Base
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MovementHeader
    {
        private ClientPacketHeader Header;
        public MovementFlags Flags;
        public uint Time;
        public OrientedVector3 Position;
    }

    public abstract class MovementCommandBase : ShardCommandBase<MovementHeader>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(MovementCommandBase));

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
                return CommandThreadSafety.NotThreadSafe;
            }
        }

        public override void ExecuteCommand(ShardSession session, ShardRequest request, MovementHeader header)
        {
            ObjectID? transportId = null;
            OrientedVector3? transportPosition = null;
            float? pitch = null;
            uint fallTime = 0;
            float? jumpVelocity = null;
            float? jumpSinAngle = null;
            float? jumpCosAngle = null;
            float? jumpXySpeed = null;

            using (BinaryReader reader = new BinaryReader(new MemoryStream(request.Packet, false), Encoding.UTF8, false))
            {
                reader.BaseStream.Seek(Marshal.SizeOf<MovementHeader>(), SeekOrigin.Begin);

                if ((header.Flags & MovementFlags.OnTransport) != 0)
                {
                    transportId = new ObjectID(reader.ReadUInt64());
                    transportPosition = new OrientedVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }

                if ((header.Flags & MovementFlags.ModeSwimming) != 0)
                    pitch = reader.ReadSingle();

                fallTime = reader.ReadUInt32();

                if((header.Flags & MovementFlags.ModeFalling) != 0)
                {
                    jumpVelocity = reader.ReadSingle();
                    jumpSinAngle = reader.ReadSingle();
                    jumpCosAngle = reader.ReadSingle();
                    jumpXySpeed = reader.ReadSingle();
                }
            }

            // TODO: implement falling, swimming, transport
            // TODO: verify that client isn't sending bogus data
            session.Player.Control.MovementFlags = header.Flags & ~(MovementFlags.ModeFalling | MovementFlags.ModeSwimming | MovementFlags.OnTransport);
            session.Player.Control.Position = header.Position;
        }
    }
}
