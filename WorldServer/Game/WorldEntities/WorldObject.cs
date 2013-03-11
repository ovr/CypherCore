/*
 * Copyright (C) 2012-2013 CypherCore <http://github.com/organizations/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */﻿

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Constants;
using Framework.Network;
using Framework.Utility;
using WorldServer.Game.Managers;
using WorldServer.Game.Maps;
using WorldServer.Network;
using Framework.ObjectDefines;
using WorldServer.Game.Movement;
using WorldServer.Game;

namespace WorldServer.Game.WorldEntities
{
    public class WorldObject
    {
        public WorldObject(bool isWorldObject)
        {
            Initialize();

            objectTypeMask |= HighGuidMask.Object;
            objectTypeId = ObjectType.Object;
            _fieldNotifyFlags = UpdateFieldFlags.ViewerDependent;
            IsInWorld = false;
            m_isWorldObject = isWorldObject;
        }

        void Initialize()
        {
            movementInfo = new MovementInfo();
            updateFlags = UpdateFlag.None;
            UpdateData_mirror = new Hashtable();
            UpdateData = new Hashtable();
        }

        public void CreateGuid(ulong guidlow, uint entry, HighGuidType guidhigh)
        {
            var guid = MakeNewGuid((uint)guidlow, entry, guidhigh);
            SetValue<UInt64>(ObjectFields.Guid, guid);
            SetValue<int>(ObjectFields.Type, (int)objectTypeMask);
            PackedGuid = guid;
        }

        public virtual void AddToWorld()
        {
            if (IsInWorld)
                return;

            IsInWorld = true;
            ClearUpdateMask(false);
        }
        public virtual void RemoveFromWorld()
        {
            if (!IsInWorld)
                return;
            //DestroyForNearbyPlayers();

            IsInWorld = false;
            ClearUpdateMask(false);
        }

        public bool IsWorldObject()
        {
            if (m_isWorldObject)
                return true;

            //if (ToCreature() && ToCreature().m_isTempWorldObject)
                //return true;

            return false;
        }

        public static ulong MakePair64(uint l, uint h)
        {
            return (ulong)(l | (ulong)h << 32);
        }
        public static uint Pair64_HiPart(int x)
        {
            return (uint)(((ulong)x >> 32) & 0x00000000FFFFFFFF);
        }
        public static uint Pair64_LoPart(int x)
        {
            return (uint)((ulong)x & 0x00000000FFFFFFFF);
        }

        public static uint Pair32_HiPart(int x)
        {
            return (ushort)(((uint)x >> 16) & 0x0000FFFF);
        }
        public static ushort Pair32_LoPart(int x)
        {
            return (ushort)((uint)x & 0x0000FFFF);
        }


        public static uint MakePair32(uint l, uint h)
        {
            return (uint)((ushort)l | h << 16);
        }
        public static uint MakePair16(uint l, uint h)
        {
            return (ushort)((byte)l | (ushort)h << 8);
        }

        public virtual bool LoadFromDB(uint guid, Map map) { return false; }

        //Gets
        public ObjectType GetTypeId() { return objectTypeId; }
        public ulong GetGUID() { return GetValue<ulong>(0); }
        public uint GetGUIDLow() { return (uint)(GetValue<ulong>(0) & 0xFFFFFFFF); }
        public uint GetGUIDHigh() { return ObjectGuid.GuidHiPart(GetValue<ulong>(0)); }
        public ulong GetPackGUID() { return PackedGuid; }
        public uint GetEntry() { return GetValue<uint>(ObjectFields.Entry); }
        public WorldSession GetSession() { return session; }
        public long GetValue(int index)
        {
            if (UpdateData[index] != null)
                return (long)Convert.ChangeType(UpdateData[index], typeof(long));
            return 0;
        }
        public bool isActiveObject() { return m_isActive; }
        public float GetDistanceZ(WorldObject obj)
        {
            float dz = Math.Abs(GetPositionZ() - obj.GetPositionZ());
            float sizefactor = GetObjectSize() + obj.GetObjectSize();
            float dist = dz - sizefactor;
            return (dist > 0 ? dist : 0);
        }
        public float GetObjectSize()
        {
            return (ValuesCount > (uint)UnitFields.CombatReach) ? GetValue<float>(UnitFields.CombatReach) : ObjectConst.DefaultWorldObjectSize;
        }

        public T GetValue<T>(int index, byte offset = 0)
        {
            if (UpdateData[index] == null)
                return default(T);
            switch (typeof(T).Name)
            {
                case "UInt64":
                case "Int64":
                    return (T)Convert.ChangeType((Convert.ToUInt64(UpdateData[index + 1]) << 32 | (uint)UpdateData[index]), typeof(T));
                case "Byte":
                case "UInt16":
                    byte[] temp = BitConverter.GetBytes((uint)UpdateData[index]);
                    return (T)Convert.ChangeType(temp[offset], typeof(T));
                case "Int32":
                case "UInt32":
                case "Single":
                default:
                    return (T)Convert.ChangeType(UpdateData[index], typeof(T));
            }
        }
        public T GetValue<T>(Enum enumindex, byte offset = 0)
        {
            int index = (int)(object)enumindex;
            if (UpdateData[index] == null)
                return default(T);

            switch (typeof(T).Name)
            {
                case "UInt64":
                case "Int64":
                    return (T)Convert.ChangeType(((Convert.ToUInt64(UpdateData[index + 1]) << 32) | (uint)UpdateData[index]), typeof(T));
                case "Byte":
                case "UInt16":
                    byte[] temp = BitConverter.GetBytes((uint)UpdateData[index]);
                    return (T)Convert.ChangeType(temp[offset], typeof(T));
                case "Int32":
                case "UInt32":
                case "Single":
                default:
                    return (T)Convert.ChangeType(UpdateData[index], typeof(T));
            }

        }
        public bool GetBit(int index) { return (_UpdateMask[index >> 3] & 1 << (index & 0x7)) != 0; }
        public Map GetMap() { return CurrMap; }
        public uint GetMapId() { return CurrMap.GetId(); }
        public uint GetZoneId()
        {
            return GetMap().GetZoneId(GetPositionX(), GetPositionY(), GetPositionZ());
        }

        public List<Creature> GetCreaturesInRange() { return GetObjectsInRange<Creature>(); }
        public List<Player> GetPlayersInRange() { return GetObjectsInRange<Player>(); }
        public List<GameObject> GetGameObjectsInRange() { return GetObjectsInRange<GameObject>(); }
        public List<T> GetObjectsInRange<T>()
        {
            /*
            uint x = CellHandler.GetPosX(this.Position.X);
            uint y = CellHandler.GetPosY(this.Position.Y);
            uint endX = (x <= CellHandler.TotalCellsPerMap) ? x + 1 : (CellHandler.TotalCellsPerMap - 1);
            uint endY = (y <= CellHandler.TotalCellsPerMap) ? y + 1 : (CellHandler.TotalCellsPerMap - 1);
            uint startX = x > 0 ? x - 1 : 0;
            uint startY = y > 0 ? y - 1 : 0;
            uint posX, posY;
            Grid cell;

            List<T> returnlist = new List<T>();

            for (posX = startX; posX <= endX; posX++)
            {
                for (posY = startY; posY <= endY; posY++)
                {
                    cell = CellHandler.GetCell(posX, posY);
                    if (cell != null)
                        returnlist.AddRange(cell.GetObjects().Where(p => p is T).Cast<T>());
                }
            }
            return returnlist;
            */
            return null;
        }
        public float GetPositionX() { return Position.X; }
        public float GetPositionY() { return Position.Y; }
        public float GetPositionZ() { return Position.Z; }
        public float GetOrientation() { return Position.Orientation; }
        public string GetName() { return Name; }

        public static bool IS_PLAYER_GUID(ulong guid) { return ObjectGuid.GuidHiPart(guid) == (uint)HighGuidMask.Player && guid != 0; }

