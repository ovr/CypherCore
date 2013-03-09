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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Constants;
using Framework.DataStorage;
using Framework.Logging;
using Framework.Network;
using Framework.Utility;
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;
using Framework.Threading.Actors;
using Framework.Configuration;

namespace WorldServer.Game.Maps
{
    public class Map
    {        
        public Map(uint _id, uint expiry, uint instanceId, Difficulty spawnmode, Map _parent = null)
        {
            i_mapEntry = DBCStorage.MapStorage.LookupByKey(_id);
            i_spawnMode = spawnmode;
            i_InstanceId = instanceId;
            m_unloadTimer = 0;
            m_VisibleDistance = ObjectConst.DEFAULT_VISIBILITY_DISTANCE;
            m_VisibilityNotifyPeriod = 1000;
            i_gridExpiry = expiry;
            m_parentMap = (_parent != null ? _parent : this);
            m_delayedUpdateTimer = new TimeTrackerSmall((int)expiry / 5);
            
            //lets initialize visibility distance for map
            //Map::InitVisibilityDistance();
            i_grids = new Grid[MapConst.MAX_NUMBER_OF_GRIDS, MapConst.MAX_NUMBER_OF_GRIDS];
            GridMaps = new GridMap[MapConst.MAX_NUMBER_OF_GRIDS, MapConst.MAX_NUMBER_OF_GRIDS];
            m_activeNonPlayers = new List<WorldObject>();
            m_activePlayers = new List<Player>();
            //sScriptMgr->OnCreateMap(this);
        }
        void LoadMapAndVMap(uint gx, uint gy)
        {
            LoadMap(gx, gy);
            // Only load the data for the base map
            if (i_InstanceId == 0)
            {
                //LoadVMap(gx, gy);
                //LoadMMap(gx, gy);
            }
        }
        void LoadMap(uint gx, uint gy)
        {
            if (i_InstanceId != 0)
            {
                if (GridMaps[gx, gy] != null)
                    return;

                // load grid map for base map
               // if (!m_parentMap->GridMaps[gx][gy])
                    //m_parentMap->EnsureGridCreated_i(GridCoord(63 - gx, 63 - gy));

                //((MapInstanced*)(m_parentMap))->AddGridMapReference(GridCoord(gx, gy));
                //GridMaps[gx][gy] = m_parentMap->GridMaps[gx][gy];
                return;
            }

            if (GridMaps[gx, gy] != null)
                return;

            // map file name
            string filename = string.Format(WorldConfig.DataPath + "/maps/{0:D3}{1:D2}{2:D2}.map", GetId(), gx, gy);
            Log.outInfo("Loading map {0}", filename);
            // loading data
            GridMaps[gx, gy] = new GridMap();
            if (!GridMaps[gx, gy].loadData(filename))
                Log.outError("Error loading map file: \n {0}\n", filename);

            //sScriptMgr->OnLoadGridMap(this, GridMaps[gx][gy], gx, gy);
        }

        public uint GetId() { return i_mapEntry.MapID; }

