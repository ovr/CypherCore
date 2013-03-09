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
using System.Linq;
using Framework.Constants;
using Framework.Logging;
using Framework.Network;
using Framework.Utility;
using WorldServer.Game.Managers;
using WorldServer.Game.Maps;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;

namespace WorldServer.Game.Packets
{
    public class MovementHandler : Cypher
    {
        [ClientOpcode(Opcodes.MSG_MoveStartForward)]
        public static void HandleMoveStartForward(ref PacketReader packet, ref WorldSession session)
        {
            Unit mover = session.GetPlayer();

            Player plrMover = mover.ToPlayer();

            var mi = new MovementInfo();

            mi.Pos = new ObjectPosition()
            {
                X = packet.ReadFloat(),
                Y = packet.ReadFloat(),
                Z = packet.ReadFloat()
            };

            byte[] mask = new byte[8];

            mask[5] = packet.GetBit();
            mask[4] = packet.GetBit();
            mi.Status.HasSplineElevation = !packet.ReadBit();
            mask[3] = packet.GetBit();
            mi.Status.HasTransportData = packet.ReadBit();
            mi.Status.HasOrientation = !packet.ReadBit();
            bool Unknown = packet.ReadBit();
            mask[6] = packet.GetBit();
            mi.Status.HasMovementFlags2 = !packet.ReadBit();
            mask[0] = packet.GetBit();
            mask[2] = packet.GetBit();
            bool Unknown2 = packet.ReadBit();
            bool Unknown3 = packet.ReadBit();
            mask[7] = packet.GetBit();
            mi.Status.IsAlive = !packet.ReadBit();
            uint counter = packet.GetBits<uint>(24);
            mask[1] = packet.GetBit();
            mi.Status.HasTimeStamp = !packet.ReadBit();
            mi.Status.HasPitch = !packet.ReadBit();
            mi.Status.HasMovementFlags = !packet.ReadBit();
            bool Unknown4 = packet.ReadBit();

             if (mi.Status.HasMovementFlags2)
                mi.Flags2 = (MovementFlag2)packet.GetBits<uint>(13);

            if (mi.Status.HasTransportData)
            {

            }
            
            if (mi.Status.HasFallData)
                mi.Status.HasFallDirection = packet.ReadBit();

            if (mi.Status.HasMovementFlags)
                mi.Flags = (MovementFlag)packet.GetBits<uint>(30);

            mi.Guid[1] = packet.ReadByteSeq(mask[1]);

            for (int i = 0; i < counter; i++)
                packet.ReadUInt32();

            mi.Guid[0] = packet.ReadByteSeq(mask[0]);
            mi.Guid[4] = packet.ReadByteSeq(mask[4]);
            mi.Guid[2] = packet.ReadByteSeq(mask[2]);
            mi.Guid[5] = packet.ReadByteSeq(mask[5]);     
            mi.Guid[3] = packet.ReadByteSeq(mask[3]);
            mi.Guid[7] = packet.ReadByteSeq(mask[7]);
            mi.Guid[6] = packet.ReadByteSeq(mask[6]);

            if (mi.Status.HasTransportData)
            {
                 
            }
            
            if (mi.Status.HasFallData)
            {

            }

            if (mi.Status.IsAlive)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasOrientation)
                mi.Pos.Orientation = packet.ReadFloat();

            if (mi.Status.HasSplineElevation)
                mi.SplineElevation = packet.ReadFloat();

            if (mi.Status.HasTimeStamp)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasPitch)
                mi.Pitch = packet.ReadFloat();

