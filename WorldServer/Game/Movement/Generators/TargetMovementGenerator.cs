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
using Framework.Constants;
using Framework.Utility;
using Framework.Graphics;

namespace WorldServer.Game.Movement
{
    public abstract class TargetedMovementGeneratorMedium<T, D> : MovementGeneratorMedium<T> where T : Unit

    {
        public TargetedMovementGeneratorMedium(Unit target, float offset = 0, float angle = 0)
        {
            i_recheckDistance = new TimeTrackerSmall();
            i_target = target;//.link(target, this);
            i_offset = offset;
            i_angle = angle;
            i_recalculateTravel = false;
            i_targetReached = false;
        }
        ~TargetedMovementGeneratorMedium() {}

        public override bool DoUpdate(T owner, uint time_diff)
        {
            if (!i_target.IsInWorld) 
                return false;

            if (!owner.isAlive())
                return false;

            if (owner.HasUnitState(UnitState.Not_Move))
            {
                _clearUnitStateMove(owner);
                return true;
            }

            // prevent movement while casting spells with cast time or channel time
            if (owner.HasUnitState(UnitState.Casting))
            {
                if (!owner.IsStopped())
                owner.StopMoving();
                return true;
            }

            // prevent crash after creature killed pet
            if (_lostTarget(owner))
            {
                _clearUnitStateMove(owner);
                return true;
            }

            i_recheckDistance.Update((int)time_diff);
            if (i_recheckDistance.Passed())
            {
                i_recheckDistance.Reset(50);
                //More distance let have better performance, less distance let have more sensitive reaction at target move.
                float allowed_dist = i_target.GetObjectSize() + owner.GetObjectSize() + ObjectConst.MeleeRange - 0.5f;
                float dest = (owner.movespline.FinalDestination() - new Vector3(i_target.GetPositionX(), i_target.GetPositionY(), i_target.GetPositionZ())).Length();
                if (dest >= allowed_dist * allowed_dist)
                    _setTargetLocation(owner);
            }

            if (owner.movespline.Finalized())
            {
                MovementInform(owner);
                if (i_angle == 0.0f && !owner.HasInArc(0.01f, i_target))
                    owner.SetInFront(i_target);

                if (!i_targetReached)
                {
                    i_targetReached = true;
                    _reachTarget(owner);
                }
            }
            else
            {
                if (i_recalculateTravel)
                    _setTargetLocation(owner);
            }
            return true;
        }
        public override void unitSpeedChanged() { i_recalculateTravel = true; }
        public void _setTargetLocation(T owner)
        {
            if (!i_target.IsInWorld)
                return;

            if (owner.HasUnitState(UnitState.Not_Move))
                return;

            float x, y, z;

            if (i_offset == 0)
            {
                if (i_target.IsWithinMeleeRange(owner))
                return;

                // to nearest random contact position
                i_target.GetRandomContactPoint(owner, out x, out y, out z, 0, ObjectConst.MeleeRange - 0.5f);
            }
            else
            {
                float dist = 0;
                float size = 0;

                // Pets need special handling.
                // We need to subtract GetObjectSize() because it gets added back further down the chain
                //  and that makes pets too far away. Subtracting it allows pets to properly
                //  be (GetCombatReach() + i_offset) away.
                if (owner.isPet())
                {
                    dist = i_target.GetCombatReach();
                    size = i_target.GetCombatReach() - i_target.GetObjectSize();
                }
                else
                {
                    dist = i_offset + 1.0f;
                    size = owner.GetObjectSize();
                }

                if (i_target.IsWithinDistInMap(owner, dist))
                    return;

                // to at i_offset distance from target and i_angle from target facing
                i_target.GetClosePoint(out x, out y, out z, size, i_offset, i_angle);
            }

            _addUnitStateMove(owner);
            i_targetReached = false;
            i_recalculateTravel = false;
            owner.AddUnitState(UnitState.Chase);

            MoveSplineInit init = new MoveSplineInit(owner);
            init.MoveTo(x, y, z);
            init.SetWalk(EnableWalking());
            init.Launch();
        }
        public void UpdateFinalDistance(float fDistance)
        {
            if (typeof(T) == typeof(Player))
                return;
            i_offset = fDistance;
            i_recalculateTravel = true;
        }

        public abstract void MovementInform(T unit);
        public abstract bool _lostTarget(T u);
        public abstract void _clearUnitStateMove(T u);
        public abstract void _addUnitStateMove(T u);
        public abstract void _reachTarget(T owner);
        public abstract bool EnableWalking();
        public abstract void _updateSpeed(T u);

