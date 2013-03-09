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

namespace WorldServer.Game.Movement
{
    public class MoveSplineInitArgs
    {
        public MoveSplineInitArgs(int path_capacity = 16)
        {
            path_Idx_offset = 0;
            velocity = 0.0f;
            parabolic_amplitude = 0.0f;
            time_perc = 0.0f;
            splineId = 0;
            initialOrientation = 0.0f;
            HasVelocity = false;
            TransformForTransport = true;
            path = new Vector3[path_capacity];
        }
        public Vector3[] path;
        public FacingInfo facing;
        public MoveSplineFlag flags = new MoveSplineFlag();
        public int path_Idx_offset;
        public float velocity;
        public float parabolic_amplitude;
        public float time_perc;
        public uint splineId;
        public float initialOrientation;
        public bool HasVelocity;
        public bool TransformForTransport;

        /** Returns true to show that the arguments were configured correctly and MoveSpline initialization will succeed. */
        public bool Validate(Unit unit) { return true; }

        bool _checkPathBounds() { return false; }
    }
}
