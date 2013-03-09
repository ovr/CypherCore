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

using System.Collections.Generic;
using System.Linq;
using System;
using System.Timers;
using Framework.Constants;
using Framework.Logging;
using WorldServer.Game.WorldEntities;
using WorldServer.Game.Managers;
using Framework.Utility;

namespace WorldServer.Game.Maps
{
    public class GridInfo
    {
        public GridInfo()
        {
            i_timer = new TimeTracker(0);
            vis_Update = new PeriodicTimer(0, RandomHelper.irand(0, 1000));
            i_unloadActiveLockCount = 0;
            i_unloadExplicitLock = false;
            i_unloadReferenceLock = false;
        }
        public GridInfo(uint expiry, bool unload = true)
        {
            i_timer = new TimeTracker((int)expiry);
            vis_Update = new PeriodicTimer(0, RandomHelper.irand(0, 1000));
            i_unloadActiveLockCount = 0;
            i_unloadExplicitLock = !unload;
            i_unloadReferenceLock = false;
        }
        public TimeTracker getTimeTracker() { return i_timer; }
        public bool getUnloadLock() { return i_unloadActiveLockCount != 0 || i_unloadExplicitLock || i_unloadReferenceLock; }
        void setUnloadExplicitLock(bool on) { i_unloadExplicitLock = on; }
        void setUnloadReferenceLock(bool on) { i_unloadReferenceLock = on; }
        void incUnloadActiveLock() { ++i_unloadActiveLockCount; }
        void decUnloadActiveLock() { if (i_unloadActiveLockCount != 0) --i_unloadActiveLockCount; }

        void setTimer(TimeTracker pTimer) { i_timer = pTimer; }
        public void ResetTimeTracker(int interval) { i_timer.Reset(interval); }
        public void UpdateTimeTracker(int diff) { i_timer.Update(diff); }
        public PeriodicTimer getRelocationTimer() { return vis_Update; }

        TimeTracker i_timer;
        PeriodicTimer vis_Update;

        ushort i_unloadActiveLockCount;                    // lock from active object spawn points (prevent clone loading)
        bool i_unloadExplicitLock;                     // explicit manual lock or config setting
        bool i_unloadReferenceLock;                     // lock from instance map copy
    }

    public class Grid
    {
        public Grid(uint id, uint x, uint y, uint expiry, bool unload = true)
        {
            i_gridId = id;
            i_x = x;
            i_y = y;
            i_GridInfo = new GridInfo(expiry, unload);
            i_gridstate = GridState_t.Invalid;
            i_GridObjectDataLoaded = false;
            i_cells = new GridCell[MapConst.MAX_NUMBER_OF_CELLS, MapConst.MAX_NUMBER_OF_CELLS];

            for (uint xx = 0; xx < MapConst.MAX_NUMBER_OF_CELLS; ++xx)
                for (uint yy = 0; yy < MapConst.MAX_NUMBER_OF_CELLS; ++yy)
                    i_cells[xx, yy] = new GridCell();
        }
        public Grid(Cell cell, uint expiry, bool unload = true)
            : this(cell.GetId(), cell.GridX(), cell.GridY(), expiry, unload) { }

        public GridCell GetGridType(uint x, uint y)
        {
            return i_cells[x, y];
        }
        public uint GetGridId() { return i_gridId; }

        // Visit all cells in Grid
        public void VisitAllCells<T>(Visitor<T> visitor) where T : Notifier
        {
            for (uint x = 0; x < MapConst.MAX_NUMBER_OF_CELLS; ++x)
                for (uint y = 0; y < MapConst.MAX_NUMBER_OF_CELLS; ++y)
                    GetGridType(x, y).Visit(visitor);
        }

        // Visit a single cell in Grid
        public void VisitCell<T>(uint x, uint y, Visitor<T> visitor) where T : Notifier
        {
            GetGridType(x, y).Visit(visitor);
        }


        public bool isGridObjectDataLoaded() { return i_GridObjectDataLoaded; }
        public void setGridObjectDataLoaded(bool pLoaded) { i_GridObjectDataLoaded = pLoaded; }
        public void SetGridState(GridState_t state) { i_gridstate = state; }
        public GridState_t GetGridState() { return i_gridstate; }
        public bool IsLoaded() { return i_GridObjectDataLoaded; }
        public void SetLoaded(bool loaded = true) { i_GridObjectDataLoaded = loaded; }
        public void ResetTimeTracker(int interval) { i_GridInfo.ResetTimeTracker(interval); }
        public void UpdateTimeTracker(int diff) { i_GridInfo.UpdateTimeTracker(diff); }
        public uint getX() { return i_x; }
        public uint getY() { return i_y; }
        public GridInfo getGridInfoRef() { return i_GridInfo; }


        uint i_gridId;
        uint i_x;
        uint i_y;
        GridInfo i_GridInfo;
        GridState_t i_gridstate;
        bool i_GridObjectDataLoaded;
        GridCell[,] i_cells;


