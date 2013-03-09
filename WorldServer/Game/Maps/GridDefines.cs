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

namespace WorldServer.Game.Maps
{
    class GridDefines
    {
        static Tuple<uint, uint> Compute(float CenterVal, float x, float y, float center_offset, float size)
        {
            // calculate and store temporary values in double format for having same result as same mySQL calculations
            double x_offset = ((double)x - center_offset) / size;
            double y_offset = ((double)y - center_offset) / size;

            uint x_val = (uint)(x_offset + CenterVal + 0.5f);
            uint y_val = (uint)(y_offset + CenterVal + 0.5f);
            return new Tuple<uint,uint>(x_val, y_val);
        }

        public static GridCoord ComputeGridCoord(float x, float y)
        {
            return new GridCoord(Compute(MapConst.CENTER_GRID_ID, x, y, MapConst.CENTER_GRID_OFFSET, MapConst.SizeOfGrids));
        }

        public static CellCoord ComputeCellCoord(float x, float y)
        {
            return new CellCoord(Compute(MapConst.CENTER_GRID_CELL_ID, x, y, MapConst.CENTER_GRID_CELL_OFFSET, MapConst.SIZE_OF_GRID_CELL));
        }

        public static bool IsValidMapCoord(float c)
        {
            var bl = Math.Abs(c);
            if (Math.Abs(c) <= (MapConst.MAP_HALFSIZE - 0.5f))
                return true;
            else
                return false;
        }

        public static bool IsValidMapCoord(float x, float y)
        {
            return (IsValidMapCoord(x) && IsValidMapCoord(y));
        }
        //todo: fix me
        public static bool IsValidMapCoord(float x, float y, float z)
        {
            return IsValidMapCoord(x, y);// && !float.IsInfinity(z);
        }

        public static bool IsValidMapCoord(float x, float y, float z, float o)
        {
            if (float.IsNaN(0) || float.IsNaN(z) || float.IsInfinity(o))
            {

            }

            return IsValidMapCoord(x, y, z);// && !float.IsInfinity(o);
        }
    }

    public class CellCoord : CoordPair
    {
        public CellCoord(uint x, uint y) : base(MapConst.TOTAL_NUMBER_OF_CELLS_PER_MAP, x, y) { }
        public CellCoord(Tuple<uint, uint> pair) : base(MapConst.TOTAL_NUMBER_OF_CELLS_PER_MAP, pair.Item1, pair.Item2) { }
    }
    public class GridCoord : CoordPair
    {
        public GridCoord(uint x, uint y) : base(MapConst.MAX_NUMBER_OF_GRIDS, x, y) { }
        public GridCoord(Tuple<uint, uint> pair) : base(MapConst.MAX_NUMBER_OF_GRIDS, pair.Item1, pair.Item2) { }
    }

    public class CoordPair
    {
        public CoordPair(uint limit, uint x = 0, uint y = 0)
        {
            Limit = limit;
            x_coord = x;
            y_coord = y;
        }

        public CoordPair(CoordPair obj)
        {
            Limit = obj.Limit;
            x_coord = obj.x_coord;
            y_coord = obj.y_coord;
        }

        //CoordPair<LIMIT> & operator=(const CoordPair<LIMIT> &obj)
        //{
        //x_coord = obj.x_coord;
        //y_coord = obj.y_coord;
        //return *this;
        //}

        public void dec_x(uint val)
        {
            if (x_coord > val)
                x_coord -= val;
            else
                x_coord = 0;
        }

        public void inc_x(uint val)
        {
            if (x_coord + val < Limit)
                x_coord += val;
            else
                x_coord = Limit - 1;
        }

        public void dec_y(uint val)
        {
            if (y_coord > val)
                y_coord -= val;
            else
                y_coord = 0;
        }

        public void inc_y(uint val)
        {
            if (y_coord + val < Limit)
                y_coord += val;
            else
                y_coord = Limit - 1;
        }

        public bool IsCoordValid()
        {
            return x_coord < Limit && y_coord < Limit;
        }

        public CoordPair normalize()
        {
            x_coord = Math.Min(x_coord, Limit - 1);
            y_coord = Math.Min(y_coord, Limit - 1);
            return this;
        }

        public uint GetId()
        {
            return y_coord * Limit + x_coord;
        }

        public uint x_coord;
        public uint y_coord;
        uint Limit;
    }
}