        public Creature ToCreature() { return (this is Unit) ? (this as Creature) : null; }
        public Player ToPlayer() { return (this is Player) ? (this as Player) : null; }
        public GameObject ToGameObject() { return (this is GameObject) ? (this as GameObject) : null; }
        public Unit ToUnit() { return (this is Unit) ? (this as Unit) : null; }
        public Corpse ToCorpse() { return (this is Corpse) ? (this as Corpse) : null; }
        //public DynamicObject ToDynamicObject() { return (this is Unit) ? (this as Creature) : null; }

        public bool HasFlag<T>(int index, T flag) { return (GetValue<uint>(index) & Convert.ToUInt32(flag)) != 0; }
        public bool HasFlag<T>(Enum enumindex, T flag) { return (GetValue<uint>(enumindex) & Convert.ToUInt32(flag)) != 0; }

        public void ApplyModFlag<T>(Enum index, T flag, bool apply)
        {
            if (apply)
                SetFlag(index, flag);
            else
                RemoveFlag(index, flag);
        }
        public void RemoveFlag<T>(Enum enumindex, T oldFlag, byte offset = 0)
        {
            int index = (int)(object)enumindex;
            if (offset > 0)
            {
                if (Convert.ToBoolean((uint)UpdateData[index] >> (offset * 8) & Convert.ToUInt32(oldFlag)))
                    UpdateData[index] = (uint)UpdateData[index] & ~(uint)(Convert.ToUInt32(oldFlag) << (offset * 8));
            }
            else
            {
                uint oldval = (uint)UpdateData[index];
                uint newval = oldval & ~Convert.ToUInt32(oldFlag);

                if (oldval != newval)
                {
                    UpdateData[index] = newval;
                    SetUpdateNeeded();
                }
            }
            SetUpdateNeeded();
        }

        //Sets
        public void SetName(string newname) { Name = newname; }
        public void SetValuesCount(uint Count)
        {
            ValuesCount = Count;
            BlockCount = (uint)(Count + 31) / 32;
            _UpdateMask = new byte[(BlockCount * 4) << 2];
        }
        public void SetFlag<T>(Enum enumindex, T newflag, byte offset = 0)
        {
            int index = (int)(object)enumindex;
            SetFlag<T>(index, newflag, offset);
        }
        public void SetFlag<T>(int index, T newflag, byte offset = 0)
        {
            if (offset > 0)
            {
                if (!Convert.ToBoolean((uint)UpdateData[index] >> (offset * 8) & Convert.ToUInt32(newflag)))
                    UpdateData[index] = (uint)UpdateData[index] | (Convert.ToUInt32(newflag) << (offset * 8));
            }
            else
            {
                uint oldval = Convert.ToUInt32(UpdateData[index]);
                uint newval = oldval | Convert.ToUInt32(newflag);

                if (oldval != newval)
                {
                    UpdateData[index] = newval;

                }
            }
            SetUpdateNeeded();
        }

        public void SetValue<T>(int index, T value, byte offset = 0)
        {
            switch (value.GetType().Name)
            {
                case "Byte":
                case "UInt16":
                    if (!UpdateData.ContainsKey(index))
                        UpdateData[index] = (uint)((uint)Convert.ChangeType(value, typeof(uint)) << (offset * (value.GetType().Name == "Byte" ? 8 : 16)));
                    else
                        UpdateData[index] = (uint)((uint)UpdateData[index] | (uint)((uint)Convert.ChangeType(value, typeof(uint)) << (offset * (value.GetType().Name == "Byte" ? 8 : 16))));
                    break;
                case "UInt64":
                case "Int64":
                    ulong tmpValue = (ulong)Convert.ChangeType(value, typeof(ulong));
                    UpdateData[index] = (uint)(tmpValue & UInt64.MaxValue);
                    UpdateData[index + 1] = (uint)((tmpValue >> 32) & UInt64.MaxValue);
                    break;
                case "Int32":
                case "UInt32":
                case "Single":
                default:
                    UpdateData[index] = value;
                    break;
            }
            SetUpdateNeeded();
        }
        public void SetValue<T>(Enum enumindex, T value, byte offset = 0)
        {
            int index = (int)(object)enumindex;
            SetValue<T>(index, value, offset);
        }
        public void SetBit(int index) { _UpdateMask[index >> 3] |= (byte)(1 << (index & 0x7)); }
        public void UnsetBit(int index) { _UpdateMask[index >> 3] &= (byte)(0xff ^ (1 << (index & 0x7))); }
        public void SetMap(Map map) { CurrMap = map; }
        public void SetUpdateNeeded()
        {
            if (IsInWorld && !RequiresUpdate)
                RequiresUpdate = true;
        }
        public void SetEntry(uint entry) { SetValue<uint>(ObjectFields.Entry, entry); }
        public void SetObjectScale(float scale) { SetValue<float>(ObjectFields.Scale, scale); }
        public void SetFieldNotifyFlag(uint flag) { _fieldNotifyFlags |= flag; }
        public void RemoveFieldNotifyFlag(uint flag) { _fieldNotifyFlags &= ~flag; }

