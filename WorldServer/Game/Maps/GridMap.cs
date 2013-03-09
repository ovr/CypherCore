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
using System.IO;
using Framework.Configuration;
using Framework.Constants;
using Framework.Logging;
using Framework.Utility;
using WorldServer.Game.Managers;
using Framework.DataStorage;

namespace WorldServer.Game.Maps
{
    public class GridMap
    {
        public GridMap()
        {
            _flags = 0;

            // Area data
            _gridArea = 0;
            _areaMap = null;

            // Height level data
            _gridHeight = MapConst.INVALID_HEIGHT;
            _gridGetHeight = getHeightFromFlat();
            m_V9 = null;
            m_V8 = null;

            // Liquid data
            _liquidType = 0;
            _liquidOffX = 0;
            _liquidOffY = 0;
            _liquidWidth = 0;
            _liquidHeight = 0;
            _liquidLevel = MapConst.INVALID_HEIGHT;
            _liquidEntry = null;
            _liquidFlags = null;
            _liquidMap = null;
        }
        ~GridMap()
        {
            unloadData();
        }

        uint _flags;

        public float[] m_V9;
        public ushort[] m_uint16_V9;
        public byte[] m_uint8_V9;

        public float[] m_V8;
        public ushort[] m_uint16_V8;
        public byte[] m_uint8_V8;

        float _gridHeight;
        float _gridIntHeightMultiplier;
        float _gridGetHeight;

        //Area data
        public ushort[] _areaMap;

        //Liquid Map
        float _liquidLevel;
        ushort[] _liquidEntry;
        byte[] _liquidFlags;
        float[] _liquidMap;
        ushort _gridArea;
        ushort _liquidType;
        byte _liquidOffX;
        byte _liquidOffY;
        byte _liquidWidth;
        byte _liquidHeight;

        public unsafe float getHeightFromUint8(float x, float y)
        {
            if (m_uint8_V8 == null || m_uint8_V9 == null)
                return _gridHeight;

            x = MapConst.MapResolution * (32 - x / MapConst.SizeOfGrids);
            y = MapConst.MapResolution * (32 - y / MapConst.SizeOfGrids);

            int x_int = (int)x;
            int y_int = (int)y;
            x -= x_int;
            y -= y_int;
            x_int &= (MapConst.MapResolution - 1);
            y_int &= (MapConst.MapResolution - 1);
            int a, b, c;

            fixed (byte* V9 = m_uint8_V9)
            {
                byte* V9_h1_ptr = &V9[x_int * 128 + x_int + y_int];
                if (x + y < 1)
                {
                    if (x > y)
                    {
                        // 1 triangle (h1, h2, h5 points)
                        int h1 = V9_h1_ptr[0];
                        int h2 = V9_h1_ptr[129];
                        int h5 = 2 * m_uint8_V8[x_int * 128 + y_int];
                        a = h2 - h1;
                        b = h5 - h1 - h2;
                        c = h1;
                    }
                    else
                    {
                        // 2 triangle (h1, h3, h5 points)
                        int h1 = V9_h1_ptr[0];
                        int h3 = V9_h1_ptr[1];
                        int h5 = 2 * m_uint8_V8[x_int * 128 + y_int];
                        a = h5 - h1 - h3;
                        b = h3 - h1;
                        c = h1;
                    }
                }
                else
                {
                    if (x > y)
                    {
                        // 3 triangle (h2, h4, h5 points)
                        int h2 = V9_h1_ptr[129];
                        int h4 = V9_h1_ptr[130];
                        int h5 = 2 * m_uint8_V8[x_int * 128 + y_int];
                        a = h2 + h4 - h5;
                        b = h4 - h2;
                        c = h5 - h4;
                    }
                    else
                    {
                        // 4 triangle (h3, h4, h5 points)
                        int h3 = V9_h1_ptr[1];
                        int h4 = V9_h1_ptr[130];
                        int h5 = 2 * m_uint8_V8[x_int * 128 + y_int];
                        a = h4 - h3;
                        b = h3 + h4 - h5;
                        c = h5 - h4;
                    }
                }
                // Calculate height
                return (float)((a * x) + (b * y) + c) * _gridIntHeightMultiplier + _gridHeight;
            }
        }