        #region Fields
        TimeTrackerSmall i_recheckDistance;
        float i_offset;
        float i_angle;
        public bool i_recalculateTravel;
        bool i_targetReached;
        public Unit i_target;
        #endregion
    }

    public class ChaseMovementGenerator<T> : TargetedMovementGeneratorMedium<T, ChaseMovementGenerator<T>> where T : Unit
    {
        public ChaseMovementGenerator(Unit target) 
            : base(target) 
        {
        }
        public ChaseMovementGenerator(Unit target, float offset, float angle)
            : base(target, offset, angle)
        {
        }

        public override MovementGeneratorType GetMovementGeneratorType() { return MovementGeneratorType.CHASE_MOTION_TYPE; }

        public override void DoInitialize(T owner)
        {
            if (owner is Player)
                owner.AddUnitState(UnitState.Chase | UnitState.Chase_Move);
            else
            {
                owner.SetWalk(false);
                owner.AddUnitState(UnitState.Chase | UnitState.Chase_Move);
            } 
            _setTargetLocation(owner);
        }
        public override void DoFinalize(T owner) { owner.ClearUnitState(UnitState.Chase | UnitState.Chase_Move); }
        public override void DoReset(T owner) { DoInitialize(owner); }
        public override bool _lostTarget(T u) { return u.getVictim() != i_target; }
        public override void _clearUnitStateMove(T u) { u.ClearUnitState(UnitState.Chase_Move); }
        public override void _addUnitStateMove(T u)  { u.AddUnitState(UnitState.Chase_Move); }

        public override bool EnableWalking() { return false; }
        public override void _updateSpeed(T u) { }
        public override void _reachTarget(T owner)
        {
            if (owner.IsWithinMeleeRange(this.i_target))
                owner.Attack(i_target, true);
        }        
        public override void MovementInform(T unit)
        {
            if (unit is Creature)
            {
                // Pass back the GUIDLow of the target. If it is pet's owner then PetAI will handle
                if ((unit as Creature).AI() != null)
                    (unit as Creature).AI().MovementInform(MovementGeneratorType.CHASE_MOTION_TYPE, i_target.GetGUIDLow());
            }
        }
    }

    public class FollowMovementGenerator<T> : TargetedMovementGeneratorMedium<T, FollowMovementGenerator<T>> where T : Unit
    {
        public FollowMovementGenerator(Unit target)
            : base(target){}
        public FollowMovementGenerator(Unit target, float offset, float angle)
            : base(target, offset, angle) {}

        public override MovementGeneratorType GetMovementGeneratorType() { return MovementGeneratorType.FOLLOW_MOTION_TYPE; }
        public override void _clearUnitStateMove(T u) { u.ClearUnitState(UnitState.Follow_Move); }
        public override void _addUnitStateMove(T u)  { u.AddUnitState(UnitState.Follow_Move); }
        public override void DoReset(T owner) { DoInitialize(owner); }
        public override bool _lostTarget(T u) { return false; }
        public override void _reachTarget(T u) {}

        public override void DoInitialize(T owner)
        {
            owner.AddUnitState(UnitState.Follow | UnitState.Follow_Move);
            _updateSpeed(owner);
            _setTargetLocation(owner);
        }
        public override void DoFinalize(T owner)
        {
            owner.ClearUnitState(UnitState.Follow | UnitState.Follow_Move);
            _updateSpeed(owner);
        }
        public override void MovementInform(T unit)
        {
            if (unit is Player)
                return;

            // Pass back the GUIDLow of the target. If it is pet's owner then PetAI will handle
            if ((unit as Creature).AI() != null)
                (unit as Creature).AI().MovementInform(MovementGeneratorType.FOLLOW_MOTION_TYPE, i_target.GetGUIDLow());
        }
        public override bool EnableWalking()
        {
            if (typeof(T) == typeof(Player))
                return false;
            else
                return i_target.IsWalking();
        }
        public override void _updateSpeed(T u)
        {
            if (u is Player)
                return;

            if (!u.isPet() || i_target.GetGUID() != u.GetOwnerGUID())
                return;
            u.UpdateSpeed(UnitMoveType.Run, true);
            u.UpdateSpeed(UnitMoveType.Walk, true);
            u.UpdateSpeed(UnitMoveType.Swim, true);
        }
    }
}
