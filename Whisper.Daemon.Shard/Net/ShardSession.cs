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
using SuperSocket.SocketBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using Whisper.Daemon.Shard.Database;
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Security;
using Whisper.Game.Characters;
using Whisper.Game.Objects;
using Whisper.Game.Units;
using Whisper.Game.World;
using Whisper.Shared.Net;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Net
{
    public class ShardSession : SessionBase<ShardSession, ShardRequest, SessionStatus>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ShardSession));
        private readonly ILog packetLog = LogManager.GetLogger("PacketLog");
        private readonly DateTime Epoch = new DateTime(1970, 1, 1);

        private readonly ConcurrentQueue<CommandClosure> commandQueue;
        
        /// <summary>
        /// Creates a new instance of the ShardSession class.
        /// </summary>
        public ShardSession() : base(SessionStatus.Connected)
        {
            commandQueue = new ConcurrentQueue<CommandClosure>();
            Cipher = new PacketCipher();
            AccountID = -1;
            Awareness = new HashSet<GameObject>();
        }

        /// <summary>
        /// Gets a reference to the ShardServer that owns this session.
        /// </summary>
        public ShardServer Server
        {
            get
            {
                return (ShardServer)AppServer;
            }
        }
        
        /// <summary>
        /// This cipher is used to encrypt and decrypt the headers of all packets sent and received by this session.
        /// </summary>
        public PacketCipher Cipher
        {
            get;
            private set;
        }

        /// <summary>
        /// This is a random integer used as a nonce during the client-shardserver authentication scheme.
        /// </summary>
        public int Seed
        {
            get;
            private set;
        }

        /// <summary>
        /// If this session has a Status of SessionStatus.Authenticated or SessionStatus.Ingame, then this will get the account ID of the authenticated account. Otherwise, it returns -1.
        /// </summary>
        public int AccountID
        {
            get;
            set;
        }

        /// <summary>
        /// If this session has a Status of SessionStatus.Ingame, then this will get the Character object representing the logged-in player. Otherwise, it returns null.
        /// </summary>
        public Character Player
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a Set of GameObjects that this session is aware of.
        /// </summary>
        protected ISet<GameObject> Awareness
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the cipher that encrypts and decrypts packet headers.
        /// </summary>
        /// <param name="sessionKey">A BigInteger object representing the session key agreed by client and server during authentication.</param>
        public void InitializeCipher(BigInteger sessionKey)
        {
            Cipher.Initialize(sessionKey);
        }

        /// <summary>
        /// Updates the state of the ShardSession to represent that the given amount of game time has passed.
        /// </summary>
        /// <param name="diff">amount of game time passed since the last call to Update</param>
        public void Update(TimeSpan diff)
        {
            // just aliases for brevity
            World world = Server.World;
            Game.World.Shard shard = Server.Shard;

            // go through objects in map
            foreach (GameObject go in shard.GetObjects(Player.MapID))
            {
                if (Awareness.Contains(go))
                {
                    // update, remove, or do nothing
                    if (go.Position.Distance(Player.Position) < world.ViewDistance)
                    {
                        // update or do nothing
                        if (go.ChangeState != ObjectChangeState.None)
                        {
                            // update
                            UpdateAwareObject(go);
                        }
                    }
                    else
                    {
                        // remove
                        RemoveAwareObject(go);
                    }
                }
                else
                {
                    // add or do nothing
                    if (go.Position.Distance(Player.Position) < world.ViewDistance)
                    {
                        // add
                        AddAwareObject(go);
                    }
                }
            }

            // next, go through objects in awareness set, in case any don't have the correct map ID or no longer exist, and remove them
            ISet<GameObject> removalSet = new HashSet<GameObject>();
            foreach (GameObject go in Awareness)
            {
                if (go.MapID != Player.MapID)
                    removalSet.Add(go);
                else if (!Server.Shard.Exists(go))
                    removalSet.Add(go);
            }

            foreach (GameObject go in removalSet)
            {
                // remove
                RemoveAwareObject(go);
            }
        }

        private void UpdateAwareObject(GameObject gameObject)
        {
            log.DebugFormat("{0} is aware of object {1} changing", Player, gameObject);

            using (ByteBuffer packet = new ByteBuffer())
            using (UpdateData updateData = new UpdateData())
            {
                gameObject.BuildTargetedUpdate(updateData, Player);
                updateData.Append(packet);

                Send(ShardServerOpcode.ObjectUpdate, packet);
            }

            // protip: don't send movement updates to yourself
            // TODO: should this be is Unit or is Character?
            if (gameObject is Character && gameObject != Player)
            {
                Unit unit = gameObject as Unit;
                ISet<ShardServerOpcode> opcodes = new HashSet<ShardServerOpcode>();

                if ((unit.MovementFlags & MovementFlags.MoveMask) != 0 && unit.OldMovementFlags == unit.MovementFlags)
                {
                    // heartbeat
                    opcodes.Add(ShardServerOpcode.MoveHeartbeat);
                }
                else
                {
                    // complicated
                    if ((unit.MovementFlags & MovementFlags.MoveLeft) != 0 && (unit.OldMovementFlags & MovementFlags.MoveLeft) == 0)
                        opcodes.Add(ShardServerOpcode.MoveStrafeStartLeft);
                    else if ((unit.MovementFlags & MovementFlags.MoveRight) != 0 && (unit.OldMovementFlags & MovementFlags.MoveRight) == 0)
                        opcodes.Add(ShardServerOpcode.MoveStrafeStartRight);
                    else if ((unit.MovementFlags & (MovementFlags.MoveLeft | MovementFlags.MoveRight)) == 0 && (unit.OldMovementFlags & (MovementFlags.MoveLeft | MovementFlags.MoveRight)) != 0)
                        opcodes.Add(ShardServerOpcode.MoveStrafeStop);

                    if ((unit.MovementFlags & MovementFlags.MoveForward) != 0 && (unit.OldMovementFlags & MovementFlags.MoveForward) == 0)
                        opcodes.Add(ShardServerOpcode.MoveStartForward);
                    else if ((unit.MovementFlags & MovementFlags.MoveBackward) != 0 && (unit.OldMovementFlags & MovementFlags.MoveBackward) == 0)
                        opcodes.Add(ShardServerOpcode.MoveStartBackward);
                    else if ((unit.MovementFlags & (MovementFlags.MoveForward | MovementFlags.MoveBackward)) == 0 && (unit.OldMovementFlags & (MovementFlags.MoveForward | MovementFlags.MoveBackward)) != 0)
                        opcodes.Add(ShardServerOpcode.MoveStop);
                    
                    if ((unit.MovementFlags & MovementFlags.TurnLeft) != 0 && (unit.OldMovementFlags & MovementFlags.TurnLeft) == 0)
                        opcodes.Add(ShardServerOpcode.MoveTurnStartLeft);
                    else if ((unit.MovementFlags & MovementFlags.TurnRight) != 0 && (unit.OldMovementFlags & MovementFlags.TurnRight) == 0)
                        opcodes.Add(ShardServerOpcode.MoveTurnStartRight);
                    else if ((unit.MovementFlags & (MovementFlags.TurnLeft | MovementFlags.TurnRight)) == 0 && (unit.OldMovementFlags & (MovementFlags.TurnLeft | MovementFlags.TurnRight)) != 0)
                        opcodes.Add(ShardServerOpcode.MoveTurnStop);
                }

                if (unit.Position.Orientation != unit.OldOrientation && (unit.MovementFlags & (MovementFlags.TurnLeft | MovementFlags.TurnRight)) == 0)
                    opcodes.Add(ShardServerOpcode.MoveSetOrientation);

                if (opcodes.Count > 0)
                {
                    using (ByteBuffer buf = new ByteBuffer())
                    {
                        buf.Append(unit.ID.GetPacked());

                        // TODO: extract this movement write into a class, since it shows up in a few different places
                        // TODO: while you do that, consider implementing some of these other movement modes
                        buf.Append((int)(unit.MovementFlags & ~(MovementFlags.ModeSwimming | MovementFlags.OnTransport | MovementFlags.ModeFalling)));
                        buf.Append(unit.MovementTime);
                        buf.Append(unit.Position);
                        buf.Append(0); // fall time

                        foreach (ShardServerOpcode opcode in opcodes)
                            Send(opcode, buf);
                    }
                }
            }
        }

        private void AddAwareObject(GameObject gameObject)
        {
            log.DebugFormat("{0} became aware of object {1}", Player, gameObject);

            using (ByteBuffer packet = new ByteBuffer())
            using (UpdateData updateData = new UpdateData())
            {
                gameObject.BuildTargetedCreationUpdate(updateData, Player);
                updateData.Append(packet);

                Send(ShardServerOpcode.ObjectUpdate, packet);
            }

            Awareness.Add(gameObject);
        }

        private void RemoveAwareObject(GameObject gameObject)
        {
            log.DebugFormat("{0} lost awareness of object {1}", Player, gameObject);

            if (Awareness.Contains(gameObject))
            {
                Awareness.Remove(gameObject);
                Send(ShardServerOpcode.ObjectDestroy, BitConverter.GetBytes(gameObject.ID.LongForm));
            }
            else
                log.WarnFormat("{0} called on session for player {1} with unknown object ID {2}", nameof(RemoveAwareObject), Player.Name, gameObject.ID);
        }
        
        /// <summary>
        /// Executes all queued commands synchronously on the currently executing Thread.
        /// </summary>
        public void ProcessQueuedCommands()
        {
            CommandClosure command;
            while (commandQueue.TryDequeue(out command))
            {
                try
                {
                    ExecuteQueuedCommand(command);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("{0} uncaught during ProcessQueuedCommands{1}{2}", ex.GetType().Name, Environment.NewLine, ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Executes a command from the command queue.
        /// </summary>
        /// <param name="command">closure for the command to execute</param>
        protected virtual void ExecuteQueuedCommand(CommandClosure command)
        {
            command();
        }

        /// <summary>
        /// Enqueues a command to be executed on the next update tick.
        /// </summary>
        /// <param name="command">command to execute</param>
        public void EnqueueCommand(CommandClosure command)
        {
            commandQueue.Enqueue(command);
        }

        /// <summary>
        /// Sends a QueryTime packet to the client with the current result of time(null) as its payload.
        /// </summary>
        public void SendQueryTimePacket()
        {
            // this is vulnerable to the year 2038 bug. remind me to fix that if i'm still working on this app in 22 years
            uint time = (uint)((DateTime.Now - Epoch).Ticks / 1000);
            Send(ShardServerOpcode.QueryTime, BitConverter.GetBytes(time));
        }

        /// <summary>
        /// Sends the specified ArraySegment as a packet to the client.
        /// </summary>
        /// <param name="segment">packet</param>
        public override void Send(ArraySegment<byte> segment)
        {
            if (packetLog.IsInfoEnabled && segment.Count >= 4)
            {
                ShardServerOpcode opcode = (ShardServerOpcode)BitConverter.ToUInt16(segment.Array, segment.Offset + 2);
                ushort size = BitConverter.ToUInt16(new byte[] { segment.Array[segment.Offset + 1], segment.Array[segment.Offset] }, 0); // big endian
                string hexDump = Strings.HexDump(segment.Array, segment.Offset + 4, size - sizeof(ushort)); // sizeof the opcode

                packetLog.InfoFormat("Server [{0}] -> Client [{1}]", LocalEndPoint.ToString(), RemoteEndPoint.ToString());
                packetLog.InfoFormat("Opcode: {0} [0x{1:x4}]", opcode, (ushort)opcode);
                packetLog.InfoFormat("Length: {0}", size - sizeof(ushort));
                packetLog.InfoFormat("{1}{0}{1}", hexDump, Environment.NewLine);
            }

            Cipher.EncryptHeader(segment);
            base.Send(segment);
        }

        /// <summary>
        /// Sends a segment of the specified byte array as a packet to the client.
        /// </summary>
        /// <param name="data">buffer</param>
        /// <param name="offset">offset into buffer from which to start sending</param>
        /// <param name="length">number of bytes to send</param>
        public sealed override void Send(byte[] data, int offset, int length)
        {
            Send(new ArraySegment<byte>(data, offset, length));
        }

        /// <summary>
        /// Sends a packet to the client constructed by prepending the size and opcode to a provided ByteBuffer.
        /// </summary>
        /// <param name="opcode">opcode</param>
        /// <param name="packet">packet contents</param>
        public void Send(ShardServerOpcode opcode, ByteBuffer packet)
        {
            Send(opcode, packet.GetBytes());
        }

        /// <summary>
        /// Sends a packet to the client constructed of an opcode and a single byte of packet data. Frequently useful for failure response codes.
        /// </summary>
        /// <param name="opcode">opcode</param>
        /// <param name="responseCode">packet contents</param>
        public void Send(ShardServerOpcode opcode, byte responseCode)
        {
            Send(opcode, new byte[] { responseCode });
        }

        /// <summary>
        /// Sends a packet to the client constructed by prepending the size and opcode to a provided byte array.
        /// </summary>
        /// <param name="opcode">opcode</param>
        /// <param name="packet">packet contents</param>
        public void Send(ShardServerOpcode opcode, byte[] packet)
        {
            using (ByteBuffer output = new ByteBuffer())
            {
                // the extra ushort is the opcode
                output.Append(Arrays.Reverse(BitConverter.GetBytes((ushort)(packet.Length + sizeof(ushort)))));
                output.Append((ushort)opcode);
                output.Append(packet);

                log.DebugFormat("sending {0} packet to client. size is {1} bytes including header", opcode, output.Size);

                Send(output.GetArraySegment());
            }
        }

        protected override void OnSessionStarted()
        {
            base.OnSessionStarted();

            // can't initialize this in constructor because Server reference is null at that time
            Seed = Server.SecureRandom.GetRandomInt();

            // start the session by challenging the client for authentication
            using (ByteBuffer packet = new ByteBuffer())
            {
                packet.Append(Seed);
                packet.Append(Server.SecureRandom.GetRandomBytes(32));
                
                Send(ShardServerOpcode.AuthChallenge, packet);
            }
        }

        protected override void OnSessionClosed(CloseReason reason)
        {
            if(Status == SessionStatus.Ingame)
            {
                log.DebugFormat("session closing and status is {0}, so removing player {1} from shard", SessionStatus.Ingame, Player);

                // enqueue the player removal since it changes shard state. capture the player in the closure before nulling it
                Character playerToRemove = Player;
                Server.EnqueueCommand(() =>
                {
                    // remove, then save
                    Server.Shard.RemoveCharacter(playerToRemove);
                    new CharacterDao().UpdateCharacter(Server.ShardDB, playerToRemove);
                });

                // update Status immediately (it is thread-safe) (these other values aren't though... potential bug?)
                Status = SessionStatus.None;
                Awareness.Clear();
                Player = null;
            }

            base.OnSessionClosed(reason);
        }
    }
}