        #region UpdateBlocks
        public void WriteUpdateObjectMovement(ref PacketWriter packet, UpdateFlag updateFlags)
        {
            ObjectGuid Guid = new ObjectGuid(GetPackGUID());
            MovementFlag movementFlags = movementInfo.GetMovementFlags();
            MovementFlag2 movementFlags2 = movementInfo.GetMovementFlags2();

            bool hasTransport = (movementInfo.TransGuid != 0);
            bool isSplineEnabled = false; //((Unit)this).IsSplineEnabled();
            bool hasPitch = false;// Convert.ToBoolean(movementFlags & (MovementFlag.Swim | MovementFlag.Fly)) || Convert.ToBoolean(movementFlags2 & (MovementFlag2.AlwaysAllowPitching));
            bool hasFallData = false;// diabled for now Convert.ToBoolean(movementFlags & (MovementFlag.Falling | MovementFlag.FallingFar | MovementFlag.FallingSlow));
            bool hasFallDirection = false;// Convert.ToBoolean(movementFlags & (MovementFlag.Falling));
            bool hasElevation = false;// Convert.ToBoolean(movementFlags & (MovementFlag.SplineElevation));
            bool hasOrientation = (GetTypeId() != ObjectType.Item);

            uint unkloopcounter1 = 0;
            uint unkloopcounter2 = 0;

            packet.WriteBit(false);
            packet.WriteBit(false);
            packet.WriteBit((uint)(updateFlags & UpdateFlag.Rotation));
            packet.WriteBit((uint)(updateFlags & UpdateFlag.HasTarget));
            packet.WriteBit(false);
            packet.WriteBit((uint)(updateFlags & UpdateFlag.Unknown3));
            packet.WriteBits(unkloopcounter1, 24);
            packet.WriteBit((uint)(updateFlags & UpdateFlag.Transport));//transport
            packet.WriteBit((uint)(updateFlags & UpdateFlag.GoTransportPosition));
            packet.WriteBit((uint)(updateFlags & UpdateFlag.Unknown2));
            packet.WriteBit(false);//bit 784
            packet.WriteBit((uint)(updateFlags & UpdateFlag.Self));
            packet.WriteBit(false);
            packet.WriteBit((uint)(updateFlags & UpdateFlag.Living));
            packet.WriteBit(false);
            packet.WriteBit((uint)(updateFlags & UpdateFlag.Unknown4));
            packet.WriteBit((uint)(updateFlags & UpdateFlag.StationaryPosition));
            packet.WriteBit((uint)(updateFlags & UpdateFlag.Vehicle));
            packet.WriteBits(unkloopcounter2, 21);
            packet.WriteBit((uint)(updateFlags & UpdateFlag.AnimKits));

            if (Convert.ToBoolean(updateFlags & UpdateFlag.Living))
            {
                packet.WriteBit(Guid[3]);
                packet.WriteBit(hasFallData);
                packet.WriteBit(true); //lackstimestamp
                packet.WriteBit(false);
                packet.WriteBit(Guid[2]);
                packet.WriteBit(false);
                packet.WriteBit(!hasPitch);
                packet.WriteBit(!Convert.ToBoolean(movementFlags2));
                packet.WriteBit(Guid[4]);
                packet.WriteBit(Guid[5]);
                packet.WriteBits(0, 24);
                packet.WriteBit(!hasElevation);
                packet.WriteBit(!Convert.ToBoolean(updateFlags & UpdateFlag.Living));//wrong
                packet.WriteBit(false);
                packet.WriteBit(Guid[0]);
                packet.WriteBit(Guid[6]);
                packet.WriteBit(Guid[7]);
                packet.WriteBit(hasTransport);
                packet.WriteBit(!hasOrientation);

                if (hasTransport)
                {
                    var tguid = movementInfo.TransGuid;

                    packet.WriteBit(tguid[3]);
                    packet.WriteBit(tguid[0]);
                    packet.WriteBit(tguid[4]);
                    packet.WriteBit(tguid[5]);
                    packet.WriteBit(tguid[2]);
                    packet.WriteBit(tguid[7]);
                    packet.WriteBit(tguid[1]);
                    packet.WriteBit(false); //time2
                    packet.WriteBit(tguid[6]);
                    packet.WriteBit(false); //time3
                }

                if (Convert.ToBoolean(movementFlags2))
                    packet.WriteBits((uint)movementFlags2, 13);

                packet.WriteBit(!Convert.ToBoolean(movementFlags));
                packet.WriteBit(Guid[1]);

                if (hasFallData)
                    packet.WriteBit(hasFallDirection);
                
                packet.WriteBit(isSplineEnabled);

                if (Convert.ToBoolean(movementFlags))
                    packet.WriteBits((uint)movementFlags, 30);

                if (isSplineEnabled)
                {
                    packet.WriteBit(true); //full spline
                    MovementPacketBuilder.WriteCreateBits(((Unit)this).movespline, ref packet);
                }
            }

            if (Convert.ToBoolean(updateFlags & UpdateFlag.GoTransportPosition))
            {
                ObjectGuid tguid = new ObjectGuid(movementInfo.TransGuid);
                packet.WriteBit(false);
                packet.WriteBit(tguid[3]);
                packet.WriteBit(tguid[1]);
                packet.WriteBit(tguid[4]);
                packet.WriteBit(tguid[7]);
                packet.WriteBit(tguid[2]);
                packet.WriteBit(tguid[5]);
                packet.WriteBit(tguid[0]);
                packet.WriteBit(tguid[6]);
                packet.WriteBit(false);
            }

            //bit 654

            //unk3
            
            if (Convert.ToBoolean(updateFlags & UpdateFlag.HasTarget))
            {
                ObjectGuid guid = new ObjectGuid();//victim
                packet.WriteBit(guid[2]);
                packet.WriteBit(guid[6]);
                packet.WriteBit(guid[7]);
                packet.WriteBit(guid[1]);
                packet.WriteBit(guid[0]);
                packet.WriteBit(guid[3]);
                packet.WriteBit(guid[4]);
                packet.WriteBit(guid[5]);
            }

            //bit 784

            if (Convert.ToBoolean(updateFlags & UpdateFlag.AnimKits))
            {
                packet.WriteBit(true);
                packet.WriteBit(true);
                packet.WriteBit(true);
            }
            packet.BitFlush();

            //bit 360

            if (Convert.ToBoolean(updateFlags & UpdateFlag.Living))
            {
                packet.WriteFloat(baseMoveSpeed[(int)UnitMoveType.FlightBack]);

                if (isSplineEnabled)
                MovementPacketBuilder.WriteCreateData(((Unit)this).movespline, ref packet);

                packet.WriteFloat(baseMoveSpeed[(int)UnitMoveType.Swim]);

                if (hasFallData)
                {
                    if (hasFallDirection)
                    {
                        packet.WriteFloat(movementInfo.Jump.cosAngle);
                        packet.WriteFloat(movementInfo.Jump.xyspeed);
                        packet.WriteFloat(movementInfo.Jump.sinAngle);
                    }
                    packet.WriteFloat(movementInfo.Jump.velocity);
                    packet.WriteUInt32(movementInfo.FallTime);
                }

                if (hasTransport)
                {
                    var tguid = movementInfo.TransGuid;

                    packet.WriteFloat(movementInfo.Pos.Z);
                    packet.WriteByteSeq(tguid[4]);
                    packet.WriteFloat(movementInfo.Pos.X);
                    //time3
                    packet.WriteByteSeq(tguid[6]);
                    packet.WriteByteSeq(tguid[5]);
                    packet.WriteByteSeq(tguid[1]);
                    packet.WriteFloat(movementInfo.Pos.Orientation);
                    packet.WriteFloat(movementInfo.Pos.Y);
                    packet.WriteInt8(movementInfo.TransSeat);
                    packet.WriteByteSeq(tguid[7]);
                    //time2
                    packet.WriteUInt32(movementInfo.TransTime);
                    packet.WriteByteSeq(tguid[0]);
                    packet.WriteByteSeq(tguid[2]);
                    packet.WriteByteSeq(tguid[3]);
                }

                packet.WriteByteSeq(Guid[1]);
                packet.WriteFloat(baseMoveSpeed[(int)UnitMoveType.TurnRate]);
                packet.WriteFloat(Position.Y);
                packet.WriteByteSeq(Guid[3]);
                packet.WriteFloat(Position.Z);
                if (hasOrientation)
                    packet.WriteFloat(Position.Orientation);

                packet.WriteFloat(baseMoveSpeed[(int)UnitMoveType.RunBack]);

                if (hasElevation)
                    packet.WriteFloat(movementInfo.SplineElevation);

                packet.WriteByteSeq(Guid[0]);
                packet.WriteByteSeq(Guid[6]);
                packet.WriteFloat(Position.X);
                //has time?
                packet.WriteFloat(baseMoveSpeed[(int)UnitMoveType.Walk]);

                if (hasPitch)
                    packet.WriteFloat(movementInfo.Pitch);

                packet.WriteByteSeq(Guid[5]);
                packet.WriteUInt32(0);//.WriteUnixTime();
                packet.WriteFloat(baseMoveSpeed[(int)UnitMoveType.PitchRate]);
                packet.WriteByteSeq(Guid[2]);
                packet.WriteFloat(baseMoveSpeed[(int)UnitMoveType.Run]);
                packet.WriteByteSeq(Guid[7]);
                packet.WriteFloat(baseMoveSpeed[(int)UnitMoveType.SwimBack]);
                packet.WriteByteSeq(Guid[4]);
                packet.WriteFloat(baseMoveSpeed[(int)UnitMoveType.Flight]);
            }

            //bit520

            if (Convert.ToBoolean(updateFlags & UpdateFlag.HasTarget))
            {
                ObjectGuid VictimGuid = new ObjectGuid();

                packet.WriteBit(VictimGuid[3]);
                packet.WriteBit(VictimGuid[4]);
                packet.WriteBit(VictimGuid[2]);
                packet.WriteBit(VictimGuid[5]);
                packet.WriteBit(VictimGuid[1]);
                packet.WriteBit(VictimGuid[6]);
                packet.WriteBit(VictimGuid[7]);
                packet.WriteBit(VictimGuid[0]);
            }

            if (Convert.ToBoolean(updateFlags & UpdateFlag.StationaryPosition))
            {
                packet.WriteFloat(Position.X);
                packet.WriteFloat(Position.Orientation);
                packet.WriteFloat(Position.Y);
                packet.WriteFloat(Position.Z);

            }

            if (Convert.ToBoolean(updateFlags & UpdateFlag.GoTransportPosition))
            {
                var tguid = movementInfo.TransGuid;

                packet.WriteBit(tguid[3]);
                packet.WriteBit(tguid[1]);
                packet.WriteInt8(movementInfo.TransSeat);
                packet.WriteFloat(movementInfo.TransPos.Z);
                packet.WriteBit(tguid[2]);
                packet.WriteBit(tguid[7]);
                //time3
                packet.WriteBit(tguid[6]);
                //time2
                packet.WriteUInt32(movementInfo.TransTime);
                packet.WriteFloat(movementInfo.TransPos.Y);
                packet.WriteFloat(movementInfo.TransPos.X);
                packet.WriteBit(tguid[0]);
                packet.WriteBit(tguid[4]);
                packet.WriteBit(tguid[5]);
                packet.WriteFloat(movementInfo.TransPos.Orientation);
            }

            //hasanimkits

            if (Convert.ToBoolean(updateFlags & UpdateFlag.Vehicle))
            {  
                packet.WriteFloat(movementInfo.Pos.Orientation);
                packet.WriteUInt32(0);//*data << uint32(((Unit*)this)->GetVehicleInfo()->GetEntry()->m_ID); // vehicle id
            }

            if (Convert.ToBoolean(updateFlags & UpdateFlag.Transport))
                packet.WriteUnixTime();//Transport Path Timer

            //bit 644

            //bit 784

            if (Convert.ToBoolean(updateFlags & UpdateFlag.Rotation))
                packet.WriteInt64(0);//(this as GameObject).PackedRotation); 
        }
        //Update_Object Create
        public void BuildCreateUpdateBlockForPlayer(ref UpdateData data, Player target)
        {
            if (target == null)
                return;

            UpdateType updatetype = UpdateType.CreateObject;
            UpdateFlag flags = updateFlags;
            uint valCount = ValuesCount;

            if (target == this)
                flags |= UpdateFlag.Self;
            else if (GetTypeId() == ObjectType.Player)
                valCount = (uint)PlayerFields.EndNotSelf;

            switch (GetTypeId())
            {
                //case ObjectType.Player:
                //case ObjectType.HIGHGUID_PET:
                case ObjectType.Corpse:
                case ObjectType.DynamicObject:
                    updatetype = UpdateType.CreateObject2;
                    break;
                //case HIGHGUID_UNIT:
                // if (ToUnit()->ToTempSummon() && IS_PLAYER_GUID(ToUnit()->ToTempSummon()->GetSummonerGUID()))
                //updateType = UPDATETYPE_CREATE_OBJECT2;
                //break;
                //case HIGHGUID_GAMEOBJECT:
                //if (IS_PLAYER_GUID(ToGameObject()->GetOwnerGUID()))
                //updateType = UPDATETYPE_CREATE_OBJECT2;
                //break;
            }

            if (Convert.ToBoolean(updateFlags & UpdateFlag.StationaryPosition))
            {
                // UPDATETYPE_CREATE_OBJECT2 for some gameobject types...
                if (GetTypeId() == ObjectType.GameObject)
                {
                    switch ((GameObjectTypes)(this as GameObject).GetGoInfo().type)
                    {
                        case GameObjectTypes.Trap:
                        case GameObjectTypes.DuelArbiter:
                        case GameObjectTypes.FlagStand:
                        case GameObjectTypes.FlagDrop:
                            updatetype = UpdateType.CreateObject2;
                            break;
                        case GameObjectTypes.Transport:
                            flags |= UpdateFlag.Transport;
                            break;
                        default:
                            break;
                    }
                }

                if (GetTypeId() == ObjectType.Unit)
                {
                    //if(((Unit*)this)->getVictim())
                    //updateFlags |= UpdateFlag.HasTarget;
                }
            }
            PacketWriter writer = new PacketWriter();
            writer.WriteUInt8((byte)updatetype);
            writer.WritePackedGuid(GetPackGUID());
            writer.WriteUInt8((byte)objectTypeId);
            WriteUpdateObjectMovement(ref writer, flags);
            SetValuesCount(valCount);
            SetCreateBits(target);
            BuildValuesUpdate(updatetype, ref writer, target);
            writer.WriteUInt8(0);
            data.AddUpdateBlock(writer);
        }
        public void BuildValuesUpdate(UpdateType updatetype, ref PacketWriter data, Player target)
        {
            if (target == null)
                return;

            bool IsActivateToQuest = false;
            //bool IsPerCasterAuraState = false;

            if (updatetype == UpdateType.CreateObject || updatetype == UpdateType.CreateObject2)
            {
                if (GetTypeId() == ObjectType.GameObject && !(this as GameObject).IsGameObjectType(GameObjectTypes.Transport))
                {
                    //if (((GameObject*)this)->ActivateToQuest(target) || target->isGameMaster())
                    //IsActivateToQuest = true;

                    //SetBit((int)GameObjectFields.Dynamic);
                }
                else if (GetTypeId() == ObjectType.Unit)
                {
                    //if (((Unit*)this)->HasAuraState(AURA_STATE_CONFLAGRATE))
                    //{
                    //IsPerCasterAuraState = true;
                    //SetBit(UNIT_FIELD_AURASTATE);
                    //}
                }
            }
            else
            {

                if (GetTypeId() == ObjectType.GameObject && !(this as GameObject).IsGameObjectType(GameObjectTypes.Transport))
                {
                    //if (((GameObject*)this)->ActivateToQuest(target) || target->isGameMaster())
                    //IsActivateToQuest = true;

                    //SetBit((int)GameObjectFields.Dynamic);
                }
                else if (GetTypeId() == ObjectType.Unit)
                {
                    //if (((Unit*)this)->HasAuraState(AURA_STATE_CONFLAGRATE))
                    //{
                    //IsPerCasterAuraState = true;
                    //updateMask->SetBit(UNIT_FIELD_AURASTATE);
                    //}
                }
            }

            data.WriteUInt8((byte)BlockCount);
            data.Write(_UpdateMask, 0, (int)BlockCount * 4);
            /*
            if (GetTypeId() == ObjectType.Unit)
            {
                for (int index = 0; index < ValuesCount; ++index)
                {
                    if (GetBit(index))
                    {
                        if (index == (int)UnitFields.NpcFlags)
                        {
                            uint appendValue = GetValue<uint>(index);

                            if (GetTypeId() == ObjectType.Unit)
                            {
                                //if (!target->canSeeSpellClickOn((Creature*)this))
                                    //appendValue &= ~UNIT_NPC_FLAG_SPELLCLICK;
                                
                                if (Convert.ToBoolean(appendValue & (uint)NPCFlags.Trainer))
                                {
                                    if (!(this as Creature).isTrainerOf(target, false))
                                        appendValue &= ~(uint)(NPCFlags.Trainer | NPCFlags.TrainerClass | NPCFlags.TrainerProfession);
                                }
                            }
                            data.WriteUInt32(appendValue);
                        }
                        else if (index == (int)UnitFields.AuraState)
                        {
                            if (IsPerCasterAuraState)
                            {
                                // IsPerCasterAuraState set if related pet caster aura state set already
                                if (((Unit*)this)->HasAuraStateForCaster(AURA_STATE_CONFLAGRATE, target->GetObjectGuid()))
                                    *data << m_uint32Values[index];
                                else
                                    *data << (m_uint32Values[index] & ~(1 << (AURA_STATE_CONFLAGRATE-1)));
                            }
                            else
                                *data << m_uint32Values[index];
                        }
                        else if (index >= (int)UnitFields.AttackRoundBaseTime && index <= (int)UnitFields.RangedAttackRoundBaseTime)
                        {
                            var val = GetValue<float>(index);
                            data.WriteUInt32((uint)(val < 0 ? 0 : val));
                        }
                        else if ((index >= (int)UnitFields.StatNegBuff && index < (int)UnitFields.Resistances) ||
                            (index >= (int)UnitFields.ResistanceBuffModsPositive && index <= ((int)UnitFields.ResistanceBuffModsPositive + 6)) ||
                            (index >= (int)UnitFields.ResistanceBuffModsNegative && index <= ((int)UnitFields.ResistanceBuffModsNegative + 6)) ||
                            (index >= (int)UnitFields.StatPosBuff && index < (int)UnitFields.StatNegBuff))
                        {
                            data.WriteUInt32(GetValue<uint>(index));
                        }
                        //else if (index == (int)UnitFields.Flags && Convert.ToBoolean(target.CharacterFlags & (uint)PlayerFlags.GM))
                        //{
                            //data.WriteUInt32((uint)(GetValue<int>(index) & ~(int)UnitFlags.NotSelectable));
                        //}
                        //else if (index == (int)UnitFields.DynamicFlags && (this is Unit))
                        //{
                            /*
                            if (!target->isAllowedToLoot((Creature*)this))
                                *data << (m_uint32Values[index] & ~(UNIT_DYNFLAG_LOOTABLE | UNIT_DYNFLAG_TAPPED_BY_PLAYER));
                            else
                            {
                                // flag only for original loot recipent
                                if (target->GetObjectGuid() == ((Creature*)this)->GetLootRecipientGuid())
                                    *data << m_uint32Values[index];
                                else
                                    *data << (m_uint32Values[index] & ~(UNIT_DYNFLAG_TAPPED | UNIT_DYNFLAG_TAPPED_BY_PLAYER));
                            }

                        //}
                        else
                            WriteValue(ref data, index);
                    }
                }
            }
            else if (GetTypeId() == ObjectType.GameObject)
            {
                for (int index = 0; index < ValuesCount; ++index)
                {
                    if (GetBit(index))
                    {
                        if (index == (int)GameObjectFields.Dynamic)
                        {                      
                            if (IsActivateToQuest)
                            {
                                switch ((GameObjectTypes)(this as GameObject).GetGoInfo().type)
                                {
                                    case GameObjectTypes.Generic:
                                        data.WriteUInt16((ushort)GameObjectDynamicLowFlags.Sparkle);
                                        break;
                                    case GameObjectTypes.Chest:
                                    case GameObjectTypes.Goober:
                                        data.WriteUInt16((ushort)(GameObjectDynamicLowFlags.Activate | GameObjectDynamicLowFlags.Sparkle));
                                        break;
                                    default:
                                        data.WriteUInt16(0);
                                        break;
                                }
                            }
                            else
                                data.WriteUInt16(0);
                            
                            data.WriteInt16(-1);
                        }
                        else
                            WriteValue(ref data, index);
                    }
                }
            }
            else
            {
                */
            for (int index = 0; index < ValuesCount; ++index)
            {
                if (GetBit(index))
                    WriteValue(data, index);
            }
            //}
        }
        public void WriteValue(PacketWriter data, int index)
        {
            if (UpdateData[index] is uint)
                data.WriteUInt32((uint)UpdateData[index]);
            else if (UpdateData[index] is float)
                data.WriteFloat((float)UpdateData[index]);
            else //if (UpdateData[index] is int)
                data.WriteInt32((int)UpdateData[index]);
        }

