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

namespace WorldServer.Game.Movement
{
    public class MoveSplineFlag
    {
        [Flags]
        public enum eFlags : uint
        {
            None = 0x00000000,
            // x00-x07 used as animation Ids storage in pair with Animation flag
            Unknown0 = 0x00000008,           // NOT VERIFIED
            FallingSlow = 0x00000010,
            Done = 0x00000020,
            Falling = 0x00000040,           // Affects elevation computation, can't be combined with Parabolic flag
            No_Spline = 0x00000080,
            Unknown2 = 0x00000100,           // NOT VERIFIED
            Flying = 0x00000200,           // Smooth movement(Catmullrom interpolation mode), flying animation
            OrientationFixed = 0x00000400,           // Model orientation fixed
            Catmullrom = 0x00000800,           // Used Catmullrom interpolation mode
            Cyclic = 0x00001000,           // Movement by cycled spline
            Enter_Cycle = 0x00002000,           // Everytimes appears with cyclic flag in monster move packet, erases first spline vertex after first cycle done
            Frozen = 0x00004000,           // Will never arrive
            TransportEnter = 0x00008000,
            TransportExit = 0x00010000,
            Unknown3 = 0x00020000,           // NOT VERIFIED
            Unknown4 = 0x00040000,           // NOT VERIFIED
            OrientationInversed = 0x00080000,
            SmoothGroundPath = 0x00100000,
            Walkmode = 0x00200000,
            UncompressedPath = 0x00400000,
            Unknown6 = 0x00800000,           // NOT VERIFIED
            Animation = 0x01000000,           // Plays animation after some time passed
            Parabolic = 0x02000000,           // Affects elevation computation, can't be combined with Falling flag
            Final_Point = 0x04000000,
            Final_Target = 0x08000000,
            Final_Angle = 0x10000000,
            Unknown7 = 0x20000000,           // NOT VERIFIED
            Unknown8 = 0x40000000,           // NOT VERIFIED
            Unknown9 = 0x80000000,           // NOT VERIFIED

            // Masks
            Mask_Final_Facing = Final_Point | Final_Target | Final_Angle,
            // animation ids stored here, see AnimType enum, used with Animation flag
            Mask_Animations = 0x7,
            // flags that shouldn't be appended into SMSG_MONSTER_MOVE\SMSG_MONSTER_MOVE_TRANSPORT packet, should be more probably
            Mask_No_Monster_Move = Mask_Final_Facing | Mask_Animations | Done,
            // Unused, not suported flags
            Mask_Unused = No_Spline | Enter_Cycle | Frozen | Unknown0 | Unknown2 | Unknown3 | Unknown4 | Unknown6 | Unknown7 | Unknown8 | Unknown9
        }

        public eFlags Raw()
        {
            return raw;
        }

        public MoveSplineFlag() { raw = 0; }
        public MoveSplineFlag(uint f) { raw = (eFlags)f; }
        public MoveSplineFlag(MoveSplineFlag f) { raw = f.raw; }

        public bool isSmooth() { return Convert.ToBoolean(raw & eFlags.Catmullrom); }
        public bool isLinear() { return !isSmooth(); }
        public bool isFacing() { return Convert.ToBoolean(raw & eFlags.Mask_Final_Facing); }

        public byte getAnimationId() { return animId; }
        public bool hasAllFlags(uint f) { return (raw & (eFlags)f) == (eFlags)f; }
        public bool hasFlag(uint f) { return (raw & (eFlags)f) != 0; }
        //std::string ToString();

        public void EnableAnimation(uint anim) { raw = (raw & ~(eFlags.Mask_Animations | eFlags.Falling | eFlags.Parabolic | eFlags.FallingSlow)) | eFlags.Animation | ((eFlags)anim & eFlags.Mask_Animations); }
        public void EnableParabolic() { raw = (raw & ~(eFlags.Mask_Animations | eFlags.Falling | eFlags.Animation | eFlags.FallingSlow)) | eFlags.Parabolic; }
        public void EnableFalling() { raw = (raw & ~(eFlags.Mask_Animations | eFlags.Parabolic | eFlags.Animation)) | eFlags.Falling; }
        public void EnableCatmullRom() { raw = (raw & ~eFlags.SmoothGroundPath) | eFlags.Catmullrom | eFlags.UncompressedPath; }
        public void EnableFacingPoint() { raw = (raw & ~eFlags.Mask_Final_Facing) | eFlags.Final_Point; }
        public void EnableFacingAngle() { raw = (raw & ~eFlags.Mask_Final_Facing) | eFlags.Final_Angle; }
        public void EnableFacingTarget() { raw = (raw & ~eFlags.Mask_Final_Facing) | eFlags.Final_Target; }
        public void EnableTransportEnter() { raw = (raw & ~eFlags.TransportExit) | eFlags.TransportEnter; }
        public void EnableTransportExit() { raw = (raw & ~eFlags.TransportEnter) | eFlags.TransportExit; }

        private eFlags raw;

        public byte animId = 0;
        public bool unknown0 = false;
        public bool fallingSlow = false;
        public bool done = false;
        public bool falling = false;
        public bool no_spline = false;
        public bool unknown2 = false;
        public bool flying = false;
        public bool orientationFixed = false;
        public bool catmullrom = false;
        public bool cyclic = false;
        public bool enter_cycle = false;
        public bool frozen = false;
        public bool transportEnter = false;
        public bool transportExit = false;
        public bool unknown3 = false;
        public bool unknown4 = false;
        public bool orientationInversed = false;
        public bool smoothGroundPath = false;
        public bool walkmode = false;
        public bool uncompressedPath = false;
        public bool unknown6 = false;
        public bool animation = false;
        public bool parabolic = false;
        public bool final_point = false;
        public bool final_target = false;
        public bool final_angle = false;
        public bool unknown7 = false;
        public bool unknown8 = false;
        public bool unknown9 = false;
    }
}
