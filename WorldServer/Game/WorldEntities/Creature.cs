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
using Framework.Constants;
using WorldServer.Game.Managers;
using Framework.Logging;
using Framework.Utility;
using Framework.DataStorage;
using WorldServer.Game.Maps;
using WorldServer.Game.AI;
using Framework.Configuration;

namespace WorldServer.Game.WorldEntities
{
    public class Creature : Unit
    {
        public Creature() : base(false)
        {
            objectTypeMask = HighGuidMask.Object | HighGuidMask.Unit;
            objectTypeId = ObjectType.Unit;
            updateFlags = UpdateFlag.Living;
            SetValuesCount((int)UnitFields.End);
            m_homePosition = new ObjectPosition();

            m_CombatDistance = ObjectConst.MeleeRange;
        }

        public override void AddToWorld()
        {
            ///- Register the creature for guid lookup
            if (!IsInWorld)
            {
                //if (m_zoneScript)
                //m_zoneScript->OnCreatureCreate(this);
                Cypher.ObjMgr.AddObject(this);
                base.AddToWorld();
                //SearchFormation();
                AIM_Initialize();
                //if (IsVehicle())
                //GetVehicleKit()->Install();
            }
        }

        public CreatureAI AI() { return (CreatureAI)i_AI; }

        bool CreateFromProto(uint guidlow, uint Entry, uint vehId, uint team, CreatureData data)
        {
            //SetZoneScript();
            //if (m_zoneScript && data)
            {
                //Entry = m_zoneScript->GetCreatureEntry(guidlow, data);
               // if (!Entry)
                    //return false;
            }

            CreatureTemplate cinfo = Cypher.ObjMgr.GetCreatureTemplate(Entry);
            if (cinfo == null)
            {
                Log.outError("Creature->CreateFromProto: creature template (guidlow: {0}, entry: {1}) does not exist.", guidlow, Entry);
                return false;
            }

            SetOriginalEntry(Entry);

            if (vehId == 0)
                vehId = cinfo.VehicleId;

            //if (vehId != 0 && !CreateVehicleKit(vehId, Entry))
                //vehId = 0;

            CreateGuid(guidlow, Entry, vehId != 0 ? HighGuidType.Vehicle : HighGuidType.Unit);

            if (!UpdateEntry(Entry, team, data))
                return false;

            return true;
        }
        bool Create(uint guidlow, Map map, uint phaseMask, uint Entry, uint vehId, uint team, float x, float y, float z, float ang, CreatureData data)
        {
            SetMap(map);
            //SetPhaseMask(phaseMask, false);

            CreatureTemplate cinfo = Cypher.ObjMgr.GetCreatureTemplate(Entry);
            if (cinfo == null)
            {
                Log.outError("Creature->Create: creature template (guidlow: {0}, entry: {1}) does not exist.", guidlow, Entry);
                return false;
            }

            //! Relocate before CreateFromProto, to initialize coords and allow
            //! returning correct zone id for selecting OutdoorPvP/Battlefield script
            Position = new ObjectPosition(x, y, z, ang);

            //oX = x;     oY = y;    dX = x;    dY = y;    m_moveTime = 0;    m_startMove = 0;
            if (!CreateFromProto(guidlow, Entry, vehId, team, data))
                return false;

            if (!IsPositionValid())
            {
                Log.outError("Creature->Create: given coordinates for creature (guidlow {0}, entry {1}) are not valid (X: {2}, Y: {3}, Z: {4}, O: {5})", guidlow, Entry, x, y, z, ang);
                return false;
            }

            switch ((CreatureEliteType)GetCreatureTemplate().Rank)
            {
                case CreatureEliteType.RARE:
                    //m_corpseDelay = sWorld->getIntConfig(CONFIG_CORPSE_DECAY_RARE);
                    break;
                case CreatureEliteType.ELITE:
                    //m_corpseDelay = sWorld->getIntConfig(CONFIG_CORPSE_DECAY_ELITE);
                    break;
                case CreatureEliteType.RAREELITE:
                    // m_corpseDelay = sWorld->getIntConfig(CONFIG_CORPSE_DECAY_RAREELITE);
                    break;
                case CreatureEliteType.WORLDBOSS:
                    //m_corpseDelay = sWorld->getIntConfig(CONFIG_CORPSE_DECAY_WORLDBOSS);
                    break;
                default:
                    //m_corpseDelay = sWorld->getIntConfig(CONFIG_CORPSE_DECAY_NORMAL);
                    break;
            }
            //LoadCreaturesAddon();

            //! Need to be called after LoadCreaturesAddon - MOVEMENTFLAG_HOVER is set there
            //if (HasUnitMovementFlag(MOVEMENTFLAG_HOVER))
            {
                //z += GetFloatValue(UNIT_FIELD_HOVERHEIGHT);

                //! Relocate again with updated Z coord
                //Relocate(x, y, z, ang);
            }

            uint displayID = GetNativeDisplayId();
            CreatureModelInfo minfo = Cypher.ObjMgr.GetCreatureModelRandomGender(displayID);
            if (minfo != null && !isTotem())                               // Cancel load if no model defined or if totem
            {
                SetDisplayId(displayID);
                SetNativeDisplayId(displayID);
                SetValue<byte>(UnitFields.Bytes, minfo.gender, 2);
            }

            //LastUsedScriptID = GetCreatureTemplate()->ScriptID;

            // TODO: Replace with spell, handle from DB
            if (isSpiritHealer() || isSpiritGuide())
            {
                //m_serverSideVisibility.SetValue(SERVERSIDE_VISIBILITY_GHOST, GHOST_VISIBILITY_GHOST);
                //m_serverSideVisibilityDetect.SetValue(SERVERSIDE_VISIBILITY_GHOST, GHOST_VISIBILITY_GHOST);
            }

            //if (Entry == VISUAL_WAYPOINT)
                //SetVisible(false);

            return true;
        }
        bool InitEntry(uint entry, uint team, CreatureData data)
        {
            CreatureTemplate normalInfo = Cypher.ObjMgr.GetCreatureTemplate(entry);
            if (normalInfo == null)
            {
                Log.outError("Creature->InitEntry creature entry {0} does not exist.", entry);
                return false;
            }

            // get difficulty 1 mode entry
            CreatureTemplate cinfo = normalInfo;
            for (var diff = (uint)GetMap().GetSpawnMode(); diff > 0; )
            {
                // we already have valid Map pointer for current creature!
                if (normalInfo.DifficultyEntry[diff - 1] != 0)
                {
                    cinfo = Cypher.ObjMgr.GetCreatureTemplate(normalInfo.DifficultyEntry[diff - 1]);
                    if (cinfo != null)
                        break;                                      // template found

                    // check and reported at startup, so just ignore (restore normalInfo)
                    cinfo = normalInfo;
                }

                // for instances heroic to normal, other cases attempt to retrieve previous difficulty
                if (diff >= (uint)Difficulty.Raid10manHeroic && GetMap().IsRaid())
                    diff -= 2;                                      // to normal raid difficulty cases
                else
                    diff--;
            }

            SetEntry(entry);                                        // normal entry always
            creatureInfo = cinfo;                                 // map mode related always

            // equal to player Race field, but creature does not have race
            SetValue<byte>(UnitFields.Bytes, 0, 0);

            SetValue<byte>(UnitFields.Bytes, (byte)cinfo.UnitClass, 1);

            // Cancel load if no model defined
            if (cinfo.GetFirstValidModelId() == 0)
            {
                Log.outError("Creature (Entry: {0}) has no model defined in table `creature_template`, can't load. ", entry);
                return false;
            }

            uint displayID = Cypher.ObjMgr.ChooseDisplayId(0, GetCreatureTemplate(), data);
            CreatureModelInfo minfo = Cypher.ObjMgr.GetCreatureModelRandomGender(displayID);
            if (minfo == null)                                             // Cancel load if no model defined
            {
                Log.outError("Creature (Entry: {0}) has no model defined in table `creature_template`, can't load. ", entry);
                return false;
            }

            SetDisplayId(displayID);
            SetNativeDisplayId(displayID);
            SetValue<byte>(UnitFields.Bytes, minfo.gender, 2);

            // Load creature equipment
            if (data == null || data.equipmentId == 0)                    // use default from the template
                LoadEquipment(cinfo.EquipmentId);
            else if (data != null && data.equipmentId != -1)               // override, -1 means no equipment
                LoadEquipment((uint)data.equipmentId);

            SetName(normalInfo.Name);                              // at normal entry always

            SetValue<float>(UnitFields.BoundingRadius, 0);//minfo->bounding_radius);
            SetValue<float>(UnitFields.CombatReach, 0);//minfo->combat_reach);

            SetValue<float>(UnitFields.ModCastingSpeed, 1.0f);
            SetValue<float>(UnitFields.ModSpellHaste, 1.0f);

            SetSpeed(UnitMoveType.Walk, cinfo.SpeedWalk);
            SetSpeed(UnitMoveType.Run, cinfo.SpeedRun);
            SetSpeed(UnitMoveType.Swim, 1.0f);      // using 1.0 rate
            SetSpeed(UnitMoveType.Flight, 1.0f);    // using 1.0 rate

            SetObjectScale(cinfo.Scale);

            SetValue<float>(UnitFields.HoverHeight, cinfo.HoverHeight);

            // checked at loading
            //m_defaultMovementType = MovementGeneratorType(cinfo->MovementType);
            //if (!m_respawnradius && m_defaultMovementType == RANDOM_MOTION_TYPE)
                //m_defaultMovementType = IDLE_MOTION_TYPE;

            for (byte i = 0; i < CreatureConst.MaxSpells; ++i)
                m_spells[i] = GetCreatureTemplate().Spells[i];

            return true;
        }
        bool UpdateEntry(uint entry, uint team, CreatureData data)
        {
            if (!InitEntry(entry, team, data))
                return false;

            CreatureTemplate cInfo = GetCreatureTemplate();

            m_regenHealth = cInfo.RegenHealth;

            // creatures always have melee weapon ready if any unless specified otherwise
            //if (!GetCreatureAddon())
                //SetSheath(SHEATH_STATE_MELEE);

            SelectLevel(GetCreatureTemplate());
            if (team == (uint)Team.Horde)
                SetFaction(cInfo.FactionH);
            else
                SetFaction(cInfo.FactionA);

            uint npcflag, unit_flags, dynamicflags;
            Cypher.ObjMgr.ChooseCreatureFlags(cInfo, out npcflag, out unit_flags, out dynamicflags, data);

            if (Convert.ToBoolean(cInfo.FlagsExtra & CreatureFlagsExtra.Worldevent))
                SetValue<uint>(UnitFields.NpcFlags, npcflag);// | sGameEventMgr->GetNPCFlag(this));
            else
                SetValue<uint>(UnitFields.NpcFlags, npcflag);

            SetAttackTime(WeaponAttackType.BaseAttack, cInfo.baseattacktime);
            SetAttackTime(WeaponAttackType.OffAttack, cInfo.baseattacktime);
            SetAttackTime(WeaponAttackType.RangedAttack, cInfo.rangeattacktime);

            SetValue<uint>(UnitFields.Flags, unit_flags);
            SetValue<uint>(UnitFields.Flags2, cInfo.UnitFlags2);

            SetValue<uint>(UnitFields.DynamicFlags, dynamicflags);

            RemoveFlag(UnitFields.Flags, UnitFlags.InCombat);

            //SetMeleeDamageSchool(SpellSchools(cInfo->dmgschool));
            //CreatureBaseStats stats = Globals.ObjMgr.GetCreatureBaseStats(getLevel(), cInfo.UnitClass);
            //float armor = (float)stats.GenerateArmor(cInfo); // TODO: Why is this treated as uint32 when it's a float?
            //SetModifierValue(UNIT_MOD_ARMOR,             BASE_VALUE, armor);
            //SetModifierValue(UNIT_MOD_RESISTANCE_HOLY,   BASE_VALUE, float(cInfo->resistance[SPELL_SCHOOL_HOLY]));
            //SetModifierValue(UNIT_MOD_RESISTANCE_FIRE,   BASE_VALUE, float(cInfo->resistance[SPELL_SCHOOL_FIRE]));
            //SetModifierValue(UNIT_MOD_RESISTANCE_NATURE, BASE_VALUE, float(cInfo->resistance[SPELL_SCHOOL_NATURE]));
            //SetModifierValue(UNIT_MOD_RESISTANCE_FROST,  BASE_VALUE, float(cInfo->resistance[SPELL_SCHOOL_FROST]));
            //SetModifierValue(UNIT_MOD_RESISTANCE_SHADOW, BASE_VALUE, float(cInfo->resistance[SPELL_SCHOOL_SHADOW]));
            //SetModifierValue(UNIT_MOD_RESISTANCE_ARCANE, BASE_VALUE, float(cInfo->resistance[SPELL_SCHOOL_ARCANE]));

            //SetCanModifyStats(true);
            //UpdateAllStats();
            
            // checked and error show at loading templates
            FactionTemplateEntry factionTemplate = DBCStorage.FactionTemplateStorage.LookupByKey(cInfo.FactionA);
            if (factionTemplate != null)
            {
                if (Convert.ToBoolean(factionTemplate.factionFlags & (uint)FactionTemplateFlags.PVP))
                    SetPvP(true);
                else
                    SetPvP(false);
            }

            // trigger creature is always not selectable and can not be attacked
            //if (isTrigger())
                //SetFlag(UnitFields.Flags, UnitFlags.NotSelectable);

            InitializeReactState();

            if (Convert.ToBoolean(cInfo.FlagsExtra & CreatureFlagsExtra.NoTaunt))
            {
                //ApplySpellImmune(0, IMMUNITY_STATE, SPELL_AURA_MOD_TAUNT, true);
                //ApplySpellImmune(0, IMMUNITY_EFFECT, SPELL_EFFECT_ATTACK_ME, true);
            }

            //! Suspect it works this way:
            //! If creature can walk and fly (usually with pathing)
            //! Set MOVEMENTFLAG_CAN_FLY. Otherwise if it can only fly
            //! Set MOVEMENTFLAG_DISABLE_GRAVITY
            //! The only time I saw Movement Flags: DisableGravity, CanFly, Flying (50332672) on the same unit
            //! it was a vehicle
            //if (cInfo.InhabitType & INHABIT_AIR && cInfo.InhabitType & INHABIT_GROUND)
                //SetCanFly(true);
            //else if (cInfo->InhabitType & INHABIT_AIR)
                //SetDisableGravity(true);
            /*! Implemented in LoadCreatureAddon. Suspect there's a rule for UNIT_BYTE_1_FLAG_HOVER
                in relation to DisableGravity also.

            else if (GetByteValue(UNIT_FIELD_BYTES_1, 3) & UNIT_BYTE_1_FLAG_HOVER)
                SetHover(true);

            */

            // TODO: Shouldn't we check whether or not the creature is in water first?
            //if (cInfo.InhabitType & INHABIT_WATER && IsInWater())
                //AddUnitMovementFlag(MOVEMENTFLAG_SWIMMING);

            return true;
        }

