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
using Framework.Constants;
using WorldServer.Game.WorldEntities;
using Framework.Logging;

namespace WorldServer.Game.Maps
{
    public class GridLoader : Notifier
    {
        public GridLoader(Grid grid, Map map, Cell cell)
        {
            i_cell = cell;
            i_grid = grid;
            i_map = map;
            i_gameObjects = 0;
            i_creatures = 0;
            i_corpses = 0;
        }

        public void LoadGrid()
        {
            i_gameObjects = 0; i_creatures = 0; i_corpses = 0;
            i_cell.cell_y = 0;
            for (uint x = 0; x < MapConst.MAX_NUMBER_OF_CELLS; ++x)
            {
                i_cell.cell_x = x;
                for (uint y = 0; y < MapConst.MAX_NUMBER_OF_CELLS; ++y)
                {
                    i_cell.cell_y = y;
                    
                    //Load creatures and game objects
                    var visitor = new Visitor<GridLoader>(this, NotifierObjectType.Grid);
                    i_grid.VisitCell(x, y, visitor);
                    
                    //Load corpses (not bones)
                    visitor = new Visitor<GridLoader>(this, NotifierObjectType.Object);
                    i_grid.VisitCell(x, y, visitor);
                    //i_corpses += visitor.i_corpses;
                }
            }
            Log.outDebug("{0} GameObjects, {1} Creatures, and {2} Corpses/Bones loaded for grid {3} on map {4}", i_gameObjects, i_creatures, i_corpses, i_grid.GetGridId(), i_map.GetId());
        }

        void LoadObject<T>(List<uint> objs, CellCoord cellCoord, ref uint count) where T : WorldObject, new()
        {
            foreach (var guid in objs)
            {
                T obj = new T();
                if (!obj.LoadFromDB(guid, i_map))
                    continue;

                Cell cell = new Cell(cellCoord);

                i_map.AddToGrid(obj, cell);
                i_map.AddToActive(obj);
                obj.AddToWorld();
                count++;
            }
        }

        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            switch (type)
            {
                case NotifierObjectType.Grid:
                    VisitGameObjects(ref objs);
                    VisitCreatures(ref objs);
                    break;
                case NotifierObjectType.Object:
                    VisitCorpses(ref objs);
                    break;
            }            
        }

        void VisitGameObjects(ref List<WorldObject> m)
        {
            CellCoord cellCoord = i_cell.GetCellCoord();
            CellObjectGuids cell_guids = Cypher.ObjMgr.GetOrCreateCellObjectGuids(i_map.GetId(), (byte)i_map.GetSpawnMode(), cellCoord.GetId());
            if (cell_guids == null)
                return;
            LoadHelper<GameObject>(cell_guids.gameobjects, cellCoord, ref m, ref i_gameObjects, i_map);
        }
        void VisitCreatures(ref List<WorldObject> m)
        {
            CellCoord cellCoord = i_cell.GetCellCoord();
            CellObjectGuids cell_guids = Cypher.ObjMgr.GetOrCreateCellObjectGuids(i_map.GetId(), (byte)i_map.GetSpawnMode(), cellCoord.GetId());
            if (cell_guids == null)
                return;
            LoadHelper<Creature>(cell_guids.creatures, cellCoord, ref m, ref i_creatures, i_map);
        }
        void VisitCorpses(ref List<WorldObject> m) 
        {
            CellCoord cellCoord = i_cell.GetCellCoord();
            // corpses are always added to spawn mode 0 and they are spawned by their instance id
            CellObjectGuids cell_guids = Cypher.ObjMgr.GetOrCreateCellObjectGuids(i_map.GetId(), 0, cellCoord.GetId());
            if (cell_guids == null)
                return;
            LoadHelper<Corpse>(cell_guids.corpses, cellCoord, ref m, ref i_corpses, i_map);
        }
        void Visit(DynamicObject m) 
        {
            //nothing
        }

