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
using Whisper.Game.Characters;
using Whisper.Game.Units;
using Whisper.Shared.Math;
using Whisper.Shared.Utility;
using log4net;

namespace Whisper.Game.Objects
{
    public class GameObject
    {
        private readonly ILog log = LogManager.GetLogger(typeof(GameObject));

        private uint[] fields;

        public GameObject(ObjectID id, ObjectTypeID typeId) : this(id, typeId, (ushort)ObjectFields.END)
        {
        }

        protected GameObject(ObjectID id, ObjectTypeID typeId, ushort fieldsCount)
        {
            ID = id;
            TypeID = typeId;
            fields = new uint[fieldsCount];
        }

        public ObjectID ID
        {
            get;
            private set;
        }

        public ObjectTypeID TypeID
        {
            get;
            private set;
        }

        public int MapID
        {
            get;
            set;
        }

        public OrientedVector3 Position
        {
            get;
            set;
        }

        public int FieldCount
        {
            get
            {
                return fields.Length;
            }
        }

        protected uint GetFieldUnsigned(ushort index)
        {
            return fields[index];
        }

        protected unsafe int GetFieldSigned(ushort index)
        {
            uint field = fields[index];
            return *(int*)&field;
        }

        protected ulong GetFieldLong(ushort index)
        {
            return fields[index] | ((ulong)fields[index + 1] << 32);
        }

        protected unsafe float GetFieldFloat(ushort index)
        {
            uint field = fields[index];
            return *(float*)&field;
        }

        protected void SetField(ushort index, uint value)
        {
            fields[index] = value;
        }

        protected void SetField(ushort index, ulong value)
        {
            fields[index] = (uint)(value & 0xffffffff);
            fields[index + 1] = (uint)(value >> 32);
        }

        protected unsafe void SetField(ushort index, int value)
        {
            fields[index] = *(uint*)&value;
        }

        protected unsafe void SetField(ushort index, float value)
        {
            fields[index] = *(uint*)&value;
        }

        public uint GetFieldUnsigned(ObjectFields field)
        {
            return GetFieldUnsigned((ushort)field);
        }

        public int GetFieldSigned(ObjectFields field)
        {
            return GetFieldSigned((ushort)field);
        }

        public ulong GetFieldLong(ObjectFields field)
        {
            return GetFieldLong((ushort)field);
        }

        public float GetFieldFloat(ObjectFields field)
        {
            return GetFieldFloat((ushort)field);
        }