        public VendorItemData GetVendorItems()
        {
            return Cypher.ObjMgr.GetNpcVendorItemList(GetEntry());
        }

        public uint GetVendorItemCurrentCount(VendorItem vItem)
        {
            //if (vItem.maxcount == 0)
                return vItem.maxcount;
            /*
            VendorItemCounts::iterator itr = m_vendorItemCounts.begin();
            for (; itr != m_vendorItemCounts.end(); ++itr)
                if (itr->itemId == vItem.item)
                    break;

            if (itr == m_vendorItemCounts.end())
                return vItem->maxcount;

            VendorItemCount vCount = &*itr;

            time_t ptime = time(NULL);

            if (time_t(vCount.lastIncrementTime + vItem.incrtime) <= ptime)
            {
                ItemTemplate pProto = sObjectMgr->GetItemTemplate(vItem.item);

                uint32 diff = uint32((ptime - vCount->lastIncrementTime) / vItem.incrtime);
                if ((vCount.count + diff * pProto.BuyCount) >= vItem.maxcount)
                {
                    m_vendorItemCounts.erase(itr);
                    return vItem.maxcount;
                }

                vCount.count += diff * pProto.BuyCount;
                vCount.lastIncrementTime = ptime;
            }

            return vCount.count;
            */
        }
        void LoadEquipment(uint equip_entry, bool force = false)
        {
            if (equip_entry == 0)
            {
                if (force)
                {
                    for (byte i = 0; i < 3; ++i)
                        SetValue<uint>(UnitFields.VirtualItemID + i, 0);
                    m_equipmentId = 0;
                }
                return;
            }

            EquipmentInfo einfo = Cypher.ObjMgr.GetEquipmentInfo(equip_entry, -1);
            if (einfo == null)
                return;

            m_equipmentId = equip_entry;
            for (byte i = 0; i < 3; ++i)
                SetValue<uint>(UnitFields.VirtualItemID + i, einfo.ItemEntry[i]);
        }