        public uint GetWorldObjectCountInNGrid<T>() where T : WorldObject
        {
            uint count = 0;
            for (uint x = 0; x < MapConst.MAX_NUMBER_OF_CELLS; ++x)
                for (uint y = 0; y < MapConst.MAX_NUMBER_OF_CELLS; ++y)
                    count += i_cells[x, y].GetWorldObjectCountInGrid<T>();
            return count;
        }


        //old shit
        /*
        void MapUnloadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Unload();
        }

        public void Init(Map _map)
        {
            map = _map;
            Active = false;
            PlayerCount = 0;
            Corpses.Clear();
            Unloadpending = false;
            Objects.Clear();
            //MapUnloadTimer.Elapsed += MapUnloadTimer_Elapsed;
            Initialized = true;
        }

        public void AddObject(WorldObject obj)
        {
            if (obj is Player)
                ++PlayerCount;
            else if (obj is Corpse)
            {
                Corpses.Add(obj);
                if (Unloadpending)
                    CancelPendingUnload();
            }
            i_container.Add(obj);
            //Objects.Add(obj);
            obj.SetMapCell(this);
        }

        public void RemoveObject(WorldObject obj)
        {
            if (obj is Player)
                PlayerCount--;
            else if (obj is Corpse)
                Corpses.Remove(obj);

            Objects.Remove(obj);
            obj.SetMapCell(null);
        }

        public void SetActivity(bool state)
        {
            if (!Active && state)
            {
                if (Unloadpending)
                    CancelPendingUnload();

                //if (sWorld.Collision)
                //{
                //CollideInterface.ActivateTile(_mapmgr->GetMapId(), _x / 8, _y / 8);
                //}
            }
            else if (Active && !state)
            {
                if (!Unloadpending && CanUnload())
                    QueueUnloadPending();

                //if (sWorld.Collision)
                //{
                // CollideInterface.DeactivateTile(_mapmgr->GetMapId(), _x / 8, _y / 8);
                //}
            }
            Active = state;
        }

        public void RemoveObjects()
        {
            if (Unloadpending == true)
                return;

            foreach (var respawn in Respawns)
            {
                switch (respawn.GetTypeId())
                {
                    case ObjectType.Unit:
                        if (!(respawn is Pet))
                        {
                            //_mapmgr->_reusable_guids_creature.push_back((*itr)->GetUIdFromGUID());
                            //TO< Creature* >(*itr)->m_respawnCell = NULL;
                            //delete TO< Creature* >(*itr);
                        }
                        break;
                    case ObjectType.GameObject:
                        //_mapmgr->_reusable_guids_gameobject.push_back((*itr)->GetUIdFromGUID());
                        //TO< GameObject* >(*itr)->m_respawnCell = NULL;
                        //delete TO< GameObject* >(*itr);
                        break;
                }
            }
            Respawns.Clear();

            foreach (var obj in Objects)
            {
                if (obj is GameObject && (obj as GameObject).IsGameObjectType(GameObjectTypes.Transport))
                    continue;

                //if (obj.IsActive())
                //obj.Deactivate(_mapmgr);

                if (obj.IsInWorld)
                    obj.RemoveFromWorld();
            }
            Objects.Clear();
            Corpses.Clear();
            PlayerCount = 0;
            i_GridObjectDataLoaded = false;
            Active = false;
        }

        public void LoadObjects()
        {
            CellObjectGuids sp = Global.ObjMgr.GetOrCreateCellObjectGuids(map.GetId(), (byte)map.GetSpawnMode(), GetId());
            if (sp == null)
                return;

            //we still have mobs loaded on cell. There is no point of loading them again
            if (i_GridObjectDataLoaded)
                return;

            i_GridObjectDataLoaded = true;
            //MapInstance pInstance = MapInfo.pInstance;
            //InstanceBossInfoMap* bossInfoMap = objmgr.m_InstanceBossInfoMap[_mapmgr->GetMapId()];

            foreach (var guid in sp.creatures)
            {
                //uint respawnTimeOverride = 0;
                /*
                if(pInstance)
                {
                    if(bossInfoMap != NULL && IS_PERSISTENT_INSTANCE(pInstance))
                    {
                        bool skip = false;
                        for(std::set<uint32>::iterator killedNpc = pInstance->m_killedNpcs.begin(); killedNpc != pInstance->m_killedNpcs.end(); ++killedNpc)
                        {
                            // Do not spawn the killed boss.
                            if((*killedNpc) == (*i)->entry)
                            {
                                skip = true;
                                break;
                            }
                            // Do not spawn the killed boss' trash.
                            InstanceBossInfoMap::const_iterator bossInfo = bossInfoMap->find((*killedNpc));
                            if(bossInfo != bossInfoMap->end() && bossInfo->second->trash.find((*i)->id) != bossInfo->second->trash.end())
                            {
                                skip = true;
                                break;
                            }
                        }
                        if(skip)
                            continue;
                        for(InstanceBossInfoMap::iterator bossInfo = bossInfoMap->begin(); bossInfo != bossInfoMap->end(); ++bossInfo)
                        {
                            if(pInstance->m_killedNpcs.find(bossInfo->second->creatureid) == pInstance->m_killedNpcs.end() && bossInfo->second->trash.find((*i)->id) != bossInfo->second->trash.end())
                            {
                                respawnTimeOverride = bossInfo->second->trashRespawnOverride;
                            }
                        }
                    }
                    else
                    {
                        // No boss information available ... fallback ...
                        if(pInstance->m_killedNpcs.find((*i)->id) != pInstance->m_killedNpcs.end())
                            continue;
                    }
                }*/
        /*
                Creature obj = new Creature();
                // if(respawnTimeOverride > 0)
                //creature.m_respawnTimeOverride = respawnTimeOverride;

                if (!obj.LoadFromDB(guid, map))
                    continue;

                obj.SetMapCell(this);
                AddObject(obj);
                map.AddObject(obj);
            }


            foreach (var guid in sp.gameobjects)
            {
                GameObject obj = new GameObject();
                if (!obj.LoadFromDB(guid, map))
                    continue;

                obj.SetMapCell(this);
                AddObject(obj);
                map.AddObject(obj);
            }
        }

        public void QueueUnloadPending()
        {
            if (Unloadpending)
                return;

            Unloadpending = true;
            Log.outDebug("Queueing unload Cell:[{0} {1}] on Map:{2}", i_x, i_y, map.GetId());
            //MapUnloadTimer.Interval = 10000; //10secs
            //MapUnloadTimer.Start();
        }

        public void CancelPendingUnload()
        {
            Log.outDebug("Cancelling unload Cell:[{0} {1}] on Map:{2}", i_x, i_y, map.GetId());
            if (!Unloadpending)
                return;

            //sEventMgr.RemoveEvents(_mapmgr, MAKE_CELL_EVENT(_x, _y));
            Unloadpending = false;
        }

        public void Unload()
        {
            //MapUnloadTimer.Stop();
            if (Active)
            {
                Unloadpending = false;
                return;
            }

            Log.outDebug("Unloading Cell:[{0} {1}] on Map:{2}", i_x, i_y, map.GetId());
            Unloadpending = false;

            RemoveObjects();
            //CellHandler.Remove(i_x, i_y);
        }

        public void CorpseGoneIdle(WorldObject corpse)
        {
            Corpses.Remove(corpse);
            CheckUnload();
        }

        public void CheckUnload()
        {
            if (!Active && !Unloadpending && CanUnload())
                QueueUnloadPending();
        }

        public bool CanUnload()
        {
            if (Corpses.Count() == 0)// && _mapmgr->m_battleground == NULL)
                return true;
            else
                return false;
        }

        //Sets
        public void SetUnloadPending(bool up) { Unloadpending = up; }

        public uint GetPlayerCount() { return PlayerCount; }
        public int GetObjectCount() { return Objects.Count(); }
        public List<WorldObject> GetObjects() { return Objects; }
        public bool HasObject(WorldObject obj) { return Objects.Any(p => p == obj); }
        public bool HasPlayers() { return ((PlayerCount > 0) ? true : false); }
        public uint GetId()
        {
            return (uint)(i_x * MapConst.MAX_NUMBER_OF_GRIDS + i_y);
        }
        public bool IsInitialized() { return Initialized; }

        public bool IsUnloadPending() { return Unloadpending; }
        public bool IsActive() { return Active; }
        */
    }

