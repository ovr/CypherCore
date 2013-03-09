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

namespace Framework.Constants
{
    public class MapConst
    {
        //Grids
        public const int MAX_NUMBER_OF_GRIDS = 64;
        public const float SizeOfGrids = 533.33333f;
        public const float CENTER_GRID_CELL_ID = (MAX_NUMBER_OF_CELLS * MAX_NUMBER_OF_GRIDS / 2);
        public const float CENTER_GRID_ID = (MAX_NUMBER_OF_GRIDS / 2);
        public const float CENTER_GRID_OFFSET = (SizeOfGrids / 2);

        //Cells
        public const int MAX_NUMBER_OF_CELLS = 8;
        public const float SIZE_OF_GRID_CELL = (SizeOfGrids / MAX_NUMBER_OF_CELLS);
        public const float CENTER_GRID_CELL_OFFSET = (SIZE_OF_GRID_CELL / 2);
        public const int TOTAL_NUMBER_OF_CELLS_PER_MAP = (MAX_NUMBER_OF_GRIDS * MAX_NUMBER_OF_CELLS);

        public const float MAP_SIZE = (SizeOfGrids * MAX_NUMBER_OF_GRIDS);
        public const float MAP_HALFSIZE = (MAP_SIZE / 2);
        public const float DEFAULT_HEIGHT_SEARCH = 50.0f;
        public const float INVALID_HEIGHT = -100000.0f;

        //Liquid
        public const int MAP_LIQUID_TYPE_NO_WATER = 0x00;
        public const int MAP_LIQUID_TYPE_WATER = 0x01;
        public const int MAP_LIQUID_TYPE_OCEAN = 0x02;
        public const int MAP_LIQUID_TYPE_MAGMA = 0x04;
        public const int MAP_LIQUID_TYPE_SLIME = 0x08;
        public const int MAP_LIQUID_TYPE_DARK_WATER = 0x10;
        public const int MAP_LIQUID_TYPE_WMO_WATER = 0x20;
        public const int MAP_ALL_LIQUIDS = (MAP_LIQUID_TYPE_WATER | MAP_LIQUID_TYPE_OCEAN | MAP_LIQUID_TYPE_MAGMA | MAP_LIQUID_TYPE_SLIME);


        public const int MIN_MAP_UPDATE_DELAY = 50;
        public const int MIN_GRID_DELAY = (TimeConst.MINUTE * TimeConst.IN_MILLISECONDS);

        public const int NoArea = 0x0001;
        public const int NoHeight = 0x0001;
        public const int HeightAsInt16 = 0x0002;
        public const int HeightAsInt8 = 0x0004;
        public const int LiquidNoType = 0x0001;
        public const int LiquidNoHeight = 0x0002;
        public const int MapResolution = 128;
    }
    public enum ZLiquidStatus
    {
        LIQUID_MAP_NO_WATER = 0x00000000,
        LIQUID_MAP_ABOVE_WATER = 0x00000001,
        LIQUID_MAP_WATER_WALK = 0x00000002,
        LIQUID_MAP_IN_WATER = 0x00000004,
        LIQUID_MAP_UNDER_WATER = 0x00000008
    }
}