        uint GetOriginalEntry() { return m_originalEntry; }
        void SetOriginalEntry(uint entry) { m_originalEntry = entry; }
        float _GetHealthMod(uint Rank)
        {
            switch ((CreatureEliteType)Rank)                                           // define rates for each elite rank
            {
                case CreatureEliteType.NORMAL:
                    return 1.0f;//sWorld->getRate(RATE_CREATURE_NORMAL_HP);
                case CreatureEliteType.ELITE:
                    return 1.0f;//sWorld->getRate(RATE_CREATURE_ELITE_ELITE_HP);
                case CreatureEliteType.RAREELITE:
                    return 1.0f;//sWorld->getRate(RATE_CREATURE_ELITE_RAREELITE_HP);
                case CreatureEliteType.WORLDBOSS:
                    return 1.0f;//sWorld->getRate(RATE_CREATURE_ELITE_WORLDBOSS_HP);
                case CreatureEliteType.RARE:
                    return 1.0f;//sWorld->getRate(RATE_CREATURE_ELITE_RARE_HP);
                default:
                    return 1.0f;//sWorld->getRate(RATE_CREATURE_ELITE_ELITE_HP);
            }
        }
        void SelectLevel(CreatureTemplate cinfo)
        {
            uint rank = isPet() ? 0 : cinfo.Rank;

            // level
            byte minlevel = (byte)Math.Min(cinfo.Maxlevel, cinfo.Minlevel);
            byte maxlevel = (byte)Math.Max(cinfo.Maxlevel, cinfo.Minlevel);
            byte level = (byte)(minlevel == maxlevel ? minlevel : RandomHelper.irand(minlevel, maxlevel));
            SetLevel(level);

            CreatureBaseStats stats = Cypher.ObjMgr.GetCreatureBaseStats(level, cinfo.UnitClass);

            // health
            float healthmod = _GetHealthMod(rank);

            uint basehp = stats.GenerateHealth(cinfo);
            uint health = (uint)(basehp * healthmod);

            SetCreateHealth(health);
            SetMaxHealth(health);
            SetHealth(health);
            //ResetPlayerDamageReq();

            // mana
            uint mana = stats.GenerateMana(cinfo);
            SetCreateMana(mana);

            switch ((Class)getClass())
            {
                case Class.Warrior:
                    setPowerType(Powers.Rage);
                    break;
                case Class.Rogue:
                    setPowerType(Powers.Energy);
                    break;
                default:
                    setPowerType(Powers.Mana);
                    SetMaxPower(Powers.Mana, (int)mana);
                    SetPower(Powers.Mana, (int)mana);
                    break;
            }

            //SetModifierValue(UNIT_MOD_HEALTH, BASE_VALUE, (float)health);
            //SetModifierValue(UNIT_MOD_MANA, BASE_VALUE, (float)mana);

            //SetBaseWeaponDamage(BASE_ATTACK, MINDAMAGE, cinfo->mindmg);
            //SetBaseWeaponDamage(BASE_ATTACK, MAXDAMAGE, cinfo->maxdmg);

            SetValue<float>(UnitFields.MinRangedDamage, cinfo.MinRangeDmg);
            SetValue<float>(UnitFields.MaxRangedDamage, cinfo.MaxRangeDmg);

            //SetModifierValue(UNIT_MOD_ATTACK_POWER, BASE_VALUE, cinfo->attackpower);
        }
        public void SetHomePosition(float x, float y, float z, float o) { m_homePosition.Relocate(x, y, z, o); }
        void SetHomePosition(ObjectPosition pos) { m_homePosition.Relocate(pos); }
        public void GetHomePosition(out float x, out float y, out float z, out float ori) { m_homePosition.GetPosition(out x, out y, out z, out ori); }
        public ObjectPosition GetHomePosition() { return m_homePosition; }