            HandleMoveUpdate(ref session, mi);
        }

        [ClientOpcode(Opcodes.MSG_MoveStartBackward)]
        public static void HandleMoveStartBackward(ref PacketReader packet, ref WorldSession session)
        {
            var mi = new MovementInfo();

            byte[] mask = new byte[8];

            mi.Pos = new ObjectPosition()
            {
                X = packet.ReadFloat(),
                Z = packet.ReadFloat(),
                Y = packet.ReadFloat()
            };

            mask[3] = packet.GetBit();
            mask[6] = packet.GetBit();
            mi.Status.HasMovementFlags = !packet.ReadBit();
            mi.Status.HasFallData = packet.ReadBit();
            mi.Status.HasSplineElevation = !packet.ReadBit();
            mi.Status.HasOrientation = !packet.ReadBit();
            mask[4] = packet.GetBit();
            mi.Status.IsAlive = !packet.ReadBit();
            mask[1] = packet.GetBit();
            mi.Status.HasTransportData = packet.ReadBit();
            bool Unknown2 = packet.ReadBit();
            mask[0] = packet.GetBit();
            bool Unknown = packet.ReadBit();
            mi.Status.HasMovementFlags2 = !packet.ReadBit();
            mask[2] = packet.GetBit();
            mi.Status.HasPitch = !packet.ReadBit();
            bool Unknown3 = packet.ReadBit();
            mask[5] = packet.GetBit();
            mask[7] = packet.GetBit();
            mi.Status.HasTimeStamp = !packet.ReadBit();
            uint counter = packet.GetBits<uint>(24);

            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.HasFallData)
                mi.Status.HasFallDirection = packet.ReadBit();

            if (mi.Status.HasMovementFlags2)
                mi.Flags2 = (MovementFlag2)packet.GetBits<uint>(13);
            
            if (mi.Status.HasMovementFlags)
                mi.Flags = (MovementFlag)packet.GetBits<uint>(30);

            mi.Guid[6] = packet.ReadByteSeq(mask[6]);
            mi.Guid[4] = packet.ReadByteSeq(mask[4]);
            mi.Guid[0] = packet.ReadByteSeq(mask[0]);
            mi.Guid[1] = packet.ReadByteSeq(mask[1]);
            mi.Guid[5] = packet.ReadByteSeq(mask[5]);
            mi.Guid[2] = packet.ReadByteSeq(mask[2]);

            for (int i = 0; i < counter; i++)
                packet.ReadUInt32();

            mi.Guid[7] = packet.ReadByteSeq(mask[7]);
            mi.Guid[3] = packet.ReadByteSeq(mask[3]);

            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.HasSplineElevation)
                mi.SplineElevation = packet.ReadFloat();

            if (mi.Status.HasFallData)
            {
                if (mi.Status.HasFallDirection)
                {
                    mi.Jump.cosAngle = packet.ReadFloat();//guessed
                    mi.Jump.sinAngle = packet.ReadFloat();//guessed
                    mi.Jump.xyspeed = packet.ReadFloat();//guessed
                }

                mi.FallTime = packet.ReadUInt32();
                mi.Jump.velocity = packet.ReadFloat();//guessed
            }

            if (mi.Status.HasTimeStamp)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasPitch)
                mi.Pitch = packet.ReadFloat();

            if (mi.Status.HasOrientation)
                mi.Pos.Orientation = packet.ReadFloat();

            if (mi.Status.IsAlive)
                mi.Time = packet.ReadUInt32();

            HandleMoveUpdate(ref session, mi);
        }

        [ClientOpcode(Opcodes.MSG_MoveHeartbeat)]
        public static void HandleMoveHeartBeat(ref PacketReader packet, ref WorldSession session)
        {
            var mi = new MovementInfo();

            byte[] mask = new byte[8];

            mi.Pos = new ObjectPosition()
            {
                X = packet.ReadFloat(),
                Y = packet.ReadFloat(),
                Z = packet.ReadFloat()
            };

            mi.Status.HasMovementFlags = !packet.ReadBit();
            mi.Status.HasFallData = packet.ReadBit();
            uint counter = packet.GetBits<uint>(24);
            mi.Status.IsAlive = !packet.ReadBit();
            mi.Status.HasMovementFlags2 = !packet.ReadBit();
            mi.Status.HasPitch = !packet.ReadBit();
            mask[4] = packet.GetBit();
            mi.Status.HasTransportData = packet.ReadBit();
            mask[7] = packet.GetBit();
            mask[0] = packet.GetBit();
            bool Unknown2 = packet.ReadBit();
            mask[3] = packet.GetBit();
            mi.Status.HasSplineElevation = !packet.ReadBit();
            mask[1] = packet.GetBit();
            bool Unknown3 = packet.ReadBit();
            mask[5] = packet.GetBit();
            mask[2] = packet.GetBit();
            mi.Status.HasOrientation = !packet.ReadBit();
            bool Unknown4 = packet.ReadBit();
            mask[6] = packet.GetBit();
            mi.Status.HasTimeStamp = !packet.ReadBit();

            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.HasFallData)
                mi.Status.HasFallDirection = packet.ReadBit();

            if (mi.Status.HasMovementFlags)
                mi.Flags = (MovementFlag)packet.GetBits<uint>(30);

            if (mi.Status.HasMovementFlags2)
                mi.Flags2 = (MovementFlag2)packet.GetBits<uint>(13);

            mi.Guid[7] = packet.ReadByteSeq(mask[7]);

            for (int i = 0; i < counter; i++)
                packet.ReadUInt32();

            mi.Guid[1] = packet.ReadByteSeq(mask[1]);
            mi.Guid[3] = packet.ReadByteSeq(mask[3]);
            mi.Guid[0] = packet.ReadByteSeq(mask[0]);
            mi.Guid[5] = packet.ReadByteSeq(mask[5]);
            mi.Guid[4] = packet.ReadByteSeq(mask[4]);
            mi.Guid[6] = packet.ReadByteSeq(mask[6]);
            mi.Guid[2] = packet.ReadByteSeq(mask[2]);

            if (mi.Status.HasFallData)
            {
                if (mi.Status.HasFallDirection)
                {
                    mi.Jump.xyspeed = packet.ReadFloat();//guessed
                    mi.Jump.sinAngle = packet.ReadFloat();//guessed
                    mi.Jump.cosAngle = packet.ReadFloat();//guessed
                }

                mi.Jump.velocity = packet.ReadFloat();
                mi.FallTime = packet.ReadUInt32();
            }


            if (mi.Status.HasTimeStamp)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.HasOrientation)
                mi.Pos.Orientation = packet.ReadFloat();

            if (mi.Status.HasPitch)
                packet.ReadFloat();

            if (mi.Status.IsAlive)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasSplineElevation)
                mi.SplineElevation = packet.ReadFloat();

            HandleMoveUpdate(ref session, mi);
        }

        [ClientOpcode(Opcodes.MSG_MoveStop)]
        public static void HandleMoveStop(ref PacketReader packet, ref WorldSession session)
        {
            var mi = new MovementInfo();

            byte[] mask = new byte[8];

            mi.Pos = new ObjectPosition()
            {
                X = packet.ReadFloat(),
                Z = packet.ReadFloat(),
                Y = packet.ReadFloat()
            };

            mi.Status.HasMovementFlags = !packet.ReadBit();
            mi.Status.HasTransportData = packet.ReadBit();
            mi.Status.HasPitch = !packet.ReadBit();
            mi.Status.HasOrientation = !packet.ReadBit();
            bool Unknown = packet.ReadBit();
            mi.Status.HasSplineElevation = !packet.ReadBit();
            uint counter = packet.GetBits<uint>(24);
            mi.Status.HasTimeStamp = !packet.ReadBit();
            mask[4] = packet.GetBit();
            bool Unknown2 = packet.ReadBit();
            mask[6] = packet.GetBit();
            mask[0] = packet.GetBit();
            mask[5] = packet.GetBit();
            mask[1] = packet.GetBit();
            mi.Status.IsAlive = !packet.ReadBit();
            mask[7] = packet.GetBit();
            mask[2] = packet.GetBit();
            bool Unknown3 = packet.ReadBit();
            mask[3] = packet.GetBit();
            mi.Status.HasMovementFlags2 = !packet.ReadBit();
            bool Unknown4 = packet.ReadBit();

            if (mi.Status.HasFallData)
                mi.Status.HasFallDirection = packet.ReadBit();
            
            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.HasMovementFlags2)
                mi.Flags2 = (MovementFlag2)packet.GetBits<uint>(13);

            if (mi.Status.HasMovementFlags)
                mi.Flags = (MovementFlag)packet.GetBits<uint>(30);

            mi.Guid[1] = packet.ReadByteSeq(mask[1]);
            mi.Guid[2] = packet.ReadByteSeq(mask[2]);
            mi.Guid[4] = packet.ReadByteSeq(mask[4]);
            mi.Guid[3] = packet.ReadByteSeq(mask[3]);

            for (int i = 0; i < counter; i++)
                packet.ReadUInt32();

            mi.Guid[5] = packet.ReadByteSeq(mask[5]);
            mi.Guid[0] = packet.ReadByteSeq(mask[0]);
            mi.Guid[6] = packet.ReadByteSeq(mask[6]);
            mi.Guid[7] = packet.ReadByteSeq(mask[7]);

            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.HasTimeStamp)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasPitch)
                mi.Pitch = packet.ReadFloat();

            if (mi.Status.HasFallData)
            {

            }

            if (mi.Status.IsAlive)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasOrientation)
                mi.Pos.Orientation = packet.ReadFloat();

            if (mi.Status.HasSplineElevation)
                mi.SplineElevation = packet.ReadFloat();

            HandleMoveUpdate(ref session, mi);
        }

        [ClientOpcode(Opcodes.MSG_MoveStartTurnLeft)]
        public static void HandleMoveStartTurnLeft(ref PacketReader packet, ref WorldSession session)
        {
            var mi = new MovementInfo();

            byte[] mask = new byte[8];

            mi.Pos = new ObjectPosition()
            {
                Z = packet.ReadFloat(),
                Y = packet.ReadFloat(),
                X = packet.ReadFloat()
            };

            bool Unknown = packet.ReadBit();
            bool Unknown2 = packet.ReadBit();
            uint counter = packet.GetBits<uint>(24);
            mask[2] = packet.GetBit();
            mask[4] = packet.GetBit();
            mask[7] = packet.GetBit();
            mask[1] = packet.GetBit();
            mi.Status.HasPitch = !packet.ReadBit();
            mask[0] = packet.GetBit();
            mi.Status.IsAlive = !packet.ReadBit();
            mi.Status.HasTransportData = packet.ReadBit();
            bool Unknown3 = packet.ReadBit();
            mask[6] = packet.GetBit();
            mi.Status.HasMovementFlags = !packet.ReadBit();
            bool Unknown4 = packet.ReadBit();
            mi.Status.HasOrientation = !packet.ReadBit();
            mi.Status.HasMovementFlags2 = !packet.ReadBit();
            mask[3] = packet.GetBit();
            mask[5] = packet.GetBit();
            mi.Status.HasTimeStamp = !packet.ReadBit();
            mi.Status.HasSplineElevation = !packet.ReadBit();

            if (mi.Status.HasTransportData)
            {

            }
            
            if (mi.Status.HasFallData)
                mi.Status.HasFallDirection = packet.ReadBit();

            if (mi.Status.HasMovementFlags2)
                mi.Flags2 = (MovementFlag2)packet.GetBits<uint>(13);

            if (mi.Status.HasMovementFlags)
                mi.Flags = (MovementFlag)packet.GetBits<uint>(30);

            mi.Guid[4] = packet.ReadByteSeq(mask[4]);
            mi.Guid[6] = packet.ReadByteSeq(mask[6]);

            for (int i = 0; i < counter; i++)
                packet.ReadUInt32();

            mi.Guid[1] = packet.ReadByteSeq(mask[1]);
            mi.Guid[2] = packet.ReadByteSeq(mask[2]);
            mi.Guid[0] = packet.ReadByteSeq(mask[0]);
            mi.Guid[7] = packet.ReadByteSeq(mask[7]);
            mi.Guid[5] = packet.ReadByteSeq(mask[5]);
            mi.Guid[3] = packet.ReadByteSeq(mask[3]);

            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.IsAlive)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasPitch)
                mi.Pitch = packet.ReadFloat();

            if (mi.Status.HasFallData)
            {
            
            }


            if (mi.Status.HasOrientation)
                mi.Pos.Orientation = packet.ReadFloat();

            if (mi.Status.HasSplineElevation)
                mi.SplineElevation = packet.ReadFloat();

            if (mi.Status.HasTimeStamp)
                mi.Time = packet.ReadUInt32();

            HandleMoveUpdate(ref session, mi);
        }

        [ClientOpcode(Opcodes.MSG_MoveStartTurnRight)]
        public static void HandleMoveStartTurnRight(ref PacketReader packet, ref WorldSession session)
        {
            var mi = new MovementInfo();
            byte[] mask = new byte[8];

            mi.Pos = new ObjectPosition()
            {
                Y = packet.ReadFloat(),
                Z = packet.ReadFloat(),
                X = packet.ReadFloat()
            };

            mask[5] = packet.GetBit();
            mask[3] = packet.GetBit();
            mi.Status.HasTimeStamp = !packet.ReadBit();
            mask[1] = packet.GetBit();
            mi.Status.HasMovementFlags2 = !packet.ReadBit();
            mask[0] = packet.GetBit();
            bool Unknown4 = packet.ReadBit();
            bool Unknown = packet.ReadBit();
            mi.Status.HasOrientation = !packet.ReadBit();
            bool HasSplineElevation = !packet.ReadBit();
            uint counter = packet.GetBits<uint>(24);
            mask[4] = packet.GetBit();
            mi.Status.HasTransportData = packet.ReadBit();
            bool Unknown2 = packet.ReadBit();
            mask[2] = packet.GetBit();
            mi.Status.IsAlive = !packet.ReadBit();
            mi.Status.HasPitch = !packet.ReadBit();
            mi.Status.HasMovementFlags = !packet.ReadBit();
            mask[7] = packet.GetBit();
            mask[6] = packet.GetBit();
            bool Unknown3 = packet.ReadBit();

            if (mi.Status.HasMovementFlags)
                mi.Flags = (MovementFlag)packet.GetBits<uint>(30);

            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.HasMovementFlags2)
                mi.Flags2 = (MovementFlag2)packet.GetBits<uint>(13);

            if (mi.Status.HasFallData)
                mi.Status.HasFallDirection = packet.ReadBit();

            mi.Guid[3] = packet.ReadByteSeq(mask[3]);
            mi.Guid[5] = packet.ReadByteSeq(mask[5]);
            mi.Guid[4] = packet.ReadByteSeq(mask[4]);
            mi.Guid[6] = packet.ReadByteSeq(mask[6]);

            for (int i = 0; i < counter; i++)
                packet.ReadUInt32();

            mi.Guid[2] = packet.ReadByteSeq(mask[2]);
            mi.Guid[7] = packet.ReadByteSeq(mask[7]);
            mi.Guid[1] = packet.ReadByteSeq(mask[1]);
            mi.Guid[0] = packet.ReadByteSeq(mask[0]);

            if (mi.Status.HasOrientation)
                mi.Pos.Orientation = packet.ReadFloat();

            if (mi.Status.HasFallData)
            {

            }

            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.HasTimeStamp)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasPitch)
                mi.Pitch = packet.ReadFloat();

            if (HasSplineElevation)
                mi.SplineElevation = packet.ReadFloat();

            if (mi.Status.IsAlive)
                mi.Time = packet.ReadUInt32();

            HandleMoveUpdate(ref session, mi);
        }

        [ClientOpcode(Opcodes.MSG_MoveStopTurn)]
        public static void HandleMoveStopTurn(ref PacketReader packet, ref WorldSession session)
        {
            var mi = new MovementInfo();

            byte[] mask = new byte[8];

            mi.Pos = new ObjectPosition()
            {
                X = packet.ReadFloat(),
                Z = packet.ReadFloat(),
                Y = packet.ReadFloat()
            };

            mi.Status.HasTimeStamp = !packet.ReadBit();
            mask[5] = packet.GetBit();
            bool Unknown = packet.ReadBit();
            mi.Status.HasTransportData = packet.ReadBit();
            bool Unknown2 = packet.ReadBit();
            mask[3] = packet.GetBit();
            mi.Status.HasSplineElevation = !packet.ReadBit();
            mask[0] = packet.GetBit();
            mi.Status.HasPitch = !packet.ReadBit();
            uint counter = packet.GetBits<uint>(24);
            mask[1] = packet.GetBit();
            mask[7] = packet.GetBit();
            mi.Status.HasMovementFlags = !packet.ReadBit();
            mi.Status.IsAlive = !packet.ReadBit();
            mask[2] = packet.GetBit();
            mask[6] = packet.GetBit();
            mi.Status.HasOrientation = !packet.ReadBit();
            bool Unknown3 = packet.ReadBit();
            mi.Status.HasMovementFlags2 = !packet.ReadBit();
            bool Unknown4 = packet.ReadBit();
            mask[4] = packet.GetBit();

            if (mi.Status.HasTransportData)
            {

            }
            
            if (mi.Status.HasFallData)
                mi.Status.HasFallDirection = packet.ReadBit();

            if (mi.Status.HasMovementFlags)
                mi.Flags = (MovementFlag)packet.GetBits<uint>(30);

            if (mi.Status.HasMovementFlags2)
                mi.Flags2 = (MovementFlag2)packet.GetBits<uint>(13);

            mi.Guid[6] = packet.ReadByteSeq(mask[6]);
            mi.Guid[0] = packet.ReadByteSeq(mask[0]);
            mi.Guid[5] = packet.ReadByteSeq(mask[5]);

            for (int i = 0; i < counter; i++)
                packet.ReadUInt32();

            mi.Guid[1] = packet.ReadByteSeq(mask[1]);
            mi.Guid[7] = packet.ReadByteSeq(mask[7]);
            mi.Guid[3] = packet.ReadByteSeq(mask[3]);
            mi.Guid[4] = packet.ReadByteSeq(mask[4]);
            mi.Guid[2] = packet.ReadByteSeq(mask[2]);

            if (mi.Status.HasTransportData)
            {

            }

            if (mi.Status.HasOrientation)
                mi.Pos.Orientation = packet.ReadFloat();

            if (mi.Status.IsAlive)
                mi.Time = packet.ReadUInt32();

            if (mi.Status.HasFallData)
            {

            }

            if (mi.Status.HasPitch)
                mi.Pitch = packet.ReadFloat();

            if (mi.Status.HasSplineElevation)
                mi.SplineElevation = packet.ReadFloat();

            if (mi.Status.HasTimeStamp)
                mi.Time = packet.ReadUInt32();

            HandleMoveUpdate(ref session, mi);
        }

        static void HandleMoveUpdate(ref WorldSession session, MovementInfo mi)
        {  
            Player pl = session.GetPlayer();
            // prevent tampered movement data
            if (mi.Guid != pl.GetGUIDLow())
            {
                Log.outError("HandleMovementOpcodes: guid error");
                return;
            }
            if (!mi.Pos.IsPositionValid())
            {
                Log.outError("HandleMovementOpcodes: Invalid Position");
                return;
            }

            PacketWriter data = new PacketWriter(Opcodes.SMSG_MoveUpdate);
            WriteMovementInfo(ref data, mi);

            //todo add support for mind controls ect.
            pl.SendMessageToSet(data, false);

            pl.movementInfo = mi;

            pl.UpdatePostion(mi.Pos);
        }

        static void WriteMovementInfo(ref PacketWriter writer, MovementInfo mi)
        {
            writer.WriteBit(mi.Guid[0]);
            writer.WriteBit(!mi.Status.HasMovementFlags);
            writer.WriteBit(!mi.Status.HasOrientation);
            writer.WriteBit(mi.Guid[2]);
            writer.WriteBit(mi.Guid[6]);
            writer.WriteBit(!mi.Status.HasMovementFlags2);
            writer.WriteBit(mi.Guid[7]);
            writer.WriteBits<uint>(0, 24);
            writer.WriteBit(mi.Guid[1]);
            if (mi.Status.HasMovementFlags)
                writer.WriteBits((uint)mi.Flags, 30);
            writer.WriteBit(mi.Guid[4]);
            writer.WriteBit(!mi.Status.IsAlive);
            writer.WriteBit(mi.Status.HasTransportData);//not sure
            if (mi.Status.HasMovementFlags2)
                writer.WriteBits((uint)mi.Flags2, 13);

            writer.WriteBit(mi.Status.HasSpline);
            writer.WriteBit(mi.Guid[5]);
            writer.WriteBit(true);//not sure
            writer.WriteBit(0);//not sure
            writer.WriteBit(mi.Status.HasFallData);
            writer.WriteBit(false); //not sure
            writer.WriteBit(!mi.Status.HasPitch); //not sure
            writer.WriteBit(mi.Guid[3]);
            writer.WriteBit(!mi.Status.HasSplineElevation); //not sure

            if (mi.Status.HasFallData)
                writer.WriteBit(mi.Status.HasFallDirection);
            /*
            if (mi.Status.HaveTransportData)
            {
                writer.WriteBit(mi.TransGuid[3]);
                writer.WriteBit(mi.Status.HaveTransportTime3);
                writer.WriteBit(mi.TransGuid[6]);
                writer.WriteBit(mi.TransGuid[1]);
                writer.WriteBit(mi.TransGuid[7]);
                writer.WriteBit(mi.TransGuid[0]);
                writer.WriteBit(mi.TransGuid[4]);
                writer.WriteBit(mi.Status.HaveTransportTime2);
                writer.WriteBit(mi.TransGuid[5]);
                writer.WriteBit(mi.TransGuid[2]);
            }
            */
            writer.BitFlush();

            if (mi.Status.HasFallData)
            {  
                writer.WriteUInt32(mi.FallTime);
                if (mi.Status.HasFallDirection)
                {
                    writer.WriteFloat(mi.Jump.xyspeed);
                    writer.WriteFloat(mi.Jump.cosAngle);
                    writer.WriteFloat(mi.Jump.sinAngle);
                }
                writer.WriteFloat(mi.Jump.velocity);
            }
            if (mi.Status.IsAlive)
                writer.WriteUInt32(mi.Time);
            writer.WriteByteSeq(mi.Guid[5]);
            writer.WriteByteSeq(mi.Guid[7]);
            writer.WriteByteSeq(mi.Guid[1]);
            writer.WriteFloat(mi.Pos.Z);
            writer.WriteByteSeq(mi.Guid[4]);
            writer.WriteByteSeq(mi.Guid[3]);
            writer.WriteByteSeq(mi.Guid[2]);
            writer.WriteByteSeq(mi.Guid[6]);
            writer.WriteByteSeq(mi.Guid[0]);

            writer.WriteFloat(mi.Pos.X);
            if (mi.Status.HasOrientation)
                writer.WriteFloat(mi.Pos.Orientation);
            writer.WriteFloat(mi.Pos.Y);


            /*
            if (mi.Status.HaveSplineElevation)
                writer.WriteFloat(mi.SplineElevation);

            if (mi.Status.HaveTransportData)
            {
                if (mi.Status.HaveTransportTime3)
                    writer.WriteUInt32(mi.TransTime2);
                writer.WriteByteSeq(mi.TransGuid[6]);
                writer.WriteInt32(mi.TransSeat);
                writer.WriteByteSeq(mi.TransGuid[5]);
                writer.WriteFloat(mi.TransPos.X);
                writer.WriteByteSeq(mi.TransGuid[1]);
                writer.WriteFloat(mi.TransPos.Orientation);
                writer.WriteByteSeq(mi.TransGuid[2]);
                if (mi.Status.HaveTransportTime2)
                    writer.WriteUInt32(mi.TransTime2);
                writer.WriteByteSeq(mi.TransGuid[0]);
                writer.WriteFloat(mi.TransPos.Z);
                writer.WriteByteSeq(mi.TransGuid[7]);
                writer.WriteByteSeq(mi.TransGuid[4]);
                writer.WriteByteSeq(mi.TransGuid[3]);
                writer.WriteFloat(mi.TransPos.Y);
                writer.WriteUInt32(mi.TransTime);
            }

            if (mi.Status.HavePitch)
                writer.WriteFloat(mi.Pitch);
            */
        }

        /*
        [Opcode(Message.CMSG_MoveFallReset)]
        [Opcode(Message.MSG_MoveFallLand)]
        [Opcode(Message.MSG_MoveJump)]
        [Opcode(Message.MSG_MoveSetFacing)]
        [Opcode(Message.MSG_MoveSetPitch)]
        [Opcode(Message.MSG_MoveStartAscend)]
        [Opcode(Message.MSG_MoveStartDescend)]
        [Opcode(Message.MSG_MoveStartPitchDown)]
        [Opcode(Message.MSG_MoveStartPitchUp)]
        [Opcode(Message.MSG_MoveStartStrafeLeft)]
        [Opcode(Message.MSG_MoveStartStrafeRight)]
        [Opcode(Message.MSG_MoveStartSwim)]
        [Opcode(Message.MSG_MoveStopAscend)]
        [Opcode(Message.MSG_MoveStopPitch)]
        [Opcode(Message.MSG_MoveStopStrafe)]
        [Opcode(Message.MSG_MoveStopSwim)]

        public static void HandleMovementOpcodes(ref PacketReader packet, ref WorldSession session)
        {
            if (movementInfo.TransGuid != 0)
            {
                if (movementInfo.TransPos.X > 50 || movementInfo.TransPos.Y > 50 || movementInfo.TransPos.Z > 50)                
                    return;                

                if (!CellHandler.IsValidMapCoord(movementInfo.Pos.X + movementInfo.TransPos.X, movementInfo.Pos.Y + movementInfo.TransPos.Y,
                    movementInfo.Pos.Z + movementInfo.TransPos.Z, movementInfo.Pos.Orientation + movementInfo.TransPos.Orientation))
                    return;

                if (plrMover != null)
                {
                    //if (!plrMover->GetTransport())
                    {
                        // elevators also cause the client to send MOVEMENTFLAG_ONTRANSPORT - just dismount if the guid can be found in the transport list
                        //for (MapManager::TransportSet::const_iterator iter = sMapMgr->m_Transports.begin(); iter != sMapMgr->m_Transports.end(); ++iter)
                        {
                            //if ((*iter)->GetGUID() == movementInfo.t_guid)
                            {
                                //plrMover->m_transport = *iter;
                                //(*iter)->AddPassenger(plrMover);
                                //break;
                            }
                        }
                    }
                    //else if (plrMover->GetTransport()->GetGUID() != movementInfo.t_guid)
                    {                
                        bool foundNewTransport = false;

                        //plrMover->m_transport->RemovePassenger(plrMover);
                        //for (MapManager::TransportSet::const_iterator iter = sMapMgr->m_Transports.begin(); iter != sMapMgr->m_Transports.end(); ++iter)
                        {
                            //if ((*iter)->GetGUID() == movementInfo.t_guid)
                            {
                                //foundNewTransport = true;
                                //plrMover->m_transport = *iter;
                                //(*iter)->AddPassenger(plrMover);
                                //break;       
                            }
                        }
                        
                        if (!foundNewTransport)
                        {
                            //plrMover->m_transport = NULL;
                            movementInfo.TransPos.Relocate(0.0f, 0.0f, 0.0f, 0.0f);
                            movementInfo.TransTime = 0;
                            movementInfo.TransSeat = -1;
                        }
                    }
                }
                
                //if (!mover->GetTransport() && !mover->GetVehicle())
                {
                    GameObject go = (mover.GetMap().GetObject(movementInfo.TransGuid) as GameObject);
                    if (go == null || !go.IsGameObjectType(GameObjectTypes.Transport))
                        movementInfo.TransGuid = new ObjectGuid();
                }
            }
            //else if (plrMover && plrMover->GetTransport())                // if we were on a transport, leave
            {
                //plrMover->m_transport->RemovePassenger(plrMover);
                //plrMover->m_transport = NULL;
                //movementInfo.TransPos.Relocate(0.0f, 0.0f, 0.0f, 0.0f);
                //movementInfo.TransTime = 0;
                //movementInfo.TransSeat = -1;
            }
            
            // fall damage generation (ignore in flight case that can be triggered also at lags in moment teleportation to another map).
            //if (packet.Opcode == Opcode(Message.MSG_MoveFallLand && plrMover && !plrMover->isInFlight())
                //plrMover->HandleFall(movementInfo);

            //if (plrMover && ((movementInfo.Flags & MovementFlag.Swim) != 0) != plrMover->IsInWater())
            {
                // now client not include swimming flag in case jumping under water
                //plrMover->SetInWater(!plrMover->IsInWater() || plrMover->GetBaseMap()->IsUnderWater(movementInfo.pos.GetPositionX(), movementInfo.pos.GetPositionY(), movementInfo.pos.GetPositionZ()));
            }


            if (plrMover != null)
            {
                //plrMover->UpdateFallInformationIfNeed(movementInfo, opcode);
                
                if (movementInfo.Pos.Z < -500.0f)
                {
                    //if (!(plrMover->GetBattleground() && plrMover->GetBattleground()->HandlePlayerUnderMap(_player)))
                    {
                        // NOTE: this is actually called many times while falling
                        // even after the player has been teleported away
                        // TODO: discard movement packets after the player is rooted
                        //if (plrMover->isAlive())
                        {
                            //plrMover->EnvironmentalDamage(DAMAGE_FALL_TO_VOID, GetPlayer()->GetMaxHealth());
                            // player can be alive if GM/etc
                            // change the death state to CORPSE to prevent the death timer from
                            // starting in the next player update
                            //if (!plrMover->isAlive())
                                //plrMover->KillPlayer();
                        }
                    }
                }
            }    
        }


        static void ReadMovementInfo(ref PacketReader packet, ref MovementInfo mi)
        {
            MSE[] sequence = MovementStatus.GetMovementStatus((uint)packet.Opcode);

            byte[] mask = new byte[8];
            byte[] guid = new byte[8];

            byte[] tmask = new byte[8];
            byte[] tguid = new byte[8];


            for (int i = 0; i < sequence.Count(); ++i)
            {
                MSE element = sequence[i];
                if (element == MSE.End)
                    break;

                if (element >= MSE.mask0 && element <= MSE.mask7)
                {
                    mask[element - MSE.mask0] = packet.GetBit();
                    continue;
                }

                if (element >= MSE.Transportmask0 && element <= MSE.Transportmask7)
                {
                    if (mi.Status.HasTransportData)
                        tmask[element - MSE.Transportmask0] = packet.GetBit();
                    continue;
                }

                if (element >= MSE.GuidByte0 && element <= MSE.GuidByte7)
                {
                    int index = element - MSE.GuidByte0;
                    guid[index] = packet.ReadByteSeq(mask[index]);
                    continue;
                }

                if (element >= MSE.TransportGuidByte0 && element <= MSE.TransportGuidByte7)
                {
                    int index = element - MSE.TransportGuidByte0;
                    if (mi.Status.HasTransportData)
                        tguid[index] = packet.ReadByteSeq(tmask[index]);
                    continue;
                }

                switch (element)
                {
                    case MSE.Flags:
                        if (mi.Status.HasMovementFlags)
                            mi.Flags = (MovementFlag)packet.GetBits<uint>(30);
                        break;
                    case MSE.Flags2:
                        if (mi.Status.HasMovementFlags2)
                            mi.Flags2 = (MovementFlag2)packet.GetBits<uint>(12);
                        break;
                    case MSE.HaveUnknownBit:
                        mi.Status.HasUnknownBit = packet.ReadBit();
                        break;
                    case MSE.Timestamp:
                        if (mi.Status.HasTimeStamp)
                            mi.Time = packet.ReadUInt32();
                        break;
                    case MSE.HaveTimeStamp:
                        mi.Status.HasTimeStamp = !packet.ReadBit();
                        break;
                    case MSE.HaveOrientation:
                        mi.Status.HasOrientation = !packet.ReadBit();
                        break;
                    case MSE.HaveMovementFlags:
                        mi.Status.HasMovementFlags = !packet.ReadBit();
                        break;
                    case MSE.HaveMovementFlags2:
                        mi.Status.HasMovementFlags2 = !packet.ReadBit();
                        break;
                    case MSE.HavePitch:
                        mi.Status.HasPitch = !packet.ReadBit();
                        break;
                    case MSE.HaveFallData:
                        mi.Status.HasFallData = packet.ReadBit();
                        break;
                    case MSE.HaveFallDirection:
                        if (mi.Status.HasFallData)
                            mi.Status.HasFallDirection = packet.ReadBit();
                        break;
                    case MSE.HaveTransportData:
                        mi.Status.HasTransportData = packet.ReadBit();
                        break;
                    case MSE.TransportHaveTime2:
                        if (mi.Status.HasTransportData)
                            mi.Status.HasTransportTime2 = packet.ReadBit();
                        break;
                    case MSE.TransportHaveTime3:
                        if (mi.Status.HasTransportData)
                            mi.Status.HasTransportTime3 = packet.ReadBit();
                        break;
                    case MSE.HaveSpline:
                        mi.Status.HasSpline = packet.ReadBit();
                        break;
                    case MSE.HaveSplineElev:
                        mi.Status.HasSplineElevation = !packet.ReadBit();
                        break;
                    case MSE.PositionX:
                        mi.Pos.X = packet.ReadFloat();
                        break;
                    case MSE.PositionY:
                        mi.Pos.Y = packet.ReadFloat();
                        break;
                    case MSE.PositionZ:
                        mi.Pos.Z = packet.ReadFloat();
                        break;
                    case MSE.PositionO:
                        if (mi.Status.HasOrientation)
                            mi.Pos.SetOrientation(packet.ReadFloat());
                        break;
                    case MSE.Pitch:
                        if (mi.Status.HasPitch)
                            mi.Pitch = packet.ReadFloat();
                        break; ;
                    case MSE.FallTime:
                        if (mi.Status.HasFallData)
                            mi.FallTime = packet.ReadUInt32();
                        break;
                    case MSE.SplineElev:
                        if (mi.Status.HasSplineElevation)
                            mi.SplineElevation = packet.ReadFloat();
                        break;
                    case MSE.FallHorizontalSpeed:
                        if (mi.Status.HasFallData && mi.Status.HasFallDirection)
                            mi.Jump.xyspeed = packet.ReadFloat();
                        break;
                    case MSE.FallVerticalSpeed:
                        if (mi.Status.HasFallData)
                            mi.Jump.velocity = packet.ReadFloat();
                        break;
                    case MSE.FallCosAngle:
                        if (mi.Status.HasFallData && mi.Status.HasFallDirection)
                            mi.Jump.cosAngle = packet.ReadFloat();
                        break;
                    case MSE.FallSinAngle:
                        if (mi.Status.HasFallData && mi.Status.HasFallDirection)
                            mi.Jump.sinAngle = packet.ReadFloat();
                        break;
                    case MSE.TransportSeat:
                        if (mi.Status.HasTransportData)
                            mi.TransSeat = packet.ReadInt32();
                        break;
                    case MSE.TransportPositionO:
                        if (mi.Status.HasTransportData)
                            mi.TransPos.SetOrientation(packet.ReadFloat());
                        break;
                    case MSE.TransportPositionX:
                        if (mi.Status.HasTransportData)
                            mi.TransPos.X = packet.ReadFloat();
                        break;
                    case MSE.TransportPositionY:
                        if (mi.Status.HasTransportData)
                            mi.TransPos.Y = packet.ReadFloat();
                        break;
                    case MSE.TransportPositionZ:
                        if (mi.Status.HasTransportData)
                            mi.TransPos.Z = packet.ReadFloat();
                        break;
                    case MSE.TransportTime:
                        if (mi.Status.HasTransportData)
                            mi.TransTime = packet.ReadUInt32();
                        break;
                    case MSE.TransportTime2:
                        if (mi.Status.HasTransportData && mi.Status.HasTransportTime2)
                            mi.TransTime2 = packet.ReadUInt32();
                        break;
                    case MSE.TransportTime3:
                        if (mi.Status.HasTransportData && mi.Status.HasTransportTime3)
                            mi.FallTime = packet.ReadUInt32();
                        break;
                    //case MSE.HaveCount:
                        //mi
                    //default:
                    //WPError(false);
                }
            }

            mi.Guid = new ObjectGuid(guid);
            mi.TransGuid = new ObjectGuid(tguid);

            //if (HaveTransportData && mi.pos.X != mi.t_pos.X)
            //if (GetPlayer()->GetTransport())
            //GetPlayer()->GetTransport()->m_position = mi->pos;
        }

        public static bool fuzzyEq(double a, double b)
        {
            return (a == b) || (Math.Abs(a - b) <= double.Epsilon);
        }

        */
    }
}
