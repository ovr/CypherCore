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
using Framework.Network;
using Framework.Constants;
using Framework.Graphics;
using Framework.ObjectDefines;

namespace WorldServer.Game.Movement
{
    public class MovementPacketBuilder
    {
        public static void WriteMonsterMove(MoveSpline move_spline, ref PacketWriter data)
        {
            WriteCommonMonsterMovePart(move_spline, ref data);

            Spline spline = move_spline.spline;
            MoveSplineFlag splineflags = move_spline.splineflags;
            if (Convert.ToBoolean(splineflags.Raw() & MoveSplineFlag.eFlags.UncompressedPath))
            {
                if (splineflags.cyclic)
                    WriteCatmullRomCyclicPath(spline, ref data);
                else
                    WriteCatmullRomPath(spline, ref data);
            }
            else
                WriteLinearPath(spline, ref data);
        }
        static void WriteCommonMonsterMovePart(MoveSpline move_spline, ref PacketWriter data)
        {
            MoveSplineFlag splineflags = move_spline.splineflags;

            data.WriteUInt8(0);                                       // sets/unsets MOVEMENTFLAG2_UNK7 (0x40)
            data.WriteVec3(move_spline.spline.getPoint(move_spline.spline.first()));
            data.WriteUInt32(move_spline.GetId());

            switch (splineflags.Raw() & MoveSplineFlag.eFlags.Mask_Final_Facing)
            {
                case MoveSplineFlag.eFlags.Final_Target:
                    data.WriteUInt8(MonsterMoveType.FacingTarget);
                    data.WriteUInt64(move_spline.facing.target);
                    break;
                case MoveSplineFlag.eFlags.Final_Angle:
                    data.WriteUInt8(MonsterMoveType.FacingAngle);
                    data.WriteFloat(move_spline.facing.angle);
                    break;
                case MoveSplineFlag.eFlags.Final_Point:
                    data.WriteUInt8(MonsterMoveType.FacingSpot);
                    data.WriteFloat(move_spline.facing.x);
                    data.WriteFloat(move_spline.facing.y);
                    data.WriteFloat(move_spline.facing.z);
                    break;
                default:
                    data.WriteUInt8(MonsterMoveType.Normal);
                    break;
            }

            // add fake Enter_Cycle flag - needed for client-side cyclic movement (client will erase first spline vertex after first cycle done)
            splineflags.enter_cycle = move_spline.isCyclic();
            data.WriteUInt32(splineflags.Raw() & ~MoveSplineFlag.eFlags.Mask_No_Monster_Move);

            if (splineflags.animation)
            {
                data.WriteUInt8(splineflags.getAnimationId());
                data.WriteInt32(move_spline.effect_start_time);
            }

            data.WriteInt32(move_spline.Duration());

            if (splineflags.parabolic)
            {
                data.WriteFloat(move_spline.vertical_acceleration);
                data.WriteInt32(move_spline.effect_start_time);
            }
        }

        public static void WriteStopMovement(Vector3 pos, uint splineId, ref PacketWriter data)
        {
            data.WriteUInt8(0);                                       // sets/unsets MOVEMENTFLAG2_UNK7 (0x40)
            data.WriteVec3(pos);
            data.WriteUInt32(splineId);
            data.WriteUInt8(MonsterMoveType.Stop);
        }

        unsafe static void WriteLinearPath(Spline spline, ref PacketWriter data)
        {
            int last_idx = spline.getPointCount() - 3;
            Vector3[] paths = spline.getPoints();

            //using pointer
            fixed (Vector3* real_path = &paths[1])
            {
                data.WriteUInt32(last_idx);
                data.WriteVec3(real_path[last_idx]);// destination
                if (last_idx > 1)
                {
                    var middle = (real_path[0] + real_path[last_idx]) / 2.0f;
                    Vector3 offset;
                    // first and last points already appended
                    for (var i = 1; i < last_idx; ++i)
                    {
                        offset = middle - real_path[i];
                        data.WritePackXYZ(offset.X, offset.Y, offset.Z);
                    }
                }
            }
            data.WriteUInt16(0);
            /*
            //non pointer might work too not sure will work right
            data.WriteUInt32(last_idx);
            data.WriteVec3(paths[last_idx + 1]);// destination

            if (last_idx > 1)
            {
                var middle = (paths[1] + paths[last_idx + 1]) / 2.0f;
                Vector3 offset;
                // first and last points already appended
                for (var i = 1; i < last_idx; ++i)
                {

                    offset = middle - paths[i + 1];
                    data.WriteVec3(offset);
                }
            }
            */
        }
        static void WriteCatmullRomPath(Spline spline, ref PacketWriter data)
        {
            int count = spline.getPointCount() - 3;
            data.WriteUInt32(count);
            for (var i = 2; i < count; i++)
                data.WriteVec3(spline.getPoint(i));
        }