        public void SetField(ObjectFields field, uint value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(ObjectFields field, int value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(ObjectFields field, ulong value)
        {
            SetField((ushort)field, value);
        }

        public void SetField(ObjectFields field, float value)
        {
            SetField((ushort)field, value);
        }

        public virtual void BuildTargetedCreationUpdate(UpdateData data, Character character)
        {
            ObjectUpdateType updateType = ObjectUpdateType.CreateObject2;
            ObjectUpdateFlags updateFlags = ObjectUpdateFlags.HasPosition | ObjectUpdateFlags.All;

            if (character == this)
                updateFlags |= ObjectUpdateFlags.Self;

            if (this is Unit)
                updateFlags |= ObjectUpdateFlags.Living;

            using (ByteBuffer buffer = new ByteBuffer())
            {
                // basic update
                buffer.Append((byte)updateType);
                buffer.Append(ID.GetPacked());
                buffer.Append((byte)TypeID);

                // movement update
                buffer.Append((byte)updateFlags);

                if((updateFlags & ObjectUpdateFlags.Living) != 0)
                {
                    Unit me = (Unit)this;

                    buffer.Append((int)me.MovementFlags);
                    buffer.Append(0); // time
                    buffer.Append(Position);
                    buffer.Append(0); // fallingTime

                    buffer.Append(me.MovementSpeed.Walking);
                    buffer.Append(me.MovementSpeed.Running);
                    buffer.Append(me.MovementSpeed.RunningBack);
                    buffer.Append(me.MovementSpeed.Swimming);
                    buffer.Append(me.MovementSpeed.SwimmingBack);
                    buffer.Append(me.MovementSpeed.Turning);
                }
                else if((updateFlags & ObjectUpdateFlags.HasPosition) != 0)
                    buffer.Append(Position);

                if ((updateFlags & ObjectUpdateFlags.MaskID) != 0)
                    buffer.Append(ID.MaskID);

                if ((updateFlags & ObjectUpdateFlags.All) != 0)
                    buffer.Append(1); // ?

                // values update
                //buffer.Append((byte)ObjectUpdateType.Values); // not sure where i found these but they don't seem to be necessary
                //buffer.Append(ID.GetPacked());


                SetField(ObjectFields.ID, ID.LongForm);
                SetField(ObjectFields.Scale, 1.0f);
                SetField(ObjectFields.Type, 0x19); //(int)ObjectTypeID.Player);
                SetField((ushort)UnitFields.Health, 51);
                SetField((ushort)UnitFields.Power1, 165);
                SetField((ushort)UnitFields.MaxHealth, 51);
                SetField((ushort)UnitFields.MaxPower1, 165);
                SetField((ushort)UnitFields.Power4, 100);
                SetField((ushort)UnitFields.MaxPower2, 1000);
                SetField((ushort)UnitFields.MaxPower4, 100);
                SetField((ushort)UnitFields.Flags, 0x8);
                SetField((ushort)UnitFields.Level, 1);
                SetField((ushort)UnitFields.Stat0, 20);
                SetField((ushort)UnitFields.Stat1, 20);
                SetField((ushort)UnitFields.Stat2, 20);
                SetField((ushort)UnitFields.Stat3, 23);
                SetField((ushort)UnitFields.Stat4, 23);
                SetField((ushort)UnitFields.DisplayID, 50);
                SetField((ushort)UnitFields.NativeDisplayID, 50);
                SetField((ushort)UnitFields.FactionTemplate, 1);
                SetField((ushort)UnitFields.AttackTimeBase, 2900);
                SetField((ushort)UnitFields.AttackTimeOffhand, 2000);
                SetField((ushort)UnitFields.AttackTimeRanged, 2000);
                SetField((ushort)UnitFields.BoundingRadius, 0.208f);
                SetField((ushort)UnitFields.CombatReach, 1.5f);
                SetField((ushort)UnitFields.DamageMin, 5.0f);
                SetField((ushort)UnitFields.DamageMax, 7.0f);
                SetField((ushort)UnitFields.ModCastSpeed, 1.0f);
                SetField((ushort)UnitFields.Resistances, 45);
                SetField((ushort)UnitFields.BaseMana, 100);
                SetField((ushort)UnitFields.BaseHealth, 31);
                SetField((ushort)UnitFields.AttackPowerRanged, 10);
                SetField((ushort)UnitFields.AttackPower, 10);
                SetField((ushort)UnitFields.DamageRangedMax, 3.4f);
                SetField((ushort)UnitFields.DamageRangedMin, 2.4f);
                SetField((ushort)CharacterFields.XPNextLevel, 400);
                SetField((ushort)CharacterFields.XPRestState, 7);
                SetField((ushort)CharacterFields.CharacterPoints2, 2);
                SetField((ushort)CharacterFields.DodgePercent, 0.84f);
                SetField((ushort)CharacterFields.CritPercent, 0.87f);
                SetField((ushort)CharacterFields.CritPercentRanged, 0.83f);

                SetField((ushort)0x045a, 0x20000000);

                SetField((ushort)CharacterFields.DamageDoneModPercent, 1.0f);
                SetField((ushort)CharacterFields.DamageDoneModPercent + 1, 1.0f);
                SetField((ushort)CharacterFields.DamageDoneModPercent + 2, 1.0f);
                SetField((ushort)CharacterFields.DamageDoneModPercent + 3, 1.0f);
                SetField((ushort)CharacterFields.DamageDoneModPercent + 4, 1.0f);
                SetField((ushort)CharacterFields.DamageDoneModPercent + 5, 1.0f);
                SetField((ushort)CharacterFields.DamageDoneModPercent + 6, 1.0f);
                SetField((ushort)CharacterFields.WatchedFactionIndex, -1);

                SetField((ushort)0x0128, 0x000017d0);
                SetField((ushort)0x0134, 0x00000038);
                SetField((ushort)0x014c, 0x00000573);
                SetField((ushort)0x0158, 0x00000037);
                SetField((ushort)0x01b8, 0x00000023);
                SetField((ushort)0x01ec, 0x00000016);
                SetField((ushort)0x01ed, 0x40000000);
                SetField((ushort)0x01ee, 0x0000000e);
                SetField((ushort)0x01ef, 0x40000000);
                SetField((ushort)0x01f2, 0x00000010);
                SetField((ushort)0x01f3, 0x40000000);
                SetField((ushort)0x01f4, 0x00000012);
                SetField((ushort)0x01f5, 0x40000000);
                SetField((ushort)0x0204, 0x00000014);
                SetField((ushort)0x0205, 0x40000000);

                SetField((ushort)CharacterFields.PackSlotFirst, 0x00000018);
                SetField((ushort)0x0215, 0x40000000);
                SetField((ushort)0x0216, 0x0000001a);
                SetField((ushort)0x0217, 0x40000000);
                SetField((ushort)0x0218, 0x0000001c);
                SetField((ushort)0x0219, 0x40000000);

                SetField((ushort)CharacterFields.SkillInfoFirst1, 0x00000006);
                SetField((ushort)0x02cf, 0x00010001);
                SetField((ushort)0x02d1, 0x00000008);
                SetField((ushort)0x02d2, 0x00010001);
                SetField((ushort)0x02d4, 0x0000005f);
                SetField((ushort)0x02d5, 0x00050001);
                SetField((ushort)0x02d7, 0x00000062);
                SetField((ushort)0x02d8, 0x012c012c);
                SetField((ushort)0x02da, 0x00000088);
                SetField((ushort)0x02db, 0x00050001);
                SetField((ushort)0x02dd, 0x000000a2);
                SetField((ushort)0x02de, 0x00050001);
                SetField((ushort)0x02e0, 0x000000e4);
                SetField((ushort)0x02e1, 0x00050001);
                SetField((ushort)0x02e3, 0x0000019f);
                SetField((ushort)0x02e4, 0x00010001);

                SetField((ushort)UnitFields.Bytes0, 0x00010801);
                SetField((ushort)UnitFields.Bytes1, 0x0000ee00);
                SetField((ushort)UnitFields.Bytes2, 0x00002800);
                SetField((ushort)CharacterFields.Bytes1, 0x05050501);
                SetField((ushort)CharacterFields.Bytes2, 0x02000000);
                SetField((ushort)CharacterFields.Bytes3, 0x00000001);

                UpdateMask updateMask = new UpdateMask(FieldCount/*, MagicUpdateMask*/);
                for (ushort i = 0; i < FieldCount; ++i)
                {
                    if (GetFieldUnsigned(i) != 0)
                        updateMask.SetBit(i);
                }

                log.DebugFormat("create updatemask block count is 0x{0:x2}", updateMask.BlockCount);
                buffer.Append(updateMask.BlockCount);
                buffer.Append(updateMask.Data);

                /*int active = 0;
                byte[] magicFieldList = Strings.HexDumpToBytes(MagicFieldList);
                for (ushort i = 0; i < FieldCount; ++i)
                {
                    if (updateMask.GetBit(i))
                        SetField(i, BitConverter.ToUInt32(magicFieldList, active++ * 4));
                }
                
                for (ushort i = 0; i < FieldCount; ++i)
                {
                    if (updateMask.GetBit(i))
                    {
                        ObjectFields of;
                        UnitFields uf;
                        CharacterFields cf;
                        ushort checker;

                        if (Enum.TryParse(i.ToString(), out of) && !ushort.TryParse(of.ToString(), out checker))
                            log.DebugFormat("Magic update mask includes ObjectFields.{0}: 0x{1:x8} [{2}] [{3}]", of, GetFieldUnsigned(i), GetFieldSigned(i), GetFieldFloat(i));
                        else if (Enum.TryParse(i.ToString(), out uf) && !ushort.TryParse(uf.ToString(), out checker))
                            log.DebugFormat("Magic update mask includes UnitFields.{0}: 0x{1:x8} [{2}] [{3}]", uf, GetFieldUnsigned(i), GetFieldSigned(i), GetFieldFloat(i));
                        else if (Enum.TryParse(i.ToString(), out cf) && !ushort.TryParse(cf.ToString(), out checker))
                            log.DebugFormat("Magic update mask includes CharacterFields.{0}: 0x{1:x8} [{2}] [{3}]", cf, GetFieldUnsigned(i), GetFieldSigned(i), GetFieldFloat(i));
                        else
                            log.DebugFormat("Magic update mask includes other thing 0x{0:x4}: 0x{1:x8} [{2}] [{3}]", i, GetFieldUnsigned(i), GetFieldSigned(i), GetFieldFloat(i));
                    }
                }*/

                // determine which fields are not actually required
                //updateMask.UnsetBit((ushort)UnitFields.Bytes0);
                //updateMask.UnsetBit((ushort)UnitFields.Bytes1);
                //updateMask.UnsetBit((ushort)UnitFields.Bytes2);
                //updateMask.UnsetBit((ushort)CharacterFields.Bytes1);
                //updateMask.UnsetBit((ushort)CharacterFields.Bytes2);
                //updateMask.UnsetBit((ushort)CharacterFields.Bytes3);

                //updateMask.UnsetBit((ushort)UnitFields.Power4); // necessary
                //updateMask.UnsetBit((ushort)UnitFields.MaxPower4); // necessary
                //updateMask.UnsetBit((ushort)UnitFields.Flags); // necessary
                //updateMask.UnsetBit((ushort)UnitFields.DisplayID);
                //updateMask.UnsetBit((ushort)UnitFields.NativeDisplayID);

                // write the value set
                for (ushort i = 0; i < FieldCount; ++i)
                {
                    if (updateMask.GetBit(i))
                        buffer.Append(GetFieldUnsigned(i));
                }

                // append the update
                data.AddUpdateBlock(buffer);
            }
        }

        private const string MagicUpdateMask =
@"15
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
00 00 00";

        private const string MagicFieldList =
       @"02 00 00 00 19 00 00 00 00 00 80 3F 33
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