        GridMap GetGridMap(float x, float y)
        {
            // half opt method
            int gx = (int)(32 - x / MapConst.SizeOfGrids); //grid x
            int gy = (int)(32 - y / MapConst.SizeOfGrids); //grid y

            // ensure GridMap is loaded
            //EnsureGridCreated(GridCoord(63-gx, 63-gy));

            return GridMaps[gx, gy];
        }
        public float GetHeight(float x, float y, float z, bool checkVMap = true, float maxSearchDist = MapConst.DEFAULT_HEIGHT_SEARCH)
        {
            // find raw .map surface under Z coordinates
            float mapHeight = MapConst.INVALID_HEIGHT;//VMAP_INVALID_HEIGHT_VALUE;
            GridMap gmap = GetGridMap(x, y);
            if (gmap != null)
            {
                float gridHeight = gmap.getHeight(x, y);
                // look from a bit higher pos to find the floor, ignore under surface case
                if (z + 2.0f > gridHeight)
                    mapHeight = gridHeight;
            }

            float vmapHeight = MapConst.INVALID_HEIGHT;//VMAP_INVALID_HEIGHT_VALUE;
            if (checkVMap)
            {
                //VMAP::IVMapManager* vmgr = VMAP::VMapFactory::createOrGetVMapManager();
                //if (vmgr->isHeightCalcEnabled())
                //vmapHeight = vmgr->getHeight(GetId(), x, y, z + 2.0f, maxSearchDist);   // look from a bit higher pos to find the floor
            }

            // mapHeight set for any above raw ground Z or <= INVALID_HEIGHT
            // vmapheight set for any under Z value or <= INVALID_HEIGHT
            if (vmapHeight > MapConst.INVALID_HEIGHT)
            {
                if (mapHeight > MapConst.INVALID_HEIGHT)
                {
                    // we have mapheight and vmapheight and must select more appropriate

                    // we are already under the surface or vmap height above map heigt
                    // or if the distance of the vmap height is less the land height distance
                    if (z < mapHeight || vmapHeight > mapHeight || Math.Abs(mapHeight - z) > Math.Abs(vmapHeight - z))
                        return vmapHeight;
                    else
                        return mapHeight;                           // better use .map surface height
                }
                else
                    return vmapHeight;                              // we have only vmapHeight (if have)
            }

            return mapHeight;                               // explicitly use map data
        }
        public float GetHeight(uint phasemask, float x, float y, float z, bool vmap = true, float maxSearchDist = MapConst.DEFAULT_HEIGHT_SEARCH)
        {
            return GetHeight(x, y, z, vmap, maxSearchDist);//Math.Max(GetHeight(x, y, z, vmap, maxSearchDist), _dynamicTree.getHeight(x, y, z, maxSearchDist, phasemask));
        }
        public float GetWaterOrGroundLevel(float x, float y, float z, out float ground, bool swim = false)
        {
            ground = 0;
            if (GetGridMap(x, y) != null)
            {
                // we need ground level (including grid height version) for proper return water level in point
                float ground_z = GetHeight(0, x, y, z, true, 50.0f);
                ground = ground_z;

                LiquidData liquid_status = new LiquidData();

                ZLiquidStatus res = getLiquidStatus(x, y, ground_z, MapConst.MAP_ALL_LIQUIDS, liquid_status);
                return res != ZLiquidStatus.LIQUID_MAP_NO_WATER ? liquid_status.level : ground_z;
            }

            return 0;// VMAP_INVALID_HEIGHT_VALUE;
        }
        ZLiquidStatus getLiquidStatus(float x, float y, float z, byte ReqLiquidType, LiquidData data)
        {
            ZLiquidStatus result = ZLiquidStatus.LIQUID_MAP_NO_WATER;
            //VMAP::IVMapManager* vmgr = VMAP::VMapFactory::createOrGetVMapManager();
            //float liquid_level = MapConst.INVALID_HEIGHT;
            float ground_level = MapConst.INVALID_HEIGHT;
            //uint liquid_type = 0;
            /*
    if (vmgr->GetLiquidLevel(GetId(), x, y, z, ReqLiquidType, liquid_level, ground_level, liquid_type))
    {
        sLog->outDebug(LOG_FILTER_MAPS, "getLiquidStatus(): vmap liquid level: %f ground: %f type: %u", liquid_level, ground_level, liquid_type);
        // Check water level and ground level
        if (liquid_level > ground_level && z > ground_level - 2)
        {
            // All ok in water -> store data
            if (data)
            {
                // hardcoded in client like this
                if (GetId() == 530 && liquid_type == 2)
                    liquid_type = 15;

                uint32 liquidFlagType = 0;
                if (LiquidTypeEntry const* liq = sLiquidTypeStore.LookupEntry(liquid_type))
                    liquidFlagType = liq->Type;

                if (liquid_type && liquid_type < 21)
                {
                    if (AreaTableEntry const* area = GetAreaEntryByAreaFlagAndMap(GetAreaFlag(x, y, z), GetId()))
                    {
                        uint32 overrideLiquid = area->LiquidTypeOverride[liquidFlagType];
                        if (!overrideLiquid && area->zone)
                        {
                            area = GetAreaEntryByAreaID(area->zone);
                            if (area)
                                overrideLiquid = area->LiquidTypeOverride[liquidFlagType];
                        }

                        if (LiquidTypeEntry const* liq = sLiquidTypeStore.LookupEntry(overrideLiquid))
                        {
                            liquid_type = overrideLiquid;
                            liquidFlagType = liq->Type;
                        }
                    }
                }

                data->level = liquid_level;
                data->depth_level = ground_level;

                data->entry = liquid_type;
                data->type_flags = 1 << liquidFlagType;
            }

            float delta = liquid_level - z;

            // Get position delta
            if (delta > 2.0f)                   // Under water
                return LIQUID_MAP_UNDER_WATER;
            if (delta > 0.0f)                   // In water
                return LIQUID_MAP_IN_WATER;
            if (delta > -0.1f)                   // Walk on water
                return LIQUID_MAP_WATER_WALK;
            result = LIQUID_MAP_ABOVE_WATER;
        }
    }
            */
            GridMap gmap = GetGridMap(x, y);
            if (gmap != null)
            {
                LiquidData map_data = new LiquidData();
                ZLiquidStatus map_result = gmap.getLiquidStatus(x, y, z, ReqLiquidType, map_data);
                // Not override LIQUID_MAP_ABOVE_WATER with LIQUID_MAP_NO_WATER:
                if (map_result != ZLiquidStatus.LIQUID_MAP_NO_WATER && (map_data.level > ground_level))
                {
                    if (data != null)
                    {
                        // hardcoded in client like this
                        if (GetId() == 530 && map_data.entry == 2)
                            map_data.entry = 15;

                        data = map_data;
                    }
                    return map_result;
                }
            }
            return result;
        }



        void EnsureGridLoadedForActiveObject(Cell cell, WorldObject obj)
        {
            EnsureGridLoaded(cell);
            Grid grid = getGrid(cell.GridX(), cell.GridY());

            // refresh grid state & timer
            if (grid.GetGridState() != GridState_t.Active)
            {
                Log.outDebug("Active object {0} triggers loading of grid [{1}, {2}] on map {3}", obj.GetGUID(), cell.GridX(), cell.GridY(), GetId());
                ResetGridExpiry(grid, 0.1f);
                grid.SetGridState(GridState_t.Active);
            }
        }
        bool EnsureGridLoaded(Cell cell)
        {
            EnsureGridCreated(new GridCoord(cell.GridX(), cell.GridY()));
            Grid grid = getGrid(cell.GridX(), cell.GridY());

            if (!grid.IsLoaded())
            {
                Log.outDebug("Loading grid[{0}, {1}] for map {2} instance {3}", cell.GridX(), cell.GridY(), GetId(), i_InstanceId);

                grid.SetLoaded();
                GridLoader loader = new GridLoader(grid, this, cell);
                loader.LoadGrid();

                // Add resurrectable corpses to world object list in grid
                //sObjectAccessor->AddCorpsesToGrid(GridCoord(cell.GridX(), cell.GridY()), grid->GetGridType(cell.CellX(), cell.CellY()), this);
                //Balance();
                return true;
            }

            return false;
        }
        void EnsureGridCreated(GridCoord p)
        {
            lock (this)
            {
                EnsureGridCreated_i(p);
            }
        }
        void EnsureGridCreated_i(GridCoord p)
        {
            if (getGrid(p.x_coord, p.y_coord) == null)
            {
                Log.outDebug("Creating grid[{0}, {1}] for map {2} instance {3}", p.x_coord, p.y_coord, GetId(), i_InstanceId);

                setGrid(new Grid(p.x_coord * MapConst.MAX_NUMBER_OF_GRIDS + p.y_coord, p.x_coord, p.y_coord, i_gridExpiry, true),//sWorld->getBoolConfig(CONFIG_GRID_UNLOAD)),
                    p.x_coord, p.y_coord);

                getGrid(p.x_coord, p.y_coord).SetGridState(GridState_t.Idle);

                //z coord
                uint gx = (MapConst.MAX_NUMBER_OF_GRIDS - 1) - p.x_coord;
                uint gy = (MapConst.MAX_NUMBER_OF_GRIDS - 1) - p.y_coord;

                if (GridMaps[gx, gy] == null)
                    LoadMapAndVMap(gx, gy);
            }
        }