        //Update_Object Values
        public void BuildValuesUpdateBlockForPlayer(ref UpdateData data, Player target)
        {
            PacketWriter writer = new PacketWriter();

            writer.WriteUInt8((byte)UpdateType.Values);
            writer.WritePackedGuid(GetPackGUID());

            uint valCount = ValuesCount;
            if (this is Player && target != this)
                valCount = (uint)PlayerFields.EndNotSelf;

            SetValuesCount(valCount);

            SetUpdateBits(target);
            BuildValuesUpdate(UpdateType.Values, ref writer, target);

            data.AddUpdateBlock(writer);
            ClearUpdateMask(false);
        }

        bool IsUpdateFieldVisible(uint flags, bool isSelf, bool isOwner, bool isItemOwner, bool isPartyMember)
        {
            if (flags == UpdateFieldFlags.None)
                return false;

            if (Convert.ToBoolean(flags & UpdateFieldFlags.All))
                return true;

            if (Convert.ToBoolean(flags & UpdateFieldFlags.Self) && isSelf)
                return true;

            if (Convert.ToBoolean(flags & UpdateFieldFlags.Owner) && isOwner)
                return true;

            if (Convert.ToBoolean(flags & UpdateFieldFlags.ItemOwner) && isItemOwner)
                return true;

            if (Convert.ToBoolean(flags & UpdateFieldFlags.Party) && isPartyMember)
                return true;

            return false;
        }
        void GetUpdateFieldData(Player target, out uint[] flags, out bool isOwner, out bool isItemOwner, out bool hasUnitAll, out bool isPartyMember)
        {
            flags = null;
            isOwner = false;
            isItemOwner = false;
            hasUnitAll = false;
            isPartyMember = false;
            // This function assumes updatefield index is always valid
            switch (GetTypeId())
            {
                case ObjectType.Item:
                case ObjectType.Container:
                    flags = UpdateFieldFlags.ItemUpdateFieldFlags;
                    isOwner = isItemOwner = (this as Item).GetOwnerGUID() == target.GetGUID();
                    break;
                case ObjectType.Unit:
                case ObjectType.Player:
                    {
                        Player plr = ToUnit().GetCharmerOrOwnerPlayerOrPlayerItself();
                        flags = UpdateFieldFlags.UnitUpdateFieldFlags;
                        isOwner = ToUnit().GetOwnerGUID() == target.GetGUID();
                        //hasUnitAll = ToUnit().HasAuraTypeWithCaster(SPELL_AURA_EMPATHY, target.GetGUID());
                        isPartyMember = plr != null && plr.IsInSameGroupWith(target);
                        break;
                    }
                case ObjectType.GameObject:
                    flags = UpdateFieldFlags.GameObjectUpdateFieldFlags;
                    isOwner = ToGameObject().GetOwnerGUID() == target.GetGUID();
                    break;
                case ObjectType.DynamicObject:
                    flags = UpdateFieldFlags.DynamicObjectUpdateFieldFlags;
                    //isOwner = ((DynamicObject*)this)->GetCasterGUID() == target.GetGUID();
                    break;
                case ObjectType.Corpse:
                    flags = UpdateFieldFlags.CorpseUpdateFieldFlags;
                    isOwner = ToCorpse().GetOwnerGUID() == target.GetGUID();
                    break;
                case ObjectType.Object:
                    break;
            }
        }
        public void ClearUpdateMask(bool remove)
        {
            if (UpdateData.Count != 0)
            {
                for (int index = 0; index < ValuesCount; ++index)
                {
                    if (UpdateData_mirror[index] != UpdateData[index])
                        UpdateData_mirror[index] = UpdateData[index];
                }
            }
        }
        void SetUpdateBits(Player target)
        {
            uint[] flags;
            bool isSelf = target == this;
            bool isOwner = false;
            bool isItemOwner = false;
            bool hasUnitAll = false;
            bool isPartyMember = false;

            GetUpdateFieldData(target, out flags, out isOwner, out isItemOwner, out hasUnitAll, out isPartyMember);

            uint valCount = ValuesCount;
            if (GetTypeId() == ObjectType.Player && target != this)
                valCount = (int)PlayerFields.EndNotSelf;

            for (var index = 0; index < valCount; ++index)
            {
                if (UpdateData[index] != UpdateData_mirror[index])
                    if (Convert.ToBoolean(_fieldNotifyFlags & flags[index]) || (Convert.ToBoolean(flags[index] & UpdateFieldFlags.UnitAll) && hasUnitAll)
                    || IsUpdateFieldVisible(flags[index], isSelf, isOwner, isItemOwner, isPartyMember))
                        SetBit(index);
            }
        }
        void SetCreateBits(Player target)
        {
            uint[] flags;
            bool isSelf = target == this;
            bool isOwner = false;
            bool isItemOwner = false;
            bool hasUnitAll = false;
            bool isPartyMember = false;

            GetUpdateFieldData(target, out flags, out isOwner, out isItemOwner, out hasUnitAll, out isPartyMember);

            uint valCount = ValuesCount;
            if (GetTypeId() == ObjectType.Player && target != this)
                valCount = (int)PlayerFields.EndNotSelf;

            for (var index = 0; index < valCount; ++index)
            {
                if (UpdateData[index] != null)
                {
                    if (Convert.ToBoolean(_fieldNotifyFlags & flags[index]) || (Convert.ToBoolean(flags[index] & UpdateFieldFlags.UnitAll) && hasUnitAll)
                        || IsUpdateFieldVisible(flags[index], isSelf, isOwner, isItemOwner, isPartyMember))
                        SetBit(index);
                }
            }

        }
        #endregion

