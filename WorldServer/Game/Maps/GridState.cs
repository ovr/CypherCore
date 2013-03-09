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
using Framework.Logging;
using WorldServer.Game.WorldEntities;

namespace WorldServer.Game.Maps
{
    public class GridState
    {
        ~GridState() { }
        public virtual void Update(Map m, Grid grid, GridInfo info, uint t_diff) { }
    }

    public class InvalidState : GridState
    {
        public override void Update(Map m, Grid grid, GridInfo info, uint t_diff) { }
    }

    public class ActiveState : GridState
    {
        public override void Update(Map m, Grid grid, GridInfo info, uint t_diff)
        {
            // Only check grid activity every (grid_expiry/10) ms, because it's really useless to do it every cycle
            info.UpdateTimeTracker((int)t_diff);
            if (info.getTimeTracker().Passed())
            {
                if (grid.GetWorldObjectCountInNGrid<Player>() == 0 && !m.ActiveObjectsNearGrid(grid))
                {
                    ObjectGridStoper worker = new ObjectGridStoper();
                    var visitor = new Visitor<ObjectGridStoper>(worker, NotifierObjectType.Grid);
                    grid.VisitAllCells(visitor);
                    grid.SetGridState(GridState_t.Idle);
                    Log.outDebug("Grid[{0}, {1}] on map {2} moved to IDLE state", grid.getX(), grid.getY(), m.GetId());
                }
                else
                {
                    m.ResetGridExpiry(grid, 0.1f);
                }

            }
        }
    }

    public class IdleState : GridState
    {
        public override void Update(Map m, Grid grid, GridInfo info, uint t_diff)
        {
            m.ResetGridExpiry(grid);
            grid.SetGridState(GridState_t.Removal);
            Log.outDebug("Grid[{0}, {1}] on map {2} moved to REMOVAL state", grid.getX(), grid.getY(), m.GetId());
        }
    }

    public class RemovalState : GridState
    {
        public override void Update(Map m, Grid grid, GridInfo info, uint t_diff)
        {
            if (!info.getUnloadLock())
            {
                info.UpdateTimeTracker((int)t_diff);
                if (info.getTimeTracker().Passed())
                {
                    if (!m.UnloadGrid(grid, false))
                    {
                        Log.outDebug("Grid[{0}, {1}] for map {2} differed unloading due to players or active objects nearby", grid.getX(), grid.getY(), m.GetId());
                        m.ResetGridExpiry(grid);
                    }
                }
            }
        }
    }
}