        Grid getGrid(uint x, uint y)
        {
            return i_grids[x, y];
        }
        void setGrid(Grid grid, uint x, uint y)
        {
            if (x >= MapConst.MAX_NUMBER_OF_GRIDS || y >= MapConst.MAX_NUMBER_OF_GRIDS)
            {
                Log.outError("map->setNGrid Invalid grid coordinates found: {0}, {1}!", x, y);
                return;
            }
            i_grids[x, y] = grid;
        }
        public void ResetGridExpiry(Grid grid, float factor = 1)
        {
            grid.ResetTimeTracker((int)(i_gridExpiry * factor));
        }
        public bool UnloadGrid(Grid grid, bool unloadAll)
        {
            uint x = grid.getX();
            uint y = grid.getY();

            if (!unloadAll)
            {
                //pets, possessed creatures (must be active), transport passengers
                if (grid.GetWorldObjectCountInNGrid<Creature>() != 0)
                    return false;

                if (ActiveObjectsNearGrid(grid))
                    return false;
            }

            Log.outDebug("Unloading grid[{0}, {1}] for map {2}", x, y, GetId());

            if (!unloadAll)
            {
                // Finish creature moves, remove and delete all creatures with delayed remove before moving to respawn grids
                // Must know real mob position before move
                //MoveAllCreaturesInMoveList();

                // move creatures to respawn grids if this is diff.grid or to remove list
                ObjectGridEvacuator worker = new ObjectGridEvacuator();
                var visitor = new Visitor<ObjectGridEvacuator>(worker, NotifierObjectType.Grid);
                grid.VisitAllCells(visitor);

                // Finish creature moves, remove and delete all creatures with delayed remove before unload
                //MoveAllCreaturesInMoveList();
            }

            {
                ObjectGridCleaner worker = new ObjectGridCleaner();
                var visitor = new Visitor<ObjectGridCleaner>(worker, NotifierObjectType.Grid);
                grid.VisitAllCells(visitor);
            }

            //RemoveAllObjectsInRemoveList();

            {
                ObjectGridUnloader worker = new ObjectGridUnloader();
                var visitor = new Visitor<ObjectGridUnloader>(worker, NotifierObjectType.Grid);
                grid.VisitAllCells(visitor);
            }
            setGrid(null, x, y);
            int gx = (int)((MapConst.MAX_NUMBER_OF_GRIDS - 1) - x);
            int gy = (int)((MapConst.MAX_NUMBER_OF_GRIDS - 1) - y);

            // delete grid map, but don't delete if it is from parent map (and thus only reference)
            //+++if (GridMaps[gx][gy]) don't check for GridMaps[gx][gy], we might have to unload vmaps
            {
                if (i_InstanceId == 0)
                {
                    if (GridMaps[gx, gy] != null)
                    {
                        GridMaps[gx, gy].unloadData();
                        //delete GridMaps[gx, gy];
                    }
                    //VMAP::VMapFactory::createOrGetVMapManager()->unloadMap(GetId(), gx, gy);
                    //MMAP::MMapFactory::createOrGetMMapManager()->unloadMap(GetId(), gx, gy);
                }
                //else
                //((MapInstance)m_parentMap).RemoveGridMapReference(GridCoord(gx, gy));

                GridMaps[gx, gy] = null;
            }
            Log.outDebug("Unloading grid[{0}, {1}] for map {2} finished", x, y, GetId());
            return true;
        }




        void resetMarkedCells() { marked_cells.SetAll(false); }
        bool isCellMarked(uint pCellId) { return marked_cells.Get((int)pCellId); }
        void markCell(uint pCellId) { marked_cells.Set((int)pCellId, true); }

        void VisitNearbyCellsOf(WorldObject obj, Visitor<UpdaterNotifier> gridVisitor, Visitor<UpdaterNotifier> worldVisitor)
        {
            // Check for valid position
            if (!obj.IsPositionValid())
                return;

            // Update mobs/objects in ALL visible cells around object!
            CellArea area = new CellArea(obj.GetPositionX(), obj.GetPositionY(), obj.GetGridActivationRange());

            for (uint x = area.low_bound.x_coord; x <= area.high_bound.x_coord; ++x)
            {
                for (uint y = area.low_bound.y_coord; y <= area.high_bound.y_coord; ++y)
                {
                    // marked cells are those that have been visited
                    // don't visit the same cell twice
                    uint cell_id = (y * MapConst.TOTAL_NUMBER_OF_CELLS_PER_MAP) + x;
                    if (isCellMarked(cell_id))
                        continue;

                    markCell(cell_id);
                    CellCoord pair = new CellCoord(x, y);
                    Cell cell = new Cell(pair);                    
                    Visit(cell, gridVisitor);
                    Visit(cell, worldVisitor);
                }
            }
        }
        public void Visit<T>(Cell cell, Visitor<T> visitor) where T : Notifier
        {
            uint x = cell.GridX();
            uint y = cell.GridY();
            uint cell_x = cell.CellX();
            uint cell_y = cell.CellY();

            if (!cell.NoCreate() || IsGridLoaded(new GridCoord(x, y)))
            {
                EnsureGridLoaded(cell);
                getGrid(x, y).VisitCell(cell_x, cell_y, visitor);
            }
        }

        bool IsGridLoaded(GridCoord p)
        {
            return (getGrid(p.x_coord, p.y_coord) != null && isGridObjectDataLoaded(p.x_coord, p.y_coord));
        }
        bool isGridObjectDataLoaded(uint x, uint y) { return getGrid(x, y).isGridObjectDataLoaded(); }
        void setGridObjectDataLoaded(bool pLoaded, uint x, uint y) { getGrid(x, y).setGridObjectDataLoaded(pLoaded); }