        //Sends
        public void SendMessageToSet(PacketWriter data, bool self)
        {
            if (IsInWorld)
                SendMessageToSetInRange(data, GetVisibilityRange(), self);
        }
        void SendMessageToSetInRange(PacketWriter data, float dist, bool self)
        {
            MessageDistDeliverer notifier = new MessageDistDeliverer(this, data, dist);
            VisitNearbyWorldObject(dist, notifier);
        }

        public void SendNotification(CypherStrings str, params object[] args)
        {
            string message = string.Format(Cypher.ObjMgr.GetCypherString(str), args);
            if (message != "")
            {
                PacketWriter data = new PacketWriter(Opcodes.SMSG_Notification);
                data.WriteBits(message.Length, 13);
                data.BitFlush();
                data.WriteString(message);
                session.Send(data);
            }
        }
        public void SendPlaySpellVisualKit(uint id, uint unkParam)
        {
            ObjectGuid guid = new ObjectGuid(GetPackGUID());

            PacketWriter data = new PacketWriter(Opcodes.SMSG_PlaySpellVisualKit);
            data.WriteUInt32(0);
            data.WriteUInt32(id);     // SpellVisualKit.dbc index
            data.WriteUInt32(unkParam);
            data.WriteBit(guid[4]);
            data.WriteBit(guid[7]);
            data.WriteBit(guid[5]);
            data.WriteBit(guid[3]);
            data.WriteBit(guid[1]);
            data.WriteBit(guid[2]);
            data.WriteBit(guid[0]);
            data.WriteBit(guid[6]);
            data.BitFlush();
            data.WriteByteSeq(guid[0]);
            data.WriteByteSeq(guid[4]);
            data.WriteByteSeq(guid[1]);
            data.WriteByteSeq(guid[6]);
            data.WriteByteSeq(guid[7]);
            data.WriteByteSeq(guid[2]);
            data.WriteByteSeq(guid[3]);
            data.WriteByteSeq(guid[5]);
            SendMessageToSet(data, true);
        }
        public void SendUpdateToPlayer(Player player)
        {
            // send create update to player
            UpdateData upd = new UpdateData(player.GetMapId());

            BuildCreateUpdateBlockForPlayer(ref upd, player);
            upd.SendPackets(ref player);
        }
        public void DestroyForPlayer(Player target, bool onDeath = false)
        {

            PacketWriter data = new PacketWriter(Opcodes.SMSG_DestroyObject);
            data.WriteUInt64(GetGUID());
            //! If the following bool is true, the client will call "void CGUnit_C::OnDeath()" for this object.
            //! OnDeath() does for eg trigger death animation and interrupts certain spells/missiles/auras/sounds...
            data.WriteUInt8(onDeath ? 1 : 0);
            target.GetSession().Send(data);
        }