        public unsafe float getHeightFromUint16(float x, float y)
        {
            if (m_uint16_V8 == null || m_uint16_V9 == null)
                return _gridHeight;

            x = MapConst.MapResolution * (32 - x / MapConst.SizeOfGrids);
            y = MapConst.MapResolution * (32 - y / MapConst.SizeOfGrids);

            int x_int = (int)x;
            int y_int = (int)y;
            x -= x_int;
            y -= y_int;
            x_int &= (MapConst.MapResolution - 1);
            y_int &= (MapConst.MapResolution - 1);
            int a, b, c;

            fixed (ushort* V9 = m_uint16_V9)
            {
                ushort* V9_h1_ptr = &V9[x_int * 128 + x_int + y_int];
                if (x + y < 1)
                {
                    if (x > y)
                    {
                        // 1 triangle (h1, h2, h5 points)
                        int h1 = V9_h1_ptr[0];
                        int h2 = V9_h1_ptr[129];
                        int h5 = 2 * m_uint16_V8[x_int * 128 + y_int];
                        a = h2 - h1;
                        b = h5 - h1 - h2;
                        c = h1;
                    }
                    else
                    {
                        // 2 triangle (h1, h3, h5 points)
                        int h1 = V9_h1_ptr[0];
                        int h3 = V9_h1_ptr[1];
                        int h5 = 2 * m_uint16_V8[x_int * 128 + y_int];
                        a = h5 - h1 - h3;
                        b = h3 - h1;
                        c = h1;
                    }
                }
                else
                {
                    if (x > y)
                    {
                        // 3 triangle (h2, h4, h5 points)
                        int h2 = V9_h1_ptr[129];
                        int h4 = V9_h1_ptr[130];
                        int h5 = 2 * m_uint16_V8[x_int * 128 + y_int];
                        a = h2 + h4 - h5;
                        b = h4 - h2;
                        c = h5 - h4;
                    }
                    else
                    {
                        // 4 triangle (h3, h4, h5 points)
                        int h3 = V9_h1_ptr[1];
                        int h4 = V9_h1_ptr[130];
                        int h5 = 2 * m_uint16_V8[x_int * 128 + y_int];
                        a = h4 - h3;
                        b = h3 + h4 - h5;
                        c = h5 - h4;
                    }
                }
                // Calculate height
                return (float)((a * x) + (b * y) + c) * _gridIntHeightMultiplier + _gridHeight;
            }
        }