        void LoadHelper<T>(List<uint> guid_set, CellCoord cell, ref List<WorldObject> m, ref uint count, Map map) where T : WorldObject, new()
        {
            foreach (var i_guid in guid_set)
            {
                T obj = new T();
                uint guid = i_guid;
                if (!obj.LoadFromDB(guid, map))
                    continue;

                AddObjectHelper(cell, ref m, ref count, map, obj);
            }
        }
        void LoadHelper<T>(Dictionary<uint, uint> cell_corpses, CellCoord cell, ref List<WorldObject> m, ref uint count, Map map) where T : WorldObject
        {
            if (cell_corpses.Count == 0)
                return;

            foreach (var itr in cell_corpses)
            {
                if (itr.Value != map.GetInstanceId())
                    continue;

                uint player_guid = itr.Key;

                Corpse obj = null;// sObjectAccessor->GetCorpseForPlayerGUID(player_guid);
                if (obj == null)
                    continue;

                // TODO: this is a hack
                // corpse's map should be reset when the map is unloaded
                // but it may still exist when the grid is unloaded but map is not
                // in that case map == currMap
                obj.SetMap(map);

                //if (obj.IsInGrid())
                {
                    //obj.AddToWorld();
                    //continue;
                }

                AddObjectHelper(cell, ref m, ref count, map, obj);
            }
        }

        void AddObjectHelper<T>(CellCoord cell, ref List<WorldObject> m, ref uint count, Map map, T obj) where T : WorldObject
        {
            map.AddToGrid(obj, new Cell(cell));
            GridLoader.SetObjectCell(obj, cell);
            obj.AddToWorld();
            if (obj.isActiveObject())
                map.AddToActive(obj);

            ++count;
        }

        static void SetObjectCell(Creature obj, CellCoord cellCoord)
        {
            Cell cell = new Cell(cellCoord);
            //obj.SetCurrentCell(cell);
        }
        static void SetObjectCell<T>(T obj, CellCoord cellCoord) where T : WorldObject { }

        Cell i_cell;
        Grid i_grid;
        Map i_map;
        uint i_gameObjects;
        uint i_creatures;
        uint i_corpses;
    }

    //Stop the creatures before unloading the Grid
    public class ObjectGridStoper : Notifier
    {
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs)
            {
                if (obj is Creature)
                    Visit((Creature)obj);
            }
        }
        void Visit(Creature m)
        {
            // stop any fights at grid de-activation and remove dynobjects created at cast by creatures
            //m.RemoveAllDynObjects();
            if (m.isInCombat())
            {
                //m.CombatStop();
                //m.DeleteThreatList();
                //m.AI().EnterEvadeMode();
            }

        }
    }

    //Move the foreign creatures back to respawn positions before unloading the Grid
    public class ObjectGridEvacuator : Notifier
    {
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs)
            {
                if (obj is Creature)
                    Visit((Creature)obj);
            }
        }
        void Visit(Creature c)
        {
            // creature in unloading grid can have respawn point in another grid
            // if it will be unloaded then it will not respawn in original grid until unload/load original grid
            // move to respawn point to prevent this case. For player view in respawn grid this will be normal respawn.
            //c.GetMap().CreatureRespawnRelocation(c, true);
        }
    }

    //Clean up and remove from world
    public class ObjectGridCleaner : Notifier
    {
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs)
                obj.CleanupsBeforeDelete();
        }
    }

    //Delete objects before deleting Grid
    public class ObjectGridUnloader : Notifier
    {
        public override void Visit(ref List<WorldObject> objs, NotifierObjectType type)
        {
            foreach (var obj in objs)
            {
                // if option set then object already saved at this moment
                //if (!sWorld->getBoolConfig(CONFIG_SAVE_RESPAWN_TIME_IMMEDIATELY))
                    //obj.SaveRespawnTime();
                //Some creatures may summon other temp summons in CleanupsBeforeDelete()
                //So we need this even after cleaner (maybe we can remove cleaner)
                //Example: Flame Leviathan Turret 33139 is summoned when a creature is deleted
                //TODO: Check if that script has the correct logic. Do we really need to summons something before deleting?
                obj.CleanupsBeforeDelete();
            }
        }
    }
}