        public static void InitStateMachine()
        {
            si_GridStates[(int)GridState_t.Invalid] = new InvalidState();
            si_GridStates[(int)GridState_t.Active] = new ActiveState();
            si_GridStates[(int)GridState_t.Idle] = new IdleState();
            si_GridStates[(int)GridState_t.Removal] = new RemovalState();
        }

        TimeTrackerSmall m_delayedUpdateTimer;

        public void DelayedUpdate(uint t_diff)
        {
            m_delayedUpdateTimer.Update((int)t_diff);
            if (!m_delayedUpdateTimer.Passed())
                return;
            //RemoveAllObjectsInRemoveList();
            
            // Don't unload grids if it's battleground, since we may have manually added GOs, creatures, those doesn't load from DB at grid re-load !
            // This isn't really bother us, since as soon as we have instanced BG-s, the whole map unloads as the BG gets ended
            if (!IsBattlegroundOrArena())
            {
                foreach (var grid in i_grids)
                {
                    if (grid == null)
                        continue;

                    GridInfo info = grid.getGridInfoRef();
                    si_GridStates[(int)grid.GetGridState()].Update(this, grid, info, t_diff);
                }
            }
        }

        public bool ActiveObjectsNearGrid(Grid grid)
        {
            CellCoord cell_min = new CellCoord(grid.getX() * MapConst.MAX_NUMBER_OF_CELLS, grid.getY() * MapConst.MAX_NUMBER_OF_CELLS);
            CellCoord cell_max = new CellCoord(cell_min.x_coord + MapConst.MAX_NUMBER_OF_CELLS, cell_min.y_coord + MapConst.MAX_NUMBER_OF_CELLS);

            //we must find visible range in cells so we unload only non-visible cells...
            float viewDist = GetVisibilityRange();
            uint cell_range = (uint)Math.Ceiling(viewDist / MapConst.SIZE_OF_GRID_CELL) + 1;

            cell_min.dec_x(cell_range);
            cell_min.dec_y(cell_range);
            cell_max.inc_x(cell_range);
            cell_max.inc_y(cell_range);

            foreach (var player in m_activePlayers)
            {
                CellCoord p = GridDefines.ComputeCellCoord(player.GetPositionX(), player.GetPositionY());
                if ((cell_min.x_coord <= p.x_coord && p.x_coord <= cell_max.x_coord) &&
                    (cell_min.y_coord <= p.y_coord && p.y_coord <= cell_max.y_coord))
                    return true;
            }

            foreach (var obj in m_activeNonPlayers)
            {
                CellCoord p = GridDefines.ComputeCellCoord(obj.GetPositionX(), obj.GetPositionY());
                if ((cell_min.x_coord <= p.x_coord && p.x_coord <= cell_max.x_coord) &&
                    (cell_min.y_coord <= p.y_coord && p.y_coord <= cell_max.y_coord))
                    return true;
            }

            return false;
        }


        static GridState[] si_GridStates = new GridState[(int)GridState_t.Max];

        //old shit







        public float GetVisibilityRange() { return m_VisibleDistance; }

        public void Update(uint diff)
        {
            //_dynamicTree.update(t_diff);
            /// update worldsessions for existing players
            //for (m_mapRefIter = m_mapRefManager.begin(); m_mapRefIter != m_mapRefManager.end(); ++m_mapRefIter)
            foreach (var player in m_activePlayers)
            {
                if (player.IsInWorld)
                {
                    //WorldSession* session = player->GetSession();
                    //MapSessionFilter updater(session);
                    //session->Update(t_diff, updater);
                }
            }

            /// update active cells around players and active objects
            resetMarkedCells();
            UpdaterNotifier update = new UpdaterNotifier(diff);
            // for creature
            var grid_object_update = new Visitor<UpdaterNotifier>(update, NotifierObjectType.Grid);
            // for pets
            var world_object_update = new Visitor<UpdaterNotifier>(update, NotifierObjectType.Object);

            // the player iterator is stored in the map object
            // to make sure calls to Map::Remove don't invalidate it
            foreach (var player in m_activePlayers)
            {
                if (!player.IsInWorld)
                    continue;

                // update players at tick
                player.Update(diff);

                VisitNearbyCellsOf(player, grid_object_update, world_object_update);
            }

            // non-player active objects, increasing iterator in the loop in case of object removal
            foreach (var obj in m_activeNonPlayers.ToList())
            {
                if (!obj.IsInWorld)
                    continue;

                VisitNearbyCellsOf(obj, grid_object_update, world_object_update);
            }

            ///- Process necessary scripts
            //if (!m_scriptSchedule.empty())
            {
                //i_scriptLock = true;
                //ScriptsProcess();
                //i_scriptLock = false;
            }

            //MoveAllCreaturesInMoveList();

            if (m_activePlayers.Count != 0 || m_activeNonPlayers.Count != 0)
                ProcessRelocationNotifies(diff);

            //sScriptMgr->OnMapUpdate(this, t_diff);
        }
        void ProcessRelocationNotifies(uint diff)
        {
            foreach (var grid in i_grids)
            {
                if (grid == null)
                    continue;

                if (grid.GetGridState() != GridState_t.Active)
                    continue;

                grid.getGridInfoRef().getRelocationTimer().TUpdate((int)diff);
                if (!grid.getGridInfoRef().getRelocationTimer().TPassed())
                    continue;

                uint gx = grid.getX(), gy = grid.getY();

                CellCoord cell_min = new CellCoord(gx * MapConst.MAX_NUMBER_OF_CELLS, gy * MapConst.MAX_NUMBER_OF_CELLS);
                CellCoord cell_max = new CellCoord(cell_min.x_coord + MapConst.MAX_NUMBER_OF_CELLS, cell_min.y_coord + MapConst.MAX_NUMBER_OF_CELLS);

                for (uint x = cell_min.x_coord; x < cell_max.x_coord; ++x)
                {
                    for (uint y = cell_min.y_coord; y < cell_max.y_coord; ++y)
                    {
                        uint cell_id = (y * MapConst.TOTAL_NUMBER_OF_CELLS_PER_MAP) + x;
                        if (!isCellMarked(cell_id))
                            continue;

                        CellCoord pair = new CellCoord(x, y);
                        Cell cell = new Cell(pair);
                        cell.SetNoCreate();

                        DelayedUnitRelocation cell_relocation = new DelayedUnitRelocation(cell, pair, this, ObjectConst.MAX_VISIBILITY_DISTANCE);
                        var grid_object_relocation = new Visitor<DelayedUnitRelocation>(cell_relocation, NotifierObjectType.Grid);
                        var world_object_relocation = new Visitor<DelayedUnitRelocation>(cell_relocation, NotifierObjectType.Object);
                        Visit(cell, grid_object_relocation);
                        Visit(cell, world_object_relocation);
                    }
                }
            }

            ResetNotifier reset = new ResetNotifier();
            var grid_notifier = new Visitor<ResetNotifier>(reset, NotifierObjectType.Grid);
            var world_notifier = new Visitor<ResetNotifier>(reset, NotifierObjectType.Object);

            foreach (var grid in i_grids)
            {
                if (grid == null)
                    continue;

                if (grid.GetGridState() != GridState_t.Active)
                    continue;

                if (!grid.getGridInfoRef().getRelocationTimer().TPassed())
                    continue;

                grid.getGridInfoRef().getRelocationTimer().TReset((int)diff, m_VisibilityNotifyPeriod);

                uint gx = grid.getX(), gy = grid.getY();

                CellCoord cell_min = new CellCoord(gx * MapConst.MAX_NUMBER_OF_CELLS, gy * MapConst.MAX_NUMBER_OF_CELLS);
                CellCoord cell_max = new CellCoord(cell_min.x_coord + MapConst.MAX_NUMBER_OF_CELLS, cell_min.y_coord + MapConst.MAX_NUMBER_OF_CELLS);

                for (uint x = cell_min.x_coord; x < cell_max.x_coord; ++x)
                {
                    for (uint y = cell_min.y_coord; y < cell_max.y_coord; ++y)
                    {
                        uint cell_id = (y * MapConst.TOTAL_NUMBER_OF_CELLS_PER_MAP) + x;
                        if (!isCellMarked(cell_id))
                            continue;

                        CellCoord pair = new CellCoord(x, y);
                        Cell cell = new Cell(pair);
                        cell.SetNoCreate();
                        Visit(cell, grid_notifier);
                        Visit(cell, world_notifier);
                    }
                }
            }
        }



