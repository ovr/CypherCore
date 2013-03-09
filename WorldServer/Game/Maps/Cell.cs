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

namespace WorldServer.Game.Maps
{
    public class Cell
    {
        public Cell(CoordPair p)
        {
            grid_x = p.x_coord / MapConst.MAX_NUMBER_OF_CELLS;
            grid_y = p.y_coord / MapConst.MAX_NUMBER_OF_CELLS;
            cell_x = p.x_coord % MapConst.MAX_NUMBER_OF_CELLS;
            cell_y = p.y_coord % MapConst.MAX_NUMBER_OF_CELLS;
            nocreate = false;
            //reserved = 0;
        }
        public Cell(float x, float y)
        {
            CoordPair p = GridDefines.ComputeCellCoord(x, y);
            grid_x = p.x_coord / MapConst.MAX_NUMBER_OF_CELLS;
            grid_y = p.y_coord / MapConst.MAX_NUMBER_OF_CELLS;
            cell_x = p.x_coord % MapConst.MAX_NUMBER_OF_CELLS;
            cell_y = p.y_coord % MapConst.MAX_NUMBER_OF_CELLS;
            nocreate = false;
            //reserved = 0;
        }

        public bool IsCellValid()
        {
            return cell_x < MapConst.MAX_NUMBER_OF_CELLS && cell_y < MapConst.MAX_NUMBER_OF_CELLS;
        }

        public uint GetId()
        {
            return (uint)(grid_x * MapConst.MAX_NUMBER_OF_GRIDS + grid_y);
        }

        public bool NoCreate() { return nocreate; }
        public void SetNoCreate() { nocreate = true; }

        public uint CellX() { return cell_x; }
        public uint CellY() { return cell_y; }
        public uint GridX() { return grid_x; }
        public uint GridY() { return grid_y; }

        uint grid_x;
        uint grid_y;
        public uint cell_x;
        public uint cell_y;
        bool nocreate;

        public CellCoord GetCellCoord()
        {
            return new CellCoord(
                grid_x * MapConst.MAX_NUMBER_OF_CELLS + cell_x,
                grid_y * MapConst.MAX_NUMBER_OF_CELLS + cell_y);
        }

        public bool DiffCell(Cell cell)
        {
            return (cell_x != cell.cell_x ||
                cell_y != cell.cell_y);
        }

        public bool DiffGrid(Cell cell)
        {
            return (grid_x != cell.grid_x ||
                grid_y != cell.grid_y);
        }

