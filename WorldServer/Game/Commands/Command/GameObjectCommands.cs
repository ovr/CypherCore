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
using WorldServer.Game.Managers;
using Framework.DataStorage;
using WorldServer.Game.Maps;

namespace WorldServer.Game.Commands.Command
{
    [CommandGroup("gobject", "", AccountTypes.GameMaster)]
    public class GameObjectCommands : CommandGroup
    {
        [Command("activate", "", AccountTypes.GameMaster)]
        public static bool Activate(string[] args, CommandGroup command)
        {
            return false;
        }
        [Command("delete", "", AccountTypes.GameMaster)]
        public static bool Delete(string[] args, CommandGroup command)
        {
            return false;
        }
        [Command("info", "", AccountTypes.GameMaster)]
        public static bool Info(string[] args, CommandGroup command)
        {
            return false;
        }
        [Command("move", "", AccountTypes.GameMaster)]
        public static bool Move(string[] args, CommandGroup command)
        {
            return false;
        }
        [Command("near", "", AccountTypes.GameMaster)]
        public static bool Near(string[] args, CommandGroup command)
        {
            return false;
        }
        [Command("target", "", AccountTypes.GameMaster)]
        public static bool Target(string[] args, CommandGroup command)
        {
            return false;
        }
        [Command("turn", "", AccountTypes.GameMaster)]
        public static bool Turn(string[] args, CommandGroup command)
        {
            return false;
        }

        [CommandGroup("add", "", AccountTypes.GameMaster)]
        public class AddCommands : CommandGroup
        {
            //same here
            public override bool Fallback(string[] @params = null, CommandGroup cmd = null)
            {
                if (@params == null || cmd == null || @params.Count() < 2)
                    return false;

                uint objectId;
                uint.TryParse(@params[0], out objectId);

                if (objectId == 0)
                    return false;

                uint spawntimeSecs;
                uint.TryParse(@params[1], out spawntimeSecs);

                GameObjectTemplate objectInfo = ObjMgr.GetGameObjectTemplate(objectId);

                if (objectInfo == null)
                {
                    cmd.SendErrorMessage(CypherStrings.GameobjectNotExist, objectId);
                    return false;
                }

                //if (objectInfo.displayId != 0 && !DBCStorage.sGameObjectDisplayInfoStore.LookupEntry(objectInfo->displayId))
                {
                    // report to DB errors log as in loading case
                    //sLog->outError(LOG_FILTER_SQL, "Gameobject (Entry %u GoType: %u) have invalid displayId (%u), not spawned.", objectId, objectInfo->type, objectInfo->displayId);
                    //handler->PSendSysMessage(LANG_GAMEOBJECT_HAVE_INVALID_DATA, objectId);
                    //return false;
                }

                Player player = cmd.GetSession().GetPlayer();
                float x = player.GetPositionX();
                float y = player.GetPositionY();
                float z = player.GetPositionZ();
                float o = player.GetOrientation();
                Map map = player.GetMap();

                GameObject obj = new GameObject();
                uint guidLow = ObjMgr.GenerateLowGuid(HighGuidType.GameObject);

                if (!obj.Create(guidLow, objectInfo.entry, map, 1/*player.GetPhaseMgr().GetPhaseMaskForSpawn()*/, x, y, z, o, 0.0f, 0.0f, 0.0f, 0.0f, 0, GameObjectState.Ready))
                    return false;

                //if (spawntimeSecs != 0)
                {
                    //obj.SetRespawnTime(spawntimeSecs);
                }

                // fill the gameobject data and save to the db
                obj.SaveToDB(map.GetId(), (byte)(1 << (int)map.GetSpawnMode()), 1);//player.GetPhaseMgr().GetPhaseMaskForSpawn());

                // this will generate a new guid if the object is in an instance
                if (!obj.LoadGameObjectFromDB(guidLow, map))
                    return false;

                // TODO: is it really necessary to add both the real and DB table guid here ?
                ObjMgr.AddGameObjectToGrid(guidLow, ObjMgr.GetGOData(guidLow));
                cmd.SendSysMessage(CypherStrings.GameobjectAdd, objectId, objectInfo.name, guidLow, x, y, z);
                return true;
            }
            public static bool AddCommand(string[] args, CommandGroup command)
            {
                if (args == null || args.Count() < 2)
                    return false;

                uint objectId;
                uint.TryParse(args[0], out objectId);

                if (objectId == 0)
                    return false;
                
                uint spawntimeSecs;
                uint.TryParse(args[1], out spawntimeSecs);
                
                GameObjectTemplate objectInfo = ObjMgr.GetGameObjectTemplate(objectId);

                if (objectInfo == null)
                {
                    command.SendErrorMessage(CypherStrings.GameobjectNotExist, objectId);
                    return false;
                }
                
                //if (objectInfo.displayId != 0 && !DBCStorage.sGameObjectDisplayInfoStore.LookupEntry(objectInfo->displayId))
                {
                    // report to DB errors log as in loading case
                    //sLog->outError(LOG_FILTER_SQL, "Gameobject (Entry %u GoType: %u) have invalid displayId (%u), not spawned.", objectId, objectInfo->type, objectInfo->displayId);
                    //handler->PSendSysMessage(LANG_GAMEOBJECT_HAVE_INVALID_DATA, objectId);
                    //return false;
                }

                Player player = command.GetSession().GetPlayer();
                float x = player.GetPositionX();
                float y = player.GetPositionY();
                float z = player.GetPositionZ();
                float o = player.GetOrientation();
                Map map = player.GetMap();

                GameObject obj = new GameObject();
                uint guidLow = ObjMgr.GenerateLowGuid(HighGuidType.GameObject);

                if (!obj.Create(guidLow, objectInfo.entry, map, 1/*player.GetPhaseMgr().GetPhaseMaskForSpawn()*/, x, y, z, o, 0.0f, 0.0f, 0.0f, 0.0f, 0, GameObjectState.Ready))
                    return false;

                //if (spawntimeSecs != 0)
                {
                    //obj.SetRespawnTime(spawntimeSecs);
                }

                // fill the gameobject data and save to the db
                //obj.SaveToDB(map->GetId(), (1 << map->GetSpawnMode()), player->GetPhaseMgr().GetPhaseMaskForSpawn());

                // this will generate a new guid if the object is in an instance
                if (!obj.LoadGameObjectFromDB(guidLow, map))
                    return false;

                // TODO: is it really necessary to add both the real and DB table guid here ?
                ObjMgr.AddGameObjectToGrid(guidLow, ObjMgr.GetGOData(guidLow));
                command.SendSysMessage(CypherStrings.GameobjectAdd, objectId, objectInfo.name, guidLow, x, y, z);
                return true;
            }
            [Command("add", "", AccountTypes.GameMaster)]
            public static bool Add(string[] args, CommandGroup command)
            {
                return false;
            }
        }

        [CommandGroup("set", "", AccountTypes.GameMaster)]
        public class SetCommands : CommandGroup
        {
            [Command("phase", "", AccountTypes.GameMaster)]
            public static bool Phase(string[] args, CommandGroup command)
            {
                return false;
            }
            [Command("state", "", AccountTypes.GameMaster)]
            public static bool State(string[] args, CommandGroup command)
            {
                return false;
            }
        }

    }
}
