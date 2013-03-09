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
using Framework.Constants;
using WorldServer.Game.Managers;
using System.Runtime.InteropServices;
using WorldServer.Game.Maps;
using Framework.Logging;
using WorldServer.Game.AI;
using Framework.Database;

namespace WorldServer.Game.WorldEntities
{

    public class GameObject : WorldObject
    {
        public GameObject() : base(false)
        {
            objectTypeMask = HighGuidMask.Object | HighGuidMask.GameObject;
            objectTypeId = ObjectType.GameObject;
            updateFlags = UpdateFlag.StationaryPosition | UpdateFlag.Rotation;
            SetValuesCount((int)GameObjectFields.End);

            m_respawnTime = 0;
            m_respawnDelayTime = 300;
            m_lootState = LootState.NotReady;
            m_spawnedByDefault = true;
            m_usetimes = 0;
            m_spellId = 0;
            m_cooldownTime = 0;
            m_goInfo = null;
            m_ritualOwner = null;
            godata = null;

            m_DBTableGuid = 0;
            m_rotation = 0;

            m_lootRecipient = 0;
            m_lootRecipientGroup = 0;
            m_groupLootTimer = 0;
            lootingGroupLowGUID = 0;

            //ResetLootMode(); // restore default loot mode
        }

        public override bool LoadFromDB(uint guid, Map map) { return LoadGameObjectFromDB(guid, map, false); }
        public bool LoadGameObjectFromDB(uint guid, Map map, bool addToMap = true)
        {
            GameObjectData data = Cypher.ObjMgr.GetGOData(guid);

            if (data == null)
            {
                Log.outError("Gameobject (GUID: {0}) not found in table `gameobject`, can't load. ", guid);
                return false;
            }

            uint entry = data.id;
            //uint32 map_id = data->mapid;                          // already used before call
            uint phaseMask = data.phaseMask;
            float x = data.posX;
            float y = data.posY;
            float z = data.posZ;
            float ang = data.orientation;

            float rotation0 = data.rotation0;
            float rotation1 = data.rotation1;
            float rotation2 = data.rotation2;
            float rotation3 = data.rotation3;

            uint animprogress = data.animprogress;
            GameObjectState go_state = data.go_state;
            uint artKit = data.artKit;

            m_DBTableGuid = guid;
            if (map.GetInstanceId() != 0) guid = Cypher.ObjMgr.GenerateLowGuid(HighGuidType.GameObject);

            if (!Create(guid, entry, map, phaseMask, x, y, z, ang, rotation0, rotation1, rotation2, rotation3, animprogress, go_state, artKit))
                return false;

            if (data.spawntimesecs >= 0)
            {
                //m_spawnedByDefault = true;

                if (!GetGoInfo().GetDespawnPossibility() && !GetGoInfo().IsDespawnAtAction())
                {
                    SetFlag(GameObjectFields.Flags, GameObjectFlags.Nodespawn);
                    //m_respawnDelayTime = 0;
                    //m_respawnTime = 0;
                }
                else
                {
                    // m_respawnDelayTime = data.spawntimesecs;
                    //m_respawnTime = GetMap().GetGORespawnTime(m_DBTableGuid);

                    // ready to respawn
                    //if (m_respawnTime && m_respawnTime <= time(NULL))
                    {
                        // m_respawnTime = 0;
                        //GetMap()->RemoveGORespawnTime(m_DBTableGuid);
                    }
                }
            }
            else
            {
                //m_spawnedByDefault = false;
                //m_respawnDelayTime = -data->spawntimesecs;
                //m_respawnTime = 0;
            }

            godata = data;

            if (addToMap && !GetMap().AddToMap(this))
                return false;

            return true;
        }

        public void SaveToDB()
        {
            // this should only be used when the gameobject has already been loaded
            // preferably after adding to map, because mapid may not be valid otherwise
            GameObjectData data = Cypher.ObjMgr.GetGOData(m_DBTableGuid);
            if (data == null)
            {
                Log.outError("GameObject->SaveToDB failed, cannot get gameobject data!");
                return;
            }

            SaveToDB(GetMap().GetId(), data.spawnMask, data.phaseMask);
        }

