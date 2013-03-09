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

namespace Framework.Constants
{
    [Flags]
    public enum MovementFlag
    {
        None                 = 0x0,
        Forward              = 0x1,
        Backward             = 0x2,
        StrafeLeft           = 0x4,
        StrafeRight          = 0x8,
        Left                 = 0x10,
        Right                = 0x20,
        PitchUp              = 0x40,
        PitchDown            = 0x80,
        Walk                 = 0x100,
        DisableGravity       = 0x200,
        Root                 = 0x400,
        Falling              = 0x8000,
        FallingFar           = 0x1000,
        PendingStop          = 0x2000,
        PendingStrafeStop    = 0x4000,
        PendingForward       = 0x8000,
        PendingBackward      = 0x10000,
        PendingStrafeLeft    = 0x20000,
        PendingStrafeRight   = 0x40000,
        PendingRoot          = 0x80000,
        Swim                 = 0x100000,
        Ascend               = 0x200000,
        Descend              = 0x400000,
        CanFly               = 0x800000,
        Fly                  = 0x1000000,
        SplineElevation      = 0x2000000,
        WaterWalk            = 0x4000000,
        FallingSlow          = 0x8000000,
        Hover                = 0x10000000,
        Collision            = 0x20000000
    }

    [Flags]
    public enum MovementFlag2
    {
        None                                = 0x0,
        NoStrafe                            = 0x1,
        NoJumping                           = 0x2,
        DisableCollision                    = 0x4,
        FullSpeedTurning                    = 0x8,
        FullSpeedPitching                   = 0x10,
        AlwaysAllowPitching                 = 0x20,
        VehicleExitVoluntary                = 0x40,
        JumpSplineInAir                     = 0x80,
        AnimTierInTrans                     = 0x100,
        PreventChangePitch                  = 0x200,
        InterpolatedMovement                = 0x400,
        //InterpolatedTurning                 = 0x00000800,
        //InterpolatedPitching                = 0x00001000,
        VehiclePassengerIsTransitionAllowed = 0x2000,
        CanTransitionBetweenSwimAndFly      = 0x4000,
        //Unk10                               = 0x00008000
    }
}
