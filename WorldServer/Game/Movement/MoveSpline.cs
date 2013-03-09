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
using Framework.Logging;

namespace WorldServer.Game.Movement
{
    public class MoveSpline
    {
        public MoveSpline()
        {
            m_Id = 0;
            time_passed = 0;
            vertical_acceleration = 0.0f;
            initialOrientation = 0.0f;
            effect_start_time = 0;
            point_Idx = 0;
            point_Idx_offset = 0;
            splineflags.done = true;
        }
        public Vector3[] getPath() { return spline.getPoints(); }
        public int timePassed() { return time_passed; }
        public void Initialize(MoveSplineInitArgs args)
        {
            splineflags = args.flags;
            facing = args.facing;
            m_Id = args.splineId;
            point_Idx_offset = args.path_Idx_offset;
            initialOrientation = args.initialOrientation;

            onTransport = false;
            time_passed = 0;
            vertical_acceleration = 0.0f;
            effect_start_time = 0;

            // Check if its a stop spline
            if (args.flags.done)
            {
                spline.clear();
                return;
            }

            init_spline(args);

            // init parabolic / animation
            // spline initialized, duration known and i able to compute parabolic acceleration
            if (Convert.ToBoolean(args.flags.Raw() & (MoveSplineFlag.eFlags.Parabolic | MoveSplineFlag.eFlags.Animation)))
            {
                effect_start_time = (int)(Duration() * args.time_perc);
                if (args.flags.parabolic && effect_start_time < Duration())
                {
                    float f_duration = (float)TimeSpan.FromMilliseconds(Duration() - effect_start_time).TotalSeconds;
                    vertical_acceleration = args.parabolic_amplitude * 8.0f / (f_duration * f_duration);
                }
            }
        }
        void init_spline(MoveSplineInitArgs args)
        {
            Spline.EvaluationMode[] modes = new Spline.EvaluationMode[2] { Spline.EvaluationMode.Linear, Spline.EvaluationMode.Catmullrom };
            if (args.flags.cyclic)
            {
                int cyclic_point = 0;
                // MoveSplineFlag::Enter_Cycle support dropped
                //if (splineflags & SPLINEFLAG_ENTER_CYCLE)
                //cyclic_point = 1;   // shouldn't be modified, came from client
                spline.Init_Spline(args.path, args.path.Length, modes[Convert.ToInt32(args.flags.isSmooth())], cyclic_point);
            }
            else
            {
                spline.Init_Spline(args.path, args.path.Length, modes[Convert.ToInt32(args.flags.isSmooth())]);
            }

            // init spline timestamps
            if (splineflags.falling)
            {
                //FallInitializer init(spline.getPoint(spline.first()).z);
                //spline.initLengths(init);
            }
            else
            {
                CommonInitializer init = new CommonInitializer(args.velocity);
                spline.initLengths(init);
            }

            // TODO: what to do in such cases? problem is in input data (all points are at same coords)
            if (spline.length() < 1)
            {
                Log.outError("MoveSpline->init_spline: zero length spline, wrong input data?");
                spline.set_length(spline.last(), spline.isCyclic() ? 1000 : 1);
            }
            point_Idx = spline.first();
        }
        public int Duration() { return spline.length(); }
        public uint GetId() { return m_Id; }
        public bool Finalized() { return splineflags.done; }
        void _Finalize()
        {
            splineflags.done = true;
            point_Idx = spline.last() - 1;
            time_passed = Duration();
        }
        public Vector4 ComputePosition()
        {
            float u = 1.0f;
            int seg_time = spline.length(point_Idx, point_Idx + 1);
            if (seg_time > 0)
                u = (time_passed - spline.length(point_Idx)) / (float)seg_time;

            Vector3 c;
            spline.Evaluate_Percent(point_Idx, u, out c);
            float orientation = initialOrientation;

            //if (splineflags.animation)
            // MoveSplineFlag::Animation disables falling or parabolic movement
            //if (splineflags.parabolic)
            //computeParabolicElevation(c.z);
            //else if (splineflags.falling)
            //computeFallElevation(c.z);

            if (splineflags.done && splineflags.isFacing())
            {
                if (splineflags.final_angle)
                    orientation = facing.angle;
                else if (splineflags.final_point)
                    orientation = (float)Math.Atan2(facing.y - c.Y, facing.x - c.X);
                //nothing to do for MoveSplineFlag::Final_Target flag
            }
            else
            {
                if (!Convert.ToBoolean(splineflags.Raw() & (MoveSplineFlag.eFlags.OrientationFixed | MoveSplineFlag.eFlags.Falling | MoveSplineFlag.eFlags.Unknown0)))
                {
                    Vector3 hermite;
                    spline.Evaluate_Derivative(point_Idx, u, out hermite);
                    orientation = (float)Math.Atan2(hermite.Y, hermite.X);
                }

                if (splineflags.orientationInversed)
                    orientation = -orientation;
            }
            
            return new Vector4(c, orientation);
        }
        public void _Interrupt() { splineflags.done = true; }
        public void updateState(int difftime)
        {
            do
            {
                _updateState(ref difftime);
            } while (difftime > 0);
        }
        UpdateResult _updateState(ref int ms_time_diff)
        {
            if (Finalized())
            {
                ms_time_diff = 0;
                return UpdateResult.Arrived;
            }

            UpdateResult result = UpdateResult.None;
            int blah = segment_time_elapsed();
            int minimal_diff = (int)Math.Min(ms_time_diff, segment_time_elapsed());
            //if (minimal_diff >= 0)
                //Log.outError("Should be negivite.");
            time_passed += minimal_diff;
            ms_time_diff -= minimal_diff;

            if (time_passed >= next_timestamp())
            {
                ++point_Idx;
                if (point_Idx < spline.last())
                {
                    result = UpdateResult.NextSegment;
                }
                else
                {
                    if (spline.isCyclic())
                    {
                        point_Idx = spline.first();
                        time_passed = time_passed % Duration();
                        result = UpdateResult.NextCycle;
                    }
                    else
                    {
                        _Finalize();
                        ms_time_diff = 0;
                        result = UpdateResult.Arrived;
                    }
                }
            }

            return result;
        }
        int next_timestamp() { return spline.length(point_Idx+1); }
        int segment_time_elapsed() { return next_timestamp()-time_passed; }
        public bool isCyclic() { return splineflags.cyclic; }

        bool Initialized() { return !spline.empty(); }
        public Vector3 FinalDestination() { return Initialized() ? spline.getPoint(spline.last()) : new Vector3(); }

        #region Fields
        public MoveSplineInitArgs InitArgs;
        public Spline spline = new Spline();
        public FacingInfo facing;
        public MoveSplineFlag splineflags = new MoveSplineFlag();
        public bool onTransport;
        uint m_Id;
        int time_passed;
        public float vertical_acceleration;
        float initialOrientation;
        public int effect_start_time;
        int point_Idx;
        int point_Idx_offset;
        #endregion

        public class CommonInitializer
        {
            public CommonInitializer(float _velocity)
            {
                velocityInv = 1000f / _velocity;
                time = 1;
            }
            public float velocityInv;
            public int time;
            public int SetGetTime(Spline s, int i)
            {
                time += (int)(s.SegLength(i) * velocityInv);
                return time;
            }
        }
        public enum UpdateResult
        {
            None         = 0x01,
            Arrived      = 0x02,
            NextCycle    = 0x04,
            NextSegment  = 0x08
        }
    }
}
