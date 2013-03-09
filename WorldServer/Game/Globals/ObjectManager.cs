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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Constants;
using Framework.Database;
using Framework.DataStorage;
using Framework.Logging;
using Framework.Singleton;
using Framework.Utility;
using WorldServer.Game.WorldEntities;
using System.Diagnostics.Contracts;
using Framework.Configuration;
using WorldServer.Game.Spells;
using WorldServer.Game.Maps;
using WorldServer.Game.Packets;
using System.Diagnostics;
using Framework.ObjectDefines;
using WorldServer.Game.Managers;

namespace WorldServer.Game
{
    public sealed class ObjectManager : SingletonBase<ObjectManager>
    {
        ObjectManager() 
        {
            m_objectMap = new Dictionary<ulong,WorldObject>();
        }

        public Player FindPlayer(ulong guid)
        {
            return GetObjectInWorld<Player>(guid);
        }
        public Player FindPlayerByName(string name)
        {
            return (Player)m_objectMap.Where(p => p.Value.GetName() == name);
        }

        public Unit GetUnit(WorldObject u, ulong guid)
        {
            return GetObjectInMap<Unit>(guid, u.GetMap());
        }

        public T GetObject<T>(WorldObject u, ulong guid)where T : WorldObject
        {
            return (T)GetObjectInMap<T>(guid, u.GetMap());
        }
        T GetObjectInMap<T>(ulong guid, Map map) where T : WorldObject
        {
            T obj = GetObjectInWorld<T>(guid);
            if (obj != null)
                if (obj.GetMap() == map)
                    return obj;
            return null;
        }

        public void AddObject<T>(T obj) where T : WorldObject
        {
            m_objectMap.Add(obj.GetGUID(), obj);
        }
        // returns object if is in world
        T GetObjectInWorld<T>(ulong guid) where T : WorldObject
        {
            return m_objectMap.LookupByKey(guid) as T;
        }

        Dictionary<ulong, WorldObject> m_objectMap;
        Dictionary<ulong, WorldObject> i_player2corpse;

        public Corpse GetCorpseForPlayerGUID(ulong guid)
        {
            //Lock

            return (Corpse)i_player2corpse.LookupByKey(guid);
        }

        public WorldObject GetObjectByTypeMask(WorldObject p, ulong guid, HighGuidMask typemask)
        {
            switch ((HighGuidType)ObjectGuid.GuidHiPart(guid))
            {
                case HighGuidType.Item:
                    if (Convert.ToBoolean(typemask & HighGuidMask.Item) && p.GetTypeId() == ObjectType.Player)
                        return ((Player)p).GetItemByGuid(guid);
                    break;
                case HighGuidType.Player:
                    if (Convert.ToBoolean(typemask & HighGuidMask.Player))
                        return GetObject<Player>(p, guid);
                    break;
                case HighGuidType.Transport:
                case HighGuidType.MOTransport:
                case HighGuidType.GameObject:
                    if (Convert.ToBoolean(typemask & HighGuidMask.GameObject))
                        return GetObject<GameObject>(p, guid);
                    break;
                case HighGuidType.Unit:
                case HighGuidType.Vehicle:
                    if (Convert.ToBoolean(typemask & HighGuidMask.Unit))
                        return GetObject<Creature>(p, guid);
                    break;
                case HighGuidType.Pet:
                    if (Convert.ToBoolean(typemask & HighGuidMask.Unit))
                        return GetObject<Pet>(p, guid);
                    break;
                case HighGuidType.DynamicObject:
                    if (Convert.ToBoolean(typemask & HighGuidMask.DynamicObject))
                        return GetObject<DynamicObject>(p, guid);
                    break;
                case HighGuidType.Corpse:
                    break;
            }

            return null;
        }



        //old shit


        public static MapDifficultyEntry GetMapDifficultyData(uint mapId, Difficulty difficulty)
        {
            return DBCStorage.MapDifficultyStorage.Values.FirstOrDefault(p => p.MapId == mapId && p.Difficulty == (uint)difficulty);
        }

        public string GetCypherString(CypherStrings cmd)
        {
            return CypherStringStorage.LookupByKey((uint)cmd);
        }

        public CreatureData GetCreatureData(uint guid)
        {
            return creatureDataStorage.LookupByKey(guid);
        }
        public GameObjectData GetGOData(uint guid)
        {
            return gameObjectDataStorage.LookupByKey(guid);
        }
        public void NewGOData(uint guid, GameObjectData data) { gameObjectDataStorage.Add(guid, data); }

        public Player GetPlayerInMap(ulong guidLow, WorldObject obj)
        {
            return null;// obj.GetMap().GetPlayer(guidLow);
        }

        public GameObjectTemplate GetGameObjectTemplate(uint entry)
        {
            return gameObjectTemplateStorage.LookupByKey(entry);
        }

        public uint ChooseDisplayId(uint team, CreatureTemplate cinfo, CreatureData data = null)
        {
            // Load creature model (display id)
            uint display_id = 0;

            if (data == null || data.displayid == 0)
                display_id = cinfo.GetRandomValidModelId();
            else
                return data.displayid;

            return display_id;
        }
        public uint GetPlayerAccountIdByGUID(ulong guid)
        {
            // prevent DB access for online player
            Player player = FindPlayer(guid);
            if (player != null)
                return player.GetSession().GetAccountId();

            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_ACCOUNT_BY_GUID);
            stmt.AddValue(0, ObjectGuid.GuidLowPart(guid));

            SQLResult result = DB.Characters.Select(stmt);

            if (result.Count != 0)
                return result.Read<uint>(0, 0);

            return 0;
        }

        public PlayerInfo GetPlayerInfo(uint race, uint class_)
        {
            if (race >= (int)Race.Max)
                return null;
            if (class_ >= (int)Class.Max)
                return null;
            var info = PlayerInfoStorage[race][class_];
            if (info.DisplayId_m == 0 || info.DisplayId_f == 0)
                return null;
            return info;
        }

        public ItemTemplate GetItemTemplate(uint ItemId) { return ItemTemplateStorage.LookupByKey(ItemId); }
        public CreatureTemplate GetCreatureTemplate(uint entry) { return creatureTemplateStorage.LookupByKey(entry); }
        public RepSpilloverTemplate GetRepSpillover(uint factionId)
        {
            return RepSpilloverStorage.LookupByKey(factionId);
        }

        public void SetHighestGuids()
        {
            SQLResult result = DB.Characters.Select("SELECT MAX(guid) FROM characters");
            if (result.Count == 1)
                _hiCharGuid = result.Read<uint>(0, 0) + 1;

            result = DB.World.Select("SELECT MAX(guid) FROM creature");
            if (result.Count == 1)
                _hiCreatureGuid = result.Read<uint>(0, 0) + 1;

            result = DB.Characters.Select("SELECT MAX(guid) FROM item_instance");
            if (result.Count == 1)
                _hiItemGuid = result.Read<uint>(0, 0) + 1;

            // Cleanup other tables from not existed guids ( >= _hiItemGuid)
            DB.Characters.Execute("DELETE FROM character_inventory WHERE item >= {0}", _hiItemGuid);      // One-time query
            DB.Characters.Execute("DELETE FROM mail_items WHERE item_guid >= {0}", _hiItemGuid);          // One-time query
            DB.Characters.Execute("DELETE FROM auctionhouse WHERE itemguid >= {0}", _hiItemGuid);         // One-time query
            DB.Characters.Execute("DELETE FROM guild_bank_item WHERE item_guid >= {0}", _hiItemGuid);     // One-time query

            result = DB.World.Select("SELECT MAX(guid) FROM gameobject");
            if (result.Count == 1)
                _hiGoGuid = result.Read<uint>(0, 0) + 1;

            result = DB.World.Select("SELECT MAX(guid) FROM transports");
            if (result.Count == 1)
                _hiMoTransGuid = result.Read<uint>(0, 0) + 1;

            result = DB.Characters.Select("SELECT MAX(id) FROM auctionhouse");
            if (result.Count == 1)
                _auctionId = result.Read<uint>(0, 0) + 1;

            result = DB.Characters.Select("SELECT MAX(id) FROM mail");
            if (result.Count == 1)
                _mailId = result.Read<uint>(0, 0) + 1;

            result = DB.Characters.Select("SELECT MAX(corpseGuid) FROM corpse");
            if (result.Count == 1)
                _hiCorpseGuid = result.Read<uint>(0, 0) + 1;

            //result = DB.Characters.Select("SELECT MAX(arenateamid) FROM arena_team");
            //if (result.Count == 1)
            //sArenaTeamMgr->SetNextArenaTeamId(result.Read<uint>(0, 0) + 1);

            result = DB.Characters.Select("SELECT MAX(setguid) FROM character_equipmentsets");
            if (result.Count == 1)
                _equipmentSetGuid = result.Read<ulong>(0, 0) + 1;

            //result = DB.Characters.Select("SELECT MAX(guildId) FROM guild");
            //if (result.Count == 1)
            //sGuildMgr->SetNextGuildId(result.Read<uint>(0, 0) + 1);

            //result = DB.Characters.Select("SELECT MAX(guid) FROM groups");
            //if (result.Count == 1)
            //sGroupMgr->SetGroupDbStoreSize(result.Read<uint>(0, 0) + 1);

            result = DB.Characters.Select("SELECT MAX(itemId) from character_void_storage");
            if (result.Count == 1)
                _voidItemId = result.Read<ulong>(0, 0) + 1;
        }
        public uint GenerateLowGuid(HighGuidType guidhigh)
        {
            switch (guidhigh)
            {
                case HighGuidType.Item:
                    Contract.Ensures(_hiItemGuid < 0xFFFFFFFE, "Item guid overflow!");
                    return _hiItemGuid++;
                case HighGuidType.Unit:
                    Contract.Ensures(_hiCreatureGuid < 0x00FFFFFE, "Creature guid overflow!");
                    return _hiCreatureGuid++;
                case HighGuidType.Pet:
                    Contract.Ensures(_hiPetGuid < 0x00FFFFFE, "Pet guid overflow!");
                    return _hiPetGuid++;
                case HighGuidType.Vehicle:
                    Contract.Ensures(_hiVehicleGuid < 0x00FFFFFF, "Vehicle guid overflow!");
                    return _hiVehicleGuid++;
                case HighGuidType.Player:
                    Contract.Ensures(_hiCharGuid < 0xFFFFFFFE, "Player guid overflow!");
                    return _hiCharGuid++;
                case HighGuidType.GameObject:
                    Contract.Ensures(_hiGoGuid < 0x00FFFFFE, "Gameobject guid overflow!");
                    return _hiGoGuid++;
                case HighGuidType.Corpse:
                    Contract.Ensures(_hiCorpseGuid < 0xFFFFFFFE, "Corpse guid overflow!");
                    return _hiCorpseGuid++;
                case HighGuidType.DynamicObject:
                    Contract.Ensures(_hiDoGuid < 0xFFFFFFFE, "DynamicObject guid overflow!");
                    return _hiDoGuid++;
                case HighGuidType.MOTransport:
                    Contract.Ensures(_hiMoTransGuid < 0xFFFFFFFE, "MO Transport guid overflow!");
                    return _hiMoTransGuid++;
                default:
                    Contract.Ensures(false, "GenerateLowGuid - Unknown HIGHGUID type");
                    return 0;
            }
        }

        public void GetPlayerClassLevelInfo(uint class_, uint level, out uint baseHP, out uint baseMana)
        {
            baseHP = 0;
            baseMana = 0;
            if (level < 1 || class_ >= (int)Class.Max)
                return;

            if (level > WorldConfig.MaxLevel)
                level = (byte)WorldConfig.MaxLevel;

            GtOCTBaseHPByClassEntry hp = DBCStorage.sGtOCTBaseHPByClassStorage.LookupByKey((class_ - 1) * SharedConst.GTMaxLevel + level - 1);
            GtOCTBaseMPByClassEntry mp = DBCStorage.sGtOCTBaseMPByClassStorage.LookupByKey((class_ - 1) * SharedConst.GTMaxLevel + level - 1);

            if (hp == null || mp == null)
            {
                Log.outError("Tried to get non-existant Class-Level combination data for base hp/mp. Class {0} Level {1}", class_, level);
                return;
            }

            baseHP = (uint)hp.ratio;
            baseMana = (uint)mp.ratio;
        }
        public PlayerLevelInfo GetPlayerLevelInfo(uint race, uint class_, uint level)
        {
            if (level < 1 || race >= (int)Race.Max || class_ >= (int)Class.Max)
                return null;

            PlayerInfo pInfo = PlayerInfoStorage[race][class_];
            if (pInfo.DisplayId_m == 0 || pInfo.DisplayId_f == 0)
                return null;

            if (level <= WorldConfig.MaxLevel)
                return pInfo.levelInfo[level - 1];
            //else
            //return BuildPlayerLevelInfo(race, class_, level, info);
            return null;
        }

        public uint GetXPForLevel(uint level)
        {
            if (level < XpPerLevel.Length)
                return XpPerLevel[level];
            return 0;
        }

        public int GetPowerIndexByClass(uint powerType, uint classId)
        {
            return DBCStorage.PowersByClass[classId][powerType];
        }

        public Dictionary<byte, byte> GetActivationClasses() { return ActivationClassStorage; }
        public Dictionary<byte, byte> GetActivationRaces() { return ActivationRacesStorage; }


        public bool IsPlayerAccount(AccountTypes gmlevel)
        {
            return gmlevel == AccountTypes.Player;
        }
        public bool IsModeratorAccount(AccountTypes gmlevel)
        {
            return gmlevel >= AccountTypes.Moderator && gmlevel <= AccountTypes.Console;
        }
        public bool IsGMAccount(AccountTypes gmlevel)
        {
            return gmlevel >= AccountTypes.GameMaster && gmlevel <= AccountTypes.Console;
        }
        public bool IsAdminAccount(AccountTypes gmlevel)
        {
            return gmlevel >= AccountTypes.Administrator && gmlevel <= AccountTypes.Console;
        }
        public CreatureBaseStats GetCreatureBaseStats(uint level, uint unitClass)
        {
            var it = creatureBaseStatsStorage.LookupByKey(WorldObject.MakePair16(level, unitClass));
            if (it != null)
                return it;
            
            return new DefaultCreatureBaseStats();
        }

        bool IsVendorItemValid(uint vendor_entry, uint id, int maxcount, uint incrtime, uint ExtendedCost, byte type, Player player = null, List<uint> skip_vendors = null, uint ORnpcflag = 0)
        {
            CreatureTemplate cInfo = GetCreatureTemplate(vendor_entry);
            if (cInfo == null)
            {
                if (player != null)
                    player.SendSysMessage(CypherStrings.CommandVendorselection);
                else
                    Log.outError("Table `(game_event_)npc_vendor` have data for not existed creature template (Entry: {0}), ignore", vendor_entry);
                return false;
            }

            if (!Convert.ToBoolean(((uint)cInfo.Npcflag | ORnpcflag) & (uint)NPCFlags.Vendor))
            {
                if (skip_vendors == null || skip_vendors.Count() == 0)
                {
                    if (player != null)
                        player.SendSysMessage(CypherStrings.CommandVendorselection);
                    else
                        Log.outError("Table `(game_event_)npc_vendor` have data for not creature template (Entry: {0}) without vendor flag, ignore", vendor_entry);

                    if (skip_vendors != null)
                        skip_vendors.Add(vendor_entry);
                }
                return false;
            }

            if ((type == (uint)ItemVendorType.Item && GetItemTemplate(id) == null) ||
                (type == (uint)ItemVendorType.Currency && DBCStorage.CurrencyTypesStorage.LookupByKey(id) == null))
            {
                if (player != null)
                    player.SendSysMessage(CypherStrings.ItemNotFound, id, type);
                else
                    Log.outError("Table `(game_event_)npc_vendor` for Vendor (Entry: {0}) have in item list non-existed item ({1}, type {2}), ignore", vendor_entry, id, type);
                return false;
            }

            //if (ExtendedCost && !sItemExtendedCostStore.LookupEntry(ExtendedCost))
            {
                //if (player != null)
                //  player.SendSysMessage(CypherStrings.ExtendedCostNotExist, ExtendedCost);
                //else
                   // Log.outError("Table `(game_event_)npc_vendor` have Item (Entry: {0}) with wrong ExtendedCost ({1}) for vendor ({2}), ignore", id, ExtendedCost, vendor_entry);
                //return false;
            }

            if (type == (uint)ItemVendorType.Item) // not applicable to currencies
            {
                if (maxcount > 0 && incrtime == 0)
                {
                    if (player != null)
                        player.SendSysMessage("MaxCount != 0 ({0}) but IncrTime == 0", maxcount);
                    else
                        Log.outError("Table `(game_event_)npc_vendor` has `maxcount` ({0}) for item {1} of vendor (Entry: {2}) but `incrtime`=0, ignore", maxcount, id, vendor_entry);
                    return false;
                }
                else if (maxcount == 0 && incrtime > 0)
                {
                    if (player != null)
                        player.SendSysMessage("MaxCount == 0 but IncrTime<>= 0");
                    else
                        Log.outError("Table `(game_event_)npc_vendor` has `maxcount`=0 for item %u of vendor (Entry: {0}) but `incrtime`<>0, ignore", id, vendor_entry);
                    return false;
                }
            }

            VendorItemData vItems = GetNpcVendorItemList(vendor_entry);
            if (vItems == null)
                return true;                                        // later checks for non-empty lists

            if (vItems.FindItemCostPair(id, ExtendedCost, type) != null)
            {
                if (player != null)
                    player.SendSysMessage(CypherStrings.ItemAlreadyInList, id, ExtendedCost, type);
                else
                    Log.outError("Table `npc_vendor` has duplicate items {0} (with extended cost {1}, type {2}) for vendor (Entry: {3}), ignoring", id, ExtendedCost, type, vendor_entry);
                return false;
            }

            if (vItems.GetItemCount() >= CreatureConst.MaxVendorItems)
            {
                if (player != null)
                    player.SendSysMessage(CypherStrings.CommandAddvendoritemitems);
                else
                    Log.outError("Table `npc_vendor` has too many items ({0} >= {1}) for vendor (Entry: {2}), ignore", vItems.GetItemCount(), CreatureConst.MaxVendorItems, vendor_entry);
                return false;
            }

            return true;
        }
        public VendorItemData GetNpcVendorItemList(uint entry)
        {
            return cacheVendorItemStorage.LookupByKey(entry);
        }

        public EquipmentInfo GetEquipmentInfo(uint entry, int id)
        {
            var equip = equipmentInfoStorage.LookupByKey(entry);

            if (equip == null)
                return null;

            if (id == -1)
                return equip.LookupByKey(RandomHelper.irand(1, equip.Count)).Item2;
            else
                return equip.LookupByKey(id).Item2;
        }
        public CellObjectGuids GetOrCreateCellObjectGuids(uint mapid, byte spawnMask, CellCoord cellCoord, bool createNotExist = false)
        {
            return GetOrCreateCellObjectGuids(mapid, spawnMask, cellCoord.GetId(), createNotExist);
        }
        public CellObjectGuids GetOrCreateCellObjectGuids(uint mapid, byte spawnMask, uint cellid, bool createNotExist = false)
        {
            uint newid = WorldObject.MakePair32(mapid, spawnMask);

            if (createNotExist)
            {
                if (_mapObjectGuidsStore.LookupByKey(newid) == null)
                    _mapObjectGuidsStore[newid] = new Dictionary<uint, CellObjectGuids>();

                if (_mapObjectGuidsStore[newid].LookupByKey(cellid) == null)
                    _mapObjectGuidsStore[newid].Add(cellid, new CellObjectGuids());
            }

            return _mapObjectGuidsStore.LookupByKey(newid).LookupByKey(cellid);
        }