        public void SaveToDB(uint mapid, byte spawnMask, uint phaseMask)
        {
            GameObjectTemplate goI = GetGoInfo();

            if (goI == null)
                return;

            if (m_DBTableGuid == 0)
                m_DBTableGuid = GetGUIDLow();
            // update in loaded data (changing data only in this place)
            GameObjectData data = new GameObjectData();

            // data->guid = guid must not be updated at save
            data.id = GetEntry();
            data.mapid = (ushort)mapid;
            data.phaseMask = (ushort)phaseMask;
            data.posX = GetPositionX();
            data.posY = GetPositionY();
            data.posZ = GetPositionZ();
            data.orientation = GetOrientation();
            data.rotation0 = GetValue<float>(GameObjectFields.ParentRotation);
            data.rotation1 = GetValue<float>(GameObjectFields.ParentRotation + 1);
            data.rotation2 = GetValue<float>(GameObjectFields.ParentRotation + 2);
            data.rotation3 = GetValue<float>(GameObjectFields.ParentRotation + 3);
            data.spawntimesecs = (int)(m_spawnedByDefault ? m_respawnDelayTime : -m_respawnDelayTime);
            data.animprogress = GetGoAnimProgress();
            data.go_state = GetGoState();
            data.spawnMask = spawnMask;
            data.artKit = GetGoArtKit();

            Cypher.ObjMgr.NewGOData(m_DBTableGuid, data);

            // Update in DB
            byte index = 0;
            PreparedStatement stmt = DB.World.GetPreparedStatement(WorldStatements.Del_GameObject);
            stmt.AddValue(0, m_DBTableGuid);
            DB.World.Execute(stmt);

            stmt = DB.World.GetPreparedStatement(WorldStatements.Ins_Gameobject);
            stmt.AddValue(index++, m_DBTableGuid);
            stmt.AddValue(index++, GetEntry());
            stmt.AddValue(index++, mapid);
            stmt.AddValue(index++, spawnMask);
            stmt.AddValue(index++, 1);//GetPhaseMask());
            stmt.AddValue(index++, GetPositionX());
            stmt.AddValue(index++, GetPositionY());
            stmt.AddValue(index++, GetPositionZ());
            stmt.AddValue(index++, GetOrientation());
            stmt.AddValue(index++, GetValue<float>(GameObjectFields.ParentRotation));
            stmt.AddValue(index++, GetValue<float>(GameObjectFields.ParentRotation + 1));
            stmt.AddValue(index++, GetValue<float>(GameObjectFields.ParentRotation + 2));
            stmt.AddValue(index++, GetValue<float>(GameObjectFields.ParentRotation + 3));
            stmt.AddValue(index++, m_respawnDelayTime);
            stmt.AddValue(index++, GetGoAnimProgress());
            stmt.AddValue(index++, GetGoState());
            DB.World.Execute(stmt);
        }