        static void WriteCatmullRomCyclicPath(Spline spline, ref PacketWriter data)
        {
            int count = spline.getPointCount() - 3;
            data.WriteUInt32(count + 1);
            data.WriteVec3(spline.getPoint(1)); // fake point, client will erase it from the spline after first cycle done
            for (var i = 1; i < count; i++)
                data.WriteVec3(spline.getPoint(i));
        }

        public static void WriteCreateBits(MoveSpline moveSpline, ref PacketWriter data)
        {
            if (!moveSpline.Finalized())
                return;
            data.WriteBit(moveSpline.Finalized());
            data.WriteBits(moveSpline.getPath().Length, 22);
            data.WriteBits(moveSpline.splineflags.Raw(), 25);

            switch (moveSpline.splineflags.Raw() & MoveSplineFlag.eFlags.Mask_Final_Facing)
            {
                case MoveSplineFlag.eFlags.Final_Target:
                    {
                        ObjectGuid targetGuid = new ObjectGuid(moveSpline.facing.target);
                        data.WriteBits(1, 2);
                        data.WriteBit(targetGuid[0]);
                        data.WriteBit(targetGuid[1]);
                        data.WriteBit(targetGuid[6]);
                        data.WriteBit(targetGuid[5]);
                        data.WriteBit(targetGuid[2]);
                        data.WriteBit(targetGuid[3]);
                        data.WriteBit(targetGuid[4]);
                        data.WriteBit(targetGuid[7]);
                        break;
                    }
                case MoveSplineFlag.eFlags.Final_Angle:
                    data.WriteBits(0, 2);
                    break;
                case MoveSplineFlag.eFlags.Final_Point:
                    data.WriteBits(3, 2);
                    break;
                default:
                    data.WriteBits(2, 2);
                    break;
            }

            data.WriteBits(moveSpline.spline.m_mode, 2);
            data.WriteBit(Convert.ToBoolean(moveSpline.splineflags.Raw() & MoveSplineFlag.eFlags.Parabolic) && moveSpline.effect_start_time < moveSpline.Duration());
        }

        public static void WriteCreateData(MoveSpline moveSpline, ref PacketWriter data)
        {
            if (!moveSpline.Finalized())
            {
                MoveSplineFlag splineFlags = moveSpline.splineflags;

                if (splineFlags.final_target)
                {
                    ObjectGuid facingGuid = new ObjectGuid(moveSpline.facing.target);
                    data.WriteByteSeq(facingGuid[3]);
                    data.WriteByteSeq(facingGuid[2]);
                    data.WriteByteSeq(facingGuid[0]);
                    data.WriteByteSeq(facingGuid[5]);
                    data.WriteByteSeq(facingGuid[6]);
                    data.WriteByteSeq(facingGuid[7]);
                    data.WriteByteSeq(facingGuid[4]);
                    data.WriteByteSeq(facingGuid[1]);
                }
                data.WriteInt32(moveSpline.timePassed());
                data.WriteUInt32(moveSpline.Duration());

                if (Convert.ToBoolean(splineFlags.Raw() & MoveSplineFlag.eFlags.Parabolic) && moveSpline.effect_start_time < moveSpline.Duration())
                    data.WriteFloat(moveSpline.vertical_acceleration);

                data.WriteFloat(1.0f);  // splineInfo.duration_mod_next; added in 3.1 
                data.WriteFloat(1.0f);  // splineInfo.duration_mod; added in 3.1

                if (splineFlags.final_point)
                {
                    data.WriteFloat(moveSpline.facing.x);
                    data.WriteFloat(moveSpline.facing.y);
                    data.WriteFloat(moveSpline.facing.z);
                }

                if (Convert.ToBoolean(splineFlags.Raw() & (MoveSplineFlag.eFlags.Parabolic | MoveSplineFlag.eFlags.Animation)))
                    data.WriteInt32(moveSpline.effect_start_time);       // added in 3.1

                int nodes = moveSpline.getPath().Length;
                for (var i = 0; i < nodes; ++i)
                {
                    data.WriteFloat(moveSpline.getPath()[i].Z);
                    data.WriteFloat(moveSpline.getPath()[i].X);
                    data.WriteFloat(moveSpline.getPath()[i].Y);
                }

                if (splineFlags.final_angle)
                    data.WriteFloat(moveSpline.facing.angle);                
            }

            if (!moveSpline.isCyclic())
            {
                Vector3 dest = moveSpline.FinalDestination();
                data.WriteFloat(dest.Z);
                data.WriteFloat(dest.X);
                data.WriteFloat(dest.Y);
            }
            else
                data.WriteVec3(Vector3.Zero);

            data.WriteUInt32(moveSpline.GetId());
        }
    }
}
