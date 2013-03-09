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
using Framework.Constants;
using Framework.Logging;
using Framework.Network;
using Framework.Utility;
using WorldServer.Game.Maps;
using WorldServer.Game.Spells;
using WorldServer.Game.Managers;
using Framework.DataStorage;
using WorldServer.Game.AI;
using System.Collections.Generic;
using Framework.ObjectDefines;
using WorldServer.Game.Movement;
using Framework.Graphics;

namespace WorldServer.Game.WorldEntities
{
    public class Unit : WorldObject
    {
        public Unit(bool isWorldObject) : base(isWorldObject)
        {
            Initialize();

            objectTypeId = ObjectType.Unit;
            objectTypeMask |= HighGuidMask.Unit;
            updateFlags = UpdateFlag.Living;
            m_unitTypeMask = UnitTypeMask.None;            
        }
        void Initialize()
        {
            movespline = new MoveSpline();
            i_motionMaster = new MotionMaster(this);
            CreateStats = new float[(int)Stats.Max];
            m_attackers = new List<Unit>();
            m_weaponDamage = new float[(int)WeaponAttackType.MaxAttack, 2];
            SpeedRates = new float[8];
            m_modAuras = new List<AuraEffect>[(int)AuraType.TotalAuras];
            CustomSpellValueMod = new Dictionary<SpellValueMod, int>();
            m_movesplineTimer = new TimeTrackerSmall();
            m_modAttackSpeedPct = new float[3];
            m_attackTimer = new uint[(int)WeaponAttackType.MaxAttack];
        }

        //Update / Loading
        public override void Update(uint p_time)
        {
                UpdateSplineMovement(p_time);
                i_motionMaster.UpdateMotion(p_time);
        }
        public override void UpdateObjectVisibility(bool forced = true)
        {
            if (!forced)
                AddToNotify(NotifyFlags.VISIBILITY_CHANGED);
            else
            {
                UpdateObjectVisibility(forced);
                // call MoveInLineOfSight for nearby creatures
                //AIRelocationNotifier notifier(*this);
                //VisitNearbyObject(GetVisibilityRange(), notifier);
            }
        }