    public class GridCell
    {
        public GridCell()
        {
            i_objects = new List<WorldObject>();
            i_container = new List<WorldObject>();
        }

        public void Visit<T>(Visitor<T> visitor) where T : Notifier
        {
            switch (visitor.GetNotifierType())
            {
                // Visit grid objects
                case NotifierObjectType.Grid:
                    visitor.GetNotifier().Visit(ref i_container, visitor.GetNotifierType());
                    break;
                // Visit world objects
                case NotifierObjectType.Object:
                    visitor.GetNotifier().Visit(ref i_objects, visitor.GetNotifierType());
                    break;
                default:
                    Log.outError("GridCell->Visit: Wrong NotifierObjectType");
                    break;
            }
        }

        public uint GetWorldObjectCountInGrid<T>()
        {
            return (uint)i_objects.Count(p => p is T);
        }

        //Inserts a container type object into the grid.
        public void AddGridObject(WorldObject obj)
        {
            i_container.Add(obj);
        }

        //Inserts a object of interested into the grid.
        public void AddWorldObject(WorldObject obj)
        {
            i_objects.Add(obj);
        }

        //Remove a container type object from the grid.
        public void RemoveGridObject(WorldObject obj)
        {
            i_container.Remove(obj);
        }

        //Remove a object of interested from the grid.
        public void RemoveWorldObject(WorldObject obj)
        {
            i_objects.Remove(obj);
        }

        /// <summary>
        /// Holds all World objects - Player, Pets, Corpse, DynamicObject(farsight)
        /// </summary>
        List<WorldObject> i_objects;

        /// <summary>
        /// Hold all Grid objects - GameObjects, Creatures, DynamicObject, Corpse(bones)
        /// </summary>
        List<WorldObject> i_container;
    }

    public enum GridState_t
    {
        Invalid = 0,
        Active = 1,
        Idle = 2,
        Removal = 3,
        Max = 4
    }
}