        int m_VisibilityNotifyPeriod;

        public void VisitWorld<T>(float x, float y, float radius, T notifier) where T : Notifier
        {
            CellCoord p = GridDefines.ComputeCellCoord(x, y);
            Cell cell = new Cell(p);
            cell.SetNoCreate();

            var world_object_notifier = new Visitor<T>(notifier, NotifierObjectType.Object);
            cell.Visit(p, world_object_notifier, this, radius, x, y);
        }
        public void VisitGrid<T>(float x, float y, float radius, T notifier) where T : Notifier
        {
            CellCoord p = GridDefines.ComputeCellCoord(x, y);
            Cell cell = new Cell(p);
            cell.SetNoCreate();

            var grid_object_notifier = new Visitor<T>(notifier, NotifierObjectType.Grid);
            cell.Visit(p, grid_object_notifier, this, radius, x, y);
        }
        public void VisitAll<T>(float x, float y, float radius, T notifier) where T : Notifier
        {
            CellCoord p = GridDefines.ComputeCellCoord(x, y);
            Cell cell = new Cell(p);
            cell.SetNoCreate();
            var world_object_notifier = new Visitor<T>(notifier, NotifierObjectType.Object);
            cell.Visit(p, world_object_notifier, this, radius, x, y);
            var grid_object_notifier = new Visitor<T>(notifier, NotifierObjectType.Grid);
            cell.Visit(p, grid_object_notifier, this, radius, x, y);
        }

        #region Fields
        Difficulty i_spawnMode;
        MapEntry i_mapEntry;
        uint m_unloadTimer;
        Map m_parentMap;
        uint i_InstanceId;
        uint i_gridExpiry;
        BitArray marked_cells = new BitArray(MapConst.TOTAL_NUMBER_OF_CELLS_PER_MAP * MapConst.TOTAL_NUMBER_OF_CELLS_PER_MAP);
        float m_VisibleDistance;
        protected readonly List<WorldObject> m_activeNonPlayers;
        protected readonly List<Player> m_activePlayers;

        Grid[,] i_grids;
        GridMap[,] GridMaps; 
        #endregion









