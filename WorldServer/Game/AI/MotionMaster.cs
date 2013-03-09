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

using Framework.Constants;
using Framework.Logging;
using WorldServer.Game.WorldEntities;
using WorldServer.Game.Movement;
using System;
using System.Collections.Generic;
using Framework.Threading.Actors;

namespace WorldServer.Game.AI
{
    public class MotionMaster
    {
        public MotionMaster(Unit unit)
        {
            Impl = new MovementGenerator[(int)MovementSlot.Max];
            _needInit = new bool[(int)MovementSlot.Max];
            _expList = null;
            _top = -1;
            _cleanFlag = MMCleanFlag.NONE;
            _owner = unit;
            for (byte i = 0; i < (int)MovementSlot.Max; ++i)
            {
                Impl[i] = null;
                _needInit[i] = true;
            }
        }
        ~MotionMaster()
        {
            while (!empty())
            {
                MovementGenerator curr = top();
                pop();
                if (curr != null) DirectDelete(curr);
            }
        }

        public void Initialize()
        {
            // clear ALL movement generators (including default)
            while (!empty())
            {
                MovementGenerator curr = top();
                pop();
                if (curr != null)
                    DirectDelete(curr);
            }

            InitDefault();
        }
        void InitDefault()
        {
            if (_owner.GetTypeId() == ObjectType.Unit)
            {
                //MovementGenerator movement = FactorySelector::selectMovementGenerator(_owner->ToCreature());
                //Mutate(movement == NULL ? &si_idleMovement : movement, MOTION_SLOT_IDLE);
            }
            else
            {
                //Mutate(&si_idleMovement, MOTION_SLOT_IDLE);
            }
        }

        void pop()
        {
            Impl[_top] = null;
            while (top() != null)
                --_top;       
        }
        void InitTop()
        {
            top().Initialize(_owner);
            _needInit[_top] = false;
        }

        public void UpdateMotion(uint diff)
        {
            lock (locker)
            {
                if (_owner == null)
                    return;

                if (_owner.HasUnitState(UnitState.Root | UnitState.Stunned)) // what about UNIT_STATE_DISTRACTED? Why is this not included?
                    return;

                _cleanFlag |= MMCleanFlag.UPDATE;
                if (top() != null && !top().Update(_owner, diff))
                {
                    _cleanFlag &= ~MMCleanFlag.UPDATE;
                    MovementExpired();
                }
                else
                    _cleanFlag &= ~MMCleanFlag.UPDATE;

                if (_expList != null)
                {
                    for (var i = 0; i < _expList.Count; ++i)
                    {
                        MovementGenerator mg = _expList[i];
                        DirectDelete(mg);
                    }

                    _expList = null;

                    if (empty())
                        Initialize();
                    else if (needInitTop())
                        InitTop();
                    else if (Convert.ToBoolean(_cleanFlag & MMCleanFlag.RESET))
                        top().Reset(_owner);

                    _cleanFlag &= ~MMCleanFlag.RESET;
                }
            }
            // probably not the best place to pu this but im not really sure where else to put it.
            //_owner->UpdateUnderwaterState(_owner->GetMap(), _owner->GetPositionX(), _owner->GetPositionY(), _owner->GetPositionZ());
        }

        bool isStatic(MovementGenerator mv) { return false;/*(mv == &si_idleMovement);*/}
        bool empty() { return (_top < 0); }
        int size() { return _top + 1; }
        MovementGenerator top() { return _top == -1 ? null : Impl[_top]; }
        MovementGenerator GetMotionSlot(int slot) { return Impl[slot]; }
        bool needInitTop() { return _needInit[_top]; }

        void DelayedDelete(MovementGenerator curr)
        {
            Log.outError("Unit (Entry {0}) is trying to delete its updating MG (Type {1})!", _owner.GetEntry(), 
                curr.GetMovementGeneratorType());
            if (isStatic(curr))
                return;
            if (_expList == null)
                _expList = new List<MovementGenerator>();
            _expList.Add(curr);
        }
        void DirectDelete(MovementGenerator curr)
        {
            if (isStatic(curr))
                return;
            curr.Finalize(_owner);
        }

        void MovementExpired(bool reset = true)
        {
            if (Convert.ToBoolean(_cleanFlag & MMCleanFlag.UPDATE))
            {
                if (reset)
                    _cleanFlag |= MMCleanFlag.RESET;
                else
                    _cleanFlag &= ~MMCleanFlag.RESET;
                DelayedExpire();
            }
            else
                DirectExpire(reset);
        }
        void DirectExpire(bool reset)
        {
            if (size() > 1)
            {
                MovementGenerator curr = top();
                pop();
                DirectDelete(curr);
            }

            while (top() == null)
                --_top;

            if (empty())
                Initialize();
            else if (needInitTop())
                InitTop();
            else if (reset)
                top().Reset(_owner);
        }
        void DelayedExpire()
        {
            if (size() > 1)
            {
                MovementGenerator curr = top();
                pop();
                DelayedDelete(curr);
            }

            while (top() == null)
                --_top;
        }