        public override bool LoadFromDB(uint guid, Map map) { return LoadCreatureFromDB(guid, map, false); }
        bool LoadCreatureFromDB(uint guid, Map map, bool addToMap = true)
        {
            CreatureData data = Cypher.ObjMgr.GetCreatureData(guid);

            if (data == null)
            {
                Log.outError("Creature (GUID: {0}) not found in table `creature`, can't load. ", guid);
                return false;
            }

            m_DBTableGuid = guid;
            if (map.GetInstanceId() == 0)
            {
                //if (map.GetCreature(MAKE_NEW_GUID(guid, data.id, HIGHGUID_UNIT)))
                //return false;
            }
            else
                guid = Cypher.ObjMgr.GenerateLowGuid(HighGuidType.Unit);

            uint team = 0;
            if (!Create(guid, map, data.phaseMask, data.id, 0, team, data.posX, data.posY, data.posZ, data.orientation, data))
                return false;

            //We should set first home position, because then AI calls home movement
            SetHomePosition(data.posX, data.posY, data.posZ, data.orientation);

            m_respawnradius = data.spawndist;

            m_respawnDelay = data.spawntimesecs;
            m_deathState = DeathState.Alive;

            //m_respawnTime = GetMap().GetCreatureRespawnTime(m_DBTableGuid);
            //if (m_respawnTime)                          // respawn on Update
            {
                //m_deathState = DeathState.Dead;
                //if (CanFly())
                {
                    //float tz = map.GetHeight(GetPhaseMask(), data.posX, data.posY, data.posZ, false);
                    //if (data.posZ - tz > 0.1f)
                        //Relocate(data->posX, data->posY, tz);
                }
            }

            uint curhealth;

            if (!m_regenHealth)
            {
                curhealth = data.curhealth;
                if (curhealth != 0)
                {
                    curhealth = (uint)(curhealth * _GetHealthMod(GetCreatureTemplate().Rank));
                    if (curhealth < 1)
                        curhealth = 1;
                }
                SetPower(Powers.Mana, (int)data.curmana);
            }
            else
            {
                curhealth = GetMaxHealth();
                SetPower(Powers.Mana, GetMaxPower(Powers.Mana));
            }

            SetHealth(m_deathState == DeathState.Alive ? curhealth : 0);

            // checked at creature_template loading
            //m_defaultMovementType = MovementGeneratorType(data->movementType);

            creaturedata = data;

            if (addToMap && !GetMap().AddToMap(this))
                return false;
            return true;
        }
        public uint UpdateVendorItemCurrentCount(VendorItem vItem, uint used_count) //todo fixme
        {
            //if (vItem.maxcount == 0)
                return 0;
            /*
            VendorItemCounts::iterator itr = m_vendorItemCounts.begin();
            for (; itr != m_vendorItemCounts.end(); ++itr)
                if (itr->itemId == vItem->item)
                    break;

            if (itr == m_vendorItemCounts.end())
            {
                uint new_count = vItem.maxcount > used_count ? vItem.maxcount - used_count : 0;
                m_vendorItemCounts.push_back(VendorItemCount(vItem.item, new_count));
                return new_count;
            }

            VendorItemCount* vCount = &*itr;

            time_t ptime = time(NULL);

            if (time_t(vCount.lastIncrementTime + vItem.incrtime) <= ptime)
            {
                ItemTemplate pProto = sObjectMgr->GetItemTemplate(vItem.item);

                uint diff = (uint)((ptime - vCount.lastIncrementTime) / vItem.incrtime);
                if ((vCount.count + diff * pProto.BuyCount) < vItem.maxcount)
                    vCount.count += diff * pProto.BuyCount;
                else
                    vCount.count = vItem->maxcount;
            }

            vCount->count = vCount->count > used_count ? vCount->count - used_count : 0;
            vCount->lastIncrementTime = ptime;
            return vCount->count;
            */
        }
        bool AIM_Initialize(CreatureAI ai = null)
        {
            // make sure nothing can change the AI during AI update
            if (m_AI_locked)
            {
                Log.outDebug("AIM_Initialize: failed to init, locked.");
                return false;
            }
            Motion_Initialize();

            i_AI = ai != null ? ai : new CreatureAI(this);//FactorySelector::selectAI(this)
            IsAIEnabled = true;
            //i_AI.InitializeAI();
            // Initialize vehicle
            //if (GetVehicleKit())
            //GetVehicleKit()->Reset();
            return true;
        }
        void Motion_Initialize()
        {
            /*
            if (!m_formation)
                i_motionMaster.Initialize();
            else if (m_formation.getLeader() == this)
            {
                m_formation.FormationReset(false);
                i_motionMaster.Initialize();
            }
            else if (m_formation->isFormed())
                i_motionMaster.MoveIdle(); //wait the order of leader
            else
                */
                i_motionMaster.Initialize();
        }