        public ulong MakeNewGuid(uint guidlow, uint entry, HighGuidType guidhigh)
        {
            return (ulong)(guidlow | ((ulong)entry << 32) | (ulong)guidhigh << 48);
        }

        public void LoadIntoDataField(string data, uint startOffset, uint count)
        {
            if (data == string.Empty)
                return;
            string[] lines = data.Split(' ');

            if (lines.Count() != count)
                return;

            for (var index = 0; index < count; ++index)
            {
                UpdateData[startOffset + index] = lines[index];
            }
        }

        public bool IsPositionValid()
        {
            return GridDefines.IsValidMapCoord(Position.X, Position.Y, Position.Z, Position.Orientation);
        }

        public void SetSession(WorldSession _session) { session = _session; }

        public void GetZoneAndAreaId(out uint zoneid, out uint areaid)
        {
            GetMap().GetZoneAndAreaId(out zoneid, out areaid, GetPositionX(), GetPositionY(), GetPositionZ());
        }

        public uint GetPhaseMask() { return m_phaseMask; }
        public bool InSamePhase(WorldObject obj) { return InSamePhase(obj.GetPhaseMask()); }
        public bool InSamePhase(uint phasemask) { return Convert.ToBoolean(GetPhaseMask() == phasemask); }

        //Map
        public bool IsInMap(WorldObject obj)
        {
            if (obj != null)
                return IsInWorld && obj.IsInWorld && (GetMap() == obj.GetMap());
            return false;
        }
        public bool IsWithinDistInMap(WorldObject obj, float dist2compare, bool is3D = true)
        {
            return obj != null && IsInMap(obj) /*&& InSamePhase(obj)*/ && _IsWithinDist(obj, dist2compare, is3D);
        }
        public bool IsWithinDist(WorldObject obj, float dist2compare, bool is3D = true)
        {
            return obj != null && _IsWithinDist(obj, dist2compare, is3D);
        }
        public bool _IsWithinDist(WorldObject obj, float dist2compare, bool is3D)
        {
            float sizefactor = GetObjectSize() + obj.GetObjectSize();
            float maxdist = dist2compare + sizefactor;
            /*
            if (m_transport && obj.GetTransport() && obj.GetTransport().GetGUIDLow() == m_transport.GetGUIDLow())
            {
                float dtx = m_movementInfo.t_pos.m_positionX - obj->m_movementInfo.t_pos.m_positionX;
                float dty = m_movementInfo.t_pos.m_positionY - obj->m_movementInfo.t_pos.m_positionY;
                float disttsq = dtx * dtx + dty * dty;
                if (is3D)
                {
                    float dtz = m_movementInfo.t_pos.m_positionZ - obj->m_movementInfo.t_pos.m_positionZ;
                    disttsq += dtz * dtz;
                }
                return disttsq < (maxdist * maxdist);
            }*/

            float dx = GetPositionX() - obj.GetPositionX();
            float dy = GetPositionY() - obj.GetPositionY();
            float distsq = dx * dx + dy * dy;
            if (is3D)
            {
                float dz = GetPositionZ() - obj.GetPositionZ();
                distsq += dz * dz;
            }

            return distsq < maxdist * maxdist;
        }
        public bool IsWithinDist3d(float x, float y, float z, float dist) { return IsInDist(x, y, z, dist + GetObjectSize()); }
        public bool IsWithinDist3d(ObjectPosition pos, float dist) { return IsInDist(pos, dist + GetObjectSize()); }
        public bool IsWithinDist2d(float x, float y, float dist) { return IsInDist2d(x, y, dist + GetObjectSize()); }
        public bool IsWithinDist2d(ObjectPosition pos, float dist) { return IsInDist2d(pos, dist + GetObjectSize()); }
        public bool IsInDist2d(float x, float y, float dist) { return GetExactDist2dSq(x, y) < dist * dist; }
        public bool IsInDist2d(ObjectPosition pos, float dist) { return GetExactDist2dSq(pos) < dist * dist; }
        public bool IsInDist(float x, float y, float z, float dist) { return GetExactDistSq(x, y, z) < dist * dist; }
        public bool IsInDist(ObjectPosition pos, float dist) { return GetExactDistSq(pos) < dist * dist; }
        public float GetExactDist2dSq(float x, float y) { float dx = Position.X - x; float dy = Position.Y - y; return dx * dx + dy * dy; }
        float GetExactDist2d(float x, float y) { return (float)Math.Sqrt(GetExactDist2dSq(x, y)); }
        public float GetExactDist2dSq(ObjectPosition pos) { float dx = Position.X - pos.X; float dy = Position.Y - pos.Y; return dx * dx + dy * dy; }
        float GetExactDist2d(ObjectPosition pos) { return (float)Math.Sqrt(GetExactDist2dSq(pos)); }
        float GetExactDistSq(float x, float y, float z) { float dz = Position.Z - z; return GetExactDist2dSq(x, y) + dz * dz; }
        float GetExactDist(float x, float y, float z) { return (float)Math.Sqrt(GetExactDistSq(x, y, z)); }
        float GetExactDistSq(ObjectPosition pos) { float dx = Position.X - pos.X; float dy = Position.Y - pos.Y; float dz = Position.Z - pos.Z; return dx * dx + dy * dy + dz * dz; }
        float GetExactDist(ObjectPosition pos) { return (float)Math.Sqrt(GetExactDistSq(pos)); }
        public bool IsWithinLOSInMap(WorldObject obj)
        {
            if (!IsInMap(obj))
                return false;

            return IsWithinLOS(obj.GetPositionX(), obj.GetPositionY(), obj.GetPositionZ());
        }
        bool IsWithinLOS(float ox, float oy, float oz)
        {
            if (IsInWorld)
                return true; //GetMap().isInLineOfSight(GetPositionX(), GetPositionY(), GetPositionZ() + 2.0f, ox, oy, oz + 2.0f, GetPhaseMask());

            return true;
        }
        public bool HasInArc(float arc, WorldObject obj)
        {
            // always have self in arc
            if (obj == this)
                return true;

            // move arc to range 0.. 2*pi
            //arc = NormalizeOrientation(arc);

            float angle = GetAngle(obj);
            angle -= GetOrientation();

            // move angle to range -pi ... +pi
            //angle = NormalizeOrientation(angle);
            if (angle > Math.PI)
                angle -= (float)(2.0f * Math.PI);

            float lborder = -1 * (arc / 2.0f);                        // in range -pi..0
            float rborder = (arc / 2.0f);                             // in range 0..pi
            return ((angle >= lborder) && (angle <= rborder));
        }