        float getHeightFromFloat(float x, float y)
        {
            if (m_uint16_V8 == null || m_uint16_V9 == null)
                return _gridHeight;

            x = MapConst.MapResolution * (32 - x / MapConst.SizeOfGrids);
            y = MapConst.MapResolution * (32 - y / MapConst.SizeOfGrids);

            int x_int = (int)x;
            int y_int = (int)y;
            x -= x_int;
            y -= y_int;
            x_int &= (MapConst.MapResolution - 1);
            y_int &= (MapConst.MapResolution - 1);

            float a, b, c;
            if (x + y < 1)
            {
                if (x > y)
                {
                    // 1 triangle (h1, h2, h5 points)
                    float h1 = m_V9[(x_int) * 129 + y_int];
                    float h2 = m_V9[(x_int + 1) * 129 + y_int];
                    float h5 = 2 * m_V8[x_int * 128 + y_int];
                    a = h2 - h1;
                    b = h5 - h1 - h2;
                    c = h1;
                }
                else
                {
                    // 2 triangle (h1, h3, h5 points)
                    float h1 = m_V9[x_int * 129 + y_int];
                    float h3 = m_V9[x_int * 129 + y_int + 1];
                    float h5 = 2 * m_V8[x_int * 128 + y_int];
                    a = h5 - h1 - h3;
                    b = h3 - h1;
                    c = h1;
                }
            }
            else
            {
                if (x > y)
                {
                    // 3 triangle (h2, h4, h5 points)
                    float h2 = m_V9[(x_int + 1) * 129 + y_int];
                    float h4 = m_V9[(x_int + 1) * 129 + y_int + 1];
                    float h5 = 2 * m_V8[x_int * 128 + y_int];
                    a = h2 + h4 - h5;
                    b = h4 - h2;
                    c = h5 - h4;
                }
                else
                {
                    // 4 triangle (h3, h4, h5 points)
                    float h3 = m_V9[(x_int) * 129 + y_int + 1];
                    float h4 = m_V9[(x_int + 1) * 129 + y_int + 1];
                    float h5 = 2 * m_V8[x_int * 128 + y_int];
                    a = h4 - h3;
                    b = h3 + h4 - h5;
                    c = h5 - h4;
                }
            }
            // Calculate height
            return a * x + b * y + c;
        }
        float getHeightFromFlat()
        {
            return _gridHeight;
        }

        public float GetLiquidHeight(float x, float y)
        {
            if (_liquidMap == null)
                return _liquidLevel;

            x = MapConst.MapResolution * (32 - x / MapConst.SizeOfGrids);
            y = MapConst.MapResolution * (32 - y / MapConst.SizeOfGrids);

            int cx_int = ((int)x & (MapConst.MapResolution - 1)) - _liquidOffX;
            int cy_int = ((int)y & (MapConst.MapResolution - 1)) - _liquidOffY;

            if (cx_int < 0 || cx_int >= _liquidHeight)
                return MapConst.INVALID_HEIGHT;

            if (cy_int < 0 || cy_int >= _liquidWidth)
                return MapConst.INVALID_HEIGHT;

            return _liquidMap[cx_int * _liquidWidth + cy_int];
        }

        public uint GetArea(float x, float y)
        {
            if (_areaMap == null)
                return 0;

            x = 16 * (32 - x / MapConst.SizeOfGrids);
            y = 16 * (32 - y / MapConst.SizeOfGrids);
            int lx = (int)x & 15;
            int ly = (int)y & 15;
            return (uint)_areaMap[lx * 16 + ly];
        }