        public bool AddPlayer(Player pl)
        {
            CellCoord cellCoord = GridDefines.ComputeCellCoord(pl.GetPositionX(), pl.GetPositionY());
            if (!cellCoord.IsCoordValid())
            {
                Log.outError("Map->AddPlayer (GUID: {0}) has invalid coordinates X:{1} Y:{2}", pl.GetGUIDLow(), pl.GetPositionX(), pl.GetPositionY());
                return false;
            }
            Cell cell = new Cell(cellCoord);
            EnsureGridLoadedForActiveObject(cell, pl);
            AddToGrid(pl, cell);

            pl.SetMap(this);
            pl.AddToWorld();

            SendInitSelf(pl);
            //SendInitTransports(pl);

            pl.m_clientGUIDs.Clear();
            pl.UpdateObjectVisibility(false);

            m_activePlayers.Add(pl);

            //sScriptMgr->OnPlayerEnterMap(this, player);
            return true;
        }
        public bool AddToMap(WorldObject obj)
        {
            //TODO: Needs clean up. An object should not be added to map twice.
            if (obj.IsInWorld)
            {
                obj.UpdateObjectVisibility(true);
                return true;
            }

            CellCoord cellCoord = GridDefines.ComputeCellCoord(obj.GetPositionX(), obj.GetPositionY());
            //It will create many problems (including crashes) if an object is not added to grid after creation
            //The correct way to fix it is to make AddToMap return false and delete the object if it is not added to grid
            //But now AddToMap is used in too many places, I will just see how many ASSERT failures it will cause
            if (!cellCoord.IsCoordValid())
            {
                Log.outError("Map->Add: Object {0} has invalid coordinates X:{1} Y:{2} grid cell [{3}:{4}]", obj.GetGUID(), obj.GetPositionX(), obj.GetPositionY(), cellCoord.x_coord, cellCoord.y_coord);
                return false; //Should delete object
            }

            Cell cell = new Cell(cellCoord);
            if (obj.isActiveObject())
                EnsureGridLoadedForActiveObject(cell, obj);
            else
                EnsureGridCreated(new GridCoord(cell.GridX(), cell.GridY()));
            AddToGrid(obj, cell);
            Log.outDebug("Object {0} enters grid[{1}, {2}]", obj.GetGUIDLow(), cell.GridX(), cell.GridY());

            //Must already be set before AddToMap. Usually during obj->Create.
            //obj->SetMap(this);
            obj.AddToWorld();

            //InitializeObject(obj);

            if (obj.isActiveObject())
                AddToActive(obj);

            //something, such as vehicle, needs to be update immediately
            //also, trigger needs to cast spell, if not update, cannot see visual
            obj.UpdateObjectVisibility(true);
            return true;
        }
        public void AddToActive<T>(T obj) where T : WorldObject { AddToActiveHelper(obj); }
        void AddToActiveHelper<T>(T obj) where T : WorldObject
        {
            m_activeNonPlayers.Add(obj);
        }
        public void AddToGrid<T>(T obj, Cell cell)where T : WorldObject
        {
            Grid grid = getGrid(cell.GridX(), cell.GridY());
            if (obj.IsWorldObject())
                grid.GetGridType(cell.CellX(), cell.CellY()).AddWorldObject(obj);
            else
                grid.GetGridType(cell.CellX(), cell.CellY()).AddGridObject(obj);

            if (obj is Creature)
                (obj as Creature).SetCurrentCell(cell);
        }
        public void RemoveFromGrid(Creature obj, Cell cell)
        {
            Grid grid = getGrid(cell.GridX(), cell.GridY());
            if (obj.IsWorldObject())
                grid.GetGridType(cell.CellX(), cell.CellY()).RemoveWorldObject(obj);
            else
                grid.GetGridType(cell.CellX(), cell.CellY()).RemoveGridObject(obj);

            //obj.SetCurrentCell(cell);
        }
        public void RemoveFromGrid<T>(T obj, Cell cell) where T : WorldObject
        {
            Grid grid = getGrid(cell.GridX(), cell.GridY());
            if (obj.IsWorldObject())
                grid.GetGridType(cell.CellX(), cell.CellY()).RemoveWorldObject(obj);
            else
                grid.GetGridType(cell.CellX(), cell.CellY()).RemoveGridObject(obj);
        }

        public void PlayerRelocation(Player pl, float x, float y, float z, float orientation)
        {
            Cell oldcell = new Cell(pl.GetPositionX(), pl.GetPositionY());
            Cell newcell = new Cell(x, y);

            //! If hovering, always increase our server-side Z position
            //! Client automatically projects correct position based on Z coord sent in monster move
            //! and UNIT_FIELD_HOVERHEIGHT sent in object updates
            //if (pl.HasUnitMovementFlag(MOVEMENTFLAG_HOVER))
                //z += player->GetFloatValue(UNIT_FIELD_HOVERHEIGHT);
            
            pl.Position.Relocate(x, y, z, orientation);
            //if (player->IsVehicle())
                //player->GetVehicleKit()->RelocatePassengers();

            if (oldcell.DiffGrid(newcell) || oldcell.DiffCell(newcell))
            {
                Log.outDebug("Player {0} relocation grid[{1}, {2}]cell[{3}, {4}]->grid[{5}, {6}]cell[{7}, {8}]", 
                    pl.GetName(), oldcell.GridX(), oldcell.GridY(), oldcell.CellX(), oldcell.CellY(), newcell.GridX(), newcell.GridY(), newcell.CellX(), newcell.CellY());

                RemoveFromGrid(pl, oldcell);
                if (oldcell.DiffGrid(newcell))
                    EnsureGridLoadedForActiveObject(newcell, pl);

                AddToGrid(pl, newcell);
            }

            pl.UpdateObjectVisibility(false);
        }








        //old shit
        private object updatelock = new object();







        public bool RemovePlayer(Player pl)
        {
            //if (!HasPlayer(pl))
            //{
                //Log.outError("Remove Player: {0} Not found in Map: {1}.", pl.GetGUIDLow(), this.GetId());
                //return false;
            //}
            pl.RemoveFromWorld();
            //pl.GetMapCell().RemoveObject(pl);
            //PlayerList.Remove(pl);

            return true;
        }
        public bool RemoveObject(WorldObject obj)
        {
            if (!HasObject(obj))
            {
                Log.outError("Remove Object: {0} Not found in Map: {1}.", obj.GetGUIDLow(), this.GetId());
                return false;
            }
            obj.RemoveFromWorld();
            //ObjectList.Remove(obj);

            return true;
        }