        public void SetReactState(ReactStates st) { m_reactState = st; }
        public ReactStates GetReactState() { return m_reactState; }
        public bool HasReactState(ReactStates state) { return (m_reactState == state); }
        void InitializeReactState()
        {
            if (isTotem() || isTrigger() || GetCreatureType() == CreatureType.Critter)// || isSpiritService())
                SetReactState(ReactStates.PASSIVE);
            else
                SetReactState(ReactStates.AGGRESSIVE);
            /*else if (isCivilian())
            SetReactState(REACT_DEFENSIVE);*/;
        }

        bool isCivilian() { return Convert.ToBoolean(GetCreatureTemplate().FlagsExtra & CreatureFlagsExtra.Civilian); }
        bool isTrigger() { return Convert.ToBoolean(GetCreatureTemplate().FlagsExtra & CreatureFlagsExtra.Trigger); }
        bool isGuard() { return Convert.ToBoolean(GetCreatureTemplate().FlagsExtra & CreatureFlagsExtra.Guard); }
        bool canWalk() { return Convert.ToBoolean(GetCreatureTemplate().InhabitType & InhabitType.Ground); }
        bool canSwim() { return Convert.ToBoolean(GetCreatureTemplate().InhabitType & InhabitType.Water); }
        public bool CanFly() { return Convert.ToBoolean(GetCreatureTemplate().InhabitType & InhabitType.Air); }

        public bool canStartAttack(Unit who, bool force)
        {
            if (isCivilian())
                return false;

            if (HasFlag(UnitFields.Flags, UnitFlags.ImmuneToNpc))
                return false;

            // Do not attack non-combat pets
            if (who.GetTypeId() == ObjectType.Unit && who.GetCreatureType() == CreatureType.NonCombatPet)
                return false;

            if (!CanFly() && (GetDistanceZ(who) > CreatureConst.AttackRangeZ + m_CombatDistance))
                //|| who->IsControlledByPlayer() && who->IsFlying()))
                // we cannot check flying for other creatures, too much map/vmap calculation
                // TODO: should switch to range attack
                return false;

            if (!force)
            {
                if (!_IsTargetAcceptable(who))
                    return false;

                if (who.isInCombat() && IsWithinDist(who, ObjectConst.AttackDistance))
                {
                    Unit victim = who.getAttackerForHelper();
                    if (victim != null)
                        if (IsWithinDistInMap(victim, WorldConfig.CreatureFamilyAssistanceRadius))
                            force = true;
                }

                if (!force && (IsNeutralToAll() || !IsWithinDistInMap(who, GetAttackDistance(who) + m_CombatDistance)))
                    return false;
            }

            if (!canCreatureAttack(who, force))
                return false;

            return IsWithinLOSInMap(who);
        }

        float GetAttackDistance(Unit player)
        {
            float aggroRate = WorldConfig.CreatureAggroRadius;
            if (aggroRate == 0)
                return 0.0f;

            uint playerlevel = player.GetLevelForTarget(this);
            uint creaturelevel = GetLevelForTarget(player);

            int leveldif = (int)playerlevel - (int)creaturelevel;

            // "The maximum Aggro Radius has a cap of 25 levels under. Example: A level 30 char has the same Aggro Radius of a level 5 char on a level 60 mob."
            if (leveldif < -25)
                leveldif = -25;

            // "The aggro radius of a mob having the same level as the player is roughly 20 yards"
            float RetDistance = 20;

            // "Aggro Radius varies with level difference at a rate of roughly 1 yard/level"
            // radius grow if playlevel < creaturelevel
            RetDistance -= (float)leveldif;

            if (creaturelevel + 5 <= WorldConfig.MaxLevel)
            {
                // detect range auras
                //RetDistance += GetTotalAuraModifier(SPELL_AURA_MOD_DETECT_RANGE);

                // detected range auras
                //RetDistance += player->GetTotalAuraModifier(SPELL_AURA_MOD_DETECTED_RANGE);
            }

            // "Minimum Aggro Radius for a mob seems to be combat range (5 yards)"
            if (RetDistance < 5)
                RetDistance = 5;

            return (RetDistance * aggroRate);
        }

