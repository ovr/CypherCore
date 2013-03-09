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

using Framework.Configuration;
using Framework.Constants;
using Framework.DataStorage;
using Framework.Singleton;
using Framework.Utility;
using System.Collections.Generic;
using System.Linq;
using WorldServer.Game.Maps;

namespace WorldServer.Game.WorldEntities
{
    public class MapManager : SingletonBase<MapManager>
    {
        Dictionary<uint, Map> i_maps;
        IntervalTimer i_timer;
        IntervalTimer i_delayTimer;
        uint i_gridCleanUpDelay;

        MapManager()
        {
            Map.InitStateMachine();
            i_maps = new Dictionary<uint, Map>();
            i_gridCleanUpDelay = (uint)WorldConfig.GridCleanUpDelay;
            i_timer = new IntervalTimer();
            i_timer.SetInterval(WorldConfig.MapUpdateInterval);
            i_delayTimer = new IntervalTimer();
            i_delayTimer.SetInterval(WorldConfig.MapUpdateInterval * 300);
        }

        Map FindBaseMap(uint mapId)
        {
            return i_maps.LookupByKey(mapId);
        }

        Map CreateMap(uint id, Player player)
        {
            Map m = CreateBaseMap(id);

            //if (m != null && m.Instanceable())
                //m = ((MapInstance)m).CreateInstanceForPlayer(id, player);

            return m;
        }
        Map CreateBaseMap(uint id)
        {
            Map map = FindBaseMap(id);

            if (map == null)
            {
                //Lock?

                var entry = DBCStorage.MapStorage.LookupByKey(id);
                if (entry != null)
                {
                    if (entry.Instanceable())
                        map = new MapInstance(id, i_gridCleanUpDelay);
                    else
                    {
                        map = new Map(id, i_gridCleanUpDelay, 0, Difficulty.Regular);
                        //map->LoadRespawnTimes();
                    }
                    i_maps[id] = map;
                }
            }
            return map;
        }

        public Map CreateMap(uint id, WorldObject obj)
        {
            Map m = CreateBaseMap(id);

            //if (m != null && m.Instanceable())
            //m = ((MapInstanced*)m)->CreateInstanceForPlayer(id, player);

            return m;
        }

        public bool IsValidMAP(uint mapid, bool startUp)
        {
            MapEntry mEntry = DBCStorage.MapStorage.LookupByKey(mapid);

            if (startUp)
                return mEntry != null ? true : false;
            else
                return mEntry != null && (!mEntry.IsDungeon() || Cypher.ObjMgr.GetInstanceTemplate(mapid) != null);

            // TODO: add check for battleground template
        }

        public bool IsValidMapCoord(uint mapid, float x, float y)
        {
            return IsValidMAP(mapid, false) && GridDefines.IsValidMapCoord(x, y);
        }

        public bool IsValidMapCoord(uint mapid, float x, float y, float z)
        {
            return IsValidMAP(mapid, false) && GridDefines.IsValidMapCoord(x, y, z);
        }

        public bool IsValidMapCoord(uint mapid, float x, float y, float z, float o)
        {
            return (IsValidMAP(mapid, false) && GridDefines.IsValidMapCoord(x, y, z, o));
        }

        public void Update(uint diff)
        {
            i_timer.Update((int)diff);
            if (!i_timer.Passed())
                return;

            foreach (var map in i_maps.Values.ToList())
                map.Update((uint)i_timer.GetCurrent());

            DelayedUpdate(diff);

            //sObjectAccessor->Update(uint32(i_timer.GetCurrent()));
            //for (TransportSet::iterator itr = m_Transports.begin(); itr != m_Transports.end(); ++itr)
            //(*itr)->Update(uint32(i_timer.GetCurrent()));

            i_timer.SetCurrent(0);
        }
        public void DelayedUpdate(uint diff)
        {   
            i_delayTimer.Update((int)diff);
            if (!i_delayTimer.Passed())
                return;

            foreach (var map in i_maps.Values.ToList())
                map.DelayedUpdate((uint)i_timer.GetCurrent());

            i_delayTimer.SetCurrent(0);
        }

        void SetMapUpdateInterval(int t)
        {
            if (t < MapConst.MIN_MAP_UPDATE_DELAY)
                t = MapConst.MIN_MAP_UPDATE_DELAY;

            i_timer.SetInterval(t);
            i_timer.Reset();
        }
        void SetGridCleanUpDelay(uint t)
        {
            if (t < MapConst.MIN_GRID_DELAY)
                i_gridCleanUpDelay = MapConst.MIN_GRID_DELAY;
            else
                i_gridCleanUpDelay = t;
        }
    }
}