        //Update
        public void UpdateCellActivity(uint x, uint y, uint radius)
        {
            uint endX = (x + radius) <= MapConst.MAX_NUMBER_OF_CELLS ? x + radius : (MapConst.MAX_NUMBER_OF_CELLS - 1);
            uint endY = (y + radius) <= MapConst.MAX_NUMBER_OF_CELLS ? y + radius : (MapConst.MAX_NUMBER_OF_CELLS - 1);
            uint startX = x > radius ? x - radius : 0;
            uint startY = y > radius ? y - radius : 0;
            uint posX, posY;

            Grid grid;
            for (posX = startX; posX <= endX; posX++)
            {
                for (posY = startY; posY <= endY; posY++)
                {
                    grid = getGrid(posX, posY);
                    //bool active = IsCellActive(posX, posY);
                    if (grid == null)
                    {
                        //if (active)
                        {
                            //grid = CellHandler.Create(posX, posY);
                            //grid.Init(this);

                            Log.outDebug("Cell:[{0},{1}] on Map:{2} is active.", posX, posY, GetId());
                            //Global.TerrainMgr.LoadTile(Id, (int)posX / 8, (int)posY / 8);

                            //grid.LoadObjects();
                            //grid.SetActivity(true);
                        }
                    }
                    else
                    {
                        //Cell is now active
                        //if (active && !grid.IsActive())
                        {
                            Log.outDebug("Cell:[{0},{1}] on Map:{2} is active.", posX, posY, GetId());
                            //Global.TerrainMgr.LoadTile(Id, (int)posX / 8, (int)posY / 8);

                            //if (!grid.IsLoaded())
                                //grid.LoadObjects();

                            //grid.SetActivity(true);
                        }
                        //Cell is no longer active
                        //else if (!active && grid.IsActive())
                        {
                            Log.outDebug("Cell:[{0},{1}] on Map:{2} is idle.", posX, posY, GetId());
                            //Global.TerrainMgr.UnloadTile((int)posX / 8, (int)posY / 8);
                            //grid.SetActivity(false);
                        }
                    }
                }
            }
        }

        bool CheckGridIntegrity(Creature c, bool moved)
        {
            Cell cur_cell = c.GetCurrentCell();
            Cell xy_cell = new Cell(c.GetPositionX(), c.GetPositionY());
            if (xy_cell != cur_cell)
            {
                Log.outDebug("Creature (GUID: {0}) X: {1} Y: {2} ({3}) is in grid[{4}, {5}]cell[{6}, {7}] instead of grid[{8}, {9}]cell[{10}, {11}]",
                    c.GetGUIDLow(), c.GetPositionX(), c.GetPositionY(), (moved ? "final" : "original"), cur_cell.GridX(), cur_cell.GridY(), cur_cell.CellX(),
                    cur_cell.CellY(), xy_cell.GridX(), xy_cell.GridY(), xy_cell.CellX(), xy_cell.CellY());
                return true;                                        // not crash at error, just output error in debug mode
            }

            return true;
        }
        public void CreatureRelocation(Creature creature, float x, float y, float z, float ang, bool respawnRelocationOnFail = true)
        {
            CheckGridIntegrity(creature, false);

            Cell old_cell = creature.GetCurrentCell();
            Cell new_cell = new Cell(x, y);

            if (!respawnRelocationOnFail && getGrid(new_cell.GridX(), new_cell.GridY()) == null)
                return;

            //! If hovering, always increase our server-side Z position
            //! Client automatically projects correct position based on Z coord sent in monster move
            //! and UNIT_FIELD_HOVERHEIGHT sent in object updates
            if (creature.HasUnitMovementFlag(MovementFlag.Hover))
                z += creature.GetValue<float>(UnitFields.HoverHeight);

            // delay creature move for grid/cell to grid/cell moves
            if (old_cell.DiffCell(new_cell) || old_cell.DiffGrid(new_cell))
            {
                Log.outDebug("Creature (GUID: {0} Entry: {1}) added to moving list from grid[{2}, {3}]cell[{4}, {5}] to grid[{6}, {7}]cell[{8}, {9}].",
                    creature.GetGUIDLow(), creature.GetEntry(), old_cell.GridX(), old_cell.GridY(), old_cell.CellX(), old_cell.CellY(), new_cell.GridX(),
                    new_cell.GridY(), new_cell.CellX(), new_cell.CellY());
                //AddCreatureToMoveList(creature, x, y, z, ang);
                // in diffcell/diffgrid case notifiers called at finishing move creature in Map::MoveAllCreaturesInMoveList
            }
            else
            {
                creature.Position.Relocate(x, y, z, ang);
                //if (creature.IsVehicle())
                //creature.GetVehicleKit().RelocatePassengers();
                creature.UpdateObjectVisibility(false);
                //RemoveCreatureFromMoveList(creature);
            }

            CheckGridIntegrity(creature, true);
        }



        //Sets
        //public void SetActive(bool active) { Active = active; }

        //Gets
        public Difficulty GetSpawnMode() { return i_spawnMode; }
        public WorldObject GetObject(uint guidLow)
        {
            return m_activeNonPlayers.FirstOrDefault(p => p.GetGUIDLow() == guidLow);
        }
        //public Player GetPlayer(ulong guidLow)
        //{
            //return m_activePlayers.FirstOrDefault(p => p.GetGUIDLow() == guidLow);
        //}
        bool IsCellActive(uint x, uint y)
        {
            //uint endX = ((x + 1) <= CellHandler.TotalCellsPerMap) ? x + 1 : (CellHandler.TotalCellsPerMap - 1);
            //uint endY = ((y + 1) <= CellHandler.TotalCellsPerMap) ? y + 1 : (CellHandler.TotalCellsPerMap - 1);
            //uint startX = x > 0 ? x - 1 : 0;
            //uint startY = y > 0 ? y - 1 : 0;
            //uint posX, posY;

            //Grid objCell;

            //for (posX = startX; posX <= endX; posX++)
            {
                //for (posY = startY; posY <= endY; posY++)
                {
                    //objCell = CellHandler.GetCell(posX, posY);

                    //if (objCell != null)
                    {
                        //if (objCell.HasPlayers())// || m_forcedcells.find(objCell) != m_forcedcells.end())
                        {
                            //return true;
                        }
                    }
                }
            }
            return false;
        }
        //public bool IsActive() { return Active; }
        public bool HasObject(WorldObject obj) { return m_activeNonPlayers.Contains(obj); }
        //public bool HasPlayer(Player pl) { return m_activePlayers.Contains(pl); }
        public uint GetInstanceId() { return i_InstanceId; }
        // Creature GetCreature(ulong guid)
        //{
        //return ObjMgr.GetObject<Creature>(this, guid);
        //}