        public bool canSeeOrDetect(WorldObject obj, bool ignoreStealth = false, bool distanceCheck = false)
        {
            if (this == obj)
                return true;

            //if (obj.IsNeverVisible() || CanNeverSee(obj))
            //return false;

            //if (obj.IsAlwaysVisibleFor(this) || CanAlwaysSee(obj))
            //return true;

            bool corpseVisibility = false;
            if (distanceCheck)
            {
                bool corpseCheck = false;
                Player thisPlayer = ToPlayer();
                if (thisPlayer != null)
                {
                    //if (thisPlayer.isDead() && thisPlayer.GetHealth() > 0 && // Cheap way to check for ghost state
                        //!(obj.m_serverSideVisibility.GetValue(SERVERSIDE_VISIBILITY_GHOST) & m_serverSideVisibility.GetValue(SERVERSIDE_VISIBILITY_GHOST) & GHOST_VISIBILITY_GHOST))
                    {
                        Corpse corpse = thisPlayer.GetCorpse();
                        if (corpse != null)
                        {
                            corpseCheck = true;
                            if (corpse.IsWithinDist(thisPlayer, GetSightRange(obj), false))
                                if (corpse.IsWithinDist(obj, GetSightRange(obj), false))
                                    corpseVisibility = true;
                        }
                    }
                }

                WorldObject viewpoint = this;
                Player player = this.ToPlayer();
                if (player != null)
                    viewpoint = player.GetViewpoint();

                if (viewpoint == null)
                    viewpoint = this;

                if (!corpseCheck && !viewpoint.IsWithinDist(obj, GetSightRange(obj), false))
                    return false;
            }

            // GM visibility off or hidden NPC
            //if (!obj.m_serverSideVisibility.GetValue(SERVERSIDE_VISIBILITY_GM))
            {
                // Stop checking other things for GMs
                //if (m_serverSideVisibilityDetect.GetValue(SERVERSIDE_VISIBILITY_GM))
                    //return true;
            }
            //else
                //return m_serverSideVisibilityDetect.GetValue(SERVERSIDE_VISIBILITY_GM) >= obj.m_serverSideVisibility.GetValue(SERVERSIDE_VISIBILITY_GM);

            // Ghost players, Spirit Healers, and some other NPCs
            //if (!corpseVisibility && !(obj.m_serverSideVisibility.GetValue(SERVERSIDE_VISIBILITY_GHOST) & m_serverSideVisibilityDetect.GetValue(SERVERSIDE_VISIBILITY_GHOST)))
            {
                // Alive players can see dead players in some cases, but other objects can't do that
                Player thisPlayer = ToPlayer();
                if (thisPlayer != null)
                {
                    Player objPlayer = obj.ToPlayer();
                    if (objPlayer != null)
                    {
                        if (thisPlayer.GetTeam() != objPlayer.GetTeam())// || !thisPlayer.IsGroupVisibleFor(objPlayer))
                            return false;
                    }
                    //else
                        //return false;
                }
                else
                    return false;
            }

            //if (obj.IsInvisibleDueToDespawn())
                //return false;

            //if (!CanDetect(obj, ignoreStealth))
                //return false;
            return true;
        }


        public void GetContactPoint(WorldObject obj, out float x, out float y, out float z, float distance2d = 0.5f)
        {
            // angle to face `obj` to `this` using distance includes size of `obj`
            GetNearPoint(obj, out x, out y, out z, obj.GetObjectSize(), distance2d, GetAngle(obj));
        }
        void GetNearPoint2D(out float x, out float y, float distance2d, float absAngle)
        {
            x = (float)(GetPositionX() + (GetObjectSize() + distance2d) * Math.Cos(absAngle));
            y = (float)(GetPositionY() + (GetObjectSize() + distance2d) * Math.Sin(absAngle));

            //NormalizeMapCoord(x);
            //NormalizeMapCoord(y);
        }

        public void GetNearPoint(WorldObject searcher, out float x, out float y, out float z, float searcher_size, float distance2d, float absAngle)
        {
            GetNearPoint2D(out x, out y, distance2d + searcher_size, absAngle);
            z = GetPositionZ();
            //UpdateAllowedPositionZ(x, y, z);
        }
        public void GetClosePoint(out float x, out float y, out float z, float size, float distance2d = 0, float angle = 0)
        {
            // angle calculated from current orientation
            GetNearPoint(null, out x, out y, out z, size, distance2d, GetOrientation() + angle);
        }

        public float GetAngle(WorldObject obj)
        {
            if (obj == null)
                return 0;

            return GetAngle(obj.GetPositionX(), obj.GetPositionY());
        }

        // Return angle in range 0..2*pi
        public float GetAngle(float x, float y)
        {
            float dx = x - GetPositionX();
            float dy = y - GetPositionY();

            float ang = (float)Math.Atan2(dy, dx);
            ang = (float)(ang >= 0 ? ang : 2 * Math.PI + ang);
            return ang;
        }
        
        //relocation and visibility system functions
        public void AddToNotify(NotifyFlags f) { m_notifyflags |= f; }
        public bool isNeedNotify(NotifyFlags f) { return Convert.ToBoolean(m_notifyflags & f); }
        NotifyFlags GetNotifyFlags() { return m_notifyflags; }
        bool NotifyExecuted(NotifyFlags f) { return Convert.ToBoolean(m_executed_notifies & f); }
        void SetNotified(NotifyFlags f) { m_executed_notifies |= f; }
        public void ResetAllNotifies() { m_notifyflags = 0; m_executed_notifies = 0; }

        public virtual void UpdateObjectVisibility(bool force)
        {
            //updates object's visibility for nearby players
            VisibleChangesNotifier notifier = new VisibleChangesNotifier(this);
            VisitNearbyWorldObject(GetVisibilityRange(), notifier);
        }
        public void VisitNearbyObject<T>(float radius, T notifier) where T : Notifier  { if (IsInWorld) GetMap().VisitAll(GetPositionX(), GetPositionY(), radius, notifier); }
        public void VisitNearbyGridObject<T>(float radius, T notifier) where T : Notifier{ if (IsInWorld) GetMap().VisitGrid(GetPositionX(), GetPositionY(), radius, notifier); }
        public void VisitNearbyWorldObject<T>(float radius, T notifier)where T : Notifier { if (IsInWorld) GetMap().VisitWorld(GetPositionX(), GetPositionY(), radius, notifier); }
        public float GetGridActivationRange()
        {
            if (ToPlayer() != null)
                return GetMap().GetVisibilityRange();
            else if (ToCreature() != null)
                return ToCreature().m_SightDistance;
            else
                return 0.0f;
        }





        public virtual void CleanupsBeforeDelete(bool finalCleanup = true) { }  // used in destructor or explicitly before mass creature delete to remove cross-references to already deleted units





        public float GetVisibilityRange()
        {
            if (isActiveObject() && ToPlayer() == null)
                return ObjectConst.MAX_VISIBILITY_DISTANCE;
            else
                return GetMap().GetVisibilityRange();
        }
        public float GetSightRange(WorldObject target = null)
        {
            if (ToUnit() != null)
            {
                if (ToPlayer() != null)
                {
                    if (target != null && target.isActiveObject() && target.ToPlayer() == null)
                        return ObjectConst.MAX_VISIBILITY_DISTANCE;
                    else
                        return GetMap().GetVisibilityRange();
                }
                else if (ToCreature() != null)
                    return ToCreature().m_SightDistance;
                else
                    return ObjectConst.SIGHT_RANGE_UNIT;
            }

            return 0.0f;
        }
        public virtual void Update(uint diff){}
        