        bool canCreatureAttack(Unit victim, bool force)
        {
            if (!victim.IsInMap(this))
                return false;

            //if (!IsValidAttackTarget(victim))
                //return false;

            //if (!victim.isInAccessiblePlaceFor(this))
                //return false;

            if (IsAIEnabled && !AI().CanAIAttack(victim))
                return false;

            if (DBCStorage.MapStorage.LookupByKey(GetMapId()).IsDungeon())
                return true;

            //Use AttackDistance in distance check if threat radius is lower. This prevents creature bounce in and out of combat every update tick.
            float dist = Math.Max(GetAttackDistance(victim), (WorldConfig.CreatureThreatRadius + m_CombatDistance));

            Unit unit = GetCharmerOrOwner();
            if (unit != null)
                return victim.IsWithinDist(unit, dist);
            else
                return true;// victim.IsInDist(m_homePosition, dist);
        }

        bool _IsTargetAcceptable(Unit target)
        {
            // if the target cannot be attacked, the target is not acceptable
            //if (IsFriendlyTo(target) || !target.isTargetableForAttack(false)
            //    || (m_vehicle && (IsOnVehicle(target) || m_vehicle.GetBase()->IsOnVehicle(target))))
              //  return false;

            if (target.HasUnitState(UnitState.Died))
            {
                // guards can detect fake death
                if (isGuard() && target.HasFlag(UnitFields.Flags2, UnitFlags2.FeignDeath))
                    return true;
                else
                    return false;
            }

            Unit myVictim = getAttackerForHelper();
            Unit targetVictim = target.getAttackerForHelper();

            // if I'm already fighting target, or I'm hostile towards the target, the target is acceptable
            if (myVictim == target || targetVictim == this || IsHostileTo(target))
                return true;

            // if the target's victim is friendly, and the target is neutral, the target is acceptable
            if (targetVictim != null && IsFriendlyTo(targetVictim))
                return true;

            // if the target's victim is not friendly, or the target is friendly, the target is not acceptable
            return false;
        }

        uint GetDBTableGUIDLow() { return m_DBTableGuid; }
        public void GetRespawnPosition(out float x, out float y, out float z, out float ori, out float dist)
        {
            if (m_DBTableGuid != 0)
            {
                CreatureData data = Cypher.ObjMgr.GetCreatureData(GetDBTableGUIDLow());
                if (data != null)
                {
                    x = data.posX;
                    y = data.posY;
                    z = data.posZ;
                    ori = data.orientation;
                    dist = data.spawndist;

                    return;
                }
            }

            x = GetPositionX();
            y = GetPositionY();
            z = GetPositionZ();
            ori = GetOrientation();
            dist = 0;
        }

        Cell _currentCell;
        public Cell GetCurrentCell() { return _currentCell; }
        public void SetCurrentCell(Cell cell) { _currentCell = cell; }

        public float m_SightDistance, m_CombatDistance;


        //old shit

        public CreatureTemplate GetCreatureTemplate() { return creatureInfo; }