        public bool Create(uint guidlow, uint name_id, Map map, uint phaseMask, float x, float y, float z, float ang, float rotation0, float rotation1, float rotation2,
            float rotation3, uint animprogress, GameObjectState go_state, uint artKit = 0)
        {
            //ASSERT(map);
            SetMap(map);

            Position = new ObjectPosition(x, y, z, ang);
            if (!IsPositionValid())
            {
                Log.outError("Gameobject (GUID: {0} Entry: {1}) not created. Suggested coordinates isn't valid (X: {2} Y: {3})", guidlow, name_id, x, y);
                return false;
            }

            SetPhaseMask(phaseMask, false);

            //SetZoneScript();
            //if (m_zoneScript)
            {
                //name_id = m_zoneScript->GetGameObjectEntry(guidlow, name_id);
                //if (!name_id)
                //return false;
            }

            GameObjectTemplate goinfo = Cypher.ObjMgr.GetGameObjectTemplate(name_id);
            if (goinfo == null)
            {
                Log.outError("Gameobject (GUID: {0} Entry: {1}) not created: non-existing entry in `gameobject_template`. Map: {2} (X: {3} Y: {4} Z: {5})", guidlow, name_id, map.GetId(), x, y, z);
                return false;
            }

            CreateGuid(guidlow, goinfo.entry, HighGuidType.GameObject);

            m_goInfo = goinfo;

            if (goinfo.type >= GameObjectTypes.Max)
            {
                Log.outError("Gameobject (GUID: {0} Entry: {1}) not created: non-existing GO type '{2}' in `gameobject_template`. It will crash client if created.", guidlow, name_id, goinfo.type);
                return false;
            }

            SetValue<float>(GameObjectFields.ParentRotation, rotation0);
            SetValue<float>(GameObjectFields.ParentRotation + 1, rotation1);

            UpdateRotationFields(rotation2, rotation3);              // GAMEOBJECT_FACING, GAMEOBJECT_ROTATION, GAMEOBJECT_PARENTROTATION+2/3

            SetObjectScale(goinfo.size);

            SetValue<uint>(GameObjectFields.FactionTemplate, goinfo.faction);
            SetValue<uint>(GameObjectFields.Flags, goinfo.flags);

            SetEntry(goinfo.entry);

            // set name for logs usage, doesn't affect anything ingame
            SetName(goinfo.name);

            SetDisplayId(goinfo.displayId);

            //m_model = GameObjectModel::Create(*this);
            // GAMEOBJECT_BYTES_1, index at 0, 1, 2 and 3
            SetGoType(goinfo.type);
            SetGoState(go_state);
            SetGoArtKit((byte)artKit);

            switch (goinfo.type)
            {
                case GameObjectTypes.DestructibleBuilding:
                    Building.Health = goinfo.Building.intactNumHits + goinfo.Building.damagedNumHits;
                    Building.MaxHealth = Building.Health;
                    SetGoAnimProgress(255);
                    SetValue<Single>(GameObjectFields.ParentRotation, goinfo.Building.destructibleData);
                    break;
                case GameObjectTypes.Transport:
                    SetValue<UInt32>(GameObjectFields.Level, goinfo.Transport.pause);
                    if (goinfo.Transport.startOpen != 0)
                        SetGoState(GameObjectState.Active);
                    SetGoAnimProgress(animprogress);
                    break;
                case GameObjectTypes.FishingNode:
                    SetGoAnimProgress(0);
                    break;
                case GameObjectTypes.Trap:
                    if (goinfo.Trap.stealthed != 0)
                    {
                        //m_stealth.AddFlag(STEALTH_TRAP);
                        //m_stealth.AddValue(STEALTH_TRAP, 70);
                    }

                    if (goinfo.Trap.invisible != 0)
                    {
                        // m_invisibility.AddFlag(INVISIBILITY_TRAP);
                        //m_invisibility.AddValue(INVISIBILITY_TRAP, 300);
                    }
                    break;
                default:
                    SetGoAnimProgress(animprogress);
                    break;
            }

            //LastUsedScriptID = GetGoInfo().ScriptId;
            //AIM_Initialize();
            return true;
        }


        /*
        void SetLootState(LootState state, Unit* unit)
{
    m_lootState = state;
    AI()->OnStateChanged(state, unit);
    sScriptMgr->OnGameObjectLootStateChanged(this, state, unit);
    if (m_model)
    {
        bool collision = false;
        // Use the current go state
        if ((GetGoState() != GO_STATE_READY && (state == GO_ACTIVATED || state == GO_JUST_DEACTIVATED)) || state == GO_READY)
            collision = !collision;

        EnableCollision(collision);
    }
}
        */
        byte GetGoArtKit() { return GetValue<Byte>(GameObjectFields.Bytes, 2); }
        void SetGoArtKit(byte kit)
        {
            SetValue<byte>(GameObjectFields.Bytes, kit, 2);
            GameObjectData data = Cypher.ObjMgr.GetGOData(m_DBTableGuid);
            if (data != null)
                data.artKit = kit;
        }
        byte GetGoAnimProgress() { return GetValue<Byte>(GameObjectFields.Bytes, 3); }
        void SetGoAnimProgress(uint animprogress) { SetValue<byte>(GameObjectFields.Bytes, (byte)animprogress, 3); }
        public GameObjectTypes GetGoType() { return (GameObjectTypes)GetValue<byte>(GameObjectFields.Bytes, 1); }
        void SetGoType(GameObjectTypes type) { SetValue<byte>(GameObjectFields.Bytes, (byte)type, 1); }
        void SetGoState(GameObjectState state)
        {
            SetValue<byte>(GameObjectFields.Bytes, (byte)state, 0);
            //sScriptMgr->OnGameObjectStateChanged(this, state);
            //if (m_model)
            {
                //if (!IsInWorld())
                //return;

                // startOpen determines whether we are going to add or remove the LoS on activation
                //bool collision = false;
                //if (state == GO_STATE_READY)
                //collision = !collision;

                //EnableCollision(collision);
            }
        }
        public GameObjectState GetGoState() { return (GameObjectState)GetValue<Byte>(GameObjectFields.Bytes, 0); }