        #region Fields
        //Public
        public bool IsInWorld { get; set; }
        public ObjectPosition Position { get; set; }
        public HighGuidMask objectTypeMask { get; set; }
        public ObjectType objectTypeId { get; set; }
        public UpdateFlag updateFlags;
        public bool RequiresUpdate = false;
        public MovementInfo movementInfo;

        //Private
        bool m_isWorldObject;
        internal string Name;
        WorldSession session;
        Hashtable UpdateData;
        Hashtable UpdateData_mirror;
        Map CurrMap;
        byte[] _UpdateMask;
        uint BlockCount;
        uint ValuesCount;
        ulong PackedGuid;
        uint _fieldNotifyFlags;
        bool m_isActive;
        uint m_phaseMask;
        NotifyFlags m_notifyflags;
        NotifyFlags m_executed_notifies;
        #endregion

        public float[] baseMoveSpeed = new float[]
        {
            2.5f,                  // MOVE_WALK
            7.0f,                  // MOVE_RUN
            4.5f,                  // MOVE_RUN_BACK
            4.722222f,             // MOVE_SWIM
            2.5f,                  // MOVE_SWIM_BACK
            3.141594f,             // MOVE_TURN_RATE
            7.0f,                  // MOVE_FLIGHT
            4.5f,                  // MOVE_FLIGHT_BACK
            3.14f                  // MOVE_PITCH_RATE
        };
    }

    public class ObjectPosition
    {
        public ObjectPosition()
            : this(0, 0, 0, 0) { }
        public ObjectPosition(float x, float y, float z, float o)
        {
            X = x;
            Y = y;
            Z = z;
            Orientation = o;
        }

        public float X;
        public float Y;
        public float Z;
        public float Orientation;

        public void SetOrientation(float orientation) { Orientation = orientation; }
        public static float NormalizeOrientation(float o)
        {
            if (o < 0)
            {
                float mod = o * -1;
                mod = (float)Math.IEEERemainder(mod, 2.0f * (float)Math.PI);
                mod = (float)(-mod + 2.0f * (float)Math.PI);
                return mod;
            }
            return (float)Math.IEEERemainder(o, 2.0f * Math.PI);
        }
        public void Relocate(float x, float y, float z) { X = x; Y = y; Z = z; }
        public void Relocate(float x, float y, float z, float o) { X = x; Y = y; Z = z; SetOrientation(o); }
        public void Relocate(ObjectPosition obj) { X = obj.X; Y = obj.Y; Z = obj.Z; SetOrientation(obj.Orientation); }
        public void RelocateOffset(ObjectPosition offset)
        {
            X = (float)(X + (offset.X * Math.Cos(Orientation) + offset.Y * Math.Sin(Orientation + Math.PI)));
            Y = (float)(Y + (offset.Y * Math.Cos(Orientation) + offset.X * Math.Sin(Orientation)));
            Z = Z + offset.Z;
            SetOrientation(Orientation + offset.Orientation);
        }
        public void ReadXYZStream(ref PacketReader data)
        {
            X = data.ReadFloat();
            Y = data.ReadFloat();
            Z = data.ReadFloat();
        }
        public byte[] WriteXYZStream()
        {
            PacketWriter data = new PacketWriter();
            data.WriteBytes(BitConverter.GetBytes(X));
            data.WriteBytes(BitConverter.GetBytes(Y));
            data.WriteBytes(BitConverter.GetBytes(Z));
            return data.GetContents();
        }
        public bool IsPositionValid() { return GridDefines.IsValidMapCoord(X, Y, Z, Orientation); }
        public void GetPosition(out float x, out float y) { x = X; y = Y; }
        public void GetPosition(out float x, out float y, out float z) { x = X; y = Y; z = Z; }
        public void GetPosition(out float x, out float y, out float z, out float o) { x = X; y = Y; z = Z; o = Orientation; }
        public void GetPosition(ObjectPosition pos)
        {
            if (pos != null)
                pos.Relocate(X, Y, Z, Orientation);
        }
    }
    public class MovementInfo
    {
        public MovementInfo()
        {
            Flags = MovementFlag.None;
            Flags2 = MovementFlag2.None;
            Time = 0;
            TransTime = 0;
            TransTime2 = 0;
            FallTime = 0;
            SplineElevation = 0;
            Pitch = 0;
            Jump = new JumpInfo();
            TransSeat = -1;
            Pos = new ObjectPosition();
            TransPos = new ObjectPosition();
            Status = new MovementStatus();
            Guid = new ObjectGuid();
            TransGuid = new ObjectGuid();
        }
        public struct JumpInfo
        {
            public float velocity;
            public float sinAngle;
            public float cosAngle;
            public float xyspeed;
        }
        public class MovementStatus
        {
            public bool HasTransportData = false,
                HasTransportTime2 = false,
                HasTransportTime3 = false,
                HasMovementFlags = false,
                HasMovementFlags2 = false,
                HasOrientation = false,
                HasTimeStamp = false,
                HasPitch = false,
                HasFallData = false,
                HasFallDirection = false,
                HasSplineElevation = false,
                HasUnknownBit = false,
                HasSpline = false,
                IsAlive = false;            
        }

        // Movement flags manipulations
        public void AddMovementFlag(MovementFlag f) { Flags |= f; }
        public void RemoveMovementFlag(MovementFlag f) { Flags &= ~f; }
        public bool HasMovementFlag(MovementFlag f) { return Convert.ToBoolean(Flags & f); }
        public bool HasMovementFlag2(MovementFlag2 f) { return Convert.ToBoolean(Flags2 & f); }
        public MovementFlag GetMovementFlags() { return Flags; }
        public void SetMovementFlags(MovementFlag f) { Flags = f; }
        public MovementFlag2 GetMovementFlags2() { return Flags2; }
        public void AddMovementFlag2(MovementFlag2 f) { Flags2 |= f; }

        // Position manipulations
        ObjectPosition GetPos() { return Pos; }
        void SetTransportData(ulong guid, float x, float y, float z, float o, uint time, byte seat) //VehicleSeatEntry seatInfo = null)
        {
            TransGuid = new ObjectGuid(guid);
            TransPos.X = x;
            TransPos.Y = y;
            TransPos.Z = z;
            TransPos.Orientation = o;
            TransTime = time;
            TransSeat = seat;
            //t_seatInfo = seatInfo;
        }
        void ClearTransportData()
        {
            TransGuid = new ObjectGuid();
            TransPos.X = 0.0f;
            TransPos.Y = 0.0f;
            TransPos.Z = 0.0f;
            TransPos.Orientation = 0.0f;
            TransTime = 0;
            TransSeat = -1;
            //t_seatInfo = null;
        }
        ulong GetTransportGuid() { return TransGuid; }
        ObjectPosition GetTransportPos() { return TransPos; }
        int GetTransportSeat() { return TransSeat; }
        //uint GetTransportDBCSeat() { return t_seatInfo ? t_seatInfo->m_ID : 0; }
        //uint GetVehicleSeatFlags() { return t_seatInfo ? t_seatInfo->m_flags : 0; }
        uint GetTransportTime() { return TransTime; }
        uint GetFallTime() { return FallTime; }
        void ChangeOrientation(float o) { Pos.Orientation = o; }
        void ChangePosition(float x, float y, float z, float o) { Pos.X = x; Pos.Y = y; Pos.Z = z; Pos.Orientation = o; }
        void UpdateTime(uint _time) { Time = _time; }

        JumpInfo GetJumpInfo() { return Jump; }

        public ObjectGuid Guid;
        public MovementStatus Status;
        public MovementFlag Flags;
        public MovementFlag2 Flags2;
        public uint Time;
        public ObjectPosition Pos;
        public ObjectGuid TransGuid;
        public ObjectPosition TransPos;
        public uint TransTime;
        public int TransSeat;
        public uint TransTime2;
        public float Pitch;
        public uint FallTime;
        public JumpInfo Jump;
        public float SplineElevation;
    }
}