        public bool loadData(string filename)
        {
            unloadData();
            if (!File.Exists(filename))
            {
                Log.outError("Cant Find File {0}", filename);
                return true;
            }

            using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                TileMapHeader header = reader.ReadStruct<TileMapHeader>();
                if (header.buildMagic != 15595)
                {
                    Log.outError("Incorrect Client Map File {0}", filename);
                    return false;
                }

                if (header.areaMapOffset != 0)
                    LoadAreaData(reader);

                if (header.heightMapOffset != 0)
                    LoadHeightData(reader);

                if (header.liquidMapOffset != 0)
                    LoadLiquidData(reader);
            }
            return true;
        }
        public void unloadData()
        {
            _areaMap = null;
            m_V9 = null;
            m_V8 = null;
            _liquidEntry = null;
            _liquidFlags = null;
            _liquidMap = null;
            _gridGetHeight = getHeightFromFlat();
        }
        void LoadAreaData(BinaryReader reader)
        {
            map_AreaHeader areaHeader = reader.ReadStruct<map_AreaHeader>();

            _gridArea = areaHeader.gridArea;

            if (!Convert.ToBoolean(areaHeader.flags & MapConst.NoArea))
                reader.ReadValues(16 * 16, out _areaMap);
        }
        void LoadHeightData(BinaryReader reader)
        {
            map_HeightHeader mapHeader = reader.ReadStruct<map_HeightHeader>();

            _gridHeight = mapHeader.gridHeight;
            //_flags = mapHeader.flags;

            if (!Convert.ToBoolean(mapHeader.flags & MapConst.NoHeight))
            {
                if (Convert.ToBoolean(mapHeader.flags & MapConst.HeightAsInt16))
                {
                    reader.ReadValues(129 * 129, out m_uint16_V9);
                    reader.ReadValues(128 * 128, out m_uint16_V8);

                    _gridIntHeightMultiplier = (mapHeader.gridMaxHeight - mapHeader.gridHeight) / 65535;
                    _gridGetHeight = getHeightFromUint16(0, 0);
                }
                else if (Convert.ToBoolean(mapHeader.flags & MapConst.HeightAsInt8))
                {
                    reader.ReadValues(129 * 129, out m_uint8_V9);
                    reader.ReadValues(128 * 128, out m_uint8_V8);
                    _gridIntHeightMultiplier = (mapHeader.gridMaxHeight - mapHeader.gridHeight) / 255;
                    _gridGetHeight = getHeightFromUint8(0, 0);
                }
                else
                {
                    reader.ReadValues(129 * 129, out m_V9);
                    reader.ReadValues(128 * 128, out m_V8);
                    _gridGetHeight = getHeightFromFloat(0, 0);
                }
            }
            else
                _gridGetHeight = getHeightFromFlat();
        }
        void LoadLiquidData(BinaryReader reader)
        {
            map_LiquidHeader liquidHeader = reader.ReadStruct<map_LiquidHeader>();

            _liquidType = liquidHeader.liquidType;
            _liquidLevel = liquidHeader.liquidLevel;
            _liquidOffX = liquidHeader.offsetX;
            _liquidOffY = liquidHeader.offsetY;
            _liquidWidth = liquidHeader.width;
            _liquidHeight = liquidHeader.height;

            if (!Convert.ToBoolean(liquidHeader.flags & MapConst.LiquidNoType))
            {
                reader.ReadValues(16 * 16, out _liquidEntry);
                reader.ReadValues(16 * 16, out _liquidFlags);
            }

            if (!Convert.ToBoolean(liquidHeader.flags & MapConst.LiquidNoHeight))
                reader.ReadValues(_liquidWidth * _liquidHeight, out _liquidMap);
        }


        public float getHeight(float x, float y) { return this._gridGetHeight; }