        void SetDisplayId(uint displayid)
        {
            SetValue<uint>(GameObjectFields.DisplayID, displayid);
            //UpdateModel();
        }

        void SetPhaseMask(uint newPhaseMask, bool update)
        {
            //WorldObject::SetPhaseMask(newPhaseMask, update);
            //if (m_model && m_model->isEnabled())
            //EnableCollision(true);
        }

        public GameObjectTemplate GetGoInfo() { return m_goInfo; }

        void UpdateRotationFields(float r2, float r3)
        {
            double atan_pow = Math.Atan(Math.Pow(2.0f, -20.0f));

            double f_rot1 = Math.Sin(Position.Orientation / 2.0f);
            double f_rot2 = Math.Cos(Position.Orientation / 2.0f);

            long i_rot1 = (long)(f_rot1 / atan_pow * (f_rot2 >= 0 ? 1.0f : -1.0f));
            long rotation = (i_rot1 << 43 >> 43) & 0x00000000001FFFFF;

            PackedRotation = rotation;

            if (r2 == 0.0f && r3 == 0.0f)
            {
                r2 = (float)f_rot1;
                r3 = (float)f_rot2;
                SetValue<Single>(GameObjectFields.ParentRotation + 2, r2);
                SetValue<Single>(GameObjectFields.ParentRotation + 3, r3);
            }
        }
        public ulong GetOwnerGUID() { return GetValue<ulong>(GameObjectFields.CreatedBy); }
        public bool IsGameObjectType(GameObjectTypes type) { return Convert.ToBoolean(type & GetGoInfo().type); }

        #region Fields
        GameObjectTemplate m_goInfo;
        GameObjectData godata;
        GameObjectBuilding Building;
        uint m_DBTableGuid;
        uint m_spellId;
        uint m_respawnTime;                          // (secs) time of next respawn (or despawn if GO have owner()),
        uint m_respawnDelayTime;                     // (secs) if 0 then current GO state no dependent from timer
        LootState m_lootState;
        bool m_spawnedByDefault;
        uint m_cooldownTime;                         // used as internal reaction delay time store (not state change reaction).
        // For traps this: spell casting cooldown, for doors/buttons: reset time.

        Player m_ritualOwner;                              // used for GAMEOBJECT_TYPE_SUMMONING_RITUAL where GO is not summoned (no owner)
        //std::set<uint64> m_unique_users;
        uint m_usetimes;

        ulong m_rotation;
        ulong m_lootRecipient;
        uint m_lootRecipientGroup;
        ushort m_LootMode;                                  // bitmask, default LOOT_MODE_DEFAULT, determines what loot will be lootable
        uint m_groupLootTimer;                            // (msecs)timer used for group loot
        uint lootingGroupLowGUID;                         // used to find group which is looting
        public long PackedRotation { get; set; }

        bool m_AI_locked;
        #endregion
    }

    [StructLayout(LayoutKind.Explicit)]
    public class GameObjectTemplate
    {
        [FieldOffset(0)]
        public uint entry;

        [FieldOffset(4)]
        public GameObjectTypes type;

        [FieldOffset(8)]
        public uint displayId;

        [FieldOffset(12)]
        public string name;

        [FieldOffset(20)]
        public string IconName;

        [FieldOffset(28)]
        public string castBarCaption;

        [FieldOffset(36)]
        public string unk1;

        [FieldOffset(44)]
        public uint faction;

        [FieldOffset(48)]
        public uint flags;

        [FieldOffset(52)]
        public float size;

        [FieldOffset(56)]
        public uint[] questItems = new uint[6];

        [FieldOffset(80)]
        public int unkInt32;

        [FieldOffset(84)]
        public string AIName;

        [FieldOffset(92)]
        public uint ScriptId;

        [FieldOffset(96)]
        public int[] RawData = new int[32];