        public void Visit<T>(CellCoord standing_cell, Visitor<T> visitor, Map map, WorldObject obj, float radius) where T : Notifier
        {
            //we should increase search radius by object's radius, otherwise
            //we could have problems with huge creatures, which won't attack nearest players etc
            Visit(standing_cell, visitor, map, radius + obj.GetObjectSize(), obj.GetPositionX(), obj.GetPositionY());
        }
        public void Visit<T>(CoordPair standing_cell, Visitor<T> visitor, Map map, float radius, float x_off, float y_off) where T : Notifier
        {
            if (!standing_cell.IsCoordValid())
                return;

            //no jokes here... Actually placing ASSERT() here was good idea, but
            //we had some problems with DynamicObjects, which pass radius = 0.0f (DB issue?)
            //maybe it is better to just return when radius <= 0.0f?
            if (radius <= 0.0f)
            {
                map.Visit(this, visitor);
                return;
            }
            //lets limit the upper value for search radius
            if (radius > MapConst.SizeOfGrids)
                radius = MapConst.SizeOfGrids;

            //lets calculate object coord offsets from cell borders.
            CellArea area = new CellArea(x_off, y_off, radius);
            //if radius fits inside standing cell
            if (area == null)
            {
                map.Visit(this, visitor);
                return;
            }

            //visit all cells, found in CalculateCellArea()
            //if radius is known to reach cell area more than 4x4 then we should call optimized VisitCircle
            //currently this technique works with MAX_NUMBER_OF_CELLS 16 and higher, with lower values
            //there are nothing to optimize because SIZE_OF_GRID_CELL is too big...
            if ((area.high_bound.x_coord > (area.low_bound.x_coord + 4)) && (area.high_bound.y_coord > (area.low_bound.y_coord + 4)))
            {
                VisitCircle(visitor, map, area.low_bound, area.high_bound);
                return;
            }

            //ALWAYS visit standing cell first!!! Since we deal with small radiuses
            //it is very essential to call visitor for standing cell firstly...
            map.Visit(this, visitor);

            // loop the cell range
            for (uint x = area.low_bound.x_coord; x <= area.high_bound.x_coord; ++x)
            {
                for (uint y = area.low_bound.y_coord; y <= area.high_bound.y_coord; ++y)
                {
                    CellCoord cellCoord = new CellCoord(x, y);
                    //lets skip standing cell since we already visited it
                    if (cellCoord != standing_cell)
                    {
                        Cell r_zone = new Cell(cellCoord);
                        r_zone.nocreate = this.nocreate;
                        map.Visit(r_zone, visitor);
                    }
                }
            }
        }
        void VisitCircle<T>(Visitor<T> visitor, Map map, CoordPair begin_cell, CoordPair end_cell) where T : Notifier
        {
            //here is an algorithm for 'filling' circum-squared octagon
            uint x_shift = (uint)Math.Ceiling((end_cell.x_coord - begin_cell.x_coord) * 0.3f - 0.5f);
            //lets calculate x_start/x_end coords for central strip...
            uint x_start = begin_cell.x_coord + x_shift;
            uint x_end = end_cell.x_coord - x_shift;

            //visit central strip with constant width...
            for (uint x = x_start; x <= x_end; ++x)
            {
                for (uint y = begin_cell.y_coord; y <= end_cell.y_coord; ++y)
                {
                    CellCoord cellCoord = new CellCoord(x, y);
                    Cell r_zone = new Cell(cellCoord);
                    r_zone.nocreate = this.nocreate;
                    map.Visit(r_zone, visitor);
                }
            }

            //if x_shift == 0 then we have too small cell area, which were already
            //visited at previous step, so just return from procedure...
            if (x_shift == 0)
                return;

            uint y_start = end_cell.y_coord;
            uint y_end = begin_cell.y_coord;
            //now we are visiting borders of an octagon...
            for (uint step = 1; step <= (x_start - begin_cell.x_coord); ++step)
            {
                //each step reduces strip height by 2 cells...
                y_end += 1;
                y_start -= 1;
                for (uint y = y_start; y >= y_end; --y)
                {
                    //we visit cells symmetrically from both sides, heading from center to sides and from up to bottom
                    //e.g. filling 2 trapezoids after filling central cell strip...
                    CellCoord cellCoord_left = new CellCoord(x_start - step, y);
                    Cell r_zone_left = new Cell(cellCoord_left);
                    r_zone_left.nocreate = this.nocreate;
                    map.Visit(r_zone_left, visitor);

                    //right trapezoid cell visit
                    CellCoord cellCoord_right = new CellCoord(x_end + step, y);
                    Cell r_zone_right = new Cell(cellCoord_right);
                    r_zone_right.nocreate = this.nocreate;
                    map.Visit(r_zone_right, visitor);
                }
            }
        }
    }


    public class CellArea
    {
        public CellArea() { }
        public CellArea(float x, float y, float radius)
        {
            if (radius <= 0.0f)
            {
                CoordPair center = GridDefines.ComputeCellCoord(x, y).normalize();
                low_bound = center;
                high_bound = center;
                return;
            }
            CoordPair centerX = GridDefines.ComputeCellCoord(x - radius, y - radius).normalize();
            CoordPair centerY = GridDefines.ComputeCellCoord(x + radius, y + radius).normalize();
            low_bound = centerX;
            high_bound = centerY;
        }

        void ResizeBorders(CoordPair begin_cell, CoordPair end_cell)
        {
            begin_cell = low_bound;
            end_cell = high_bound;
        }

        public CoordPair low_bound;
        public CoordPair high_bound;
    }
}
