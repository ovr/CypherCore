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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.Graphics;
using WorldServer.Game.WorldEntities;
using Framework.Network;
using Framework.Constants;
using WorldServer.Game.AI;

namespace WorldServer.Game.Movement
{
    public class MoveSplineInit
    {
        public MoveSplineInit(Unit m)
        {
            unit = m;
            args.splineId = MotionMaster.GetSplineId();

            // Elevators also use MOVEMENTFLAG_ONTRANSPORT but we do not keep track of their position changes
            args.TransformForTransport = unit.GetTransGUID() != 0;
            // mix existing state into new
            args.flags.walkmode = unit.movementInfo.HasMovementFlag(MovementFlag.Walk);
            args.flags.flying = unit.movementInfo.HasMovementFlag(MovementFlag.CanFly | MovementFlag.DisableGravity);
            args.flags.smoothGroundPath = true; // enabled by default, CatmullRom mode or client config "pathSmoothing" will disable this
        }

        public void MoveTo(Vector3 dest)
        {
            args.path_Idx_offset = 0;
            Array.Resize(ref args.path, 2);
            TransportPathTransform transform = new TransportPathTransform(unit, args.TransformForTransport);                      
            args.path[1] = transform.Calc(dest);
        }
        public void MoveTo(float x, float y, float z)
        {
            Vector3 v = new Vector3(x, y, z);
            MoveTo(v);
        }
        public void SetWalk(bool enable) { args.flags.walkmode = enable; }
        UnitMoveType SelectSpeedType(MovementFlag moveFlags)
        {
            /*! Not sure about MOVEMENTFLAG_CAN_FLY here - do creatures that can fly
                but are on ground right now also have it? If yes, this needs a more
                dynamic check, such as is flying now
            */
            if (Convert.ToBoolean(moveFlags & (MovementFlag.Fly | MovementFlag.CanFly | MovementFlag.DisableGravity)))
            {
                if (Convert.ToBoolean(moveFlags & MovementFlag.Backward /*&& speed_obj.flight >= speed_obj.flight_back*/))
                    return UnitMoveType.FlightBack;
                else
                    return UnitMoveType.Flight;
            }
            else if (Convert.ToBoolean(moveFlags & MovementFlag.Swim))
            {
                if (Convert.ToBoolean(moveFlags & MovementFlag.Backward /*&& speed_obj.swim >= speed_obj.swim_back*/))
                    return UnitMoveType.SwimBack;
                else
                    return UnitMoveType.Swim;
            }
            else if (Convert.ToBoolean(moveFlags & MovementFlag.Walk))
            {
                //if (speed_obj.run > speed_obj.walk)
                return UnitMoveType.Walk;
            }
            else if (Convert.ToBoolean(moveFlags & MovementFlag.Backward /*&& speed_obj.run >= speed_obj.run_back*/))
                return UnitMoveType.RunBack;

            return UnitMoveType.Run;
        }
        public void Launch()
        {
            MoveSpline move_spline = unit.movespline;

            Vector4 real_position = new Vector4(unit.GetPositionX(), unit.GetPositionY(), unit.GetPositionZ(), unit.GetOrientation());
            // Elevators also use MOVEMENTFLAG_ONTRANSPORT but we do not keep track of their position changes
            if (unit.GetTransGUID() != 0)
            {
                //real_position.x = unit.GetTransOffsetX();
                //real_position.y = unit.GetTransOffsetY();
                //real_position.z = unit.GetTransOffsetZ();
                //real_position.orientation = unit.GetTransOffsetO();
            }

            // there is a big chance that current position is unknown if current state is not finalized, need compute it
            // this also allows calculate spline position and update map position in much greater intervals
            // Don't compute for transport movement if the unit is in a motion between two transports
            if (!move_spline.Finalized() && move_spline.onTransport == (unit.GetTransGUID() != 0))
                real_position = move_spline.ComputePosition();

            // should i do the things that user should do? - no.
            if (args.path.Count() == 0)
                return;

            // correct first vertex
            args.path[0] = new Vector3(real_position.X, real_position.Y, real_position.Z);
            args.initialOrientation = real_position.O;
            move_spline.onTransport = (unit.GetTransGUID() != 0);

            MovementFlag moveFlags = unit.movementInfo.GetMovementFlags();
            if (args.flags.walkmode)
                moveFlags |= MovementFlag.Walk;
            else
                moveFlags &= ~MovementFlag.Walk;

            moveFlags |= MovementFlag.Forward;

            if (!args.HasVelocity)
                args.velocity = unit.GetSpeed(SelectSpeedType(moveFlags));

            if (!args.Validate(unit))
                return;

            //if (Convert.ToBoolean(moveFlags & MovementFlag.Root))
            //moveFlags &= ~MOVEMENTFLAG_MASK_MOVING;

            unit.movementInfo.SetMovementFlags(moveFlags);
            move_spline.Initialize(args);

            PacketWriter data = new PacketWriter(Opcodes.SMSG_MonsterMove);
            data.WritePackedGuid(unit.GetPackGUID());
            if (unit.GetTransGUID() != 0)
            {
                data = new PacketWriter(Opcodes.SMSG_MonsterMoveTransport);
                data.WritePackedGuid(unit.GetPackGUID());
                data.WritePackedGuid(unit.GetTransGUID());
                data.WriteUInt8(0);//unit.GetTransSeat());
            }

            MovementPacketBuilder.WriteMonsterMove(move_spline, ref data);
            unit.SendMessageToSet(data, true);
        }
        public void SetFacing(Unit target)
        {
            args.flags.EnableFacingTarget();
            args.facing.target = target.GetGUID();
        }

        public void SetFacing(float angle)
        {
            if (args.TransformForTransport)
            {
                //if (Unit vehicle = unit->GetVehicleBase())
                //angle -= vehicle->GetOrientation();
                //else if (Transport* transport = unit->GetTransport())
                //angle -= transport->GetOrientation();
            }

            //args.facing.angle = wrap(angle, 0.f, (float)G3D::twoPi());
            args.flags.EnableFacingAngle();
        }
        public void SetFacing(Vector3 spot)
        {
            TransportPathTransform transform = new TransportPathTransform(unit, args.TransformForTransport);
            Vector3 finalSpot = transform.Calc(spot);
            args.facing.x = finalSpot.X;
            args.facing.y = finalSpot.Y;
            args.facing.z = finalSpot.Z;
            args.flags.EnableFacingPoint();
        }
        public void MovebyPath(Vector3[] controls, int path_offset)
        {
            args.path_Idx_offset = path_offset;
            Array.Resize(ref args.path, controls.Length);
            args.path = Array.ConvertAll(controls, r => r = new TransportPathTransform(unit, args.TransformForTransport).Calc(r));
        }

        MoveSplineInitArgs args = new MoveSplineInitArgs();
        Unit unit;
    }


        // Transforms coordinates from global to transport offsets
    public class TransportPathTransform
    {
        public TransportPathTransform(Unit owner, bool transformForTransport)
        {
            _owner = owner;
            _transformForTransport = transformForTransport;
            }
        //Vector3 operator()(Vector3 input);
        public Vector3 Calc(Vector3 input)
        {
            if (_transformForTransport)
            {
                //if (TransportBase* transport = _owner.GetDirectTransport())
                {
                    //float unused = 0.0f; // need reference
                    //transport->CalculatePassengerOffset(input.x, input.y, input.z, unused);
                }
            }
            return input;
        }

        Unit _owner;
        bool _transformForTransport;
    }
}