        [FieldOffset(224)]
        public door Door;
        [FieldOffset(224)]
        public button Button;
        [FieldOffset(224)]
        public questgiver QuestGiver;
        [FieldOffset(224)]
        public chest Chest;
        [FieldOffset(224)]
        public generic Generic;
        [FieldOffset(224)]
        public trap Trap;
        [FieldOffset(224)]
        public chair Chair;
        [FieldOffset(224)]
        public spellFocus SpellFocus;
        [FieldOffset(224)]
        public text Text;
        [FieldOffset(224)]
        public goober Goober;
        [FieldOffset(224)]
        public transport Transport;
        [FieldOffset(224)]
        public areadamage AreaDamage;
        [FieldOffset(224)]
        public camera Camera;
        [FieldOffset(224)]
        public moTransport MoTransport;
        [FieldOffset(224)]
        public summoningRitual SummoningRitual;
        [FieldOffset(224)]
        public guardpost GuardPost;
        [FieldOffset(224)]
        public spellcaster SpellCaster;
        [FieldOffset(224)]
        public meetingstone MeetingStone;
        [FieldOffset(224)]
        public flagstand FlagStand;
        [FieldOffset(224)]
        public fishinghole FishingHole;
        [FieldOffset(224)]
        public flagdrop FlagDrop;
        [FieldOffset(224)]
        public miniGame MiniGame;
        [FieldOffset(224)]
        public capturePoint CapturePoint;
        [FieldOffset(224)]
        public auraGenerator AuraGenerator;
        [FieldOffset(224)]
        public dungeonDifficulty DungeonDifficulty;
        [FieldOffset(224)]
        public barberChair BarberChair;
        [FieldOffset(224)]
        public building Building;
        [FieldOffset(224)]
        public trapDoor TrapDoor;

        // helpers
        public bool IsDespawnAtAction()
        {
            switch (type)
            {
                case GameObjectTypes.Chest:
                    return Chest.consumable != 0;
                case GameObjectTypes.Goober:
                    return Goober.consumable != 0;
                default:
                    return false;
            }
        }

        public uint GetLockId()
        {
            switch (type)
            {
                case GameObjectTypes.Door:
                    return Door.lockId;
                case GameObjectTypes.Button:
                    return Button.lockId;
                case GameObjectTypes.QuestGiver:
                    return QuestGiver.lockId;
                case GameObjectTypes.Chest:
                    return Chest.lockId;
                case GameObjectTypes.Trap:
                    return Trap.lockId;
                case GameObjectTypes.Goober:
                    return Goober.lockId;
                case GameObjectTypes.AreaDamage:
                    return AreaDamage.lockId;
                case GameObjectTypes.Camera:
                    return Camera.lockId;
                case GameObjectTypes.FlagStand:
                    return FlagStand.lockId;
                case GameObjectTypes.FishingHole:
                    return FishingHole.lockId;
                case GameObjectTypes.FlagDrop:
                    return FlagDrop.lockId;
                default:
                    return 0;
            }
        }

        public bool GetDespawnPossibility()                      // despawn at targeting of cast?
        {
            switch (type)
            {
                case GameObjectTypes.Door:
                    return Door.noDamageImmune != 0;
                case GameObjectTypes.Button:
                    return Button.noDamageImmune != 0;
                case GameObjectTypes.QuestGiver:
                    return QuestGiver.noDamageImmune != 0;
                case GameObjectTypes.Goober:
                    return Goober.noDamageImmune != 0;
                case GameObjectTypes.FlagStand:
                    return FlagStand.noDamageImmune != 0;
                case GameObjectTypes.FlagDrop:
                    return FlagDrop.noDamageImmune != 0;
                default:
                    return true;
            }
        }

        public uint GetCharges()                               // despawn at uses amount
        {
            switch (type)
            {
                //case GAMEOBJECT_TYPE_TRAP:        return trap.charges;
                case GameObjectTypes.GuardPost:
                    return GuardPost.charges;
                case GameObjectTypes.SpellCaster:
                    return SpellCaster.charges;
                default:
                    return 0;
            }
        }

        uint GetLinkedGameObjectEntry()
        {
            switch (type)
            {
                case GameObjectTypes.Chest:
                    return Chest.linkedTrapId;
                case GameObjectTypes.SpellFocus:
                    return SpellFocus.linkedTrapId;
                case GameObjectTypes.Goober:
                    return Goober.linkedTrapId;
                default: return 0;
            }
        }

