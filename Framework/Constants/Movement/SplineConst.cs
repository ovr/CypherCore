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

namespace Framework.Constants
{
    class SplineConst
    {
    }

    public enum MonsterMoveType
    {
        Normal = 0,
        Stop = 1,
        FacingSpot = 2,
        FacingTarget = 3,
        FacingAngle = 4
    }
    public enum SplineFlag : uint
    {
        None = 0x00000000,
        Forward = 0x00000001,
        Backward = 0x00000002,
        StrafeLeft = 0x00000004,
        Straferight = 0x00000008,
        TurnLeft = 0x00000010,
        TurnRight = 0x00000020,
        PitchUp = 0x00000040,
        PitchDown = 0x00000080,
        Done = 0x00000100,
        Falling = 0x00000200,
        NoSpline = 0x00000400,
        Trajectory = 0x00000800,
        WalkMode = 0x00001000,
        Flying = 0x00002000,
        Knockback = 0x00004000,
        FinalPoint = 0x00008000,
        FinalTarget = 0x00010000,
        FinalOrientation = 0x00020000,
        CatmullRom = 0x00040000,
        Cyclic = 0x00080000,
        EnterCicle = 0x00100000,
        AnimationTier = 0x00200000,
        Frozen = 0x00400000,
        Transport = 0x00800000,
        TransportExit = 0x01000000,
        Unknown7 = 0x02000000,
        Unknown8 = 0x04000000,
        OrientationInverted = 0x08000000,
        UsePathSmoothing = 0x10000000,
        Animation = 0x20000000,
        UncompressedPath = 0x40000000,
        Unknown10 = 0x80000000
    }
    public enum MovementSlot
    {
        IDLE,
        ACTIVE,
        CONTROLLED,
        Max
    }

    public enum MovementGeneratorType
    {
        IDLE_MOTION_TYPE = 0,                              // IdleMovementGenerator.h
        RANDOM_MOTION_TYPE = 1,                              // RandomMovementGenerator.h
        WAYPOINT_MOTION_TYPE = 2,                              // WaypointMovementGenerator.h
        MAX_DB_MOTION_TYPE = 3,                              // *** this and below motion types can't be set in DB.
        ANIMAL_RANDOM_MOTION_TYPE = MAX_DB_MOTION_TYPE,         // AnimalRandomMovementGenerator.h
        CONFUSED_MOTION_TYPE = 4,                              // ConfusedMovementGenerator.h
        CHASE_MOTION_TYPE = 5,                              // TargetedMovementGenerator.h
        HOME_MOTION_TYPE = 6,                              // HomeMovementGenerator.h
        FLIGHT_MOTION_TYPE = 7,                              // WaypointMovementGenerator.h
        POINT_MOTION_TYPE = 8,                              // PointMovementGenerator.h
        FLEEING_MOTION_TYPE = 9,                              // FleeingMovementGenerator.h
        DISTRACT_MOTION_TYPE = 10,                             // IdleMovementGenerator.h
        ASSISTANCE_MOTION_TYPE = 11,                             // PointMovementGenerator.h (first part of flee for assistance)
        ASSISTANCE_DISTRACT_MOTION_TYPE = 12,                   // IdleMovementGenerator.h (second part of flee for assistance)
        TIMED_FLEEING_MOTION_TYPE = 13,                         // FleeingMovementGenerator.h (alt.second part of flee for assistance)
        FOLLOW_MOTION_TYPE = 14,
        ROTATE_MOTION_TYPE = 15,
        EFFECT_MOTION_TYPE = 16,
        NULL_MOTION_TYPE = 17
    };
}