        #region Loads
        public void LoadCypherStrings()
        {
            var time = Time.getMSTime();
            CypherStringStorage.Clear();

            SQLResult result = DB.World.Select("SELECT * FROM cypher_string");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 CypherStrings. DB table `cypher_string` is empty.");
                WorldServer.ShutDownServer();
            }
            uint count = 0;
            for (int i = 0; i < result.Count; i++)
            {
                CypherStringStorage.Add(result.Read<uint>(i, 0), result.Read<string>(i, 1));
                count++;
            }
            Log.outInfo("Loaded {0} CypherStrings in {1} ms", count, Time.getMSTimeDiffNow(time));
            Log.outInit();
        }

        public void LoadActivationClassRaces()
        {
            var time = Time.getMSTime();
            ActivationClassStorage.Clear();
            ActivationRacesStorage.Clear();

            SQLResult result = DB.Auth.Select("SELECT * FROM realm_classes WHERE realmId = {0}", WorldConfig.RealmId);

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 ActivationClasses. DB table `realm_classes` is empty.");
                WorldServer.ShutDownServer();
                return;
            }

            uint count = 0;
            for (int i = 0; i < result.Count; i++)
            {
                ActivationClassStorage.Add(result.Read<byte>(i, 1), result.Read<byte>(i, 2));
                count++;
            }
            Log.outInfo("Loaded {0} ActivationClasses in {1} ms", count, Time.getMSTimeDiffNow(time));
            Log.outInit();

            time = Time.getMSTime();
            result = DB.Auth.Select("SELECT * FROM realm_races WHERE realmId = {0}", WorldConfig.RealmId);

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 ActivationRaces. DB table `realm_races` is empty.");
                WorldServer.ShutDownServer();
                return;
            }

            count = 0;
            for (int i = 0; i < result.Count; i++)
            {
                ActivationRacesStorage.Add(result.Read<byte>(i, 1), result.Read<byte>(i, 2));
                count++;
            }

            Log.outInfo("Loaded {0} ActivationRaces in {1} ms", count, Time.getMSTimeDiffNow(time));
            Log.outInit();
        }

        //Player Create Info
        public void LoadPlayerInfo()
        {
            var time = Time.getMSTime();
            // Load playercreate
            {
                //                                         0     1      2    3     4           5           6           7
                SQLResult result = DB.World.Select("SELECT race, class, map, zone, position_x, position_y, position_z, orientation FROM playercreateinfo");

                if (result.Count == 0)
                {
                    Log.outError("Loaded 0 player create definitions. DB table `playercreateinfo` is empty.");
                    return;
                }

                uint count = 0;
                for (var i = 0; i < result.Count; i++)
                {
                    uint current_race = result.Read<uint>(i, 0);
                    uint current_class = result.Read<uint>(i, 1);
                    uint mapId = result.Read<uint>(i, 2);
                    uint zoneId = result.Read<uint>(i, 3);
                    float positionX = result.Read<float>(i, 4);
                    float positionY = result.Read<float>(i, 5);
                    float positionZ = result.Read<float>(i, 6);
                    float orientation = result.Read<float>(i, 7);

                    if (current_race >= (int)Race.Max)
                    {
                        Log.outError("Wrong race {0} in `playercreateinfo` table, ignoring.", current_race);
                        continue;
                    }

                    var rEntry = DBCStorage.CharRaceStorage.LookupByKey(current_race);
                    if (rEntry == null)
                    {
                        Log.outError("Wrong race {0} in `playercreateinfo` table, ignoring.", current_race);
                        continue;
                    }

                    if (current_class >= (int)Class.Max)
                    {
                        Log.outError("Wrong class {0} in `playercreateinfo` table, ignoring.", current_class);
                        continue;
                    }

                    if (DBCStorage.CharClassStorage.LookupByKey(current_class).ClassID == 0)
                    {
                        Log.outError("Wrong class {0} in `playercreateinfo` table, ignoring.", current_class);
                        continue;
                    }
                    if (PlayerInfoStorage[current_race] == null)
                        PlayerInfoStorage[current_race] = new PlayerInfo[(int)Class.Max];

                    PlayerInfo pInfo = new PlayerInfo();

                    pInfo.MapId = mapId;
                    pInfo.ZoneId = zoneId;
                    pInfo.PositionX = positionX;
                    pInfo.PositionY = positionY;
                    pInfo.PositionZ = positionZ;
                    pInfo.Orientation = orientation;

                    pInfo.DisplayId_m = rEntry.Model_m;
                    pInfo.DisplayId_f = rEntry.Model_f;

                    PlayerInfoStorage[current_race][current_class] = pInfo;

                    ++count;
                }
                Log.outInfo("Loaded {0} player create definitions in {1} ms", count, Time.getMSTimeDiffNow(time));
                Log.outInit();
            }
            time = Time.getMSTime();
            // Load playercreate items
            Log.outInfo( "Loading Player Create Items Data...");
            {
                //                                         0     1      2       3
                SQLResult result = DB.World.Select("SELECT race, class, itemid, amount FROM playercreateinfo_item");

                if (result.Count == 0)
                    Log.outError("Loaded 0 custom player create items. DB table `playercreateinfo_item` is empty.");
                else
                {
                    uint count = 0;
                    for (var i = 0; i < result.Count; i++)
                    {
                        uint current_race = result.Read<uint>(i, 0);
                        if (current_race >= (int)Race.Max)
                        {
                            Log.outError("Wrong race {0} in `playercreateinfo_item` table, ignoring.", current_race);
                            continue;
                        }

                        uint current_class = result.Read<uint>(i, 1);
                        if (current_class >= (int)Class.Max)
                        {
                            Log.outError("Wrong class {0} in `playercreateinfo_item` table, ignoring.", current_class);
                            continue;
                        }

                        uint item_id = result.Read<uint>(i, 2);
                        if (GetItemTemplate(item_id).ItemId == 0)
                        {
                            Log.outError("Item id {0} (race {1} class {2}) in `playercreateinfo_item` table but not listed in `item_template`, ignoring.", item_id, current_race, current_class);
                            continue;
                        }

                        int amount = result.Read<int>(i, 3);

                        if (amount == 0)
                        {
                            Log.outError("Item id {0} (class {1} race {2}) have amount == 0 in `playercreateinfo_item` table, ignoring.", item_id, current_race, current_class);
                            continue;
                        }

                        if (current_race == 0 || current_class == 0)
                        {
                            uint min_race = current_race != 0 ? current_race : 1;
                            uint max_race = current_race != 0 ? current_race + 1 : (int)Race.Max;
                            uint min_class = current_class != 0 ? current_class : 1;
                            uint max_class = current_class != 0 ? current_class + 1 : (int)Class.Max;
                            for (var r = min_race; r < max_race; ++r)
                                for (var c = min_class; c < max_class; ++c)
                                    PlayerCreateInfoAddItemHelper(r, c, item_id, amount);

                        }
                        else
                            PlayerCreateInfoAddItemHelper(current_race, current_class, item_id, amount);

                        ++count;
                    }
                    Log.outInfo("Loaded {0} custom player create items in {1} ms", count, Time.getMSTimeDiffNow(time));
                    Log.outInit();
                }
            }
            time = Time.getMSTime();
            // Load playercreate spells
            Log.outInfo("Loading Player Create Spell Data...");
            {
                SQLResult result = DB.World.Select("SELECT race, class, Spell FROM `playercreateinfo_spell`");

                if (result.Count == 0)
                    Log.outError("Loaded 0 player create spells. DB table `playercreateinfo_spell` is empty.");
                else
                {
                    uint count = 0;
                    for (var i = 0; i < result.Count; i++)
                    {
                        uint current_race = result.Read<uint>(i, 0);
                        if (current_race >= (int)Race.Max)
                        {
                            Log.outError("Wrong race {0} in `playercreateinfo_spell` table, ignoring.", current_race);
                            continue;
                        }

                        uint current_class = result.Read<uint>(i, 1);
                        if (current_class >= (int)Class.Max)
                        {
                            Log.outError("Wrong class {0} in `playercreateinfo_spell` table, ignoring.", current_class);
                            continue;
                        }

                        if (current_race == 0 || current_class == 0)
                        {
                            uint min_race = current_race != 0 ? current_race : 1;
                            uint max_race = current_race != 0 ? current_race + 1 : (int)Race.Max;
                            uint min_class = current_class != 0 ? current_class : 1;
                            uint max_class = current_class != 0 ? current_class + 1 : (int)Class.Max;
                            for (var r = min_race; r < max_race; ++r)
                                for (var c = min_class; c < max_class; ++c)
                                    PlayerInfoStorage[r][c].spell.Add(result.Read<uint>(i, 2));
                        }
                        else
                            PlayerInfoStorage[current_race][current_class].spell.Add(result.Read<uint>(i, 2));

                        ++count;
                    }

                    Log.outInfo("Loaded {0} player create spells in {1} ms", count, Time.getMSTimeDiffNow(time));
                    Log.outInit();
                }
            }
            time = Time.getMSTime();
            // Load playercreate actions
            Log.outInfo("Loading Player Create Action Data...");
            {
                //                                         0     1      2       3       4
                SQLResult result = DB.World.Select("SELECT race, class, button, action, type FROM playercreateinfo_action");

                if (result.Count == 0)
                    Log.outError("Loaded 0 player create actions. DB table `playercreateinfo_action` is empty.");
                else
                {
                    uint count = 0;
                    for (var i = 0; i < result.Count; i++)
                    {
                        uint current_race = result.Read<uint>(i, 0);
                        if (current_race >= (int)Race.Max)
                        {
                            Log.outError("Wrong race {0} in `playercreateinfo_action` table, ignoring.", current_race);
                            continue;
                        }

                        uint current_class = result.Read<uint>(i, 1);
                        if (current_class >= (int)Class.Max)
                        {
                            Log.outError("Wrong class {0} in `playercreateinfo_action` table, ignoring.", current_class);
                            continue;
                        }

                        PlayerInfoStorage[current_race][current_class].action.Add(new PlayerCreateInfoAction(result.Read<byte>(i, 2), result.Read<uint>(i, 3), result.Read<byte>(i, 4)));
                        ++count;
                    }
                    Log.outInfo(">> Loaded {0} player create actions in {1} ms", count, Time.getMSTimeDiffNow(time));
                    Log.outInit();
                }
            }
            time = Time.getMSTime();
            // Loading levels data (class/race dependent)
            Log.outInfo("Loading Player Create Level Stats Data...");
            {
                //                                         0     1      2      3    4    5    6    7
                SQLResult result = DB.World.Select("SELECT race, class, level, str, agi, sta, inte, spi FROM player_levelstats");

                if (result.Count == 0)
                {
                    Log.outError("Loaded 0 level stats definitions. DB table `player_levelstats` is empty.");
                    //exit(1);
                    return;
                }

                uint count = 0;
                for (var i = 0; i < result.Count; i++)
                {

                    uint current_race = result.Read<uint>(i, 0);
                    if (current_race >= (int)Race.Max)
                    {
                        Log.outError("Wrong race {0} in `player_levelstats` table, ignoring.", current_race);
                        continue;
                    }

                    uint current_class = result.Read<uint>(i, 1);
                    if (current_class >= (int)Class.Max)
                    {
                        Log.outError("Wrong class {0} in `player_levelstats` table, ignoring.", current_class);
                        continue;
                    }

                    uint current_level = result.Read<uint>(i, 2);
                    if (current_level > WorldConfig.MaxLevel)
                    {
                        if (current_level > 255)        // hardcoded level maximum
                            Log.outError("Wrong (> {0}) level {1} in `player_levelstats` table, ignoring.", 255, current_level);
                        else
                        {
                            Log.outError("Unused (> MaxPlayerLevel in worldserver.conf) level {0} in `player_levelstats` table, ignoring.", current_level);
                            ++count;                                // make result loading percent "expected" correct in case disabled detail mode for example.
                        }
                        continue;
                    }

                    var pInfo = PlayerInfoStorage[current_race][current_class];
                    if (pInfo == null)
                        continue;

                    var levelinfo = new PlayerLevelInfo();

                    for (var x = 0; x < (int)Stats.Max; x++)
                    {
                        levelinfo.stats[x] = result.Read<byte>(i, x + 3);
                    }
                    pInfo.levelInfo[current_level - 1] = levelinfo;
                    ++count;
                }

                // Fill gaps and check integrity
                for (uint race = 0; race < (int)Race.Max; ++race)
                {
                    // skip non existed races
                    if (DBCStorage.CharRaceStorage.LookupByKey(race) == null || PlayerInfoStorage[race] == null)
                        continue;

                    for (uint class_ = 0; class_ < (int)Class.Max; ++class_)
                    {
                        // skip non existed classes
                        if (DBCStorage.CharClassStorage.LookupByKey(class_) == null || PlayerInfoStorage[race][class_] == null)
                            continue;

                        PlayerInfo pInfo = PlayerInfoStorage[race][class_];
                        if (pInfo == null)
                            continue;

                        // skip non loaded combinations
                        if (pInfo.DisplayId_m == 0 || pInfo.DisplayId_f == 0)
                            continue;

                        // skip expansion races if not playing with expansion
                        //if (sWorld->getIntConfig(CONFIG_EXPANSION) < 1 && (race == RACE_BLOODELF || race == RACE_DRAENEI))
                        //continue;

                        // skip expansion classes if not playing with expansion
                        // if (sWorld->getIntConfig(CONFIG_EXPANSION) < 2 && class_ == CLASS_DEATH_KNIGHT)
                        //continue;

                        // fatal error if no level 1 data
                        if (pInfo.levelInfo == null || pInfo.levelInfo[0] == null)
                        {
                            Log.outError("Race {0} Class {1} Level 1 does not have stats data!", race, class_);
                            //Globals.WorldMgr.ShutDownServer();
                            //return;
                        }

                        // fill level gaps
                        for (var level = 1; level < WorldConfig.MaxLevel; ++level)
                        {
                            if (pInfo.levelInfo[level] == null)//.stats[0] == 0)
                            {
                                Log.outError("Race {0} Class {1} Level {2} does not have stats data. Using stats data of level {3}.", race, class_, level + 1, level);
                                pInfo.levelInfo[level] = pInfo.levelInfo[level - 1];
                            }
                        }
                    }
                }

                Log.outInfo("Loaded {0} level stats definitions in {1} ms", count, Time.getMSTimeDiffNow(time));
                Log.outInit();
            }
            time = Time.getMSTime();
            // Loading xp per level data
            Log.outInfo("Loading Player Create XP Data...");
            {
                XpPerLevel = new uint[WorldConfig.MaxLevel];
                for (var level = 0; level < WorldConfig.MaxLevel; ++level)
                    XpPerLevel[level] = 0;

                //                                          0    1
                SQLResult result = DB.World.Select("SELECT lvl, xp_for_next_level FROM player_xp_for_level");

                if (result.Count == 0)
                {
                    Log.outError("Loaded 0 xp for level definitions. DB table `player_xp_for_level` is empty.");
                    WorldServer.ShutDownServer();
                    return;
                }

                uint count = 0;
                for (var i = 0; i < result.Count; i++)
                {
                    uint current_level = result.Read<uint>(i, 0);
                    uint current_xp = result.Read<uint>(i, 1);

                    if (current_level >= 85)//sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL))
                    {
                        if (current_level > 255)//STRONG_MAX_LEVEL)        // hardcoded level maximum
                            Log.outError("Wrong (> {0}) level {1} in `player_xp_for_level` table, ignoring.", 255, current_level);
                        else
                        {
                            Log.outError("Unused (> MaxPlayerLevel in worldserver.conf) level {0} in `player_xp_for_levels` table, ignoring.", current_level);
                            ++count;                                // make result loading percent "expected" correct in case disabled detail mode for example.
                        }
                        continue;
                    }
                    //PlayerXPperLevel
                    XpPerLevel[current_level] = current_xp;
                    ++count;
                }

                // fill level gaps
                for (var level = 1; level < 85; level++)//sWorld->getIntConfig(CONFIG_MAX_PLAYER_LEVEL); ++level)
                {
                    if (XpPerLevel[level] == 0)
                    {
                        Log.outError("Level {0} does not have XP for level data. Using data of level [{1}] + 100.", level + 1, level);
                        XpPerLevel[level] = XpPerLevel[level - 1] + 100;
                    }
                }
                Log.outInfo("Loaded {0} xp for level definitions in {1} ms", count, Time.getMSTimeDiffNow(time));
                Log.outInit();
            }
        }
        void PlayerCreateInfoAddItemHelper(uint race_, uint class_, uint itemId, int count)
        {
            if (count > 0)
                PlayerInfoStorage[race_][class_].item.Add(new PlayerCreateInfoItem(itemId, (uint)count));
            else
            {
                if (count < -1)
                    Log.outError("Invalid count {0} specified on item {1} be removed from original player create info (use -1)!", count, itemId);

                uint RaceClass = race_ | (class_ << 8);
                bool doneOne = false;
                foreach (var entry in DBCStorage.CharStartOutfitStorage.Values)
                {
                    if (entry.Mask == RaceClass || entry.Mask == (RaceClass | (1 << 16)))
                    {
                        bool found = false;
                        for (var x = 0; x < 24; ++x)
                        {
                            if (entry.ItemId[x] > 0 && entry.ItemId[x] == itemId)
                            {
                                found = true;
                                entry.ItemId[x] = 0;
                                break;
                            }
                        }

                        if (!found)
                            Log.outError("Item {0} specified to be removed from original create info not found in dbc!", itemId);

                        if (!doneOne)
                            doneOne = true;
                        else
                            break;
                    }

                }
            }
        }

        public void LoadReputationSpillover()
        {
            var time = Time.getMSTime();

            RepSpilloverStorage.Clear();                      // for reload case

            //                                        0        1         2       3       4         5       6       7         8       9       10        11      12      13        14      15
            SQLResult result = DB.World.Select("SELECT faction, faction1, rate_1, rank_1, faction2, rate_2, rank_2, faction3, rate_3, rank_3, faction4, rate_4, rank_4, faction5, rate_5, rank_5 FROM " +
            "reputation_spillover_template");
            if (result.Count == 0)
            {
                Log.outError("Loaded `reputation_spillover_template`, table is empty.");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint factionId = result.Read<uint>(i, 0);

                RepSpilloverTemplate repTemplate = new RepSpilloverTemplate();

                repTemplate.faction[0] = result.Read<uint>(i, 1);
                repTemplate.faction_rate[0] = result.Read<float>(i, 2);
                repTemplate.faction_rank[0] = result.Read<uint>(i, 3);
                repTemplate.faction[1] = result.Read<uint>(i, 4);
                repTemplate.faction_rate[1] = result.Read<float>(i, 5);
                repTemplate.faction_rank[1] = result.Read<uint>(i, 6);
                repTemplate.faction[2] = result.Read<uint>(i, 7);
                repTemplate.faction_rate[2] = result.Read<float>(i, 8);
                repTemplate.faction_rank[2] = result.Read<uint>(i, 9);
                repTemplate.faction[3] = result.Read<uint>(i, 10);
                repTemplate.faction_rate[3] = result.Read<float>(i, 11);
                repTemplate.faction_rank[3] = result.Read<uint>(i, 12);
                repTemplate.faction[4] = result.Read<uint>(i, 13);
                repTemplate.faction_rate[4] = result.Read<float>(i, 14);
                repTemplate.faction_rank[4] = result.Read<uint>(i, 15);

                var factionEntry = DBCStorage.FactionStorage.LookupByKey(factionId);

                if (factionEntry.ID == 0)
                {
                    Log.outError("Faction (faction.dbc) {0} does not exist but is used in `reputation_spillover_template`", factionId);
                    continue;
                }

                if (factionEntry.team == 0)
                {
                    Log.outError("Faction (faction.dbc) {0} in `reputation_spillover_template` does not belong to any team, skipping", factionId);
                    continue;
                }

                for (var x = 0; x < 5; ++x)
                {
                    if (repTemplate.faction[x] != 0)
                    {
                        var factionSpillover = DBCStorage.FactionStorage.LookupByKey(repTemplate.faction[x]);

                        if (factionSpillover.ID == 0)
                        {
                            Log.outError("Spillover faction (faction.dbc) {0} does not exist but is used in `reputation_spillover_template` for faction {1}, skipping", repTemplate.faction[i], factionId);
                            continue;
                        }

                        if (factionSpillover.reputationListID < 0)
                        {
                            Log.outError("Spillover faction (faction.dbc) {0} for faction {1} in `reputation_spillover_template` can not be listed for client, and then useless, skipping",
                                repTemplate.faction[i], factionId);
                            continue;
                        }

                        if (repTemplate.faction_rank[x] >= (uint)ReputationRank.Max)
                        {
                            Log.outError("Rank {0} used in `reputation_spillover_template` for spillover faction {1} is not valid, skipping", repTemplate.faction_rank[i], repTemplate.faction[i]);
                            continue;
                        }
                    }
                }

                factionEntry = DBCStorage.FactionStorage.LookupByKey(repTemplate.faction[0]);
                if (repTemplate.faction[0] != 0 && factionEntry.ID == 0)
                {
                    Log.outError("Faction (faction.dbc) {0} does not exist but is used in `reputation_spillover_template`", repTemplate.faction[0]);
                    continue;
                }
                factionEntry = DBCStorage.FactionStorage.LookupByKey(repTemplate.faction[1]);
                if (repTemplate.faction[1] != 0 && factionEntry.ID == 0)
                {
                    Log.outError("Faction (faction.dbc) {0} does not exist but is used in `reputation_spillover_template`", repTemplate.faction[1]);
                    continue;
                }
                factionEntry = DBCStorage.FactionStorage.LookupByKey(repTemplate.faction[2]);
                if (repTemplate.faction[2] != 0 && factionEntry.ID == 0)
                {
                    Log.outError("Faction (faction.dbc) {0} does not exist but is used in `reputation_spillover_template`", repTemplate.faction[2]);
                    continue;
                }
                factionEntry = DBCStorage.FactionStorage.LookupByKey(repTemplate.faction[3]);
                if (repTemplate.faction[3] != 0 && factionEntry.ID == 0)
                {
                    Log.outError("Faction (faction.dbc) {0} does not exist but is used in `reputation_spillover_template`", repTemplate.faction[3]);
                    continue;
                }
                factionEntry = DBCStorage.FactionStorage.LookupByKey(repTemplate.faction[4]);
                if (repTemplate.faction[4] != 0 && factionEntry.ID == 0)
                {
                    Log.outError("Faction (faction.dbc) {0} does not exist but is used in `reputation_spillover_template`", repTemplate.faction[4]);
                    continue;
                }
                RepSpilloverStorage.Add(factionId, repTemplate);
                ++count;
            }
            Log.outInfo("Loaded {0} reputation_spillover_template in {1} ms", count, Time.getMSTimeDiffNow(time));
        }

        //Creatures
        public void LoadCreatureTemplates()
        {
            var time = Time.getMSTime();
            //                                         0              1                 2                  3                 4            5           6        7         8
            SQLResult result = DB.World.Select("SELECT entry, difficulty_entry_1, difficulty_entry_2, difficulty_entry_3, KillCredit1, KillCredit2, modelid1, modelid2, modelid3, " +
                //9         10      11       12           13           14        15     16      17        18        19         20         21
                "modelid4, name, subname, IconName, gossip_menu_id, minlevel, maxlevel, exp, exp_unk, faction_A, faction_H, npcflag, speed_walk, " +
                //22          23     24     25     26       27           28             29              30               31            32          33          34
                "speed_run, scale, rank, mindmg, maxdmg, dmgschool, attackpower, dmg_multiplier, baseattacktime, rangeattacktime, unit_class, unit_flags, unit_flags2, " +
                //35             36         37             38             39             40          41           42              43           44
                "dynamicflags, family, trainer_type, trainer_class, trainer_race, minrangedmg, maxrangedmg, rangedattackpower, type, " +
                //45           46        47         48            49          50          51           52           53           54         55
                "type_flags, type_flags2, lootid, pickpocketloot, skinloot, resistance1, resistance2, resistance3, resistance4, resistance5, resistance6, " +
                //56       57      58      59      60      61      62      63       64               65       66       67       68         69
                "spell1, spell2, spell3, spell4, spell5, spell6, spell7, spell8, PetSpellDataId, VehicleId, mingold, maxgold, AIName, MovementType, " +
                //70           71         72         73            74            75          76           77          78          79           80          81
                "InhabitType, HoverHeight, Health_mod, Mana_mod, Mana_mod_extra, Armor_mod, RacialLeader, questItem1, questItem2, questItem3, questItem4, questItem5, " +
                //82           83            84         85               86                  87          88
                "questItem6, movementId, RegenHealth, mechanic_immune_mask, flags_extra, ScriptName FROM creature_template;");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 creatures. DB table `creature_template` is empty.");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint entry = result.Read<uint>(i, 0);

                CreatureTemplate creature = new CreatureTemplate();
                creature.Entry = entry;

                for (var x = 0; x < SharedConst.MaxDifficulty - 1; ++x)
                    creature.DifficultyEntry[x] = result.Read<uint>(i, 1 + x);

                for (var x = 0; x < 2; ++x)
                    creature.KillCredit[x] = result.Read<uint>(i, 4 + x);

                creature.ModelId[0] = result.Read<uint>(i, 6);
                creature.ModelId[1] = result.Read<uint>(i, 7);
                creature.ModelId[2] = result.Read<uint>(i, 8);
                creature.ModelId[3] = result.Read<uint>(i, 9);
                creature.Name = result.Read<string>(i, 10);
                creature.SubName = result.Read<string>(i, 11);
                creature.IconName = result.Read<string>(i, 12);
                creature.GossipMenuId = result.Read<uint>(i, 13);
                creature.Minlevel = result.Read<uint>(i, 14);
                creature.Maxlevel = result.Read<uint>(i, 15);
                creature.expansion = result.Read<uint>(i, 16);
                creature.expansionUnk = result.Read<uint>(i, 17);
                creature.FactionA = result.Read<uint>(i, 18);
                creature.FactionH = result.Read<uint>(i, 19);
                creature.Npcflag = (NPCFlags)result.Read<uint>(i, 20);
                creature.SpeedWalk = result.Read<float>(i, 21);
                creature.SpeedRun = result.Read<float>(i, 22);
                creature.Scale = result.Read<float>(i, 23);
                creature.Rank = result.Read<uint>(i, 24);
                creature.Mindmg = result.Read<float>(i, 25);
                creature.Maxdmg = result.Read<float>(i, 26);
                creature.DmgSchool = result.Read<uint>(i, 27);
                creature.AttackPower = result.Read<uint>(i, 28);
                creature.DmgMultiplier = result.Read<float>(i, 29);
                creature.baseattacktime = result.Read<uint>(i, 30);
                creature.rangeattacktime = result.Read<uint>(i, 31);
                creature.UnitClass = result.Read<uint>(i, 32);
                creature.UnitFlags = result.Read<uint>(i, 33);
                creature.UnitFlags2 = result.Read<uint>(i, 34);
                creature.DynamicFlags = result.Read<uint>(i, 35);
                creature.Family = result.Read<uint>(i, 36);
                creature.TrainerType = (TrainerType)result.Read<uint>(i, 37);
                //creature.TrainerSpell = result.Read<uint>(i, 38);
                creature.TrainerClass = (Class)result.Read<uint>(i, 38);
                creature.TrainerRace = (Race)result.Read<uint>(i, 39);
                creature.MinRangeDmg = result.Read<float>(i, 40);
                creature.MaxRangeDmg = result.Read<float>(i, 41);
                creature.RangedAttackPower = result.Read<uint>(i, 42);
                creature.CreatureType = result.Read<uint>(i, 43);
                creature.TypeFlags = (CreatureTypeFlags)result.Read<uint>(i, 44);
                creature.TypeFlags2 = result.Read<uint>(i, 45);
                creature.LootId = result.Read<uint>(i, 46);
                creature.PickPocketId = result.Read<uint>(i, 47);
                creature.SkinLootId = result.Read<uint>(i, 48);

                for (var x = (int)SpellSchools.Holy; x < (int)SpellSchools.Max; ++x)
                    creature.Resistance[x] = result.Read<int>(i, 49 + x - 1);

                for (var x = 0; x < 8; ++x)
                    creature.Spells[x] = result.Read<uint>(i, 55 + x);

                creature.PetSpellDataId = result.Read<uint>(i, 63);
                creature.VehicleId = result.Read<uint>(i, 64);
                creature.MinGold = result.Read<uint>(i, 65);
                creature.MaxGold = result.Read<uint>(i, 66);
                creature.AIName = result.Read<string>(i, 67);
                creature.MovementType = result.Read<uint>(i, 68);
                creature.InhabitType = (InhabitType)result.Read<uint>(i, 69);
                creature.HoverHeight = result.Read<float>(i, 70);
                creature.HeathMod = result.Read<float>(i, 71);
                creature.ManaMod = result.Read<float>(i, 72);
                creature.ManaExtraMod = result.Read<float>(i, 73);
                creature.ArmorMod = result.Read<float>(i, 74);
                creature.RacialLeader = result.Read<bool>(i, 75);

                for (var x = 0; x < 6; ++x)
                    creature.QuestItems[x] = result.Read<uint>(i, 76 + x);

                creature.MovementId = result.Read<uint>(i, 82);
                creature.RegenHealth = result.Read<bool>(i, 83);
                //creature.EquipmentId = result.Read<uint>(i, 85);
                creature.MechanicImmuneMask = result.Read<uint>(i, 84);
                creature.FlagsExtra = (CreatureFlagsExtra)result.Read<uint>(i, 85);
                //creatureTemplate.ScriptID           = GetScriptId(result.Read<string>(i, 86));

                count++;
                creatureTemplateStorage.Add(entry, creature);
            }

            // Checking needs to be done after loading because of the difficulty self referencing
            //for (CreatureTemplateContainer::const_iterator itr = _creatureTemplateStore.begin(); itr != _creatureTemplateStore.end(); ++itr)
            //CheckCreatureTemplate(&itr->second);

            Log.outInfo("Loaded {0} creature definitions in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        public void LoadCreatureTemplateAddons()
        {
            var time = Time.getMSTime();
            //                                         0      1        2      3       4       5      6
            SQLResult result = DB.World.Select("SELECT entry, path_id, mount, bytes1, bytes2, emote, auras FROM creature_template_addon");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 creature template addon definitions. DB table `creature_template_addon` is empty.");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint entry = result.Read<uint>(i, 0);
                if (GetCreatureTemplate(entry) == null)
                {
                    Log.outError("Creature template (Entry: {0}) does not exist but has a record in `creature_template_addon`", entry);
                    continue;
                }

                CreatureAddon creatureAddon = new CreatureAddon();
                creatureAddon.path_id = result.Read<uint>(i, 1);
                creatureAddon.mount = result.Read<uint>(i, 2);
                creatureAddon.bytes1 = result.Read<uint>(i, 3);
                creatureAddon.bytes2 = result.Read<uint>(i, 4);
                creatureAddon.emote = result.Read<uint>(i, 5);

                string temp = result.Read<string>(i, 6);
                string[] tokens = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                creatureAddon.auras = new uint[tokens.Count()];
                byte x = 0;
                foreach (var itr in tokens)
                {
                    uint id;
                    uint.TryParse(itr, out id);
                    SpellInfo AdditionalSpellInfo = Cypher.SpellMgr.GetSpellInfo(id);
                    if (AdditionalSpellInfo == null)
                    {
                        Log.outError("Creature (Entry: {0}) has wrong spell {1} defined in `auras` field in `creature_template_addon`.", entry, id);
                        continue;
                    }
                    creatureAddon.auras[x++] = id;
                }

                if (creatureAddon.mount != 0)
                {
                    if (DBCStorage.CreatureDisplayInfoStorage.LookupByKey(creatureAddon.mount) == null)
                    {
                        Log.outError("Creature (Entry: {0}) has invalid displayInfoId ({1}) for mount defined in `creature_template_addon`", entry, creatureAddon.mount);
                        creatureAddon.mount = 0;
                    }
                }

                //if (!sEmotesStore.LookupEntry(creatureAddon.emote))
                {
                    //Log.outError("Creature (Entry: {0}) has invalid emote ({1}) defined in `creature_addon`.", entry, creatureAddon.emote);
                    //creatureAddon.emote = 0;
                }
                creatureTemplateAddonStorage.Add(entry, creatureAddon);
                count++;
            }
            Log.outInfo("Loaded {0} creature template addons in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        /*
        void CheckCreatureTemplate(CreatureTemplate cInfo)
        {
            if (!cInfo)
                return;

            bool ok = true;                                     // bool to allow continue outside this loop
            for (uint32 diff = 0; diff < MAX_DIFFICULTY - 1 && ok; ++diff)
            {
                if (!cInfo->DifficultyEntry[diff])
                    continue;
                ok = false;                                     // will be set to true at the end of this loop again

                CreatureTemplate difficultyInfo = GetCreatureTemplate(cInfo->DifficultyEntry[diff]);
                if (!difficultyInfo)
                {
                    Log.outError("Creature (Entry: %u) has `difficulty_entry_%u`=%u but creature entry %u does not exist.",
                        cInfo->Entry, diff + 1, cInfo->DifficultyEntry[diff], cInfo->DifficultyEntry[diff]);
                    continue;
                }

                bool ok2 = true;
                for (uint32 diff2 = 0; diff2 < MAX_DIFFICULTY - 1 && ok2; ++diff2)
                {
                    ok2 = false;
                    if (_difficultyEntries[diff2].find(cInfo->Entry) != _difficultyEntries[diff2].end())
                    {
                        Log.outError("Creature (Entry: %u) is listed as `difficulty_entry_%u` of another creature, but itself lists %u in `difficulty_entry_%u`.",
                            cInfo->Entry, diff2 + 1, cInfo->DifficultyEntry[diff], diff + 1);
                        continue;
                    }

                    if (_difficultyEntries[diff2].find(cInfo->DifficultyEntry[diff]) != _difficultyEntries[diff2].end())
                    {
                        Log.outError("Creature (Entry: %u) already listed as `difficulty_entry_%u` for another entry.", cInfo->DifficultyEntry[diff], diff2 + 1);
                        continue;
                    }

                    if (_hasDifficultyEntries[diff2].find(cInfo->DifficultyEntry[diff]) != _hasDifficultyEntries[diff2].end())
                    {
                        Log.outError("Creature (Entry: %u) has `difficulty_entry_%u`=%u but creature entry %u has itself a value in `difficulty_entry_%u`.",
                            cInfo->Entry, diff + 1, cInfo->DifficultyEntry[diff], cInfo->DifficultyEntry[diff], diff2 + 1);
                        continue;
                    }
                    ok2 = true;
                }
                if (!ok2)
                    continue;

                if (cInfo->unit_class != difficultyInfo->unit_class)
                {
                    Log.outError("Creature (Entry: %u, class %u) has different `unit_class` in difficulty %u mode (Entry: %u, class %u).",
                        cInfo->Entry, cInfo->unit_class, diff + 1, cInfo->DifficultyEntry[diff], difficultyInfo->unit_class);
                    continue;
                }

                if (cInfo->npcflag != difficultyInfo->npcflag)
                {
                    Log.outError("Creature (Entry: %u) has different `npcflag` in difficulty %u mode (Entry: %u).", cInfo->Entry, diff + 1, cInfo->DifficultyEntry[diff]);
                    continue;
                }

                if (cInfo->trainer_class != difficultyInfo->trainer_class)
                {
                    Log.outError("Creature (Entry: %u) has different `trainer_class` in difficulty %u mode (Entry: %u).", cInfo->Entry, diff + 1, cInfo->DifficultyEntry[diff]);
                    continue;
                }

                if (cInfo->trainer_race != difficultyInfo->trainer_race)
                {
                    Log.outError("Creature (Entry: %u) has different `trainer_race` in difficulty %u mode (Entry: %u).", cInfo->Entry, diff + 1, cInfo->DifficultyEntry[diff]);
                    continue;
                }

                if (cInfo->trainer_type != difficultyInfo->trainer_type)
                {
                    Log.outError("Creature (Entry: %u) has different `trainer_type` in difficulty %u mode (Entry: %u).", cInfo->Entry, diff + 1, cInfo->DifficultyEntry[diff]);
                    continue;
                }

                if (cInfo->trainer_spell != difficultyInfo->trainer_spell)
                {
                    Log.outError("Creature (Entry: %u) has different `trainer_spell` in difficulty %u mode (Entry: %u).", cInfo->Entry, diff + 1, cInfo->DifficultyEntry[diff]);
                    continue;
                }

                if (!difficultyInfo->AIName.empty())
                {
                    Log.outError("Creature (Entry: %u) lists difficulty %u mode entry %u with `AIName` filled in. `AIName` of difficulty 0 mode creature is always used instead.",
                        cInfo->Entry, diff + 1, cInfo->DifficultyEntry[diff]);
                    continue;
                }

                if (difficultyInfo->ScriptID)
                {
                    Log.outError("Creature (Entry: %u) lists difficulty %u mode entry %u with `ScriptName` filled in. `ScriptName` of difficulty 0 mode creature is always used instead.",
                        cInfo->Entry, diff + 1, cInfo->DifficultyEntry[diff]);
                    continue;
                }

                _hasDifficultyEntries[diff].insert(cInfo->Entry);
                _difficultyEntries[diff].insert(cInfo->DifficultyEntry[diff]);
                ok = true;
            }

            FactionTemplateEntry factionTemplate = sFactionTemplateStore.LookupEntry(cInfo->faction_A);
            if (!factionTemplate)
                Log.outError("Creature (Entry: %u) has non-existing faction_A template (%u).", cInfo->Entry, cInfo->faction_A);

            factionTemplate = sFactionTemplateStore.LookupEntry(cInfo->faction_H);
            if (!factionTemplate)
                Log.outError("Creature (Entry: %u) has non-existing faction_H template (%u).", cInfo->Entry, cInfo->faction_H);

            // used later for scale
            CreatureDisplayInfoEntry displayScaleEntry = NULL;

            if (cInfo->Modelid1)
            {
                CreatureDisplayInfoEntry displayEntry = sCreatureDisplayInfoStore.LookupEntry(cInfo->Modelid1);
                if (!displayEntry)
                {
                    Log.outError("Creature (Entry: %u) lists non-existing Modelid1 id (%u), this can crash the client.", cInfo->Entry, cInfo->Modelid1);
                    const_cast<CreatureTemplate*>(cInfo)->Modelid1 = 0;
                }
                else if (!displayScaleEntry)
                    displayScaleEntry = displayEntry;

                CreatureModelInfo modelInfo = GetCreatureModelInfo(cInfo->Modelid1);
                if (!modelInfo)
                    Log.outError("No model data exist for `Modelid1` = %u listed by creature (Entry: %u).", cInfo->Modelid1, cInfo->Entry);
            }

            if (cInfo->Modelid2)
            {
                CreatureDisplayInfoEntry displayEntry = sCreatureDisplayInfoStore.LookupEntry(cInfo->Modelid2);
                if (!displayEntry)
                {
                    Log.outError("Creature (Entry: %u) lists non-existing Modelid2 id (%u), this can crash the client.", cInfo->Entry, cInfo->Modelid2);
                    const_cast<CreatureTemplate*>(cInfo)->Modelid2 = 0;
                }
                else if (!displayScaleEntry)
                    displayScaleEntry = displayEntry;

                CreatureModelInfo modelInfo = GetCreatureModelInfo(cInfo->Modelid2);
                if (!modelInfo)
                    Log.outError("No model data exist for `Modelid2` = %u listed by creature (Entry: %u).", cInfo->Modelid2, cInfo->Entry);
            }

            if (cInfo->Modelid3)
            {
                CreatureDisplayInfoEntry displayEntry = sCreatureDisplayInfoStore.LookupEntry(cInfo->Modelid3);
                if (!displayEntry)
                {
                    Log.outError("Creature (Entry: %u) lists non-existing Modelid3 id (%u), this can crash the client.", cInfo->Entry, cInfo->Modelid3);
                    const_cast<CreatureTemplate*>(cInfo)->Modelid3 = 0;
                }
                else if (!displayScaleEntry)
                    displayScaleEntry = displayEntry;

                CreatureModelInfo modelInfo = GetCreatureModelInfo(cInfo->Modelid3);
                if (!modelInfo)
                    Log.outError("No model data exist for `Modelid3` = %u listed by creature (Entry: %u).", cInfo->Modelid3, cInfo->Entry);
            }

            if (cInfo->Modelid4)
            {
                CreatureDisplayInfoEntry displayEntry = sCreatureDisplayInfoStore.LookupEntry(cInfo->Modelid4);
                if (!displayEntry)
                {
                    Log.outError("Creature (Entry: %u) lists non-existing Modelid4 id (%u), this can crash the client.", cInfo->Entry, cInfo->Modelid4);
                    const_cast<CreatureTemplate*>(cInfo)->Modelid4 = 0;
                }
                else if (!displayScaleEntry)
                    displayScaleEntry = displayEntry;

                CreatureModelInfo modelInfo = GetCreatureModelInfo(cInfo->Modelid4);
                if (!modelInfo)
                    Log.outError("No model data exist for `Modelid4` = %u listed by creature (Entry: %u).", cInfo->Modelid4, cInfo->Entry);
            }

            if (!displayScaleEntry)
                Log.outError("Creature (Entry: %u) does not have any existing display id in Modelid1/Modelid2/Modelid3/Modelid4.", cInfo->Entry);

            for (int k = 0; k < MAX_KILL_CREDIT; ++k)
            {
                if (cInfo->KillCredit[k])
                {
                    if (!GetCreatureTemplate(cInfo->KillCredit[k]))
                    {
                        Log.outError("Creature (Entry: %u) lists non-existing creature entry %u in `KillCredit%d`.", cInfo->Entry, cInfo->KillCredit[k], k + 1);
                        const_cast<CreatureTemplate*>(cInfo)->KillCredit[k] = 0;
                    }
                }
            }

            if (!cInfo->unit_class || ((1 << (cInfo->unit_class-1)) & CLASSMASK_ALL_CREATURES) == 0)
            {
                Log.outError("Creature (Entry: %u) has invalid unit_class (%u) in creature_template. Set to 1 (UNIT_CLASS_WARRIOR).", cInfo->Entry, cInfo->unit_class);
                const_cast<CreatureTemplate*>(cInfo)->unit_class = UNIT_CLASS_WARRIOR;
            }

            if (cInfo->dmgschool >= MAX_SPELL_SCHOOL)
            {
                Log.outError("Creature (Entry: %u) has invalid spell school value (%u) in `dmgschool`.", cInfo->Entry, cInfo->dmgschool);
                const_cast<CreatureTemplate*>(cInfo)->dmgschool = SPELL_SCHOOL_NORMAL;
            }

            if (cInfo->baseattacktime == 0)
                const_cast<CreatureTemplate*>(cInfo)->baseattacktime  = BASE_ATTACK_TIME;

            if (cInfo->rangeattacktime == 0)
                const_cast<CreatureTemplate*>(cInfo)->rangeattacktime = BASE_ATTACK_TIME;

            if ((cInfo->npcflag & UNIT_NPC_FLAG_TRAINER) && cInfo->trainer_type >= MAX_TRAINER_TYPE)
                Log.outError("Creature (Entry: %u) has wrong trainer type %u.", cInfo->Entry, cInfo->trainer_type);

            if (cInfo->type && !sCreatureTypeStore.LookupEntry(cInfo->type))
            {
                Log.outError("Creature (Entry: %u) has invalid creature type (%u) in `type`.", cInfo->Entry, cInfo->type);
                const_cast<CreatureTemplate*>(cInfo)->type = CREATURE_TYPE_HUMANOID;
            }

            // must exist or used hidden but used in data horse case
            if (cInfo->family && !sCreatureFamilyStore.LookupEntry(cInfo->family) && cInfo->family != CREATURE_FAMILY_HORSE_CUSTOM)
            {
                Log.outError("Creature (Entry: %u) has invalid creature family (%u) in `family`.", cInfo->Entry, cInfo->family);
                const_cast<CreatureTemplate*>(cInfo)->family = 0;
            }

            if (cInfo->InhabitType <= 0 || cInfo->InhabitType > INHABIT_ANYWHERE)
            {
                Log.outError("Creature (Entry: %u) has wrong value (%u) in `InhabitType`, creature will not correctly walk/swim/fly.", cInfo->Entry, cInfo->InhabitType);
                const_cast<CreatureTemplate*>(cInfo)->InhabitType = INHABIT_ANYWHERE;
            }

            if (cInfo->HoverHeight < 0.0f)
            {
                Log.outError("Creature (Entry: %u) has wrong value (%f) in `HoverHeight`", cInfo->Entry, cInfo->HoverHeight);
                const_cast<CreatureTemplate*>(cInfo)->HoverHeight = 1.0f;
            }

            if (cInfo->VehicleId)
            {
                VehicleEntry vehId = sVehicleStore.LookupEntry(cInfo->VehicleId);
                if (!vehId)
                {
                     Log.outError("Creature (Entry: %u) has a non-existing VehicleId (%u). This *WILL* cause the client to freeze!", cInfo->Entry, cInfo->VehicleId);
                     const_cast<CreatureTemplate*>(cInfo)->VehicleId = 0;
                }
            }

            if (cInfo->PetSpellDataId)
            {
                CreatureSpellDataEntry spellDataId = sCreatureSpellDataStore.LookupEntry(cInfo->PetSpellDataId);
                if (!spellDataId)
                    Log.outError("Creature (Entry: %u) has non-existing PetSpellDataId (%u).", cInfo->Entry, cInfo->PetSpellDataId);
            }

            for (uint8 j = 0; j < CREATURE_MAX_SPELLS; ++j)
            {
                if (cInfo->spells[j] && !sSpellMgr->GetSpellInfo(cInfo->spells[j]))
                {
                    Log.outError("Creature (Entry: %u) has non-existing Spell%d (%u), set to 0.", cInfo->Entry, j+1, cInfo->spells[j]);
                    const_cast<CreatureTemplate*>(cInfo)->spells[j] = 0;
                }
            }

            if (cInfo->MovementType >= MAX_DB_MOTION_TYPE)
            {
                Log.outError("Creature (Entry: %u) has wrong movement generator type (%u), ignored and set to IDLE.", cInfo->Entry, cInfo->MovementType);
                const_cast<CreatureTemplate*>(cInfo)->MovementType = IDLE_MOTION_TYPE;
            }

            if (cInfo->equipmentId > 0)                          // 0 no equipment
            {
                if (!GetEquipmentInfo(cInfo->equipmentId))
                {
                    Log.outError("Table `creature_template` lists creature (Entry: %u) with `equipment_id` %u not found in table `creature_equip_template`, set to no equipment.", cInfo->Entry, cInfo->equipmentId);
                    const_cast<CreatureTemplate*>(cInfo)->equipmentId = 0;
                }
            }

            /// if not set custom creature scale then load scale from CreatureDisplayInfo.dbc
            if (cInfo->scale <= 0.0f)
            {
                if (displayScaleEntry)
                    const_cast<CreatureTemplate*>(cInfo)->scale = displayScaleEntry->scale;
                else
                    const_cast<CreatureTemplate*>(cInfo)->scale = 1.0f;
            }

            if (cInfo->expansion > MAX_CREATURE_BASE_HP)
            {
                Log.outError("Table `creature_template` lists creature (Entry: %u) with `exp` %u. Ignored and set to 0.", cInfo->Entry, cInfo->expansion);
                const_cast<CreatureTemplate*>(cInfo)->expansion = 0;
            }

            if (cInfo->expansionUnknown > MAX_CREATURE_BASE_HP)
            {
                Log.outError("Table `creature_template` lists creature (Entry: %u) with `exp_unk` %u. Ignored and set to 0.", cInfo->Entry, cInfo->expansionUnknown);
                const_cast<CreatureTemplate*>(cInfo)->expansionUnknown = 0;
            }

            if (uint32 badFlags = (cInfo->flags_extra & ~CREATURE_FLAG_EXTRA_DB_ALLOWED))
            {
                Log.outError("Table `creature_template` lists creature (Entry: %u) with disallowed `flags_extra` %u, removing incorrect flag.", cInfo->Entry, badFlags);
                const_cast<CreatureTemplate*>(cInfo)->flags_extra &= CREATURE_FLAG_EXTRA_DB_ALLOWED;
            }

            const_cast<CreatureTemplate*>(cInfo)->dmg_multiplier *= Creature::_GetDamageMod(cInfo->rank);
        }
        */
        public void LoadCreatureAddons()
        {
            var time = Time.getMSTime();
            //                                         0       1       2      3       4       5      6
            SQLResult result = DB.World.Select("SELECT guid, path_id, mount, bytes1, bytes2, emote, auras FROM creature_addon");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 creature addon definitions. DB table `creature_addon` is empty.");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint guid = result.Read<uint>(i, 0);

                CreatureData creData = GetCreatureData(guid);
                if (creData == null)
                {
                    Log.outError("Creature (GUID: {0}) does not exist but has a record in `creature_addon`", guid);
                    continue;
                }

                CreatureAddon creatureAddon = new CreatureAddon();

                creatureAddon.path_id = result.Read<uint>(i, 1);
                //if (creData.movementType == WAYPOINT_MOTION_TYPE && !creatureAddon.path_id)
                {
                    //const_cast<CreatureData*>(creData)->movementType = IDLE_MOTION_TYPE;
                    //Log.outError("Creature (GUID {0}) has movement type set to WAYPOINT_MOTION_TYPE but no path assigned", guid);
                }

                creatureAddon.mount = result.Read<uint>(i, 2);
                creatureAddon.bytes1 = result.Read<uint>(i, 3);
                creatureAddon.bytes2 = result.Read<uint>(i, 4);
                creatureAddon.emote = result.Read<uint>(i, 5);


                string[] tokens = result.Read<string>(i, 6).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                byte x = 0;
                creatureAddon.auras = new uint[tokens.Count()];
                foreach (var itr in tokens)
                {
                    uint id;
                    uint.TryParse(itr, out id);
                    SpellInfo AdditionalSpellInfo = Cypher.SpellMgr.GetSpellInfo(id);
                    if (AdditionalSpellInfo == null)
                    {
                        Log.outError("Creature (GUID: {0}) has wrong spell {1} defined in `auras` field in `creature_addon`.", guid, id);
                        continue;
                    }
                    creatureAddon.auras[x++] = id;
                }

                if (creatureAddon.mount != 0)
                {
                    if (DBCStorage.CreatureDisplayInfoStorage.LookupByKey(creatureAddon.mount) == null)
                    {
                        Log.outError("Creature (GUID: {0}) has invalid displayInfoId ({1}) for mount defined in `creature_addon`", guid, creatureAddon.mount);
                        creatureAddon.mount = 0;
                    }
                }

                //if (!sEmotesStore.LookupEntry(creatureAddon.emote))
                {
                    //Log.outError("Creature (GUID: %u) has invalid emote (%u) defined in `creature_addon`.", guid, creatureAddon.emote);
                    //creatureAddon.emote = 0;
                }
                creatureAddonStorage.Add(guid, creatureAddon);
                count++;
            }

            Log.outInfo("Loaded {0} creature addons in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        public void LoadEquipmentTemplates()
        {
            var time = Time.getMSTime();
            SQLResult result = DB.World.Select("SELECT entry, id, itemEntry1, itemEntry2, itemEntry3 FROM creature_equip_template");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 creature equipment templates. DB table `creature_equip_template` is empty!");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint entry = result.Read<uint>(i, 0);

                EquipmentInfo equipmentInfo = new EquipmentInfo();// _equipmentInfoStore[entry];

                if (GetCreatureTemplate(entry) == null)
                {
                    Log.outError("Creature template (Entry: {0}) does not exist but has a record in `creature_equip_template`", entry);
                    continue;
                }

                uint id = result.Read<uint>(i, 1);

                equipmentInfo.ItemEntry[0] = result.Read<uint>(i, 2);
                equipmentInfo.ItemEntry[1] = result.Read<uint>(i, 3);
                equipmentInfo.ItemEntry[2] = result.Read<uint>(i, 4);

                for (var x = 0; x < CreatureConst.MaxEquipmentItems; ++x)
                {
                    if (equipmentInfo.ItemEntry[x] == 0)
                        continue;

                    ItemEntry dbcItem = DB2Storage.ItemEntryStorage.LookupByKey(equipmentInfo.ItemEntry[x]);

                    if (dbcItem == null)
                    {
                        Log.outError("Unknown item (entry {0}) in creature_equip_template.itemEntry{1} for entry = {2}, forced to 0.",
                            equipmentInfo.ItemEntry[x], x + 1, entry);
                        equipmentInfo.ItemEntry[x] = 0;
                        continue;
                    }

                    if (dbcItem.inventoryType != (uint)InventoryType.Weapon &&
                        dbcItem.inventoryType != (uint)InventoryType.Shield &&
                        dbcItem.inventoryType != (uint)InventoryType.Ranged &&
                        dbcItem.inventoryType != (uint)InventoryType.Weapon2Hand &&
                        dbcItem.inventoryType != (uint)InventoryType.WeaponMainhand &&
                        dbcItem.inventoryType != (uint)InventoryType.WeaponOffhand &&
                        dbcItem.inventoryType != (uint)InventoryType.Holdable &&
                        dbcItem.inventoryType != (uint)InventoryType.Thrown &&
                        dbcItem.inventoryType != (uint)InventoryType.RangedRight)
                    {
                        Log.outError("Item (entry {0}) in creature_equip_template.itemEntry{1} for entry = {2} is not equipable in a hand, forced to 0.",
                            equipmentInfo.ItemEntry[x], x + 1, entry);
                        equipmentInfo.ItemEntry[x] = 0;
                    }
                }

                if (!equipmentInfoStorage.ContainsKey(entry))
                    equipmentInfoStorage[entry] = new List<Tuple<uint, EquipmentInfo>>();

                equipmentInfoStorage[entry].Add(new Tuple<uint,EquipmentInfo>(id, equipmentInfo));
                ++count;
            }
            Log.outInfo("Loaded {0} equipment templates in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        public void LoadCreatureClassLevelStats()
        {
            var time = Time.getMSTime();
            SQLResult result = DB.World.Select("SELECT level, class, basehp0, basehp1, basehp2, basehp3, basemana, basearmor FROM creature_classlevelstats");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 creature base stats. DB table `creature_classlevelstats` is empty.");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                byte Level = result.Read<byte>(i, 0);
                byte Class = result.Read<byte>(i, 1);

                CreatureBaseStats stats = new CreatureBaseStats();

                for (var x = 0; x < CreatureConst.MaxBaseHp; ++x)
                    stats.BaseHealth[x] = result.Read<uint>(i, x + 2);

                stats.BaseMana = result.Read<uint>(i, 5);
                stats.BaseArmor = result.Read<uint>(i, 6);

                if (Class == 0 || ((1 << (Class - 1)) & SharedConst.ClassMaskAllCreatures) == 0)
                    Log.outError("Creature base stats for level {0} has invalid class {1}", Level, Class);

                for (byte x = 0; x < CreatureConst.MaxBaseHp; ++x)
                {
                    if (stats.BaseHealth[x] < 1)
                    {
                        Log.outError("Creature base stats for class {0}, level {1} has invalid zero base HP[{2}] - set to 1", Class, Level, i);
                        stats.BaseHealth[x] = 1;
                    }
                }
                creatureBaseStatsStorage.Add(WorldObject.MakePair16(Level, Class), stats);
                ++count;
            }

            foreach (var itr in creatureTemplateStorage.Values)
            {
                for (var lvl = itr.Minlevel; lvl <= itr.Maxlevel; ++lvl)
                {
                    if (creatureBaseStatsStorage.LookupByKey(WorldObject.MakePair16(lvl, itr.UnitClass)) == null)
                        Log.outError("Missing base stats for creature class {0} level {1}", itr.UnitClass, lvl);
                }
            }

            Log.outInfo("Loaded {0} creature base stats in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        public void LoadCreatureModelInfo()
        {
            var time = Time.getMSTime();
            SQLResult result = DB.World.Select("SELECT modelid, bounding_radius, combat_reach, gender, modelid_other_gender FROM creature_model_info");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 creature model definitions. DB table `creature_model_info` is empty.");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint modelId = result.Read<uint>(i, 0);

                CreatureModelInfo modelInfo = new CreatureModelInfo();
                modelInfo.bounding_radius = result.Read<float>(i, 1);
                modelInfo.combat_reach = result.Read<float>(i, 2);
                modelInfo.gender = result.Read<byte>(i, 3);
                modelInfo.modelid_other_gender = result.Read<uint>(i, 4);

                // Checks

                if (DBCStorage.CreatureDisplayInfoStorage.LookupByKey(modelId) == null)
                    Log.outError("Table `creature_model_info` has model for not existed display id ({0}).", modelId);

                if (modelInfo.gender > (byte)Gender.None)
                {
                    Log.outError("Table `creature_model_info` has wrong gender ({0}) for display id ({1}).", modelInfo.gender, modelId);
                    modelInfo.gender = (byte)Gender.Male;
                }

                if (modelInfo.modelid_other_gender != 0 && DBCStorage.CreatureDisplayInfoStorage.LookupByKey(modelInfo.modelid_other_gender) == null)
                {
                    Log.outError("Table `creature_model_info` has not existed alt.gender model ({0}) for existed display id ({1}).", modelInfo.modelid_other_gender, modelId);
                    modelInfo.modelid_other_gender = 0;
                }

                //if (modelInfo.combat_reach < 0.1f)
                //modelInfo.combat_reach = DEFAULT_COMBAT_REACH;
                creatureModelStorage.Add(modelId, modelInfo);
                count++;
            }

            Log.outInfo("Loaded {0} creature model based info in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        /*
        public void LoadLinkedRespawn()
        {
            linkedRespawnStore.Clear();
    //                                                 0        1          2
    SQLResult result = DB.World.Select("SELECT guid, linkedGuid, linkType FROM linked_respawn ORDER BY guid ASC");

    if (result.Count == 0)
    {
        Log.outError("Loaded 0 linked respawns. DB table `linked_respawn` is empty.");
        return;
    }

    for (var i = 0; i < result.Count; i++)
    {
        uint guidLow = result.Read<uint>(i, 0);
        uint linkedGuidLow = result.Read<uint>(i, 1);
        byte linkType = result.Read<byte>(i, 2);

        uint guid = 0, linkedGuid = 0;
        bool error = false;
        switch (linkType)
        {
            case CREATURE_TO_CREATURE:
            {
                const CreatureData slave = GetCreatureData(guidLow);
                if (!slave)
                {
                    Log.outError("Couldn't get creature data for GUIDLow %u", guidLow);
                    error = true;
                    break;
                }

                const CreatureData master = GetCreatureData(linkedGuidLow);
                if (!master)
                {
                    Log.outError("Couldn't get creature data for GUIDLow %u", linkedGuidLow);
                    error = true;
                    break;
                }

                const MapEntry map = sMapStore.LookupEntry(master->mapid);
                if (!map || !map->Instanceable() || (master->mapid != slave->mapid))
                {
                    Log.outError("Creature '%u' linking to '%u' on an unpermitted map.", guidLow, linkedGuidLow);
                    error = true;
                    break;
                }

                if (!(master->spawnMask & slave->spawnMask))  // they must have a possibility to meet (normal/heroic difficulty)
                {
                    Log.outError("LinkedRespawn: Creature '%u' linking to '%u' with not corresponding spawnMask", guidLow, linkedGuidLow);
                    error = true;
                    break;
                }

                guid = MAKE_NEW_GUID(guidLow, slave->id, HIGHGUID_UNIT);
                linkedGuid = MAKE_NEW_GUID(linkedGuidLow, master->id, HIGHGUID_UNIT);
                break;
            }
            case CREATURE_TO_GO:
            {
                const CreatureData slave = GetCreatureData(guidLow);
                if (!slave)
                {
                    Log.outError("Couldn't get creature data for GUIDLow %u", guidLow);
                    error = true;
                    break;
                }

                const GameObjectData master = GetGOData(linkedGuidLow);
                if (!master)
                {
                    Log.outError("Couldn't get gameobject data for GUIDLow %u", linkedGuidLow);
                    error = true;
                    break;
                }

                const MapEntry map = sMapStore.LookupEntry(master->mapid);
                if (!map || !map->Instanceable() || (master->mapid != slave->mapid))
                {
                    Log.outError("Creature '%u' linking to '%u' on an unpermitted map.", guidLow, linkedGuidLow);
                    error = true;
                    break;
                }

                if (!(master->spawnMask & slave->spawnMask))  // they must have a possibility to meet (normal/heroic difficulty)
                {
                    Log.outError("LinkedRespawn: Creature '%u' linking to '%u' with not corresponding spawnMask", guidLow, linkedGuidLow);
                    error = true;
                    break;
                }

                guid = MAKE_NEW_GUID(guidLow, slave->id, HIGHGUID_UNIT);
                linkedGuid = MAKE_NEW_GUID(linkedGuidLow, master->id, HIGHGUID_GAMEOBJECT);
                break;
            }
            case GO_TO_GO:
            {
                const GameObjectData slave = GetGOData(guidLow);
                if (!slave)
                {
                    Log.outError("Couldn't get gameobject data for GUIDLow %u", guidLow);
                    error = true;
                    break;
                }

                const GameObjectData master = GetGOData(linkedGuidLow);
                if (!master)
                {
                    Log.outError("Couldn't get gameobject data for GUIDLow %u", linkedGuidLow);
                    error = true;
                    break;
                }

                const MapEntry map = sMapStore.LookupEntry(master->mapid);
                if (!map || !map->Instanceable() || (master->mapid != slave->mapid))
                {
                    Log.outError("Creature '%u' linking to '%u' on an unpermitted map.", guidLow, linkedGuidLow);
                    error = true;
                    break;
                }

                if (!(master->spawnMask & slave->spawnMask))  // they must have a possibility to meet (normal/heroic difficulty)
                {
                    Log.outError("LinkedRespawn: Creature '%u' linking to '%u' with not corresponding spawnMask", guidLow, linkedGuidLow);
                    error = true;
                    break;
                }

                guid = MAKE_NEW_GUID(guidLow, slave->id, HIGHGUID_GAMEOBJECT);
                linkedGuid = MAKE_NEW_GUID(linkedGuidLow, master->id, HIGHGUID_GAMEOBJECT);
                break;
            }
            case GO_TO_CREATURE:
            {
                const GameObjectData slave = GetGOData(guidLow);
                if (!slave)
                {
                    Log.outError("Couldn't get gameobject data for GUIDLow %u", guidLow);
                    error = true;
                    break;
                }

                const CreatureData master = GetCreatureData(linkedGuidLow);
                if (!master)
                {
                    Log.outError("Couldn't get creature data for GUIDLow %u", linkedGuidLow);
                    error = true;
                    break;
                }

                const MapEntry map = sMapStore.LookupEntry(master->mapid);
                if (!map || !map->Instanceable() || (master->mapid != slave->mapid))
                {
                    Log.outError("Creature '%u' linking to '%u' on an unpermitted map.", guidLow, linkedGuidLow);
                    error = true;
                    break;
                }

                if (!(master->spawnMask & slave->spawnMask))  // they must have a possibility to meet (normal/heroic difficulty)
                {
                    Log.outError("LinkedRespawn: Creature '%u' linking to '%u' with not corresponding spawnMask", guidLow, linkedGuidLow);
                    error = true;
                    break;
                }

                guid = MAKE_NEW_GUID(guidLow, slave->id, HIGHGUID_GAMEOBJECT);
                linkedGuid = MAKE_NEW_GUID(linkedGuidLow, master->id, HIGHGUID_UNIT);
                break;
            }
        }

        if (!error)
            _linkedRespawnStore[guid] = linkedGuid;
    }

    sLog->outInfo(LOG_FILTER_SERVER_LOADING, ">> Loaded " UI64FMTD " linked respawns in %u ms", uint64(_linkedRespawnStore.size()), GetMSTimeDiffToNow(oldMSTime));
        }
        */
        public void LoadTrainerSpell()
        {
            var time = Time.getMSTime();
            SQLResult result = DB.World.Select("SELECT b.entry, a.spell, a.spellcost, a.reqskill, a.reqskillvalue, a.reqlevel FROM npc_trainer AS a " +
                "INNER JOIN npc_trainer AS b ON a.entry = -(b.spell) " +
                "UNION SELECT * FROM npc_trainer WHERE spell > 0");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 Trainers. DB table `npc_trainer` is empty!");
                return;
            }
            int count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint entry = result.Read<uint>(i, 0);
                int spell = result.Read<int>(i, 1);
                uint spellCost = result.Read<uint>(i, 2);
                uint reqSkill = result.Read<uint>(i, 3);
                uint reqSkillValue = result.Read<uint>(i, 4);
                uint reqLevel = result.Read<uint>(i, 5);

                AddSpellToTrainer(entry, spell, spellCost, reqSkill, reqSkillValue, reqLevel);
                count++;
            }

            Log.outInfo("Loaded {0} Trainers in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        void AddSpellToTrainer(uint entry, int spell, uint spellCost, uint reqSkill, uint reqSkillValue, uint reqLevel)
        {
            CreatureTemplate cInfo = GetCreatureTemplate(entry);
            if (cInfo == null)
            {
                Log.outError("Table `npc_trainer` contains an entry for a non-existing creature template (Entry: {0}), ignoring", entry);
                return;
            }

            if (!Convert.ToBoolean(cInfo.Npcflag & NPCFlags.Trainer))
            {
                Log.outError("Table `npc_trainer` contains an entry for a creature template (Entry: {0}) without trainer flag, ignoring", entry);
                return;
            }

            SpellInfo spellinfo = Cypher.SpellMgr.GetSpellInfo((uint)spell);
            if (spellinfo == null)
            {
                Log.outError("Table `npc_trainer` contains an entry (Entry: %u) for a non-existing spell (Spell: %u), ignoring", entry, spell);
                return;
            }

            if (!Cypher.SpellMgr.IsSpellValid(spellinfo))
            {
                Log.outError("Table `npc_trainer` contains an entry (Entry: %u) for a broken spell (Spell: %u), ignoring", entry, spell);
                return;
            }

            //if (GetTalentSpellCost(spell))
            {
                //Log.outError("Table `npc_trainer` contains an entry (Entry: %u) for a non-existing spell (Spell: %u) which is a talent, ignoring", entry, spell);
                //return;
            }

            if (cacheTrainerSpellStorage.LookupByKey(entry) == null)
                cacheTrainerSpellStorage.Add(entry, new TrainerSpellData());


            TrainerSpellData data = cacheTrainerSpellStorage[entry];

            TrainerSpell trainerSpell = new TrainerSpell();
            trainerSpell.spellId = (uint)spell;
            trainerSpell.spellCost = spellCost;
            trainerSpell.reqSkill = reqSkill;
            trainerSpell.reqSkillValue = reqSkillValue;
            trainerSpell.reqLevel = reqLevel;

            if (trainerSpell.reqLevel == 0)
                trainerSpell.reqLevel = spellinfo.SpellLevel;

            // calculate learned spell for profession case when stored cast-spell
            trainerSpell.learnedSpell[0] = (uint)spell;

            for (byte i = 0; i < SharedConst.MaxSpellEffects; ++i)
            {
                if (spellinfo.Effects[i] == null)
                    continue;
                if (spellinfo.Effects[i].Effect != (uint)SpellEffects.LearnSpell)
                    continue;

                if (trainerSpell.learnedSpell[0] == spell)
                    trainerSpell.learnedSpell[0] = 0;
                // player must be able to cast spell on himself
                //if (spellinfo.Effects[i].TargetA.GetTarget() != 0 && spellinfo.Effects[i].TargetA.GetTarget() != TARGET_UNIT_TARGET_ALLY
                //&& spellinfo.Effects[i].TargetA.GetTarget() != TARGET_UNIT_TARGET_ANY && spellinfo.Effects[i].TargetA.GetTarget() != TARGET_UNIT_CASTER)
                {
                    //sLog.outError(LOG_FILTER_SQL, "Table `npc_trainer` has spell %u for trainer entry %u with learn effect which has incorrect target type, ignoring learn effect!", spell, entry);
                    //continue;
                }

                trainerSpell.learnedSpell[i] = spellinfo.Effects[i].TriggerSpell;

                if (trainerSpell.learnedSpell[i] != 0)
                {
                    SpellInfo learnedSpellInfo = Cypher.SpellMgr.GetSpellInfo(trainerSpell.learnedSpell[i]);
                    if (learnedSpellInfo != null && learnedSpellInfo.IsProfession())
                        data.trainerType = 2;
                }
            }
            data.spellList.Add((uint)spell, trainerSpell);
            return;
        }
        public void LoadVendors()
        {
            var time = Time.getMSTime();
            // For reload case
            cacheVendorItemStorage.Clear();

            List<uint> skip_vendors = new List<uint>();

            SQLResult result = DB.World.Select("SELECT entry, item, maxcount, incrtime, ExtendedCost, type FROM npc_vendor ORDER BY entry, slot ASC");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 Vendors. DB table `npc_vendor` is empty!");
                return;
            }

            uint count = 0;

            for (var i = 0; i < result.Count; i++)
            {
                uint entry = result.Read<uint>(i, 0);
                int item_id = result.Read<int>(i, 1);

                // if item is a negative, its a reference
                if (item_id < 0)
                    continue;//skip for now todo: fix me
                //count += LoadReferenceVendor(entry, -item_id, 0, &skip_vendors);
                else
                {
                    uint maxcount = result.Read<uint>(i, 2);
                    uint incrtime = result.Read<uint>(i, 3);
                    uint ExtendedCost = result.Read<uint>(i, 4);
                    byte type = result.Read<byte>(i, 5);

                    //if (!IsVendorItemValid(entry, item_id, maxcount, incrtime, ExtendedCost, type, null, skip_vendors))
                    //continue;

                    if (cacheVendorItemStorage.LookupByKey(entry) == null)
                        cacheVendorItemStorage.Add(entry, new VendorItemData());

                    cacheVendorItemStorage[entry].AddItem(item_id, maxcount, incrtime, ExtendedCost, type);
                    ++count;
                }
            }
            Log.outInfo("Loaded {0} Vendors in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        public void LoadCreatures()
        {
            var time = Time.getMSTime();
            //                                         0              1   2    3        4             5           6           7           8            9              10
            SQLResult result = DB.World.Select("SELECT creature.guid, id, map, modelid, equipment_id, position_x, position_y, position_z, orientation, spawntimesecs, spawndist, " +
                //   11               12         13       14            15         16         17          18          19                20                   21
                "currentwaypoint, curhealth, curmana, MovementType, spawnMask, phaseMask, eventEntry, pool_entry, creature.npcflag, creature.unit_flags, creature.dynamicflags " +
                "FROM creature LEFT OUTER JOIN game_event_creature ON creature.guid = game_event_creature.guid LEFT OUTER JOIN pool_creature ON creature.guid = pool_creature.guid");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 creatures. DB table `creature` is empty.");
                return;
            }

            // Build single time for check spawnmask
            Hashtable spawnMasks = new Hashtable();
            foreach (var map in DBCStorage.MapStorage.Values)
                for (int k = 0; k < SharedConst.MaxDifficulty; ++k)
                    if (GetMapDifficultyData(map.MapID, (Difficulty)k) != null)
                        spawnMasks[map.MapID] = (uint)(spawnMasks[map.MapID] == null ? (uint)(1 << k) : (uint)spawnMasks[map.MapID] | (uint)(1 << k));

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint guid = result.Read<uint>(i, 0);
                uint entry = result.Read<uint>(i, 1);

                CreatureTemplate cInfo = GetCreatureTemplate(entry);
                if (cInfo == null)
                {
                    Log.outError("Table `creature` has creature (GUID: {0}) with non existing creature entry {1}, skipped.", guid, entry);
                    continue;
                }

                CreatureData data = new CreatureData();
                data.id = entry;
                data.mapid = result.Read<uint>(i, 2);
                data.displayid = result.Read<uint>(i, 3);
                data.equipmentId = result.Read<int>(i, 4);
                data.posX = result.Read<float>(i, 5);
                data.posY = result.Read<float>(i, 6);
                data.posZ = result.Read<float>(i, 7);
                data.orientation = result.Read<float>(i, 8);
                data.spawntimesecs = result.Read<uint>(i, 9);
                data.spawndist = result.Read<float>(i, 10);
                data.currentwaypoint = result.Read<uint>(i, 11);
                data.curhealth = result.Read<uint>(i, 12);
                data.curmana = result.Read<uint>(i, 13);
                data.movementType = result.Read<byte>(i, 14);
                data.spawnMask = result.Read<byte>(i, 15);
                data.phaseMask = result.Read<uint>(i, 16);
                //ushort gameEvent = result.Read<ushort>(i, 17);
                //uint PoolId = result.Read<uint>(i, 18);
                data.npcflag = result.Read<uint>(i, 19);
                data.unit_flags = result.Read<uint>(i, 20);
                data.dynamicflags = result.Read<uint>(i, 21);

                MapEntry mapEntry = DBCStorage.MapStorage.LookupByKey(data.mapid);
                if (mapEntry == null)
                {
                    Log.outError("Table `creature` have creature (GUID: {0}) that spawned at not existed map (Id: {1}), skipped.", guid, data.mapid);
                    continue;
                }

                if (data.mapid != 0)
                {
                    //if (Convert.ToBoolean(data.spawnMask & ~Convert.ToUInt32(spawnMasks[data.mapid])))
                        //Log.outError("Table `creature` have creature (GUID: {0}) that have wrong spawn mask {1} including not supported difficulty modes for map (Id: {2}) " +
                            //"spawnMasks[data.mapid]: {3}.", guid, data.spawnMask, data.mapid, spawnMasks[data.mapid]);
                }


                bool ok = true;
                for (uint diff = 0; diff < SharedConst.MaxDifficulty - 1 && ok; ++diff)
                {
                    //if (_difficultyEntries[diff].find(data.id) != _difficultyEntries[diff].end())
                    {
                        //Log.outError("Table `creature` have creature (GUID: %u) that listed as difficulty %u template (entry: %u) in `creature_template`, skipped.",
                        //guid, diff + 1, data.id);
                        //ok = false;
                    }
                }
                if (!ok)
                    continue;

                // -1 random, 0 no equipment,
                //if (data.equipmentId != 0)
                {
                   // if (GetEquipmentInfo(data.id, data.equipmentId) == null)
                    {
                      //  Log.outError("Table `creature` have creature (Entry: {0}) with equipment_id {1} not found in table `creature_equip_template`, set to no equipment.", data.id, data.equipmentId);
                      //  data.equipmentId = 0;
                    }
                }

                if (Convert.ToBoolean(cInfo.FlagsExtra & CreatureFlagsExtra.InstanceBind))
                {
                    if (mapEntry == null || !mapEntry.IsDungeon())
                        Log.outError("Table `creature` have creature (GUID: {0} Entry: {1}) with `creature_template`.`flags_extra` including CREATURE_FLAG_EXTRA_INSTANCE_BIND " +
                            "but creature are not in instance.", guid, data.id);
                }

                if (data.spawndist < 0.0f)
                {
                    Log.outError("Table `creature` have creature (GUID: {0} Entry: {1}) with `spawndist`< 0, set to 0.", guid, data.id);
                    data.spawndist = 0.0f;
                }
                //else if (data.movementType == RANDOM_MOTION_TYPE)
                // {
                //if (data.spawndist == 0.0f)
                //{
                //Log.outError("Table `creature` have creature (GUID: %u Entry: %u) with `MovementType`=1 (random movement) but with `spawndist`=0, replace by idle movement type (0).", guid, data.id);
                //data.movementType = IDLE_MOTION_TYPE;
                //}
                //}
                //else if (data.movementType == IDLE_MOTION_TYPE)
                {
                    //if (data.spawndist != 0.0f)
                    {
                        // Log.outError("Table `creature` have creature (GUID: %u Entry: %u) with `MovementType`=0 (idle) have `spawndist`<>0, set to 0.", guid, data.id);
                        //data.spawndist = 0.0f;
                    }
                }

                if (data.phaseMask == 0)
                {
                    Log.outError("Table `creature` have creature (GUID: {0} Entry: {1}) with `phaseMask`=0 (not visible for anyone), set to 1.", guid, data.id);
                    data.phaseMask = 1;
                }

                // Add to grid if not managed by the game event or pool system
                //if (gameEvent == 0 && PoolId == 0)
                AddCreatureToGrid(guid, data);

                creatureDataStorage.Add(guid, data);
                count++;
            }
            Log.outInfo("Loaded {0} creatures in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        public void AddCreatureToGrid(uint guid, CreatureData data)
        {
            byte mask = data.spawnMask;
            for (byte i = 0; mask != 0; i++, mask >>= 1)
            {
                if (Convert.ToBoolean(mask & 1))
                {
                    CellCoord cellCoord = GridDefines.ComputeCellCoord(data.posX, data.posY);
                    var cell_guids = GetOrCreateCellObjectGuids(data.mapid, i, cellCoord, true);
                    cell_guids.creatures.Add(guid);
                }
            }
        }

        //GameObject
        public void LoadGameObjectTemplate()
        {
            var time = Time.getMSTime();
            //                                           0      1      2        3       4             5          6      7       8     9        10         11          12
            SQLResult result = DB.World.Select("SELECT entry, type, displayId, name, IconName, castBarCaption, unk1, faction, flags, size, questItem1, questItem2, questItem3, " +
                //13          14          15       16     17     18     19     20     21     22     23     24     25      26      27      28
                "questItem4, questItem5, questItem6, data0, data1, data2, data3, data4, data5, data6, data7, data8, data9, data10, data11, data12, " +
                //29      30      31      32      33      34      35      36      37      38      39      40      41      42      43      44
                "data13, data14, data15, data16, data17, data18, data19, data20, data21, data22, data23, data24, data25, data26, data27, data28, " +
                //45      46      47       48       49        50
                "data29, data30, data31, unkInt32, AIName, ScriptName FROM gameobject_template");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 gameobject definitions. DB table `gameobject_template` is empty.");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint entry = result.Read<uint>(i, 0);

                GameObjectTemplate got = new GameObjectTemplate();

                got.entry = entry;
                got.type = (GameObjectTypes)result.Read<uint>(i, 1);
                got.displayId = result.Read<uint>(i, 2);
                got.name = result.Read<string>(i, 3);
                got.IconName = result.Read<string>(i, 4);
                got.castBarCaption = result.Read<string>(i, 5);
                got.unk1 = result.Read<string>(i, 6);
                got.faction = result.Read<uint>(i, 7);
                got.flags = result.Read<uint>(i, 8);
                got.size = result.Read<float>(i, 9);

                for (byte x = 0; x < GameObjectConst.MaxQuestItems; ++x)
                    got.questItems[x] = result.Read<uint>(i, 10 + x);

                for (byte x = 0; x < GameObjectConst.MaxGOData; ++x)
                    got.RawData[x] = result.Read<int>(i, 16 + x);

                got.unkInt32 = result.Read<int>(i, 48);
                got.AIName = result.Read<string>(i, 49);
                //got.ScriptId = result.Read<uint>(i, 50);

                switch (got.type)
                {
                    case GameObjectTypes.Door:
                        got.Door = got.RawData.ReadGameObjectData<GameObjectTemplate.door>();
                        break;
                    case GameObjectTypes.Button:
                        got.Button = got.RawData.ReadGameObjectData<GameObjectTemplate.button>();
                        break;
                    case GameObjectTypes.QuestGiver:
                        got.QuestGiver = got.RawData.ReadGameObjectData<GameObjectTemplate.questgiver>();
                        break;
                    case GameObjectTypes.Chest:
                        got.Chest = got.RawData.ReadGameObjectData<GameObjectTemplate.chest>();
                        break;
                    case GameObjectTypes.Generic:
                        got.Generic = got.RawData.ReadGameObjectData<GameObjectTemplate.generic>();
                        break;
                    case GameObjectTypes.Trap:
                        got.Trap = got.RawData.ReadGameObjectData<GameObjectTemplate.trap>();
                        break;
                    case GameObjectTypes.Chair:
                        got.Chair = got.RawData.ReadGameObjectData<GameObjectTemplate.chair>();
                        break;
                    case GameObjectTypes.SpellFocus:
                        got.SpellFocus = got.RawData.ReadGameObjectData<GameObjectTemplate.spellFocus>();
                        break;
                    case GameObjectTypes.Text:
                        got.Text = got.RawData.ReadGameObjectData<GameObjectTemplate.text>();
                        break;
                    case GameObjectTypes.Goober:
                        got.Goober = got.RawData.ReadGameObjectData<GameObjectTemplate.goober>();
                        break;
                    case GameObjectTypes.Transport:
                        got.Transport = got.RawData.ReadGameObjectData<GameObjectTemplate.transport>();
                        break;
                    case GameObjectTypes.AreaDamage:
                        got.AreaDamage = got.RawData.ReadGameObjectData<GameObjectTemplate.areadamage>();
                        break;
                    case GameObjectTypes.Camera:
                        got.Camera = got.RawData.ReadGameObjectData<GameObjectTemplate.camera>();
                        break;
                    case GameObjectTypes.MoTransport:
                        got.MoTransport = got.RawData.ReadGameObjectData<GameObjectTemplate.moTransport>();
                        break;
                    case GameObjectTypes.Ritual:
                        got.SummoningRitual = got.RawData.ReadGameObjectData<GameObjectTemplate.summoningRitual>();
                        break;
                    case GameObjectTypes.GuardPost:
                        got.GuardPost = got.RawData.ReadGameObjectData<GameObjectTemplate.guardpost>();
                        break;
                    case GameObjectTypes.SpellCaster:
                        got.SpellCaster = got.RawData.ReadGameObjectData<GameObjectTemplate.spellcaster>();
                        break;
                    case GameObjectTypes.MeetingStone:
                        got.MeetingStone = got.RawData.ReadGameObjectData<GameObjectTemplate.meetingstone>();
                        break;
                    case GameObjectTypes.FlagStand:
                        got.FlagStand = got.RawData.ReadGameObjectData<GameObjectTemplate.flagstand>();
                        break;
                    case GameObjectTypes.FishingHole:
                        got.FishingHole = got.RawData.ReadGameObjectData<GameObjectTemplate.fishinghole>();
                        break;
                    case GameObjectTypes.FlagDrop:
                        got.FlagDrop = got.RawData.ReadGameObjectData<GameObjectTemplate.flagdrop>();
                        break;
                    case GameObjectTypes.MiniGame:
                        got.MiniGame = got.RawData.ReadGameObjectData<GameObjectTemplate.miniGame>();
                        break;
                    case GameObjectTypes.CapturePoint:
                        got.CapturePoint = got.RawData.ReadGameObjectData<GameObjectTemplate.capturePoint>();
                        break;
                    case GameObjectTypes.AuraGenerator:
                        got.AuraGenerator = got.RawData.ReadGameObjectData<GameObjectTemplate.auraGenerator>();
                        break;
                    case GameObjectTypes.DungeonDifficulty:
                        got.DungeonDifficulty = got.RawData.ReadGameObjectData<GameObjectTemplate.dungeonDifficulty>();
                        break;
                    case GameObjectTypes.BarberChair:
                        got.BarberChair = got.RawData.ReadGameObjectData<GameObjectTemplate.barberChair>();
                        break;
                    case GameObjectTypes.DestructibleBuilding:
                        got.Building = got.RawData.ReadGameObjectData<GameObjectTemplate.building>();
                        break;
                    case GameObjectTypes.TrapDoor:
                        got.TrapDoor = got.RawData.ReadGameObjectData<GameObjectTemplate.trapDoor>();
                        break;
                }

                // Checks todo: fix me
                /*
                switch (got.type)
                {
                    case GameObjectTypes.Door:                      //0
                        {
                            if (got.Door.lockId)
                                CheckGOLockId(&got, got.door.lockId, 1);
                            CheckGONoDamageImmuneId(&got, got.door.noDamageImmune, 3);
                            break;
                        }
                    case GAMEOBJECT_TYPE_BUTTON:                    //1
                        {
                            if (got.button.lockId)
                                CheckGOLockId(&got, got.button.lockId, 1);
                            CheckGONoDamageImmuneId(&got, got.button.noDamageImmune, 4);
                            break;
                        }
                    case GAMEOBJECT_TYPE_QUESTGIVER:                //2
                        {
                            if (got.questgiver.lockId)
                                CheckGOLockId(&got, got.questgiver.lockId, 0);
                            CheckGONoDamageImmuneId(&got, got.questgiver.noDamageImmune, 5);
                            break;
                        }
                    case GAMEOBJECT_TYPE_CHEST:                     //3
                        {
                            if (got.chest.lockId)
                                CheckGOLockId(&got, got.chest.lockId, 0);

                            CheckGOConsumable(&got, got.chest.consumable, 3);

                            if (got.chest.linkedTrapId)              // linked trap
                                CheckGOLinkedTrapId(&got, got.chest.linkedTrapId, 7);
                            break;
                        }
                    case GAMEOBJECT_TYPE_TRAP:                      //6
                        {
                            if (got.trap.lockId)
                                CheckGOLockId(&got, got.trap.lockId, 0);
                            break;
                        }
                    case GAMEOBJECT_TYPE_CHAIR:                     //7
                        CheckAndFixGOChairHeightId(&got, got.chair.height, 1);
                        break;
                    case GAMEOBJECT_TYPE_SPELL_FOCUS:               //8
                        {
                            if (got.spellFocus.focusId)
                            {
                                if (!sSpellFocusObjectStore.LookupEntry(got.spellFocus.focusId))
                                    sLog->outError(LOG_FILTER_SQL, "GameObject (Entry: %u GoType: %u) have data0=%u but SpellFocus (Id: %u) not exist.",
                                    entry, got.type, got.spellFocus.focusId, got.spellFocus.focusId);
                            }

                            if (got.spellFocus.linkedTrapId)        // linked trap
                                CheckGOLinkedTrapId(&got, got.spellFocus.linkedTrapId, 2);
                            break;
                        }
                    case GAMEOBJECT_TYPE_GOOBER:                    //10
                        {
                            if (got.goober.lockId)
                                CheckGOLockId(&got, got.goober.lockId, 0);

                            CheckGOConsumable(&got, got.goober.consumable, 3);

                            if (got.goober.pageId)                  // pageId
                            {
                                if (!GetPageText(got.goober.pageId))
                                    sLog->outError(LOG_FILTER_SQL, "GameObject (Entry: %u GoType: %u) have data7=%u but PageText (Entry %u) not exist.",
                                    entry, got.type, got.goober.pageId, got.goober.pageId);
                            }
                            CheckGONoDamageImmuneId(&got, got.goober.noDamageImmune, 11);
                            if (got.goober.linkedTrapId)            // linked trap
                                CheckGOLinkedTrapId(&got, got.goober.linkedTrapId, 12);
                            break;
                        }
                    case GAMEOBJECT_TYPE_AREADAMAGE:                //12
                        {
                            if (got.areadamage.lockId)
                                CheckGOLockId(&got, got.areadamage.lockId, 0);
                            break;
                        }
                    case GAMEOBJECT_TYPE_CAMERA:                    //13
                        {
                            if (got.camera.lockId)
                                CheckGOLockId(&got, got.camera.lockId, 0);
                            break;
                        }
                    case GAMEOBJECT_TYPE_MO_TRANSPORT:              //15
                        {
                            if (got.moTransport.taxiPathId)
                            {
                                if (got.moTransport.taxiPathId >= sTaxiPathNodesByPath.size() || sTaxiPathNodesByPath[got.moTransport.taxiPathId].empty())
                                    sLog->outError(LOG_FILTER_SQL, "GameObject (Entry: %u GoType: %u) have data0=%u but TaxiPath (Id: %u) not exist.",
                                    entry, got.type, got.moTransport.taxiPathId, got.moTransport.taxiPathId);
                            }
                            break;
                        }
                    case GAMEOBJECT_TYPE_SUMMONING_RITUAL:          //18
                        break;
                    case GAMEOBJECT_TYPE_SPELLCASTER:               //22
                        {
                            // always must have spell
                            CheckGOSpellId(&got, got.spellcaster.spellId, 0);
                            break;
                        }
                    case GAMEOBJECT_TYPE_FLAGSTAND:                 //24
                        {
                            if (got.flagstand.lockId)
                                CheckGOLockId(&got, got.flagstand.lockId, 0);
                            CheckGONoDamageImmuneId(&got, got.flagstand.noDamageImmune, 5);
                            break;
                        }
                    case GAMEOBJECT_TYPE_FISHINGHOLE:               //25
                        {
                            if (got.fishinghole.lockId)
                                CheckGOLockId(&got, got.fishinghole.lockId, 4);
                            break;
                        }
                    case GAMEOBJECT_TYPE_FLAGDROP:                  //26
                        {
                            if (got.flagdrop.lockId)
                                CheckGOLockId(&got, got.flagdrop.lockId, 0);
                            CheckGONoDamageImmuneId(&got, got.flagdrop.noDamageImmune, 3);
                            break;
                        }
                    case GAMEOBJECT_TYPE_BARBER_CHAIR:              //32
                        CheckAndFixGOChairHeightId(&got, got.barberChair.chairheight, 0);
                        break;
                }
                */
                gameObjectTemplateStorage.Add(entry, got);
                ++count;
            }
            Log.outInfo("Loaded {0} game object templates in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        public void LoadGameobjects()
        {
            var time = Time.getMSTime();
            //                                              0                1   2    3           4           5           6
            SQLResult result = DB.World.Select("SELECT gameobject.guid, id, map, position_x, position_y, position_z, orientation, " +
                //   7          8          9          10         11             12            13     14         15         16          17
                "rotation0, rotation1, rotation2, rotation3, spawntimesecs, animprogress, state, spawnMask, phaseMask, eventEntry, pool_entry " +
                "FROM gameobject LEFT OUTER JOIN game_event_gameobject ON gameobject.guid = game_event_gameobject.guid " +
                "LEFT OUTER JOIN pool_gameobject ON gameobject.guid = pool_gameobject.guid");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 gameobjects. DB table `gameobject` is empty.");

                return;
            }
            uint count = 0;

            // build single time for check spawnmask
            Hashtable spawnMasks = new Hashtable();
            foreach (var map in DBCStorage.MapStorage.Values)
                for (int k = 0; k < SharedConst.MaxDifficulty; ++k)
                    if (GetMapDifficultyData(map.MapID, (Difficulty)k) != null)
                        spawnMasks[map.MapID] = (uint)(spawnMasks[map.MapID] == null ? (uint)(1 << k) : (uint)spawnMasks[map.MapID] | (uint)(1 << k));

            for (var i = 0; i < result.Count; i++)
            {
                uint guid = result.Read<uint>(i, 0);
                uint entry = result.Read<uint>(i, 1);

                GameObjectTemplate gInfo = GetGameObjectTemplate(entry);
                if (gInfo == null)
                {
                    Log.outError("Table `gameobject` has gameobject (GUID: {0}) with non existing gameobject entry {1}, skipped.", guid, entry);
                    continue;
                }

                if (gInfo.displayId == 0)
                {
                    switch (gInfo.type)
                    {
                        case GameObjectTypes.Trap:
                        case GameObjectTypes.SpellFocus:
                            break;
                        default:
                            Log.outError("Gameobject (GUID: {0} Entry {1} GoType: {2}) doesn't have a displayId ({3}), not loaded.", guid, entry, gInfo.type, gInfo.displayId);
                            break;
                    }
                }

                //if (gInfo.displayId != 0 && !sGameObjectDisplayInfoStore.LookupEntry(gInfo.displayId))
                {
                    //Log.outError("Gameobject (GUID: {0} Entry {1} GoType: {2}) has an invalid displayId ({3}), not loaded.", guid, entry, gInfo.type, gInfo.displayId);
                    //continue;
                }

                GameObjectData data = new GameObjectData();
                data.id = entry;
                data.mapid = result.Read<ushort>(i, 2);
                data.posX = result.Read<float>(i, 3);
                data.posY = result.Read<float>(i, 4);
                data.posZ = result.Read<float>(i, 5);
                data.orientation = result.Read<float>(i, 6);
                data.rotation0 = result.Read<float>(i, 7);
                data.rotation1 = result.Read<float>(i, 8);
                data.rotation2 = result.Read<float>(i, 9);
                data.rotation3 = result.Read<float>(i, 10);
                data.spawntimesecs = result.Read<int>(i, 11);

                MapEntry mapEntry = DBCStorage.MapStorage.LookupByKey(data.mapid);
                if (mapEntry == null)
                {
                    Log.outError("Table `gameobject` has gameobject (GUID: {0} Entry: {1}) spawned on a non-existed map (Id: {2}), skip", guid, data.id, data.mapid);
                    continue;
                }

                if (data.spawntimesecs == 0 && gInfo.IsDespawnAtAction())
                {
                    Log.outError("Table `gameobject` has gameobject (GUID: {0} Entry: {1}) with `spawntimesecs` (0) value, but the gameobejct is marked as despawnable at action.", guid, data.id);
                }

                data.animprogress = result.Read<uint>(i, 12);
                data.artKit = 0;

                uint go_state = result.Read<uint>(i, 13);
                if (go_state >= (uint)GameObjectState.Max)
                {
                    Log.outError("Table `gameobject` has gameobject (GUID: {0} Entry: {1}) with invalid `state` ({2}) value, skip", guid, data.id, go_state);
                    continue;
                }
                data.go_state = (GameObjectState)go_state;

                data.spawnMask = result.Read<byte>(i, 14);

                //if (!Convert.ToBoolean(data.spawnMask & ~(Convert.ToUInt32(spawnMasks[data.mapid]))))
                //Log.outError("Table `gameobject` has gameobject (GUID: {0} Entry: {1}) that has wrong spawn mask {2} including not supported difficulty modes for map (Id: {3}), skip",
                //guid, data.id, data.spawnMask, data.mapid);

                data.phaseMask = result.Read<ushort>(i, 15);
                //short gameEvent     = result.Read<uint>(i, 16);
                //uint PoolId        = result.Read<uint>(i, 17);

                if (data.rotation2 < -1.0f || data.rotation2 > 1.0f)
                {
                    Log.outError("Table `gameobject` has gameobject (GUID: {0} Entry: {1}) with invalid rotation2 ({2}) value, skip", guid, data.id, data.rotation2);
                    continue;
                }

                if (data.rotation3 < -1.0f || data.rotation3 > 1.0f)
                {
                    Log.outError("Table `gameobject` has gameobject (GUID: {0} Entry: {1}) with invalid rotation3 ({2}) value, skip", guid, data.id, data.rotation3);
                    continue;
                }

                if (!Cypher.MapMgr.IsValidMapCoord(data.mapid, data.posX, data.posY, data.posZ, data.orientation))
                {
                    Log.outError("Table `gameobject` has gameobject (GUID: {0} Entry: {1}) with invalid coordinates, skip", guid, data.id);
                    continue;
                }

                if (data.phaseMask == 0)
                {
                    Log.outError("Table `gameobject` has gameobject (GUID: {0} Entry: {1}) with `phaseMask`=0 (not visible for anyone), set to 1.", guid, data.id);
                    data.phaseMask = 1;
                }

                //if (gameEvent == 0 && PoolId == 0)                      // if not this is to be managed by GameEvent System or Pool system
                AddGameObjectToGrid(guid, data);

                gameObjectDataStorage.Add(guid, data);
                count++;
            }
            Log.outInfo("Loaded {0} gameobjects in {1} ms", count, Time.getMSTimeDiffNow(time));
        }
        public void AddGameObjectToGrid(uint guid, GameObjectData data)
        {
            byte mask = data.spawnMask;
            for (byte i = 0; mask != 0; i++, mask >>= 1)
            {
                if (Convert.ToBoolean(mask & 1))
                {
                    CellCoord cellCoord = GridDefines.ComputeCellCoord(data.posX, data.posY);
                    var cell_guids = GetOrCreateCellObjectGuids(data.mapid, i, cellCoord, true);
                    cell_guids.gameobjects.Add(guid);
                }
            }
        }

        //Items
        public void LoadItemTemplates()
        {
            var time = Time.getMSTime();
            uint sparseCount = 0;
            uint dbCount = 0;

            foreach (var db2Data in DB2Storage.ItemEntryStorage.Values)
            {
                var sparse = DB2Storage.ItemSparseStorage.LookupByKey(db2Data.Id);
                if (sparse == null)
                    continue;

                var itemTemplate = new ItemTemplate();

                itemTemplate.ItemId = sparse.Id;
                itemTemplate.Class = (ItemClass)db2Data.Class;
                itemTemplate.SubClass = db2Data.SubClass;
                itemTemplate.SoundOverrideSubclass = db2Data.SoundOverrideSubclass;
                itemTemplate.Name1 = sparse.Name;
                itemTemplate.DisplayInfoID = db2Data.DisplayId;
                itemTemplate.Quality = (ItemQuality)sparse.Quality;
                itemTemplate.Flags = (ItemFlags)sparse.Flags;
                itemTemplate.Flags2 = (ItemFlags2)sparse.Flags2;
                itemTemplate.Unk430_1 = sparse.Unk430_1;
                itemTemplate.Unk430_2 = sparse.Unk430_2;
                itemTemplate.BuyCount = Math.Max(sparse.BuyCount, 1u);
                itemTemplate.BuyPrice = sparse.BuyPrice;
                itemTemplate.SellPrice = sparse.SellPrice;
                itemTemplate.inventoryType = (InventoryType)db2Data.inventoryType;
                itemTemplate.AllowableClass = sparse.AllowableClass;
                itemTemplate.AllowableRace = sparse.AllowableRace;
                itemTemplate.ItemLevel = sparse.ItemLevel;
                itemTemplate.RequiredLevel = sparse.RequiredLevel;
                itemTemplate.RequiredSkill = sparse.RequiredSkill;
                itemTemplate.RequiredSkillRank = sparse.RequiredSkillRank;
                itemTemplate.RequiredSpell = sparse.RequiredSpell;
                itemTemplate.RequiredHonorRank = sparse.RequiredHonorRank;
                itemTemplate.RequiredCityRank = sparse.RequiredCityRank;
                itemTemplate.RequiredReputationFaction = sparse.RequiredReputationFaction;
                itemTemplate.RequiredReputationRank = sparse.RequiredReputationRank;
                itemTemplate.MaxCount = sparse.MaxCount;
                itemTemplate.Stackable = sparse.Stackable;
                itemTemplate.ContainerSlots = sparse.ContainerSlots;
                for (var i = 0; i < ItemConst.MaxStats; ++i)
                {
                    itemTemplate.ItemStat[i].ItemStatType = sparse.ItemStatType[i];
                    itemTemplate.ItemStat[i].ItemStatValue = sparse.ItemStatValue[i];
                    itemTemplate.ItemStat[i].ItemStatUnk1 = sparse.ItemStatUnk1[i];
                    itemTemplate.ItemStat[i].ItemStatUnk2 = sparse.ItemStatUnk2[i];
                }

                itemTemplate.ScalingStatDistribution = sparse.ScalingStatDistribution;
                float dmgmin, dmgmax, dps;
                // cache item damage
                FillItemDamageFields(sparse.ItemLevel, db2Data.Class, db2Data.SubClass, sparse.Quality, sparse.Delay, sparse.StatScalingFactor, sparse.inventoryType,
                    sparse.Flags2, out dmgmin, out dmgmax, out dps);

                itemTemplate.DamageMin = dmgmin;
                itemTemplate.DamageMax = dmgmax;
                itemTemplate.DPS = dps;

                itemTemplate.DamageType = sparse.DamageType;
                itemTemplate.Armor = FillItemArmor(sparse.ItemLevel, db2Data.Class, db2Data.SubClass, sparse.Quality, sparse.inventoryType);
                itemTemplate.Delay = sparse.Delay;
                itemTemplate.RangedModRange = sparse.RangedModRange;
                for (var i = 0; i < ItemConst.MaxSpells; ++i)
                {
                    itemTemplate.Spells[i].SpellId = sparse.SpellId[i];
                    itemTemplate.Spells[i].SpellTrigger = sparse.SpellTrigger[i];
                    itemTemplate.Spells[i].SpellCharges = sparse.SpellCharges[i];
                    itemTemplate.Spells[i].SpellCooldown = sparse.SpellCooldown[i];
                    itemTemplate.Spells[i].SpellCategory = sparse.SpellCategory[i];
                    itemTemplate.Spells[i].SpellCategoryCooldown = sparse.SpellCategoryCooldown[i];
                }

                itemTemplate.SpellPPMRate = 0.0f;
                itemTemplate.Bonding = (ItemBondingType)sparse.Bonding;
                itemTemplate.Description = sparse.Description;
                itemTemplate.PageText = sparse.PageText;
                itemTemplate.LanguageID = sparse.LanguageID;
                itemTemplate.PageMaterial = sparse.PageMaterial;
                itemTemplate.StartQuest = sparse.StartQuest;
                itemTemplate.LockID = sparse.LockID;
                itemTemplate.Material = sparse.Material;
                itemTemplate.Sheath = sparse.Sheath;
                itemTemplate.RandomProperty = sparse.RandomProperty;
                itemTemplate.RandomSuffix = sparse.RandomSuffix;
                itemTemplate.ItemSet = sparse.ItemSet;
                itemTemplate.MaxDurability = FillMaxDurability(db2Data.Class, db2Data.SubClass, sparse.inventoryType, sparse.Quality, sparse.ItemLevel);
                itemTemplate.Area = sparse.Area;
                itemTemplate.Map = sparse.Map;
                itemTemplate.BagFamily = (BagFamilyMask)sparse.BagFamily;
                itemTemplate.TotemCategory = sparse.TotemCategory;
                for (var i = 0; i < ItemConst.MaxSockets; ++i)
                {
                    itemTemplate.Socket[i].Color = sparse.Color[i];
                    itemTemplate.Socket[i].Content = sparse.Content[i];
                }

                itemTemplate.socketBonus = sparse.SocketBonus;
                itemTemplate.GemProperties = sparse.GemProperties;

                //uint disenchantID, requiredDisenchantSkill = 0;
                //FillDisenchantFields(out disenchantID, out requiredDisenchantSkill, itemTemplate);

                //itemTemplate.DisenchantID = disenchantID;
                //itemTemplate.RequiredDisenchantSkill = requiredDisenchantSkill;

                itemTemplate.ArmorDamageModifier = sparse.ArmorDamageModifier;
                itemTemplate.Duration = sparse.Duration;
                itemTemplate.ItemLimitCategory = sparse.ItemLimitCategory;
                itemTemplate.HolidayId = sparse.HolidayId;
                itemTemplate.StatScalingFactor = sparse.StatScalingFactor;
                itemTemplate.CurrencySubstitutionId = sparse.CurrencySubstitutionId;
                itemTemplate.CurrencySubstitutionCount = sparse.CurrencySubstitutionCount;
                itemTemplate.ScriptId = 0;
                itemTemplate.FoodType = 0;
                itemTemplate.MinMoneyLoot = 0;
                itemTemplate.MaxMoneyLoot = 0;
                ++sparseCount;
                ItemTemplateStorage.Add(sparse.Id, itemTemplate);
            }

            // Load missing items from item_template AND overwrite data from Item-sparse.db2 (item_template is supposed to contain Item-sparse.adb data)
            SQLResult result = DB.World.Select("SELECT entry, Class, SubClass, SoundOverrideSubclass, Name, DisplayId, Quality, Flags, FlagsExtra, Unk430_1, Unk430_2, BuyCount, BuyPrice, SellPrice, " +
                "InventoryType, AllowableClass, AllowableRace, ItemLevel, RequiredLevel, RequiredSkill, RequiredSkillRank, RequiredSpell, " +
                "RequiredHonorRank, RequiredCityRank, RequiredReputationFaction, RequiredReputationRank, MaxCount, Stackable, ContainerSlots, " +
                "stat_type1, stat_value1, stat_unk1_1, stat_unk2_1, stat_type2, stat_value2, stat_unk1_2, stat_unk2_2, stat_type3, stat_value3, stat_unk1_3, stat_unk2_3, " +
                "stat_type4, stat_value4, stat_unk1_4, stat_unk2_4, stat_type5, stat_value5, stat_unk1_5, stat_unk2_5, stat_type6, stat_value6, stat_unk1_6, stat_unk2_6, " +
                "stat_type7, stat_value7, stat_unk1_7, stat_unk2_7, stat_type8, stat_value8, stat_unk1_8, stat_unk2_8, stat_type9, stat_value9, stat_unk1_9, stat_unk2_9, " +
                "stat_type10, stat_value10, stat_unk1_10, stat_unk2_10, ScalingStatDistribution, DamageType, Delay, RangedModRange, " +
                "spellid_1, spelltrigger_1, spellcharges_1, spellcooldown_1, spellcategory_1, spellcategorycooldown_1, " +
                "spellid_2, spelltrigger_2, spellcharges_2, spellcooldown_2, spellcategory_2, spellcategorycooldown_2, " +
                "spellid_3, spelltrigger_3, spellcharges_3, spellcooldown_3, spellcategory_3, spellcategorycooldown_3, " +
                "spellid_4, spelltrigger_4, spellcharges_4, spellcooldown_4, spellcategory_4, spellcategorycooldown_4, " +
                "spellid_5, spelltrigger_5, spellcharges_5, spellcooldown_5, spellcategory_5, spellcategorycooldown_5, " +
                "Bonding, Description, PageText, LanguageID, PageMaterial, StartQuest, LockID, Material, Sheath, RandomProperty, RandomSuffix, ItemSet, Area, Map, BagFamily, " +
                "TotemCategory, SocketColor_1, SocketContent_1, SocketColor_2, SocketContent_2, SocketColor_3, SocketContent_3, SocketBonus, GemProperties, ArmorDamageModifier, " +
                "Duration, ItemLimitCategory, HolidayId, StatScalingFactor, CurrencySubstitutionId, CurrencySubstitutionCount FROM item_template");

            if (result.Count != 0)
            {
                for (var i = 0; i < result.Count; i++)
                {
                    uint itemId = result.Read<uint>(i, 0);
                    if (ItemTemplateStorage.LookupByKey(itemId) != null)
                    {
                        --sparseCount;
                        ItemTemplateStorage.Remove(itemId);
                    }
                    var itemTemplate = new ItemTemplate();

                    itemTemplate.ItemId = itemId;
                    itemTemplate.Class = (ItemClass)result.Read<uint>(i, 1);
                    itemTemplate.SubClass = result.Read<uint>(i, 2);
                    itemTemplate.SoundOverrideSubclass = result.Read<int>(i, 3);
                    itemTemplate.Name1 = result.Read<string>(i, 4);
                    itemTemplate.DisplayInfoID = result.Read<uint>(i, 5);
                    itemTemplate.Quality = (ItemQuality)result.Read<uint>(i, 6);
                    itemTemplate.Flags = (ItemFlags)result.Read<long>(i, 7);
                    itemTemplate.Flags2 = (ItemFlags2)result.Read<uint>(i, 8);
                    itemTemplate.Unk430_1 = result.Read<float>(i, 9);
                    itemTemplate.Unk430_2 = result.Read<float>(i, 10);
                    itemTemplate.BuyCount = result.Read<uint>(i, 11);
                    itemTemplate.BuyPrice = result.Read<uint>(i, 12);
                    itemTemplate.SellPrice = result.Read<uint>(i, 13);

                    itemTemplate.inventoryType = (InventoryType)result.Read<uint>(i, 14);
                    itemTemplate.AllowableClass = result.Read<int>(i, 15);
                    itemTemplate.AllowableRace = result.Read<int>(i, 16);
                    itemTemplate.ItemLevel = result.Read<uint>(i, 17);
                    itemTemplate.RequiredLevel = result.Read<int>(i, 18);
                    itemTemplate.RequiredSkill = result.Read<uint>(i, 19);
                    itemTemplate.RequiredSkillRank = result.Read<uint>(i, 20);
                    itemTemplate.RequiredSpell = result.Read<uint>(i, 21);
                    itemTemplate.RequiredHonorRank = result.Read<uint>(i, 22);
                    itemTemplate.RequiredCityRank = result.Read<uint>(i, 23);
                    itemTemplate.RequiredReputationFaction = result.Read<uint>(i, 24);
                    itemTemplate.RequiredReputationRank = result.Read<uint>(i, 25);
                    itemTemplate.MaxCount = result.Read<uint>(i, 26);
                    itemTemplate.Stackable = result.Read<uint>(i, 27);
                    itemTemplate.ContainerSlots = result.Read<uint>(i, 28);
                    for (var x = 0; x < ItemConst.MaxStats; ++x)
                    {
                        itemTemplate.ItemStat[x].ItemStatType = result.Read<int>(i, 29 + x * 4 + 0);
                        itemTemplate.ItemStat[x].ItemStatValue = result.Read<int>(i, 29 + x * 4 + 1);
                        itemTemplate.ItemStat[x].ItemStatUnk1 = result.Read<int>(i, 29 + x * 4 + 2);
                        itemTemplate.ItemStat[x].ItemStatUnk2 = result.Read<int>(i, 29 + x * 4 + 3);
                    }

                    itemTemplate.ScalingStatDistribution = result.Read<uint>(i, 69);

                    // cache item damage
                    float dmgmin, dmgmax, dps;
                    FillItemDamageFields(itemTemplate.ItemLevel, (uint)itemTemplate.Class, itemTemplate.SubClass, (uint)itemTemplate.Quality, result.Read<uint>(i, 71), result.Read<float>(i, 131),
                        (uint)itemTemplate.inventoryType, (uint)itemTemplate.Flags2, out dmgmin, out dmgmax, out dps);

                    itemTemplate.DamageMin = dmgmin;
                    itemTemplate.DamageMax = dmgmax;
                    itemTemplate.DPS = dps;

                    itemTemplate.DamageType = result.Read<uint>(i, 70);
                    itemTemplate.Armor = FillItemArmor(itemTemplate.ItemLevel, (uint)itemTemplate.Class, itemTemplate.SubClass, (uint)itemTemplate.Quality, (uint)itemTemplate.inventoryType);

                    itemTemplate.Delay = result.Read<uint>(i, 71);
                    itemTemplate.RangedModRange = result.Read<float>(i, 72);
                    for (var x = 0; x < ItemConst.MaxSpells; ++x)
                    {
                        itemTemplate.Spells[x].SpellId = result.Read<int>(i, 73 + 6 * x + 0);
                        itemTemplate.Spells[x].SpellTrigger = result.Read<int>(i, 73 + 6 * x + 1);
                        itemTemplate.Spells[x].SpellCharges = result.Read<int>(i, 73 + 6 * x + 2);
                        itemTemplate.Spells[x].SpellCooldown = result.Read<int>(i, 73 + 6 * x + 3);
                        itemTemplate.Spells[x].SpellCategory = result.Read<int>(i, 73 + 6 * x + 4);
                        itemTemplate.Spells[x].SpellCategoryCooldown = result.Read<int>(i, 73 + 6 * x + 5);
                    }

                    itemTemplate.SpellPPMRate = 0.0f;
                    itemTemplate.Bonding = (ItemBondingType)result.Read<uint>(i, 103);
                    itemTemplate.Description = result.Read<string>(i, 104);
                    itemTemplate.PageText = result.Read<uint>(i, 105);
                    itemTemplate.LanguageID = result.Read<uint>(i, 106);
                    itemTemplate.PageMaterial = result.Read<uint>(i, 107);
                    itemTemplate.StartQuest = result.Read<uint>(i, 108);
                    itemTemplate.LockID = result.Read<uint>(i, 109);
                    itemTemplate.Material = result.Read<int>(i, 110);
                    itemTemplate.Sheath = result.Read<uint>(i, 111);
                    itemTemplate.RandomProperty = result.Read<int>(i, 112);
                    itemTemplate.RandomSuffix = result.Read<uint>(i, 113);
                    itemTemplate.ItemSet = result.Read<uint>(i, 114);
                    itemTemplate.MaxDurability = FillMaxDurability((uint)itemTemplate.Class, itemTemplate.SubClass, (uint)itemTemplate.inventoryType, (uint)itemTemplate.Quality, itemTemplate.ItemLevel);

                    itemTemplate.Area = result.Read<uint>(i, 115);
                    itemTemplate.Map = result.Read<uint>(i, 116);
                    itemTemplate.BagFamily = (BagFamilyMask)result.Read<uint>(i, 117);
                    itemTemplate.TotemCategory = result.Read<uint>(i, 118);
                    for (var x = 0; x < ItemConst.MaxSockets; ++x)
                    {
                        itemTemplate.Socket[x].Color = result.Read<uint>(i, 119 + x * 2);
                        itemTemplate.Socket[x].Content = result.Read<uint>(i, 119 + x * 2 + 1);
                    }

                    itemTemplate.socketBonus = result.Read<int>(i, 125);
                    itemTemplate.GemProperties = result.Read<uint>(i, 126);

                    //uint disenchantID, requiredDisenchantSkill = 0;
                    //FillDisenchantFields(out disenchantID, out requiredDisenchantSkill, itemTemplate);

                    //itemTemplate.DisenchantID = disenchantID;
                    //itemTemplate.RequiredDisenchantSkill = requiredDisenchantSkill;

                    itemTemplate.ArmorDamageModifier = result.Read<float>(i, 127);
                    itemTemplate.Duration = result.Read<uint>(i, 128);
                    itemTemplate.ItemLimitCategory = result.Read<uint>(i, 129);
                    itemTemplate.HolidayId = result.Read<uint>(i, 130);
                    itemTemplate.StatScalingFactor = result.Read<float>(i, 131);
                    itemTemplate.CurrencySubstitutionId = result.Read<int>(i, 132);
                    itemTemplate.CurrencySubstitutionCount = result.Read<int>(i, 133);
                    itemTemplate.ScriptId = 0;
                    itemTemplate.FoodType = 0;
                    itemTemplate.MinMoneyLoot = 0;
                    itemTemplate.MaxMoneyLoot = 0;

                    ItemTemplateStorage.Add(itemId, itemTemplate);
                    ++dbCount;
                }
            }
            Log.outInfo("Loaded {0} Item-sparse.db2 and {1} database Items in {2} ms", sparseCount, dbCount, Time.getMSTimeDiffNow(time));
        }
        void FillItemDamageFields(uint itemLevel, uint itemClass, uint itemSubClass, uint quality, uint delay, float statScalingFactor, uint inventoryType, uint flags2,
            out float minDamage, out float maxDamage, out float dps)
        {
            minDamage = maxDamage = dps = 0.0f;
            if (itemClass != (uint)ItemClass.Weapon || quality > (uint)ItemQuality.Artifact)
                return;

            Dictionary<uint, ItemDamageEntry> store = null;
            if ((int)inventoryType > 0xD + 13)
                return;

            switch ((InventoryType)inventoryType)
            {
                case InventoryType.Ammo:
                    store = DBCStorage.ItemDamageAmmoStorage;
                    break;
                case InventoryType.Weapon2Hand:
                    if (Convert.ToBoolean(flags2 & (uint)ItemFlags2.CasterWeapon))
                        store = DBCStorage.ItemDamageTwoHandCasterStorage;
                    else
                        store = DBCStorage.ItemDamageTwoHandStorage;
                    break;
                case InventoryType.Ranged:
                case InventoryType.Thrown:
                case InventoryType.RangedRight:
                    switch ((ItemSubClassWeapon)itemSubClass)
                    {
                        case ItemSubClassWeapon.Wand:
                            store = DBCStorage.ItemDamageWandStorage;
                            break;
                        case ItemSubClassWeapon.Thrown:
                            store = DBCStorage.ItemDamageThrownStorage;
                            break;
                        case ItemSubClassWeapon.Bow:
                        case ItemSubClassWeapon.Gun:
                        case ItemSubClassWeapon.Crossbow:
                            store = DBCStorage.ItemDamageRangedStorage;
                            break;
                        default:
                            return;
                    }
                    break;
                case InventoryType.Weapon:
                case InventoryType.WeaponMainhand:
                case InventoryType.WeaponOffhand:
                    if (Convert.ToBoolean(flags2 & (uint)ItemFlags2.CasterWeapon))
                        store = DBCStorage.ItemDamageOneHandCasterStorage;
                    else
                        store = DBCStorage.ItemDamageOneHandStorage;
                    break;
                default:
                    return;
            }

            if (store == null)
                return;

            var damageInfo = store.LookupByKey(itemLevel);
            if (damageInfo == null)
                return;

            dps = damageInfo.DPS[quality];
            float avgDamage = dps * delay * 0.001f;
            minDamage = (statScalingFactor * -0.5f + 1.0f) * avgDamage;
            maxDamage = (float)Math.Floor(avgDamage * (statScalingFactor * 0.5f + 1.0f) + 0.5f);
        }
        uint FillItemArmor(uint itemlevel, uint itemClass, uint itemSubclass, uint quality, uint inventoryType)
        {
            if (quality > (int)ItemQuality.Artifact)
                return 0;

            // all items but shields
            if (itemClass != (uint)ItemClass.Armor || itemSubclass != (uint)ItemSubClassArmor.Shield)
            {
                ItemArmorQualityEntry armorQuality = DBCStorage.ItemArmorQualityStorage.LookupByKey(itemlevel);
                ItemArmorTotalEntry armorTotal = DBCStorage.ItemArmorTotalStorage.LookupByKey(itemlevel);
                if (armorQuality == null || armorTotal == null)
                    return 0;

                if (inventoryType == (uint)InventoryType.Robe)
                    inventoryType = (uint)InventoryType.Chest;

                //ArmorLocationEntry location = DBCStorage.ArmorLocationStorage.LookupByKey((uint)inventoryType);
                //if (location.InventoryType == 0)
                //return 0;

                if (itemSubclass < (uint)ItemSubClassArmor.Cloth || itemSubclass > (uint)ItemSubClassArmor.Plate)
                    return 0;

                //return (uint)(armorQuality.Value[quality] * armorTotal.Value[itemSubclass - 1] * location.Value[itemSubclass - 1] + 0.5f);
                return 0;
            }

            // shields
            ItemArmorShieldEntry shield = DBCStorage.ItemArmorShieldStorage.LookupByKey(itemlevel);
            if (shield == null)
                return 0;

            return (uint)(shield.Value[quality] + 0.5f);
        }
        uint FillMaxDurability(uint itemClass, uint itemSubClass, uint inventoryType, uint quality, uint itemLevel)
        {
            if (itemClass != (uint)ItemClass.Armor && itemClass != (uint)ItemClass.Weapon)
                return 0;

            var qualityMultipliers = new float[]
            {
                1.0f, 1.0f, 1.0f, 1.17f, 1.37f, 1.68f, 0.0f, 0.0f
            };

            var armorMultipliers = new float[]
            {
                0.00f, // INVTYPE_NON_EQUIP
                0.59f, // INVTYPE_HEAD
                0.00f, // INVTYPE_NECK
                0.59f, // INVTYPE_SHOULDERS
                0.00f, // INVTYPE_BODY
                1.00f, // INVTYPE_CHEST
                0.35f, // INVTYPE_WAIST
                0.75f, // INVTYPE_LEGS
                0.49f, // INVTYPE_FEET
                0.35f, // INVTYPE_WRISTS
                0.35f, // INVTYPE_HANDS
                0.00f, // INVTYPE_FINGER
                0.00f, // INVTYPE_TRINKET
                0.00f, // INVTYPE_WEAPON
                1.00f, // INVTYPE_SHIELD
                0.00f, // INVTYPE_RANGED
                0.00f, // INVTYPE_CLOAK
                0.00f, // INVTYPE_2HWEAPON
                0.00f, // INVTYPE_BAG
                0.00f, // INVTYPE_TABARD
                1.00f, // INVTYPE_ROBE
                0.00f, // INVTYPE_WEAPONMAINHAND
                0.00f, // INVTYPE_WEAPONOFFHAND
                0.00f, // INVTYPE_HOLDABLE
                0.00f, // INVTYPE_AMMO
                0.00f, // INVTYPE_THROWN
                0.00f, // INVTYPE_RANGEDRIGHT
                0.00f, // INVTYPE_QUIVER
                0.00f, // INVTYPE_RELIC
            };

            var weaponMultipliers = new float[]
            {
                0.89f, // ITEM_SUBCLASS_WEAPON_AXE
                1.03f, // ITEM_SUBCLASS_WEAPON_AXE2
                0.77f, // ITEM_SUBCLASS_WEAPON_BOW
                0.77f, // ITEM_SUBCLASS_WEAPON_GUN
                0.89f, // ITEM_SUBCLASS_WEAPON_MACE
                1.03f, // ITEM_SUBCLASS_WEAPON_MACE2
                1.03f, // ITEM_SUBCLASS_WEAPON_POLEARM
                0.89f, // ITEM_SUBCLASS_WEAPON_SWORD
                1.03f, // ITEM_SUBCLASS_WEAPON_SWORD2
                0.00f, // ITEM_SUBCLASS_WEAPON_Obsolete
                1.03f, // ITEM_SUBCLASS_WEAPON_STAFF
                0.00f, // ITEM_SUBCLASS_WEAPON_EXOTIC
                0.00f, // ITEM_SUBCLASS_WEAPON_EXOTIC2
                0.64f, // ITEM_SUBCLASS_WEAPON_FIST_WEAPON
                0.00f, // ITEM_SUBCLASS_WEAPON_MISCELLANEOUS
                0.64f, // ITEM_SUBCLASS_WEAPON_DAGGER
                0.64f, // ITEM_SUBCLASS_WEAPON_THROWN
                0.00f, // ITEM_SUBCLASS_WEAPON_SPEAR
                0.77f, // ITEM_SUBCLASS_WEAPON_CROSSBOW
                0.64f, // ITEM_SUBCLASS_WEAPON_WAND
                0.64f, // ITEM_SUBCLASS_WEAPON_FISHING_POLE
            };

            float levelPenalty = 1.0f;
            if (itemLevel <= 28)
                levelPenalty = 0.966f - (float)(28u - itemLevel) / 54.0f;

            if (itemClass == (uint)ItemClass.Armor)
            {
                if (inventoryType > (uint)InventoryType.Robe)
                    return 0;

                return 5 * (uint)(23.0f * qualityMultipliers[quality] * armorMultipliers[inventoryType] * levelPenalty + 0.5f);
            }

            return 5 * (uint)(17.0f * qualityMultipliers[quality] * weaponMultipliers[itemSubClass] * levelPenalty + 0.5f);
        }
        //todo: This is soooooooo Slow!!!!!!!!!!!!!!!!!!!!!!!!!  Fix me
        void FillDisenchantFields(out uint disenchantID, out uint requiredDisenchantSkill, ItemTemplate itemTemplate)
        {
            disenchantID = 0;
            requiredDisenchantSkill = 0;
            if (Convert.ToBoolean(itemTemplate.Flags & (ItemFlags.Conjured | ItemFlags.Unk6)) ||
                itemTemplate.Bonding == ItemBondingType.QuestItem || itemTemplate.Area != 0 || itemTemplate.Map != 0 ||
                itemTemplate.Stackable > 1 ||
                itemTemplate.Quality < ItemQuality.Uncommon || itemTemplate.Quality > ItemQuality.Epic ||
                !(itemTemplate.Class == ItemClass.Armor || itemTemplate.Class == ItemClass.Weapon)) //||
                //!(Item::GetSpecialPrice(&itemTemplate) || sItemCurrencyCostStore.LookupEntry(itemTemplate.ItemId)))
                return;
            uint Class = (uint)itemTemplate.Class;
            ItemQuality Quality = itemTemplate.Quality;

            //Maybe list of keys will be faster?
            var list = DBCStorage.ItemDisenchantLootStorage.Values;
            foreach (var disenchant in list)
            {
                if (disenchant.ItemClass == (uint)itemTemplate.Class &&
                    disenchant.ItemQuality == (uint)itemTemplate.Quality &&
                    disenchant.MinItemLevel <= itemTemplate.ItemLevel &&
                    disenchant.MaxItemLevel >= itemTemplate.ItemLevel)
                {
                    if (disenchant.Id == 60 || disenchant.Id == 61)   // epic item disenchant ilvl range 66-99 (classic)
                    {
                        if (itemTemplate.RequiredLevel > 60 || itemTemplate.RequiredSkillRank > 300)
                            continue;                                   // skip to epic item disenchant ilvl range 90-199 (TBC)
                    }
                    else if (disenchant.Id == 66 || disenchant.Id == 67)  // epic item disenchant ilvl range 90-199 (TBC)
                    {
                        if (itemTemplate.RequiredLevel <= 60 || (itemTemplate.RequiredSkill != 0 && itemTemplate.RequiredSkillRank <= 300))
                            continue;
                    }

                    disenchantID = disenchant.Id;
                    requiredDisenchantSkill = disenchant.RequiredDisenchantSkill;
                    return;
                }
            }
        }
        public void LoadItemTemplateAddon()
        {
            var time = Time.getMSTime();

            uint count = 0;
            SQLResult result = DB.World.Select("SELECT Id, FlagsCu, FoodType, MinMoneyLoot, MaxMoneyLoot, SpellPPMChance FROM item_template_addon");
            if (result.Count != 0)
            {
                for (var i = 0; i < result.Count; i++)
                {
                    uint itemId = result.Read<uint>(i, 0);
                    ItemTemplate itemTemplate = GetItemTemplate(itemId);
                    if (itemTemplate == null)
                    {
                        Log.outError("Item {0} specified in `item_template_addon` does not exist, skipped.", itemId);
                        continue;
                    }

                    uint minMoneyLoot = result.Read<uint>(i, 3);
                    uint maxMoneyLoot = result.Read<uint>(i, 4);
                    if (minMoneyLoot > maxMoneyLoot)
                    {
                        Log.outError("Minimum money loot specified in `item_template_addon` for item {0} was greater than maximum amount, swapping.", itemId);
                        uint temp = minMoneyLoot;
                        minMoneyLoot = maxMoneyLoot;
                        maxMoneyLoot = temp;
                    }
                    itemTemplate.FlagsCu = result.Read<uint>(i, 1);
                    itemTemplate.FoodType = result.Read<uint>(i, 2);
                    itemTemplate.MinMoneyLoot = minMoneyLoot;
                    itemTemplate.MaxMoneyLoot = maxMoneyLoot;
                    itemTemplate.SpellPPMRate = result.Read<float>(i, 5);
                    ++count;
                }
            }
            Log.outInfo("Loaded {0} item addon templates in {1} ms", count, Time.getMSTimeDiffNow(time));
        }

        public void LoadInstanceTemplate()
        {
            var time = Time.getMSTime();
            //                                          0     1       2        4
            SQLResult result = DB.World.Select("SELECT map, parent, script, allowMount FROM instance_template");

            if (result.Count == 0)
            {
                Log.outInfo("Loaded 0 instance templates. DB table `page_text` is empty!");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint mapID = result.Read<uint>(i, 0);

                if (!Cypher.MapMgr.IsValidMAP(mapID, true))
                {
                    Log.outError("ObjectMgr->LoadInstanceTemplate: bad mapid {0} for template!", mapID);
                    continue;
                }

                var instanceTemplate = new InstanceTemplate();
                instanceTemplate.AllowMount = result.Read<bool>(i, 3);
                instanceTemplate.Parent = result.Read<uint>(i, 1);
                instanceTemplate.ScriptId = 0;//sObjectMgr->GetScriptId(fields[2].GetCString());

                instanceTemplateStorage.Add(mapID, instanceTemplate);

                ++count;
            }
            Log.outInfo("Loaded {0} instance templates in {1} ms", count, Time.getMSTimeDiffNow(time));
        }


        #endregion

        public int GetAreaFlagByAreaID(uint area_id)
        {
            if (DBCStorage.AreaFlagByAreaID.ContainsKey(area_id))
                return (int)DBCStorage.AreaFlagByAreaID[area_id];

            return -1;
        }
        public AreaTableEntry GetAreaEntryByAreaID(uint area_id)
        {
            int areaflag = GetAreaFlagByAreaID(area_id);
            if (areaflag < 0)
                return null;

            return DBCStorage.AreaTableStorage.LookupByKey((uint)areaflag);
        }
        public AreaTableEntry GetAreaEntryByAreaFlagAndMap(uint area_flag, uint map_id)
        {
            if (area_flag != 0)
                return DBCStorage.AreaTableStorage.LookupByKey(area_flag);

            MapEntry mapEntry = DBCStorage.MapStorage.LookupByKey(map_id);
            if (mapEntry != null)
                return GetAreaEntryByAreaID(mapEntry.linked_zone);

            return null;
        }
        public uint GetAreaFlagByMapId(uint mapid)
        {
            return DBCStorage.AreaFlagByMapID.Find(p => p.Key == mapid).Value;
        }

        public InstanceTemplate GetInstanceTemplate(uint mapID)
        {
            return instanceTemplateStorage.LookupByKey(mapID);
        }

        public void ChooseCreatureFlags(CreatureTemplate cinfo, out uint npcflag, out uint unit_flags, out uint dynamicflags, CreatureData data = null)
        {
            npcflag = (uint)cinfo.Npcflag;
            unit_flags = cinfo.UnitFlags;
            dynamicflags = cinfo.DynamicFlags;

            if (data != null)
            {
                if (data.npcflag != 0)
                    npcflag = data.npcflag;

                if (data.unit_flags != 0)
                    unit_flags = data.unit_flags;

                if (data.dynamicflags != 0)
                    dynamicflags = data.dynamicflags;
            }
        }
        public CreatureModelInfo GetCreatureModelRandomGender(uint displayID)
        {
            CreatureModelInfo modelInfo = GetCreatureModelInfo(displayID);
            if (modelInfo == null)
                return null;

            // If a model for another gender exists, 50% chance to use it
            if (modelInfo.modelid_other_gender != 0 && RandomHelper.rand_norm() == 0)
            {
                CreatureModelInfo minfo_tmp = GetCreatureModelInfo(modelInfo.modelid_other_gender);
                if (minfo_tmp == null)
                    Log.outError("Model (Entry: {0}) has modelid_other_gender {1} not found in table `creature_model_info`. ", displayID, modelInfo.modelid_other_gender);
                else
                {
                    // Model ID changed
                    displayID = modelInfo.modelid_other_gender;
                    return minfo_tmp;
                }
            }

            return modelInfo;
        }

        CreatureModelInfo GetCreatureModelInfo(uint modelId)
        {
            return creatureModelStorage.LookupByKey(modelId);
        }
        public TrainerSpellData GetNpcTrainerSpells(uint entry)
        {
            return cacheTrainerSpellStorage.LookupByKey(entry);
        }

        #region Fields
        Dictionary<uint, string> CypherStringStorage = new Dictionary<uint, string>();
        Dictionary<byte, byte> ActivationClassStorage = new Dictionary<byte, byte>();
        Dictionary<byte, byte> ActivationRacesStorage = new Dictionary<byte, byte>();
        /// <summary>
        /// _mapObjectGuidsStore[mapid + spawnMask]["cellCoord.GetId()] == CellObjectGuids
        /// </summary>
        Dictionary<uint, Dictionary<uint, CellObjectGuids>> _mapObjectGuidsStore = new Dictionary<uint, Dictionary<uint, CellObjectGuids>>();

        //Creature
        Dictionary<uint, CreatureTemplate> creatureTemplateStorage = new Dictionary<uint, CreatureTemplate>();
        Dictionary<uint, CreatureModelInfo> creatureModelStorage = new Dictionary<uint, CreatureModelInfo>();
        Dictionary<uint, CreatureData> creatureDataStorage = new Dictionary<uint, CreatureData>();
        Dictionary<ulong, CreatureAddon> creatureAddonStorage = new Dictionary<ulong, CreatureAddon>();
        Dictionary<uint, CreatureAddon> creatureTemplateAddonStorage = new Dictionary<uint, CreatureAddon>();
        Dictionary<uint, List<Tuple<uint, EquipmentInfo>>> equipmentInfoStorage = new Dictionary<uint,List<Tuple<uint,EquipmentInfo>>>();
        //Dictionary<uint, Dictionary<uint, EquipmentInfo>> equipmentInfoStorage = new Dictionary<uint,Dictionary<uint,EquipmentInfo>>();
        Dictionary<ulong, ulong> linkedRespawnStorage = new Dictionary<ulong, ulong>();
        Dictionary<uint, CreatureBaseStats> creatureBaseStatsStorage = new Dictionary<uint, CreatureBaseStats>();

        Dictionary<uint, VendorItemData> cacheVendorItemStorage = new Dictionary<uint, VendorItemData>();
        Dictionary<uint, TrainerSpellData> cacheTrainerSpellStorage = new Dictionary<uint, TrainerSpellData>();

        //GameObject
        Dictionary<uint, GameObjectTemplate> gameObjectTemplateStorage = new Dictionary<uint, GameObjectTemplate>();
        Dictionary<uint, GameObjectData> gameObjectDataStorage = new Dictionary<uint, GameObjectData>();

        //Item
        Dictionary<uint, ItemTemplate> ItemTemplateStorage = new Dictionary<uint, ItemTemplate>();

        //Player
        PlayerInfo[][] PlayerInfoStorage = new PlayerInfo[(int)Race.Max][];
        Dictionary<uint, RepSpilloverTemplate> RepSpilloverStorage = new Dictionary<uint, RepSpilloverTemplate>();

        Dictionary<uint, InstanceTemplate> instanceTemplateStorage = new Dictionary<uint, InstanceTemplate>();


        // first free low guid for selected guid type
        uint _hiCharGuid;
        uint _hiCreatureGuid;
        uint _hiPetGuid;
        uint _hiVehicleGuid;
        uint _hiItemGuid;
        uint _hiGoGuid;
        uint _hiDoGuid;
        uint _hiCorpseGuid;
        uint _hiMoTransGuid;

        // first free id for selected id type
        uint _auctionId;
        ulong _equipmentSetGuid;
        uint _itemTextId;
        uint _mailId;
        uint _hiPetNumber;
        ulong _voidItemId;



        public uint[] XpPerLevel;
        #endregion
    }
    public class CellObjectGuids
    {
        public List<uint> creatures = new List<uint>();
        public List<uint> gameobjects = new List<uint>();
        public Dictionary<uint, uint> corpses = new Dictionary<uint, uint>();
    }

}