        uint GetAutoCloseTime()
        {
            uint autoCloseTime = 0;
            switch (type)
            {
                case GameObjectTypes.Door:
                    autoCloseTime = Door.autoCloseTime;
                    break;
                case GameObjectTypes.Button:
                    autoCloseTime = Button.autoCloseTime;
                    break;
                case GameObjectTypes.Trap:
                    autoCloseTime = (uint)Trap.autoCloseTime;
                    break;
                case GameObjectTypes.Goober:
                    autoCloseTime = Goober.autoCloseTime;
                    break;
                case GameObjectTypes.Transport:
                    autoCloseTime = Transport.autoCloseTime;
                    break;
                case GameObjectTypes.AreaDamage:
                    autoCloseTime = AreaDamage.autoCloseTime;
                    break;
                default: break;
            }
            return autoCloseTime / 1000;// IN_MILLISECONDS;              // prior to 3.0.3, conversion was / 0x10000;
        }

        uint GetLootId()
        {
            switch (type)
            {
                case GameObjectTypes.Chest:
                    return Chest.lootId;
                case GameObjectTypes.FishingHole:
                    return FishingHole.lootId;
                default: return 0;
            }
        }

        uint GetGossipMenuId()
        {
            switch (type)
            {
                case GameObjectTypes.QuestGiver:
                    return QuestGiver.gossipID;
                case GameObjectTypes.Goober:
                    return Goober.gossipID;
                default:
                    return 0;
            }
        }

        uint GetEventScriptId()
        {
            switch (type)
            {
                case GameObjectTypes.Goober:
                    return Goober.eventId;
                case GameObjectTypes.Chest:
                    return Chest.eventId;
                case GameObjectTypes.Camera:
                    return Camera.eventID;
                default:
                    return 0;
            }
        }

        uint GetCooldown()                              // Cooldown preventing goober and traps to cast spell
        {
            switch (type)
            {
                case GameObjectTypes.Trap:
                    return Trap.cooldown;
                case GameObjectTypes.Goober:
                    return Goober.cooldown;
                default:
                    return 0;
            }
        }

