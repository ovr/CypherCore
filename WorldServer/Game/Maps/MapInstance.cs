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

namespace WorldServer.Game.Maps
{
    public class MapInstance : Map
    {
        public MapInstance(uint id, uint UnloadGridTime) 
            : base (id, UnloadGridTime, 0, Difficulty.DungeonNormal)
        {
            //m_InstancedMaps.clear();
            //memset(&GridMapReference, 0, MAX_NUMBER_OF_GRIDS*MAX_NUMBER_OF_GRIDS*sizeof(uint16));
        }
    }

    public class InstanceTemplate
    {
        public uint Parent;
        public uint ScriptId;
        public bool AllowMount;
    }
}
