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

namespace Framework.Database
{
    public struct WorldStatements
    {
        public const string Sel_QuestPools = "SELECT entry, pool_entry FROM pool_quest";
        public const string Del_CreatureLinkedRespawn = "DELETE FROM linked_respawn WHERE guid = ?";
        public const string Rep_CreatureLinkedRespawn = "REPLACE INTO linked_respawn (guid, linkedGuid) VALUES (?, ?)";
        public const string Sel_CreatureTEXT = "SELECT entry, groupid, id, text, type, language, probability, emote, duration, sound FROM creature_text";
        public const string Sel_SmartScripts = "SELECT entryorguid, source_type, id, link, event_type, event_phase_mask, event_chance, event_flags, event_param1, event_param2, event_param3, event_param4, action_type, action_param1, action_param2, action_param3, action_param4, action_param5, action_param6, target_type, target_param1, target_param2, target_param3, target_x, target_y, target_z, target_o FROM smart_scripts ORDER BY entryorguid, source_type, id, link";
        public const string Sel_SmartScriptsWP = "SELECT entry, pointid, position_x, position_y, position_z FROM waypoints ORDER BY entry, pointid";
        public const string Del_GameObject = "DELETE FROM gameobject WHERE guid = ?";
        public const string Del_EventGameObject = "DELETE FROM game_event_gameobject WHERE guid = ?";
        public const string Ins_GraveYardZone = "INSERT INTO game_graveyard_zone (id, ghost_zone, faction) VALUES (?, ?, ?)";
        public const string Del_GraveYardZone = "DELETE FROM game_graveyard_zone WHERE id = ? AND ghost_zone = ? AND faction = ?";
        public const string Ins_GameTele = "INSERT INTO game_tele (id, position_x, position_y, position_z, orientation, map, name) VALUES (?, ?, ?, ?, ?, ?, ?)";
        public const string Del_GameTele = "DELETE FROM game_tele WHERE name = ?";
        public const string Ins_NpcVendor = "INSERT INTO npc_vendor (entry, item, maxcount, incrtime, extendedcost, type) VALUES(?, ?, ?, ?, ?, ?)";
        public const string Del_NpcVendor = "DELETE FROM npc_vendor WHERE entry = ? AND item = ? AND type = ?";
        public const string Sel_NpcVendorRef = "SELECT item, maxcount, incrtime, ExtendedCost, type FROM npc_vendor WHERE entry = ? AND type = ? ORDER BY slot ASC";
        public const string Upd_CreatureMovementType = "UPDATE creature SET MovementType = ? WHERE guid = ?";
        public const string Upd_CreatureFaction = "UPDATE creature_template SET faction_A = ?, faction_H = ? WHERE entry = ?";
        public const string Upd_CreatureNPCFlag = "UPDATE creature_template SET npcflag = ? WHERE entry = ?";
        public const string Upd_CreaturePosition = "UPDATE creature SET position_x = ?, position_y = ?, position_z = ?, orientation = ? WHERE guid = ?";
        public const string Upd_CreatureSpawnDistance = "UPDATE creature SET spawndist = ?, MovementType = ? WHERE guid = ?";
        public const string Upd_CreatureSpawnTimeSecs = "UPDATE creature SET spawntimesecs = ? WHERE guid = ?";
        public const string Ins_CreatureFormation = "INSERT INTO creature_formations (leaderGUID, memberGUID, dist, angle, groupAI) VALUES (?, ?, ?, ?, ?)";
        public const string Ins_WaypointData = "INSERT INTO waypoint_data (id, point, position_x, position_y, position_z) VALUES (?, ?, ?, ?, ?)";
        public const string Del_WaypointData = "DELETE FROM waypoint_data WHERE id = ? AND point = ?";
        public const string Upd_WaypointDataPoint = "UPDATE waypoint_data SET point = point - 1 WHERE id = ? AND point > ?";
        public const string Upd_WaypointDataPosition = "UPDATE waypoint_data SET position_x = ?, position_y = ?, position_z = ? where id = ? AND point = ?";
        public const string Upd_WaypointDataWPGUID = "UPDATE waypoint_data SET wpguid = ? WHERE id = ? and point = ?";
        public const string Sel_WaypointDataMaxID = "SELECT MAX(id) FROM waypoint_data";
        public const string Sel_WaypointDataMaxPoint = "SELECT MAX(point) FROM waypoint_data WHERE id = ?";
        public const string Sel_WaypointDataByID = "SELECT point, position_x, position_y, position_z, orientation, move_flag, delay, action, action_chance FROM waypoint_data WHERE id = ? ORDER BY point";
        public const string Sel_WaypointDataPosByID = "SELECT point, position_x, position_y, position_z FROM waypoint_data WHERE id = ?";
        public const string Sel_WaypointDataPosFirstByID = "SELECT position_x, position_y, position_z FROM waypoint_data WHERE point = 1 AND id = ?";
        public const string Sel_WaypointDataPosLastByID = "SELECT position_x, position_y, position_z, orientation FROM waypoint_data WHERE id = ? ORDER BY point DESC LIMIT 1";
        public const string Sel_WaypointDataByWPGUID = "SELECT id, point FROM waypoint_data WHERE wpguid = ?";
        public const string Sel_WaypointDataAllByWPGUID = "SELECT id, point, delay, move_flag, action, action_chance FROM waypoint_data WHERE wpguid = ?";
        public const string Upd_WaypointDataAllWPGUID = "UPDATE waypoint_data SET wpguid = 0";
        public const string Sel_WaypointDataByPos = "SELECT id, point FROM waypoint_data WHERE (abs(position_x - ?) <= ?) and (abs(position_y - ?) <= ?) and (abs(position_z - ?) <= ?)";
        public const string Sel_WaypointDataWPGUIDByID = "SELECT wpguid FROM waypoint_data WHERE id = ? and wpguid <> 0";
        public const string Sel_WaypointDataAction = "SELECT DISTINCT action FROM waypoint_data";
        public const string Sel_WaypointScriptsMaxID = "SELECT MAX(guid) FROM waypoint_scripts";
        public const string Ins_CreatureAddon = "INSERT INTO creature_addon(guid, path_id) VALUES (?, ?)";
        public const string Upd_CreatureAddonPath = "UPDATE creature_addon SET path_id = ? WHERE guid = ?";
        public const string Del_CreatureAddon = "DELETE FROM creature_addon WHERE guid = ?";
        public const string Sel_CreatureAddonByGUID = "SELECT guid FROM creature_addon WHERE guid = ?";
        public const string Ins_WaypointScript = "INSERT INTO waypoint_scripts (guid) VALUES (?)";
        public const string Del_WaypointScript = "DELETE FROM waypoint_scripts WHERE guid = ?";
        public const string Upd_WaypointScriptID = "UPDATE waypoint_scripts SET id = ? WHERE guid = ?";
        public const string Upd_WaypointScriptX = "UPDATE waypoint_scripts SET x = ? WHERE guid = ?";
        public const string Upd_WaypointScriptY = "UPDATE waypoint_scripts SET y = ? WHERE guid = ?";
        public const string Upd_WaypointScriptZ = "UPDATE waypoint_scripts SET z = ? WHERE guid = ?";
        public const string Upd_WaypointScriptO = "UPDATE waypoint_scripts SET o = ? WHERE guid = ?";
        public const string Sel_WaypointScriptIDByGUID = "SELECT id FROM waypoint_scripts WHERE guid = ?";
        public const string Del_Creature = "DELETE FROM creature WHERE guid = ?";
        public const string Ins_CreatureTransport = "INSERT INTO creature_transport (guid, Npcentry, transport_entry,  TransOffsetX, TransOffsetY, TransOffsetZ, TransOffsetO) values (?, ?, ?, ?, ?, ?, ?)";
        public const string Upd_CreatureTransportEmote = "UPDATE creature_transport SET emote = ? WHERE transport_entry = ? AND guid = ?";
        public const string Sel_Commands = "SELECT name, security, help FROM command";
        public const string Sel_CreatureTemplate = "SELECT difficulty_entry_1, difficulty_entry_2, difficulty_entry_3, KillCredit1, KillCredit2, modelid1, modelid2, modelid3, modelid4, name, subname, IconName, gossip_menu_id, minlevel, maxlevel, exp, faction_A, faction_H, npcflag, speed_walk, speed_run, scale, rank, mindmg, maxdmg, dmgschool, attackpower, dmg_multiplier, baseattacktime, rangeattacktime, unit_class, unit_flags, unit_flags2, dynamicflags, family, trainer_type, trainer_spell, trainer_class, trainer_race, minrangedmg, maxrangedmg, rangedattackpower, type, type_flags, lootid, pickpocketloot, skinloot, resistance1, resistance2, resistance3, resistance4, resistance5, resistance6, spell1, spell2, spell3, spell4, spell5, spell6, spell7, spell8, PetSpellDataId, VehicleId, mingold, maxgold, AIName, MovementType, InhabitType, HoverHeight, Health_mod, Mana_mod, Mana_mod_extra, Armor_mod, RacialLeader, questItem1, questItem2, questItem3, questItem4, questItem5, questItem6, movementId, RegenHealth, equipment_id, mechanic_immune_mask, flags_extra, ScriptName FROM creature_template WHERE entry = ?";
        public const string Sel_WaypointScriptByID = "SELECT guid, delay, command, datalong, datalong2, dataint, x, y, z, o FROM waypoint_scripts WHERE id = ?";
        public const string Sel_IP2NationCountry = "SELECT c.country FROM ip2nationCountries c, ip2nation i WHERE i.ip < ? AND c.code = i.country ORDER BY i.ip DESC LIMIT 0,1";
        public const string Sel_ItemTemplateByName = "SELECT entry FROM item_template WHERE name = ?";
        public const string Sel_CreatureByID = "SELECT guid FROM creature WHERE id = ?";
        public const string Sel_GameobjectNearest = "SELECT guid, id, position_x, position_y, position_z, map, (POW(position_x - ?, 2) + POW(position_y - ?, 2) + POW(position_z - ?, 2)) AS order_ FROM gameobject WHERE map = ? AND (POW(position_x - ?, 2) + POW(position_y - ?, 2) + POW(position_z - ?, 2)) <= ? ORDER BY order_";
        public const string Ins_Creature = "INSERT INTO creature (guid, id , map, spawnMask, phaseMask, modelid, equipment_id, position_x, position_y, position_z, orientation, spawntimesecs, spawndist, currentwaypoint, curhealth, curmana, MovementType, npcflag, unit_flags, dynamicflags) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
        public const string Del_GameEventCreature = "DELETE FROM game_event_creature WHERE guid = ?";
        public const string Del_GameEventModeEquip = "DELETE FROM game_event_model_equip WHERE guid = ?";
        public const string Ins_Gameobject = "INSERT INTO gameobject (guid, id, map, spawnMask, phaseMask, position_x, position_y, position_z, orientation, rotation0, rotation1, rotation2, rotation3, spawntimesecs, animprogress, state) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
        public const string Ins_Disables = "INSERT INTO disables (entry, sourceType, flags, comment) VALUES (?, ?, ?, ?)";
        public const string Sel_Disables = "SELECT entry FROM disables WHERE entry = ? AND sourceType = ?";
        public const string Del_Disables = "DELETE FROM disables WHERE entry = ? AND sourceType = ?";
    }
}