        #region TypeStructs
        [StructLayout(LayoutKind.Sequential)]
        public struct door
        {
            public uint startOpen;                               //0 used client side to determine GO_ACTIVATED means open/closed
            public uint lockId;                                  //1 -> Lock.dbc
            public uint autoCloseTime;                           //2 secs till autoclose = autoCloseTime / 0x10000
            public uint noDamageImmune;                          //3 break opening whenever you recieve damage?
            public uint openTextID;                              //4 can be used to replace castBarCaption?
            public uint closeTextID;                             //5
            public uint ignoredByPathing;                        //6
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct button
        {
            public uint startOpen;                               //0
            public uint lockId;                                  //1 -> Lock.dbc
            public uint autoCloseTime;                           //2 secs till autoclose = autoCloseTime / 0x10000
            public uint linkedTrap;                              //3
            public uint noDamageImmune;                          //4 isBattlegroundObject
            public uint large;                                   //5
            public uint openTextID;                              //6 can be used to replace castBarCaption?
            public uint closeTextID;                             //7
            public uint losOK;                                   //8
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct questgiver
        {
            public uint lockId;                                  //0 -> Lock.dbc
            public uint questList;                               //1
            public uint pageMaterial;                            //2
            public uint gossipID;                                //3
            public uint customAnim;                              //4
            public uint noDamageImmune;                          //5
            public uint openTextID;                              //6 can be used to replace castBarCaption?
            public uint losOK;                                   //7
            public uint allowMounted;                            //8
            public uint large;                                   //9
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct chest
        {
            public uint lockId;                                  //0 -> Lock.dbc
            public uint lootId;                                  //1
            public uint chestRestockTime;                        //2
            public uint consumable;                              //3
            public uint minSuccessOpens;                         //4
            public uint maxSuccessOpens;                         //5
            public uint eventId;                                 //6 lootedEvent
            public uint linkedTrapId;                            //7
            public uint questId;                                 //8 not used currently but store quest required for GO activation for player
            public uint level;                                   //9
            public uint losOK;                                   //10
            public uint leaveLoot;                               //11
            public uint notInCombat;                             //12
            public uint logLoot;                                 //13
            public uint openTextID;                              //14 can be used to replace castBarCaption?
            public uint groupLootRules;                          //15
            public uint floatingTooltip;                         //16
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct generic
        {
            public uint floatingTooltip;                         //0
            public uint highlight;                               //1
            public uint serverOnly;                              //2
            public uint large;                                   //3
            public uint floatOnWater;                            //4
            public int questID;                                  //5
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct trap
        {
            public uint lockId;                                  //0 -> Lock.dbc
            public uint level;                                   //1
            public uint radius;                                  //2 radius for trap activation
            public uint spellId;                                 //3
            public uint type;                                    //4 0 trap with no despawn after cast. 1 trap despawns after cast. 2 bomb casts on spawn.
            public uint cooldown;                                //5 time in secs
            public int autoCloseTime;                            //6
            public uint startDelay;                              //7
            public uint serverOnly;                              //8
            public uint stealthed;                               //9
            public uint large;                                   //10
            public uint invisible;                               //11
            public uint openTextID;                              //12 can be used to replace castBarCaption?
            public uint closeTextID;                             //13
            public uint ignoreTotems;                            //14
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct chair
        {
            public uint slots;                                   //0
            public uint height;                                  //1
            public uint onlyCreatorUse;                          //2
            public uint triggeredEvent;                          //3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct spellFocus
        {
            public uint focusId;                                 //0
            public uint dist;                                    //1
            public uint linkedTrapId;                            //2
            public uint serverOnly;                              //3
            public uint questID;                                 //4
            public uint large;                                   //5
            public uint floatingTooltip;                         //6
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct text
        {
            public uint pageID;                                  //0
            public uint language;                                //1
            public uint pageMaterial;                            //2
            public uint allowMounted;                            //3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct goober
        {
            public uint lockId;                                  //0 -> Lock.dbc
            public int questId;                                  //1
            public uint eventId;                                 //2
            public uint autoCloseTime;                           //3
            public uint customAnim;                              //4
            public uint consumable;                              //5
            public uint cooldown;                                //6
            public uint pageId;                                  //7
            public uint language;                                //8
            public uint pageMaterial;                            //9
            public uint spellId;                                 //10
            public uint noDamageImmune;                          //11
            public uint linkedTrapId;                            //12
            public uint large;                                   //13
            public uint openTextID;                              //14 can be used to replace castBarCaption?
            public uint closeTextID;                             //15
            public uint losOK;                                   //16 isBattlegroundObject
            public uint allowMounted;                            //17
            public uint floatingTooltip;                         //18
            public uint gossipID;                                //19
            public uint WorldStateSetsState;                     //20
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct transport
        {
            public uint pause;                                   //0
            public uint startOpen;                               //1
            public uint autoCloseTime;                           //2 secs till autoclose = autoCloseTime / 0x10000
            public uint pause1EventID;                           //3
            public uint pause2EventID;                           //4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct areadamage
        {
            public uint lockId;                                  //0
            public uint radius;                                  //1
            public uint damageMin;                               //2
            public uint damageMax;                               //3
            public uint damageSchool;                            //4
            public uint autoCloseTime;                           //5 secs till autoclose = autoCloseTime / 0x10000
            public uint openTextID;                              //6
            public uint closeTextID;                             //7
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct camera
        {
            public uint lockId;                                  //0 -> Lock.dbc
            public uint cinematicId;                             //1
            public uint eventID;                                 //2
            public uint openTextID;                              //3 can be used to replace castBarCaption?
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct moTransport
        {
            public uint taxiPathId;                              //0
            public uint moveSpeed;                               //1
            public uint accelRate;                               //2
            public uint startEventID;                            //3
            public uint stopEventID;                             //4
            public uint transportPhysics;                        //5
            public uint mapID;                                   //6
            public uint worldState1;                             //7
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct summoningRitual
        {
            public uint reqParticipants;                         //0
            public uint spellId;                                 //1
            public uint animSpell;                               //2
            public uint ritualPersistent;                        //3
            public uint casterTargetSpell;                       //4
            public uint casterTargetSpellTargets;                //5
            public uint castersGrouped;                          //6
            public uint ritualNoTargetCheck;                     //7
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct guardpost
        {
            public uint creatureID;                              //0
            public uint charges;                                 //1
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct spellcaster
        {
            public uint spellId;                                 //0
            public uint charges;                                 //1
            public uint partyOnly;                               //2
            public uint allowMounted;                            //3
            public uint large;                                   //4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct meetingstone
        {
            public uint minLevel;                                //0
            public uint maxLevel;                                //1
            public uint areaID;                                  //2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct flagstand
        {
            public uint lockId;                                  //0
            public uint pickupSpell;                             //1
            public uint radius;                                  //2
            public uint returnAura;                              //3
            public uint returnSpell;                             //4
            public uint noDamageImmune;                          //5
            public uint openTextID;                              //6
            public uint losOK;                                   //7
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct fishinghole
        {
            public uint radius;                                  //0 how close bobber must land for sending loot
            public uint lootId;                                  //1
            public uint minSuccessOpens;                         //2
            public uint maxSuccessOpens;                         //3
            public uint lockId;                                  //4 -> Lock.dbc; possibly 1628 for all?
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct flagdrop
        {
            public uint lockId;                                  //0
            public uint eventID;                                 //1
            public uint pickupSpell;                             //2
            public uint noDamageImmune;                          //3
            public uint openTextID;                              //4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct miniGame
        {
            public uint gameType;                                //0
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct capturePoint
        {
            public uint radius;                                  //0
            public uint spell;                                   //1
            public uint worldState1;                             //2
            public uint worldstate2;                             //3
            public uint winEventID1;                             //4
            public uint winEventID2;                             //5
            public uint contestedEventID1;                       //6
            public uint contestedEventID2;                       //7
            public uint progressEventID1;                        //8
            public uint progressEventID2;                        //9
            public uint neutralEventID1;                         //10
            public uint neutralEventID2;                         //11
            public uint neutralPercent;                          //12
            public uint worldstate3;                             //13
            public uint minSuperiority;                          //14
            public uint maxSuperiority;                          //15
            public uint minTime;                                 //16
            public uint maxTime;                                 //17
            public uint large;                                   //18
            public uint highlight;                               //19
            public uint startingValue;                           //20
            public uint unidirectional;                          //21
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct auraGenerator
        {
            public uint startOpen;                               //0
            public uint radius;                                  //1
            public uint auraID1;                                 //2
            public uint conditionID1;                            //3
            public uint auraID2;                                 //4
            public uint conditionID2;                            //5
            public uint serverOnly;                              //6
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct dungeonDifficulty
        {
            public uint mapID;                                   //0
            public uint difficulty;                              //1
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct barberChair
        {
            public uint chairheight;                             //0
            public uint heightOffset;                            //1
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct building
        {
            public uint intactNumHits;                           //0
            public uint creditProxyCreature;                     //1
            public uint state1Name;                              //2
            public uint intactEvent;                             //3
            public uint damagedDisplayId;                        //4
            public uint damagedNumHits;                          //5
            public uint empty3;                                  //6
            public uint empty4;                                  //7
            public uint empty5;                                  //8
            public uint damagedEvent;                            //9
            public uint destroyedDisplayId;                      //10
            public uint empty7;                                  //11
            public uint empty8;                                  //12
            public uint empty9;                                  //13
            public uint destroyedEvent;                          //14
            public uint empty10;                                 //15
            public uint debuildingTimeSecs;                      //16
            public uint empty11;                                 //17
            public uint destructibleData;                        //18
            public uint rebuildingEvent;                         //19
            public uint empty12;                                 //20
            public uint empty13;                                 //21
            public uint damageEvent;                             //22
            public uint empty14;                                 //23
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct trapDoor
        {
            public uint whenToPause;                             // 0
            public uint startOpen;                               // 1
            public uint autoClose;                               // 2
        }
        #endregion
    }

    public struct GameObjectBuilding
    {
        public uint Health;
        public uint MaxHealth;
    }
    public class GameObjectData
    {
        public uint id;                                              // entry in gamobject_template
        public ushort mapid;
        public ushort phaseMask;
        public float posX;
        public float posY;
        public float posZ;
        public float orientation;
        public float rotation0;
        public float rotation1;
        public float rotation2;
        public float rotation3;
        public int spawntimesecs;
        public uint animprogress;
        public GameObjectState go_state;
        public byte spawnMask;
        public byte artKit;
        public bool dbData = true;
    }
}