        //Movement
        void UpdateSplineMovement(uint t_diff)
        {
            int positionUpdateDelay = 400;

            if (movespline.Finalized())
                return;

            movespline.updateState((int)t_diff);
            bool arrived = movespline.Finalized();

            if (arrived)
                DisableSpline();

            m_movesplineTimer.Update((int)t_diff);
            if (m_movesplineTimer.Passed() || arrived)
            {
                m_movesplineTimer.Reset(positionUpdateDelay);
                Vector4 loc = movespline.ComputePosition();

                if (GetTransGUID() != 0)//if (HasUnitMovementFlag(MOVEMENTFLAG_ONTRANSPORT))
                {
                    ObjectPosition pos = movementInfo.TransPos;
                    pos.X = loc.X;
                    pos.Y = loc.Y;
                    pos.Z = loc.Z;
                    pos.Orientation = loc.O;

                    //if (TransportBase* transport = GetDirectTransport())
                    //transport.CalculatePassengerPosition(loc.x, loc.y, loc.z, loc.orientation);
                }
                if (HasUnitState(UnitState.Cannot_Turn))
                    loc.O = GetOrientation();
                UpdatePosition(loc.X, loc.Y, loc.Z, loc.O);
            }
        }
        void DisableSpline()
        {
            movementInfo.RemoveMovementFlag(MovementFlag.Forward);
            movespline._Interrupt();
        }
        public bool IsWalking() { return movementInfo.HasMovementFlag(MovementFlag.Walk); }
        public void SetSpeed(UnitMoveType mtype, float rate, bool forced = false)
        {
            if (rate < 0)
                rate = 0.0f;

            int type = (int)mtype;

            if (SpeedRates[type] == rate)
                return;

            SpeedRates[type] = rate;

            //propagateSpeedChange();

            PacketWriter data;
            var guid = new ObjectGuid(GetPackGUID());

            if (!forced)
            {
                switch (mtype)
                {
                    case UnitMoveType.Walk:
                        data = new PacketWriter(Opcodes.SMSG_MoveSplineSetWalkMode);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[4]);
                        data.BitFlush();
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[5]);
                        break;
                    case UnitMoveType.Run:
                        data = new PacketWriter(Opcodes.SMSG_MoveSplineSetRunSpeed);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[2]);
                        data.BitFlush();
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[1]);
                        break;
                    case UnitMoveType.RunBack:
                        data = new PacketWriter(Opcodes.SMSG_MoveSplineSetRunBackSpeed);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[4]);
                        data.BitFlush();
                        data.WriteByteSeq(guid[1]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[7]);
                        break;
                    case UnitMoveType.Swim:
                        data = new PacketWriter(Opcodes.SMSG_MoveSplineSetSwimSpeed);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[1]);
                        data.BitFlush();
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[3]);
                        break;
                    case UnitMoveType.SwimBack:
                        data = new PacketWriter(Opcodes.SMSG_MoveSplineSetSwimBackSpeed);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[2]);
                        data.BitFlush();
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[2]);
                        break;
                    case UnitMoveType.TurnRate:
                        data = new PacketWriter(Opcodes.SMSG_MoveSplineSetTurnRate);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[0]);
                        data.BitFlush();
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[0]);
                        break;
                    case UnitMoveType.Flight:
                        data = new PacketWriter(Opcodes.SMSG_MoveSplineSetFlightSpeed);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[2]);
                        data.BitFlush();
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteFloat(SpeedRates[type]);
                        break;
                    case UnitMoveType.FlightBack:
                        data = new PacketWriter(Opcodes.SMSG_MoveSplineSetFlightBackSpeed);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[7]);
                        data.BitFlush();
                        data.WriteByteSeq(guid[5]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[4]);
                        break;
                    case UnitMoveType.PitchRate:
                        data = new PacketWriter(Opcodes.SMSG_MoveSplineSetPitchRate);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[2]);
                        data.BitFlush();
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[4]);
                        break;
                    default:
                        Log.outError("Unit::SetSpeed: Unsupported move type ({0}), data not sent to client.", mtype);
                        return;
                }
                //SendToSet(data, true);
            }
            else
            {
                if (this is Player)
                {
                    // register forced speed changes for WorldSession::HandleForceSpeedChangeAck
                    // and do it only for real sent packets and use run for run/mounted as client expected
                    //++ToPlayer().m_forced_speed_changes[mtype];

                    //if (!isInCombat())
                    //if (Pet* pet = ToPlayer().GetPet())
                    //pet.SetSpeed(mtype, m_speed_rate[mtype], forced);
                }

                switch (mtype)
                {
                    case UnitMoveType.Walk:
                        data = new PacketWriter(Opcodes.SMSG_MoveSetWalkSpeed);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[7]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteUInt32(0);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[3]);
                        break;
                    case UnitMoveType.Run:
                        data = new PacketWriter(Opcodes.SMSG_MoveSetRunSpeed);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[4]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteUInt32(0);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[2]);
                        break;
                    case UnitMoveType.RunBack:
                        data = new PacketWriter(Opcodes.SMSG_MoveSetRunBackSpeed);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[7]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteUInt32(0);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[6]);
                        break;
                    case UnitMoveType.Swim:
                        data = new PacketWriter(Opcodes.SMSG_MoveSetSwimSpeed);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[6]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteUInt32(0);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[4]);
                        break;
                    case UnitMoveType.SwimBack:
                        data = new PacketWriter(Opcodes.SMSG_MoveSetSwimBackSpeed);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[7]);
                        data.WriteUInt32(0);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[2]);
                        break;
                    case UnitMoveType.TurnRate:
                        data = new PacketWriter(Opcodes.SMSG_MoveSetTurnRate);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[3]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteUInt32(0);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[4]);
                        break;
                    case UnitMoveType.Flight:
                        data = new PacketWriter(Opcodes.SMSG_MoveSetFlightSpeed);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[4]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteUInt32(0);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[4]);
                        break;
                    case UnitMoveType.FlightBack:
                        data = new PacketWriter(Opcodes.SMSG_MoveSetFlightBackSpeed);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[4]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[5]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteUInt32(0);
                        data.WriteByteSeq(guid[6]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteByteSeq(guid[5]);
                        data.WriteByteSeq(guid[7]);
                        break;
                    case UnitMoveType.PitchRate:
                        data = new PacketWriter(Opcodes.SMSG_MoveSetPitchRate);
                        data.WriteBit(guid[1]);
                        data.WriteBit(guid[2]);
                        data.WriteBit(guid[6]);
                        data.WriteBit(guid[7]);
                        data.WriteBit(guid[0]);
                        data.WriteBit(guid[3]);
                        data.WriteBit(guid[5]);
                        data.WriteBit(guid[4]);
                        data.WriteFloat(SpeedRates[type]);
                        data.WriteByteSeq(guid[6]);
                        data.WriteByteSeq(guid[4]);
                        data.WriteByteSeq(guid[0]);
                        data.WriteUInt32(0);
                        data.WriteByteSeq(guid[1]);
                        data.WriteByteSeq(guid[2]);
                        data.WriteByteSeq(guid[7]);
                        data.WriteByteSeq(guid[3]);
                        data.WriteByteSeq(guid[5]);
                        break;
                    default:
                        Log.outError("Unit::SetSpeed: Unsupported move type ({0}), data not sent to client.", mtype);
                        return;
                }
                SendMessageToSet(data, true);
            }
        }
        public float GetSpeed(UnitMoveType mtype)
        {
            return SpeedRates[(int)mtype] * baseMoveSpeed[(int)mtype];
        }
        public bool UpdatePosition(float x, float y, float z, float orientation, bool teleport = false)
        {
            if (!GridDefines.IsValidMapCoord(x, y, z, orientation))
            {
                Log.outError("Unit::UpdatePosition({0}, {1}, {2}) .. bad coordinates!", x, y, z);
                return false;
            }

            bool turn = (GetOrientation() != orientation);
            bool relocated = (teleport || GetPositionX() != x || GetPositionY() != y || GetPositionZ() != z);

            //if (turn)
            //RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_TURNING);

            if (relocated)
            {
                //RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_MOVE);

                // move and update visible state if need
                if (GetTypeId() == ObjectType.Player)
                    GetMap().PlayerRelocation(ToPlayer(), x, y, z, orientation);
                else
                    GetMap().CreatureRelocation(ToCreature(), x, y, z, orientation);
            }
            else if (turn)
                UpdateOrientation(orientation);

            // code block for underwater state update
            //UpdateUnderwaterState(GetMap(), x, y, z);

            return (relocated || turn);
        }
        void UpdateOrientation(float orientation)
        {
            Position.SetOrientation(orientation);
            //if (IsVehicle())
            //GetVehicleKit().RelocatePassengers();
        }
        public void StopMoving()
        {
            ClearUnitState(UnitState.Moving);

            // not need send any packets if not in world or not moving
            if (!IsInWorld || movespline.Finalized())
                return;

            MoveSplineInit init = new MoveSplineInit(this);
            init.MoveTo(GetPositionX(), GetPositionY(), GetPositionZ());
            init.SetFacing(GetOrientation());
            init.Launch();
        }
        public bool SetWalk(bool enable)
        {
            if (enable == IsWalking())
                return false;

            if (enable)
                AddUnitMovementFlag(MovementFlag.Walk);
            else
                RemoveUnitMovementFlag(MovementFlag.Walk);

            return true;
        }
        public bool IsStopped() { return !(HasUnitState(UnitState.Moving)); }
        public void SetInFront(Unit target)
        {
            if (!HasUnitState(UnitState.Cannot_Turn))
                Position.SetOrientation(GetAngle(target));
        }
        public void UpdateSpeed(UnitMoveType type, bool something)
        {
            //not done yet.
        }
        public MotionMaster GetMotionMaster() { return i_motionMaster; }
        public void GetRandomContactPoint(Unit obj, out float x, out float y, out float z, float distance2dMin, float distance2dMax)
        {
            float combat_reach = GetCombatReach();
            if (combat_reach < 0.1f) // sometimes bugged for players
                combat_reach = ObjectConst.DefaultCombatReach;

            int attacker_number = getAttackers().Count;
            if (attacker_number > 0)
                --attacker_number;
            GetNearPoint(obj, out x, out y, out z, obj.GetCombatReach(), distance2dMin + (distance2dMax - distance2dMin) * (float)RandomHelper.rand_norm()
                , GetAngle(obj) + (attacker_number != 0 ? (float)((Math.PI / 2) - Math.PI * (float)RandomHelper.rand_norm() * (float)attacker_number / combat_reach * 0.3f) : 0.0f));
        }
        void AddUnitMovementFlag(MovementFlag f) { movementInfo.Flags |= f; }
        void RemoveUnitMovementFlag(MovementFlag f) { movementInfo.Flags &= ~f; }
        public bool HasUnitMovementFlag(MovementFlag f) { return (movementInfo.Flags & f) == f; }
        MovementFlag GetUnitMovementFlags() { return movementInfo.Flags; }
        void SetUnitMovementFlags(MovementFlag f) { movementInfo.Flags = f; }

        //Unit
        public void SetLevel(uint lvl)
        {
            SetValue<uint>(UnitFields.Level, lvl);
            // group update
            //if ((this is Player))// && ToPlayer().GetGroup())
            //ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_LEVEL);

            //if (GetTypeId() == TYPEID_PLAYER)
            //sWorld.UpdateCharacterNameDataLevel(ToPlayer().GetGUIDLow(), lvl);
        }
        public uint getLevel() { return GetValue<uint>(UnitFields.Level); }
        public uint GetLevelForTarget(Unit target) { return target.getLevel(); }

        public byte getRace() { return GetValue<byte>(UnitFields.Bytes, 0); }
        public Race GetRace() { return (Race)GetValue<byte>(UnitFields.Bytes, 0); }
        public uint getRaceMask() { return (uint)(1 << (getRace() - 1)); }
        public byte getClass() { return GetValue<byte>(UnitFields.Bytes, 1); }
        public Class GetClass() { return (Class)GetValue<byte>(UnitFields.Bytes, 1); }
        public uint getClassMask() { return (uint)(1 << (getClass() - 1)); }
        public byte getGender() { return GetValue<byte>(UnitFields.Bytes, 2); }

        public void SetNativeDisplayId(uint modelId) { SetValue<uint>(UnitFields.NativeDisplayID, modelId); }
        public void SetDisplayId(uint modelId)
        {
            SetValue<uint>(UnitFields.DisplayID, modelId);

            if (GetTypeId() == ObjectType.Unit && ToCreature().isPet())
            {
                //Pet pet = ToPet();
                //if (!pet.isControlled())
                //return;
                //Unit owner = GetOwner();
                //if (owner && (owner.GetTypeId() == TYPEID_PLAYER) && owner.ToPlayer().GetGroup())
                //owner.ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_PET_MODEL_ID);
            }
        }
        public uint GetNativeDisplayId() { return GetValue<uint>(UnitFields.NativeDisplayID); }

        Unit GetOwner()
        {
            ulong ownerid = GetOwnerGUID();
            if (ownerid != 0)
                return Cypher.ObjMgr.GetObject<Unit>(this, ownerid);

            return null;
        }
        Unit GetCharmer()
        {
            ulong charmerid = GetCharmerGUID();
            if (charmerid != 0)
                return Cypher.ObjMgr.GetObject<Unit>(this, charmerid);
            return null;
        }
        Unit GetCharmerOrOwnerOrSelf()
        {
            Unit u = GetCharmerOrOwner();
            if (u != null)
                return u;

            return (Unit)this;
        }
        public Player GetCharmerOrOwnerPlayerOrPlayerItself()
        {
            ulong guid = GetCharmerOrOwnerGUID();
            if (IS_PLAYER_GUID(guid))
                return Cypher.ObjMgr.FindPlayer(guid);//*this, guid);

            return GetTypeId() == ObjectType.Player ? (this as Player) : null;
        }
        public Unit GetCharmerOrOwner() { return GetCharmerGUID() != 0 ? GetCharmer() : GetOwner(); }
        ulong GetCharmerOrOwnerGUID() { return GetCharmerGUID() != 0 ? GetCharmerGUID() : GetOwnerGUID(); }
        ulong GetCharmerGUID() { return GetValue<ulong>(UnitFields.CharmedBy); }
        public ulong GetOwnerGUID() { return GetValue<ulong>(UnitFields.SummonedBy); }
        public void SetOwnerGUID(ulong owner)
        {
            if (GetOwnerGUID() == owner)
                return;

            SetValue<ulong>(UnitFields.SummonedBy, owner);
            if (owner == 0)
                return;

            // Update owner dependent fields
            Player player = Cypher.ObjMgr.GetPlayerInMap(owner, this);
            if (player == null || !player.HaveAtClient(this)) // if player cannot see this unit yet, he will receive needed data with create object
                return;

            SetFieldNotifyFlag(UpdateFieldFlags.Owner);

            UpdateData udata = new UpdateData(GetMapId());
            BuildValuesUpdateBlockForPlayer(ref udata, player);
            udata.SendPackets(ref player);

            RemoveFieldNotifyFlag(UpdateFieldFlags.Owner);
        }

        public ShapeshiftForm GetShapeshiftForm() { return (ShapeshiftForm)GetValue<byte>(UnitFields.Bytes2, 3); }
        public CreatureType GetCreatureType()
        {
            if (GetTypeId() == ObjectType.Player)
            {
                ShapeshiftForm form = GetShapeshiftForm();
                SpellShapeshiftFormEntry ssEntry = DBCStorage.SpellShapeshiftFormStorage.LookupByKey((uint)form);
                if (ssEntry != null && ssEntry.creatureType > 0)
                    return (CreatureType)ssEntry.creatureType;
                else
                    return CreatureType.Humanoid;
            }
            else
                return (CreatureType)ToCreature().GetCreatureTemplate().CreatureType;
        }
        Player GetAffectingPlayer()
        {
            if (GetCharmerOrOwnerGUID() == 0)
                return GetTypeId() == ObjectType.Player ? (Player)this : null;

            Unit owner = GetCharmerOrOwner();
            if (owner != null)
                return owner.GetCharmerOrOwnerPlayerOrPlayerItself();
            return null;
        }

        public bool isAlive() { return (m_deathState == DeathState.Alive); }
        public bool isDying() { return (m_deathState == DeathState.JustDied); }
        public bool isDead() { return (m_deathState == DeathState.Dead || m_deathState == DeathState.Corpse); }
        public bool isSummon() { return Convert.ToBoolean(m_unitTypeMask & UnitTypeMask.Summon); }
        public bool isGuardian() { return Convert.ToBoolean(m_unitTypeMask & UnitTypeMask.Guardian); }
        public bool isPet() { return Convert.ToBoolean(m_unitTypeMask & UnitTypeMask.Pet); }
        public bool isHunterPet() { return Convert.ToBoolean(m_unitTypeMask & UnitTypeMask.HunterPet); }
        public bool isTotem() { return Convert.ToBoolean(m_unitTypeMask & UnitTypeMask.Totem); }
        public bool IsVehicle() { return Convert.ToBoolean(m_unitTypeMask & UnitTypeMask.Vehicle); }

        public void AddUnitState(UnitState f) { m_state |= f; }
        public bool HasUnitState(UnitState f) { return Convert.ToBoolean(m_state & f); }
        public void ClearUnitState(UnitState f) { m_state &= ~f; }
        public void SetStandFlags(Enum flags) { SetFlag(UnitFields.Bytes1, flags, 2); }
        public void RemoveStandFlags(Enum flags) { RemoveFlag(UnitFields.Bytes1, flags, 2); }

        //Faction
        public bool IsNeutralToAll()
        {
            FactionTemplateEntry my_faction = getFactionTemplateEntry();
            if (my_faction == null || my_faction.faction == 0)
                return true;

            FactionEntry raw_faction = DBCStorage.FactionStorage.LookupByKey(my_faction.faction);
            if (raw_faction != null && raw_faction.reputationListID >= 0)
                return false;

            return my_faction.IsNeutralToAll();
        }
        public bool IsHostileTo(Unit unit)
        {
            return GetReactionTo(unit) <= ReputationRank.Hostile;
        }
        public bool IsFriendlyTo(Unit unit)
        {
            return GetReactionTo(unit) >= ReputationRank.Friendly;
        }
        ReputationRank GetReactionTo(Unit target)
        {
            // always friendly to self
            if (this == target)
                return ReputationRank.Friendly;

            // always friendly to charmer or owner
            if (GetCharmerOrOwnerOrSelf() == target.GetCharmerOrOwnerOrSelf())
                return ReputationRank.Friendly;

            if (HasFlag(UnitFields.Flags, UnitFlags.PvpAttackable))
            {
                if (target.HasFlag(UnitFields.Flags, UnitFlags.PvpAttackable))
                {
                    Player selfPlayerOwner = GetAffectingPlayer();
                    Player targetPlayerOwner = target.GetAffectingPlayer();

                    if (selfPlayerOwner != null && targetPlayerOwner != null)
                    {
                        // always friendly to other unit controlled by player, or to the player himself
                        if (selfPlayerOwner == targetPlayerOwner)
                            return ReputationRank.Friendly;

                        // duel - always hostile to opponent
                        //if (selfPlayerOwner.duel && selfPlayerOwner.duel.opponent == targetPlayerOwner && selfPlayerOwner.duel.startTime != 0)
                        //return ReputationRank.Hostile;

                        // same group - checks dependant only on our faction - skip FFA_PVP for example
                        //if (selfPlayerOwner.IsInRaidWith(targetPlayerOwner))
                        //return ReputationRank.Friendly; // return true to allow config option AllowTwoSide.Interaction.Group to work
                        // however client seems to allow mixed group parties, because in 13850 client it works like:
                        // return GetFactionReactionTo(getFactionTemplateEntry(), target);
                    }

                    // check FFA_PVP
                    if (Convert.ToBoolean(GetValue<byte>(UnitFields.Bytes2, 1) & (byte)UnitPVPStateFlags.FFAPVP)
                        && Convert.ToBoolean(target.GetValue<byte>(UnitFields.Bytes2, 1) & (byte)UnitPVPStateFlags.FFAPVP))
                        return ReputationRank.Hostile;

                    if (selfPlayerOwner != null)
                    {
                        FactionTemplateEntry targetFactionTemplateEntry = target.getFactionTemplateEntry();
                        if (targetFactionTemplateEntry != null)
                        {
                            ReputationRank repRank = selfPlayerOwner.GetReputationMgr().GetForcedRankIfAny(targetFactionTemplateEntry);
                            if (repRank != ReputationRank.None)
                                return repRank;
                            if (!selfPlayerOwner.HasFlag(UnitFields.Flags2, UnitFlags2.IgnoreReputation))
                            {
                                FactionEntry targetFactionEntry = DBCStorage.FactionStorage.LookupByKey(targetFactionTemplateEntry.faction);
                                if (targetFactionEntry != null)
                                {
                                    if (targetFactionEntry.CanHaveReputation())
                                    {
                                        // check contested flags
                                        if (Convert.ToBoolean(targetFactionTemplateEntry.factionFlags & (uint)FactionTemplateFlags.ContestedGuard)
                                            && selfPlayerOwner.HasFlag(PlayerFields.PlayerFlags, PlayerFlags.ContestedPVP))
                                            return ReputationRank.Hostile;

                                        // if faction has reputation, hostile state depends only from AtWar state
                                        if (selfPlayerOwner.GetReputationMgr().IsAtWar(targetFactionEntry))
                                            return ReputationRank.Hostile;
                                        return ReputationRank.Friendly;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // do checks dependant only on our faction
            return GetFactionReactionTo(getFactionTemplateEntry(), target);
        }
        ReputationRank GetFactionReactionTo(FactionTemplateEntry factionTemplateEntry, Unit target)
        {
            // always neutral when no template entry found
            if (factionTemplateEntry == null)
                return ReputationRank.Neutral;

            FactionTemplateEntry targetFactionTemplateEntry = target.getFactionTemplateEntry();
            if (targetFactionTemplateEntry == null)
                return ReputationRank.Neutral;

            Player targetPlayerOwner = target.GetAffectingPlayer();
            if (targetPlayerOwner != null)
            {
                // check contested flags
                if (Convert.ToBoolean(factionTemplateEntry.factionFlags & (uint)FactionTemplateFlags.ContestedGuard)
                    && targetPlayerOwner.HasFlag(PlayerFields.PlayerFlags, PlayerFlags.ContestedPVP))
                    return ReputationRank.Hostile;
                ReputationRank repRank = targetPlayerOwner.GetReputationMgr().GetForcedRankIfAny(factionTemplateEntry);
                if (repRank != ReputationRank.None)
                    return repRank;
                if (!target.HasFlag(UnitFields.Flags2, UnitFlags2.IgnoreReputation))
                {
                    FactionEntry factionEntry = DBCStorage.FactionStorage.LookupByKey(factionTemplateEntry.faction);
                    if (factionEntry != null)
                    {
                        if (factionEntry.CanHaveReputation())
                        {
                            // CvP case - check reputation, don't allow state higher than neutral when at war
                            repRank = targetPlayerOwner.GetReputationMgr().GetRank(factionEntry);
                            if (targetPlayerOwner.GetReputationMgr().IsAtWar(factionEntry))
                                repRank = (ReputationRank)Math.Min((int)ReputationRank.Neutral, (int)repRank);
                            return repRank;
                        }
                    }
                }
            }

            // common faction based check
            if (factionTemplateEntry.IsHostileTo(targetFactionTemplateEntry))
                return ReputationRank.Hostile;
            if (factionTemplateEntry.IsFriendlyTo(targetFactionTemplateEntry))
                return ReputationRank.Friendly;
            if (targetFactionTemplateEntry.IsFriendlyTo(factionTemplateEntry))
                return ReputationRank.Friendly;
            if (Convert.ToBoolean(factionTemplateEntry.factionFlags & (uint)FactionTemplateFlags.HostileByDefault))
                return ReputationRank.Hostile;
            // neutral by default
            return ReputationRank.Neutral;
        }
        public FactionTemplateEntry getFactionTemplateEntry()
        {
            FactionTemplateEntry entry = DBCStorage.FactionTemplateStorage.LookupByKey(getFaction());
            if (entry == null)
            {
                ulong guid = 0;                             // prevent repeating spam same faction problem

                if (GetGUID() != guid)
                {
                    Player player = ToPlayer();
                    Creature creature = ToCreature();
                    if (player != null)
                        Log.outError("Player {0} has invalid faction (faction template id) #{1}", player.GetName(), getFaction());
                    else if (creature != null)
                        Log.outError("Creature (template id: {0}) has invalid faction (faction template id) #{1}", creature.GetCreatureTemplate().Entry, getFaction());
                    else
                        Log.outError("Unit (name={0}, type={1}) has invalid faction (faction template id) #{2}", GetName(), GetTypeId(), getFaction());

                    guid = GetGUID();
                }
            }
            return entry;
        }
        public uint getFaction() { return GetValue<uint>(UnitFields.FactionTemplate); }
        public void SetFaction(uint faction) { SetValue<uint>(UnitFields.FactionTemplate, faction); }

        //Transport
        public float GetTransOffsetX() { return movementInfo.TransPos.X; }
        public float GetTransOffsetY() { return movementInfo.TransPos.Y; }
        public float GetTransOffsetZ() { return movementInfo.TransPos.Z; }
        public float GetTransOffsetO() { return movementInfo.TransPos.Orientation; }
        public ulong GetTransGUID()
        {
            //if (GetVehicle())
            //return GetVehicleBase().GetGUID();
            //if (GetTransport())
            //return GetTransport().GetGUID();

            return 0;
        }

        //Spells
        public void CastSpell(SpellCastTargets targets, SpellInfo spellInfo, CustomSpellValues value, TriggerCastFlags triggerFlags, Item castItem, AuraEffect triggeredByAura, ulong originalCaster)
        {
            if (spellInfo == null)
            {
                Log.outError("CastSpell: unknown spell by caster: {0} {1})", ((this is Player) ? "player (GUID:" : "creature (Entry:"), ((this is Player) ? GetGUIDLow() : GetEntry()));
                return;
            }
            Spell spell = new Spell(this, spellInfo, triggerFlags, originalCaster);

            if (value != null)
                foreach (var itr in value)
                    spell.SetSpellValue(itr.Key, itr.Value);

            spell.m_CastItem = castItem;
            spell.Prepare(targets, triggeredByAura);
        }
        void CastSpell(Unit victim, uint spellId, bool triggered, Item castItem, AuraEffect triggeredByAura, ulong originalCaster)
        {
            CastSpell(victim, spellId, triggered ? TriggerCastFlags.FullMask : TriggerCastFlags.None, castItem, triggeredByAura, originalCaster);
        }
        void CastSpell(Unit victim, uint spellId, TriggerCastFlags triggerFlags = TriggerCastFlags.None, Item castItem = null, AuraEffect triggeredByAura = null, ulong originalCaster = 0)
        {
            SpellInfo spellInfo = Cypher.SpellMgr.GetSpellInfo(spellId);
            if (spellInfo == null)
            {
                Log.outError("CastSpell: unknown spell id {0} by caster: {1} {2})", spellId, (GetTypeId() == ObjectType.Player ?
                    "player (GUID:" : "creature (Entry:"), (GetTypeId() == ObjectType.Player ? GetGUIDLow() : GetEntry()));
                return;
            }

            CastSpell(victim, spellInfo, triggerFlags, castItem, triggeredByAura, originalCaster);
        }
        void CastSpell(Unit victim, SpellInfo spellInfo, bool triggered, Item castItem = null, AuraEffect triggeredByAura = null, ulong originalCaster = 0)
        {
            CastSpell(victim, spellInfo, triggered ? TriggerCastFlags.FullMask : TriggerCastFlags.None, castItem, triggeredByAura, originalCaster);
        }
        void CastSpell(Unit victim, SpellInfo spellInfo, TriggerCastFlags triggerFlags, Item castItem, AuraEffect triggeredByAura, ulong originalCaster)
        {
            SpellCastTargets targets = new SpellCastTargets();
            targets.SetUnitTarget(victim);
            CastSpell(targets, spellInfo, null, triggerFlags, castItem, triggeredByAura, originalCaster);
        }
        public void ModSpellCastTime(SpellInfo spellProto, int castTime, Spell spell)
        {
            if (spellProto == null || castTime < 0)
                return;
            // called from caster
            //if (Player modOwner = GetSpellModOwner())
            //modOwner.ApplySpellMod(spellProto.Id, SPELLMOD_CASTING_TIME, castTime, spell);

            if (!Convert.ToBoolean(spellProto.Attributes & (SpellAttr0.Ability | SpellAttr0.Tradespell)) && ((this is Player) && spellProto.SpellFamilyName != 0) || (this is Unit))
                castTime = (int)(castTime * GetValue<float>(UnitFields.ModCastingSpeed));
            else if (Convert.ToBoolean(spellProto.Attributes & SpellAttr0.ReqAmmo) && !Convert.ToBoolean(spellProto.AttributesEx2 & SpellAttr2.AutorepeatFlag))
                castTime = (int)(castTime * 0);//m_modAttackSpeedPct[RANGED_ATTACK]);
            //else if (spellProto.SpellVisual[0] == 3881 && HasAura(67556)) // cooking with Chef Hat.
            //castTime = 500;
        }
        public float ApplyEffectModifiers(SpellInfo spellProto, uint effect_index, float value)
        {
            Player modOwner = GetSpellModOwner();
            if (modOwner != null)
            {
                /*
                modOwner.ApplySpellMod(spellProto.Id, SPELLMOD_ALL_EFFECTS, value);
                switch (effect_index)
                {
                    case 0:
                        modOwner.ApplySpellMod(spellProto.Id, SPELLMOD_EFFECT1, value);
                        break;
                    case 1:
                        modOwner.ApplySpellMod(spellProto.Id, SPELLMOD_EFFECT2, value);
                        break;
                    case 2:
                        modOwner.ApplySpellMod(spellProto.Id, SPELLMOD_EFFECT3, value);
                        break;
                }
                 */
            }
            return value;
        }//needs fixed
        List<AuraEffect> GetAuraEffectsByType(AuraType type) { return m_modAuras[(int)type]; }
        public int GetTotalAuraModifier(AuraType auratype)
        {
            Dictionary<SpellGroup, int> SameEffectSpellGroup = new Dictionary<SpellGroup, int>();
            int modifier = 0;

            var mTotalAuraList = GetAuraEffectsByType(auratype);
            foreach (var aura in mTotalAuraList)
                if (!Cypher.SpellMgr.AddSameEffectStackRuleSpellGroups(aura.GetSpellInfo(), aura.GetAmount(), out SameEffectSpellGroup))
                    modifier += aura.GetAmount();

            foreach (var itr in SameEffectSpellGroup)
                modifier += itr.Value;

            return modifier;
        }
        public uint GetMaxSkillValueForLevel(Unit target = null) { return (target != null ? GetLevelForTarget(target) : getLevel()) * 5; }
        Player GetSpellModOwner()
        {
            if (this is Player)
                return (Player)this;
            if (ToCreature().isPet() || ToCreature().isTotem())
            {
                Unit owner = GetOwner();
                if (owner != null && owner.GetTypeId() == ObjectType.Player)
                    return (Player)owner;
            }
            return null;
        }
        void _RegisterAuraEffect(AuraEffect aurEff, bool apply)
        {
            //if (apply)
            //m_modAuras[aurEff.GetAuraType()].Add(aurEff);
            //else
            //m_modAuras[aurEff.GetAuraType()].Remove(aurEff);
        }

        //Stats
        public void SetCreateStat(Stats stat, float val) { CreateStats[(int)stat] = val; }
        public void SetStat(Stats stat, int val) { SetValue<int>(UnitFields.Stats + (int)stat, val); }
        public void SetCreateHealth(uint val) { SetValue<uint>(UnitFields.BaseHealth, val); }
        public uint GetCreateHealth() { return GetValue<uint>(UnitFields.BaseHealth); }
        public void SetCreateMana(uint val) { SetValue<uint>(UnitFields.BaseMana, val); }
        public uint GetCreateMana() { return GetValue<uint>(UnitFields.BaseMana); }
        public uint GetArmor() { return GetResistance(SpellSchools.Normal); }
        public void SetArmor(int val) { SetResistance(SpellSchools.Normal, val); }
        public uint GetResistance(SpellSchools school) { return GetValue<uint>(UnitFields.Resistances + (int)school); }
        public void SetResistance(SpellSchools school, int val) { SetValue<int>(UnitFields.Resistances + (int)school, val); }
        public float GetCreateStat(Stats stat) { return CreateStats[(int)stat]; }
        public void InitStatBuffMods()
        {
            for (var i = Stats.Strength; i < Stats.Max; ++i)
            {
                SetValue<float>(UnitFields.StatPosBuff + (int)i, 0);
                SetValue<float>(UnitFields.StatNegBuff + (int)i, 0);
            }
        }
        public float GetResistanceBuffMods(SpellSchools school, bool positive)
        {
            return GetValue<float>((positive ? UnitFields.ResistanceBuffModsPositive : UnitFields.ResistanceBuffModsNegative) + (int)school);
        }
        public void SetResistanceBuffMods(SpellSchools school, bool positive, float val)
        {
            SetValue<float>((positive ? UnitFields.ResistanceBuffModsPositive : UnitFields.ResistanceBuffModsNegative) + (int)school, val);
        }
        public void SetHealth(uint val)
        {
            if (getDeathState() == DeathState.JustDied)
                val = 0;
            else if (GetTypeId() == ObjectType.Player && getDeathState() == DeathState.Dead)
                val = 1;
            else
            {
                uint maxHealth = GetMaxHealth();
                if (maxHealth < val)
                    val = maxHealth;
            }

            SetValue<uint>(UnitFields.Health, val);

            // group update
            //if (Player * player = ToPlayer())
            {
                //if (player.GetGroup())
                //player.SetGroupUpdateFlag(GROUP_UPDATE_FLAG_CUR_HP);
            }
            //else if (Pet * pet = ToCreature().ToPet())
            {
                // if (pet.isControlled())
                {
                    // Unit* owner = GetOwner();
                    // if (owner && (owner.GetTypeId() == TYPEID_PLAYER) && owner.ToPlayer().GetGroup())
                    // owner.ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_PET_CUR_HP);
                }
            }
        }
        public void SetMaxHealth(uint val)
        {
            if (val == 0)
                val = 1;

            uint health = GetHealth();
            SetValue<uint>(UnitFields.MaxHealth, val);

            // group update
            //if (GetTypeId() == TYPEID_PLAYER)
            {
                //if (ToPlayer().GetGroup())
                //ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_MAX_HP);
            }
            //else if (Pet * pet = ToCreature().ToPet())
            {
                //if (pet.isControlled())
                {
                    //Unit* owner = GetOwner();
                    //if (owner && (owner.GetTypeId() == TYPEID_PLAYER) && owner.ToPlayer().GetGroup())
                    //owner.ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_PET_MAX_HP);
                }
            }

            if (val < health)
                SetHealth(val);
        }
        public void SetFullHealth() { SetHealth(GetMaxHealth()); }
        public uint GetHealth() { return GetValue<uint>(UnitFields.Health); }
        public uint GetMaxHealth() { return GetValue<uint>(UnitFields.MaxHealth); }

        //Powers
        public Powers getPowerType() { return (Powers)GetValue<byte>(UnitFields.Bytes, 3); }
        public void setPowerType(Powers new_powertype)
        {
            SetValue<byte>(UnitFields.Bytes, (byte)new_powertype, 3);

            if (GetTypeId() == ObjectType.Player)
            {
                //if (ToPlayer().GetGroup())
                //ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_POWER_TYPE);
            }
            else if (GetTypeId() == ObjectType.Unit)
            {
                //Pet pet = ToCreature().ToPet();
                //if (pet.isControlled())
                {
                    //Unit* owner = GetOwner();
                    //if (owner && (owner.GetTypeId() == TYPEID_PLAYER) && owner.ToPlayer().GetGroup())
                    //owner.ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_PET_POWER_TYPE);
                }
            }

            switch (new_powertype)
            {
                default:
                case Powers.Mana:
                    break;
                case Powers.Rage:
                    SetMaxPower(Powers.Rage, GetCreatePowers(Powers.Rage));
                    SetPower(Powers.Rage, 0);
                    break;
                case Powers.Focus:
                    SetMaxPower(Powers.Focus, GetCreatePowers(Powers.Focus));
                    SetPower(Powers.Focus, GetCreatePowers(Powers.Focus));
                    break;
                case Powers.Energy:
                    SetMaxPower(Powers.Energy, GetCreatePowers(Powers.Energy));
                    break;
            }
        }
        public void SetMaxPower(Powers power, int val)
        {
            int powerIndex = Cypher.ObjMgr.GetPowerIndexByClass((uint)power, getClass());
            if (powerIndex == (int)Powers.Max)
                return;

            int cur_power = GetPower(power);
            SetValue<int>(UnitFields.MaxPower + powerIndex, val);

            // group update
            if (GetTypeId() == ObjectType.Player)
            {
                //if (ToPlayer().GetGroup())
                //ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_MAX_POWER);
            }
            //Pet pet = ToCreature().ToPet();
            //if (pet != null)
            {
                //if (pet.isControlled())
                {
                    //Unit* owner = GetOwner();
                    //if (owner && (owner.GetTypeId() == TYPEID_PLAYER) && owner.ToPlayer().GetGroup())
                    // owner.ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_PET_MAX_POWER);
                }
            }

            if (val < cur_power)
                SetPower(power, val);
        }
        public void SetPower(Powers power, int val)
        {
            int powerIndex = Cypher.ObjMgr.GetPowerIndexByClass((uint)power, getClass());
            if (powerIndex == (int)Powers.Max)
                return;

            int maxPower = (int)GetMaxPower(power);
            if (maxPower < val)
                val = maxPower;

            SetValue<int>(UnitFields.Power + powerIndex, val);

            if (IsInWorld)
            {
                PacketWriter data = new PacketWriter(Opcodes.SMSG_PowerUpdate);
                data.WritePackedGuid(GetPackGUID());
                data.WriteUInt32(1); //power count
                data.WriteUInt8(powerIndex);
                data.WriteInt32(val);
                SendMessageToSet(data, GetTypeId() == ObjectType.Player ? true : false);
            }

            // group update
            //if (Player* player = ToPlayer())
            {
                //if (player.GetGroup())
                //player.SetGroupUpdateFlag(GROUP_UPDATE_FLAG_CUR_POWER);
            }
            //else if (Pet* pet = ToCreature().ToPet())
            {
                //if (pet.isControlled())
                {
                    //Unit* owner = GetOwner();
                    //if (owner && (owner.GetTypeId() == TYPEID_PLAYER) && owner.ToPlayer().GetGroup())
                    //owner.ToPlayer().SetGroupUpdateFlag(GROUP_UPDATE_FLAG_PET_CUR_POWER);
                }
            }
        }
        public int GetPower(Powers power)
        {
            int powerIndex = Cypher.ObjMgr.GetPowerIndexByClass((uint)power, getClass());
            if (powerIndex == (int)Powers.Max)
                return 0;

            return GetValue<int>(UnitFields.Power + powerIndex);
        }
        public int GetMaxPower(Powers power)
        {
            int powerIndex = Cypher.ObjMgr.GetPowerIndexByClass((uint)power, getClass());
            if (powerIndex == (int)Powers.Max)
                return 0;

            return GetValue<int>(UnitFields.MaxPower + powerIndex);
        }
        public int GetCreatePowers(Powers power)
        {
            switch (power)
            {
                case Powers.Mana:
                    return (int)GetCreateMana();
                case Powers.Rage:
                    return 1000;
                case Powers.Focus:
                    if (GetTypeId() == ObjectType.Player && getClass() == (byte)Class.Hunter)
                        return 100;
                    //return (GetTypeId() == ObjectType.Player || !((this as Creature).isPet() || ((this as Pet).getPetType() != HUNTER_PET ? 0 : 100);
                    break;
                case Powers.Energy:
                    return 100;
                case Powers.RunicPower:
                    return 1000;
                case Powers.Runes:
                    return 0;
                case Powers.SoulShards:
                    return 3;
                case Powers.Eclipse:
                    return 0;
                case Powers.HolyPower:
                    return 0;
                case Powers.Health:
                    return 0;
                default:
                    break;
            }

            return 0;
        }

        //Combat
        public DeathState getDeathState() { return m_deathState; }
        public bool isInCombat() { return HasFlag(UnitFields.Flags, UnitFlags.InCombat); }
        public bool Attack(Unit victim, bool meleeAttack)
        {
            if (victim == null || victim == this)
                return false;

            // dead units can neither attack nor be attacked
            if (!isAlive() || !victim.IsInWorld || !victim.isAlive())
                return false;

            // player cannot attack in mount state
            //if (GetTypeId() == ObjectType.Player && IsMounted())
            //return false;

            // nobody can attack GM in GM-mode
            if (victim.GetTypeId() == ObjectType.Player)
            {
                if (victim.ToPlayer().isGameMaster())
                    return false;
            }
            else
            {
                //if (victim.ToCreature().IsInEvadeMode())
                //return false;
            }

            // remove SPELL_AURA_MOD_UNATTACKABLE at attack (in case non-interruptible spells stun aura applied also that not let attack)
            //if (HasAuraType(SPELL_AURA_MOD_UNATTACKABLE))
            //RemoveAurasByType(SPELL_AURA_MOD_UNATTACKABLE);

            if (m_attacking != null)
            {
                if (m_attacking == victim)
                {
                    // switch to melee attack from ranged/magic
                    if (meleeAttack)
                    {
                        if (!HasUnitState(UnitState.Melee_Attacking))
                        {
                            AddUnitState(UnitState.Melee_Attacking);
                            SendMeleeAttackStart(victim);
                            return true;
                        }
                    }
                    else if (HasUnitState(UnitState.Melee_Attacking))
                    {
                        ClearUnitState(UnitState.Melee_Attacking);
                        SendMeleeAttackStop(victim);
                        return true;
                    }
                    return false;
                }

                // switch target
                //InterruptSpell(CURRENT_MELEE_SPELL);
                //if (!meleeAttack)
                //ClearUnitState(UNIT_STATE_MELEE_ATTACKING);
            }

            if (m_attacking != null)
                m_attacking._removeAttacker(this);

            m_attacking = victim;
            m_attacking._addAttacker(this);

            // Set our target
            SetTarget(victim.GetGUID());

            if (meleeAttack)
                AddUnitState(UnitState.Melee_Attacking);

            // set position before any AI calls/assistance
            //if (GetTypeId() == TYPEID_UNIT)
            //    ToCreature().SetCombatStartPosition(GetPositionX(), GetPositionY(), GetPositionZ());

            if (GetTypeId() == ObjectType.Unit && !ToCreature().isPet())
            {
                // should not let player enter combat by right clicking target - doesn't helps
                //SetInCombatWith(victim);
                //if (victim.GetTypeId() == TYPEID_PLAYER)
                //victim.SetInCombatWith(this);
                //AddThreat(victim, 0.0f);

                //ToCreature().SendAIReaction(AI_REACTION_HOSTILE);
                //ToCreature().CallAssistance();
            }

            // delay offhand weapon attack to next attack time
            //if (haveOffhandWeapon())
            //resetAttackTimer(OFF_ATTACK);

            if (meleeAttack)
                SendMeleeAttackStart(victim);

            // Let the pet know we've started attacking someting. Handles melee attacks only
            // Spells such as auto-shot and others handled in WorldSession::HandleCastSpellOpcode
            if (this.GetTypeId() == ObjectType.Player)
            {
                //Pet playerPet = this.ToPlayer().GetPet();

                //if (playerPet && playerPet.isAlive())
                //playerPet.AI().OwnerAttacked(victim);
            }

            return true;
        }
        public void SendMeleeAttackStart(Unit victim)
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_Attackstart);

            data.WriteUInt64(GetGUID());
            data.WriteUInt64(victim.GetGUID());
            SendMessageToSet(data, true);
            Log.outDebug("WORLD: Sent SMSG_ATTACKSTART");
        }
        void SendMeleeAttackStop(Unit victim)
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_Attackstop);
            data.WritePackedGuid(GetPackGUID());
            data.WritePackedGuid(victim != null ? victim.GetPackGUID() : 0);
            data.WriteUInt32(0);                                     //! Can also take the value 0x01, which seems related to updating rotation
            SendMessageToSet(data, true);
            Log.outDebug("WORLD: Sent SMSG_ATTACKSTOP");

            if (victim != null)
                Log.outInfo("{0} {1} stopped attacking {2} {3}", (GetTypeId() == ObjectType.Player ? "Player" : "Creature"), GetGUIDLow(),
                    (victim.GetTypeId() == ObjectType.Player ? "player" : "creature"), victim.GetGUIDLow());
            else
                Log.outInfo("{0} {1} stopped attacking", (GetTypeId() == ObjectType.Player ? "Player" : "Creature"), GetGUIDLow());
        }
        void SetTarget(ulong guid)
        {
            //if (!_focusSpell)
            SetValue<ulong>(UnitFields.Target, guid);
        }
        public bool AttackStop()
        {
            if (m_attacking == null)
                return false;

            Unit victim = m_attacking;

            m_attacking._removeAttacker(this);
            m_attacking = null;

            // Clear our target
            SetTarget(0);

            ClearUnitState(UnitState.Melee_Attacking);

            //InterruptSpell(CURRENT_MELEE_SPELL);

            // reset only at real combat stop
            Creature creature = ToCreature();
            if (creature != null)
            {
                //creature.SetNoCallAssistance(false);

                //if (creature.HasSearchedAssistance())
                {
                    //creature.SetNoSearchAssistance(false);
                    //UpdateSpeed(MOVE_RUN, false);
                }
            }

            SendMeleeAttackStop(victim);
            return true;
        }
        void _addAttacker(Unit pAttacker) { m_attackers.Add(pAttacker); }
        void _removeAttacker(Unit pAttacker) { m_attackers.Remove(pAttacker); }
        public Unit getVictim() { return m_attacking; }
        public Unit getAttackerForHelper()
        {
            if (getVictim() != null)
                return getVictim();

            if (m_attackers.Count != 0)
                return m_attackers[0];

            return null;
        }
        List<Unit> getAttackers() { return m_attackers; }
        public float GetCombatReach() { return GetValue<float>(UnitFields.CombatReach); }
        public bool haveOffhandWeapon()
        {
            if (GetTypeId() == ObjectType.Player)
                return ToPlayer().GetWeaponForAttack(WeaponAttackType.OffAttack, true) != null;
            else
                return m_canDualWield;
        }
        public void resetAttackTimer(WeaponAttackType type = WeaponAttackType.BaseAttack)
        {
            m_attackTimer[(int)type] = (uint)(GetAttackTime(type) * m_modAttackSpeedPct[(int)type]);
        }
        public void setAttackTimer(WeaponAttackType type, uint time) { m_attackTimer[(int)type] = time; }
        public uint getAttackTimer(WeaponAttackType type) { return m_attackTimer[(int)type]; }
        public bool isAttackReady(WeaponAttackType type = WeaponAttackType.BaseAttack) { return m_attackTimer[(int)type] == 0; }
        uint GetAttackTime(WeaponAttackType att)
        {
            float f_BaseAttackTime = GetValue<float>(UnitFields.AttackRoundBaseTime + (int)att) / m_modAttackSpeedPct[(int)att];
            return (uint)f_BaseAttackTime;
        }
        public void AttackerStateUpdate(Unit victim, WeaponAttackType attType = WeaponAttackType.BaseAttack, bool extra = false)
        {
            if (HasUnitState(UnitState.Cannot_Autoattack) || HasFlag(UnitFields.Flags, UnitFlags.Pacified))
                return;

            if (!victim.isAlive())
                return;

            if ((attType == WeaponAttackType.BaseAttack || attType == WeaponAttackType.OffAttack) && !IsWithinLOSInMap(victim))
                return;

            CombatStart(victim);
            //RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_MELEE_ATTACK);

            if (attType != WeaponAttackType.BaseAttack && attType != WeaponAttackType.OffAttack)
                return;                                             // ignore ranged case

            // melee attack spell casted at main hand attack only - no normal melee dmg dealt
            //if (attType == WeaponAttackType.BaseAttack && m_currentSpells[CURRENT_MELEE_SPELL] && !extra)
                //m_currentSpells[CURRENT_MELEE_SPELL].cast();
            //else
            {
                // attack can be redirected to another target
                //victim = GetMeleeHitRedirectTarget(victim);

                CalcDamageInfo damageInfo;
                CalculateMeleeDamage(victim, 0, out damageInfo, attType);
                // Send log damage message to client
                //DealDamageMods(victim, damageInfo.damage, &damageInfo.absorb);
                SendAttackStateUpdate(damageInfo);

                //TriggerAurasProcOnEvent(damageInfo);
                //ProcDamageAndSpell(damageInfo.target, damageInfo.procAttacker, damageInfo.procVictim, damageInfo.procEx, damageInfo.damage, damageInfo.attackType);

                //DealMeleeDamage(&damageInfo, true);

                if (GetTypeId() == ObjectType.Player)
                    Log.outDebug("AttackerStateUpdate: (Player) {0} attacked {1} (TypeId: {2}) for {3} dmg, absorbed {4}, blocked {5}, resisted {6}.",
                        GetGUIDLow(), victim.GetGUIDLow(), victim.GetTypeId(), damageInfo.damage, damageInfo.absorb, damageInfo.blocked_amount, damageInfo.resist);
                else
                    Log.outDebug("AttackerStateUpdate: (NPC) {0} attacked {1} (TypeId: {2}) for {3} dmg, absorbed {4}, blocked {5}, resisted {6}.",
                        GetGUIDLow(), victim.GetGUIDLow(), victim.GetTypeId(), damageInfo.damage, damageInfo.absorb, damageInfo.blocked_amount, damageInfo.resist);
            }
        }
        void SendAttackStateUpdate(CalcDamageInfo damageInfo)
        {
            Log.outDebug("WORLD: Sending SMSG_ATTACKERSTATEUPDATE");

            uint count = 1;
            //size_t maxsize = 4+5+5+4+4+1+4+4+4+4+4+1+4+4+4+4+4*12;
            PacketWriter data = new PacketWriter(Opcodes.SMSG_Attackerstateupdate);
            data.WriteUInt32(damageInfo.HitInfo);
            data.WriteUInt64(damageInfo.attacker.GetPackGUID());
            data.WriteUInt64(damageInfo.target.GetPackGUID());
            data.WriteUInt32(damageInfo.damage);                     // Full damage
            int overkill = (int)(damageInfo.damage - damageInfo.target.GetHealth());
            data.WriteUInt32(overkill < 0 ? 0 : overkill);            // Overkill
            data.WriteUInt8(count);                                   // Sub damage count

            for (uint i = 0; i < count; ++i)
            {
                data.WriteUInt32(damageInfo.damageSchoolMask);       // School of sub damage
                data.WriteFloat(damageInfo.damage);                  // sub damage
                data.WriteUInt32(damageInfo.damage);                 // Sub Damage
            }

            if (Convert.ToBoolean(damageInfo.HitInfo & (HitInfo.FULL_ABSORB | HitInfo.PARTIAL_ABSORB)))
            {
                for (uint i = 0; i < count; ++i)
                    data.WriteUInt32(damageInfo.absorb);             // Absorb
            }

            if (Convert.ToBoolean(damageInfo.HitInfo & (HitInfo.FULL_RESIST | HitInfo.PARTIAL_RESIST)))
            {
                for (uint i = 0; i < count; ++i)
                    data.WriteUInt32(damageInfo.resist);             // Resist
            }

            data.WriteUInt8(damageInfo.TargetState);
            data.WriteUInt32(0);  // Unknown attackerstate
            data.WriteUInt32(0);  // Melee spellid

            if (Convert.ToBoolean(damageInfo.HitInfo & HitInfo.BLOCK))
                data.WriteUInt32(damageInfo.blocked_amount);

            if (Convert.ToBoolean(damageInfo.HitInfo & HitInfo.RAGE_GAIN))
                data.WriteUInt32(0);

            //! Probably used for debugging purposes, as it is not known to appear on retail servers
            if (Convert.ToBoolean(damageInfo.HitInfo & HitInfo.UNK1))
            {
                data.WriteUInt32(0);
                data.WriteFloat(0);
                data.WriteFloat(0);
                data.WriteFloat(0);
                data.WriteFloat(0);
                data.WriteFloat(0);
                data.WriteFloat(0);
                data.WriteFloat(0);
                data.WriteFloat(0);
                for (uint i = 0; i < 2; ++i)
                {
                    data.WriteFloat(0);
                    data.WriteFloat(0);
                }
                data.WriteUInt32(0);
            }

            SendMessageToSet(data, true);
        }
        void CombatStart(Unit target, bool initialAggro = true)
        {
            if (initialAggro)
            {
                //if (!target.IsStandState())
                //target.SetStandState(UNIT_STAND_STATE_STAND);

                if (!target.isInCombat() && target.GetTypeId() != ObjectType.Player
                    && !target.ToCreature().HasReactState(ReactStates.PASSIVE) && target.ToCreature().IsAIEnabled)
                {
                    //if (target.isPet())
                    //  target.ToCreature().AI().AttackedBy(this); // PetAI has special handler before AttackStart()
                    //else
                    target.ToCreature().AI().AttackStart(this);
                }

                SetInCombatWith(target);
                target.SetInCombatWith(this);
            }
            //Unit who = target.GetCharmerOrOwnerOrSelf();
            //if (who.GetTypeId() == ObjectType.Player)
            //SetContestedPvP(who.ToPlayer());

            Player me = GetCharmerOrOwnerPlayerOrPlayerItself();
            //if (me && who.IsPvP()
            //&& (who.GetTypeId() !=  TYPEID_PLAYER
            //|| !me.duel || me.duel.opponent != who))
            //{
            //me.UpdatePvP(true);
            // me.RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_ENTER_PVP_COMBAT);
            //}
        }
        void SetInCombatWith(Unit enemy)
        {
            Unit eOwner = enemy.GetCharmerOrOwnerOrSelf();
            //if (eOwner.IsPvP())
            {
                // SetInCombatState(true, enemy);
                //return;
            }

            // check for duel
            if (eOwner.GetTypeId() == ObjectType.Player)// && eOwner.ToPlayer().duel)
            {
                //Unit myOwner = GetCharmerOrOwnerOrSelf();
                //if (((Player)eOwner).duel.opponent == myOwner)
                {
                    //SetInCombatState(true, enemy);
                    //return;
                }
            }
            SetInCombatState(false, enemy);
        }
        void SetInCombatState(bool PvP, Unit enemy)
        {
            // only alive units can be in combat
            if (!isAlive())
                return;

            //if (PvP)
            //m_CombatTimer = 5000;

            if (isInCombat() || HasUnitState(UnitState.Evade))
                return;

            SetFlag(UnitFields.Flags, UnitFlags.InCombat);

            Creature creature = ToCreature();
            if (creature != null)
            {
                // Set home position at place of engaging combat for escorted creatures
                //if ((IsAIEnabled && creature.AI().IsEscorted()) ||
                // GetMotionMaster().GetCurrentMovementGeneratorType() == WAYPOINT_MOTION_TYPE ||
                //GetMotionMaster().GetCurrentMovementGeneratorType() == POINT_MOTION_TYPE)
                //creature.SetHomePosition(GetPositionX(), GetPositionY(), GetPositionZ(), GetOrientation());

                if (enemy != null)
                {
                    if (IsAIEnabled)
                    {
                        //creature.AI().EnterCombat(enemy);
                        RemoveFlag(UnitFields.Flags, UnitFlags.ImmuneToPc); // unit has engaged in combat, remove immunity so players can fight back
                    }
                    //if (creature.GetFormation())
                    //creature.GetFormation().MemberAttackStart(creature, enemy);
                }

                if (isPet())
                {
                    UpdateSpeed(UnitMoveType.Run, true);
                    UpdateSpeed(UnitMoveType.Swim, true);
                    UpdateSpeed(UnitMoveType.Flight, true);
                }

                //if (!Convert.ToBoolean(creature.GetCreatureTemplate().TypeFlags & CreatureTypeFlags.CREATURE_TYPEFLAGS_MOUNTED_COMBAT))
                //Dismount();
            }

            //for (Unit::ControlList::iterator itr = m_Controlled.begin(); itr != m_Controlled.end(); ++itr)
            {
                //(*itr).SetInCombatState(PvP, enemy);
                //(*itr).SetFlag(UNIT_FIELD_FLAGS, UNIT_FLAG_PET_IN_COMBAT);
            }
        }
        // TODO for melee need create structure as in
        void CalculateMeleeDamage(Unit victim, uint damage, out CalcDamageInfo damageInfo, WeaponAttackType attackType)
        {
            damageInfo = new CalcDamageInfo();

            damageInfo.attacker = this;
            damageInfo.target = victim;
            damageInfo.damageSchoolMask = 0;//GetMeleeDamageSchoolMask();
            damageInfo.attackType = attackType;
            damageInfo.damage = 0;
            damageInfo.cleanDamage = 0;
            damageInfo.absorb = 0;
            damageInfo.resist = 0;
            damageInfo.blocked_amount = 0;

            damageInfo.TargetState = 0;
            damageInfo.HitInfo = 0;
            damageInfo.procAttacker = ProcFlags.NONE;
            damageInfo.procVictim = ProcFlags.NONE;
            damageInfo.procEx = ProcFlagsExLegacy.NONE;
            //damageInfo.hitOutCome       = MELEE_HIT_EVADE;

            if (victim == null)
                return;

            if (!isAlive() || !victim.isAlive())
                return;

            // Select HitInfo/procAttacker/procVictim flag based on attack type
            switch (attackType)
            {
                case WeaponAttackType.BaseAttack:
                    damageInfo.procAttacker = ProcFlags.DONE_MELEE_AUTO_ATTACK | ProcFlags.DONE_MAINHAND_ATTACK;
                    damageInfo.procVictim = ProcFlags.TAKEN_MELEE_AUTO_ATTACK;
                    break;
                case WeaponAttackType.OffAttack:
                    damageInfo.procAttacker = ProcFlags.DONE_MELEE_AUTO_ATTACK | ProcFlags.DONE_OFFHAND_ATTACK;
                    damageInfo.procVictim = ProcFlags.TAKEN_MELEE_AUTO_ATTACK;
                    damageInfo.HitInfo = HitInfo.OFFHAND;
                    break;
                default:
                    return;
            }

            // Physical Immune check
            //if (damageInfo.target.IsImmunedToDamage(SpellSchoolMask(damageInfo.damageSchoolMask)))
            {
                //damageInfo.HitInfo |= HitInfo.NORMALSWING;
                //damageInfo.TargetState = VictimState.IS_IMMUNE;

                //damageInfo.procEx |= ProcFlagsExLegacy.IMMUNE;
                //damageInfo.damage = 0;
                //damageInfo.cleanDamage = 0;
                //return;
            }

            damage += CalculateDamage(damageInfo.attackType, false, true);
            // Add melee damage bonus
            //damage = MeleeDamageBonusDone(damageInfo.target, damage, damageInfo.attackType);
            //damage = damageInfo.target.MeleeDamageBonusTaken(this, damage, damageInfo.attackType);

            // Script Hook For CalculateMeleeDamage -- Allow scripts to change the Damage pre class mitigation calculations
            //sScriptMgr.ModifyMeleeDamage(damageInfo.target, damageInfo.attacker, damage);

            // Calculate armor reduction
            //if (IsDamageReducedByArmor((SpellSchoolMask)(damageInfo.damageSchoolMask)))
            {
                //damageInfo.damage = CalcArmorReducedDamage(damageInfo.target, damage, NULL, damageInfo.attackType);
                //damageInfo.cleanDamage += damage - damageInfo.damage;
            }
            //else
                //damageInfo.damage = damage;
            bool isCrit = false;
            //damageInfo.TargetState = RollMeleeOutcomeAgainst(damageInfo.target, damageInfo.attackType, out isCrit);

            switch (damageInfo.TargetState)
            {
                case VictimState.EVADES:
                    damageInfo.HitInfo |= HitInfo.MISS | HitInfo.SWINGNOHITSOUND;
                    damageInfo.procEx |= ProcFlagsExLegacy.EVADE;
                    damageInfo.damage = 0;
                    damageInfo.cleanDamage = 0;
                    return;
                case VictimState.INTACT:
                    damageInfo.HitInfo |= HitInfo.MISS;
                    damageInfo.procEx |= ProcFlagsExLegacy.MISS;
                    damageInfo.damage = 0;
                    damageInfo.cleanDamage = 0;
                    break;
                case VictimState.HIT:
                    if (isCrit)
                    {
                        damageInfo.HitInfo |= HitInfo.CRITICALHIT;

                        damageInfo.procEx |= ProcFlagsExLegacy.CRITICAL_HIT;
                        // Crit bonus calc
                        damageInfo.damage += damageInfo.damage;
                        float mod = 0.0f;
                        // Apply SPELL_AURA_MOD_ATTACKER_RANGED_CRIT_DAMAGE or SPELL_AURA_MOD_ATTACKER_MELEE_CRIT_DAMAGE
                        //if (damageInfo.attackType == RANGED_ATTACK)
                            //mod += damageInfo.target.GetTotalAuraModifier(SPELL_AURA_MOD_ATTACKER_RANGED_CRIT_DAMAGE);
                        //else
                            //mod += damageInfo.target.GetTotalAuraModifier(SPELL_AURA_MOD_ATTACKER_MELEE_CRIT_DAMAGE);

                        // Increase crit damage from SPELL_AURA_MOD_CRIT_DAMAGE_BONUS
                        //mod += (GetTotalAuraMultiplierByMiscMask(SPELL_AURA_MOD_CRIT_DAMAGE_BONUS, damageInfo.damageSchoolMask) - 1.0f) * 100;

                        //if (mod != 0)
                            //AddPct(damageInfo.damage, mod);
                    }
                    else
                    {
                        damageInfo.procEx |= ProcFlagsExLegacy.NORMAL_HIT;
                    }
                    break;
                case VictimState.PARRY:
                    damageInfo.procEx |= ProcFlagsExLegacy.PARRY;
                    damageInfo.cleanDamage += damageInfo.damage;
                    damageInfo.damage = 0;
                    break;
                case VictimState.DODGE:
                    damageInfo.procEx |= ProcFlagsExLegacy.DODGE;
                    damageInfo.cleanDamage += damageInfo.damage;
                    damageInfo.damage = 0;
                    break;
                case VictimState.BLOCKS:
                    //damageInfo.TargetState = VICTIMSTATE_HIT;
                    damageInfo.procEx |= ProcFlagsExLegacy.BLOCK | ProcFlagsExLegacy.NORMAL_HIT;
                    // 30% damage blocked, double blocked amount if block is critical
                    //damageInfo.blocked_amount = CalculatePct(damageInfo.damage, damageInfo.target.isBlockCritical() ? damageInfo.target.GetBlockPercent() * 2 : damageInfo.target.GetBlockPercent());
                    damageInfo.damage -= damageInfo.blocked_amount;
                    damageInfo.cleanDamage += damageInfo.blocked_amount;
                    break;
                /*
            case MELEE_HIT_GLANCING:
            {
                damageInfo.HitInfo     |= HITINFO_GLANCING;
                damageInfo.TargetState  = VICTIMSTATE_HIT;
                damageInfo.procEx      |= PROC_EX_NORMAL_HIT;
                int32 leveldif = int32(victim.getLevel()) - int32(getLevel());
                if (leveldif > 3)
                    leveldif = 3;
                float reducePercent = 1 - leveldif * 0.1f;
                damageInfo.cleanDamage += damageInfo.damage - uint32(reducePercent * damageInfo.damage);
                damageInfo.damage = uint32(reducePercent * damageInfo.damage);
                break;
            }
            case MELEE_HIT_CRUSHING:
                damageInfo.HitInfo     |= HITINFO_CRUSHING;
                damageInfo.TargetState  = VICTIMSTATE_HIT;
                damageInfo.procEx      |= PROC_EX_NORMAL_HIT;
                // 150% normal damage
                damageInfo.damage += (damageInfo.damage / 2);
                break;
                */
                default:
                    break;
            }

            // Always apply HITINFO_AFFECTS_VICTIM in case its not a miss
            if (!Convert.ToBoolean(damageInfo.HitInfo & HitInfo.MISS))
                damageInfo.HitInfo |= HitInfo.AFFECTS_VICTIM;

            uint resilienceReduction = damageInfo.damage;
            //ApplyResilience(victim, &resilienceReduction, damageInfo.hitOutCome == MELEE_HIT_CRIT);
            resilienceReduction = damageInfo.damage - resilienceReduction;
            damageInfo.damage -= resilienceReduction;
            damageInfo.cleanDamage += resilienceReduction;

            // Calculate absorb resist
            if ((int)damageInfo.damage > 0)
            {
                damageInfo.procVictim |= ProcFlags.TAKEN_DAMAGE;
                // Calculate absorb & resists
                //CalcAbsorbResist(damageInfo.target, SpellSchoolMask(damageInfo.damageSchoolMask), DIRECT_DAMAGE, damageInfo.damage, &damageInfo.absorb, &damageInfo.resist);

                if (damageInfo.absorb != 0)
                {
                    damageInfo.HitInfo |= (damageInfo.damage - damageInfo.absorb == 0 ? HitInfo.FULL_ABSORB : HitInfo.PARTIAL_ABSORB);
                    damageInfo.procEx |= ProcFlagsExLegacy.ABSORB;
                }

                if (damageInfo.resist != 0)
                    damageInfo.HitInfo |= (damageInfo.damage - damageInfo.resist == 0 ? HitInfo.FULL_RESIST : HitInfo.PARTIAL_RESIST);

                damageInfo.damage -= damageInfo.absorb + damageInfo.resist;
            }
            else // Impossible get negative result but....
                damageInfo.damage = 0;
        }
        uint CalculateDamage(WeaponAttackType attType, bool normalized, bool addTotalPct)
        {
            float min_damage, max_damage;

            if (GetTypeId() == ObjectType.Player && (normalized || !addTotalPct))
                ToPlayer().CalculateMinMaxDamage(attType, normalized, addTotalPct, out min_damage, out max_damage);
            else
            {
                switch (attType)
                {
                    case WeaponAttackType.RangedAttack:
                        min_damage = GetValue<float>(UnitFields.MinRangedDamage);
                        max_damage = GetValue<float>(UnitFields.MaxRangedDamage);
                        break;
                    case WeaponAttackType.BaseAttack:
                        min_damage = GetValue<float>(UnitFields.MinDamage);
                        max_damage = GetValue<float>(UnitFields.MaxDamage);
                        break;
                    case WeaponAttackType.OffAttack:
                        min_damage = GetValue<float>(UnitFields.MinOffHandDamage);
                        max_damage = GetValue<float>(UnitFields.MaxOffHandDamage);
                        break;
                    // Just for good manner
                    default:
                        min_damage = 0.0f;
                        max_damage = 0.0f;
                        break;
                }
            }

            if (min_damage > max_damage)
            {
                min_damage = min_damage + max_damage;
                max_damage = min_damage - max_damage;
                min_damage = min_damage - max_damage;
            }

            if (max_damage == 0.0f)
                max_damage = 5.0f;

            return RandomHelper.urand((int)min_damage, (int)max_damage);
        }
        public float GetWeaponDamageRange(WeaponAttackType attType, WeaponDamageRange type)
        {
            if (attType == WeaponAttackType.OffAttack && !haveOffhandWeapon())
                return 0.0f;

            return m_weaponDamage[(int)attType, (int)type];
        }
        public float GetAPMultiplier(WeaponAttackType attType, bool normalized)
        {
            if (!normalized || GetTypeId() != ObjectType.Player)
                return (float)(GetAttackTime(attType)) / 1000.0f;

            Item Weapon = ToPlayer().GetWeaponForAttack(attType, true);
            if (Weapon == null)
                return 2.4f;                                         // fist attack

            switch (Weapon.GetTemplate().inventoryType)
            {
                case InventoryType.Weapon2Hand:
                    return 3.3f;
                case InventoryType.Ranged:
                case InventoryType.RangedRight:
                case InventoryType.Thrown:
                    return 2.8f;
                case InventoryType.Weapon:
                case InventoryType.WeaponMainhand:
                case InventoryType.WeaponOffhand:
                default:
                    return Weapon.GetTemplate().SubClass == (uint)ItemSubClassWeapon.Dagger ? 1.7f : 2.4f;
            }
        }
        public float GetTotalAttackPowerValue(WeaponAttackType attType)
        {
            if (attType == WeaponAttackType.RangedAttack)
            {
                int ap = GetValue<int>(UnitFields.RangedAttackPower);
                if (ap < 0)
                    return 0.0f;
                return ap * (1.0f + GetValue<float>(UnitFields.RangedAttackPowerMultiplier));
            }
            else
            {
                int ap = GetValue<int>(UnitFields.AttackPower);
                if (ap < 0)
                    return 0.0f;
                return ap * (1.0f + GetValue<float>(UnitFields.AttackPowerMultiplier));
            }
        }
        public float GetModifierValue(UnitMods unitMod, UnitModifierType modifierType)
        {
            if (unitMod >= UnitMods.END || modifierType >= UnitModifierType.MODIFIER_TYPE_END)
            {
                Log.outError("attempt to access non-existing modifier value from UnitMods!");
                return 0.0f;
            }

            //if (modifierType == UnitModifierType.TOTAL_PCT && m_auraModifiersGroup[unitMod][modifierType] <= 0.0f)
                return 0.0f;

            //return m_auraModifiersGroup[unitMod][modifierType];
        }
        public bool IsWithinMeleeRange(Unit obj, float dist = (5.0f - 2.0f * 2))
        {
            if (obj == null || !IsInMap(obj) || !InSamePhase(obj))
                return false;

            float dx = GetPositionX() - obj.GetPositionX();
            float dy = GetPositionY() - obj.GetPositionY();
            float dz = GetPositionZ() - obj.GetPositionZ();
            float distsq = (dx * dx) + (dy * dy) + (dz * dz);

            float sizefactor = GetMeleeReach() + obj.GetMeleeReach();
            float maxdist = dist + sizefactor;

            return distsq < maxdist * maxdist;
        }
        float GetMeleeReach() { float reach = GetValue<float>(UnitFields.CombatReach); return reach > ObjectConst.MinMeleeReach ? reach : ObjectConst.MinMeleeReach; }
        public void SetAttackTime(WeaponAttackType att, uint val) { SetValue<float>(UnitFields.AttackRoundBaseTime + (int)att, val * m_modAttackSpeedPct[(int)att]); }
        public void SetPvP(bool state)
        {
            if (state)
                SetFlag<byte>(UnitFields.Bytes2, (byte)UnitPVPStateFlags.PVP, 1);
            else
                RemoveFlag<byte>(UnitFields.Bytes2, (byte)UnitPVPStateFlags.PVP, 1);
        }
                

        #region Fields
        //Public 
        public DeathState m_deathState { get; set; }
        public UnitAI i_AI { get; set; }
        public MotionMaster i_motionMaster;
        public MoveSpline movespline;
        
        
        //Private
        bool IsAIEnabled, NeedChangeAI;
        float[,] m_weaponDamage;
        List<Unit> m_attackers;
        Unit m_attacking;
        float[] SpeedRates;
        UnitTypeMask m_unitTypeMask;
        float[] CreateStats;
        List<AuraEffect>[] m_modAuras;
        Dictionary<SpellValueMod, int> CustomSpellValueMod;
        UnitState m_state;
        TimeTrackerSmall m_movesplineTimer;
        uint[] m_attackTimer;
        float[] m_modAttackSpeedPct;
        bool m_canDualWield;
        #endregion
    }
    public enum SpellValueMod
    {
        BASE_POINT0,
        BASE_POINT1,
        BASE_POINT2,
        RADIUS_MOD,
        MAX_TARGETS,
        AURA_STACK
    }

    public class CustomSpellValues : Dictionary<SpellValueMod, int>
    {
        public void AddSpellMod(SpellValueMod mod, int value)
        {
            Add(mod, value);
        }
    }

    public struct CalcDamageInfo
    {
        public Unit attacker;             // Attacker
        public Unit target;               // Target for damage
        public uint damageSchoolMask;
        public uint damage;
        public uint absorb;
        public uint resist;
        public uint blocked_amount;
        public HitInfo HitInfo;
        public VictimState TargetState;
        // Helper
        public WeaponAttackType attackType; //
        public ProcFlags procAttacker;
        public ProcFlags procVictim;
        public ProcFlagsExLegacy procEx;
        public uint cleanDamage;          // Used only for rage calculation
        //MeleeHitOutcome hitOutCome;  // TODO: remove this field (need use TargetState)
    }
}
