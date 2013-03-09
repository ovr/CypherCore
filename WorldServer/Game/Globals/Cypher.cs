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

using WorldServer.Game.WorldEntities;

namespace WorldServer.Game
{
    public class Cypher
    {
        public static WorldManager WorldMgr;
        public static AccountManager AcctMgr;
        public static SpellManager SpellMgr;
        public static ObjectManager ObjMgr;
        public static GuildManager GuildMgr;
        public static ItemManager ItemMgr;
        public static MapManager MapMgr;

        public static void Initialize()
        {
            AcctMgr = AccountManager.GetInstance();
            SpellMgr = SpellManager.GetInstance();
            ObjMgr = ObjectManager.GetInstance();
            GuildMgr = GuildManager.GetInstance();
            ItemMgr = ItemManager.GetInstance();
            MapMgr = MapManager.GetInstance();
            
            WorldMgr = WorldManager.GetInstance();
        }
    }
}