        ushort getArea(float x, float y)
        {
            if (_areaMap == null)
                return _gridArea;

            x = 16 * (32 - x / MapConst.SizeOfGrids);
            y = 16 * (32 - y / MapConst.SizeOfGrids);
            int lx = (int)x & 15;
            int ly = (int)y & 15;
            return _areaMap[lx * 16 + ly];
        }
        // Get water state on map
        public ZLiquidStatus getLiquidStatus(float x, float y, float z, byte ReqLiquidType, LiquidData data)
        {
            // Check water type (if no water return)
            if (_liquidType == 0 && _liquidFlags == null)
                return ZLiquidStatus.LIQUID_MAP_NO_WATER;

            // Get cell
            float cx = MapConst.MapResolution * (32 - x / MapConst.SizeOfGrids);
            float cy = MapConst.MapResolution * (32 - y / MapConst.SizeOfGrids);

            int x_int = (int)cx & (MapConst.MapResolution - 1);
            int y_int = (int)cy & (MapConst.MapResolution - 1);

            // Check water type in cell
            int idx = (x_int >> 3) * 16 + (y_int >> 3);
            byte type = _liquidFlags != null ? _liquidFlags[idx] : (byte)_liquidType;
            uint entry = 0;
            if (_liquidEntry != null)
            {
                LiquidTypeEntry liquidEntry = DBCStorage.LiquidTypeStorage.LookupByKey(_liquidEntry[idx]);
                if (liquidEntry != null)
                {
                    entry = liquidEntry.Id;
                    type &= MapConst.MAP_LIQUID_TYPE_DARK_WATER;
                    uint liqTypeIdx = liquidEntry.Type;
                    if (entry < 21)
                    {
                        AreaTableEntry area = Cypher.ObjMgr.GetAreaEntryByAreaFlagAndMap(getArea(x, y), 0);// MAPID_INVALID);
                        if (area != null)
                        {
                            uint overrideLiquid = area.LiquidTypeOverride[liquidEntry.Type];
                            if (overrideLiquid == 0 && area.zone == 0)
                            {
                                area = Cypher.ObjMgr.GetAreaEntryByAreaID(area.zone);
                                if (area != null)
                                    overrideLiquid = area.LiquidTypeOverride[liquidEntry.Type];
                            }
                            LiquidTypeEntry liq = DBCStorage.LiquidTypeStorage.LookupByKey(overrideLiquid);
                            if (liq != null)
                            {
                                entry = overrideLiquid;
                                liqTypeIdx = liq.Type;
                            }
                        }
                    }
                    type |= (byte)(1 << (int)liqTypeIdx);
                }
            }

            if (type == 0)
                return ZLiquidStatus.LIQUID_MAP_NO_WATER;

            // Check req liquid type mask
            if (ReqLiquidType != 0 && !Convert.ToBoolean(ReqLiquidType & type))
                return ZLiquidStatus.LIQUID_MAP_NO_WATER;

            // Check water level:
            // Check water height map
            int lx_int = x_int - _liquidOffY;
            int ly_int = y_int - _liquidOffX;
            if (lx_int < 0 || lx_int >= _liquidHeight)
                return ZLiquidStatus.LIQUID_MAP_NO_WATER;
            if (ly_int < 0 || ly_int >= _liquidWidth)
                return ZLiquidStatus.LIQUID_MAP_NO_WATER;

            // Get water level
            float liquid_level = _liquidMap != null ? _liquidMap[lx_int * _liquidWidth + ly_int] : _liquidLevel;
            // Get ground level (sub 0.2 for fix some errors)
            float ground_level = getHeight(x, y);

            // Check water level and ground level
            if (liquid_level < ground_level || z < ground_level - 2)
                return ZLiquidStatus.LIQUID_MAP_NO_WATER;

            // All ok in water -> store data
            if (data != null)
            {
                data.entry = entry;
                data.type_flags = type;
                data.level = liquid_level;
                data.depth_level = ground_level;
            }

            // For speed check as int values
            float delta = liquid_level - z;

            if (delta > 2.0f)                   // Under water
                return ZLiquidStatus.LIQUID_MAP_UNDER_WATER;
            if (delta > 0.0f)                   // In water
                return ZLiquidStatus.LIQUID_MAP_IN_WATER;
            if (delta > -0.1f)                   // Walk on water
                return ZLiquidStatus.LIQUID_MAP_WATER_WALK;
            // Above water
            return ZLiquidStatus.LIQUID_MAP_ABOVE_WATER;
        }
    }
    public struct TileMapHeader
    {
        public uint mapMagic;
        public uint versionMagic;
        public uint buildMagic;
        public uint areaMapOffset;
        public uint areaMapSize;
        public uint heightMapOffset;
        public uint heightMapSize;
        public uint liquidMapOffset;
        public uint liquidMapSize;
        public uint holesOffset;
        public uint holesSize;
    }
    public struct map_AreaHeader
    {
        public uint fourcc;
        public ushort flags;
        public ushort gridArea;
    }
    public struct map_HeightHeader
    {
        public uint fourcc;
        public uint flags;
        public float gridHeight;
        public float gridMaxHeight;
    }
    public struct map_LiquidHeader
    {
        public uint fourcc;
        public ushort flags;
        public ushort liquidType;
        public byte offsetX;
        public byte offsetY;
        public byte width;
        public byte height;
        public float liquidLevel;
    }
    public class LiquidData
    {
        public uint type_flags;
        public uint entry;
        public float level;
        public float depth_level;
    }
}