        void Clear(bool reset = true)
        {
            if (Convert.ToBoolean(_cleanFlag & MMCleanFlag.UPDATE))
            {
                if (reset)
                    _cleanFlag |= MMCleanFlag.RESET;
                else
                    _cleanFlag &= ~MMCleanFlag.RESET;
                DelayedClean();
            }
            else
                DirectClean(reset);
        }
        void DirectClean(bool reset)
        {
            while (size() > 1)
            {
                MovementGenerator curr = top();
                pop();
                if (curr != null) DirectDelete(curr);
            }

            if (needInitTop())
                InitTop();
            else if (reset)
                top().Reset(_owner);
        }
        void DelayedClean()
        {
            while (size() > 1)
            {
                MovementGenerator curr = top();
                pop();
                if (curr != null)
                    DelayedDelete(curr);
            }
        }

        void Mutate(MovementGenerator m, MovementSlot _slot)
        {
            int slot = (int)_slot;
            MovementGenerator curr = Impl[slot];
            if (curr != null)
            {
                Impl[slot] = null; // in case a new one is generated in this slot during directdelete
                if (_top == slot && Convert.ToBoolean(_cleanFlag & MMCleanFlag.UPDATE))
                    DelayedDelete(curr);
                else
                    DirectDelete(curr);
            }
            else if (_top < slot)
            {
                _top = slot;
            }

            Impl[slot] = m;
            if (_top > slot)
                _needInit[slot] = true;
            else
            {
                _needInit[slot] = false;
                m.Initialize(_owner);
            }
        }

        public void MoveChase(Unit target, float dist = 0.0f, float angle = 0.0f)
        {
            // ignore movement request if target not exist
            if (target == null || target == _owner || _owner.HasFlag(UnitFields.Flags, UnitFlags.DisableMove))
                return;

            //_owner->ClearUnitState(UNIT_STATE_FOLLOW);
            if (_owner.GetTypeId() == ObjectType.Player)
            {
                Log.outDebug("Player (GUID: {0}) chase to {1} (GUID: {2})", _owner.GetGUIDLow(), target.GetTypeId() == ObjectType.Player ? "player" : "creature",
                    target.GetTypeId() == ObjectType.Player ? target.GetGUIDLow() : target.ToCreature().GetGUIDLow());
                Mutate(new ChaseMovementGenerator<Player>(target, dist, angle), MovementSlot.ACTIVE);
            }
            else
            {
                Log.outDebug("Creature (Entry: {0} GUID: {1}) chase to {2} (GUID: {3})", _owner.GetEntry(), _owner.GetGUIDLow(), target.GetTypeId() == ObjectType.Player ? "player" : "creature",
                    target.GetTypeId() == ObjectType.Player ? target.GetGUIDLow() : target.ToCreature().GetGUIDLow());

                Mutate(new ChaseMovementGenerator<Creature>(target, dist, angle), MovementSlot.ACTIVE);
            }
        }
        public void MoveFollow(Unit target, float dist, float angle, MovementSlot slot = MovementSlot.ACTIVE)
        {
            // ignore movement request if target not exist
            if (target == null || target == _owner || _owner.HasFlag(UnitFields.Flags, UnitFlags.DisableMove))
                return;

            //_owner->AddUnitState(UNIT_STATE_FOLLOW);
            if (_owner.GetTypeId() == ObjectType.Player)
            {
                Log.outDebug("Player (GUID: {0}) follow to {1} (GUID: {2})", _owner.GetGUIDLow(),
                    target.GetTypeId() == ObjectType.Player ? "player" : "creature",
                    target.GetTypeId() == ObjectType.Player ? target.GetGUIDLow() : target.ToCreature().GetGUIDLow());
                Mutate(new FollowMovementGenerator<Player>(target, dist, angle), slot);
            }
            else
            {
                Log.outDebug("Creature (Entry: {0} GUID: {1}) follow to {2} (GUID: {3})",
                    _owner.GetEntry(), _owner.GetGUIDLow(),
                    target.GetTypeId() == ObjectType.Player ? "player" : "creature",
                    target.GetTypeId() == ObjectType.Player ? target.GetGUIDLow() : target.ToCreature().GetGUIDLow());
                Mutate(new FollowMovementGenerator<Creature>(target, dist, angle), slot);
            }
        }

        enum MMCleanFlag
        {
            NONE = 0,
            UPDATE = 1, // Clear or Expire called from update
            RESET = 2  // Flag if need top()->Reset()
        }

        public static uint GetSplineId()
        {
            return counter++;
        }
        static uint counter = 0;

        #region Fields
        object locker = new object();
        MovementGenerator[] Impl;
        bool[] _needInit;
        int _top;
        MMCleanFlag _cleanFlag;
        List<MovementGenerator> _expList;
        Unit _owner;
        #endregion
    }
}
