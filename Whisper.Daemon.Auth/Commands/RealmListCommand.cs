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
using System.Runtime.InteropServices;
using Whisper.Daemon.Auth.Lookup;
using Whisper.Daemon.Auth.Net;
using Whisper.Shared.Net;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Auth.Commands
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RealmList
    {
        public AuthRequestOpcode Opcode;
        private int unk0;
    }

    public sealed class RealmListCommand : CommandBase<RealmList, AuthSession, AuthRequest, AuthStatus>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(RealmListCommand));

        public override string Name
        {
            get
            {
                return AuthRequestOpcode.RealmList.ToString();
            }
        }

        public override AuthStatus PermissibleStatus
        {
            get
            {
                return AuthStatus.Authenticated;
            }
        }

        public override void ExecuteCommand(AuthSession session, AuthRequest request, RealmList header)
        {
            using (ByteBuffer packet = new ByteBuffer())
            {
                BuildRealmListPacket(session, packet);

                using (ByteBuffer response = new ByteBuffer())
                {
                    response.Append((byte)AuthRequestOpcode.RealmList);
                    response.Append((ushort)packet.Size);
                    response.Append(packet.GetBytes());

                    session.Send(response.GetArraySegment());
                }
            }
        }

        private void BuildRealmListPacket(AuthSession session, ByteBuffer packet)
        {
            int shardCount = session.Server.AuthDB.ExecuteQuery("select count(0) from shard where enabled = true", result =>
            {
                if (result.Read())
                    return result.GetInt32(0);
                else
                {
                    log.Error("got no results when querying for count on shard table");
                    return 0;
                }
            });

            if(shardCount > byte.MaxValue)
            {
                log.WarnFormat("shard count {0} is being truncated to maximum client allowed value of {1}", shardCount, byte.MaxValue);
                shardCount = byte.MaxValue;
            }

            packet.Append(0);
            packet.Append((byte)shardCount);
            
            session.Server.AuthDB.ExecuteQuery("select name, address, port, recommended, type, last_ping from shard where enabled = true limit ?", byte.MaxValue, result =>
            {
                while (result.Read())
                {
                    string name = result.GetString(0);
                    string address = result.GetString(1) + ':' + result.GetInt32(2);
                    
                    ShardType type = (ShardType)result.GetByte(4);
                    ShardFlags flags = ShardFlags.None;
                    byte characterCount = 0;
                    float population = 0.0f;
                    byte category = 0;

                    if (DateTime.Now - result.GetDateTime(5) > new TimeSpan(0, 0, 0, 0, session.Server.AppConfig.ShardOfflineTimeoutMilliseconds))
                        flags |= ShardFlags.Offline;

                    if (result.GetBoolean(3))
                        flags |= ShardFlags.Recommended;

                    packet.Append((int)type);
                    packet.Append((byte)flags);
                    packet.Append(name);
                    packet.Append(address);
                    packet.Append(population);
                    packet.Append(characterCount);
                    packet.Append(category);
                    packet.Append((byte)0);
                }
                return true;
            });

            packet.Append((short)2);
        }
    }
}