        uint GetAreaFlag(float x, float y, float z)
        {
            bool throwaway;
            return GetAreaFlag(x, y, z, out throwaway);
        }
        uint GetAreaFlag(float x, float y, float z, out bool isOutdoors)
        {
            //uint mogpFlags;
            //int adtId, rootId, groupId;
            //WMOAreaTableEntry const* wmoEntry = 0;
            AreaTableEntry atEntry = null;
            bool haveAreaInfo = false;

            //if (GetAreaInfo(x, y, z, mogpFlags, adtId, rootId, groupId))
            {
                //haveAreaInfo = true;
                //wmoEntry = GetWMOAreaTableEntryByTripple(rootId, adtId, groupId);
                //if (wmoEntry)
                //atEntry = GetAreaEntryByAreaID(wmoEntry->areaId);
            }

            uint areaflag;

            if (atEntry != null)
                areaflag = atEntry.exploreFlag;
            else
            {

                GridMap gmap = GetGridMap(x, y);
                if (gmap != null)
                    areaflag = gmap.GetArea(x, y);
                // this used while not all *.map files generated (instances)
                else
                    areaflag = Cypher.ObjMgr.GetAreaFlagByMapId(GetId());
            }

            if (haveAreaInfo)
                isOutdoors = false;//IsOutdoorWMO(mogpFlags, adtId, rootId, groupId, wmoEntry, atEntry);
            else
                isOutdoors = true;

            return areaflag;
        }


        uint GetAreaId(float x, float y, float z)
        {
            return GetAreaIdByAreaFlag(GetAreaFlag(x, y, z), GetId());
        }

        public uint GetZoneId(float x, float y, float z)
        {
            return GetZoneIdByAreaFlag(GetAreaFlag(x, y, z), GetId());
        }

        public void GetZoneAndAreaId(out uint zoneid, out uint areaid, float x, float y, float z)
        {
            GetZoneAndAreaIdByAreaFlag(out zoneid, out areaid, GetAreaFlag(x, y, z), GetId());
        }
        public void GetZoneAndAreaId(float x, float y, float z, out uint zoneid, out uint areaid)
        {
            GetZoneAndAreaIdByAreaFlag(out zoneid, out areaid, GetAreaFlag(x, y, z), GetId());
        }
        void GetZoneAndAreaIdByAreaFlag(out uint zoneid, out uint areaid, uint areaflag, uint map_id)
        {
            AreaTableEntry entry = Cypher.ObjMgr.GetAreaEntryByAreaFlagAndMap(areaflag, map_id);

            areaid = entry != null ? entry.ID : 0;
            zoneid = entry != null ? ((entry.zone != 0) ? entry.zone : entry.ID) : 0;
        }


        uint GetAreaIdByAreaFlag(uint areaflag, uint map_id)
        {
            var entry = Cypher.ObjMgr.GetAreaEntryByAreaFlagAndMap(areaflag, map_id);

            if (entry != null)
                return entry.ID;
            else
                return 0;
        }
        uint GetZoneIdByAreaFlag(uint areaflag, uint map_id)
        {
            var entry = Cypher.ObjMgr.GetAreaEntryByAreaFlagAndMap(areaflag, map_id);

            if (entry != null)
                return (entry.zone != 0) ? entry.zone : entry.ID;
            else
                return 0;
        }

        public void SendInitSelf(Player pl)
        {
            UpdateData data = new UpdateData(pl.GetMapId());

            // attach to player data current transport data
            //if (Transport * transport = player->GetTransport())
            {
                //transport->BuildCreateUpdateBlockForPlayer(&data, player);
            }

            pl.BuildCreateUpdateBlockForSelf(ref data, pl);
            pl.knownObjects.Add(pl);

            // build other passengers at transport also (they always visible and marked as visible and will not send at visibility update at add to map
            //if (Transport* transport = player->GetTransport())
            {
                //for (Transport::PlayerSet::const_iterator itr = transport->GetPassengers().begin(); itr != transport->GetPassengers().end(); ++itr)
                {
                    //if (player != (*itr) && player->HaveAtClient(*itr))
                    {
                        // (*itr)->BuildCreateUpdateBlockForPlayer(&data, player);
                    }
                }
            }
            data.SendPackets(ref pl);
            Log.outDebug("Sent InitSelf for Player: {0} ({1})", pl.GetGUID(), pl.GetName());
        }







        //new shit
        public bool Instanceable() { return i_mapEntry != null && i_mapEntry.Instanceable(); }
        public bool IsDungeon() { return i_mapEntry != null && i_mapEntry.IsDungeon(); }
        public bool IsNonRaidDungeon() { return i_mapEntry != null && i_mapEntry.IsNonRaidDungeon(); }
        public bool IsRaid() { return i_mapEntry != null && i_mapEntry.IsRaid(); }
        public bool IsRaidOrHeroicDungeon() { return IsRaid() || i_spawnMode > Difficulty.DungeonNormal; }
        public bool IsHeroic() { return IsRaid() ? i_spawnMode >= Difficulty.Raid10manHeroic : i_spawnMode >= Difficulty.DungeonHeroic; }
        //public bool Is25ManRaid() { return IsRaid() && SpawnMode & RAID_DIFFICULTY_MASK_25MAN); }   // since 25man difficulties are 1 and 3, we can check them like that
        public bool IsBattleground() { return i_mapEntry != null && i_mapEntry.IsBattleGround(); }
        public bool IsBattleArena() { return i_mapEntry != null && i_mapEntry.IsBattleArena(); }
        public bool IsBattlegroundOrArena() { return i_mapEntry != null && i_mapEntry.IsBattleGroundOrArena(); }
        public bool GetEntrancePos(int mapid, float x, float y)
        {
            //if (MapInfo == null)
            return false;
            //return MapInfo.GetEntrancePos(mapid, x, y);
        }



    }
}
