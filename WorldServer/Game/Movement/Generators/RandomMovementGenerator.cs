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
using WorldServer.Game.WorldEntities;
using Framework.Utility;
using Framework.Constants;
using WorldServer.Game.Maps;

namespace WorldServer.Game.Movement.Generators
{
    public class RandomMovementGenerator<T> : MovementGeneratorMedium<T> where T : Creature
    {
        public RandomMovementGenerator(float spawn_dist = 0.0f)
        {
            i_nextMoveTime = new TimeTrackerSmall();
            wander_distance = spawn_dist;
        }

        TimeTrackerSmall i_nextMoveTime;
        //uint i_nextMove;
        float wander_distance;

        public override MovementGeneratorType GetMovementGeneratorType()
        {
            throw new NotImplementedException();
        }
        public override void unitSpeedChanged()
        {
            throw new NotImplementedException();
        }

        public override void DoInitialize(T creature)
        {
            if (!creature.isAlive())
                return;

            //if (wander_distance == 0)
                //wander_distance = creature.GetRespawnRadius();

            creature.AddUnitState(UnitState.Roaming | UnitState.Roaming_Move);
            _setRandomLocation(creature);

        }
        public override void DoFinalize(T creature)
        {
            creature.ClearUnitState(UnitState.Roaming | UnitState.Roaming_Move);
            creature.SetWalk(false);
        }
        public override void DoReset(T creature)
        {
            DoInitialize(creature);
        }

        public override bool DoUpdate(T creature, uint diff)
        {
            if (creature.HasUnitState(UnitState.Root | UnitState.Stunned | UnitState.Distracted))
            {
                i_nextMoveTime.Reset(0);  // Expire the timer
                creature.ClearUnitState(UnitState.Roaming_Move);
                return true;
            }

            if (creature.movespline.Finalized())
            {
                i_nextMoveTime.Update((int)diff);
                if (i_nextMoveTime.Passed())
                    _setRandomLocation(creature);
            }
            return true;
        }

        public void _setRandomLocation(T creature)
        {
            float respX, respY, respZ, respO, destX, destY, destZ, travelDistZ;
            creature.GetHomePosition(out respX, out respY, out respZ, out respO);
            Map map = creature.GetMap();//GetBaseMap();

            // For 2D/3D system selection
            //bool is_land_ok  = creature.CanWalk();                // not used?
            //bool is_water_ok = creature.CanSwim();                // not used?
            bool is_air_ok = creature.CanFly();

            float angle = (float)(RandomHelper.rand_norm() * (Math.PI * 2.0f));
            float range = (float)(RandomHelper.rand_norm() * wander_distance);
            float distanceX = (float)(range * Math.Cos(angle));
            float distanceY = (float)(range * Math.Sin(angle));

            destX = respX + distanceX;
            destY = respY + distanceY;

            // prevent invalid coordinates generation
            //NormalizeMapCoord(destX);
            //NormalizeMapCoord(destY);

            travelDistZ = distanceX * distanceX + distanceY * distanceY;

            if (is_air_ok)                                          // 3D system above ground and above water (flying mode)
            {
                // Limit height change
                float distanceZ = (float)(RandomHelper.rand_norm() * Math.Sqrt(travelDistZ) / 2.0f);
                destZ = respZ + distanceZ;
                float Null;
                float levelZ = map.GetWaterOrGroundLevel(destX, destY, destZ - 2.0f, out Null);

                // Problem here, we must fly above the ground and water, not under. Let's try on next tick
                if (levelZ >= destZ)
                    return;
            }
            //else if (is_water_ok)                                 // 3D system under water and above ground (swimming mode)
            else                                                    // 2D only
            {
                // 10.0 is the max that vmap high can check (MAX_CAN_FALL_DISTANCE)
                travelDistZ = (float)(travelDistZ >= 100.0f ? 10.0f : Math.Sqrt(travelDistZ));

                // The fastest way to get an accurate result 90% of the time.
                // Better result can be obtained like 99% accuracy with a ray light, but the cost is too high and the code is too long.
                destZ = map.GetHeight(creature.GetPhaseMask(), destX, destY, respZ + travelDistZ - 2.0f, false);

                if (Math.Abs(destZ - respZ) > travelDistZ)              // Map check
                {
                    // Vmap Horizontal or above
                    destZ = map.GetHeight(creature.GetPhaseMask(), destX, destY, respZ - 2.0f, true);

                    if (Math.Abs(destZ - respZ) > travelDistZ)
                    {
                        // Vmap Higher
                        destZ = map.GetHeight(creature.GetPhaseMask(), destX, destY, respZ + travelDistZ - 2.0f, true);

                        // let's forget this bad coords where a z cannot be find and retry at next tick
                        if (Math.Abs(destZ - respZ) > travelDistZ)
                            return;
                    }
                }
            }

            if (is_air_ok)
                i_nextMoveTime.Reset(0);
            else
                i_nextMoveTime.Reset(RandomHelper.irand(500, 10000));

            creature.AddUnitState(UnitState.Roaming_Move);

            MoveSplineInit init = new MoveSplineInit(creature);
            init.MoveTo(destX, destY, destZ);
            init.SetWalk(true);
            init.Launch();

            //Call for creature group update
            //if (creature.GetFormation() && creature.GetFormation().getLeader() == creature)
            //creature.GetFormation().LeaderMoveTo(destX, destY, destZ);
        }

        public bool GetResetPosition(Creature creature, float x, float y, float z)
        {
            float radius;
            float Null;
            creature.GetRespawnPosition(out x, out y, out z, out Null, out radius);

            // use current if in range
            //if (creature.IsWithinDist2d(x, y, radius))
            //creature.GetPosition(x, y, z);

            return true;
        }
    }
}
