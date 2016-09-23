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
using Whisper.Daemon.Shard.Lookup;
using Whisper.Daemon.Shard.Net;
using Whisper.Game.Objects;
using Whisper.Game.Characters;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Shard.Commands
{
    public sealed class CharacterListCommand : ShardCommandBase<ClientPacketHeader>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CharacterListCommand));

        public override string Name
        {
            get
            {
                return ShardClientOpcode.CharacterList.ToString();
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
                return CommandThreadSafety.Immediate;
            }
        }

        public override void ExecuteCommand(ShardSession session, ShardRequest request, ClientPacketHeader header)
        {
            int count = session.Server.ShardDB.ExecuteQuery("select count(0) from `character` where account_id = ?", session.AccountID, result =>
            {
                if (result.Read())
                    return result.GetInt32(0);
                else
                {
                    log.Error("selecting count on character table returned no result row");
                    return 0;
                }
            });

            if(count > byte.MaxValue)
            {
                log.WarnFormat("character count [{0}] for account id [{1}] is larger than maximum [{2}] and is being limited", count, session.AccountID, byte.MaxValue);
                count = byte.MaxValue;
            }

            using (ByteBuffer response = new ByteBuffer())
            {
                response.Append((byte)count);

                //                                          0   1     2     3      4    5     6     7           8           9            10     11      12          13          14          15            16
                session.Server.ShardDB.ExecuteQuery("select id, name, race, class, sex, skin, face, hair_style, hair_color, facial_hair, level, map_id, position_x, position_y, position_z, player_flags, zone_id from `character` where account_id = ? limit ?", new object[] { session.AccountID, byte.MaxValue }, result =>
                {
                    while(result.Read())
                    {
                        response.Append(new ObjectID(result.GetInt32(0), ObjectID.Type.Player)); // id
                        response.Append(result.GetString(1)); // name
                        response.Append(result.GetByte(2)); // race
                        response.Append(result.GetByte(3)); // class
                        response.Append(result.GetByte(4)); // sex
                        response.Append(result.GetByte(5)); // skin
                        response.Append(result.GetByte(6)); // face
                        response.Append(result.GetByte(7)); // hair style
                        response.Append(result.GetByte(8)); // hair color
                        response.Append(result.GetByte(9)); // facial hair
                        response.Append(result.GetByte(10)); // level
                        response.Append(result.GetInt32(16)); // zone
                        response.Append(result.GetInt32(11)); // map id
                        response.Append(result.GetFloat(12)); // x
                        response.Append(result.GetFloat(13)); // y
                        response.Append(result.GetFloat(14)); // z
                        response.Append(0); // guild id
                        response.Append(result.GetInt32(15)); // player flags
                        response.Append((byte)1); // first login
                        response.Append(0); // pet display id
                        response.Append(0); // pet level
                        response.Append(0); // pet family

                        for(int i = 0; i < Enum.GetValues(typeof(EquipmentSlot)).Length; ++i)
                        {
                            response.Append(0); // display info id
                            response.Append((byte)0); // inventory type
                        }

                        response.Append(0); // first bag display info id
                        response.Append((byte)0); // first bag inventory type
                    }

                    return true;
                });

                session.Send(ShardServerOpcode.CharacterList, response);
            }
        }
    }
}