        bool isVendor() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.Vendor); }
        bool isTrainer() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.Trainer); }
        bool isQuestGiver() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.QuestGiver); }
        bool isGossip() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.Gossip); }
        bool isTaxi() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.FlightMaster); }
        bool isGuildMaster() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.Petitioner); }
        bool isBattleMaster() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.BattleMaster); }
        bool isBanker() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.Banker); }
        bool isInnkeeper() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.Innkeeper); }
        bool isSpiritHealer() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.SpiritHealer); }
        bool isSpiritGuide() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.SpiritGuide); }
        bool isTabardDesigner() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.TabardDesigner); }
        bool isAuctioner() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.Auctioneer); }
        bool isArmorer() { return HasFlag((int)UnitFields.NpcFlags, NPCFlags.Repair); }

        public bool isTrainerOf(Player player, bool msg)
        {
            if (!isTrainer())
                return false;

            TrainerSpellData trainer_spells = GetTrainerSpells();

            if ((trainer_spells == null || trainer_spells.spellList.Count == 0) && GetCreatureTemplate().TrainerType != TrainerType.Pets)
            {
                //sLog->outError(LOG_FILTER_SQL, "Creature %u (Entry: %u) have UNIT_NPC_FLAG_TRAINER but have empty trainer spell list.",
                //GetGUIDLow(), GetEntry());
                return false;
            }

            switch (GetCreatureTemplate().TrainerType)
            {
                case TrainerType.Class:
                    if (player.GetClass() != GetCreatureTemplate().TrainerClass)
                    {
                        if (msg)
                        {
                            //player->PlayerTalkClass->ClearMenus();
                            switch (GetCreatureTemplate().TrainerClass)
                            {
                                case Class.Druid:  //player->PlayerTalkClass->SendGossipMenu(4913, GetGUID()); break;
                                case Class.Hunter: //player->PlayerTalkClass->SendGossipMenu(10090, GetGUID()); break;
                                case Class.Mage:   //player->PlayerTalkClass->SendGossipMenu(328, GetGUID()); break;
                                case Class.Paladin://player->PlayerTalkClass->SendGossipMenu(1635, GetGUID()); break;
                                case Class.Priest: //player->PlayerTalkClass->SendGossipMenu(4436, GetGUID()); break;
                                case Class.Rogue:  //player->PlayerTalkClass->SendGossipMenu(4797, GetGUID()); break;
                                case Class.Shaman: //player->PlayerTalkClass->SendGossipMenu(5003, GetGUID()); break;
                                case Class.Warlock://player->PlayerTalkClass->SendGossipMenu(5836, GetGUID()); break;
                                case Class.Warrior://player->PlayerTalkClass->SendGossipMenu(4985, GetGUID()); break;
                                    break;
                            }
                        }
                        return false;
                    }
                    break;
                case TrainerType.Pets:
                    if (player.GetClass() != Class.Hunter)
                    {
                        //player->PlayerTalkClass->ClearMenus();
                        //player->PlayerTalkClass->SendGossipMenu(3620, GetGUID());
                        return false;
                    }
                    break;
                case TrainerType.Mounts:
                    if (player.GetRace() != GetCreatureTemplate().TrainerRace)
                    {
                        if (msg)
                        {
                            //player->PlayerTalkClass->ClearMenus();
                            switch (GetCreatureTemplate().TrainerRace)
                            {
                                case Race.Dwarf:        //player->PlayerTalkClass->SendGossipMenu(5865, GetGUID()); break;
                                case Race.Gnome:        //player->PlayerTalkClass->SendGossipMenu(4881, GetGUID()); break;
                                case Race.Human:        //player->PlayerTalkClass->SendGossipMenu(5861, GetGUID()); break;
                                case Race.NightElf:     //player->PlayerTalkClass->SendGossipMenu(5862, GetGUID()); break;
                                case Race.Draenei:      //player->PlayerTalkClass->SendGossipMenu(5864, GetGUID()); break;
                                case Race.Worgen:
                                case Race.Orc:          //player->PlayerTalkClass->SendGossipMenu(5863, GetGUID()); break;
                                case Race.Tauren:       //player->PlayerTalkClass->SendGossipMenu(5864, GetGUID()); break;
                                case Race.Troll:        //player->PlayerTalkClass->SendGossipMenu(5816, GetGUID()); break;
                                case Race.Undead://player->PlayerTalkClass->SendGossipMenu(624, GetGUID()); break;
                                case Race.BloodElf:     //player->PlayerTalkClass->SendGossipMenu(5862, GetGUID()); break;
                                case Race.Goblin:
                                    break;

                            }
                        }
                        return false;
                    }
                    break;
                case TrainerType.Tradeskills:
                    if (GetCreatureTemplate().TrainerSpell != 0 && !player.HasSpell(GetCreatureTemplate().TrainerSpell))
                    {
                        if (msg)
                        {
                            //player->PlayerTalkClass->ClearMenus();
                            //player->PlayerTalkClass->SendGossipMenu(11031, GetGUID());
                        }
                        return false;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        public TrainerSpellData GetTrainerSpells() { return Cypher.ObjMgr.GetNpcTrainerSpells(GetEntry()); }

        #region Fields
        CreatureTemplate creatureInfo;
        CreatureData creaturedata;
        ObjectPosition m_homePosition;
        uint[] m_spells = new uint[CreatureConst.MaxSpells];
        bool m_AI_locked;
        public bool IsAIEnabled;

        /// Timers
        uint m_corpseRemoveTime;                          // (msecs)timer for death or corpse disappearance
        uint m_respawnTime;                               // (secs) time of next respawn
        uint m_respawnDelay;                              // (secs) delay between corpse disappearance and respawning
        uint m_corpseDelay;                               // (secs) delay between death and corpse disappearance
        float m_respawnradius;

        uint m_originalEntry;
        bool m_regenHealth;
        uint m_DBTableGuid;
        uint m_equipmentId;

        ReactStates m_reactState;                           // for AI, not charmInfo

        #endregion
    }

    //Creature Classes
    public class CreatureTemplate
    {
        public uint Entry;
        public uint[] DifficultyEntry = new uint[3];
        public uint[] KillCredit = new uint[2];
        public uint[] ModelId = new uint[4];
        public string Name;
        public string SubName;
        public string IconName;
        public uint GossipMenuId;
        public uint Minlevel;
        public uint Maxlevel;
        public uint expansion;
        public uint expansionUnk;
        public uint FactionA;
        public uint FactionH;
        public NPCFlags Npcflag;
        public float SpeedWalk;
        public float SpeedRun;
        public float Scale;
        public uint Rank;
        public float Mindmg;
        public float Maxdmg;
        public uint DmgSchool;
        public uint AttackPower;
        public float DmgMultiplier;
        public uint baseattacktime;
        public uint rangeattacktime;
        public uint UnitClass;
        public uint UnitFlags;
        public uint UnitFlags2;
        public uint DynamicFlags;
        public uint Family;
        public TrainerType TrainerType;
        public uint TrainerSpell;
        public Class TrainerClass;
        public Race TrainerRace;
        public float MinRangeDmg;
        public float MaxRangeDmg;
        public uint RangedAttackPower;
        public uint CreatureType;
        public CreatureTypeFlags TypeFlags;
        public uint TypeFlags2;
        public uint LootId;
        public uint PickPocketId;
        public uint SkinLootId;
        public int[] Resistance = new int[7];
        public uint[] Spells = new uint[8];
        public uint PetSpellDataId;
        public uint VehicleId;
        public uint MinGold;
        public uint MaxGold;
        public string AIName;
        public uint MovementType;
        public InhabitType InhabitType;
        public float HoverHeight;
        public float HeathMod;
        public float ManaMod;
        public float ManaExtraMod;
        public float ArmorMod;
        public bool RacialLeader;
        public uint[] QuestItems = new uint[6];
        public uint MovementId;
        public bool RegenHealth;
        public uint EquipmentId;
        public uint MechanicImmuneMask;
        public CreatureFlagsExtra FlagsExtra;
        public uint ScriptID;

        public uint GetRandomValidModelId()
        {
            byte c = 0;
            uint[] modelIDs = new uint[4];
            
            if (ModelId[0] != 0)
                modelIDs[c++] = ModelId[0];
            if (ModelId[1] != 0)
                modelIDs[c++] = ModelId[1];
            if (ModelId[2] != 0)
                modelIDs[c++] = ModelId[2];
            if (ModelId[3] != 0)
                modelIDs[c++] = ModelId[3];
            
            return c > 0 ? modelIDs[RandomHelper.irand(0, c-1)] : 0;
        }
        public uint GetFirstValidModelId()
        {
            if (ModelId[0] != 0)
                return ModelId[0];
            if (ModelId[1] != 0)
                return ModelId[1];
            if (ModelId[2] != 0)
                return ModelId[2];
            if (ModelId[3] != 0)
                return ModelId[3];
            return 0;
        }
    }
    public class CreatureData
    {
        public uint id;                                           // entry in creature_template
        public uint mapid;
        public uint phaseMask;
        public uint displayid;
        public int equipmentId;
        public float posX;
        public float posY;
        public float posZ;
        public float orientation;
        public uint spawntimesecs;
        public float spawndist;
        public uint currentwaypoint;
        public uint curhealth;
        public uint curmana;
        public byte movementType;
        public byte spawnMask;
        public uint npcflag;
        public uint unit_flags;                          // enum UnitFlags mask values
        public uint dynamicflags;
        public bool dbData;
    }
    public struct CreatureAddon
    {
        public uint path_id;
        public uint mount;
        public uint bytes1;
        public uint bytes2;
        public uint emote;
        public uint[] auras;
    }
    public class CreatureModelInfo
    {
        public float bounding_radius;
        public float combat_reach;
        public byte gender;
        public uint modelid_other_gender;
    }
    public class CreatureBaseStats
    {
        public uint[] BaseHealth = new uint[CreatureConst.MaxBaseHp];
        public uint BaseMana;
        public uint BaseArmor;

        // Helpers
        public uint GenerateHealth(CreatureTemplate info)
        {
            return (uint)((BaseHealth[info.expansion] * info.HeathMod) + 0.5f);
        }

        public uint GenerateMana(CreatureTemplate info)
        {
            // Mana can be 0.
            if (BaseMana == 0)
                return 0;

            return (uint)((BaseMana * info.ManaMod * info.ManaExtraMod) + 0.5f);
        }

        public uint GenerateArmor(CreatureTemplate info)
        {
            return (uint)((BaseArmor * info.ArmorMod) + 0.5f);
        }

        //static CreatureBaseStats GetBaseStats(uint8 level, uint8 unitClass);
    }
    public class DefaultCreatureBaseStats : CreatureBaseStats
    {
        public DefaultCreatureBaseStats()
        {
            BaseArmor = 1;
            for (byte j = 0; j < 4; ++j)
                BaseHealth[j] = 1;
            BaseMana = 0;
        }
    }
    public class EquipmentInfo
    {
        public uint[] ItemEntry = new uint[CreatureConst.MaxEquipmentItems];
    }
    public class VendorItem
    {
        public VendorItem(uint _item, uint _maxcount, uint _incrtime, uint _ExtendedCost, byte _Type)
        {
            item = _item;
            maxcount = _maxcount;
            incrtime = _incrtime;
            ExtendedCost = _ExtendedCost;
            Type = _Type; 
        }
        
        public uint item;
        public uint maxcount;                                        // 0 for infinity item amount
        public uint incrtime;                                        // time for restore items amount if maxcount != 0
        public uint ExtendedCost;
        public byte  Type;

        //helpers
        public bool IsGoldRequired(ItemTemplate pProto) { return Convert.ToBoolean(pProto.Flags2 & ItemFlags2.ExtCostRequiresGold) || ExtendedCost == 0; }
    }
    public class VendorItemData
    {
        List<VendorItem> m_items = new List<VendorItem>();

        public VendorItem GetItem(uint slot)
        {
            if (slot >= m_items.Count)
                return null;

            return m_items[(int)slot];
        }
        public bool Empty() { return m_items.Count == 0; }
        public int GetItemCount() { return m_items.Count; }
        public void AddItem(int item, uint maxcount, uint ptime, uint ExtendedCost, byte type)
        {
            m_items.Add(new VendorItem((uint)item, maxcount, ptime, ExtendedCost, type));
        }
        public bool RemoveItem(uint item_id, byte type)
        {
            int i = m_items.RemoveAll(p => p.Type == type && p.item == item_id);
            if (i == 0)
                return false;
            else
                return true;
        }
        public VendorItem FindItemCostPair(uint item_id, uint extendedCost, byte type)
        {
            return m_items.Find(p => p.item == item_id && p.ExtendedCost == extendedCost && p.Type == type);
        }
        public void Clear() { m_items.Clear(); }
    }
    public class TrainerSpell
    {
        public uint spellId;
        public uint spellCost;
        public uint reqSkill;
        public uint reqSkillValue;
        public uint reqLevel;
        public uint[] learnedSpell = new uint[3];

        // helpers
        public bool IsCastable() { return learnedSpell[0] != spellId; }
    }
    public class TrainerSpellData
    {
        public TrainerSpellData()
        {
            trainerType = 0;
        }
        ~TrainerSpellData() { spellList.Clear(); }

        public Dictionary<uint, TrainerSpell> spellList = new Dictionary<uint,TrainerSpell>();
        public uint trainerType;                                     // trainer type based at trainer spells, can be different from creature_template value.
        // req. for correct show non-prof. trainers like weaponmaster, allowed values 0 and 2.

        public TrainerSpell Find(uint spell_id)
        {
            return spellList.LookupByKey(spell_id);
        }
    }

    public struct VendorItemCount
    {
        uint itemId;
        uint count;
        uint lastIncrementTime;
    }
}
