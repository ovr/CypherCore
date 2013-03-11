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
using System.Text.RegularExpressions;
using Framework.Configuration;
using Framework.Constants;
using Framework.Database;
using Framework.DataStorage;
using Framework.Logging;
using Framework.Network;
using Framework.Utility;
using WorldServer.Game.Managers;
using WorldServer.Game.Maps;
using WorldServer.Game.Packets;
using WorldServer.Game.Spells;
using WorldServer.Network;
using Framework.ObjectDefines;

namespace WorldServer.Game.WorldEntities
{
    public class Player : Unit
    {
        public override void Update(uint p_time)
        {
            if (!IsInWorld)
                return;
            /*
                // undelivered mail
                if (m_nextMailDelivereTime && m_nextMailDelivereTime <= time(NULL))
                {
                    SendNewMail();
                    ++unReadMails;

                    // It will be recalculate at mailbox open (for unReadMails important non-0 until mailbox open, it also will be recalculated)
                    m_nextMailDelivereTime = 0;
                }

                // If this is set during update SetSpellModTakingSpell call is missing somewhere in the code
                // Having this would prevent more aura charges to be dropped, so let's crash
                if (m_spellModTakingSpell)
                {
                    sLog->outFatal(LOG_FILTER_SPELLS_AURAS, "Player has m_spellModTakingSpell %u during update!", m_spellModTakingSpell->m_spellInfo->Id);
                    m_spellModTakingSpell = NULL;
                }
                        */
            //used to implement delayed far teleports
            //SetCanDelayTeleport(true);
            base.Update(p_time);
            //SetCanDelayTeleport(false);

            long now = Time.UnixTime;

            //UpdatePvPFlag(now);

            //UpdateContestedPvP(p_time);

            //UpdateDuelFlag(now);

            //CheckDuelDistance(now);

            //UpdateAfkReport(now);

            //if (isCharmed())
            //if (Unit* charmer = GetCharmer())
            // if (charmer->GetTypeId() == TYPEID_UNIT && charmer->isAlive())
            //UpdateCharmedAI();

            // Update items that have just a limited lifetime
            //if (now > m_Last_tick)
            //UpdateItemDuration(uint32(now - m_Last_tick));

            // check every second
            //if (now > m_Last_tick + 1)
            //UpdateSoulboundTradeItems();
            /*
                // If mute expired, remove it from the DB
                if (GetSession()->m_muteTime && GetSession()->m_muteTime < now)
                {
                    GetSession()->m_muteTime = 0;
                    PreparedStatement* stmt = LoginDatabase.GetPreparedStatement(LOGIN_UPD_MUTE_TIME);
                    stmt->setInt64(0, 0); // Set the mute time to 0
                    stmt->setString(1, "");
                    stmt->setString(2, "");
                    stmt->setUInt32(3, GetSession()->GetAccountId());
                    LoginDatabase.Execute(stmt);
                }

                if (!m_timedquests.empty())
                {
                    QuestSet::iterator iter = m_timedquests.begin();
                    while (iter != m_timedquests.end())
                    {
                        QuestStatusData& q_status = m_QuestStatus[*iter];
                        if (q_status.Timer <= p_time)
                        {
                            uint32 quest_id  = *iter;
                            ++iter;                                     // current iter will be removed in FailQuest
                            FailQuest(quest_id);
                        }
                        else
                        {
                            q_status.Timer -= p_time;
                            m_QuestStatusSave[*iter] = true;
                            ++iter;
                        }
                    }
                }

                m_achievementMgr->UpdateTimedAchievements(p_time);
                        */
            if (HasUnitState(UnitState.Melee_Attacking) && !HasUnitState(UnitState.Casting))
            {
                Unit victim = getVictim();
                if (victim != null)
                {
                    // default combat reach 10
                    // TODO add weapon, skill check

                    if (isAttackReady(WeaponAttackType.BaseAttack))
                    {
                        if (!IsWithinMeleeRange(victim))
                        {
                            setAttackTimer(WeaponAttackType.BaseAttack, 100);
                            if (m_swingErrorMsg != 1)               // send single time (client auto repeat)
                            {
                                SendAttackSwingNotInRange();
                                m_swingErrorMsg = 1;
                            }
                        }
                        //120 degrees of radiant range
                        else if (!HasInArc((float)(2 * Math.PI / 3), victim))
                        {
                            setAttackTimer(WeaponAttackType.BaseAttack, 100);
                            if (m_swingErrorMsg != 2)               // send single time (client auto repeat)
                            {
                                SendAttackSwingBadFacingAttack();
                                m_swingErrorMsg = 2;
                            }
                        }
                        else
                        {
                            m_swingErrorMsg = 0;                    // reset swing error state

                            // prevent base and off attack in same time, delay attack at 0.2 sec
                            if (haveOffhandWeapon())
                                if (getAttackTimer(WeaponAttackType.OffAttack) < 200)
                                    setAttackTimer(WeaponAttackType.OffAttack, 200);

                            // do attack
                            AttackerStateUpdate(victim, WeaponAttackType.BaseAttack);
                            resetAttackTimer(WeaponAttackType.BaseAttack);
                        }
                    }

                    if (haveOffhandWeapon() && isAttackReady(WeaponAttackType.OffAttack))
                    {
                        if (!IsWithinMeleeRange(victim))
                            setAttackTimer(WeaponAttackType.OffAttack, 100);
                        else if (!HasInArc((float)(2 * Math.PI / 3), victim))
                            setAttackTimer(WeaponAttackType.OffAttack, 100);
                        else
                        {
                            // prevent base and off attack in same time, delay attack at 0.2 sec
                            if (getAttackTimer(WeaponAttackType.BaseAttack) < 200)
                                setAttackTimer(WeaponAttackType.BaseAttack, 200);

                            // do attack
                            AttackerStateUpdate(victim, WeaponAttackType.OffAttack);
                            resetAttackTimer(WeaponAttackType.OffAttack);
                        }
                    }

                    /*Unit* owner = victim->GetOwner();
                    Unit* u = owner ? owner : victim;
                    if (u->IsPvP() && (!duel || duel->opponent != u))
                    {
                        UpdatePvP(true);
                        RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_ENTER_PVP_COMBAT);
                    }*/
                }
            }

            if (HasFlag(PlayerFields.PlayerFlags, PlayerFlags.Resting))
            {
                /*
                if (roll_chance_i(3) && GetTimeInnEnter() > 0)      // freeze update
                {
                    time_t time_inn = time(NULL)-GetTimeInnEnter();
                    if (time_inn >= 10)                             // freeze update
                    {
                        float bubble = 0.125f*sWorld->getRate(RATE_REST_INGAME);
                                                                    // speed collect rest bonus (section/in hour)
                        SetRestBonus(GetRestBonus()+ time_inn*((float)GetUInt32Value(PLAYER_NEXT_LEVEL_XP)/72000)*bubble);
                        UpdateInnerTime(time(NULL));
                    }
                }
                */
            }
            /*
                if (m_weaponChangeTimer > 0)
                {
                    if (p_time >= m_weaponChangeTimer)
                        m_weaponChangeTimer = 0;
                    else
                        m_weaponChangeTimer -= p_time;
                }

                if (m_zoneUpdateTimer > 0)
                {
                    if (p_time >= m_zoneUpdateTimer)
                    {
                        uint32 newzone, newarea;
                        GetZoneAndAreaId(newzone, newarea);

                        if (m_zoneUpdateId != newzone)
                            UpdateZone(newzone, newarea);                // also update area
                        else
                        {
                            // use area updates as well
                            // needed for free far all arenas for example
                            if (m_areaUpdateId != newarea)
                                UpdateArea(newarea);

                            m_zoneUpdateTimer = ZONE_UPDATE_INTERVAL;
                        }
                    }
                    else
                        m_zoneUpdateTimer -= p_time;
                }

                if (m_timeSyncTimer > 0)
                {
                    if (p_time >= m_timeSyncTimer)
                        SendTimeSync();
                    else
                        m_timeSyncTimer -= p_time;
                }
            */
            if (isAlive())
            {
                //m_regenTimer += p_time;
                //RegenerateAll();
            }

            if (m_deathState == DeathState.Dead)
                KillPlayer();
            /*
                if (m_nextSave > 0)
                {
                    if (p_time >= m_nextSave)
                    {
                        // m_nextSave reset in SaveToDB call
                        sScriptMgr->OnPlayerSave(this);
                        SaveToDB();
                        sLog->outDebug(LOG_FILTER_PLAYER, "Player '%s' (GUID: %u) saved", GetName().c_str(), GetGUIDLow());
                    }
                    else
                        m_nextSave -= p_time;
                }

                //Handle Water/drowning
                HandleDrowning(p_time);

                // Played time
                if (now > m_Last_tick)
                {
                    uint32 elapsed = uint32(now - m_Last_tick);
                    m_Played_time[PLAYED_TIME_TOTAL] += elapsed;        // Total played time
                    m_Played_time[PLAYED_TIME_LEVEL] += elapsed;        // Level played time
                    m_Last_tick = now;
                }

                if (GetDrunkValue())
                {
                    m_drunkTimer += p_time;
                    if (m_drunkTimer > 9 * IN_MILLISECONDS)
                        HandleSobering();
                }

                if (HasPendingBind())
                {
                    if (_pendingBindTimer <= p_time)
                    {
                        // Player left the instance
                        if (_pendingBindId == GetInstanceId())
                            BindToInstance();
                        SetPendingBind(0, 0);
                    }
                    else
                        _pendingBindTimer -= p_time;
                }

                // not auto-free ghost from body in instances
                if (m_deathTimer > 0 && !GetBaseMap()->Instanceable() && !HasAuraType(SPELL_AURA_PREVENT_RESURRECTION))
                {
                    if (p_time >= m_deathTimer)
                    {
                        m_deathTimer = 0;
                        BuildPlayerRepop();
                        RepopAtGraveyard();
                    }
                    else
                        m_deathTimer -= p_time;
                }
            */
            //UpdateEnchantTime(p_time);
            //UpdateHomebindTime(p_time);
            /*
                if (!_instanceResetTimes.empty())
                {
                    for (InstanceTimeMap::iterator itr = _instanceResetTimes.begin(); itr != _instanceResetTimes.end();)
                    {
                        if (itr->second < now)
                            _instanceResetTimes.erase(itr++);
                        else
                            ++itr;
                    }
                }
              
            // group update
            SendUpdateToOutOfRangeGroupMembers();

            Pet pet = GetPet();
            if (pet && !pet->IsWithinDistInMap(this, GetMap()->GetVisibilityRange()) && !pet->isPossessed())
                //if (pet && !pet->IsWithinDistInMap(this, GetMap()->GetVisibilityDistance()) && (GetCharmGUID() && (pet->GetGUID() != GetCharmGUID())))
                RemovePet(pet, PET_SAVE_NOT_IN_SLOT, true);

            //we should execute delayed teleports only for alive(!) players
            //because we don't want player's ghost teleported from graveyard
            if (IsHasDelayedTeleport() && isAlive())
                TeleportTo(m_teleport_dest, m_teleport_options);
            */
        }

        uint m_deathTimer;
        void KillPlayer()
        {
            //if (IsFlying() && !GetTransport())
                //i_motionMaster.MoveFall();

            //SetRooted(true);

            //StopMirrorTimers();                                     //disable timers(bars)

            //setDeathState(CORPSE);

            SetValue<uint>(UnitFields.DynamicFlags, 0);// UInt32Value(UNIT_DYNAMIC_FLAGS, UNIT_DYNFLAG_NONE);
            //ApplyModFlag(PlayerFields.Bytes, PLAYER_FIELD_BYTE_RELEASE_TIMER, !sMapStore.LookupEntry(GetMapId())->Instanceable() && !HasAuraType(SPELL_AURA_PREVENT_RESURRECTION));

            // 6 minutes until repop at graveyard
            m_deathTimer = 6 * TimeConst.MINUTE * TimeConst.IN_MILLISECONDS;

            //UpdateCorpseReclaimDelay();                             // dependent at use SetDeathPvP() call before kill
            //SendCorpseReclaimDelay();

            // don't create corpse at this moment, player might be falling

            // update visibility
            UpdateObjectVisibility();
        }
        byte m_swingErrorMsg;


        void SendAttackSwingNotInRange()
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_AttackswingNotinrange);
            GetSession().Send(data);
        }
        void SendAttackSwingBadFacingAttack()
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_AttackswingBadfacing);
            GetSession().Send(data);
        }

        public void CalculateMinMaxDamage(WeaponAttackType attType, bool normalized, bool addTotalPct, out float min_damage, out float max_damage)
        {
            UnitMods unitMod;

            switch (attType)
            {
                case WeaponAttackType.BaseAttack:
                default:
                    unitMod = UnitMods.DAMAGE_MAINHAND;
                    break;
                case WeaponAttackType.OffAttack:
                    unitMod = UnitMods.DAMAGE_OFFHAND;
                    break;
                case WeaponAttackType.RangedAttack:
                    unitMod = UnitMods.DAMAGE_RANGED;
                    break;
            }

            float att_speed = GetAPMultiplier(attType, normalized);

            float base_value = GetModifierValue(unitMod, UnitModifierType.BASE_VALUE) + GetTotalAttackPowerValue(attType) / 14.0f * att_speed;
            float base_pct = GetModifierValue(unitMod, UnitModifierType.BASE_PCT);
            float total_value = GetModifierValue(unitMod, UnitModifierType.TOTAL_VALUE);
            float total_pct = addTotalPct ? GetModifierValue(unitMod, UnitModifierType.TOTAL_PCT) : 1.0f;

            float weapon_mindamage = GetWeaponDamageRange(attType, WeaponDamageRange.MINDAMAGE);
            float weapon_maxdamage = GetWeaponDamageRange(attType, WeaponDamageRange.MAXDAMAGE);
            /*
            if (IsInFeralForm())                                    //check if player is druid and in cat or bear forms
            {
                float weaponSpeed = UnitConst.BASE_ATTACK_TIME / 1000.0f;
                if (Item * weapon = GetWeaponForAttack(BASE_ATTACK, true))
                    weaponSpeed = weapon->GetTemplate()->Delay / 1000;

                if (GetShapeshiftForm() == FORM_CAT)
                {
                    weapon_mindamage = weapon_mindamage / weaponSpeed;
                    weapon_maxdamage = weapon_maxdamage / weaponSpeed;
                }
                else if (GetShapeshiftForm() == FORM_BEAR)
                {
                    weapon_mindamage = weapon_mindamage / weaponSpeed + weapon_mindamage / 2.5;
                    weapon_maxdamage = weapon_mindamage / weaponSpeed + weapon_maxdamage / 2.5;
                }
            }
            else*/ if (!CanUseAttackType(attType))      //check if player not in form but still can't use (disarm case)
            {
                //cannot use ranged/off attack, set values to 0
                if (attType != WeaponAttackType.BaseAttack)
                {
                    min_damage = 0;
                    max_damage = 0;
                    return;
                }
                weapon_mindamage = UnitConst.BASE_MINDAMAGE;
                weapon_maxdamage = UnitConst.BASE_MAXDAMAGE;
            }
            /*
            TODO: Is this still needed after ammo has been removed?
            else if (attType == RANGED_ATTACK)                       //add ammo DPS to ranged damage
            {
                weapon_mindamage += ammo * att_speed;
                weapon_maxdamage += ammo * att_speed;
            }*/

            min_damage = ((base_value + weapon_mindamage) * base_pct + total_value) * total_pct;
            max_damage = ((base_value + weapon_maxdamage) * base_pct + total_value) * total_pct;
        }



        public WorldObject GetViewpoint()
        {
            ulong guid = GetValue<ulong>(PlayerFields.FarsightObject);
            if (guid != 0)
                return (WorldObject)Cypher.ObjMgr.GetObjectByTypeMask(this, guid, HighGuidMask.Seer);
            return null;
        }
        public Corpse GetCorpse()
        {
            return Cypher.ObjMgr.GetCorpseForPlayerGUID(GetGUID());
        }




        int GetPlayerSkillIndex(int x)
        {
            return (int)PlayerFields.Skill + (x);
        }
        int GetPlayerSkillValueIndex(int x)
        {
            return (GetPlayerSkillIndex(x) + 112) + 1;
        }
        int GetPlayerSkillBonusIndex(int x)
        {
            return (GetPlayerSkillIndex(x) + 112) + 2;
        }
        uint SkillValue(uint x)
        {
            return Pair32_LoPart((int)x);
        }
        uint SkillMax(uint x)
        {
            return Pair32_HiPart((int)x);
        }
        uint MakeSkillValue(uint val, uint max)
        {
            return MakePair32(val, max);
        }

        short GetSkillTempBonus(int x)
        {
            return (short)Pair32_LoPart(x);
        }
        short GetSkillPermBonus(int x)
        {
            return (short)Pair32_HiPart(x);
        }
        uint MakrSkillBonus(uint t, uint p)
        {
            return MakePair32(t, p);
        }

        public Player(ref WorldSession _session) : base(true)
        {
            objectTypeMask |= HighGuidMask.Player;
            objectTypeId = ObjectType.Player;
            SetValuesCount((int)PlayerFields.End);

            SetSession(_session);

            reputationMgr = new ReputationMgr(this);
            m_seer = this;
            //achievementMgr = new AchievementMgr(this)
            m_clientGUIDs = new List<ulong>();
        }

        #region General

        //Team
        Team TeamForRace(byte race)
        {
            var rEntry = DBCStorage.CharRaceStorage.LookupByKey(race);
            if (rEntry != null)
            {
                switch (rEntry.TeamID)
                {
                    case 1:
                        return Team.Horde;
                    case 7:
                        return Team.Alliance;
                    case 42:
                        return Team.TeamOther;
                }
                Log.outError("Race ({0}) has wrong teamid ({1}) in DBC: wrong DBC files?", race, rEntry.TeamID);
            }
            else
            Log.outError("Race ({0}) not found in DBC: wrong DBC files?", race);

            return Team.Alliance;
        }
        public Team GetTeam() { return team; }
        TeamId GetTeamId() { return team == Team.Alliance ? TeamId.Alliance : TeamId.Horde; }

        //Money
        public ulong GetMoney() { return GetValue<ulong>(PlayerFields.Coinage); }
        public bool HasEnoughMoney(ulong amount) { return GetMoney() >= amount; }
        public void ModifyMoney(long d)
        {
            //sScriptMgr->OnPlayerMoneyChanged(this, d);

            if (d < 0)
                SetMoney((ulong)(GetMoney() > (ulong)-d ? (long)GetMoney() + d : 0));
            else
            {
                ulong newAmount = 0;
                if (GetMoney() < (ulong)(PlayerConst.MaxMoneyAmount - d))
                    newAmount = (ulong)((long)GetMoney() + d);
                else
                {
                    // "At Gold Limit"
                    newAmount = PlayerConst.MaxMoneyAmount;
                    if (d > 0)
                        SendEquipError(InventoryResult.TooMuchGold, null, null);
                }
                SetMoney(newAmount);
            }
        }
        public void SetMoney(ulong value)
        {
            SetValue<ulong>(PlayerFields.Coinage, value);
            //MoneyChanged(value);
            //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_HIGHEST_GOLD_VALUE_OWNED);
        }

        //Target
        public ulong GetSelection() { return GetValue<ulong>(UnitFields.Target); }
        public T GetSelection<T>() where T : WorldObject 
        { 
            var blah = GetSelection(); 
            return (T)Cypher.ObjMgr.GetObject<T>(this, GetSelection());

        }

        //LoginFlag
        public bool HasAtLoginFlag(AtLoginFlags f) { return Convert.ToBoolean(atLoginFlags & f); }
        public void SetAtLoginFlag(AtLoginFlags f) { atLoginFlags |= f; }
        public void RemoveAtLoginFlag(AtLoginFlags flags, bool persist = false)
        {
            atLoginFlags &= ~flags;
            if (persist)
            {
                PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_UPD_REM_AT_LOGIN_FLAG);
                stmt.AddValue(0, (ushort)flags);
                stmt.AddValue(1, GetGUIDLow());

                DB.Characters.Execute(stmt);
            }
        }
        public bool isGameMaster() { return Convert.ToBoolean(m_ExtraFlags & PlayerExtraFlags.GM_On); }
        void SetFactionForRace(byte race)
        {
            team = TeamForRace(race);

            var rEntry = DBCStorage.CharRaceStorage.LookupByKey(race);
            SetFaction(rEntry.FactionID);
        }
        public void SetInGuild(ulong guid = 0)
        {
            GuildGuid = new ObjectGuid(guid);
            Guild guild = Cypher.GuildMgr.GetGuildByGuid(guid);
            if (guild == null)
            {
                SetValue<UInt64>(ObjectFields.Data, 0);
                SetValue<Int32>(ObjectFields.Type, GetValue<int>(ObjectFields.Type) & ~0x00010000);
            }
            else
            {
                SetValue<UInt64>(ObjectFields.Data, GuildGuid);
                SetValue<Int32>(ObjectFields.Type, GetValue<int>(ObjectFields.Type) | 0x00010000);
                SetValue<UInt32>(PlayerFields.GuildLevel, 0);
                SetFlag(PlayerFields.PlayerFlags, (uint)PlayerFlags.GuildLevelEnabled);
                PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.Updguildid);
                stmt.AddValue(0, guild.Guid);
                stmt.AddValue(1, GetGUIDLow());
                DB.Characters.Execute(stmt);
                //"UPDATE characters SET GuildGuid = {0} WHERE guid = {1}", guild.Guid, pl.Guid);
            }

        }
        public void SetGuildInvited(ulong guid = 0) { GuildInvited = guid; }
        public void SetGuildRank(uint rankId) { SetValue<UInt32>(PlayerFields.GuildRankID, rankId); }
        public void SetGuildLevel(uint level) { SetValue<UInt32>(PlayerFields.GuildLevel, level); }
        public void SendMovementSetCanFly(bool apply)
        {
            ObjectGuid guid = new ObjectGuid(GetGUID());
            PacketWriter data;
            if (apply)
            {
                data = new PacketWriter(Opcodes.SMSG_MoveSetCanFly);
                
                data.WriteBit(guid[1]);
                data.WriteBit(guid[6]);
                data.WriteBit(guid[5]);
                data.WriteBit(guid[0]);
                data.WriteBit(guid[7]);
                data.WriteBit(guid[4]);
                data.WriteBit(guid[2]);
                data.WriteBit(guid[3]);

                data.WriteByteSeq(guid[6]);
                data.WriteByteSeq(guid[3]);
                data.WriteUInt32(0);          //! movement counter
                data.WriteByteSeq(guid[2]);
                data.WriteByteSeq(guid[1]);
                data.WriteByteSeq(guid[4]);
                data.WriteByteSeq(guid[7]);
                data.WriteByteSeq(guid[0]);
                data.WriteByteSeq(guid[5]);
            }
            else
            {
                data = new PacketWriter(Opcodes.SMSG_MoveUnsetCanFly);
                data.WriteBit(guid[1]);
                data.WriteBit(guid[4]);
                data.WriteBit(guid[2]);
                data.WriteBit(guid[5]);
                data.WriteBit(guid[0]);
                data.WriteBit(guid[3]);
                data.WriteBit(guid[6]);
                data.WriteBit(guid[7]);

                data.WriteByteSeq(guid[4]);
                data.WriteByteSeq(guid[6]);
                data.WriteUInt32(0);           //! movement counter
                data.WriteByteSeq(guid[1]);
                data.WriteByteSeq(guid[0]);
                data.WriteByteSeq(guid[2]);
                data.WriteByteSeq(guid[3]);
                data.WriteByteSeq(guid[5]);
                data.WriteByteSeq(guid[7]);
            }
            GetSession().Send(data);
        }
        public void SetFreePrimaryProfessions(uint profs) { SetValue<uint>(PlayerFields.CharacterPoints, profs); }
        public void SetSelection(ulong guid) { SetValue<ulong>(UnitFields.Target, guid); }
        public void GiveLevel(uint level)
        {
            var oldLevel = getLevel();
            if (level == oldLevel)
                return;

            //Level = (byte)level;
            /*
            //PlayerLevelInfo info;
            //sObjectMgr.GetPlayerLevelInfo(getRace(), getClass(), level, &info);

            //uint32 basehp = 0, basemana = 0;
            //sObjectMgr.GetPlayerClassLevelInfo(getClass(), level, basehp, basemana);

            // send levelup info to client
            WorldPacket data(SMSG_LEVELUP_INFO, (4+4+MAX_POWERS_PER_CLASS*4+MAX_STATS*4));
            data << uint32(level);
            data << uint32(int32(basehp) - int32(GetCreateHealth()));
            // for (int i = 0; i < MAX_STORED_POWERS; ++i)          // Powers loop (0-10)
            data << uint32(int32(basemana)   - int32(GetCreateMana()));
            data << uint32(0);
            data << uint32(0);
            data << uint32(0);
            data << uint32(0);
            // end for
            for (uint8 i = STAT_STRENGTH; i < MAX_STATS; ++i)       // Stats loop (0-4)
                data << uint32(int32(info.stats[i]) - GetCreateStat(Stats(i)));

            GetSession().SendPacket(&data);

            SetUInt32Value(PLAYER_NEXT_LEVEL_XP, sObjectMgr.GetXPForLevel(level));

            //update level, max level of skills
            m_Played_time[PLAYED_TIME_LEVEL] = 0;                   // Level Played Time reset

            _ApplyAllLevelScaleItemMods(false);
            */
            SetLevel(level);
            /*
            UpdateSkillsForLevel();

            // save base values (bonuses already included in stored stats
            for (uint8 i = STAT_STRENGTH; i < MAX_STATS; ++i)
                SetCreateStat(Stats(i), info.stats[i]);

            SetCreateHealth(basehp);
            SetCreateMana(basemana);

            InitTalentForLevel();
            InitTaxiNodesForLevel();
            InitGlyphsForLevel();

            UpdateAllStats();

            if (sWorld.getBoolConfig(CONFIG_ALWAYS_MAXSKILL)) // Max weapon skill when leveling up
                UpdateSkillsToMaxSkillsForLevel();

            // set current level health and mana/energy to maximum after applying all mods.
            SetFullHealth();
            SetPower(POWER_MANA, GetMaxPower(POWER_MANA));
            SetPower(POWER_ENERGY, GetMaxPower(POWER_ENERGY));
            if (GetPower(POWER_RAGE) > GetMaxPower(POWER_RAGE))
                SetPower(POWER_RAGE, GetMaxPower(POWER_RAGE));
            SetPower(POWER_FOCUS, 0);

            _ApplyAllLevelScaleItemMods(true);

            // update level to hunter/summon pet
            if (Pet* pet = GetPet())
                pet.SynchronizeLevelWithOwner();

            if (MailLevelReward const* mailReward = sObjectMgr.GetMailLevelReward(level, getRaceMask()))
            {
                //- TODO: Poor design of mail system
                SQLTransaction trans = CharacterDatabase.BeginTransaction();
                MailDraft(mailReward.mailTemplateId).SendMailTo(trans, this, MailSender(MAIL_CREATURE, mailReward.senderEntry));
                CharacterDatabase.CommitTransaction(trans);
            }

            UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_REACH_LEVEL);

            // Refer-A-Friend
            if (GetSession().GetRecruiterId())
                if (level < sWorld.getIntConfig(CONFIG_MAX_RECRUIT_A_FRIEND_BONUS_PLAYER_LEVEL))
                    if (level % 2 == 0)
                    {
                        ++m_grantableLevels;

                        if (!HasByteFlag(PLAYER_FIELD_BYTES, 1, 0x01))
                            SetByteFlag(PLAYER_FIELD_BYTES, 1, 0x01);
                    }

            sScriptMgr.OnPlayerLevelChanged(this, oldLevel);
            */
        }

        #endregion

        //Create
        public bool Create(uint guidlow, ref PacketReader packet)
        {
            //FIXME: outfitId not used in player creating
            // TODO: need more checks against packet modifications
            // should check that skin, face, hair* are valid via DBC per race/class

            byte pClass = packet.ReadByte();
            byte hairStyle = packet.ReadByte();
            byte facialHair = packet.ReadByte();
            byte race = packet.ReadByte();
            byte face = packet.ReadByte();
            byte skin = packet.ReadByte();
            byte gender = packet.ReadByte();
            byte hairColor = packet.ReadByte();
            byte outfitId = packet.ReadByte();

            packet.ReadBit();
            uint nameLength = packet.GetBits<uint>(7);
            string name = packet.ReadString(nameLength);
            SQLResult result = DB.Characters.Select("SELECT * from characters WHERE name = '{0}'", name);
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_CharCreate);

            if (result.Count != 0)
            {
                // Name already in use
                writer.WriteUInt8((byte)ResponseCodes.CharCreateNameInUse);
                GetSession().Send(writer);
                return false;
            }

            CreateGuid(guidlow, 0, HighGuidType.Player);

            Name = name;

            PlayerInfo info = Cypher.ObjMgr.GetPlayerInfo(race, pClass);
            if (info == null)
            {
                Log.outError("PlayerCreate: Possible hacking-attempt: Account {0} tried creating a character named '{1}' with an invalid race/class pair ({2}/{3}) - refusing to do so.",
                    GetSession().GetAccount().Id, GetName(), race, pClass);
                return false;
            }

            Items = new Item[(int)PlayerSlots.Count];

            Position = new ObjectPosition(info.PositionX, info.PositionY, info.PositionZ, info.Orientation);

            var cEntry = DBCStorage.CharClassStorage.LookupByKey(pClass);
            if (cEntry.ClassID == 0)
            {
                Log.outError("PlayerCreate: Possible hacking-attempt: Account {0} tried creating a character named '{1}' with an invalid character class ({2}) - refusing to do so (wrong DBC-files?)",
                    GetSession().GetAccount().Id, GetName(), pClass);
                return false;
            }

            SetMap(Cypher.MapMgr.CreateMap(info.MapId, this));

            int powertype = (int)cEntry.powerType;

            SetValue<float>(UnitFields.BoundingRadius, SharedConst.DefaultWorldObjectSize);
            SetValue<float>(UnitFields.CombatReach, 1.5f);

            SetFactionForRace(race);

            //if (!IsValidGender(createInfo.Gender))
            {
                //sLog.outError(LOG_FILTER_PLAYER, "Player::Create: Possible hacking-attempt: Account %u tried creating a character named '%s' with an invalid gender (%hu) - refusing to do so",
                //GetSession().GetAccountId(), m_name.c_str(), createInfo.Gender);
                //return false;
            }

            int RaceClassGender = race | pClass << 8 | gender << 16;

            SetValue<uint>(UnitFields.Bytes, (uint)(RaceClassGender | (powertype << 24)));
            InitDisplayIds();
            //if (sWorld.getIntConfig(CONFIG_GAME_TYPE) == REALM_TYPE_PVP || sWorld.getIntConfig(CONFIG_GAME_TYPE) == REALM_TYPE_RPPVP)
            {
                //SetValue<byte>(UnitFields.Bytes2, 0x01, 1);
                SetFlag(UnitFields.Flags, (uint)UnitFlags.Pvp);
            }

            SetFlag(UnitFields.Flags2, (uint)UnitFlags2.RegeneratePower);
            SetValue<float>(UnitFields.HoverHeight, 1.0f);            // default for players in 3.0.3

            SetValue<int>(PlayerFields.WatchedFactionIndex, -1);  // -1 is default value

            SetValue<uint>(PlayerFields.Bytes, (uint)(skin | (face << 8) | (hairStyle << 16) | (hairColor << 24)));
            SetValue<uint>(PlayerFields.Bytes2, (uint)(facialHair | (0x00 << 8) | (0x00 << 16) | (int)PlayerRestState.NotRAFLinked << 24));
            //(((GetSession().IsARecruiter() || GetSession().GetRecruiterId() != 0) ? REST_STATE_RAF_LINKED : REST_STATE_NOT_RAF_LINKED) << 24)));
            SetValue<byte>(PlayerFields.Bytes3, gender, 0);
            SetValue<byte>(PlayerFields.Bytes3, 0, 3);                     // BattlefieldArenaFaction (0 or 1)

            SetValue<ulong>(ObjectFields.Data, 0);
            SetValue<uint>(PlayerFields.GuildRankID, 0);
            SetGuildLevel(0);
            SetValue<uint>(PlayerFields.GuildTimeStamp, 0);

            for (int i = 0; i < (int)PlayerTitle.KnowTitlesSize; ++i)
                SetValue<ulong>(PlayerFields.KnownTitles + i, 0);  // 0=disabled
            SetValue<uint>(PlayerFields.PlayerTitle, 0);
            SetValue<uint>(PlayerFields.YesterdayHonorableKills, 0);
            SetValue<uint>(PlayerFields.LifetimeHonorableKills, 0);

            // set starting level
            int start_level = getClass() != (byte)Class.Deathknight
                ? 1//sWorld.getIntConfig(CONFIG_START_PLAYER_LEVEL)
                : 55;//sWorld.getIntConfig(CONFIG_START_HEROIC_PLAYER_LEVEL);

            if (!Cypher.ObjMgr.IsPlayerAccount(GetSession().GetSecurity()))
            {
                int gm_level = 85;// sWorld.getIntConfig(CONFIG_START_GM_LEVEL);
                if (gm_level > start_level)
                    start_level = gm_level;
            }

            SetValue<uint>(UnitFields.Level, (uint)start_level);

            //InitRunes();

            SetValue<long>(PlayerFields.Coinage, 0);//sWorld.getIntConfig(CONFIG_START_PLAYER_MONEY));
            //SetCurrency(CURRENCY_TYPE_HONOR_POINTS, sWorld.getIntConfig(CONFIG_START_HONOR_POINTS));
            //SetCurrency(CURRENCY_TYPE_CONQUEST_POINTS, sWorld.getIntConfig(CONFIG_START_ARENA_POINTS));

            // start with every map explored
            //if (sWorld.getBoolConfig(CONFIG_START_ALL_EXPLORED))
            {
                //for (uint8 i=0; i<PLAYER_EXPLORED_ZONES_SIZE; i++)
                //SetFlag(PLAYER_EXPLORED_ZONES_1+i, 0xFFFFFFFF);
            }
            /*
            //Reputations if "StartAllReputation" is enabled, -- TODO: Fix this in a better way
            //if (sWorld.getBoolConfig(CONFIG_START_ALL_REP))
            {
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(942), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(935), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(936), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(1011), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(970), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(967), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(989), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(932), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(934), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(1038), 42999);
                GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(1077), 42999);

                // Factions depending on team, like cities and some more stuff
                switch (GetTeam())
                {
                    case Team.Alliance:
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(72), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(47), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(69), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(930), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(730), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(978), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(54), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(946), 42999);
                        break;
                    case Team.Horde:
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(76), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(68), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(81), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(911), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(729), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(941), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(530), 42999);
                        GetReputationMgr().SetReputation(DBCStorage.FactionStorage.LookupByKey(947), 42999);
                        break;
                    default:
                        break;
                }
            }
            */

            // Played time
            //m_Last_tick = time(NULL);
            //m_Played_time[PLAYED_TIME_TOTAL] = 0;
            //m_Played_time[PLAYED_TIME_LEVEL] = 0;

            // base stats and related field values
            InitStatsForLevel();
            //InitTaxiNodesForLevel();
            //InitGlyphsForLevel();
            //InitTalentForLevel();
            InitPrimaryProfessions();                               // to max set before any spell added

            // apply original stats mods before spell loading or item equipment that call before equip _RemoveStatsMods()
            //UpdateMaxHealth();                                      // Update max Health (for add bonus from stamina)
            SetFullHealth();
            if (getPowerType() == Powers.Mana)
            {
                //UpdateMaxPower(POWER_MANA);                         // Update max Mana (for add bonus from intellect)
                SetPower(Powers.Mana, GetMaxPower(Powers.Mana));
            }

            if (getPowerType() == Powers.RunicPower)
            {
                SetPower(Powers.Runes, 8);
                SetMaxPower(Powers.Runes, 8);
                SetPower(Powers.RunicPower, 0);
                SetMaxPower(Powers.RunicPower, 1000);
            }

            //Slow
            // original spells
            learnDefaultSpells();

            // original action bar
            //for (PlayerCreateInfoActions::const_iterator action_itr = info.action.begin(); action_itr != info.action.end(); ++action_itr)
            //addActionButton(action_itr.button, action_itr.action, action_itr.type);

            // original items
            CharStartOutfit oEntry = DBCStorage.CharStartOutfitStorage.Values.FirstOrDefault(p => p.Mask == RaceClassGender);
            if (oEntry.Mask != 0)
            {
                for (int j = 0; j < ItemConst.MaxOutfitItems; ++j)
                {
                    if (oEntry.ItemId[j] <= 0)
                        continue;

                    uint itemId = (uint)oEntry.ItemId[j];

                    ItemTemplate iProto = Cypher.ObjMgr.GetItemTemplate(itemId);
                    if (iProto == null)
                        continue;

                    // BuyCount by default
                    uint count = iProto.BuyCount;

                    // special amount for food/drink
                    if (iProto.Class == ItemClass.Consumable && iProto.SubClass == (uint)ItemSubClassConsumable.FoodDrink)
                    {
                        switch (iProto.Spells[0].SpellCategory)
                        {
                            case 11:                                // food
                                count = (uint)(getClass() == (byte)Class.Deathknight ? 10 : 4);
                                break;
                            case 59:                                // drink
                                count = 2;
                                break;
                        }
                        if (iProto.GetMaxStackSize() < count)
                            count = iProto.GetMaxStackSize();
                    }
                    StoreNewItemInBestSlots(itemId, count);
                }
            }
            foreach (var item in info.item)
                StoreNewItemInBestSlots(item.item_id, item.item_amount);

            // bags and main-hand weapon must equipped at this moment
            // now second pass for not equipped (offhand weapon/shield if it attempt equipped before main-hand weapon)
            // or ammo not equipped in special bag
            for (var i = InventorySlots.ItemStart; i < InventorySlots.ItemEnd; i++)
            {
                Item pItem = GetItemByPos(InventorySlots.Bag0, i);
                if (pItem != null)
                {
                    ushort eDest;
                    // equip offhand weapon/shield if it attempt equipped before main-hand weapon
                    InventoryResult msg = CanEquipItem(ItemConst.NullSlot, out eDest, pItem, false);
                    if (msg == InventoryResult.Ok)
                    {
                        RemoveItem(InventorySlots.Bag0, i, true);
                        EquipItem(eDest, pItem, true);
                    }
                    // move other items to more appropriate slots
                    else
                    {
                        List<ItemPosCount> sDest = new List<ItemPosCount>();
                        msg = CanStoreItem(ItemConst.NullBag, ItemConst.NullSlot, ref sDest, pItem, false);
                        if (msg == InventoryResult.Ok)
                        {
                            RemoveItem(InventorySlots.Bag0, i, true);
                            pItem = StoreItem(sDest, pItem, true);
                        }
                    }
                }
            }
            // all item positions resolved

            return true;
        }
        void InitDisplayIds()
        {
            PlayerInfo info = Cypher.ObjMgr.GetPlayerInfo(getRace(), getClass());
            if (info == null)
            {
                Log.outError("Player {0} has incorrect race/class pair. Can't init display ids.", GetGUIDLow());
                return;
            }

            byte gender = getGender();
            switch ((Gender)gender)
            {
                case Gender.Female:
                    SetDisplayId(info.DisplayId_f);
                    SetNativeDisplayId(info.DisplayId_f);
                    break;
                case Gender.Male:
                    SetDisplayId(info.DisplayId_m);
                    SetNativeDisplayId(info.DisplayId_m);
                    break;
                default:
                    Log.outError("Invalid gender {0} for player", gender);
                    return;
            }
        }

        //Creature
        public Creature GetNPCIfCanInteractWith(ulong guid, NPCFlags npcflagmask)
        {
            // unit checks
            if (guid == 0)
                return null;

            if (!IsInWorld)
                return null;

            //if (isInFlight())
            //return null;

            // exist (we need look pets also for some interaction (quest/etc)
            Creature creature = Cypher.ObjMgr.GetObject<Creature>(this, guid);
            if (creature == null)
                return null;

            // Deathstate checks
            if (!isAlive() && !Convert.ToBoolean(creature.GetCreatureTemplate().TypeFlags & CreatureTypeFlags.CREATURE_TYPEFLAGS_GHOST))
                return null;

            // alive or spirit healer
            if (!creature.isAlive() && !Convert.ToBoolean(creature.GetCreatureTemplate().TypeFlags & CreatureTypeFlags.CREATURE_TYPEFLAGS_DEAD_INTERACT))
                return null;

            // appropriate npc type
            if (!creature.HasFlag((int)UnitFields.NpcFlags, npcflagmask))
                return null;

            // not allow interaction under control, but allow with own pets
            //if (creature.GetCharmerGUID())
            //return NULL;

            // not enemy
            //if (creature.IsHostileTo(this))
            //return NULL;

            // not unfriendly
            //if (FactionTemplateEntry const* factionTemplate = sFactionTemplateStore.LookupEntry(creature.getFaction()))
            //if (factionTemplate.faction)
            //if (FactionEntry const* faction = sFactionStore.LookupEntry(factionTemplate.faction))
            //if (faction.reputationListID >= 0 && GetReputationMgr().GetRank(faction) <= REP_UNFRIENDLY)
            //return NULL;

            // not too far
            //if (!creature.IsWithinDistInMap(this, INTERACTION_DISTANCE))
            //return NULL;

            return creature;
        }
        public TrainerSpellState GetTrainerSpellState(TrainerSpell trainer_spell)
        {
            if (trainer_spell.spellId == 0)
                return TrainerSpellState.Red;

            bool hasSpell = true;
            for (var i = 0; i < 3; ++i)
            {
                if (trainer_spell.learnedSpell[i] == 0)
                    continue;

                if (!HasSpell(trainer_spell.learnedSpell[i]))
                {
                    hasSpell = false;
                    break;
                }
            }
            // known spell
            if (hasSpell)
                return TrainerSpellState.Gray;

            // check skill requirement
            //if (trainer_spell.reqSkill && GetBaseSkillValue(trainer_spell.reqSkill) < trainer_spell.reqSkillValue)
            //return TrainerSpellState.Red;

            // check level requirement
            if (getLevel() < trainer_spell.reqLevel)
                return TrainerSpellState.Red;

            for (var i = 0; i < 3; ++i)
            {
                if (trainer_spell.learnedSpell[i] == 0)
                    continue;

                // check race/class requirement
                //if (!IsSpellFitByClassAndRace(trainer_spell.learnedSpell[i]))
                //return TrainerSpellState.Red;

                //if (uint32 prevSpell = sSpellMgr.GetPrevSpellInChain(trainer_spell.learnedSpell[i]))
                {
                    // check prev.rank requirement
                    //if (prevSpell && !HasSpell(prevSpell))
                    //return TrainerSpellState.Red;
                }

                //SpellsRequiringSpellMapBounds spellsRequired = sSpellMgr.GetSpellsRequiredForSpellBounds(trainer_spell.learnedSpell[i]);
                //for (SpellsRequiringSpellMap::const_iterator itr = spellsRequired.first; itr != spellsRequired.second; ++itr)
                {
                    // check additional spell requirement
                    //if (!HasSpell(itr.second))
                    //return TrainerSpellState.Red;
                }
            }

            // check primary prof. limit
            // first rank of primary profession spell when there are no proffesions avalible is disabled
            for (var i = 0; i < 3; ++i)
            {
                if (trainer_spell.learnedSpell[i] == 0)
                    continue;

                SpellInfo learnedSpellInfo = Cypher.SpellMgr.GetSpellInfo(trainer_spell.learnedSpell[i]);
                //if (learnedSpellInfo != null && learnedSpellInfo.IsPrimaryProfessionFirstRank() && (GetFreePrimaryProfessionPoints() == 0))
                //return TrainerSpellState.GreenDisabled;
            }

            return TrainerSpellState.Green;
        }

        //Gameobject
        public GameObject CanInteractWith()
        {
            return null;
        }

        #region Items
        //Gets
        public Item GetItemByGuid(ulong guid)
        {
            for (byte i = (byte)EquipmentSlot.Start; i < InventorySlots.ItemEnd; ++i)
            {
                Item pItem = GetItemByPos(InventorySlots.Bag0, i);
                if (pItem != null)
                    if (pItem.GetGUID() == guid)
                        return pItem;
            }

            for (byte i = InventorySlots.BankItemStart; i < InventorySlots.BankItemEnd; ++i)
            {
                Item pItem = GetItemByPos(InventorySlots.Bag0, i);
                if (pItem != null)
                    if (pItem.GetGUID() == guid)
                        return pItem;
            }

            for (byte i = InventorySlots.BagStart; i < InventorySlots.BagEnd; ++i)
            {
                Bag pBag = GetBagByPos(i);
                if (pBag != null)
                    for (byte j = 0; j < pBag.GetBagSize(); ++j)
                    {
                        Item pItem = pBag.GetItemByPos(j);
                        if (pItem != null)
                            if (pItem.GetGUID() == guid)
                                return pItem;
                    }
            }

            for (byte i = InventorySlots.BankBagStart; i < InventorySlots.BankBagEnd; ++i)
            {
                Bag  pBag = GetBagByPos(i);
                if (pBag != null)
                    for (byte j = 0; j < pBag.GetBagSize(); ++j)
                    {
                        Item pItem = pBag.GetItemByPos(j);
                        if (pItem != null)
                            if (pItem.GetGUID() == guid)
                                return pItem;
                    }
            }
            return null;
        }
        public Item GetWeaponForAttack(WeaponAttackType attackType, bool useable = false)
        {
            EquipmentSlot slot;
            switch (attackType)
            {
                case WeaponAttackType.BaseAttack:
                    slot = EquipmentSlot.MainHand;
                    break;
                case WeaponAttackType.OffAttack:
                    slot = EquipmentSlot.OffHand;
                    break;
                case WeaponAttackType.RangedAttack:
                    slot = EquipmentSlot.Ranged;
                    break;
                default:
                    return null;
            }

            Item item = null;
            if (useable)
                item = GetUseableItemByPos(InventorySlots.BagStart, (byte)slot);
            else
                item = GetItemByPos(InventorySlots.BagStart, (byte)slot);
            if (item == null || item.GetTemplate().Class != ItemClass.Weapon)
                return null;

            if (!useable)
                return item;

            //if (item.IsBroken() || IsInFeralForm())
            //return null;

            return item;
        }
        Bag GetBagByPos(byte bag)
        {
            if ((bag >= InventorySlots.BagStart && bag < InventorySlots.BagEnd)
                || (bag >= InventorySlots.BankBagStart && bag < InventorySlots.BankBagEnd))
            {
                Item item = GetItemByPos(InventorySlots.Bag0, bag);
                if (item != null)
                    return item.ToBag();
            }
            return null;
        }
        byte FindEquipSlot(ItemTemplate proto, uint slot, bool swap)
        {
            Class playerClass = (Class)getClass();

            byte[] slots = new byte[4];
            slots[0] = ItemConst.NullSlot;
            slots[1] = ItemConst.NullSlot;
            slots[2] = ItemConst.NullSlot;
            slots[3] = ItemConst.NullSlot;
            switch (proto.inventoryType)
            {
                case InventoryType.Head:
                    slots[0] = (byte)EquipmentSlot.Head;
                    break;
                case InventoryType.Neck:
                    slots[0] = (byte)EquipmentSlot.Neck;
                    break;
                case InventoryType.Shoulders:
                    slots[0] = (byte)EquipmentSlot.Shoulders;
                    break;
                case InventoryType.Body:
                    slots[0] = (byte)EquipmentSlot.Shirt;
                    break;
                case InventoryType.Chest:
                    slots[0] = (byte)EquipmentSlot.Chest;
                    break;
                case InventoryType.Robe:
                    slots[0] = (byte)EquipmentSlot.Chest;
                    break;
                case InventoryType.Waist:
                    slots[0] = (byte)EquipmentSlot.Waist;
                    break;
                case InventoryType.Legs:
                    slots[0] = (byte)EquipmentSlot.Legs;
                    break;
                case InventoryType.Feet:
                    slots[0] = (byte)EquipmentSlot.Feet;
                    break;
                case InventoryType.Wrists:
                    slots[0] = (byte)EquipmentSlot.Wrist;
                    break;
                case InventoryType.Hands:
                    slots[0] = (byte)EquipmentSlot.Hands;
                    break;
                case InventoryType.Finger:
                    slots[0] = (byte)EquipmentSlot.Finger1;
                    slots[1] = (byte)EquipmentSlot.Finger2;
                    break;
                case InventoryType.Trinket:
                    slots[0] = (byte)EquipmentSlot.Trinket1;
                    slots[1] = (byte)EquipmentSlot.Trinket2;
                    break;
                case InventoryType.Cloak:
                    slots[0] = (byte)EquipmentSlot.Cloak;
                    break;
                case InventoryType.Weapon:
                    {
                        slots[0] = (byte)EquipmentSlot.MainHand;

                        // suggest offhand slot only if know dual wielding
                        // (this will be replace mainhand weapon at auto equip instead unwonted "you don't known dual wielding" ...
                        //if (CanDualWield())
                        //slots[1] = (byte)EquipmentSlot.OffHand;
                        break;
                    }
                case InventoryType.Shield:
                    slots[0] = (byte)EquipmentSlot.OffHand;
                    break;
                case InventoryType.Ranged:
                    slots[0] = (byte)EquipmentSlot.Ranged;
                    break;
                case InventoryType.Weapon2Hand:
                    slots[0] = (byte)EquipmentSlot.MainHand;
                    Item mhWeapon = GetItemByPos(InventorySlots.Bag0, (byte)EquipmentSlot.MainHand);
                    if (mhWeapon != null)
                    {
                        ItemTemplate mhWeaponProto = mhWeapon.GetTemplate();
                        if (mhWeaponProto != null)
                        {
                            if (mhWeaponProto.SubClass == (uint)ItemSubClassWeapon.Polearm || mhWeaponProto.SubClass == (uint)ItemSubClassWeapon.Staff)
                            {
                                //(this as Player).AutoUnequipOffhandIfNeed(true);
                                break;
                            }
                        }
                    }

                    if (GetItemByPos(InventorySlots.Bag0, (byte)EquipmentSlot.OffHand) != null)
                    {
                        if (proto.SubClass == (uint)ItemSubClassWeapon.Polearm || proto.SubClass == (uint)ItemSubClassWeapon.Staff)
                        {
                            //const_cast<Player*>(this).AutoUnequipOffhandIfNeed(true);
                            break;
                        }
                    }
                    //if (CanDualWield() && CanTitanGrip() && proto.SubClass != ITEM_SUBCLASS_WEAPON_POLEARM && proto.SubClass != ITEM_SUBCLASS_WEAPON_STAFF)
                    //slots[1] = (byte)EquipmentSlot.OffHand;
                    break;
                case InventoryType.Tabard:
                    slots[0] = (byte)EquipmentSlot.Tabard;
                    break;
                case InventoryType.WeaponMainhand:
                    slots[0] = (byte)EquipmentSlot.MainHand;
                    break;
                case InventoryType.WeaponOffhand:
                    slots[0] = (byte)EquipmentSlot.OffHand;
                    break;
                case InventoryType.Holdable:
                    slots[0] = (byte)EquipmentSlot.OffHand;
                    break;
                case InventoryType.Thrown:
                    slots[0] = (byte)EquipmentSlot.Ranged;
                    break;
                case InventoryType.RangedRight:
                    slots[0] = (byte)EquipmentSlot.Ranged;
                    break;
                case InventoryType.Bag:
                    slots[0] = InventorySlots.BagStart + 0;
                    slots[1] = InventorySlots.BagStart + 1;
                    slots[2] = InventorySlots.BagStart + 2;
                    slots[3] = InventorySlots.BagStart + 3;
                    break;
                case InventoryType.Relic:
                    {
                        if (playerClass == Class.Paladin || playerClass == Class.Druid ||
                            playerClass == Class.Shaman || playerClass == Class.Deathknight)
                            slots[0] = (byte)EquipmentSlot.Ranged;
                        break;
                    }
                default:
                    return ItemConst.NullSlot;
            }

            if (slot != ItemConst.NullSlot)
            {
                if (swap || GetItemByPos(InventorySlots.Bag0, (byte)slot) == null)
                    for (byte i = 0; i < 4; ++i)
                        if (slots[i] == slot)
                            return (byte)slot;
            }
            else
            {
                // search free slot at first
                for (byte i = 0; i < 4; ++i)
                    if (slots[i] != ItemConst.NullSlot && GetItemByPos(InventorySlots.Bag0, slots[i]) == null)
                        // in case 2hand equipped weapon (without titan grip) offhand slot empty but not free
                        if (slots[i] != (byte)EquipmentSlot.OffHand)// || !IsTwoHandUsed())
                            return slots[i];

                // if not found free and can swap return first appropriate from used
                for (byte i = 0; i < 4; ++i)
                    if (slots[i] != ItemConst.NullSlot && swap)
                        return slots[i];
            }

            // no free position
            return ItemConst.NullSlot;
        }
        uint GetItemCount(uint item, bool inBankAlso = false, Item skipItem = null)
        {
            uint count = 0;
            for (byte i = (byte)EquipmentSlot.Start; i < InventorySlots.ItemEnd; i++)
            {
                Item pItem = GetItemByPos(InventorySlots.Bag0, i);
                if (pItem != null)
                    if (pItem != skipItem && pItem.GetEntry() == item)
                        count += pItem.GetStackCount();
            }

            for (var i = InventorySlots.BagStart; i < InventorySlots.BagEnd; ++i)
            {
                Bag pBag = GetBagByPos(i);
                if (pBag != null)
                    count += pBag.GetItemCount(item, skipItem);
            }

            if (skipItem != null && skipItem.GetTemplate().GemProperties != 0)
            {
                for (byte i = (byte)EquipmentSlot.Start; i < InventorySlots.ItemEnd; ++i)
                {
                    Item pItem = GetItemByPos(InventorySlots.Bag0, i);
                    if (pItem != null)
                        if (pItem != skipItem && pItem.GetTemplate().Socket[0].Color != 0)
                            count += pItem.GetGemCountWithID(item);
                }
            }

            if (inBankAlso)
            {
                // checking every item from 39 to 74 (including bank bags)
                for (var i = InventorySlots.BankItemStart; i < InventorySlots.BankBagEnd; ++i)
                {
                    Item pItem = GetItemByPos(InventorySlots.Bag0, i);
                    if (pItem != null)
                        if (pItem != skipItem && pItem.GetEntry() == item)
                            count += pItem.GetStackCount();
                }

                for (var i = InventorySlots.BankBagStart; i < InventorySlots.BankBagEnd; ++i)
                {
                    Bag pBag = GetBagByPos(i);
                    if (pBag != null)
                        count += pBag.GetItemCount(item, skipItem);
                }

                if (skipItem != null && skipItem.GetTemplate().GemProperties != 0)
                {
                    for (var i = InventorySlots.BankItemStart; i < InventorySlots.BankItemEnd; ++i)
                    {
                        Item pItem = GetItemByPos(InventorySlots.Bag0, i);
                        if (pItem != null)
                            if (pItem != skipItem && pItem.GetTemplate().Socket[0].Color != 0)
                                count += pItem.GetGemCountWithID(item);
                    }
                }
            }

            return count;
        }
        Item GetUseableItemByPos(byte bag, byte slot) //Does additional check for disarmed weapons
        {
            if (!CanUseAttackType(GetAttackBySlot(slot)))
                return null;
            return GetItemByPos(bag, slot);
        }
        public Item GetItemByPos(ushort pos)
        {
            byte bag = (byte)(pos >> 8);
            byte slot = (byte)(pos & 255);
            return GetItemByPos(bag, slot);
        }
        public Item GetItemByPos(byte bag, byte slot)
        {
            if (bag == InventorySlots.Bag0 && slot < InventorySlots.BankBagEnd)
                return Items[slot];
            Bag pBag = GetBagByPos(bag);
            if (pBag != null)
                return pBag.GetItemByPos(slot);
            return null;
        }
        WeaponAttackType GetAttackBySlot(byte slot)
        {
            switch ((EquipmentSlot)slot)
            {
                case EquipmentSlot.MainHand:
                    return WeaponAttackType.BaseAttack;
                case EquipmentSlot.OffHand:
                    return WeaponAttackType.OffAttack;
                case EquipmentSlot.Ranged:
                    return WeaponAttackType.RangedAttack;
                default:
                    return WeaponAttackType.MaxAttack;
            }
        }
        public bool IsEquipmentPos(ushort pos) { return IsEquipmentPos((byte)(pos >> 8), (byte)(pos & 255)); }
        public bool IsInventoryPos(ushort pos) { return IsInventoryPos((byte)(pos >> 8), (byte)(pos & 255)); }
        public bool IsInventoryPos(byte bag, byte slot)
        {
            if (bag == InventorySlots.Bag0 && slot == ItemConst.NullSlot)
                return true;
            if (bag == InventorySlots.Bag0 && (slot >= InventorySlots.ItemStart && slot < InventorySlots.ItemEnd))
                return true;
            if (bag >= InventorySlots.BagStart && bag < InventorySlots.BagEnd)
                return true;

            return false;
        }
        public bool IsEquipmentPos(byte bag, byte slot)
        {
            if (bag == InventorySlots.Bag0 && (slot < (byte)EquipmentSlot.End))
                return true;
            if (bag == InventorySlots.Bag0 && (slot >= InventorySlots.BagStart && slot < InventorySlots.BagEnd))
                return true;
            return false;
        }
        public bool IsBankPos(byte bag, byte slot)
        {
            if (bag == InventorySlots.Bag0 && (slot >= InventorySlots.BankItemStart && slot < InventorySlots.BankItemEnd))
                return true;
            if (bag == InventorySlots.Bag0 && (slot >= InventorySlots.BankBagStart && slot < InventorySlots.BankBagEnd))
                return true;
            if (bag >= InventorySlots.BankBagStart && bag < InventorySlots.BankBagEnd)
                return true;
            return false;
        }
        public bool IsBankPos(ushort pos) { return IsBankPos((byte)(pos >> 8), (byte)(pos & 255)); }
        public bool IsBagPos(ushort pos)
        {
            byte bag = (byte)(pos >> 8);
            byte slot = (byte)(pos & 255);
            if (bag == InventorySlots.Bag0 && (slot >= InventorySlots.BagStart && slot < InventorySlots.BagEnd))
                return true;
            if (bag == InventorySlots.Bag0 && (slot >= InventorySlots.BankBagStart && slot < InventorySlots.BankBagEnd))
                return true;
            return false;
        }
        byte GetBankBagSlotCount() { return GetValue<byte>(PlayerFields.Bytes2, 2); }

        //Sets
        void SetVisibleItemSlot(byte slot, Item pItem)
        {
            if (pItem != null)
            {
                SetValue<uint>(PlayerFields.VisibleItems + (slot * 2), pItem.GetVisibleEntry());
                SetValue<ushort>(PlayerFields.VisibleItems + 1 + (slot * 2), (ushort)pItem.GetEnchantmentId(EnchantmentSlot.PERM_ENCHANTMENT_SLOT), 0);
                SetValue<ushort>(PlayerFields.VisibleItems + 1 + (slot * 2), (ushort)pItem.GetEnchantmentId(EnchantmentSlot.TEMP_ENCHANTMENT_SLOT), 1);
            }
            else
            {
                SetValue<uint>(PlayerFields.VisibleItems + (slot * 2), 0);
                SetValue<uint>(PlayerFields.VisibleItems + 1 + (slot * 2), 0);
            }
        }

        //Checks
        public InventoryResult CanStoreItem(byte bag, byte slot, ref List<ItemPosCount> dest, Item pItem, bool swap = false)
        {
            if (pItem == null)
                return InventoryResult.ItemNotFound;
            uint count = pItem.GetStackCount();
            return CanStoreItem(bag, slot, ref dest, pItem.GetEntry(), count, pItem, swap);

        }
        public InventoryResult CanStoreItem(byte bag, byte slot, ref List<ItemPosCount> dest, uint entry, uint count, Item pItem, bool swap)
        {
            uint throwaway;
            return CanStoreItem(bag, slot, ref dest, entry, count, pItem, swap, out throwaway);
        }
        public InventoryResult CanStoreItem(byte bag, byte slot, ref List<ItemPosCount> dest, uint entry, uint count, Item pItem, bool swap, out uint no_space_count)
        {
            no_space_count = 0;
            Log.outDebug("STORAGE: CanStoreItem bag = {0}, slot = {1}, item = {2}, count = {3}", bag, slot, entry, count);

            ItemTemplate pProto = Cypher.ObjMgr.GetItemTemplate(entry);
            if (pProto == null)
            {
                no_space_count = count;
                return swap ? InventoryResult.CantSwap : InventoryResult.ItemNotFound;
            }

            if (pItem != null)
            {
                // item used
                if (pItem.LootGenerated)
                {
                    no_space_count = count;
                    return InventoryResult.LootGone;
                }

                if (pItem.IsBindedNotWith(this))
                {
                    no_space_count = count;
                    return InventoryResult.NotOwner;
                }
            }

            // check count of items (skip for auto move for same player from bank)
            uint no_similar_count = 0;                            // can't store this amount similar items
            InventoryResult res = CanTakeMoreSimilarItems(entry, count, pItem, out no_similar_count);
            if (res != InventoryResult.Ok)
            {
                if (count == no_similar_count)
                {
                    no_space_count = no_similar_count;
                    return res;
                }
                count -= no_similar_count;
            }

            // in specific slot
            if (bag != ItemConst.NullBag && slot != ItemConst.NullSlot)
            {
                res = CanStoreItem_InSpecificSlot(bag, slot, ref dest, pProto, ref count, swap, pItem);
                if (res != InventoryResult.Ok)
                {
                    no_space_count = count + no_similar_count;
                    return res;
                }

                if (count == 0)
                {
                    if (no_similar_count == 0)
                        return InventoryResult.Ok;

                    no_space_count = count + no_similar_count;
                    return InventoryResult.ItemMaxCount;
                }
            }

            // not specific slot or have space for partly store only in specific slot

            // in specific bag
            if (bag != ItemConst.NullBag)
            {
                // search stack in bag for merge to
                if (pProto.Stackable != 1)
                {
                    if (bag == InventorySlots.Bag0)               // inventory
                    {
                        res = CanStoreItem_InInventorySlots(InventorySlots.ItemStart, InventorySlots.ItemEnd, ref dest, pProto, ref count, true, pItem, bag, slot);
                        if (res != InventoryResult.Ok)
                        {
                            no_space_count = count + no_similar_count;
                            return res;
                        }

                        if (count == 0)
                        {
                            if (no_similar_count == 0)
                                return InventoryResult.Ok;

                            no_space_count = count + no_similar_count;
                            return InventoryResult.ItemMaxCount;
                        }
                    }
                    else                                            // equipped bag
                    {
                        // we need check 2 time (specialized/non_specialized), use NULL_BAG to prevent skipping bag
                        res = CanStoreItem_InBag(bag, ref dest, pProto, ref count, true, false, pItem, ItemConst.NullBag, slot);
                        if (res != InventoryResult.Ok)
                            res = CanStoreItem_InBag(bag, ref dest, pProto, ref count, true, true, pItem, ItemConst.NullBag, slot);

                        if (res != InventoryResult.Ok)
                        {
                            no_space_count = count + no_similar_count;
                            return res;
                        }

                        if (count == 0)
                        {
                            if (no_similar_count == 0)
                                return InventoryResult.Ok;

                            no_space_count = count + no_similar_count;
                            return InventoryResult.ItemMaxCount;
                        }
                    }
                }

                // search free slot in bag for place to
                if (bag == InventorySlots.Bag0)                     // inventory
                {
                    res = CanStoreItem_InInventorySlots(InventorySlots.ItemStart, InventorySlots.ItemEnd, ref dest, pProto, ref count, false, pItem, bag, slot);
                    if (res != InventoryResult.Ok)
                    {
                        no_space_count = count + no_similar_count;
                        return res;
                    }

                    if (count == 0)
                    {
                        if (no_similar_count == 0)
                            return InventoryResult.Ok;

                        no_space_count = count + no_similar_count;
                        return InventoryResult.ItemMaxCount;
                    }
                }
                else                                                // equipped bag
                {
                    res = CanStoreItem_InBag(bag, ref dest, pProto, ref count, false, false, pItem, ItemConst.NullBag, slot);
                    if (res != InventoryResult.Ok)
                        res = CanStoreItem_InBag(bag, ref dest, pProto, ref count, false, true, pItem, ItemConst.NullBag, slot);

                    if (res != InventoryResult.Ok)
                    {
                        no_space_count = count + no_similar_count;
                        return res;
                    }

                    if (count == 0)
                    {
                        if (no_similar_count == 0)
                            return InventoryResult.Ok;

                        no_space_count = count + no_similar_count;
                        return InventoryResult.ItemMaxCount;
                    }
                }
            }

            // not specific bag or have space for partly store only in specific bag

            // search stack for merge to
            if (pProto.Stackable != 1)
            {
                res = CanStoreItem_InInventorySlots(InventorySlots.ItemStart, InventorySlots.ItemEnd, ref dest, pProto, ref count, true, pItem, bag, slot);
                if (res != InventoryResult.Ok)
                {
                    no_space_count = count + no_similar_count;
                    return res;
                }

                if (count == 0)
                {
                    if (no_similar_count == 0)
                        return InventoryResult.Ok;

                    no_space_count = count + no_similar_count;
                    return InventoryResult.ItemMaxCount;
                }

                if (pProto.BagFamily != 0)
                {
                    for (byte i = InventorySlots.BagStart; i < InventorySlots.BagEnd; i++)
                    {
                        res = CanStoreItem_InBag(i, ref dest, pProto, ref count, true, false, pItem, bag, slot);
                        if (res != InventoryResult.Ok)
                            continue;

                        if (count == 0)
                        {
                            if (no_similar_count == 0)
                                return InventoryResult.Ok;

                            no_space_count = count + no_similar_count;
                            return InventoryResult.ItemMaxCount;
                        }
                    }
                }

                for (byte i = InventorySlots.BagStart; i < InventorySlots.BagEnd; i++)
                {
                    res = CanStoreItem_InBag(i, ref dest, pProto, ref count, true, true, pItem, bag, slot);
                    if (res != InventoryResult.Ok)
                        continue;

                    if (count == 0)
                    {
                        if (no_similar_count == 0)
                            return InventoryResult.Ok;

                        no_space_count = count + no_similar_count;
                        return InventoryResult.ItemMaxCount;
                    }
                }
            }

            // search free slot - special bag case
            if (pProto.BagFamily != 0)
            {
                for (byte i = InventorySlots.BagStart; i < InventorySlots.BagEnd; i++)
                {
                    res = CanStoreItem_InBag(i, ref dest, pProto, ref count, false, false, pItem, bag, slot);
                    if (res != InventoryResult.Ok)
                        continue;

                    if (count == 0)
                    {
                        if (no_similar_count == 0)
                            return InventoryResult.Ok;

                        no_space_count = count + no_similar_count;
                        return InventoryResult.ItemMaxCount;
                    }
                }
            }

            if (pItem != null && pItem.IsNotEmptyBag())
                return InventoryResult.BagInBag;

            // search free slot
            res = CanStoreItem_InInventorySlots(InventorySlots.ItemStart, InventorySlots.ItemEnd, ref dest, pProto, ref count, false, pItem, bag, slot);
            if (res != InventoryResult.Ok)
            {
                no_space_count = count + no_similar_count;
                return res;
            }

            if (count == 0)
            {
                if (no_similar_count == 0)
                    return InventoryResult.Ok;

                no_space_count = count + no_similar_count;
                return InventoryResult.ItemMaxCount;
            }

            for (var i = InventorySlots.BagStart; i < InventorySlots.BagEnd; i++)
            {
                res = CanStoreItem_InBag(i, ref dest, pProto, ref count, false, true, pItem, bag, slot);
                if (res != InventoryResult.Ok)
                    continue;

                if (count == 0)
                {
                    if (no_similar_count == 0)
                        return InventoryResult.Ok;

                    no_space_count = count + no_similar_count;
                    return InventoryResult.ItemMaxCount;
                }
            }

            no_space_count = count + no_similar_count;

            return InventoryResult.InvFull;
        }

        InventoryResult CanStoreNewItem(byte bag, byte slot, ref List<ItemPosCount> dest, uint item, uint count, out uint no_space_count)
        {
            return CanStoreItem(bag, slot, ref dest, item, count, null, false, out no_space_count);
        }
        InventoryResult CanStoreNewItem(byte bag, byte slot, ref List<ItemPosCount> dest, uint item, uint count)
        {
            uint blah;
            return CanStoreItem(bag, slot, ref dest, item, count, null, false, out blah);
        }

        InventoryResult CanStoreItem_InInventorySlots(byte slot_begin, byte slot_end, ref List<ItemPosCount> dest, ItemTemplate pProto, ref uint count, bool merge, Item pSrcItem, byte skip_bag, byte skip_slot)
        {
            //this is never called for non-bag slots so we can do this
            if (pSrcItem != null && pSrcItem.IsNotEmptyBag())
                return InventoryResult.DestroyNonemptyBag;

            for (var j = slot_begin; j < slot_end; j++)
            {
                // skip specific slot already processed in first called CanStoreItem_InSpecificSlot
                if (InventorySlots.Bag0 == skip_bag && j == skip_slot)
                    continue;

                Item pItem2 = GetItemByPos(InventorySlots.Bag0, j);

                // ignore move item (this slot will be empty at move)
                if (pItem2 == pSrcItem)
                    pItem2 = null;

                // if merge skip empty, if !merge skip non-empty
                if ((pItem2 != null) != merge)
                    continue;

                uint need_space = pProto.GetMaxStackSize();

                if (pItem2 != null)
                {
                    // can be merged at least partly
                    InventoryResult res = pItem2.CanBeMergedPartlyWith(pProto);
                    if (res != InventoryResult.Ok)
                        continue;

                    // descrease at current stacksize
                    need_space -= pItem2.GetStackCount();
                }

                if (need_space > count)
                    need_space = count;

                ItemPosCount newPosition = new ItemPosCount((ushort)((int)InventorySlots.Bag0 << 8 | j), need_space);
                if (!newPosition.isContainedIn(ref dest))
                {
                    dest.Add(newPosition);
                    count -= need_space;

                    if (count == 0)
                        return InventoryResult.Ok;
                }
            }
            return InventoryResult.Ok;
        }
        InventoryResult CanStoreItem_InSpecificSlot(byte bag, byte slot, ref List<ItemPosCount> dest, ItemTemplate pProto, ref uint count, bool swap, Item pSrcItem)
        {
            Item pItem2 = GetItemByPos(bag, slot);

            // ignore move item (this slot will be empty at move)
            if (pItem2 == pSrcItem)
                pItem2 = null;

            uint need_space;

            if (pSrcItem != null && pSrcItem.IsNotEmptyBag() && !IsBagPos((ushort)(bag << 8 | slot)))
                return InventoryResult.DestroyNonemptyBag;

            // empty specific slot - check item fit to slot
            if (pItem2 == null || swap)
            {
                if (bag == InventorySlots.Bag0)
                {
                    // prevent cheating
                    if ((slot >= InventorySlots.BuyBackStart && slot < InventorySlots.BuyBackEnd) || slot >= (byte)PlayerSlots.End)
                        return InventoryResult.WrongBagType;
                }
                else
                {
                    Bag pBag = GetBagByPos(bag);
                    if (pBag == null)
                        return InventoryResult.WrongBagType;

                    ItemTemplate pBagProto = pBag.GetTemplate();
                    if (pBagProto == null)
                        return InventoryResult.WrongBagType;

                    if (slot >= pBagProto.ContainerSlots)
                        return InventoryResult.WrongBagType;

                    if (!Item.ItemCanGoIntoBag(pProto, pBagProto))
                        return InventoryResult.WrongBagType;
                }

                // non empty stack with space
                need_space = pProto.GetMaxStackSize();
            }
            // non empty slot, check item type
            else
            {
                // can be merged at least partly
                InventoryResult res = pItem2.CanBeMergedPartlyWith(pProto);
                if (res != InventoryResult.Ok)
                    return res;

                // free stack space or infinity
                need_space = pProto.GetMaxStackSize() - pItem2.GetStackCount();
            }

            if (need_space > count)
                need_space = count;

            ItemPosCount newPosition = new ItemPosCount((ushort)((int)bag << 8 | slot), need_space);
            if (!newPosition.isContainedIn(ref dest))
            {
                dest.Add(newPosition);
                count -= need_space;
            }
            return InventoryResult.Ok;
        }
        InventoryResult CanStoreItem_InBag(byte bag, ref List<ItemPosCount> dest, ItemTemplate pProto, ref uint count, bool merge, bool non_specialized, Item pSrcItem, byte skip_bag, byte skip_slot)
        {
            // skip specific bag already processed in first called CanStoreItem_InBag
            if (bag == skip_bag)
                return InventoryResult.WrongBagType;

            // skip not existed bag or self targeted bag
            Bag pBag = GetBagByPos(bag);
            if (pBag == null || pBag == pSrcItem)
                return InventoryResult.WrongBagType;

            if (pSrcItem != null && pSrcItem.IsNotEmptyBag())
                return InventoryResult.DestroyNonemptyBag;

            ItemTemplate pBagProto = pBag.GetTemplate();
            if (pBagProto == null)
                return InventoryResult.WrongBagType;

            // specialized bag mode or non-specilized
            if (non_specialized != (pBagProto.Class == ItemClass.Container && pBagProto.SubClass == (uint)ItemSubClassContainer.Container))
                return InventoryResult.WrongBagType;

            if (!Item.ItemCanGoIntoBag(pProto, pBagProto))
                return InventoryResult.WrongBagType;

            for (byte j = 0; j < pBag.GetBagSize(); j++)
            {
                // skip specific slot already processed in first called CanStoreItem_InSpecificSlot
                if (j == skip_slot)
                    continue;

                Item pItem2 = GetItemByPos(bag, j);

                // ignore move item (this slot will be empty at move)
                if (pItem2 == pSrcItem)
                    pItem2 = null;

                // if merge skip empty, if !merge skip non-empty
                if ((pItem2 != null) != merge)
                    continue;

                uint need_space = pProto.GetMaxStackSize();

                if (pItem2 != null)
                {
                    // can be merged at least partly
                    InventoryResult res = pItem2.CanBeMergedPartlyWith(pProto);
                    if (res != InventoryResult.Ok)
                        continue;

                    // descrease at current stacksize
                    need_space -= pItem2.GetStackCount();
                }

                if (need_space > count)
                    need_space = count;

                ItemPosCount newPosition = new ItemPosCount((ushort)(((int)bag << 8) | j), need_space);
                if (!newPosition.isContainedIn(ref dest))
                {
                    dest.Add(newPosition);
                    count -= need_space;

                    if (count == 0)
                        return InventoryResult.Ok;
                }
            }

            return InventoryResult.Ok;
        }


        InventoryResult CanEquipNewItem(byte slot, out ushort dest, uint item, bool swap)
        {
            dest = 0;
            Item pItem = Item.CreateItem(item, 1, this);
            if (pItem != null)
            {
                InventoryResult result = CanEquipItem(slot, out dest, pItem, swap);
                return result;
            }

            return InventoryResult.ItemNotFound;
        }
        public InventoryResult CanEquipItem(byte slot, out ushort dest, Item pItem, bool swap, bool not_loading = true)
        {
            dest = 0;
            if (pItem != null)
            {
                //sLog.outDebug(LOG_FILTER_PLAYER_ITEMS, "STORAGE: CanEquipItem slot = %u, item = %u, count = %u", slot, pItem.GetEntry(), pItem.GetCount());
                ItemTemplate pProto = pItem.GetTemplate();
                if (pProto != null)
                {
                    // item used
                    if (pItem.LootGenerated)
                        return InventoryResult.LootGone;

                    if (pItem.IsBindedNotWith(this))
                        return InventoryResult.NotOwner;

                    // check count of items (skip for auto move for same player from bank)
                    InventoryResult res = CanTakeMoreSimilarItems(pItem);
                    if (res != InventoryResult.Ok)
                        return res;

                    // check this only in game
                    if (not_loading)
                    {
                        // May be here should be more stronger checks; STUNNED checked
                        // ROOT, CONFUSED, DISTRACTED, FLEEING this needs to be checked.
                        //if (HasUnitState(UNIT_STATE_STUNNED))
                        //return InventoryResult.GenericStunned;

                        // do not allow equipping gear except weapons, offhands, projectiles, relics in
                        // - combat
                        // - in-progress arenas
                        //if (!pProto.CanChangeEquipStateInCombat())
                        {
                            //if (isInCombat())
                            //return InventoryResult.NotInCombat;

                            //if (Battleground* bg = GetBattleground())
                            //if (bg.isArena() && bg.GetStatus() == STATUS_IN_PROGRESS)
                            //return InventoryResult.NotDuringArenaMatch;
                        }

                        //if (isInCombat()&& (pProto.Class == ItemClass.Weapon || pProto.inventoryType == InventoryType.Relic) && m_weaponChangeTimer != 0)
                        //return InventoryResult.ClientLockedOut;         // maybe exist better err

                        //if (IsNonMeleeSpellCasted(false))
                        //return EQUIP_ERR_CLIENT_LOCKED_OUT;
                    }

                    //ScalingStatDistributionEntry const* ssd = pProto.ScalingStatDistribution ? sScalingStatDistributionStore.LookupEntry(pProto.ScalingStatDistribution) : 0;
                    // check allowed level (extend range to upper values if MaxLevel more or equal max player level, this let GM set high level with 1...max range items)
                    //if (ssd && ssd.MaxLevel < DEFAULT_MAX_LEVEL && ssd.MaxLevel < getLevel())
                    //return EQUIP_ERR_NOT_EQUIPPABLE;

                    byte eslot = FindEquipSlot(pProto, slot, swap);
                    if (eslot == ItemConst.NullSlot)
                        return InventoryResult.NotEquippable;

                    res = CanUseItem(pItem, not_loading);
                    if (res != InventoryResult.Ok)
                        return res;

                    if (!swap && GetItemByPos(InventorySlots.Bag0, eslot) != null)
                        return InventoryResult.NoSlotAvailable;

                    // if swap ignore item (equipped also)
                    InventoryResult res2 = CanEquipUniqueItem(pItem, (byte)(swap ? eslot : ItemConst.NullSlot));
                    if (res2 != InventoryResult.Ok)
                        return res2;

                    // check unique-equipped special item classes
                    if (pProto.Class == ItemClass.Quiver)
                        for (byte i = InventorySlots.BagStart; i < InventorySlots.BagEnd; ++i)
                        {
                            Item pBag = GetItemByPos(InventorySlots.Bag0, i);
                            if (pBag != null)
                            {
                                if (pBag != pItem)
                                {
                                    ItemTemplate pBagProto = pBag.GetTemplate();
                                    if (pBagProto != null)
                                        if (pBagProto.Class == pProto.Class && (!swap || pBag.GetSlot() != eslot))
                                            return (pBagProto.SubClass == (uint)ItemSubClassQuiver.AmmoPouch)
                                                ? InventoryResult.OnlyOneAmmo
                                                : InventoryResult.OnlyOneQuiver;
                                }
                            }
                        }

                    InventoryType type = pProto.inventoryType;

                    if (eslot == (byte)EquipmentSlot.OffHand)
                    {
                        // Do not allow polearm to be equipped in the offhand (rare case for the only 1h polearm 41750)
                        if (type == InventoryType.Weapon && pProto.SubClass == (uint)ItemSubClassWeapon.Polearm)
                            return InventoryResult.skillnotfound2h;
                        else if (type == InventoryType.Weapon || type == InventoryType.WeaponOffhand)
                        {
                            //if (!CanDualWield())
                            //return InventoryResult.skillnotfound2h;
                        }
                        else if (type == InventoryType.Weapon2Hand)
                        {
                            //if (!CanDualWield() || !CanTitanGrip())
                            //return InventoryResult.skillnotfound2h;
                        }

                        //if (IsTwoHandUsed())
                        //return InventoryResult.Equipped2handed;
                    }

                    // equip two-hand weapon case (with possible unequip 2 items)
                    if (type == InventoryType.Weapon2Hand)
                    {
                        if (eslot == (byte)EquipmentSlot.OffHand)
                        {
                            //if (!CanTitanGrip())
                            return InventoryResult.NotEquippable;
                        }
                        else if (eslot != (byte)EquipmentSlot.MainHand)
                            return InventoryResult.NotEquippable;

                        //if (!CanTitanGrip())
                        {
                            // offhand item must can be stored in inventory for offhand item and it also must be unequipped
                            Item offItem = GetItemByPos(InventorySlots.Bag0, (byte)EquipmentSlot.OffHand);
                            List<ItemPosCount> off_dest = new List<ItemPosCount>();
                            if (offItem != null && (!not_loading || CanUnequipItem((ushort)(((int)InventorySlots.Bag0 << 8) | (int)EquipmentSlot.OffHand), false) != InventoryResult.Ok ||
                                CanStoreItem(ItemConst.NullBag, ItemConst.NullSlot, ref off_dest, offItem, false) != InventoryResult.Ok))
                                return swap ? InventoryResult.CantSwap : InventoryResult.InvFull;
                        }
                    }
                    dest = (ushort)(((uint)InventorySlots.Bag0 << 8) | eslot);
                    return InventoryResult.Ok;
                }
            }
            return !swap ? InventoryResult.ItemNotFound : InventoryResult.CantSwap;
        }
        InventoryResult CanEquipUniqueItem(Item pItem, byte eslot, uint limit_count = 1)
        {
            ItemTemplate pProto = pItem.GetTemplate();

            // proto based limitations
            InventoryResult res = CanEquipUniqueItem(pProto, eslot, limit_count);
            if (res != InventoryResult.Ok)
                return res;

            // check unique-equipped on gems
            for (var enchant_slot = EnchantmentSlot.SOCK_ENCHANTMENT_SLOT; enchant_slot < EnchantmentSlot.SOCK_ENCHANTMENT_SLOT + 3; ++enchant_slot)
            {
                uint enchant_id = pItem.GetEnchantmentId(enchant_slot);
                if (enchant_id == 0)
                    continue;
                SpellItemEnchantmentEntry enchantEntry = DBCStorage.SpellItemEnchantmentStorage.LookupByKey(enchant_id);
                if (enchantEntry == null)
                    continue;

                ItemTemplate pGem = Cypher.ObjMgr.GetItemTemplate(enchantEntry.GemID);
                if (pGem == null)
                    continue;

                // include for check equip another gems with same limit category for not equipped item (and then not counted)
                uint gem_limit_count = (uint)(!pItem.IsEquipped() && pGem.ItemLimitCategory != 0 ? pItem.GetGemCountWithLimitCategory(pGem.ItemLimitCategory) : 1);

                InventoryResult ress = CanEquipUniqueItem(pGem, eslot, gem_limit_count);
                if (ress != InventoryResult.Ok)
                    return ress;
            }

            return InventoryResult.Ok;
        }
        InventoryResult CanEquipUniqueItem(ItemTemplate itemProto, byte except_slot, uint limit_count = 1)
        {
            // check unique-equipped on item
            if (Convert.ToBoolean(itemProto.Flags & ItemFlags.UniqueEquipped))
            {
                // there is an equip limit on this item
                //if (HasItemOrGemWithIdEquipped(itemProto.ItemId, 1, except_slot))
                return InventoryResult.ItemUniqueEquippable;
            }

            // check unique-equipped limit
            if (itemProto.ItemLimitCategory != 0)
            {
                //ItemLimitCategoryEntry const* limitEntry = sItemLimitCategoryStore.LookupEntry(itemProto.ItemLimitCategory);
                //if (!limitEntry)
                //return InventoryResult.NotEquippable;

                // NOTE: limitEntry.mode not checked because if item have have-limit then it applied and to equip case

                //if (limit_count > limitEntry.maxCount)
                //return InventoryResult.ItemMaxLimitCategoryEquippedExceededIs;

                // there is an equip limit on this item
                //if (HasItemOrGemWithLimitCategoryEquipped(itemProto.ItemLimitCategory, limitEntry.maxCount - limit_count + 1, except_slot))
                //return InventoryResult.ItemMaxCountEquippedSocketed;
            }

            return InventoryResult.Ok;
        }
        public InventoryResult CanUnequipItem(ushort pos, bool swap)
        {
            // Applied only to equipped items and bank bags
            if (!IsEquipmentPos(pos) && !IsBagPos(pos))
                return InventoryResult.Ok;

            Item pItem = GetItemByPos(pos);

            // Applied only to existed equipped item
            if (pItem == null)
                return InventoryResult.Ok;

            //sLog.outDebug(LOG_FILTER_PLAYER_ITEMS, "STORAGE: CanUnequipItem slot = %u, item = %u, count = %u", pos, pItem.GetEntry(), pItem.GetCount());

            ItemTemplate pProto = pItem.GetTemplate();
            if (pProto == null)
                return InventoryResult.ItemNotFound;

            // item used
            if (pItem.LootGenerated)
                return InventoryResult.LootGone;

            // do not allow unequipping gear except weapons, offhands, projectiles, relics in
            // - combat
            // - in-progress arenas
            //if (!pProto.CanChangeEquipStateInCombat())
            {
                //if (isInCombat())
                //return EQUIP_ERR_NOT_IN_COMBAT;

                //if (Battleground* bg = GetBattleground())
                //if (bg.isArena() && bg.GetStatus() == STATUS_IN_PROGRESS)
                //return EQUIP_ERR_NOT_DURING_ARENA_MATCH;
            }

            if (!swap && pItem.IsNotEmptyBag())
                return InventoryResult.DestroyNonemptyBag;

            return InventoryResult.Ok;
        }
        InventoryResult CanTakeMoreSimilarItems(Item pItem)
        {
            uint blah;
            return CanTakeMoreSimilarItems(pItem.GetEntry(), pItem.GetStackCount(), pItem, out blah);
        }
        InventoryResult CanTakeMoreSimilarItems(uint entry, uint count)
        {
            uint blah;
            return CanTakeMoreSimilarItems(entry, count, null, out blah);
        }
        InventoryResult CanTakeMoreSimilarItems(uint entry, uint count, Item pItem, out uint no_space_count)
        {
            no_space_count = 0;
            ItemTemplate pProto = Cypher.ObjMgr.GetItemTemplate(entry);
            if (pProto == null)
            {
                no_space_count = count;
                return InventoryResult.ItemMaxCount;
            }

            //if (pItem != null && pItem.m_lootGenerated)
                //return InventoryResult.LootGone;

            // no maximum
            if ((pProto.MaxCount <= 0 && pProto.ItemLimitCategory == 0) || pProto.MaxCount == 2147483647)
                return InventoryResult.Ok;

            if (pProto.MaxCount > 0)
            {
                uint curcount = GetItemCount(pProto.ItemId, true, pItem);
                if (curcount + count > pProto.MaxCount)
                {
                    no_space_count = count + curcount - pProto.MaxCount;
                    return InventoryResult.ItemMaxCount;
                }
            }

            // check unique-equipped limit
            if (pProto.ItemLimitCategory != 0)
            {
                /*
                ItemLimitCategoryEntry limitEntry = sItemLimitCategoryStore.LookupEntry(pProto.ItemLimitCategory);
                if (!limitEntry)
                {
                    no_space_count = count;
                    return EQUIP_ERR_NOT_EQUIPPABLE;
                }

                if (limitEntry.mode == ITEM_LIMIT_CATEGORY_MODE_HAVE)
                {
                    uint32 curcount = GetItemCountWithLimitCategory(pProto.ItemLimitCategory, pItem);
                    if (curcount + count > uint32(limitEntry.maxCount))
                    {
                        no_space_count = count + curcount - limitEntry.maxCount;
                        return EQUIP_ERR_ITEM_MAX_LIMIT_CATEGORY_COUNT_EXCEEDED_IS;
                    }
                }
                */
            }

            return InventoryResult.Ok;
        }
        InventoryResult CanUseItem(Item pItem, bool not_loading = true)
        {
            if (pItem != null)
            {
                //sLog.outDebug(LOG_FILTER_PLAYER_ITEMS, "STORAGE: CanUseItem item = %u", pItem.GetEntry());

                if (!isAlive() && not_loading)
                    return InventoryResult.PlayerDead;

                //if (isStunned())
                //    return EQUIP_ERR_GENERIC_STUNNED;

                ItemTemplate pProto = pItem.GetTemplate();
                if (pProto != null)
                {
                    if (pItem.IsBindedNotWith(this))
                        return InventoryResult.NotOwner;

                    InventoryResult res = CanUseItem(pProto);
                    if (res != InventoryResult.Ok)
                        return res;

                    if (pItem.GetSkill() != 0)
                    {
                        bool allowEquip = false;
                        uint itemSkill = pItem.GetSkill();
                        // Armor that is binded to account can "morph" from plate to mail, etc. if skill is not learned yet.
                        if (pProto.Quality == ItemQuality.Heirloom && pProto.Class == ItemClass.Armor && !HasSkill(itemSkill))
                        {
                            // TODO: when you right-click already equipped item it throws EQUIP_ERR_PROFICIENCY_NEEDED.

                            // In fact it's a visual bug, everything works properly... I need sniffs of operations with
                            // binded to account items from off server.

                            switch (GetClass())
                            {
                                case Class.Hunter:
                                case Class.Shaman:
                                    allowEquip = (itemSkill == (uint)Skill.Mail);
                                    break;
                                case Class.Paladin:
                                case Class.Warrior:
                                    allowEquip = (itemSkill == (uint)Skill.PlateMail);
                                    break;
                            }
                        }
                        if (!allowEquip && GetSkillValue(itemSkill) == 0)
                            return InventoryResult.ProficiencyNeeded;
                    }

                    if (pProto.RequiredReputationFaction != 0 && (uint)GetReputationRank(pProto.RequiredReputationFaction) < pProto.RequiredReputationRank)
                        return InventoryResult.CantEquipReputation;

                    return InventoryResult.Ok;
                }
            }
            return InventoryResult.ItemNotFound;
        }
        InventoryResult CanUseItem(ItemTemplate proto)
        {
            // Used by group, function NeedBeforeGreed, to know if a prototype can be used by a player

            if (proto != null)
            {
                if (Convert.ToBoolean(proto.Flags2 & ItemFlags2.HordeOnly) && GetTeam() != Team.Horde)
                    return InventoryResult.CantEquipEver;

                if (Convert.ToBoolean(proto.Flags2 & ItemFlags2.AllianceOnly) && GetTeam() != Team.Alliance)
                    return InventoryResult.CantEquipEver;

                if ((proto.AllowableClass & getClassMask()) == 0 || (proto.AllowableRace & getRaceMask()) == 0)
                    return InventoryResult.CantEquipEver;

                if (proto.RequiredSkill != 0)
                {
                    if (GetSkillValue(proto.RequiredSkill) == 0)
                        return InventoryResult.ProficiencyNeeded;
                    else if (GetSkillValue(proto.RequiredSkill) < proto.RequiredSkillRank)
                        return InventoryResult.CantEquipSkill;
                }

                if (proto.RequiredSpell != 0 && !HasSpell(proto.RequiredSpell))
                    return InventoryResult.ProficiencyNeeded;

                if (getLevel() < proto.RequiredLevel)
                    return InventoryResult.CantEquipLevelI;

                // If World Event is not active, prevent using event dependant items
                //if (proto.HolidayId && !IsHolidayActive((HolidayIds)proto.HolidayId))
                //return EQUIP_ERR_CLIENT_LOCKED_OUT;

                return InventoryResult.Ok;
            }

            return InventoryResult.ItemNotFound;
        }
        public InventoryResult CanBankItem(byte bag, byte slot, ref List<ItemPosCount> dest, Item pItem, bool swap, bool not_loading = true)
        {
            if (pItem == null)
                return swap ? InventoryResult.CantSwap : InventoryResult.ItemNotFound;

            uint count = pItem.GetStackCount();

            Log.outDebug("STORAGE: CanBankItem bag = {0}, slot = {1}, item = {2}, count = {3}", bag, slot, pItem.GetEntry(), count);
            ItemTemplate pProto = pItem.GetTemplate();
            if (pProto == null)
                return swap ? InventoryResult.CantSwap : InventoryResult.ItemNotFound;

            // item used
            if (pItem.LootGenerated)
                return InventoryResult.LootGone;

            if (pItem.IsBindedNotWith(this))
                return InventoryResult.NotOwner;

            // Currency tokens are not supposed to be swapped out of their hidden bag
            //if(pItem.IsCurrencyToken())
            {
                //sLog.outError(LOG_FILTER_PLAYER, "Possible hacking attempt: Player %s [guid: %u] tried to move token [guid: %u, entry: %u] out of the currency bag!",
                //GetName(), GetGUIDLow(), pItem.GetGUIDLow(), pProto.ItemId);
                //return EQUIP_ERR_CANT_SWAP;
            }

            // check count of items (skip for auto move for same player from bank)
            InventoryResult res = CanTakeMoreSimilarItems(pItem);
            if (res != InventoryResult.Ok)
                return res;

            // in specific slot
            if (bag != ItemConst.NullBag && slot != ItemConst.NullSlot)
            {
                if (slot >= InventorySlots.BagStart && slot < InventorySlots.BagEnd)
                {
                    if (!pItem.IsBag())
                        return InventoryResult.WrongSlot;

                    if (slot - InventorySlots.BagStart >= GetBankBagSlotCount())
                        return InventoryResult.NoBankSlot;

                    res = CanUseItem(pItem, not_loading);
                    if (res != InventoryResult.Ok)
                        return res;
                }

                res = CanStoreItem_InSpecificSlot(bag, slot, ref dest, pProto, ref count, swap, pItem);
                if (res != InventoryResult.Ok)
                    return res;

                if (count == 0)
                    return InventoryResult.Ok;
            }

            // not specific slot or have space for partly store only in specific slot

            // in specific bag
            if (bag != ItemConst.NullBag)
            {
                if (pItem.IsNotEmptyBag())
                    return InventoryResult.BagInBag;

                // search stack in bag for merge to
                if (pProto.Stackable != 1)
                {
                    if (bag == InventorySlots.Bag0)
                    {
                        res = CanStoreItem_InInventorySlots(InventorySlots.BankItemStart, InventorySlots.BankItemEnd, ref dest, pProto, ref count, true, pItem, bag, slot);
                        if (res != InventoryResult.Ok)
                            return res;

                        if (count == 0)
                            return InventoryResult.Ok;
                    }
                    else
                    {
                        res = CanStoreItem_InBag(bag, ref dest, pProto, ref count, true, false, pItem, ItemConst.NullBag, slot);
                        if (res != InventoryResult.Ok)
                            res = CanStoreItem_InBag(bag, ref dest, pProto, ref count, true, true, pItem, ItemConst.NullBag, slot);

                        if (res != InventoryResult.Ok)
                            return res;

                        if (count == 0)
                            return InventoryResult.Ok;
                    }
                }

                // search free slot in bag
                if (bag == InventorySlots.Bag0)
                {
                    res = CanStoreItem_InInventorySlots(InventorySlots.BankItemStart, InventorySlots.BankItemEnd, ref dest, pProto, ref count, false, pItem, bag, slot);
                    if (res != InventoryResult.Ok)
                        return res;

                    if (count == 0)
                        return InventoryResult.Ok;
                }
                else
                {
                    res = CanStoreItem_InBag(bag, ref dest, pProto, ref count, false, false, pItem, ItemConst.NullBag, slot);
                    if (res != InventoryResult.Ok)
                        res = CanStoreItem_InBag(bag, ref dest, pProto, ref count, false, true, pItem, ItemConst.NullBag, slot);

                    if (res != InventoryResult.Ok)
                        return res;

                    if (count == 0)
                        return InventoryResult.Ok;
                }
            }

            // not specific bag or have space for partly store only in specific bag

            // search stack for merge to
            if (pProto.Stackable != 1)
            {
                // in slots
                res = CanStoreItem_InInventorySlots(InventorySlots.BankItemStart, InventorySlots.BankItemEnd, ref dest, pProto, ref count, true, pItem, bag, slot);
                if (res != InventoryResult.Ok)
                    return res;

                if (count == 0)
                    return InventoryResult.Ok;

                // in special bags
                if (pProto.BagFamily != BagFamilyMask.NONE)
                {
                    for (byte i = InventorySlots.BankBagStart; i < InventorySlots.BankBagEnd; i++)
                    {
                        res = CanStoreItem_InBag(i, ref dest, pProto, ref count, true, false, pItem, bag, slot);
                        if (res != InventoryResult.Ok)
                            continue;

                        if (count == 0)
                            return InventoryResult.Ok;
                    }
                }

                for (byte i = InventorySlots.BankBagStart; i < InventorySlots.BankBagEnd; i++)
                {
                    res = CanStoreItem_InBag(i, ref dest, pProto, ref count, true, true, pItem, bag, slot);
                    if (res != InventoryResult.Ok)
                        continue;

                    if (count == 0)
                        return InventoryResult.Ok;
                }
            }

            // search free place in special bag
            if (pProto.BagFamily != BagFamilyMask.NONE)
            {
                for (byte i = InventorySlots.BagStart; i < InventorySlots.BankBagEnd; i++)
                {
                    res = CanStoreItem_InBag(i, ref dest, pProto, ref count, false, false, pItem, bag, slot);
                    if (res != InventoryResult.Ok)
                        continue;

                    if (count == 0)
                        return InventoryResult.Ok;
                }
            }

            // search free space
            res = CanStoreItem_InInventorySlots(InventorySlots.BankItemStart, InventorySlots.BankItemEnd, ref dest, pProto, ref count, false, pItem, bag, slot);
            if (res != InventoryResult.Ok)
                return res;

            if (count == 0)
                return InventoryResult.Ok;

            for (byte i = InventorySlots.BankBagStart; i < InventorySlots.BankBagEnd; i++)
            {
                res = CanStoreItem_InBag(i, ref dest, pProto, ref count, false, true, pItem, bag, slot);
                if (res != InventoryResult.Ok)
                    continue;

                if (count == 0)
                    return InventoryResult.Ok;
            }
            return InventoryResult.BankFull;
        }
        bool HasItemCount(uint item, uint count, bool inBankAlso = false)
        {
            uint tempcount = 0;
            for (byte i = (byte)EquipmentSlot.Start; i < InventorySlots.ItemEnd; i++)
            {
                Item pItem = GetItemByPos(InventorySlots.Bag0, i);
                if (pItem != null && pItem.GetEntry() == item)// && !pItem.IsInTrade())//todo fix
                {
                    tempcount += pItem.GetStackCount();
                    if (tempcount >= count)
                        return true;
                }
            }

            for (byte i = InventorySlots.BagStart; i < InventorySlots.BagEnd; i++)
            {
                Bag pBag = GetBagByPos(i);
                if (pBag != null)
                {
                    for (byte j = 0; j < pBag.GetBagSize(); j++)
                    {
                        Item pItem = GetItemByPos(i, j);
                        if (pItem != null && pItem.GetEntry() == item)// && !pItem.IsInTrade()) //todo fix me
                        {
                            tempcount += pItem.GetStackCount();
                            if (tempcount >= count)
                                return true;
                        }
                    }
                }
            }

            if (inBankAlso)
            {
                for (byte i = InventorySlots.BankItemStart; i < InventorySlots.BankItemEnd; i++)
                {
                    Item pItem = GetItemByPos(InventorySlots.Bag0, i);
                    if (pItem != null && pItem.GetEntry() == item)// && !pItem.IsInTrade())
                    {
                        tempcount += pItem.GetStackCount();
                        if (tempcount >= count)
                            return true;
                    }
                }
                for (byte i = InventorySlots.BankBagStart; i < InventorySlots.BankBagEnd; i++)
                {
                    Bag pBag = GetBagByPos(i);
                    if (pBag != null)
                    {
                        for (byte j = 0; j < pBag.GetBagSize(); j++)
                        {
                            Item pItem = GetItemByPos(i, j);
                            if (pItem != null && pItem.GetEntry() == item)//&& !pItem.IsInTrade())
                            {
                                tempcount += pItem.GetStackCount();
                                if (tempcount >= count)
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void AddItemToBuyBackSlot(Item pItem)
        {
            if (pItem != null)
            {
                uint slot = 0;// m_currentBuybackSlot;
                // if current back slot non-empty search oldest or free
                if (Items[slot] != null)
                {
                    uint oldest_time = GetValue<uint>(PlayerFields.BuybackTimestamp);
                    uint oldest_slot = InventorySlots.BuyBackStart;

                    for (byte i = InventorySlots.BuyBackStart + 1; i < InventorySlots.BuyBackEnd; ++i)
                    {
                        // found empty
                        if (Items[i] == null)
                        {
                            slot = i;
                            break;
                        }

                        uint i_time = GetValue<uint>(PlayerFields.BuybackTimestamp + i - InventorySlots.BuyBackStart);

                        if (oldest_time > i_time)
                        {
                            oldest_time = i_time;
                            oldest_slot = i;
                        }
                    }

                    // find oldest
                    slot = oldest_slot;
                }

                //RemoveItemFromBuyBackSlot(slot, true);
                Log.outDebug("STORAGE: AddItemToBuyBackSlot item = {0}, slot = {1}", pItem.GetEntry(), slot);

                Items[slot] = pItem;
                var time = DateTime.Now;
                //uint etime = uint(time - m_logintime + (30 * 3600));
                int eslot = (int)slot - InventorySlots.BuyBackStart;

                //SetValue<uint>(PlayerFields.BuyBackSlots + (eslot * 2), (uint)pItem.GetGUID());
                ItemTemplate proto = pItem.GetTemplate();
                if (proto != null)
                    SetValue<uint>(PlayerFields.BuybackPrice + eslot, proto.SellPrice * pItem.GetStackCount());
                else
                    SetValue<uint>(PlayerFields.BuybackPrice + eslot, 0);
                //SetValue<uint>(PlayerFields.BuybackTimestamp + eslot, etime);

                // move to next (for non filled list is move most optimized choice)
                //if (m_currentBuybackSlot < InventorySlots.BuyBackEnd - 1)
                    //++m_currentBuybackSlot;
            }
        }

        //Load
        Item LoadItem(uint guid, uint entry, uint timeDiff)
        {
            Item item = null;
            ItemTemplate proto = Cypher.ObjMgr.GetItemTemplate(entry);
            if (proto != null)
            {
                bool remove = false;
                item = Cypher.ItemMgr.NewItemOrBag(proto);

                if (item.LoadFromDB(guid, GetGUID(), entry))
                {
                    PreparedStatement stmt = null;

                    // Do not allow to have item limited to another map/zone in alive state
                    //if (isAlive() && item.IsLimitedToAnotherMapOrZone(pl.GetMapId(), pl.Zone))
                    {
                        //Log.outDebug("LoadInventory: player (GUID: %u, name: '%s', map: %u) has item (GUID: %u, entry: %u) limited to another map (%u). Deleting item.",
                        //pl.GetGUIDLow(), pl.Name, pl.GetMapId(), item.GetGUIDLow(), item.GetEntry(), pl.Zone);
                        //remove = true;
                    }
                    // "Conjured items disappear if you are logged out for more than 15 minutes"
                    //else if (timeDiff > 15 * MINUTE && proto.Flags & (ItemFlags.Conjured))
                    {
                        // Log.outDebug("LoadInventory: player (GUID: {0}, name: {1}, diff: {2}) has conjured item (GUID: {3}, entry: {4}) with expired lifetime (15 minutes). Deleting item.",
                        // pl.GetGUIDLow(), pl.Name, timeDiff, item.GetGUIDLow(), item.GetEntry());
                        //remove = true;
                    }
                    if (item.HasFlag((int)ItemFields.Flags, ItemFieldFlags.Refundable))
                    {
                        //if (item.GetPlayedTime() > (2 * HOUR))
                        {
                            //Log.outDebug("LoadInventory: player (GUID: {0}, name: {1}) has item (GUID: {2}, entry: {3}) with expired refund time ({4}). Deleting refund data and removing " +
                            //"efundable flag.", pl.GetGUIDLow(), pl.Name, item.GetGUIDLow(), item.GetEntry(), item.GetPlayedTime());

                            //stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_ITEM_REFUND_INSTANCE);
                            //stmt.AddValue(0, item.GetGUIDLow());
                            //DB.Characters.Execute(stmt);

                            //item.RemoveFlag(ITEM_FIELD_FLAGS, ITEM_FLAG_REFUNDABLE);
                        }
                        //else
                        {
                            stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_ITEM_REFUNDS);
                            stmt.AddValue(0, item.GetGUIDLow());
                            stmt.AddValue(1, GetGUIDLow());
                            if (DB.Characters.Execute(stmt))
                            {
                                //item.SetRefundRecipient((*result)[0].GetUInt32());
                                //item.SetPaidMoney((*result)[1].GetUInt32());
                                //item.SetPaidExtendedCost((*result)[2].GetUInt16());
                                //AddRefundReference(item.GetGUIDLow());
                            }
                            else
                            {
                                Log.outDebug("LoadInventory: player (GUID: {0}, name: {1}) has item (GUID: {2}, entry: {3}) with refundable flags, but without data in item_refund_instance. Removing flag.",
                                    GetGUIDLow(), GetName(), item.GetGUIDLow(), item.GetEntry());
                                //item.RemoveFlag(ITEM_FIELD_FLAGS, ITEM_FLAG_REFUNDABLE);
                            }
                        }
                    }
                    else if (item.HasFlag((int)ItemFields.Flags, ItemFieldFlags.BopTradeable))
                    {
                        stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_ITEM_BOP_TRADE);
                        stmt.AddValue(0, item.GetGUIDLow());
                        if (DB.Characters.Execute(stmt))
                        {
                            //string strGUID = (*result)[0].GetString();
                            //Tokens GUIDlist(strGUID, ' ');
                            //AllowedLooterSet looters;
                            //for (Tokens::iterator itr = GUIDlist.begin(); itr != GUIDlist.end(); ++itr)
                            //looters.insert(atol(*itr));
                            //item.SetSoulboundTradeable(looters);
                            //AddTradeableItem(item);
                        }
                        else
                        {
                            Log.outDebug("LoadInventory: player (GUID: {0}, name: {1}) has item (GUID: {2}, entry: {3}) with ITEM_FLAG_BOP_TRADEABLE flag, " +
                                "but without data in item_soulbound_trade_data. Removing flag.", GetGUIDLow(), GetName(), item.GetGUIDLow(), item.GetEntry());
                            //item.RemoveFlag(ITEM_FIELD_FLAGS, ITEM_FLAG_BOP_TRADEABLE);
                        }
                    }
                    else if (proto.HolidayId != 0)
                    {
                        remove = true;
                        //GameEventMgr::GameEventDataMap const& events = sGameEventMgr.GetEventMap();
                        //GameEventMgr::ActiveEvents const& activeEventsList = sGameEventMgr.GetActiveEventList();
                        //for (GameEventMgr::ActiveEvents::const_iterator itr = activeEventsList.begin(); itr != activeEventsList.end(); ++itr)
                        {
                            //if (uint32(events[*itr].holiday_id) == proto.HolidayId)
                            {
                                //remove = false;
                                //break;
                            }
                        }
                    }
                }
                else
                {
                    Log.outError("LoadInventory: player (GUID: {0}, name: {1}) has broken item (GUID: {2}, entry: {3}) in inventory. Deleting item.",
                        GetGUIDLow(), GetName(), guid, entry);
                    remove = true;
                }
                // Remove item from inventory if necessary
                if (remove)
                {
                    //ItemDeleteFromInventoryDB(trans, itemGuid);
                    item.FSetState(ItemUpdateState.Removed);
                    item.SaveToDB();                           // it also deletes item object!
                    item = null;
                }
            }
            else
            {
                Log.outError("LoadInventory: player (GUID: {0}, name: {1}) has unknown item (entry: {2}) in inventory. Deleting item.",
                    GetGUIDLow(), GetName(), entry);
                //Item::DeleteFromInventoryDB(trans, itemGuid);
                //Item::DeleteFromDB(trans, itemGuid);
            }
            return item;
        }

        //Add
        // Return stored item (if stored to stack, it can diff. from pItem). And pItem ca be deleted in this case.
        Item _StoreItem(ushort pos, Item pItem, uint count, bool clone, bool update)
        {
            if (pItem == null)
                return null;

            byte bag = (byte)(pos >> 8);
            byte slot = (byte)(pos & 255);

            //sLog.outDebug(LOG_FILTER_PLAYER_ITEMS, "STORAGE: StoreItem bag = %u, slot = %u, item = %u, count = %u, guid = %u", bag, slot, pItem.GetEntry(), count, pItem.GetGUIDLow());

            Item pItem2 = GetItemByPos(bag, slot);

            if (pItem2 == null)
            {
                if (clone)
                    pItem = pItem.CloneItem(count, this);
                else
                    pItem.SetStackCount(count);

                if (pItem == null)
                    return null;

                if (pItem.GetTemplate().Bonding == ItemBondingType.PickedUp ||
                    pItem.GetTemplate().Bonding == ItemBondingType.QuestItem ||
                    (pItem.GetTemplate().Bonding == ItemBondingType.Equiped && IsBagPos(pos)))
                    pItem.SetBinding(true);

                Bag pBag = bag == InventorySlots.Bag0 ? null : GetBagByPos(bag);
                if (pBag == null)
                {
                    Items[slot] = pItem;
                    SetValue<ulong>(PlayerFields.InvSlots + (slot * 2), pItem.GetGUID());
                    pItem.SetValue<ulong>(ItemFields.ContainedIn, GetGUID());
                    pItem.SetValue<ulong>(ItemFields.Owner, GetGUID());

                    pItem.SetSlot(slot);
                    pItem.SetContainer(null);
                }
                else
                    pBag.StoreItem(slot, pItem, update);

                if (IsInWorld && update)
                {
                    pItem.AddToWorld();
                    pItem.SendUpdateToPlayer(this);
                }

                pItem.SetState(ItemUpdateState.Changed, this);
                if (pBag != null)
                    pBag.SetState(ItemUpdateState.Changed, this);

                //AddEnchantmentDurations(pItem);
                //AddItemDurations(pItem);


                ItemTemplate proto = pItem.GetTemplate();
                //for (byte i = 0; i < MAX_ITEM_PROTO_SPELLS; ++i)
                //if (proto.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_ON_NO_DELAY_USE && proto.Spells[i].SpellId > 0) // On obtain trigger
                //if (bag == INVENTORY_SLOT_BAG_0 || (bag >= INVENTORY_SLOT_BAG_START && bag < INVENTORY_SLOT_BAG_END))
                //if (!HasAura(proto.Spells[i].SpellId))
                // CastSpell(this, proto.Spells[i].SpellId, true, pItem);

                return pItem;
            }
            else
            {
                if (pItem2.GetTemplate().Bonding == ItemBondingType.PickedUp ||
                    pItem2.GetTemplate().Bonding == ItemBondingType.QuestItem ||
                    (pItem2.GetTemplate().Bonding == ItemBondingType.Equiped && IsBagPos(pos)))
                    pItem2.SetBinding(true);

                pItem2.SetStackCount(pItem2.GetStackCount() + count);
                if (IsInWorld && update)
                    pItem2.SendUpdateToPlayer(this);

                if (!clone)
                {
                    // delete item (it not in any slot currently)
                    if (IsInWorld && update)
                    {
                        pItem.RemoveFromWorld();
                        pItem.DestroyForPlayer(this);
                    }

                    //RemoveEnchantmentDurations(pItem);
                    //RemoveItemDurations(pItem);

                    pItem.SetOwnerGUID(GetGUID());                 // prevent error at next SetState in case trade/mail/buy from vendor
                    pItem.SetNotRefundable(this);
                    pItem.ClearSoulboundTradeable(this);
                    //RemoveTradeableItem(pItem);
                    pItem.SetState(ItemUpdateState.Removed, this);
                }

                //AddEnchantmentDurations(pItem2);

                pItem2.SetState(ItemUpdateState.Changed, this);

                //const ItemTemplate* proto = pItem2.GetTemplate();
                //for (uint8 i = 0; i < MAX_ITEM_PROTO_SPELLS; ++i)
                //if (proto.Spells[i].SpellTrigger == ITEM_SPELLTRIGGER_ON_NO_DELAY_USE && proto.Spells[i].SpellId > 0) // On obtain trigger
                //if (bag == INVENTORY_SLOT_BAG_0 || (bag >= INVENTORY_SLOT_BAG_START && bag < INVENTORY_SLOT_BAG_END))
                //if (!HasAura(proto.Spells[i].SpellId))
                //CastSpell(this, proto.Spells[i].SpellId, true, pItem2);

                return pItem2;
            }
        }
        public Item StoreItem(List<ItemPosCount> dest, Item pItem, bool update)
        {
            if (pItem == null)
                return null;

            Item lastItem = pItem;
            for (var i = 0; i < dest.Count; i++)// itr in dest)
            {
                var itr = dest[i];
                ushort pos = itr.pos;
                uint count = itr.count;

                if (i == dest.Count() -1)
                {
                    lastItem = _StoreItem(pos, pItem, count, false, update);
                    break;
                }

                lastItem = _StoreItem(pos, pItem, count, true, update);
            }
            return lastItem;
        }
        bool StoreNewItemInBestSlots(uint titem_id, uint titem_amount)
        {
            //sLog.outDebug(LOG_FILTER_PLAYER_ITEMS, "STORAGE: Creating initial item, itemId = %u, count = %u", titem_id, titem_amount);
            InventoryResult msg;
            // attempt equip by one
            while (titem_amount > 0)
            {
                ushort eDest = 0;
                msg = CanEquipNewItem(ItemConst.NullSlot, out eDest, titem_id, false);
                if (msg != InventoryResult.Ok)
                    break;

                EquipNewItem(eDest, titem_id, true);
                //AutoUnequipOffhandIfNeed();
                titem_amount--;
            }

            if (titem_amount == 0)
                return true;                                        // equipped

            // attempt store
            List<ItemPosCount> sDest = new List<ItemPosCount>();
            // store in main bag to simplify second pass (special bags can be not equipped yet at this moment)
            msg = CanStoreNewItem(InventorySlots.Bag0, ItemConst.NullSlot, ref sDest, titem_id, titem_amount);
            if (msg == InventoryResult.Ok)
            {
                StoreNewItem(ref sDest, titem_id, true, Item.GenerateItemRandomPropertyId(titem_id));
                return true;                                        // stored
            }

            // item can't be added
            Log.outError("STORAGE: Can't equip or store initial item {0} for race {1} class {2}, error msg = {3}", titem_id, getRace(), getClass(), msg);
            return false;
        }

        Item StoreNewItem(ref List<ItemPosCount> dest, uint item, bool update, int randomPropertyId = 0)
        {
            List<uint> allowedLooters = new List<uint>();
            return StoreNewItem(ref dest, item, update, randomPropertyId, ref allowedLooters);
        }
        // Return stored item (if stored to stack, it can diff. from pItem). And pItem ca be deleted in this case.
        Item StoreNewItem(ref List<ItemPosCount> dest, uint item, bool update, int randomPropertyId, ref List<uint> allowedLooters)
        {
            uint count = 0;
            foreach (var itr in dest)
                count += itr.count;

            Item pItem = Item.CreateItem(item, count, this);
            if (pItem != null)
            {
                //ItemAddedQuestCheck(item, count);
                //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_RECEIVE_EPIC_ITEM, item, count);
                //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_OWN_ITEM, item, 1);
                //if (randomPropertyId != 0)
                //pItem.SetItemRandomProperties(randomPropertyId);
                pItem = StoreItem(dest, pItem, update);

                if (allowedLooters.Count > 1 && pItem.GetTemplate().GetMaxStackSize() == 1 && pItem.IsSoulBound())
                {
                    //pItem.SetSoulboundTradeable(allowedLooters);
                    //pItem.SetValue<uint>(ItemFields.CreatePlayedTime, GetTotalPlayedTime());
                    //AddTradeableItem(pItem);

                    // save data
                    StringBuilder ss = new StringBuilder();
                    foreach (var itr in allowedLooters)
                        ss.AppendFormat("{0} ", itr);

                    PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_INS_ITEM_BOP_TRADE);
                    stmt.AddValue(0, pItem.GetGUIDLow());
                    stmt.AddValue(1, ss.ToString());
                    DB.Characters.Execute(stmt);
                }
            }
            return pItem;
        }

        bool _StoreOrEquipNewItem(uint vendorslot, uint item, byte count, byte bag, byte slot, int price, ItemTemplate pProto, Creature pVendor, VendorItem crItem, bool bStore)
        {
            List<ItemPosCount> vDest = new List<ItemPosCount>();
            ushort uiDest = 0;
            InventoryResult msg = bStore ? CanStoreNewItem(bag, slot, ref vDest, item, count) : CanEquipNewItem(slot, out uiDest, item, false);
            if (msg != InventoryResult.Ok)
            {
                SendEquipError(msg, null, null, item);
                return false;
            }

            ModifyMoney(-price);

            if (crItem.ExtendedCost != 0) // case for new honor system
            {
                ItemExtendedCostEntry iece = DB2Storage.ItemExtendedCostStorage.LookupByKey(crItem.ExtendedCost);
                for (int i = 0; i < ItemConst.MaxExtCostItems; ++i)
                {
                    //if (iece.RequiredItem[i] != 0)
                    //DestroyItemCount(iece.RequiredItem[i], iece.RequiredItemCount[i], true);
                }

                for (int i = 0; i < ItemConst.MaxExtCostCurrencies; ++i)
                {
                    //if (iece.RequiredCurrency[i] != 0)
                    //ModifyCurrency(iece.RequiredCurrency[i], -int32(iece.RequiredCurrencyCount[i]), true, true);
                }
            }

            Item it = bStore ? StoreNewItem(ref vDest, item, true) : EquipNewItem(uiDest, item, true);
            if (it != null)
            {
                uint new_count = pVendor.UpdateVendorItemCurrentCount(crItem, count);

                PacketWriter data = new PacketWriter(Opcodes.SMSG_BuyItem);
                data.WriteUInt64(pVendor.GetGUID());
                data.WriteUInt32(vendorslot + 1);                   // numbered from 1 at client
                data.WriteUInt32(crItem.maxcount > 0 ? new_count : 0xFFFFFFFF);
                data.WriteUInt32(count);
                GetSession().Send(data);
                SendNewItem(it, count, true, false, false);

                //if (!bStore)
                //AutoUnequipOffhandIfNeed();

                //if (pProto.Flags & ITEM_PROTO_FLAG_REFUNDABLE && crItem.ExtendedCost && pProto.GetMaxStackSize() == 1)
                {
                    //it.SetFlag(ITEM_FIELD_FLAGS, ITEM_FLAG_REFUNDABLE);
                    //it.SetRefundRecipient(GetGUIDLow());
                    //it.SetPaidMoney(price);
                    //it.SetPaidExtendedCost(crItem.ExtendedCost);
                    //it.SaveRefundDataToDB();
                    //AddRefundReference(it.GetGUIDLow());
                }
            }
            return true;
        }
        Item EquipNewItem(ushort pos, uint item, bool update)
        {
            Item pItem = Item.CreateItem(item, 1, this);
            if (pItem != null)
            {
                //ItemAddedQuestCheck(item, 1);
                //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_RECEIVE_EPIC_ITEM, item, 1);
                return EquipItem(pos, pItem, update);
            }

            return null;
        }
        public Item EquipItem(ushort pos, Item pItem, bool update)
        {
            //AddEnchantmentDurations(pItem);
            //AddItemDurations(pItem);

            byte bag = (byte)(pos >> 8);
            byte slot = (byte)(pos & 255);

            Item pItem2 = GetItemByPos(bag, slot);

            if (pItem2 == null)
            {
                VisualizeItem(slot, pItem);

                if (isAlive())
                {
                    ItemTemplate pProto = pItem.GetTemplate();

                    // item set bonuses applied only at equip and removed at unequip, and still active for broken items
                    //if (pProto != null && pProto.ItemSet != 0)
                    //AddItemsSetItem(this, pItem);

                    //_ApplyItemMods(pItem, slot, true);

                    //if (pProto && isInCombat() && (pProto.Class == ITEM_CLASS_WEAPON || pProto.InventoryType == INVTYPE_RELIC) && m_weaponChangeTimer == 0)
                    {
                        //uint32 cooldownSpell = getClass() == CLASS_ROGUE ? 6123 : 6119;
                        //SpellInfo const* spellProto = sSpellMgr.GetSpellInfo(cooldownSpell);

                        //if (!spellProto)
                        //sLog.outError(LOG_FILTER_PLAYER, "Weapon switch cooldown spell %u couldn't be found in Spell.dbc", cooldownSpell);
                        //else
                        {
                            //m_weaponChangeTimer = spellProto.StartRecoveryTime;

                            //GetGlobalCooldownMgr().AddGlobalCooldown(spellProto, m_weaponChangeTimer);

                            //WorldPacket data(SMSG_SPELL_COOLDOWN, 8+1+4);
                            //data << uint64(GetGUID());
                            //data << uint8(1);
                            //data << uint32(cooldownSpell);
                            //data << uint32(0);
                            //GetSession().SendPacket(&data);
                        }
                    }
                }

                if (IsInWorld && update)
                {
                    pItem.AddToWorld();
                    pItem.SendUpdateToPlayer(this);
                }

                //ApplyEquipCooldown(pItem);

                // update expertise and armor penetration - passive auras may need it

                //if (slot == (byte)EquipmentSlot.MainHand)
                //UpdateExpertise(BASE_ATTACK);
                //else if (slot == (byte)EquipmentSlot.OffHand)
                //UpdateExpertise(OFF_ATTACK);

                switch ((EquipmentSlot)slot)
                {
                    case EquipmentSlot.MainHand:
                    case EquipmentSlot.OffHand:
                    case EquipmentSlot.Ranged:
                    //RecalculateRating(CR_ARMOR_PENETRATION);
                    default:
                        break;
                }
            }
            else
            {
                pItem2.SetStackCount(pItem2.GetStackCount() + pItem.GetStackCount());
                if (IsInWorld && update)
                    pItem2.SendUpdateToPlayer(this);

                // delete item (it not in any slot currently)
                pItem.DeleteFromDB();
                if (IsInWorld && update)
                {
                    pItem.RemoveFromWorld();
                    pItem.DestroyForPlayer(this);
                }

                //RemoveEnchantmentDurations(pItem);
                //RemoveItemDurations(pItem);

                pItem.SetOwnerGUID(GetGUID());                     // prevent error at next SetState in case trade/mail/buy from vendor
                pItem.SetNotRefundable(this);
                pItem.ClearSoulboundTradeable(this);
                //RemoveTradeableItem(pItem);
                pItem.SetState(ItemUpdateState.Removed, this);
                pItem2.SetState(ItemUpdateState.Changed, this);

                //ApplyEquipCooldown(pItem2);

                return pItem2;
            }

            // only for full equip instead adding to stack
            //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_EQUIP_ITEM, pItem.GetEntry());
            //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_EQUIP_EPIC_ITEM, pItem.GetEntry(), slot);

            return pItem;
        }
        void QuickEquipItem(ushort pos, Item pItem)
        {
            if (pItem != null)
            {
                //AddEnchantmentDurations(pItem);
                //AddItemDurations(pItem);

                byte slot = (byte)(pos & 255);
                VisualizeItem(slot, pItem);

                if (IsInWorld)
                {
                    pItem.AddToWorld();
                    pItem.SendUpdateToPlayer(this);
                }

                //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_EQUIP_ITEM, pItem.GetEntry());
                //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_EQUIP_EPIC_ITEM, pItem.GetEntry(), slot);
            }
        }
        void VisualizeItem(byte slot, Item pItem)
        {
            if (pItem == null)
                return;

            // check also  BIND_WHEN_PICKED_UP and BIND_QUEST_ITEM for .additem or .additemset case by GM (not binded at adding to inventory)
            if (pItem.GetTemplate().Bonding == ItemBondingType.Equiped || pItem.GetTemplate().Bonding == ItemBondingType.PickedUp || pItem.GetTemplate().Bonding == ItemBondingType.QuestItem)
                pItem.SetBinding(true);

            Log.outDebug("STORAGE: EquipItem slot = {0}, item = {1}", slot, pItem.GetEntry());

            Items[slot] = pItem;
            SetValue<ulong>(PlayerFields.InvSlots + (slot * 2), pItem.GetPackGUID());
            pItem.SetValue<ulong>(ItemFields.ContainedIn, GetPackGUID());
            pItem.SetValue<ulong>(ItemFields.Owner, GetPackGUID());
            pItem.SetSlot(slot);
            pItem.SetContainer(null);

            if (slot < (byte)EquipmentSlot.End)
                SetVisibleItemSlot(slot, pItem);

            pItem.SetState(ItemUpdateState.Changed, this);
        }
        public Item BankItem(List<ItemPosCount> dest, Item pItem, bool update)
        {
            return StoreItem(dest, pItem, update);
        }
        // Return true is the bought item has a max count to force refresh of window by caller
        public bool BuyItemFromVendorSlot(ulong vendorguid, uint vendorslot, uint item, byte count, byte bag, byte slot)
        {
            // cheating attempt
            if (count < 1)
                count = 1;

            // cheating attempt
            if (slot > ItemConst.MaxBagSize && slot != ItemConst.NullSlot)
                return false;

            if (!isAlive())
                return false;

            ItemTemplate pProto = Cypher.ObjMgr.GetItemTemplate(item);
            if (pProto == null)
            {
                SendBuyError(BuyResult.CantFindItem, null, item);
                return false;
            }

            Creature creature = GetNPCIfCanInteractWith(vendorguid, NPCFlags.Vendor);
            if (creature == null)
            {
                Log.outDebug("WORLD: BuyItemFromVendor - Unit (GUID: {0}) not found or you can't interact with him.", ObjectGuid.GuidLowPart(vendorguid));
                SendBuyError(BuyResult.DistanceTooFar, null, item);
                return false;
            }

            VendorItemData vItems = creature.GetVendorItems();
            if (vItems == null || vItems.Empty())
            {
                SendBuyError(BuyResult.CantFindItem, creature, item);
                return false;
            }

            if (vendorslot >= vItems.GetItemCount())
            {
                SendBuyError(BuyResult.CantFindItem, creature, item);
                return false;
            }

            VendorItem crItem = vItems.GetItem(vendorslot);
            // store diff item (cheating)
            if (crItem == null || crItem.item != item)
            {
                SendBuyError(BuyResult.CantFindItem, creature, item);
                return false;
            }

            // check current item amount if it limited
            if (crItem.maxcount != 0)
            {
                if (creature.GetVendorItemCurrentCount(crItem) < pProto.BuyCount * count)
                {
                    SendBuyError(BuyResult.ItemAlreadySold, creature, item);
                    return false;
                }
            }

            if (pProto.RequiredReputationFaction != 0 && ((uint)GetReputationRank(pProto.RequiredReputationFaction) < pProto.RequiredReputationRank))
            {
                SendBuyError(BuyResult.ReputationRequire, creature, item);
                return false;
            }

            if (crItem.ExtendedCost != 0)
            {
                // Can only buy full stacks for extended cost
                if (pProto.BuyCount != count)
                {
                    SendEquipError(InventoryResult.CantBuyQuantity, null, null);
                    return false;
                }

                ItemExtendedCostEntry iece = DB2Storage.ItemExtendedCostStorage.LookupByKey(crItem.ExtendedCost);
                if (iece == null)
                {
                    Log.outError("Item {0} have wrong ExtendedCost field value {1}", pProto.ItemId, crItem.ExtendedCost);
                    return false;
                }

                for (byte i = 0; i < ItemConst.MaxExtCostItems; ++i)
                {
                    if (iece.RequiredItem[i] != 0 && !HasItemCount(iece.RequiredItem[i], (iece.RequiredItemCount[i] * count)))
                    {
                        SendEquipError(InventoryResult.VendorMissingTurnins, null, null);
                        return false;
                    }
                }

                for (byte i = 0; i < ItemConst.MaxExtCostCurrencies; ++i)
                {
                    if (iece.RequiredCurrency[i] == 0)
                        continue;

                    CurrencyTypesEntry entry = DBCStorage.CurrencyTypesStorage.LookupByKey(iece.RequiredCurrency[i]);
                    if (entry == null)
                    {
                        SendBuyError(BuyResult.CantFindItem, creature, item);
                        return false;
                    }

                    uint precision = (uint)(Convert.ToBoolean(entry.Flags & (uint)CurrencyFlags.HighPrecision) ? 100 : 1);

                    //if (!HasCurrency(iece.RequiredCurrency[i], (iece.RequiredCurrencyCount[i] * count) / precision)) todo fixme
                    {
                        //SendEquipError(InventoryResult.VendorMissingTurnins, null, null);
                        //return false;
                    }
                }

                // check for personal arena rating requirement
                //if (GetMaxPersonalArenaRatingRequirement(iece.RequiredArenaSlot) < iece.RequiredPersonalArenaRating) todo fixme
                {
                    // probably not the proper equip err
                    //SendEquipError(InventoryResult.CantEquipRank, null, null);
                    //return false;
                }
            }

            uint price = 0;
            if (crItem.IsGoldRequired(pProto) && pProto.BuyPrice > 0) //Assume price cannot be negative (do not know why it is int32)
            {
                uint maxCount = (uint)(UInt64.MaxValue / pProto.BuyPrice);
                if (count > maxCount)
                {
                    Log.outError("Player {0} tried to buy {1} item id {2}, causing overflow", GetName(), count, pProto.ItemId);
                    count = (byte)maxCount;
                }
                price = pProto.BuyPrice * count; //it should not exceed MAX_MONEY_AMOUNT

                // reputation discount
                price = (uint)(Math.Floor(price * GetReputationPriceDiscount(creature)));

                //int priceMod = GetTotalAuraModifier(SPELL_AURA_MOD_VENDOR_ITEMS_PRICES);
                //if (priceMod != 0)
                //price -= CalculatePctN(price, priceMod);
                ulong blah = GetMoney();

                if (!HasEnoughMoney(price))
                {
                    SendBuyError(BuyResult.NotEnoughtMoney, creature, item);
                    return false;
                }
            }

            if ((bag == ItemConst.NullBag && slot == ItemConst.NullSlot) || IsInventoryPos(bag, slot))
            {
                if (!_StoreOrEquipNewItem(vendorslot, item, count, bag, slot, (int)price, pProto, creature, crItem, true))
                    return false;
            }
            else if (IsEquipmentPos(bag, slot))
            {
                if (count != 1)
                {
                    SendEquipError(InventoryResult.NotEquippable, null, null);
                    return false;
                }
                if (!_StoreOrEquipNewItem(vendorslot, item, count, bag, slot, (int)price, pProto, creature, crItem, false))
                    return false;
            }
            else
            {
                SendEquipError(InventoryResult.WrongSlot, null, null);
                return false;
            }

            if (crItem.maxcount != 0) // bought
            {
                //if (pProto.Quality > ItemQuality.Epic || (pProto.Quality == ItemQuality.Epic && pProto.ItemLevel >= MinNewsItemLevel[sWorld.getIntConfig(CONFIG_EXPANSION)]))
                //if (Guild* guild = sGuildMgr.GetGuildById(GetGuildId()))
                //guild.GetNewsLog().AddNewEvent(GUILD_NEWS_ITEM_PURCHASED, time(NULL), GetGUID(), 0, item);
                return true;
            }

            return false;
        }
        public void AutoUnequipOffhandIfNeed(bool force = false)
        {
            Item offItem = GetItemByPos(InventorySlots.Bag0, (byte)EquipmentSlot.OffHand);
            if (offItem == null)
                return;

            ItemTemplate offtemplate = offItem.GetTemplate();

            // unequip offhand weapon if player doesn't have dual wield anymore
            //if (/*!CanDualWield() &&*/(offtemplate.inventoryType == InventoryType.WeaponOffhand || offtemplate.inventoryType == InventoryType.Weapon))
                //force = true;

            // need unequip offhand for 2h-weapon without TitanGrip (in any from hands)
            //if (!force && (/*CanTitanGrip() || */(offtemplate.inventoryType != InventoryType.Weapon2Hand/* && !IsTwoHandUsed()*/)))
                //return;

            List<ItemPosCount> off_dest = new List<ItemPosCount>();
            InventoryResult off_msg = CanStoreItem(ItemConst.NullBag, ItemConst.NullSlot, ref off_dest, offItem, false);
            if (off_msg == InventoryResult.Ok)
            {
                RemoveItem(InventorySlots.Bag0, (byte)EquipmentSlot.OffHand, true);
                StoreItem(off_dest, offItem, true);
            }
            else
            {
                //MoveItemFromInventory(INVENTORY_SLOT_BAG_0, EQUIPMENT_SLOT_OFFHAND, true);
                offItem.DeleteFromInventoryDB();                   // deletes item from character's inventory
                offItem.SaveToDB();                                // recursive and not have transaction guard into self, item not in inventory and can be save standalone

                //string subject = GetSession()->GetCypherString(LANG_NOT_EQUIPPED_ITEM);
                //MailDraft(subject, "There were problems with equipping one or several items").AddItem(offItem).SendMailTo(trans, this, MailSender(this, MAIL_STATIONERY_GM), MAIL_CHECK_MASK_COPIED);
            }
        }

        //Remove
        public void RemoveItem(byte bag, byte slot, bool update)
        {
            // note: removeitem does not actually change the item
            // it only takes the item out of storage temporarily
            // note2: if removeitem is to be used for delinking
            // the item must be removed from the player's updatequeue

            Item pItem = GetItemByPos(bag, slot);
            if (pItem != null)
            {
                //sLog.outDebug(LOG_FILTER_PLAYER_ITEMS, "STORAGE: RemoveItem bag = %u, slot = %u, item = %u", bag, slot, pItem.GetEntry());

                //RemoveEnchantmentDurations(pItem);
                //RemoveItemDurations(pItem);
                //RemoveTradeableItem(pItem);

                if (bag == InventorySlots.Bag0)
                {
                    if (slot < InventorySlots.BagEnd)
                    {
                        ItemTemplate pProto = pItem.GetTemplate();
                        // item set bonuses applied only at equip and removed at unequip, and still active for broken items

                        //if (pProto != null && pProto.ItemSet)
                        //RemoveItemsSetItem(this, pProto);

                        //_ApplyItemMods(pItem, slot, false);

                        // remove item dependent auras and casts (only weapon and armor slots)
                        if (slot < (byte)EquipmentSlot.End)
                        {
                            //RemoveItemDependentAurasAndCasts(pItem);

                            // remove held enchantments, update expertise
                            if (slot == (byte)EquipmentSlot.MainHand)
                            {
                                //if (pItem.GetItemSuffixFactor())
                                {
                                    //pItem.ClearEnchantment(PROP_ENCHANTMENT_SLOT_3);
                                    //pItem.ClearEnchantment(PROP_ENCHANTMENT_SLOT_4);
                                }
                                //else
                                {
                                    //pItem.ClearEnchantment(PROP_ENCHANTMENT_SLOT_0);
                                    //pItem.ClearEnchantment(PROP_ENCHANTMENT_SLOT_1);
                                }

                                //UpdateExpertise(BASE_ATTACK);
                            }
                            //else if (slot == EquipmentSlot.OffHand)
                            //UpdateExpertise(OFF_ATTACK);
                            // update armor penetration - passive auras may need it
                            switch ((EquipmentSlot)slot)
                            {
                                case EquipmentSlot.MainHand:
                                case EquipmentSlot.OffHand:
                                case EquipmentSlot.Ranged:
                                //RecalculateRating(CR_ARMOR_PENETRATION);
                                default:
                                    break;
                            }
                        }
                    }

                    Items[slot] = null;
                    SetValue<ulong>(PlayerFields.InvSlots + (slot * 2), 0);

                    if (slot < (byte)EquipmentSlot.End)
                        SetVisibleItemSlot(slot, null);
                }
                Bag pBag = GetBagByPos(bag);
                if (pBag != null)
                    pBag.RemoveItem(slot, update);

                pItem.SetValue<ulong>(ItemFields.ContainedIn, 0);
                // pItem.SetUInt64Value(ITEM_FIELD_OWNER, 0); not clear owner at remove (it will be set at store). This used in mail and auction code
                pItem.SetSlot(ItemConst.NullSlot);
                if (IsInWorld && update)
                    pItem.SendUpdateToPlayer(this);
            }
        }

        public void SwapItem(ushort src, ushort dst)
        {
            byte srcbag = (byte)(src >> 8);
            byte srcslot = (byte)(src & 255);

            byte dstbag = (byte)(dst >> 8);
            byte dstslot = (byte)(dst & 255);

            Item pSrcItem = GetItemByPos(srcbag, srcslot);
            Item pDstItem = GetItemByPos(dstbag, dstslot);

            if (pSrcItem == null)
                return;

            Log.outDebug("STORAGE: SwapItem bag = {0}, slot = {1}, item = {2}", dstbag, dstslot, pSrcItem.GetEntry());

            if (!isAlive())
            {
                SendEquipError(InventoryResult.PlayerDead, pSrcItem, pDstItem);
                return;
            }

            // SRC checks

            if (pSrcItem.LootGenerated)                           // prevent swap looting item
            {
                //best error message found for attempting to swap while looting
                SendEquipError(InventoryResult.ClientLockedOut, pSrcItem);
                return;
            }

            // check unequip potability for equipped items and bank bags
            if (IsEquipmentPos(src) || IsBagPos(src))
            {
                // bags can be swapped with empty bag slots, or with empty bag (items move possibility checked later)
                InventoryResult msg = CanUnequipItem(src, !IsBagPos(src) || IsBagPos(dst) || (pDstItem != null && pDstItem.ToBag() != null && pDstItem.ToBag().IsEmpty()));
                if (msg != InventoryResult.Ok)
                {
                    SendEquipError(msg, pSrcItem, pDstItem);
                    return;
                }
            }

            // prevent put equipped/bank bag in self
            if (IsBagPos(src) && srcslot == dstbag)
            {
                SendEquipError(InventoryResult.BagInBag, pSrcItem, pDstItem);
                return;
            }

            // prevent equipping bag in the same slot from its inside
            if (IsBagPos(dst) && srcbag == dstslot)
            {
                SendEquipError(InventoryResult.CantSwap, pSrcItem, pDstItem);
                return;
            }

            // DST checks

            if (pDstItem != null)
            {
                if (pDstItem.LootGenerated)                       // prevent swap looting item
                {
                    //best error message found for attempting to swap while looting
                    SendEquipError(InventoryResult.ClientLockedOut, pDstItem);
                    return;
                }

                // check unequip potability for equipped items and bank bags
                if (IsEquipmentPos(dst) || IsBagPos(dst))
                {
                    // bags can be swapped with empty bag slots, or with empty bag (items move possibility checked later)
                    InventoryResult msg = CanUnequipItem(dst, !IsBagPos(dst) || IsBagPos(src) || (pSrcItem.ToBag() != null && pSrcItem.ToBag().IsEmpty()));
                    if (msg != InventoryResult.Ok)
                    {
                        SendEquipError(msg, pSrcItem, pDstItem);
                        return;
                    }
                }
            }

            // NOW this is or item move (swap with empty), or swap with another item (including bags in bag possitions)
            // or swap empty bag with another empty or not empty bag (with items exchange)
            // Move case
            if (pDstItem == null)
            {
                if (IsInventoryPos(dst))
                {
                    List<ItemPosCount> dest = new List<ItemPosCount>();
                    InventoryResult msg = CanStoreItem(dstbag, dstslot, ref dest, pSrcItem, false);
                    if (msg != InventoryResult.Ok)
                    {
                        SendEquipError(msg, pSrcItem, null);
                        return;
                    }

                    RemoveItem(srcbag, srcslot, true);
                    StoreItem(dest, pSrcItem, true);
                    //if (IsBankPos(src))
                    //ItemAddedQuestCheck(pSrcItem.GetEntry(), pSrcItem.GetCount());
                }
                else if (IsBankPos(dst))
                {
                    List<ItemPosCount> dest = new List<ItemPosCount>();
                    InventoryResult msg = CanBankItem(dstbag, dstslot, ref dest, pSrcItem, false);
                    if (msg != InventoryResult.Ok)
                    {
                        SendEquipError(msg, pSrcItem, null);
                        return;
                    }

                    RemoveItem(srcbag, srcslot, true);
                    //BankItem(dest, pSrcItem, true);
                    //ItemRemovedQuestCheck(pSrcItem.GetEntry(), pSrcItem.GetCount());
                }
                else if (IsEquipmentPos(dst))
                {
                    ushort _dest;
                    InventoryResult msg = CanEquipItem(dstslot, out _dest, pSrcItem, false);
                    if (msg != InventoryResult.Ok)
                    {
                        SendEquipError(msg, pSrcItem, null);
                        return;
                    }

                    RemoveItem(srcbag, srcslot, true);
                    EquipItem(_dest, pSrcItem, true);
                    //AutoUnequipOffhandIfNeed();
                }

                return;
            }

            // attempt merge to / fill target item
            if (!pSrcItem.IsBag() && !pDstItem.IsBag())
            {
                InventoryResult msg;
                List<ItemPosCount> sDest = new List<ItemPosCount>();
                ushort eDest = 0;
                if (IsInventoryPos(dst))
                    msg = CanStoreItem(dstbag, dstslot, ref sDest, pSrcItem, false);
                else if (IsBankPos(dst))
                    msg = CanBankItem(dstbag, dstslot, ref sDest, pSrcItem, false);
                else if (IsEquipmentPos(dst))
                    msg = CanEquipItem(dstslot, out eDest, pSrcItem, false);
                else
                    return;

                // can be merge/fill
                if (msg == InventoryResult.Ok)
                {
                    if (pSrcItem.GetStackCount() + pDstItem.GetStackCount() <= pSrcItem.GetTemplate().GetMaxStackSize())
                    {
                        RemoveItem(srcbag, srcslot, true);

                        if (IsInventoryPos(dst))
                            StoreItem(sDest, pSrcItem, true);
                        //else if (IsBankPos(dst))
                        //BankItem(sDest, pSrcItem, true);
                        else if (IsEquipmentPos(dst))
                        {
                            EquipItem(eDest, pSrcItem, true);
                            //AutoUnequipOffhandIfNeed();
                        }
                    }
                    else
                    {
                        pSrcItem.SetStackCount(pSrcItem.GetStackCount() + pDstItem.GetStackCount() - pSrcItem.GetTemplate().GetMaxStackSize());
                        pDstItem.SetStackCount(pSrcItem.GetTemplate().GetMaxStackSize());
                        pSrcItem.SetState(ItemUpdateState.Changed, this);
                        pDstItem.SetState(ItemUpdateState.Changed, this);
                        if (IsInWorld)
                        {
                            pSrcItem.SendUpdateToPlayer(this);
                            pDstItem.SendUpdateToPlayer(this);
                        }
                    }
                    //SendRefundInfo(pDstItem);
                    return;
                }
            }

            // impossible merge/fill, do real swap
            InventoryResult _msg = InventoryResult.Ok;

            // check src.dest move possibility
            List<ItemPosCount> _sDest = new List<ItemPosCount>();
            ushort _eDest = 0;
            if (IsInventoryPos(dst))
                _msg = CanStoreItem(dstbag, dstslot, ref _sDest, pSrcItem, true);
            //else if (IsBankPos(dst))
            //msg = CanBankItem(dstbag, dstslot, sDest, pSrcItem, true);
            else if (IsEquipmentPos(dst))
            {
                _msg = CanEquipItem(dstslot, out _eDest, pSrcItem, true);
                if (_msg == InventoryResult.Ok)
                    _msg = CanUnequipItem(_eDest, true);
            }

            if (_msg != InventoryResult.Ok)
            {
                SendEquipError(_msg, pSrcItem, pDstItem);
                return;
            }

            // check dest.src move possibility
            List<ItemPosCount> sDest2 = new List<ItemPosCount>();
            ushort eDest2 = 0;
            if (IsInventoryPos(src))
                _msg = CanStoreItem(srcbag, srcslot, ref sDest2, pDstItem, true);
            //else if (IsBankPos(src))
            //msg = CanBankItem(srcbag, srcslot, sDest2, pDstItem, true);
            else if (IsEquipmentPos(src))
            {
                _msg = CanEquipItem(srcslot, out eDest2, pDstItem, true);
                if (_msg == InventoryResult.Ok)
                    _msg = CanUnequipItem(eDest2, true);
            }

            if (_msg != InventoryResult.Ok)
            {
                SendEquipError(_msg, pDstItem, pSrcItem);
                return;
            }

            // Check bag swap with item exchange (one from empty in not bag possition (equipped (not possible in fact) or store)
            Bag srcBag = pSrcItem.ToBag();
            if (srcBag != null)
            {
                Bag dstBag = pDstItem.ToBag();
                if (dstBag != null)
                {
                    Bag emptyBag = null;
                    Bag fullBag = null;
                    if (srcBag.IsEmpty() && !IsBagPos(src))
                    {
                        emptyBag = srcBag;
                        fullBag = dstBag;
                    }
                    else if (dstBag.IsEmpty() && !IsBagPos(dst))
                    {
                        emptyBag = dstBag;
                        fullBag = srcBag;
                    }

                    // bag swap (with items exchange) case
                    if (emptyBag != null && fullBag != null)
                    {
                        ItemTemplate emptyProto = emptyBag.GetTemplate();
                        byte count = 0;

                        for (byte i = 0; i < fullBag.GetBagSize(); ++i)
                        {
                            Item bagItem = fullBag.GetItemByPos(i);
                            if (bagItem == null)
                                continue;

                            ItemTemplate bagItemProto = bagItem.GetTemplate();
                            if (bagItemProto == null || !Item.ItemCanGoIntoBag(bagItemProto, emptyProto))
                            {
                                // one from items not go to empty target bag
                                SendEquipError(InventoryResult.BagInBag, pSrcItem, pDstItem);
                                return;
                            }

                            ++count;
                        }

                        if (count > emptyBag.GetBagSize())
                        {
                            // too small targeted bag
                            SendEquipError(InventoryResult.CantSwap, pSrcItem, pDstItem);
                            return;
                        }

                        // Items swap
                        count = 0;                                      // will pos in new bag
                        for (byte i = 0; i < fullBag.GetBagSize(); ++i)
                        {
                            Item bagItem = fullBag.GetItemByPos(i);
                            if (bagItem == null)
                                continue;

                            fullBag.RemoveItem(i, true);
                            emptyBag.StoreItem(count, bagItem, true);
                            bagItem.SetState(ItemUpdateState.Changed, this);

                            ++count;
                        }
                    }
                }
            }            

            // now do moves, remove...
            RemoveItem(dstbag, dstslot, false);
            RemoveItem(srcbag, srcslot, false);

            // add to dest
            if (IsInventoryPos(dst))
                StoreItem(_sDest, pSrcItem, true);
            //else if (IsBankPos(dst))
            //BankItem(sDest, pSrcItem, true);
            else if (IsEquipmentPos(dst))
                EquipItem(_eDest, pSrcItem, true);

            // add to src
            if (IsInventoryPos(src))
                StoreItem(sDest2, pDstItem, true);
            //else if (IsBankPos(src))
            //BankItem(sDest2, pDstItem, true);
            else if (IsEquipmentPos(src))
                EquipItem(eDest2, pDstItem, true);

            /*
    // if player is moving bags and is looting an item inside this bag
    // release the loot
    if (GetLootGUID())
    {
        bool released = false;
        if (IsBagPos(src))
        {
            Bag* bag = pSrcItem.ToBag();
            for (uint32 i = 0; i < bag.GetBagSize(); ++i)
            {
                if (Item* bagItem = bag.GetItemByPos(i))
                {
                    if (bagItem.m_lootGenerated)
                    {
                        m_session.DoLootRelease(GetLootGUID());
                        released = true;                    // so we don't need to look at dstBag
                        break;
                    }
                }
            }
        }

        if (!released && IsBagPos(dst) && pDstItem)
        {
            Bag* bag = pDstItem.ToBag();
            for (uint32 i = 0; i < bag.GetBagSize(); ++i)
            {
                if (Item* bagItem = bag.GetItemByPos(i))
                {
                    if (bagItem.m_lootGenerated)
                    {
                        m_session.DoLootRelease(GetLootGUID());
                        released = true;                    // not realy needed here
                        break;
                    }
                }
            }
        }
    }
            */

            //AutoUnequipOffhandIfNeed();
        }

        #endregion

        #region Combat
        bool CanUseAttackType(WeaponAttackType attacktype)
        {
            switch (attacktype)
            {
                case WeaponAttackType.BaseAttack:
                    return !HasFlag((int)UnitFields.Flags, UnitFlags.Disarmed);
                case WeaponAttackType.OffAttack:
                    return !HasFlag((int)UnitFields.Flags2, UnitFlags2.DisarmOffhand);
                case WeaponAttackType.RangedAttack:
                    return !HasFlag((int)UnitFields.Flags2, UnitFlags2.DisarmRanged);
            }
            return true;
        }
        #endregion

        #region DB
        //Loading
        public bool LoadCharacter(ulong guidLow)
        {
            ////                                                     0     1        2     3     4        5      6    7      8     9           10              11
            //QueryResult* result = CharacterDatabase.PQuery("SELECT guid, account, name, race, class, gender, level, xp, money, playerBytes, playerBytes2, playerFlags, "
            // 12          13          14          15   16           17        18        19         20         21          22           23                 24
            //"position_x, position_y, position_z, map, orientation, taximask, cinematic, totaltime, leveltime, rest_bonus, logout_time, is_logout_resting, resettalents_cost, "
            // 25                 26          27       28       29       30       31         32           33            34        35    36      37                 38         39
            //"resettalents_time, talentTree, trans_x, trans_y, trans_z, trans_o, transguid, extra_flags, stable_slots, at_login, zone, online, death_expire_time, taxi_path, instance_mode_mask, "
            //    40           41          42              43           44            45
            //"totalKills, todayKills, yesterdayKills, chosenTitle, watchedFaction, drunk, "
            // 46      47      48      49      50      51      52           53         54          55             56
            //"health, power1, power2, power3, power4, power5, instance_id, speccount, activespec, exploredZones, equipmentCache, "
            // 57           58          59
            //"knownTitles, actionBars, grantableLevels FROM characters WHERE guid = '%u'", guid);
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER);
            stmt.AddValue(0, guidLow);
            SQLResult result = DB.Characters.Select(stmt);

            if (result.Count == 0)
            {
                Log.outError("Player (GUID: {0}) not found in table `characters`, can't load. ", guidLow);
                return false;
            }

            uint dbAccountId = result.Read<uint>(0, 1);

            // check if the character's account in the db and the logged in account match.
            // player should be able to load/delete character only with correct account!
            if (dbAccountId != GetSession().GetAccountId())
            {
                Log.outError("Player (GUID: {0}) loading from wrong account (is: {1}, should be: {2})", GetGUIDLow(), GetSession().GetAccount().Id, dbAccountId);
                return false;
            }

            CreateGuid(guidLow, 0, HighGuidType.Player);

            uint gender = result.Read<byte>(0, 5);
            if (gender >= (uint)Gender.None)
            {
                //error
                return false;
            }
            Name = result.Read<string>(0, 2);

            uint bytes0 = 0;
            bytes0 |= result.Read<byte>(0, 3);                         // race
            bytes0 |= result.Read<uint>(0, 4) << 8;                    // class
            bytes0 |= gender << 16;                                 // gender
            SetValue<uint>(UnitFields.Bytes, bytes0);

            SetValue<uint>(UnitFields.Level, result.Read<uint>(0, 6));
            SetValue<uint>(PlayerFields.XP, result.Read<uint>(0, 7));

            LoadIntoDataField(result.Read<string>(0, 55), (int)PlayerFields.ExploredZones, PlayerConst.ExploredZonesSize);
            LoadIntoDataField(result.Read<string>(0, 57), (int)PlayerFields.KnownTitles, (int)PlayerTitle.KnowTitlesSize * 2);

            SetValue<float>(UnitFields.BoundingRadius, SharedConst.DefaultWorldObjectSize);
            SetValue<float>(UnitFields.CombatReach, 1.5f);
            SetValue<float>(UnitFields.HoverHeight, 1.0f);

            // load achievements before anything else to prevent multiple gains for the same achievement/criteria on every loading (as loading does call UpdateAchievementCriteria)
            //m_achievementMgr.LoadFromDB(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADACHIEVEMENTS), holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADCRITERIAPROGRESS));

            ulong money = result.Read<ulong>(0, 8);
            if (money > PlayerConst.MaxMoneyAmount)
                money = PlayerConst.MaxMoneyAmount;
            SetMoney(money);

            SetValue<uint>(PlayerFields.Bytes, result.Read<uint>(0, 9));
            SetValue<uint>(PlayerFields.Bytes2, result.Read<uint>(0, 10));
            SetValue<byte>(PlayerFields.Bytes3, (byte)gender, 0);
            SetValue<byte>(PlayerFields.Bytes3, result.Read<byte>(0, 45), 1);
            SetValue<uint>(PlayerFields.PlayerFlags, result.Read<uint>(0, 11));
            SetValue<int>(PlayerFields.WatchedFactionIndex, -1);//result.Read<int>(0, 44));

            SetValue<byte>(PlayerFields.Bytes, result.Read<byte>(0, 58), 2);

            InitDisplayIds();

            //Need to call it to initialize m_team (m_team can be calculated from race)
            //Other way is to saves m_team into characters table.
            SetFactionForRace(getRace());

            // load home bind and check in same time class/race pair, it used later for restore broken positions
            if (!LoadHomeBind())
                return false;

            InitPrimaryProfessions();                               // to max set before any spell loaded

            // init saved position, and fix it later if problematic
            uint transGUID = result.Read<uint>(0, 31);

            Position = new ObjectPosition(result.Read<float>(0, 12), result.Read<float>(0, 13), result.Read<float>(0, 14), result.Read<float>(0, 16));

            uint mapId = result.Read<uint>(0, 15);
            uint instanceId = result.Read<uint>(0, 52);

            uint dungeonDiff = result.Read<uint>(0, 39) & 0x0F;
            if (dungeonDiff >= SharedConst.MaxDungeonDifficulty)
                dungeonDiff = (uint)Difficulty.DungeonNormal;
            uint raidDiff = (result.Read<uint>(0, 39) >> 4) & 0x0F;
            if (raidDiff >= SharedConst.MaxRaidDifficulty)
                raidDiff = (uint)Difficulty.Raid10manNormal;
            //SetDungeonDifficulty(Difficulty(dungeonDiff));          // may be changed in _LoadGroup
            //SetRaidDifficulty(Difficulty(raidDiff));                // may be changed in _LoadGroup

            string taxi_nodes = result.Read<string>(0, 38);

            //_LoadGroup(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADGROUP));

            //_LoadArenaTeamInfo(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADARENAINFO));

            // check arena teams integrity
            //for (var arena_slot = 0; arena_slot < MAX_ARENA_SLOT; ++arena_slot)
            {
                //uint32 arena_team_id = GetArenaTeamId(arena_slot);
                //if (!arena_team_id)
                //continue;

                //if (ArenaTeam* at = sArenaTeamMgr.GetArenaTeamById(arena_team_id))
                //if (at.IsMember(GetGUID()))
                //continue;

                // arena team not exist or not member, cleanup fields
                //for (int j = 0; j < 6; ++j)
                //SetArenaTeamInfoField(arena_slot, ArenaTeamInfoType(j), 0);
            }

            LoadCurrency();
            SetValue<uint>(PlayerFields.LifetimeHonorableKills, result.Read<uint>(0, 40));
            SetValue<ushort>(PlayerFields.YesterdayHonorableKills, result.Read<ushort>(0, 41), 0);
            SetValue<ushort>(PlayerFields.YesterdayHonorableKills, result.Read<ushort>(0, 42), 1);

            //_LoadBoundInstances(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADBOUNDINSTANCES));
            //_LoadInstanceTimeRestrictions(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADINSTANCELOCKTIMES));
            //_LoadBGData(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADBGDATA));

            MapEntry mapEntry = DBCStorage.MapStorage.LookupByKey(mapId);
            if (mapEntry == null || !IsPositionValid()) 
            {
                Log.outError("Player (guidlow {0}) have invalid coordinates (MapId: {1} X: {2} Y: {3} Z: {4} O: {5}). Teleport to default race/class locations.", guidLow, mapId, GetPositionX(), GetPositionY(), 
                GetPositionZ(), GetOrientation());
                //RelocateToHomebind();
            }
            else if (mapEntry != null && mapEntry.IsBattleGroundOrArena())
            {
                /*
                Battleground currentBg = NULL;
                if (m_bgData.bgInstanceID)                                                //saved in Battleground
                    currentBg = sBattlegroundMgr.GetBattleground(m_bgData.bgInstanceID, BATTLEGROUND_TYPE_NONE);

                bool player_at_bg = currentBg && currentBg.IsPlayerInBattleground(GetGUID());

                if (player_at_bg && currentBg.GetStatus() != STATUS_WAIT_LEAVE)
                {
                    BattlegroundQueueTypeId bgQueueTypeId = sBattlegroundMgr.BGQueueTypeId(currentBg.GetTypeID(), currentBg.GetArenaType());
                    AddBattlegroundQueueId(bgQueueTypeId);

                    m_bgData.bgTypeID = currentBg.GetTypeID();

                    //join player to battleground group
                    currentBg.EventPlayerLoggedIn(this);
                    currentBg.AddOrSetPlayerToCorrectBgGroup(this, m_bgData.bgTeam);

                    SetInviteForBattlegroundQueueType(bgQueueTypeId, currentBg.GetInstanceID());
                }
                    // Bg was not found - go to Entry Point
                else
                {
                    // leave bg
                    if (player_at_bg)
                        currentBg.RemovePlayerAtLeave(GetGUID(), false, true);

                    // Do not look for instance if bg not found
                    const WorldLocation& _loc = GetBattlegroundEntryPoint();
                    mapId = _loc.GetMapId(); instanceId = 0;

                    if (mapId == MAPID_INVALID) // Battleground Entry Point not found (???)
                    {
                        sLog.outError(LOG_FILTER_PLAYER, "Player (guidlow %d) was in BG in database, but BG was not found, and entry point was invalid! Teleport to default race/class locations.", guid);
                        RelocateToHomebind();
                    }
                    else
                        Relocate(&_loc);

                    // We are not in BG anymore
                    m_bgData.bgInstanceID = 0;
                }
                */
            }
                // currently we do not support transport in bg
            else if (transGUID != 0)
            {
                movementInfo.TransGuid = new ObjectGuid(MakeNewGuid(transGUID, 0, HighGuidType.MOTransport));
                movementInfo.TransPos = new ObjectPosition(result.Read<float>(0, 27), result.Read<float>(0, 28), result.Read<float>(0, 29), result.Read<float>(0, 30));

                if (!GridDefines.IsValidMapCoord(GetPositionX()+movementInfo.TransPos.X, GetPositionY()+movementInfo.TransPos.Y,
                    GetPositionZ()+movementInfo.TransPos.Z, GetOrientation()+movementInfo.TransPos.Orientation) ||
                    // transport size limited
                    movementInfo.TransPos.X > 250 || movementInfo.TransPos.Y > 250 || movementInfo.TransPos.Z > 250)
                {
                    Log.outError("Player (guidlow {0}) have invalid transport coordinates (X: {1} Y: {2} Z: {3} O: {4}). Teleport to bind location.",
                        guidLow, GetPositionX()+movementInfo.TransPos.X, GetPositionY()+movementInfo.TransPos.Y,
                        GetPositionZ()+movementInfo.TransPos.Z, GetOrientation()+movementInfo.TransPos.Orientation);

                    //RelocateToHomebind();
                }
                else
                {
                    /*
                    for (MapManager::TransportSet::iterator iter = sMapMgr.m_Transports.begin(); iter != sMapMgr.m_Transports.end(); ++iter)
                    {
                        if ((*iter).GetGUIDLow() == transGUID)
                        {
                            m_transport = *iter;
                            m_transport.AddPassenger(this);
                            mapId = (m_transport.GetMapId());
                            break;
                        }
                    }
                    if (!m_transport)
                    {
                        sLog.outError(LOG_FILTER_PLAYER, "Player (guidlow %d) have problems with transport guid (%u). Teleport to bind location.",
                            guid, transGUID);

                        RelocateToHomebind();
                    }
                    */
                }
            }
            // currently we do not support taxi in instance
            /*
            else if (!taxi_nodes.empty())
    {
        instanceId = 0;

        // Not finish taxi flight path
        if (m_bgData.HasTaxiPath())
        {
            for (int i = 0; i < 2; ++i)
                m_taxi.AddTaxiDestination(m_bgData.taxiPath[i]);
        }
        else if (!m_taxi.LoadTaxiDestinationsFromString(taxi_nodes, GetTeam()))
        {
            // problems with taxi path loading
            TaxiNodesEntry const* nodeEntry = NULL;
            if (uint32 node_id = m_taxi.GetTaxiSource())
                nodeEntry = sTaxiNodesStore.LookupEntry(node_id);

            if (!nodeEntry)                                      // don't know taxi start node, to homebind
            {
                sLog.outError(LOG_FILTER_PLAYER, "Character %u have wrong data in taxi destination list, teleport to homebind.", GetGUIDLow());
                RelocateToHomebind();
            }
            else                                                // have start node, to it
            {
                sLog.outError(LOG_FILTER_PLAYER, "Character %u have too short taxi destination list, teleport to original node.", GetGUIDLow());
                mapId = nodeEntry.map_id;
                Relocate(nodeEntry.x, nodeEntry.y, nodeEntry.z, 0.0f);
            }
            m_taxi.ClearTaxiDestinations();
        }

        if (uint32 node_id = m_taxi.GetTaxiSource())
        {
            // save source node as recall coord to prevent recall and fall from sky
            TaxiNodesEntry const* nodeEntry = sTaxiNodesStore.LookupEntry(node_id);
            if (nodeEntry && nodeEntry.map_id == GetMapId())
            {
                ASSERT(nodeEntry);                                  // checked in m_taxi.LoadTaxiDestinationsFromString
                mapId = nodeEntry.map_id;
                Relocate(nodeEntry.x, nodeEntry.y, nodeEntry.z, 0.0f);
            }

            // flight will started later
        }
    }
            */
            // NOW player must have valid map
            // load the player's map here if it's not already loaded
            Map map = Cypher.MapMgr.CreateMap(mapId, this);
            if (map == null)
            {
                /*
                instanceId = 0;
                AreaTrigger at = sObjectMgr.GetGoBackTrigger(mapId);
                if (at)
                {
                    sLog.outError(LOG_FILTER_PLAYER, "Player (guidlow %d) is teleported to gobacktrigger (Map: %u X: %f Y: %f Z: %f O: %f).", guid, mapId, GetPositionX(), GetPositionY(), GetPositionZ(), GetOrientation());
                    Relocate(at.target_X, at.target_Y, at.target_Z, GetOrientation());
                    mapId = at.target_mapId;
                }
                else
                {
                    sLog.outError(LOG_FILTER_PLAYER, "Player (guidlow %d) is teleported to home (Map: %u X: %f Y: %f Z: %f O: %f).", guid, mapId, GetPositionX(), GetPositionY(), GetPositionZ(), GetOrientation());
                    RelocateToHomebind();
                }
                */
                map = Cypher.MapMgr.CreateMap(mapId, this);
                if (map == null)
                {
                    PlayerInfo info = Cypher.ObjMgr.GetPlayerInfo(getRace(), getClass());
                    mapId = info.MapId;
                    Position.Relocate(info.PositionX, info.PositionY, info.PositionZ, 0.0f);
                    Log.outError("Player (guidlow {0}) have invalid coordinates (X: {1} Y: {2} Z: {3} O: {4}). Teleport to default race/class locations.", guidLow, GetPositionX(), GetPositionY(),
                        GetPositionZ(), GetOrientation());
                    map = Cypher.MapMgr.CreateMap(mapId, this);
                    if (map == null)
                    {
                        Log.outError("Player (guidlow {0}) has invalid default map coordinates (X: {1} Y: {2} Z: {3} O: {4}). or instance couldn't be created", guidLow, GetPositionX(), GetPositionY(),
                            GetPositionZ(), GetOrientation());
                        return false;
                    }
                }
            }
            // if the player is in an instance and it has been reset in the meantime teleport him to the entrance
            /*
            if (instanceId && !sInstanceSaveMgr.GetInstanceSave(instanceId) && !map.IsBattlegroundOrArena())
            {
                AreaTrigger at = sObjectMgr.GetMapEntranceTrigger(mapId);
                if (at)
                    Relocate(at.target_X, at.target_Y, at.target_Z, at.target_Orientation);
                else
                {
                    sLog.outError(LOG_FILTER_PLAYER, "Player %s(GUID: %u) logged in to a reset instance (map: %u) and there is no area-trigger leading to this map. Thus he can't be ported back to the entrance. 
             This _might_ be an exploit attempt.", GetName(), GetGUIDLow(), mapId);
                    RelocateToHomebind();
                }
            }
            */
            SetMap(map);
            //StoreRaidMapDifficulty();

            // randomize first save time in range [CONFIG_INTERVAL_SAVE] around [CONFIG_INTERVAL_SAVE]
            // this must help in case next save after mass player load after server startup
            //m_nextSave = urand(m_nextSave / 2, m_nextSave * 3 / 2);

            //SaveRecallPosition();

            //time_t now = time(NULL);
            //time_t logoutTime = time_t(fields[22].GetUInt32());

            // since last logout (in seconds)
            //uint32 time_diff = uint32(now - logoutTime); //uint64 is excessive for a time_diff in seconds.. uint32 allows for 136~ year difference.

            // set value, including drunk invisibility detection
            // calculate sobering. after 15 minutes logged out, the player will be sober again
            //uint8 newDrunkValue = 0;
            //if (time_diff < uint32(GetDrunkValue()) * 9)
            //newDrunkValue = GetDrunkValue() - time_diff / 9;

            //SetDrunkValue(newDrunkValue);

            //m_cinematic = fields[18].GetUInt8();
            //m_Played_time[PLAYED_TIME_TOTAL] = fields[19].GetUInt32();
            //m_Played_time[PLAYED_TIME_LEVEL] = fields[20].GetUInt32();

            //SetTalentResetCost(fields[24].GetUInt32());
            //SetTalentResetTime(time_t(fields[25].GetUInt32()));

            //m_taxi.LoadTaxiMask(fields[17].GetCString());            // must be before InitTaxiNodesForLevel

            uint extraflags = result.Read<uint>(0, 32);

            //m_stableSlots = fields[33].GetUInt8();
            //if (m_stableSlots > MAX_PET_STABLES)
            {
                // sLog.outError(LOG_FILTER_PLAYER, "Player can have not more %u stable slots, but have in DB %u", MAX_PET_STABLES, uint32(m_stableSlots));
                // m_stableSlots = MAX_PET_STABLES;
            }

            atLoginFlags = (AtLoginFlags)result.Read<uint>(0, 34);

            // Honor system
            // Update Honor kills data
            //m_lastHonorUpdateTime = logoutTime;
            //UpdateHonorFields();

            // m_deathExpireTime = time_t(fields[37].GetUInt32());
            //if (m_deathExpireTime > now+MAX_DEATH_COUNT*DEATH_EXPIRE_STEP)
            // m_deathExpireTime = now+MAX_DEATH_COUNT*DEATH_EXPIRE_STEP-1;

            // clear channel spell data (if saved at channel spell casting)
            SetValue<ulong>(UnitFields.ChannelObject, 0);
            SetValue<uint>(UnitFields.ChannelSpell, 0);

            // clear charm/summon related fields
            SetOwnerGUID(0);
            SetValue<ulong>(UnitFields.CharmedBy, 0);
            SetValue<ulong>(UnitFields.Charm, 0);
            SetValue<ulong>(UnitFields.Summon, 0);
            SetValue<ulong>(PlayerFields.FarsightObject, 0);
            //SetCreatorGUID(0);

            //RemoveFlag(UnitFields.Flags2, UnitFlags2.ForceMove);

            // reset some aura modifiers before aura apply
            SetValue<uint>(PlayerFields.TrackCreatureMask, 0);
            SetValue<uint>(PlayerFields.TrackResourceMask, 0);

            // make sure the unit is considered out of combat for proper loading
            //ClearInCombat();

            // make sure the unit is considered not in duel for proper loading
            SetValue<ulong>(PlayerFields.DuelArbiter, 0);
            SetValue<uint>(PlayerFields.DuelTeam, 0);

            // reset stats before loading any modifiers
            InitStatsForLevel();
            //InitGlyphsForLevel();
            //InitTaxiNodesForLevel();
            //InitRunes();

            // rest bonus can only be calculated after InitStatsForLevel()
            //m_rest_bonus = fields[21].GetFloat();

            //if (time_diff > 0)
            {
                //speed collect rest bonus in offline, in logout, far from tavern, city (section/in hour)
                //float bubble0 = 0.031f;
                //speed collect rest bonus in offline, in logout, in tavern, city (section/in hour)
                //float bubble1 = 0.125f;
                //float bubble = fields[23].GetUInt8() > 0
                //? bubble1*sWorld.getRate(RATE_REST_OFFLINE_IN_TAVERN_OR_CITY)
                //: bubble0*sWorld.getRate(RATE_REST_OFFLINE_IN_WILDERNESS);

                //SetRestBonus(GetRestBonus()+ time_diff*((float)GetUInt32Value(PLAYER_NEXT_LEVEL_XP)/72000)*bubble);
            }
            SetCharacterFields();
            // load skills after InitStatsForLevel because it triggering aura apply also
            LoadSkills();
            //UpdateSkillsForLevel(); //update skills after load, to make sure they are correctly update at player load

            // apply original stats mods before spell loading or item equipment that call before equip _RemoveStatsMods()


            //SetSpecsCount(fields[53].GetUInt8());
            //SetActiveSpec(fields[54].GetUInt8());

            // sanity check
            //if (GetSpecsCount() > MAX_TALENT_SPECS || GetActiveSpec() > MAX_TALENT_SPEC || GetSpecsCount() < MIN_TALENT_SPECS)
            {
                //SetActiveSpec(0);
                //sLog.outError(LOG_FILTER_PLAYER, "Player %s(GUID: %u) has SpecCount = %u and ActiveSpec = %u.", GetName(), GetGUIDLow(), GetSpecsCount(), GetActiveSpec());
            }

            //_LoadTalents(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADTALENTS));
            LoadSpells();

            //_LoadGlyphs(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADGLYPHS));
            //_LoadAuras(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADAURAS), time_diff);
            //_LoadGlyphAuras();
            // add ghost flag (must be after aura load: PLAYER_FLAGS_GHOST set in aura)
            if (HasFlag(PlayerFields.PlayerFlags, PlayerFlags.Ghost))
                deathState = DeathState.Dead;

            // after spell load, learn rewarded spell if need also
            //_LoadQuestStatus(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADQUESTSTATUS));
            //_LoadQuestStatusRewarded(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADQUESTSTATUSREW));
            //_LoadDailyQuestStatus(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADDAILYQUESTSTATUS));
            //_LoadWeeklyQuestStatus(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADWEEKLYQUESTSTATUS));
            //_LoadSeasonalQuestStatus(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADSEASONALQUESTSTATUS));
            //_LoadRandomBGStatus(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADRANDOMBG));

            // after spell and quest load
            //InitTalentForLevel();
            learnDefaultSpells();

            // must be before inventory (some items required reputation check)
            reputationMgr.LoadFromDB();

            LoadInventory();

            //if (IsVoidStorageUnlocked())
            //_LoadVoidStorage(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADVOIDSTORAGE));

            // update items with duration and realtime
            //UpdateItemDuration(time_diff, true);

            //_LoadActions(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADACTIONS));

            // unread mails and next delivery time, actual mails not loaded
            //_LoadMailInit(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADMAILCOUNT), holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADMAILDATE));

            //m_social = sSocialMgr.LoadFromDB(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADSOCIALLIST), GetGUIDLow());

            // check PLAYER_CHOSEN_TITLE compatibility with PLAYER__FIELD_KNOWN_TITLES
            // note: PLAYER__FIELD_KNOWN_TITLES updated at quest status loaded
            int curTitle = result.Read<int>(0, 43);
            if (curTitle != 0 && !HasTitle(curTitle))
                curTitle = 0;

            SetValue<int>(PlayerFields.PlayerTitle, curTitle);

            // has to be called after last Relocate() in Player::LoadFromDB
            //SetFallInformation(0, GetPositionZ());

            //_LoadSpellCooldowns(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADSPELLCOOLDOWNS));

            // Spell code allow apply any auras to dead character in load time in aura/spell/item loading
            // Do now before stats re-calculation cleanup for ghost state unexpected auras
            //if (!isAlive())
            //RemoveAllAurasOnDeath();
            //else
            //RemoveAllAurasRequiringDeadTarget();

            //apply all stat bonuses from items and auras
            //SetCanModifyStats(true);
            //UpdateAllStats();

            // restore remembered power/health values (but not more max values)
            uint savedHealth = result.Read<uint>(0, 46);
            SetHealth(savedHealth > GetMaxHealth() ? GetMaxHealth() : savedHealth);
            int loadedPowers = 0;
            for (uint i = 0; i < (int)Powers.Max; ++i)
            {
                if (Cypher.ObjMgr.GetPowerIndexByClass(i, getClass()) != (int)Powers.Max)
                {
                    uint savedPower = result.Read<uint>(0, 47 + loadedPowers);
                    uint maxPower = GetValue<uint>(UnitFields.MaxPower + loadedPowers);
                    SetPower((Powers)i, (int)(savedPower > maxPower ? maxPower : savedPower));
                    loadedPowers++;
                    if (loadedPowers >= (int)Powers.MaxPerClass)
                        break;
                }
            }

            for (; loadedPowers < (int)Powers.MaxPerClass; ++loadedPowers)
                SetValue<uint>(UnitFields.Power + loadedPowers, 0);

            SetPower(Powers.Eclipse, 0);

            // must be after loading spells and talents
            //Tokens talentTrees(fields[26].GetString(), ' ', MAX_TALENT_SPECS);
            //for (uint8 i = 0; i < MAX_TALENT_SPECS; ++i)
            {
                //if (i >= talentTrees.size())
                //break;

                // uint32 talentTree = atol(talentTrees[i]);
                //if (sTalentTabStore.LookupEntry(talentTree))
                //SetPrimaryTalentTree(i, talentTree);
                //else if (i == GetActiveSpec())
                //SetAtLoginFlag(AT_LOGIN_RESET_TALENTS); // invalid tree, reset talents
            }

            //sLog.outDebug(LOG_FILTER_PLAYER_LOADING, "The value of player %s after load item and aura is: ", m_name.c_str());
            //outDebugValues();

            /*
    // GM state
    if (!AccountMgr::IsPlayerAccount(GetSession().GetSecurity()))
    {
        switch (sWorld.getIntConfig(CONFIG_GM_LOGIN_STATE))
        {
            default:
            case 0:                      break;             // disable
            case 1: SetGameMaster(true); break;             // enable
            case 2:                                         // save state
                if (extraflags & PLAYER_EXTRA_GM_ON)
                    SetGameMaster(true);
                break;
        }

        switch (sWorld.getIntConfig(CONFIG_GM_VISIBLE_STATE))
        {
            default:
            case 0: SetGMVisible(false); break;             // invisible
            case 1:                      break;             // visible
            case 2:                                         // save state
                if (extraflags & PLAYER_EXTRA_GM_INVISIBLE)
                    SetGMVisible(false);
                break;
        }

        switch (sWorld.getIntConfig(CONFIG_GM_CHAT))
        {
            default:
            case 0:                  break;                 // disable
            case 1: SetGMChat(true); break;                 // enable
            case 2:                                         // save state
                if (extraflags & PLAYER_EXTRA_GM_CHAT)
                    SetGMChat(true);
                break;
        }

        switch (sWorld.getIntConfig(CONFIG_GM_WHISPERING_TO))
        {
            default:
            case 0:                          break;         // disable
            case 1: SetAcceptWhispers(true); break;         // enable
            case 2:                                         // save state
                if (extraflags & PLAYER_EXTRA_ACCEPT_WHISPERS)
                    SetAcceptWhispers(true);
                break;
        }
    }
            */

            // RaF stuff.
            //m_grantableLevels = fields[59].GetUInt8();
            //if (GetSession().IsARecruiter() || (GetSession().GetRecruiterId() != 0))
            //SetFlag(UNIT_DYNAMIC_FLAGS, UNIT_DYNFLAG_REFER_A_FRIEND);

            //if (m_grantableLevels > 0)
            //SetByteValue(PLAYER_FIELD_BYTES, 1, 0x01);

            //_LoadDeclinedNames(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADDECLINEDNAMES));

            //achievementMgr.CheckAllAchievementCriteria(this);

            //_LoadEquipmentSets(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOADEQUIPMENTSETS));

            //_LoadCUFProfiles(holder.GetPreparedResult(PLAYER_LOGIN_QUERY_LOAD_CUF_PROFILES));


            //GuildGuid = result.Read<UInt64>(0, 18);


            return true;
        }
        public void SendInitialPacketsBeforeAddToMap()
        {
            /// Pass 'this' as argument because we're not stored in ObjectAccessor yet
            //GetSocial().SendSocialList(this);

            // Homebind
            PacketWriter data = new PacketWriter(Opcodes.SMSG_Bindpointupdate);
            //data << m_homebindX << m_homebindY << m_homebindZ;
            //data << (uint32) m_homebindMapId;
            //data << (uint32) m_homebindAreaId;
            //GetSession().SendPacket(&data);

            // SMSG_SET_PROFICIENCY
            // SMSG_SET_PCT_SPELL_MODIFIER
            // SMSG_SET_FLAT_SPELL_MODIFIER
            // SMSG_UPDATE_AURA_DURATION

            //SendTalentsInfoData(false);

            SendInitialSpells();

            //data.Initialize(SMSG_SEND_UNLEARN_SPELLS, 4);
            //data << uint32(0);                                      // count, for (count) uint32;
            //GetSession().SendPacket(&data);

            //SendInitialActionButtons();
            reputationMgr.SendInitialReputations();
            //m_achievementMgr.SendAllAchievementData(this);

            //SendEquipmentSetList();

            data = new PacketWriter(Opcodes.SMSG_LoginSettimespeed);

            DateTime time = DateTime.Now;
            uint packedtime = (uint)(((uint)time.Year - 100) << 24 | (uint)time.Month << 20 | (uint)(time.Day - 1) << 14 | (uint)(time.DayOfWeek) << 11 | (uint)time.Hour << 6 | (uint)time.Minute);
            data.WriteUInt32(packedtime);
            data.WriteFloat(0.01666667f);                             // game speed
            data.WriteUInt32(0);                                      // added in 3.1.2
            //session.Send(data);

            //GetReputationMgr().SendForceReactions();                // SMSG_SET_FORCED_REACTIONS

            // SMSG_TALENTS_INFO x 2 for pet (unspent points and talents in separate packets...)
            // SMSG_PET_GUIDS
            // SMSG_UPDATE_WORLD_STATE
            // SMSG_POWER_UPDATE

            //SendCurrencies();
            //SetMover(this);
        }
        public void SendInitialPacketsAfterAddToMap()
        {
            UpdateVisibilityForPlayer();

            // update zone
            uint newzone, newarea;
            GetZoneAndAreaId(out newzone, out newarea);
            UpdateZone(newzone, newarea);                            // also call SendInitWorldStates();

            //ResetTimeSync();
            //SendTimeSync();

            //Player::GetSession()->SendLoadCUFProfiles();

            //CastSpell(this, 836, true);                             // LOGINEFFECT
            /*
                // set some aura effects that send packet to player client after add player to map
                // SendMessageToSet not send it to player not it map, only for aura that not changed anything at re-apply
                // same auras state lost at far teleport, send it one more time in this case also
                static const AuraType auratypes[] =
                {
                    SPELL_AURA_MOD_FEAR,     SPELL_AURA_TRANSFORM,                 SPELL_AURA_WATER_WALK,
                    SPELL_AURA_FEATHER_FALL, SPELL_AURA_HOVER,                     SPELL_AURA_SAFE_FALL,
                    SPELL_AURA_FLY,          SPELL_AURA_MOD_INCREASE_MOUNTED_FLIGHT_SPEED, SPELL_AURA_NONE
                };
                for (AuraType const* itr = &auratypes[0]; itr && itr[0] != SPELL_AURA_NONE; ++itr)
                {
                    Unit::AuraEffectList const& auraList = GetAuraEffectsByType(*itr);
                    if (!auraList.empty())
                        auraList.front()->HandleEffect(this, AURA_EFFECT_HANDLE_SEND_FOR_CLIENT, true);
                }

                if (HasAuraType(SPELL_AURA_MOD_STUN))
                    SetRooted(true);

                // manual send package (have code in HandleEffect(this, AURA_EFFECT_HANDLE_SEND_FOR_CLIENT, true); that must not be re-applied.
                if (HasAuraType(SPELL_AURA_MOD_ROOT))
                    SendMoveRoot(2);

                SendAurasForTarget(this);
                SendEnchantmentDurations();                             // must be after add to map
                SendItemDurations();                                    // must be after add to map

                // raid downscaling - send difficulty to player
                if (GetMap()->IsRaid())
                {
                    if (GetMap()->GetDifficulty() != GetRaidDifficulty())
                    {
                        StoreRaidMapDifficulty();
                        SendRaidDifficulty(GetGroup() != NULL, GetStoredRaidDifficulty());
                    }
                }
                else if (GetRaidDifficulty() != GetStoredRaidDifficulty())
                    SendRaidDifficulty(GetGroup() != NULL);
                        */
        }
        public void SendInitialSpells()
        {
            PacketWriter temp = new PacketWriter();

            int count = 0;
            foreach (var spell in Spells)
            {
                if (spell.State == PlayerSpellState.Removed)
                    continue;

                if (!spell.Active || spell.Disabled)
                    continue;

                count++;
                temp.WriteUInt32(spell.SpellId);
            }

            PacketWriter data = new PacketWriter(Opcodes.SMSG_InitialSpells);
            data.WriteBits<uint>((uint)Spells.Count, 24);
            data.WriteBit(1);
            data.BitFlush();
            data.WriteBytes(temp);
            Log.outDebug("CHARACTER: Sent Initial Spells");
            GetSession().Send(data);
        }
        void InitStatsForLevel(bool reapplyMods = false)
        {
            //if (reapplyMods)                                        //reapply stats values only on .reset stats (level) command
            //_RemoveAllStatBonuses();

            uint basehp, basemana;
            Cypher.ObjMgr.GetPlayerClassLevelInfo(getClass(), getLevel(), out basehp, out basemana);

            PlayerLevelInfo info = Cypher.ObjMgr.GetPlayerLevelInfo(getRace(), getClass(), getLevel());

            SetValue<uint>(PlayerFields.MaxLevel, WorldConfig.MaxLevel);
            SetValue<uint>(PlayerFields.NextLevelXP, Cypher.ObjMgr.GetXPForLevel(getLevel()));

            // reset before any aura state sources (health set/aura apply)
            SetValue<uint>(UnitFields.AuraState, 0);

            UpdateSkillsForLevel();

            // set default cast time multiplier
            SetValue<float>(UnitFields.ModCastingSpeed, 1.0f);
            SetValue<float>(UnitFields.ModSpellHaste, 1.0f);
            //SetValue<float>(PlayerFields.ModHaste, 1.0f);
            SetValue<float>(PlayerFields.ModRangedHaste, 1.0f);

            // reset size before reapply auras
            SetObjectScale(1.0f);

            // save base values (bonuses already included in stored stats
            for (var i = Stats.Strength; i < Stats.Max; ++i)
                SetCreateStat(i, info.stats[(int)i]);

            for (var i = Stats.Strength; i < Stats.Max; ++i)
                SetStat(i, info.stats[(int)i]);

            SetCreateHealth(basehp);

            //set create powers
            SetCreateMana(basemana);

            InitStatBuffMods();

            //reset rating fields values
            for (var index = PlayerFields.CombatRatings; index < PlayerFields.CombatRatings + (int)CombatRating.Max; ++index)
                SetValue<uint>(index, 0);

            SetValue<uint>(PlayerFields.ModHealingDonePos, 0);
            SetValue<float>(PlayerFields.ModHealingPercent, 1.0f);
            SetValue<float>(PlayerFields.ModHealingDonePercent, 1.0f);
            for (byte i = 0; i < 7; ++i)
            {
                SetValue<uint>(PlayerFields.ModDamageDoneNeg + i, 0);
                SetValue<uint>(PlayerFields.ModDamageDonePos + i, 0);
                SetValue<float>(PlayerFields.ModDamageDonePercent + i, 1.00F);
            }
            SetValue<float>(PlayerFields.ModSpellPowerPercent, 1.0f);

            //reset attack power, damage and attack speed fields
            SetValue<float>(UnitFields.AttackRoundBaseTime, 2000.0F);
            SetValue<float>(UnitFields.AttackRoundBaseTime + 1, 2000.0F);
            SetValue<float>(UnitFields.RangedAttackRoundBaseTime, 2000.0F);

            SetValue<float>(UnitFields.MinDamage, 0.0f);
            SetValue<float>(UnitFields.MaxDamage, 0.0f);
            SetValue<float>(UnitFields.MinOffHandDamage, 0.0f);
            SetValue<float>(UnitFields.MaxOffHandDamage, 0.0f);
            SetValue<float>(UnitFields.MinRangedDamage, 0.0f);
            SetValue<float>(UnitFields.MaxRangedDamage, 0.0f);
            SetValue<float>(PlayerFields.WeaponDmgMultipliers, 1.0f);

            SetValue<int>(UnitFields.AttackPower, 0);
            SetValue<float>(UnitFields.AttackPowerMultiplier, 0.0f);
            SetValue<int>(UnitFields.RangedAttackPower, 0);
            SetValue<float>(UnitFields.RangedAttackPowerMultiplier, 0.0f);

            // Base crit values (will be recalculated in UpdateAllStats() at loading and in _ApplyAllStatBonuses() at reset
            SetValue<float>(PlayerFields.CritPercentage, 0.0f);
            SetValue<float>(PlayerFields.OffhandCritPercentage, 0.0f);
            SetValue<float>(PlayerFields.RangedCritPercentage, 0.0f);

            // Init spell schools (will be recalculated in UpdateAllStats() at loading and in _ApplyAllStatBonuses() at reset
            for (byte i = 0; i < 7; ++i)
                SetValue<float>(PlayerFields.SpellCritPercentage + i, 0.0f);

            SetValue<float>(PlayerFields.ParryPercentage, 0.0f);
            SetValue<float>(PlayerFields.BlockPercentage, 0.0f);

            // Static 30% damage blocked
            SetValue<uint>(PlayerFields.ShieldBlock, 30);

            // Dodge percentage
            SetValue<float>(PlayerFields.DodgePercentage, 0.0f);

            // set armor (resistance 0) to original value (create_agility*2)
            SetArmor((int)(GetCreateStat(Stats.Agility) * 2));
            SetResistanceBuffMods(SpellSchools.Normal, true, 0.0f);
            SetResistanceBuffMods(SpellSchools.Normal, false, 0.0f);
            // set other resistance to original value (0)
            for (var i = 1; i < (int)SpellSchools.Max; ++i)
            {
                SetResistance((SpellSchools)i, 0);
                SetResistanceBuffMods((SpellSchools)i, true, 0.0f);
                SetResistanceBuffMods((SpellSchools)i, false, 0.0f);
            }

            SetValue<uint>(PlayerFields.ModTargetResistance, 0);
            SetValue<uint>(PlayerFields.ModTargetPhysicalResistance, 0);
            for (var i = 0; i < (int)SpellSchools.Max; ++i)
            {
                SetValue<uint>(UnitFields.PowerCostModifier + i, 0);
                SetValue<float>(UnitFields.PowerCostMultiplier + i, 0.0f);
            }
            // Reset no reagent cost field
            for (byte i = 0; i < 3; ++i)
                SetValue<uint>(PlayerFields.NoReagentCostMask + i, 0);
            // Init data for form but skip reapply item mods for form
            InitDataForForm(reapplyMods);

            // save new stats
            for (var i = Powers.Mana; i < Powers.Max; ++i)
                SetMaxPower(i, GetCreatePowers(i));

            SetMaxHealth(basehp);                     // stamina bonus will applied later

            // cleanup mounted state (it will set correctly at aura loading if player saved at mount.
            SetValue<uint>(UnitFields.MountDisplayID, 0);

            // cleanup unit flags (will be re-applied if need at aura load).
            //RemoveFlag(UnitFields.Flags,
                //UnitFlags.NonAttackable | UnitFlags.DisableMove | UnitFlags.NotAttackable1 |
                //UnitFlags.ImmuneToPc | UnitFlags.ImmuneToNpc | UnitFlags.Looting |
                //UnitFlags.PetInCombat | UnitFlags.Silenced | UnitFlags.Pacified |
                //UnitFlags.Stunned | UnitFlags.InCombat | UnitFlags.Disarmed |
                //UnitFlags.Confused | UnitFlags.Fleeing | UnitFlags.NotSelectable |
                //UnitFlags.Skinnable | UnitFlags.Mount | UnitFlags.TaxiFlight);
            SetFlag(UnitFields.Flags, UnitFlags.Pvp);   // must be set

            SetFlag(UnitFields.Flags2, UnitFlags2.RegeneratePower);// must be set

            // cleanup player flags (will be re-applied if need at aura load), to avoid have ghost flag without ghost aura, for example.
            //RemoveFlag(PlayerFields.PlayerFlags, PlayerFlags.AFK | PlayerFlags.DND | PlayerFlags.GM | PlayerFlags.Ghost | PlayerFlags.AllowOnlyAbility);

            //RemoveStandFlags(UnitStandFlags.All);                 // one form stealth modified bytes
            //RemoveFlag(UnitFields.Bytes2, UnitPVPStateFlags.FFAPVP | UnitPVPStateFlags.Sanctuary, 1);

            // restore if need some important flags
            SetValue<uint>(PlayerFields.Bytes2, 0);                 // flags empty by default

            //if (reapplyMods)                                        // reapply stats values only on .reset stats (level) command
                //_ApplyAllStatBonuses();

            // set current level health and mana/energy to maximum after applying all mods.
            SetFullHealth();
            SetPower(Powers.Mana, GetMaxPower(Powers.Mana));
            SetPower(Powers.Energy, GetMaxPower(Powers.Energy));
            if (GetPower(Powers.Rage) > GetMaxPower(Powers.Rage))
                SetPower(Powers.Rage, GetMaxPower(Powers.Rage));
            SetPower(Powers.Focus, GetMaxPower(Powers.Focus));
            SetPower(Powers.RunicPower, 0);

            // update level to hunter/summon pet
            //if (Pet * pet = GetPet())
                //pet.SynchronizeLevelWithOwner();
        }
        void InitDataForForm(bool reapplyMods)
        {            
            ShapeshiftForm form = GetShapeshiftForm();

            SpellShapeshiftFormEntry ssEntry = DBCStorage.SpellShapeshiftFormStorage.LookupByKey((uint)form);
            if (ssEntry != null && ssEntry.attackSpeed != 0)
            {
                //SetAttackTime(BASE_ATTACK, ssEntry.attackSpeed);
                //SetAttackTime(OFF_ATTACK, ssEntry.attackSpeed);
                //SetAttackTime(RANGED_ATTACK, BASE_ATTACK_TIME);
            }
            else
                //SetRegularAttackTime();

            switch (form)
            {
                case ShapeshiftForm.Ghoul:
                case ShapeshiftForm.Cat:
                    {
                        if (getPowerType() != Powers.Energy)
                            setPowerType(Powers.Energy);
                        break;
                    }
                case ShapeshiftForm.Bear:
                    {
                        if (getPowerType() != Powers.Rage)
                            setPowerType(Powers.Rage);
                        break;
                    }
                default:                                            // 0, for example
                    {
                        CharClasses cEntry = DBCStorage.CharClassStorage.LookupByKey(getClass());
                        if (cEntry != null && cEntry.powerType < (uint)Powers.Max && (uint)getPowerType() != cEntry.powerType)
                            setPowerType((Powers)cEntry.powerType);
                        break;
                    }
            }

            // update auras at form change, ignore this at mods reapply (.reset stats/etc) when form not change.
            //if (!reapplyMods)
                //UpdateEquipSpellsAtFormChange();

            //UpdateAttackPowerAndDamage();
            //UpdateAttackPowerAndDamage(true);
        }
        void LoadTaxiMask(string data)
        {
            string[] lines = data.Split(' ');

            for (var index = 0; index < lines.Count(); index++)
            {
                //m_taximask[index] = sTaxiNodesMask[index] & uint32(atol(*iter));
            }
        }
        bool LoadHomeBind()
        {
            PlayerInfo info = Cypher.ObjMgr.GetPlayerInfo(getRace(), getClass());
            if (info == null)
            {
                Log.outError("Player (Name {0}) has incorrect race/class ({1}/{2}) pair. Can't be loaded.", GetName(), getRace(), getClass());
                return false;
            }

            bool ok = false;
            // SELECT mapId, zoneId, posX, posY, posZ FROM character_homebind WHERE guid = ?
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_HOMEBIND);
            stmt.AddValue(0, GetGUIDLow());
            SQLResult result = DB.Characters.Select(stmt);
            if (result.Count == 1)
            {
                m_homebindMapId = result.Read<uint>(0, 0);
                m_homebindZoneId = result.Read<uint>(0, 1);
                m_homebindX = result.Read<float>(0, 2);
                m_homebindY = result.Read<float>(0, 3);
                m_homebindZ = result.Read<float>(0, 4);

                MapEntry bindMapEntry = DBCStorage.MapStorage.LookupByKey(m_homebindMapId);

                // accept saved data only for valid position (and non instanceable), and accessable
                if (GridDefines.IsValidMapCoord(m_homebindMapId, m_homebindX, m_homebindY, m_homebindZ) &&
                    !bindMapEntry.Instanceable() && GetSession().GetExpansion() >= bindMapEntry.Expansion())
                    ok = true;
                else
                {
                    stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_PLAYER_HOMEBIND);
                    stmt.AddValue(0, GetGUIDLow());
                    DB.Characters.Execute(stmt);
                }
            }

            if (!ok)
            {
                m_homebindMapId = info.MapId;
                m_homebindZoneId = info.ZoneId;
                m_homebindX = info.PositionX;
                m_homebindY = info.PositionY;
                m_homebindZ = info.PositionZ;

                stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_INS_PLAYER_HOMEBIND);
                stmt.AddValue(0, GetGUIDLow());
                stmt.AddValue(1, m_homebindMapId);
                stmt.AddValue(2, m_homebindZoneId);
                stmt.AddValue(3, m_homebindX);
                stmt.AddValue(4, m_homebindY);
                stmt.AddValue(5, m_homebindZ);
                DB.Characters.Execute(stmt);
            }

            Log.outDebug("Setting player home position - mapid: {0}, areaid: {1}, X: {2}, Y: {3}, Z: {4}",
                m_homebindMapId, m_homebindZoneId, m_homebindX, m_homebindY, m_homebindZ);

            return true;
        }
        void LoadInventory()
        {
            //"SELECT creatorGuid, giftCreatorGuid, count, duration, charges, flags, enchantments, randomPropertyId, durability, playedTime, text, bag, slot, " +
            //"item, itemEntry FROM character_inventory ci JOIN item_instance ii ON ci.item = ii.guid WHERE ci.guid = ? ORDER BY bag, slot";

            //NOTE: the "order by `bag`" is important because it makes sure
            //the bagMap is filled before items in the bags are loaded
            //NOTE2: the "order by `slot`" is needed because mainhand weapons are (wrongly?)
            //expected to be equipped before offhand items (TODO: fixme)
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_INVENTORY);
            stmt.AddValue(0, GetGUIDLow());
            SQLResult result = DB.Characters.Select(stmt);

            if (result.Count != 0)
            {
                Dictionary<uint, Bag> bagMap = new Dictionary<uint, Bag>();                               // fast guid lookup for bags
                Dictionary<uint, Item> invalidBagMap = new Dictionary<uint, Item>();                       // fast guid lookup for bags
                //std::list<Item*> problematicItems;

                // Prevent items from being added to the queue while loading
                m_itemUpdateQueueBlocked = true;
                for (var i = 0; i < result.Count; i++)
                {
                    Item item = LoadItem(result.Read<uint>(i, 13), result.Read<uint>(i, 14), 0);
                    if (item != null)
                    {
                        uint bagGuid = result.Read<uint>(i, 11);
                        byte slot = result.Read<byte>(i, 12);

                        InventoryResult err = InventoryResult.Ok;
                        // Item is not in bag
                        if (bagGuid == 0)
                        {
                            item.SetContainer(null);
                            item.SetSlot(slot);

                            if (IsInventoryPos(InventorySlots.Bag0, slot))
                            {
                                List<ItemPosCount> dest = new List<ItemPosCount>();
                                err = CanStoreItem(InventorySlots.Bag0, slot, ref dest, item, false);
                                if (err == InventoryResult.Ok)
                                    item = StoreItem(dest, item, true);
                            }
                            else if (IsEquipmentPos(InventorySlots.Bag0, slot))
                            {
                                ushort dest;

                                err = CanEquipItem(slot, out dest, item, false, false);
                                if (err == InventoryResult.Ok)
                                    QuickEquipItem(dest, item);
                            }
                            else if (IsBankPos(InventorySlots.Bag0, slot))
                            {
                                List<ItemPosCount> dest = new List<ItemPosCount>();
                                err = CanBankItem(InventorySlots.Bag0, slot, ref dest, item, false, false);
                                if (err == InventoryResult.Ok)
                                    item = BankItem(dest, item, true);
                            }

                            // Remember bags that may contain items in them
                            if (err == InventoryResult.Ok)
                            {
                                if (IsBagPos(item.GetPos()))
                                {
                                    Bag pBag = item.ToBag();
                                    if (pBag != null)
                                        bagMap.Add(item.GetGUIDLow(), pBag);
                                }
                            }
                            else if (IsBagPos(item.GetPos()))
                                if (item.IsBag())
                                    invalidBagMap.Add(item.GetGUIDLow(), item);
                        }
                        else
                        {
                            item.SetSlot(ItemConst.NullSlot);
                            // Item is in the bag, find the bag
                            var itr = bagMap.LookupByKey(bagGuid);
                            if (itr != null)
                            {
                                List<ItemPosCount> dest = new List<ItemPosCount>();
                                err = CanStoreItem(itr.GetSlot(), slot, ref dest, item);
                                if (err == InventoryResult.Ok)
                                    item = StoreItem(dest, item, true);
                            }
                            else if (invalidBagMap.LookupByKey(bagGuid) != null)
                            {
                                //var itr = invalidBagMap.LookupByKey(bagGuid);
                                //if (find(problematicItems.begin(), problematicItems.end(), itr.second) != problematicItems.end())
                                //err = InventoryResult.InternalBagError;
                            }
                            else
                            {
                                Log.outError("LoadInventory: player (GUID: {0}, name: '{1}') has item (GUID: {2}, entry: {3}) which doesnt have a valid bag (Bag GUID: {4}, slot: {5}). Possible cheat?",
                                    GetGUIDLow(), GetName(), item.GetGUIDLow(), item.GetEntry(), bagGuid, slot);
                                item.DeleteFromInventoryDB();
                                continue;
                            }

                        }

                        // Item's state may have changed after storing
                        if (err == InventoryResult.Ok)
                            item.SetState(ItemUpdateState.Unchanged, this);
                        else
                        {
                            Log.outError("LoadInventory: player (GUID: {0}, name: '{1}') has item (GUID: {2}, entry: {3}) which can't be loaded into inventory (Bag GUID: {4}, slot: {5}) by reason {6}. " +
                                "Item will be sent by mail.", GetGUIDLow(), GetName(), item.GetGUIDLow(), item.GetEntry(), bagGuid, slot, err);
                            item.DeleteFromInventoryDB();
                            //problematicItems.push_back(item);
                        }
                    }
                }

                m_itemUpdateQueueBlocked = false;

                // Send problematic items by mail
                //while (!problematicItems.empty())
                {
                    //string subject = GetSession().GetCypherString(LANG_NOT_EQUIPPED_ITEM);

                    //MailDraft draft(subject, "There were problems with equipping item(s).");
                    //for (uint8 i = 0; !problematicItems.empty() && i < MAX_MAIL_ITEMS; ++i)
                    {
                        //draft.AddItem(problematicItems.front());
                        //problematicItems.pop_front();
                    }
                    //draft.SendMailTo(trans, this, MailSender(this, MAIL_STATIONERY_GM), MAIL_CHECK_MASK_COPIED);
                }
                //CharacterDatabase.CommitTransaction(trans);
            }
            //if (isAlive())
            //_ApplyAllItemMods();
        }
        void LoadCurrency()
        {
            CurrencyStorage = new Dictionary<uint, PlayerCurrency>();
            //       0         1           2
            //SELECT currency, week_count, total_count FROM character_currency WHERE guid = ?
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_PLAYER_CURRENCY);
            stmt.AddValue(0, GetGUIDLow());
            SQLResult result = DB.Characters.Select(stmt);
            if (result.Count == 0)
                return;

            for (var i = 0; i < result.Count; i++)
            {
                ushort currencyID = result.Read<ushort>(i, 0);

                CurrencyTypesEntry currency = DBCStorage.CurrencyTypesStorage.LookupByKey(currencyID);
                if (currency == null)
                    continue;

                PlayerCurrency cur = new PlayerCurrency();
                cur.state = PlayerCurrencyState.Unchanged;
                cur.weekCount = result.Read<uint>(i, 1);
                cur.totalCount = result.Read<uint>(i, 2);

                CurrencyStorage.Add(currencyID, cur);
            }
        }

        //Save
        public void SaveToDB(bool create = false)
        {
            //INSERT INTO characters (guid, account, name, race, class, gender, level, xp, money, playerBytes, playerBytes2, playerFlags = " +
            //"map, instance_id, instance_mode_mask, position_x, position_y, position_z, orientation = " +
            //"taximask, cinematic = " +
            //"totaltime, leveltime, rest_bonus, logout_time, is_logout_resting, resettalents_cost, resettalents_time, talentTree = " +
            //"extra_flags, stable_slots, at_login, zone = " +
            //"death_expire_time, taxi_path, totalKills = " +
            //"todayKills, yesterdayKills, chosenTitle, watchedFaction, drunk, health, power1, power2, power3 = " +
            //"power4, power5, latency, speccount, activespec, exploredZones, equipmentCache, knownTitles, actionBars, grantableLevels)
            
            // delay auto save at any saves (manual, in code, or autosave)
            //m_nextSave = sWorld.getIntConfig(CONFIG_INTERVAL_SAVE);

            //lets allow only players in world to be saved
            //if (IsBeingTeleportedFar())
            {
                //ScheduleDelayedOperation(DELAYED_SAVE_PLAYER);
                //return;
            }

            // first save/honor gain after midnight will also update the player's honor fields
            //UpdateHonorFields();

            PreparedStatement stmt;
            var index = 0;
            if (create)
            {
                stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_INS_CHARACTER);
                //! Insert query
                //! TO DO: Filter out more redundant fields that can take their default value at player create
                stmt.AddValue(index++, GetGUIDLow());
                stmt.AddValue(index++, GetSession().GetAccountId());
                stmt.AddValue(index++, GetName());
                stmt.AddValue(index++, getRace());
                stmt.AddValue(index++, getClass());
                stmt.AddValue(index++, getGender());
                stmt.AddValue(index++, getLevel());
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.XP));
                stmt.AddValue(index++, GetMoney());
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.Bytes));
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.Bytes2));
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.PlayerFlags));
                stmt.AddValue(index++, (ushort)GetMapId());
                stmt.AddValue(index++, (uint)0);//GetInstanceId());
                stmt.AddValue(index++, (byte)0);//(GetDungeonDifficulty()) | uint8(GetRaidDifficulty()) << 4));
                stmt.AddValue(index++, GetPositionX());
                stmt.AddValue(index++, GetPositionY());
                stmt.AddValue(index++, GetPositionZ());
                stmt.AddValue(index++, GetOrientation());

                StringBuilder ss = new StringBuilder();
                ss.Append("");//m_taxi);
                stmt.AddValue(index++, ss.ToString());
                stmt.AddValue(index++, 0);//m_cinematic);
                stmt.AddValue(index++, 0);//m_Played_time[PLAYED_TIME_TOTAL]);
                stmt.AddValue(index++, 0);//m_Played_time[PLAYED_TIME_LEVEL]);
                stmt.AddValue(index++, 0);//finiteAlways(m_rest_bonus));
                stmt.AddValue(index++, Time.getMSTime());//uint32(time(NULL)));//wrong?
                stmt.AddValue(index++, HasFlag(PlayerFields.PlayerFlags, PlayerFlags.Resting) ? 1 : 0);
                //save, far from tavern/city
                //save, but in tavern/city
                stmt.AddValue(index++, 0);//GetTalentResetCost());
                stmt.AddValue(index++, 0);//GetTalentResetTime());

                ss.Clear();
                for (var i = 0; i < PlayerConst.MaxTalentSpecs; ++i)
                    ss.AppendFormat("{0} ", 0);//GetPrimaryTalentTree(i));
                stmt.AddValue(index++, ss.ToString());
                stmt.AddValue(index++, (ushort)m_ExtraFlags);
                stmt.AddValue(index++, 0);// m_stableSlots);
                stmt.AddValue(index++, (ushort)atLoginFlags);
                stmt.AddValue(index++, GetZoneId());
                stmt.AddValue(index++, 0);//uint32(m_deathExpireTime));

                ss.Clear();
                //ss << m_taxi.SaveTaxiDestinationsToString();

                stmt.AddValue(index++, ss.ToString());
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.LifetimeHonorableKills));
                stmt.AddValue(index++, GetValue<ushort>(PlayerFields.YesterdayHonorableKills, 0));
                stmt.AddValue(index++, GetValue<ushort>(PlayerFields.YesterdayHonorableKills, 1));
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.PlayerTitle));
                stmt.AddValue(index++, GetValue<int>(PlayerFields.WatchedFactionIndex));
                stmt.AddValue(index++, 0);//GetDrunkValue());
                stmt.AddValue(index++, GetHealth());

                int storedPowers = 0;
                for (uint i = 0; i < (int)Powers.Max; ++i)
                {
                    if (Cypher.ObjMgr.GetPowerIndexByClass(i, getClass()) != (int)Powers.Max)
                    {
                        stmt.AddValue(index++, GetValue<uint>(UnitFields.Power + storedPowers));
                        storedPowers++;
                        if (storedPowers >= (int)Powers.MaxPerClass)
                            break;
                    }
                }

                for (; storedPowers < PlayerConst.MaxPowersPerClass; ++storedPowers)
                    stmt.AddValue(index++, 0);

                stmt.AddValue(index++, 0);//GetSession().GetLatency());

                stmt.AddValue(index++, 0);//GetSpecsCount());
                stmt.AddValue(index++, 0);//GetActiveSpec());

                ss.Clear();
                for (var i = 0; i < PlayerConst.ExploredZonesSize; ++i)
                    ss.AppendFormat("{0} ", GetValue<uint>(PlayerFields.ExploredZones + i));
                stmt.AddValue(index++, ss.ToString());

                ss.Clear();
                // cache equipment...
                for (var i = 0; i < (int)EquipmentSlot.End * 2; ++i)
                    ss.AppendFormat("{0} ", GetValue<uint>(PlayerFields.VisibleItems + i));

                // ...and bags for enum opcode
                for (var i = InventorySlots.BagStart; i < InventorySlots.BagEnd; ++i)
                {
                    Item item = GetItemByPos(InventorySlots.Bag0, i);
                    if (item != null)
                        ss.Append(item.GetEntry());
                    else
                        ss.Append('0');

                    ss.Append(" 0 ");
                }
                stmt.AddValue(index++, ss.ToString());

                ss.Clear();
                for (var i = 0; i < (int)PlayerTitle.KnowTitlesSize * 2; ++i)
                    ss.AppendFormat("{0} ", GetValue<uint>(PlayerFields.KnownTitles + i));
                stmt.AddValue(index++, ss.ToString());

                stmt.AddValue(index++, GetValue<byte>(PlayerFields.Bytes, 2));
                stmt.AddValue(index++, 0);//m_grantableLevels);
            }
            else
            {
                // Update query
                stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_UPD_CHARACTER);
                stmt.AddValue(index++, GetName());
                stmt.AddValue(index++, getRace());
                stmt.AddValue(index++, getClass());
                stmt.AddValue(index++, getGender());
                stmt.AddValue(index++, getLevel());
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.XP));
                stmt.AddValue(index++, GetMoney());
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.Bytes));
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.Bytes2));
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.PlayerFlags));

                //if (!IsBeingTeleported())
                {
                    stmt.AddValue(index++, (ushort)GetMapId());
                    stmt.AddValue(index++, (uint)0);//GetInstanceId());
                    stmt.AddValue(index++, (byte)0);//uint8(GetDungeonDifficulty()) | uint8(GetRaidDifficulty()) << 4));
                    stmt.AddValue(index++, GetPositionX());
                    stmt.AddValue(index++, GetPositionY());
                    stmt.AddValue(index++, GetPositionZ());
                    stmt.AddValue(index++, GetOrientation());
                }
                //else
                {
                    /*
                    stmt.AddValue(index++, (uint16)GetTeleportDest().GetMapId());
                    stmt.AddValue(index++, (uint32)0);
                    stmt.AddValue(index++, (uint8(GetDungeonDifficulty()) | uint8(GetRaidDifficulty()) << 4));
                    stmt.AddValue(index++, finiteAlways(GetTeleportDest().GetPositionX()));
                    stmt.AddValue(index++, finiteAlways(GetTeleportDest().GetPositionY()));
                    stmt.AddValue(index++, finiteAlways(GetTeleportDest().GetPositionZ()));
                    stmt.AddValue(index++, finiteAlways(GetTeleportDest().GetOrientation()));
                    */
                }
                StringBuilder ss = new StringBuilder();

                ss.Append("");// m_taxi;
                stmt.AddValue(index++, ss.ToString());
                stmt.AddValue(index++, 0);//m_cinematic);
                stmt.AddValue(index++, 0);//m_Played_time[PLAYED_TIME_TOTAL]);
                stmt.AddValue(index++, 0);//m_Played_time[PLAYED_TIME_LEVEL]);
                stmt.AddValue(index++, 0);//finiteAlways(m_rest_bonus));
                stmt.AddValue(index++, Time.getMSTime());//uint32(time(NULL)));
                stmt.AddValue(index++, HasFlag(PlayerFields.PlayerFlags, PlayerFlags.Resting) ? 1 : 0);
                //save, far from tavern/city
                //save, but in tavern/city
                stmt.AddValue(index++, 0);//GetTalentResetCost());
                stmt.AddValue(index++, 0);//GetTalentResetTime());

                ss.Clear();
                for (var i = 0; i < PlayerConst.MaxTalentSpecs; ++i)
                    ss.AppendFormat("{0} ", 0);// GetPrimaryTalentTree(i) + " ");
                stmt.AddValue(index++, ss.ToString());
                stmt.AddValue(index++, (ushort)m_ExtraFlags);
                stmt.AddValue(index++, 0);//m_stableSlots);
                stmt.AddValue(index++, (ushort)atLoginFlags);
                stmt.AddValue(index++, GetZoneId());
                stmt.AddValue(index++, 0);//uint32(m_deathExpireTime));

                ss.Clear();
                //ss << m_taxi.SaveTaxiDestinationsToString();

                stmt.AddValue(index++, ss.ToString());
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.LifetimeHonorableKills));
                stmt.AddValue(index++, GetValue<ushort>(PlayerFields.YesterdayHonorableKills, 0));
                stmt.AddValue(index++, GetValue<ushort>(PlayerFields.YesterdayHonorableKills, 1));
                stmt.AddValue(index++, GetValue<uint>(PlayerFields.PlayerTitle));
                stmt.AddValue(index++, GetValue<int>(PlayerFields.WatchedFactionIndex));
                stmt.AddValue(index++, 0);//GetDrunkValue());
                stmt.AddValue(index++, GetHealth());

                int storedPowers = 0;
                for (uint i = 0; i < (int)Powers.Max; ++i)
                {
                    if (Cypher.ObjMgr.GetPowerIndexByClass(i, getClass()) != (int)Powers.Max)
                    {
                        stmt.AddValue(index++, GetValue<uint>(UnitFields.Power + storedPowers));
                        storedPowers++;
                        if (storedPowers >= (int)Powers.MaxPerClass)
                            break;
                    }
                }

                for (; storedPowers < PlayerConst.MaxPowersPerClass; ++storedPowers)
                    stmt.AddValue(index++, 0);

                stmt.AddValue(index++, 0);//GetSession().GetLatency());

                stmt.AddValue(index++, 0);//GetSpecsCount());
                stmt.AddValue(index++, 0);//GetActiveSpec());

                ss.Clear();
                for (var i = 0; i < PlayerConst.ExploredZonesSize; ++i)
                    ss.AppendFormat("{0} ", GetValue<uint>(PlayerFields.ExploredZones + i));
                stmt.AddValue(index++, ss.ToString());

                ss.Clear();
                // cache equipment...
                for (var i = 0; i < (int)EquipmentSlot.End * 2; ++i)
                    ss.AppendFormat("{0} ", GetValue<uint>(PlayerFields.VisibleItems + i));

                // ...and bags for enum opcode
                for (var i = InventorySlots.BagStart; i < InventorySlots.BagEnd; ++i)
                {
                    Item item = GetItemByPos(InventorySlots.Bag0, i);
                    if (item != null)
                        ss.Append(item.GetEntry());
                    else
                        ss.Append('0');
                    ss.Append(" 0 ");
                }

                stmt.AddValue(index++, ss.ToString());

                ss.Clear();
                for (var i = 0; i < (int)PlayerTitle.KnowTitlesSize * 2; ++i)
                    ss.AppendFormat("{0} ", GetValue<uint>(PlayerFields.KnownTitles + i));

                stmt.AddValue(index++, ss.ToString());
                stmt.AddValue(index++, GetValue<byte>(PlayerFields.Bytes, 2));
                stmt.AddValue(index++, 0);//m_grantableLevels);

                stmt.AddValue(index++, IsInWorld ? 1 : 0);
                // Index
                stmt.AddValue(index++, GetGUIDLow());
            }

            DB.Characters.Execute(stmt);

            //if (m_mailsUpdated)                                     //save mails only when needed
                //_SaveMail(trans);
            
            //_SaveBGData(trans);
            SaveInventory();
            //_SaveVoidStorage(trans);
            //_SaveQuestStatus(trans);
            //_SaveDailyQuestStatus(trans);
            //_SaveWeeklyQuestStatus(trans);
            //_SaveSeasonalQuestStatus(trans);
            //_SaveTalents(trans);
            SaveSpells();
            //_SaveSpellCooldowns(trans);
            //_SaveActions(trans);
            //_SaveAuras(trans);
            SaveSkills();
            //m_achievementMgr.SaveToDB(trans);
            //reputationMgr.SaveToDB();
            //_SaveEquipmentSets(trans);
            GetSession().SaveTutorialsData();                 // changed only while character in game
            //_SaveGlyphs(trans);
            //_SaveInstanceTimeRestrictions(trans);
            //_SaveCurrency(trans);
            //_SaveCUFProfiles(trans);

            // check if stats should only be saved on logout
            // save stats can be out of transaction
            //if (m_session.isLogingOut() || !sWorld.getBoolConfig(CONFIG_STATS_SAVE_ONLY_ON_LOGOUT))
                //_SaveStats(trans);
            
            // save pet (hunter pet level and experience and all type pets health/mana).
            //if (Pet* pet = GetPet())
                //pet.SavePetToDB(PET_SAVE_AS_CURRENT);
        }
        void SaveSpells()
        {
            PreparedStatement stmt = null;

            foreach (var spell in Spells)
            {
                if (spell.State == PlayerSpellState.Removed || spell.State == PlayerSpellState.Changed)
                {
                    stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_CHAR_SPELL_BY_SPELL);
                    stmt.AddValue(0, spell.SpellId);
                    stmt.AddValue(1, GetGUIDLow());
                    DB.Characters.Execute(stmt);
                }

                // add only changed/new not dependent spells
                if (!spell.Dependent && (spell.State == PlayerSpellState.New || spell.State == PlayerSpellState.Changed))
                {
                    stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_INS_CHAR_SPELL);
                    stmt.AddValue(0, GetGUIDLow());
                    stmt.AddValue(1, spell.SpellId);
                    stmt.AddValue(2, spell.Active);
                    stmt.AddValue(3, spell.Disabled);
                    DB.Characters.Execute(stmt);
                }

                if (spell.State == PlayerSpellState.Removed)
                    Spells.Remove(spell);
                else
                {
                    spell.State = PlayerSpellState.Unchanged;
                    continue;
                }
            }
        }
        void SaveSkills()
        {
            PreparedStatement stmt;// = null;

            foreach (var skill in Skills)
            {
                if (skill.Value.State == SkillState.Unchanged)
                    continue;

                if (skill.Value.State == SkillState.Deleted)
                {
                    stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_CHAR_SKILL_BY_SKILL);
                    stmt.AddValue(0, GetGUIDLow());
                    stmt.AddValue(1, skill.Key);
                    DB.Characters.Execute(stmt);

                    Skills.Remove(skill.Key);
                    continue;
                }

                uint valueData = GetValue<uint>(GetPlayerSkillValueIndex(skill.Value.Pos));
                uint value = SkillValue(valueData);
                uint max = SkillMax(valueData);

                switch (skill.Value.State)
                {
                    case SkillState.New:
                        stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_INS_CHAR_SKILLS);
                        stmt.AddValue(0, GetGUIDLow());
                        stmt.AddValue(1, (ushort)skill.Key);
                        stmt.AddValue(2, value);
                        stmt.AddValue(3, max);
                        DB.Characters.Execute(stmt);
                        break;
                    case SkillState.Changed:
                        stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_UDP_CHAR_SKILLS);
                        stmt.AddValue(0, value);
                        stmt.AddValue(1, max);
                        stmt.AddValue(2, GetGUIDLow());
                        stmt.AddValue(3, (ushort)skill.Key);
                        DB.Characters.Execute(stmt);
                        break;
                    default:
                        break;
                }
                skill.Value.State = SkillState.Unchanged;
            }
        }
        void SaveInventory()
        {
            PreparedStatement stmt;
            // force items in buyback slots to new state
            // and remove those that aren't already
            for (var i = InventorySlots.BuyBackStart; i < InventorySlots.BuyBackEnd; ++i)
            {
                Item item = Items[i];
                if (item == null || item.GetState() == ItemUpdateState.New)
                    continue;

                stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_CHAR_INVENTORY_BY_ITEM);
                stmt.AddValue(0, item.GetGUIDLow());
                DB.Characters.Execute(stmt);

                stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_ITEM_INSTANCE);
                stmt.AddValue(0, item.GetGUIDLow());
                DB.Characters.Execute(stmt);
                Items[i].FSetState(ItemUpdateState.New);
            }

            // Updated played time for refundable items. We don't do this in Player::Update because there's simply no need for it,
            // the client auto counts down in real time after having received the initial played time on the first
            // SMSG_ITEM_REFUND_INFO_RESPONSE packet.
            // Item::UpdatePlayedTime is only called when needed, which is in DB saves, and item refund info requests.
            /*
            std::set<uint32>::iterator i_next;
            for (std::set<uint32>::iterator itr = m_refundableItems.begin(); itr!= m_refundableItems.end(); itr = i_next)
            {
                // use copy iterator because itr may be invalid after operations in this loop
                i_next = itr;
                ++i_next;

                Item* iPtr = GetItemByGuid(MAKE_NEW_GUID(*itr, 0, HIGHGUID_ITEM));
                if (iPtr)
                {
                    iPtr.UpdatePlayedTime(this);
                    continue;
                }
                else
                {
                    sLog.outError(LOG_FILTER_PLAYER, "Can't find item guid %u but is in refundable storage for player %u ! Removing.", *itr, GetGUIDLow());
                    m_refundableItems.erase(itr);
                }
            }
            */
            // update enchantment durations
            //for (EnchantDurationList::iterator itr = m_enchantDuration.begin(); itr != m_enchantDuration.end(); ++itr)
                //itr.item.SetEnchantmentDuration(itr.slot, itr.leftduration, this);

            // if no changes
            if (ItemUpdateQueue.Count == 0)
                return;

            uint lowGuid = GetGUIDLow();
            for (var i = 0; i < ItemUpdateQueue.Count; ++i)
            {
                Item item = ItemUpdateQueue[i];
                if (item == null)
                    continue;

                Bag container = item.GetContainer();
                uint bag_guid = container != null ? container.GetGUIDLow() : 0;

                if (item.GetState() != ItemUpdateState.Removed)
                {
                    Item test = GetItemByPos(item.GetBagSlot(), item.GetSlot());
                    if (test == null)
                    {
                        uint bagTestGUID = 0;
                        Item test2 = GetItemByPos(InventorySlots.Bag0, item.GetBagSlot());
                        if (test2 != null)
                            bagTestGUID = test2.GetGUIDLow();
                        Log.outError("Player(GUID: {0} Name: {1}).SaveInventory - the bag({2}) and slot({3}) values for the item with guid {4} (state {5}) are incorrect, " +
                            "the player doesn't have an item at that position!", lowGuid, GetName(), item.GetBagSlot(), item.GetSlot(), item.GetGUIDLow(), item.GetState());
                        // according to the test that was just performed nothing should be in this slot, delete
                        stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_CHAR_INVENTORY_BY_BAG_SLOT);
                        stmt.AddValue(0, bagTestGUID);
                        stmt.AddValue(1, item.GetSlot());
                        stmt.AddValue(2, lowGuid);
                        DB.Characters.Execute(stmt);

                        // also THIS item should be somewhere else, cheat attempt
                        item.FSetState(ItemUpdateState.Removed); // we are IN updateQueue right now, can't use SetState which modifies the queue
                        //DeleteRefundReference(item.GetGUIDLow());
                    }
                    else if (test != item)
                    {
                        Log.outError("Player(GUID: {0} Name: {1}).SaveInventory - the bag({2}) and slot({3}) values for the item with guid {4} are incorrect, " +                            
                            "the item with guid {5} is there instead!", lowGuid, GetName(), item.GetBagSlot(), item.GetSlot(), item.GetGUIDLow(), test.GetGUIDLow());
                        // save all changes to the item...
                        if (item.GetState() != ItemUpdateState.New) // only for existing items, no dupes
                            item.SaveToDB();
                        // ...but do not save position in inventory
                        continue;
                    }
                }

                switch (item.GetState())
                {
                    case ItemUpdateState.New:
                    case ItemUpdateState.Changed:
                        stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_REP_INVENTORY_ITEM);
                        stmt.AddValue(0, lowGuid);
                        stmt.AddValue(1, bag_guid);
                        stmt.AddValue(2, item.GetSlot());
                        stmt.AddValue(3, item.GetGUIDLow());
                        DB.Characters.Execute(stmt);
                        break;
                    case ItemUpdateState.Removed:
                        stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_CHAR_INVENTORY_BY_ITEM);
                        stmt.AddValue(0, item.GetGUIDLow());
                        DB.Characters.Execute(stmt);
                        break;
                    case ItemUpdateState.Unchanged:
                        break;
                }

                item.SaveToDB();                                   // item have unchanged inventory record and can be save standalone
            }
            ItemUpdateQueue.Clear();
        }
        #endregion

        #region Reputation
        public ReputationRank GetReputationRank(uint faction)
        {
            var factionEntry = DBCStorage.FactionStorage.LookupByKey(faction);
            return GetReputationMgr().GetRank(factionEntry);
        }
        public ReputationMgr GetReputationMgr() { return reputationMgr; }
        #endregion

        #region Sends / Updates
        public void UpdateEnvironment(ref List<WorldObject> updatedObjects)
        {
            List<WorldObject> toRemove = new List<WorldObject>();
            toRemove.AddRange(knownObjects);
            toRemove.Remove(this);

            UpdateData data = new UpdateData(GetMapId());
            foreach (var obj in GetObjectsInRange<WorldObject>())
            {
                if (!knownObjects.Contains(obj))
                {
                    obj.BuildCreateUpdateBlockForPlayer(ref data, this);

                    knownObjects.Add(obj);
                    if (obj.RequiresUpdate)
                        updatedObjects.Add(obj);
                }
                else if (obj.RequiresUpdate)
                {
                    updatedObjects.Add(obj);
                    obj.BuildValuesUpdateBlockForPlayer(ref data, this);
                }
                toRemove.Remove(obj);
            }

            foreach (var obj in toRemove)
            {
                data.AddOutOfRangeGUID(obj.GetPackGUID());
                knownObjects.Remove(obj);
            }

            data.SendPackets(GetSession());
            toRemove.Clear();
        }

        void BeforeVisibilityDestroy(Creature t, Player p)
        {
            //if (p.GetPetGUID() == t.GetGUID() && t.ToCreature().isPet())
            //((Pet*)t)->Remove(PET_SAVE_NOT_IN_SLOT, true);
        }
        public void UpdateVisibilityOf(WorldObject target)
        {
            if (HaveAtClient(target))
            {
                if (!canSeeOrDetect(target, false, true))
                {
                    if (target.GetTypeId() == ObjectType.Unit)
                        BeforeVisibilityDestroy(target.ToCreature(), this);

                    target.DestroyForPlayer(this);
                    m_clientGUIDs.Remove(target.GetGUID());
                }
            }
            else
            {
                if (canSeeOrDetect(target, false, true))
                {
                    //if (target->isType(TYPEMASK_UNIT) && ((Unit*)target)->m_Vehicle)
                    //    UpdateVisibilityOf(((Unit*)target)->m_Vehicle);

                    target.SendUpdateToPlayer(this);
                    m_clientGUIDs.Add(target.GetGUID());

                    // target aura duration for caster show only if target exist at caster client
                    // send data at target visibility change (adding to client)
                    if (target.GetTypeId() == ObjectType.Unit)//not sure
                        SendInitialVisiblePackets((Unit)target);
                }
            }
        }
        public void UpdateVisibilityOf(WorldObject target, ref UpdateData data, ref List<Unit> visibleNow)
        {
            if (HaveAtClient(target))
            {
                if (!canSeeOrDetect(target, false, true))
                {
                    //BeforeVisibilityDestroy<T>(target, this);

                    //target.BuildOutOfRangeUpdateBlock(ref data);
                    m_clientGUIDs.Remove(target.GetGUID());
                }
            }
            else //if (visibleNow.size() < 30 || target->GetTypeId() == TYPEID_UNIT && target->ToCreature()->IsVehicle())
            {
                if (canSeeOrDetect(target, false, true))
                {
                    //if (target->isType(TYPEMASK_UNIT) && ((Unit*)target)->m_Vehicle)
                    //    UpdateVisibilityOf(((Unit*)target)->m_Vehicle, data, visibleNow);

                    target.BuildCreateUpdateBlockForPlayer(ref data, this);
                    m_clientGUIDs.Add(target.GetGUID());//UpdateVisibilityOf_helper(m_clientGUIDs, target, visibleNow);

                }
            }
        }
        void SendInitialVisiblePackets(Unit target)
        {
            //SendAurasForTarget(target);
            if (target.isAlive())
            {
                if (target.HasUnitState(UnitState.Melee_Attacking) && target.getVictim() != null)
                    target.SendMeleeAttackStart(target.getVictim());
            }
        }

        public override void UpdateObjectVisibility(bool forced = true)
        {
            if (!forced)
                AddToNotify(NotifyFlags.VISIBILITY_CHANGED);
            else
            {
                base.UpdateObjectVisibility(true);
                UpdateVisibilityForPlayer();
            }
        }

        void UpdateVisibilityForPlayer()
        {
            // updates visibility of all objects around point of view for current player
            var notifier = new VisibleNotifier(this);
            m_seer.VisitNearbyObject(GetSightRange(), notifier);
            notifier.SendToSelf();   // send gathered data
        }

        public void BuildCreateUpdateBlockForSelf(ref UpdateData data, Player target)
        {
            for (var i = 0; i < (byte)EquipmentSlot.End; ++i)
            {
                if (Items[i] == null)
                    continue;

                Items[i].BuildCreateUpdateBlockForPlayer(ref data, target);
            }

            if (target == this)
            {
                for (var i = InventorySlots.BagStart; i < InventorySlots.BankBagEnd; ++i)
                {
                    if (Items[i] == null)
                        continue;
                    
                    Items[i].BuildCreateUpdateBlockForPlayer(ref data, target);
                }
            }
            BuildCreateUpdateBlockForPlayer(ref data, target);
        }
        public void SendBroadcast(PacketWriter packet)
        {
            foreach (var player in GetPlayersInRange())
                player.GetSession().Send(packet);
        }
        void SendMessageToSetInRange(PacketWriter data, float dist, bool self)
        {
            if (self)
                GetSession().Send(data);

            MessageDistDeliverer notifier = new MessageDistDeliverer(this, data, dist);
            VisitNearbyWorldObject(dist, notifier);
        }
        public void SendDirectMessage(PacketWriter data) { GetSession().Send(data); }
        public bool UpdatePostion(ObjectPosition obj, bool teleport = false)
        {
            if (!UpdatePosition(obj.X, obj.Y, obj.Z, obj.Orientation, teleport))
                return false;

            //if (movementInfo.flags & MOVEMENTFLAG_MOVING)
            //    mover.RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_MOVE);
            //if (movementInfo.flags & MOVEMENTFLAG_TURNING)
            //    mover.RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_TURNING);
            //AURA_INTERRUPT_FLAG_JUMP not sure

            // group update
            //if (GetGroup())
            //SetGroupUpdateFlag(GROUP_UPDATE_FLAG_POSITION);

            //if (GetTrader() && !IsWithinDistInMap(GetTrader(), INTERACTION_DISTANCE))
            //GetSession().SendCancelTrade();

            //CheckAreaExploreAndOutdoor();

            return true;
        }
        public void SendSysMessage(CypherStrings str, params object[] args)
        {
            string input = Cypher.ObjMgr.GetCypherString(str);
            string pattern = @"%(\d+(\.\d+)?)?(d|f|s|u)";

            int count = 0;
            string result = Regex.Replace(input, pattern, m =>
            {
                //var number = m.Groups["Number"].Value;
                //var type = m.Groups["Type"].Value;
                // now you can have custom logic to check the type appropriately
                // check the types, format with the count for the current parameter
                return String.Concat("{", count++, "}");
            });

            SendSysMessage(result, args);
        }
        public void SendSysMessage(string str, params object[] args)
        {
            string msg = string.Format(str, args);
            ChatHandler.SendSysMessage(GetSession(), msg);
        }
        public void SendBuyError(BuyResult msg, Creature creature, uint item)//, uint /*param*/)
        {
            Log.outDebug("WORLD: Sent SMSG_BUY_FAILED");
            PacketWriter data = new PacketWriter(Opcodes.SMSG_BuyFailed);
            data.WriteUInt64(creature != null ? creature.GetGUID() : 0);
            data.WriteUInt32(item);
            data.WriteUInt8(msg);
            GetSession().Send(data);
        }
        public void SendSellError(SellResult msg, Creature creature, ulong guid)
        {
            Log.outDebug("WORLD: Sent SMSG_SELL_ITEM");
            PacketWriter data = new PacketWriter(Opcodes.SMSG_SellItem);
            data.WriteUInt64(creature != null ? creature.GetGUID() : 0);
            data.WriteUInt32(guid);
            data.WriteUInt8(msg);
            GetSession().Send(data);
        }
        public void SendEquipError(InventoryResult msg, Item pItem, Item pItem2 = null, uint itemid = 0)
        {
            Log.outDebug("WORLD: Sent SMSG_INVENTORY_CHANGE_FAILURE ({0})", msg);
            PacketWriter data = new PacketWriter(Opcodes.SMSG_InventoryChangeFailure);
            data.WriteUInt8(msg);

            if (msg != InventoryResult.Ok)
            {
                data.WriteUInt64(pItem != null ? pItem.GetGUID() : 0);
                data.WriteUInt64(pItem2 != null ? pItem2.GetGUID() : 0);
                data.WriteUInt8(0);                                   // bag type subclass, used with EQUIP_ERR_EVENT_AUTOEQUIP_BIND_CONFIRM and EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG2

                switch (msg)
                {
                    case InventoryResult.CantEquipLevelI:
                    case InventoryResult.PurchaseLevelTooLow:
                        {
                            ItemTemplate proto = pItem != null ? pItem.GetTemplate() : Cypher.ObjMgr.GetItemTemplate(itemid);
                            data.WriteUInt32(proto != null ? proto.RequiredLevel : 0);
                            break;
                        }
                    case InventoryResult.NoOutput:    // no idea about this one...
                        {
                            data.WriteUInt64(0); // item guid
                            data.WriteUInt32(0); // slot
                            data.WriteUInt64(0); // container
                            break;
                        }
                    case InventoryResult.ItemMaxLimitCategoryCountExceededIs:
                    case InventoryResult.ItemMaxLimitCategorySocketedExceededIs:
                    case InventoryResult.ItemMaxLimitCategoryEquippedExceededIs:
                        {
                            ItemTemplate proto = pItem != null ? pItem.GetTemplate() : Cypher.ObjMgr.GetItemTemplate(itemid);
                            data.WriteUInt32(proto != null ? proto.ItemLimitCategory : 0);
                            break;
                        }
                    default:
                        break;
                }
            }
            GetSession().Send(data);
        }
        void SendNewItem(Item item, uint count, bool received, bool created, bool broadcast)
        {
            if (item == null)                                               // prevent crash
                return;

            // last check 2.0.10
            PacketWriter data = new PacketWriter(Opcodes.SMSG_ItemPushResult);
            data.WriteUInt64(GetGUID());                              // player GUID
            data.WriteUInt32(received);                               // 0=looted, 1=from npc
            data.WriteUInt32(created);                                // 0=received, 1=created
            data.WriteUInt32(1);                                      // bool print error to chat
            data.WriteUInt8(item.GetBagSlot());                      // bagslot
            // item slot, but when added to stack: 0xFFFFFFFF
            data.WriteInt32((item.GetStackCount() == count) ? item.GetSlot() : -1);
            data.WriteUInt32(item.GetEntry());                       // item id
            data.WriteUInt32(item.GetItemSuffixFactor());            // SuffixFactor
            data.WriteInt32(item.GetItemRandomPropertyId());         // random item property id
            data.WriteUInt32(count);                                  // count of items
            data.WriteUInt32(GetItemCount(item.GetEntry()));         // count of items in inventory

            //if (broadcast)// && GetGroup()) todo fixme
            //GetGroup()->BroadcastPacket(&data, true);
            //else
            GetSession().Send(data);
        }
        #endregion

        #region Skills
        void LoadSkills()
        {
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_SKILLS);
            stmt.AddValue(0, GetGUIDLow());
            SQLResult result = DB.Characters.Select(stmt);

            //var professionCount = 0;
            var count = 0;
            if (result.Count != 0)
            {
                for (; count < result.Count; count++)
                {
                    var skill = result.Read<ushort>(count, 0);
                    var value = result.Read<ushort>(count, 1);
                    var max = result.Read<ushort>(count, 2);

                    SkillLineEntry pSkill = DBCStorage.SkillLineStorage.LookupByKey(skill);
                    if (pSkill.id == 0)
                    {
                        Log.outError("Character {0} has skill {1} that does not exist.", GetGUIDLow(), skill);
                        continue;
                    }

                    // set fixed skill ranges
                    switch (Cypher.SpellMgr.GetSkillType(pSkill, false))
                    {
                        case SkillType.Language:
                            value = max = 300;
                            break;
                        case SkillType.Mono:
                            value = max = 1;
                            break;
                        default:
                            break;
                    }
                    if (value == 0)
                    {
                        Log.outError("Character {0} has skill {1} with value 0. Will be deleted.", GetGUIDLow(), skill);

                        stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_CHARACTER_SKILL);
                        stmt.AddValue(0, GetGUIDLow());
                        stmt.AddValue(1, skill);
                        DB.Characters.Execute(stmt);
                        continue;
                    }
                    
                    // enable unlearn button for primary professions only
                    if (pSkill.categoryId == (int)SkillCategory.Profession)
                        SetValue<uint>(GetPlayerSkillIndex(count), MakePair32(skill, 1));
                    else
                        SetValue<uint>(GetPlayerSkillIndex(count), MakePair32(skill, 0));
                    
                    SetValue(GetPlayerSkillValueIndex(count), MakeSkillValue(value, max));
                    SetValue(GetPlayerSkillBonusIndex(count), 0);

                    Skills.Add(skill, new SkillStatusData((uint)count, SkillState.Unchanged));

                    //learnSkillRewardedSpells(skill, value);

                    if (count >= SkillConst.MaxPlayerSkills)                      // client limit
                    {
                        Log.outError("Character {0} has more than {1} skills.", GetGUIDLow(), SkillConst.MaxPlayerSkills);
                        break;
                    }
                }
            }
            for (; count < SkillConst.MaxPlayerSkills; count++)
            {
                SetValue<uint>(GetPlayerSkillIndex(count), 0);
                SetValue<uint>(GetPlayerSkillValueIndex(count), 0);
                SetValue<uint>(GetPlayerSkillBonusIndex(count), 0);
            }

            // special settings
            if (GetClass() == Class.Deathknight)
            {
                byte base_level = (byte)Math.Min(getLevel(), 55);
                if (base_level < 1)
                    base_level = 1;
                ushort base_skill = (ushort)((base_level - 1) * 5);
                if (base_skill < 1)
                    base_skill = 1;                                 // skill mast be known and then > 0 in any case

                if (GetPureSkillValue((uint)Skill.FirstAid) < base_skill)
                    SetSkill((uint)Skill.FirstAid, 0, base_skill, base_skill);
                if (GetPureSkillValue((uint)Skill.Axes) < base_skill)
                    SetSkill((uint)Skill.Axes, 0, base_skill, base_skill);
                if (GetPureSkillValue((uint)Skill.Defense) < base_skill)
                    SetSkill((uint)Skill.Defense, 0, base_skill, base_skill);
                if (GetPureSkillValue((uint)Skill.Polearms) < base_skill)
                    SetSkill((uint)Skill.Polearms, 0, base_skill, base_skill);
                if (GetPureSkillValue((uint)Skill.Swords) < base_skill)
                    SetSkill((uint)Skill.Swords, 0, base_skill, base_skill);
                if (GetPureSkillValue((uint)Skill.Axes2h) < base_skill)
                    SetSkill((uint)Skill.Axes2h, 0, base_skill, base_skill);
                if (GetPureSkillValue((uint)Skill.Swords2h) < base_skill)
                    SetSkill((uint)Skill.Swords2h, 0, base_skill, base_skill);
                if (GetPureSkillValue((uint)Skill.Unarmed) < base_skill)
                    SetSkill((uint)Skill.Unarmed, 0, base_skill, base_skill);
            }

        }
        void UpdateSkillsForLevel()
        {
            uint maxSkill = GetMaxSkillValueForLevel();

            foreach (var itr in Skills)
            {
                if (itr.Value.State == SkillState.Deleted)
                    continue;

                uint pskill = itr.Key;
                SkillLineEntry pSkill = DBCStorage.SkillLineStorage.LookupByKey(pskill);
                if (pSkill == null)
                    continue;

                if (Cypher.SpellMgr.GetSkillType(pSkill, false) != SkillType.Level)
                    continue;

                int valueIndex = GetPlayerSkillValueIndex(itr.Value.Pos);
                int data = GetValue<int>(valueIndex);
                uint max = Pair32_HiPart(data);
                uint val = Pair32_LoPart(data);

                /// update only level dependent max skill values
                if (max != 1)
                {
                    SetValue<uint>(valueIndex, MakePair32(val, maxSkill));
                    if (itr.Value.State != SkillState.New)
                        itr.Value.State = SkillState.Changed;
                }
            }
        }
        ushort GetSkillValue(uint skill)
        {
            if (skill == 0)
                return 0;

            var itr = Skills.LookupByKey(skill);
            if (itr == null || itr.State == SkillState.Deleted)
                return 0;

            int bonus = GetValue<int>(GetPlayerSkillBonusIndex(itr.Pos));

            int result = (int)SkillValue(GetValue<uint>(GetPlayerSkillValueIndex(itr.Pos)));
            result += (int)GetSkillTempBonus(bonus);
            result += (int)GetSkillPermBonus(bonus);
            return (ushort)(result < 0 ? 0 : result);
        }
        ushort GetMaxSkillValue(uint skill)
        {
            if (skill == 0)
                return 0;

            var itr = Skills.LookupByKey(skill);
            if (itr == null || itr.State == SkillState.Deleted)
                return 0;

            int bonus = GetValue<int>(GetPlayerSkillBonusIndex(itr.Pos));

            int result = (int)SkillMax(GetValue<uint>(GetPlayerSkillValueIndex(itr.Pos)));
            result += (int)GetSkillTempBonus(bonus);
            result += (int)GetSkillPermBonus(bonus);
            return (ushort)(result < 0 ? 0 : result);
        }
        ushort GetPureSkillValue(uint skill)
        {
            if (skill == 0)
                return 0;

            var itr = Skills.LookupByKey((uint)skill);
            if (itr == null || itr.State == SkillState.Deleted)
                return 0;

            var bonus = GetValue<uint>(GetPlayerSkillBonusIndex(itr.Pos));

            return (ushort)SkillValue(GetValue<uint>(GetPlayerSkillValueIndex(itr.Pos)));
        }
        ushort GetSkillStep(uint skill)
        {
            if (skill == 0)
                return 0;

            var itr = Skills.LookupByKey(skill);
            if (itr == null || itr.State == SkillState.Deleted)
                return 0;

            return (ushort)Pair32_HiPart(GetValue<int>(GetPlayerSkillIndex(itr.Pos)));
        }
        ushort GetPureMaxSkillValue(uint skill)
        {
            if (skill == 0)
                return 0;

            var itr = Skills.LookupByKey(skill);
            if (itr == null || itr.State == SkillState.Deleted)
                return 0;

            return (ushort)SkillMax(GetValue<uint>(GetPlayerSkillValueIndex(itr.Pos)));
        }
        ushort GetBaseSkillValue(uint skill)
        {
            if (skill == 0)
                return 0;

            var itr = Skills.LookupByKey(skill);
            if (itr == null || itr.State == SkillState.Deleted)
                return 0;

            int result = (int)SkillValue(GetValue<uint>(GetPlayerSkillValueIndex(itr.Pos)));
            result += GetSkillPermBonus(GetValue<int>(GetPlayerSkillBonusIndex(itr.Pos)));
            return (ushort)(result < 0 ? 0 : result);
        }
        bool HasSkill(uint skill)
        {
            if (skill == 0)
                return false;

            return Skills.LookupByKey(skill) != null;
        }
        void SetSkill(uint id, uint step, uint newVal, uint maxVal)
        {
            if (id == 0)
                return;

            uint currVal;
            var itr = Skills.LookupByKey(id);

            //has skill
            if (itr != null && itr.State != SkillState.Deleted)
            {
                currVal = SkillValue(GetValue<uint>(GetPlayerSkillValueIndex(itr.Pos)));
                if (newVal != 0)
                {
                    // if skill value is going down, update enchantments before setting the new value
                    //if (newVal < currVal)
                    //UpdateSkillEnchantments(id, currVal, newVal);

                    // update step
                    SetValue<uint>(GetPlayerSkillIndex(itr.Pos), MakePair32(id, step));
                    // update value
                    SetValue<uint>(GetPlayerSkillValueIndex(itr.Pos), MakeSkillValue(newVal, maxVal));

                    if (itr.State != SkillState.New)
                        itr.State = SkillState.Changed;

                    //learnSkillRewardedSpells(id, newVal);
                    // if skill value is going up, update enchantments after setting the new value
                    //if (newVal > currVal)
                    //UpdateSkillEnchantments(id, currVal, newVal);

                    //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_REACH_SKILL_LEVEL, id);
                    //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_LEARN_SKILL_LEVEL, id);
                }
                else                                                //remove
                {
                    //remove enchantments needing this skill
                    //UpdateSkillEnchantments(id, currVal, 0);
                    // clear skill fields
                    SetValue<uint>(GetPlayerSkillIndex(itr.Pos), 0);
                    SetValue<uint>(GetPlayerSkillValueIndex(itr.Pos), 0);
                    SetValue<uint>(GetPlayerSkillBonusIndex(itr.Pos), 0);

                    // mark as deleted or simply remove from map if not saved yet
                    if (itr.State != SkillState.New)
                        itr.State = SkillState.Deleted;
                    else
                        Skills.Remove(id);

                    // remove all spells that related to this skill
                    //var pAbility = DBCStorage.SkillLineAbilityStorage.First(p => p.Value.skillId == id);
                    //if (pAbility.Value.skillId != 0)
                        //removeSpell(sSpellMgr.GetFirstSpellInChain(pAbility.spellId));                        

                    // Clear profession lines
                    if (GetValue<uint>(PlayerFields.ProfessionSkillLine) == id)
                        SetValue<uint>(PlayerFields.ProfessionSkillLine, 0);
                    else if (GetValue<uint>(PlayerFields.ProfessionSkillLine + 1) == id)
                        SetValue<uint>(PlayerFields.ProfessionSkillLine + 1, 0);
                }
            }
            else if (newVal != 0)                                        //add
            {
                currVal = 0;
                for (int i = 0; i < SkillConst.MaxPlayerSkills; ++i)
                {
                    uint blah1 = GetValue<uint>(GetPlayerSkillIndex(i));
                    if (GetValue<uint>(GetPlayerSkillIndex(i)) == 0)
                    {
                        var skillEntry = DBCStorage.SkillLineStorage.LookupByKey(id);
                        if (skillEntry == null)
                        {
                            Log.outError("Skill not found in SkillLineStore: skill #{0}", id);
                            return;
                        }
                        int blah = GetPlayerSkillIndex(i);

                        SetValue<uint>(GetPlayerSkillIndex(i), MakePair32(id, step));
                        SetValue<uint>(GetPlayerSkillValueIndex(i), MakeSkillValue(newVal, maxVal));
                        if (skillEntry.categoryId == (uint)SkillCategory.Profession)
                        {
                            if (GetValue<uint>(PlayerFields.ProfessionSkillLine) == 0)
                                SetValue<uint>(PlayerFields.ProfessionSkillLine, id);
                            else if (GetValue<uint>(PlayerFields.ProfessionSkillLine + 1) == 0)
                                SetValue<uint>(PlayerFields.ProfessionSkillLine + 1, id);
                        }

                        //UpdateSkillEnchantments(id, currVal, newVal);
                        //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_REACH_SKILL_LEVEL, id);
                        //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_LEARN_SKILL_LEVEL, id);

                        // insert new entry or update if not deleted old entry yet
                        if (itr != null)
                        {
                            itr.Pos = (byte)i;
                            itr.State = SkillState.Changed;
                        }
                        else
                            Skills.Add(id, new SkillStatusData((uint)i, SkillState.New));

                        // apply skill bonuses
                        SetValue<uint>(GetPlayerSkillBonusIndex(i), 0);

                        // temporary bonuses
                        //AuraEffectList mModSkill = GetAuraEffectsByType(SPELL_AURA_MOD_SKILL);
                        //for (AuraEffectList::const_iterator j = mModSkill.begin(); j != mModSkill.end(); ++j)
                        //if ((*j).GetMiscValue() == int32(id))
                        //(*j).HandleEffect(this, AURA_EFFECT_HANDLE_SKILL, true);

                        // permanent bonuses
                        //AuraEffectList mModSkillTalent = GetAuraEffectsByType(SPELL_AURA_MOD_SKILL_TALENT);
                        //for (AuraEffectList::const_iterator j = mModSkillTalent.begin(); j != mModSkillTalent.end(); ++j)
                        //if ((*j).GetMiscValue() == int32(id))
                        //(*j).HandleEffect(this, AURA_EFFECT_HANDLE_SKILL, true);

                        // Learn all spells for skill
                        //learnSkillRewardedSpells(id, newVal);
                        return;
                    }
                }
            }
        }
        #endregion

        #region Spells
        void learnDefaultSpells()
        {
            // learn default race/class spells
            var info = Cypher.ObjMgr.GetPlayerInfo(getRace(), getClass());
            foreach (var itr in info.spell)
            {
                uint tspell = itr;
                //Log.outDebug("PLAYER (Class: {0} Race: {1}): Adding initial spell, id = {2}", getClass(), getRace(), tspell);
                if (!IsInWorld)                                    // will send in INITIAL_SPELLS in list anyway at map add
                    AddSpell(tspell, true, true, true, false);
                else                                                // but send in normal spell in game learn case
                    LearnSpell(tspell, true);
            }
        }
        void LoadSpells()
        {
            Spells.Clear();
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_CHARACTER_SPELL);
            stmt.AddValue(0, GetGUIDLow());
            SQLResult result = DB.Characters.Select(stmt);

            if (result.Count == 0)
                return;

            for (int i = 0; i < result.Count; i++)
            {
                bool active = result.Read<bool>(i, "active");
                bool disabled = result.Read<bool>(i, "disabled");
                AddSpell(result.Read<uint>(i, "spellId"), active, false, false, disabled);
            }
        }
        public void LearnSpell(uint spellId, bool dependent)
        {
            PlayerSpell spell = Spells.Find(p => p.SpellId == spellId);

            bool disabled = (spell != null) ? spell.Disabled : false;
            bool active = disabled ? spell.Active : true;

            bool learning = AddSpell(spellId, active, true, dependent, false);

            if (learning && IsInWorld)
            {
                PacketWriter data = new PacketWriter(Opcodes.SMSG_LearnedSpells);
                data.WriteUInt32(spellId);
                data.WriteUInt32(0);
                GetSession().Send(data);
            }

            // learn all disabled higher ranks and required spells (recursive)
            if (disabled)
            {
                uint nextSpell = Cypher.SpellMgr.GetNextSpellInChain(spellId);
                if (nextSpell != 0)
                {
                    var _spell = Spells.Find(p => p.SpellId == nextSpell);
                    if (_spell.SpellId != 0 && _spell.Disabled)
                        LearnSpell(nextSpell, false);
                }

                //SpellsRequiringSpellMapBounds spellsRequiringSpell = sSpellMgr.GetSpellsRequiringSpellBounds(spell_id);
                //for (SpellsRequiringSpellMap::const_iterator itr2 = spellsRequiringSpell.first; itr2 != spellsRequiringSpell.second; ++itr2)
                {
                    //PlayerSpellMap::iterator iter2 = m_spells.find(itr2.second);
                    //if (iter2 != m_spells.end() && iter2.second.disabled)
                    //LearnSpell(itr2.second, false);
                }
            }

        }
        bool IsNeedCastPassiveSpellAtLearn(SpellInfo spellInfo)
        {
            // note: form passives activated with shapeshift spells be implemented by HandleShapeshiftBoosts instead of spell_learn_spell
            // talent dependent passives activated at form apply have proper stance data
            ShapeshiftForm form = GetShapeshiftForm();
            bool need_cast = (spellInfo.Stances == 0 || (form != ShapeshiftForm.None && Convert.ToBoolean(spellInfo.Stances & (1 << ((int)form - 1)))) ||
            (form == 0 && Convert.ToBoolean(spellInfo.AttributesEx2 & SpellAttr2.NotNeedShapeshift)));

            //Check CasterAuraStates
            return need_cast && (spellInfo.CasterAuraState == 0 || HasAuraState((AuraStateType)spellInfo.CasterAuraState));
        }
        bool AddSpell(uint spellId, bool active, bool learning, bool dependent, bool disabled, bool loading = false)
        {
            SpellInfo spellInfo = Cypher.SpellMgr.GetSpellInfo(spellId);
            if (spellInfo == null)
            {
                if (!IsInWorld && !learning)
                {
                    Log.outError("Player::addSpell: Non-existed in SpellStore spell #{0} request, deleting for all characters in `character_spell`.", spellId);

                    PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_INVALID_SPELL);
                    stmt.AddValue(0, spellId);
                    DB.Characters.Execute(stmt);
                }
                else
                    Log.outError("Player::addSpell: Non-existed in SpellStore spell #{0} request.", spellId);

                return false;
            }

            if (!Cypher.SpellMgr.IsSpellValid(spellInfo, this, false))
            {
                if (!IsInWorld && !learning)
                {
                    Log.outError("Player::addSpell: Broken spell #%u learning not allowed, deleting for all characters in `character_spell`.", spellId);

                    PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_INVALID_SPELL);
                    stmt.AddValue(0, spellId);
                    DB.Characters.Execute(stmt);
                }
                else
                    Log.outError("Player::addSpell: Broken spell #%u learning not allowed.", spellId);

                return false;
            }
            PlayerSpellState state = learning ? PlayerSpellState.New : PlayerSpellState.Unchanged;

            bool dependent_set = false;
            bool disabled_case = false;
            bool superceded_old = false;

            PlayerSpell itr = Spells.Find(p => p.SpellId == spellId);

            //if (itr.SpellId != 0 && itr.State == PlayerSpellState.Temporary)
            //RemoveTemporarySpell(spellId);
            if (itr != null)
            {
                uint next_active_spell_id = 0;
                // fix activate state for non-stackable low rank (and find next spell for !active case)
                if (!spellInfo.IsStackableWithRanks() && spellInfo.IsRanked())
                {
                    uint next = spellInfo.ChainEntry.next;
                    if (next != 0)
                    {
                        if (HasSpell(next))
                        {
                            // high rank already known so this must !active
                            active = false;
                            next_active_spell_id = next;
                        }
                    }
                }

                // not do anything if already known in expected state
                if (itr.State != PlayerSpellState.Removed && itr.Active == active &&
                    itr.Dependent == dependent && itr.Disabled == disabled)
                {
                    if (!IsInWorld && !learning)
                        itr.State = PlayerSpellState.Unchanged;
                    return false;
                }

                // dependent spell known as not dependent, overwrite state
                if (itr.State != PlayerSpellState.Removed && !itr.Dependent && dependent)
                {
                    itr.Dependent = dependent;
                    if (itr.State != PlayerSpellState.New)
                        itr.State = PlayerSpellState.Changed;
                    dependent_set = true;
                }

                // update active state for known spell
                if (itr.Active != active && itr.State != PlayerSpellState.Removed && !itr.Disabled)
                {
                    itr.Active = active;

                    if (!IsInWorld && !learning && !dependent_set) // explicitly load from DB and then exist in it already and set correctly
                        itr.State = PlayerSpellState.Unchanged;
                    else if (itr.State != PlayerSpellState.New)
                        itr.State = PlayerSpellState.Changed;

                    if (active)
                    {
                        //if (spellInfo.IsPassive() && IsNeedCastPassiveSpellAtLearn(spellInfo))
                        //CastSpell (this, spellId, true);
                    }
                    else if (IsInWorld)
                    {
                        if (next_active_spell_id != 0)
                        {
                            // update spell ranks in spellbook and action bar
                            PacketWriter data = new PacketWriter(Opcodes.SMSG_SupercededSpell);
                            data.WriteUInt32(spellId);
                            data.WriteUInt32(next_active_spell_id);
                            GetSession().Send(data);
                        }
                        else
                        {
                            PacketWriter data = new PacketWriter(Opcodes.SMSG_RemovedSpell);
                            data.WriteUInt32(spellId);
                            GetSession().Send(data);
                        }
                    }
                    return active;
                }

                if (itr.Disabled != disabled && itr.State != PlayerSpellState.Removed)
                {
                    if (itr.State != PlayerSpellState.New)
                        itr.State = PlayerSpellState.Changed;
                    itr.Disabled = disabled;

                    if (disabled)
                        return false;

                    disabled_case = true;
                }
                else switch (itr.State)
                    {
                        case PlayerSpellState.Unchanged:
                            return false;
                        case PlayerSpellState.Removed:
                            {
                                Spells.Remove(itr);
                                state = PlayerSpellState.Changed;
                                break;
                            }
                        default:
                            {
                                // can be in case spell loading but learned at some previous spell loading
                                if (!IsInWorld && !learning && !dependent_set)
                                    itr.State = PlayerSpellState.Unchanged;
                                return false;
                            }
                    }
            }

            if (!disabled_case) // skip new spell adding if spell already known (disabled spells case)
            {
                /*
                // talent: unlearn all other talent ranks (high and low)
                if (TalentSpellPos talentPos = GetTalentSpellPos(spellId))
                {
                    if (TalentEntry talentInfo = sTalentStore.LookupEntry(talentPos.talent_id))
                    {
                        for (uint8 rank = 0; rank < MAX_TALENT_RANK; ++rank)
                        {
                            // skip learning spell and no rank spell case
                            uint32 rankSpellId = talentInfo.RankID[rank];
                            if (!rankSpellId || rankSpellId == spellId)
                                continue;

                            removeSpell(rankSpellId, false, false);
                        }
                    }
                }
        
                // non talent spell: learn low ranks (recursive call)
                else*/
                if (spellInfo.ChainEntry != null)//  uint prev_spell = )
                {
                    uint prev_spell = spellInfo.ChainEntry.prev;//sSpellMgr.GetPrevSpellInChain(spellId)
                    if (!IsInWorld || disabled)                    // at spells loading, no output, but allow save
                        AddSpell(spellInfo.ChainEntry.prev, active, true, true, disabled);
                    else                                            // at normal learning
                        LearnSpell(prev_spell, true);
                }

                PlayerSpell newspell = new PlayerSpell();
                newspell.State = state;
                newspell.Active = active;
                newspell.Dependent = dependent;
                newspell.Disabled = disabled;
                newspell.SpellId = spellId;

                // replace spells in action bars and spellbook to bigger rank if only one spell rank must be accessible
                if (newspell.Active && !newspell.Disabled && !spellInfo.IsStackableWithRanks() && spellInfo.IsRanked())
                {
                    foreach (var spell in Spells)
                    //for (PlayerSpellMap::iterator itr2 = m_spells.begin(); itr2 != m_spells.end(); ++itr2)
                    {
                        if (spell.State == PlayerSpellState.Removed)
                            continue;

                        SpellInfo i_spellInfo = Cypher.SpellMgr.GetSpellInfo(spell.SpellId);
                        if (i_spellInfo == null)
                            continue;

                        if (spellInfo.IsDifferentRankOf(i_spellInfo))
                        {
                            if (spell.Active)
                            {
                                if (spellInfo.IsHighRankOf(i_spellInfo))
                                {
                                    if (IsInWorld)                 // not send spell (re-/over-)learn packets at loading
                                    {
                                        PacketWriter data = new PacketWriter(Opcodes.SMSG_SupercededSpell);
                                        data.WriteUInt32(spell.SpellId);
                                        data.WriteUInt32(spellId);
                                        GetSession().Send(data);
                                    }

                                    // mark old spell as disable (SMSG_SUPERCEDED_SPELL replace it in client by new)
                                    spell.Active = false;
                                    if (spell.State != PlayerSpellState.New)
                                        spell.State = PlayerSpellState.Changed;
                                    superceded_old = true;          // new spell replace old in action bars and spell book.
                                }
                                else
                                {
                                    if (IsInWorld)                 // not send spell (re-/over-)learn packets at loading
                                    {
                                        PacketWriter data = new PacketWriter(Opcodes.SMSG_SupercededSpell);
                                        data.WriteUInt32(spellId);
                                        data.WriteUInt32(spell.SpellId);
                                        GetSession().Send(data);
                                    }

                                    // mark new spell as disable (not learned yet for client and will not learned)
                                    newspell.Active = false;
                                    if (newspell.State != PlayerSpellState.New)
                                        newspell.State = PlayerSpellState.Changed;
                                }
                            }
                        }
                    }
                }
                Spells.Add(newspell);

                // return false if spell disabled
                if (newspell.Disabled)
                    return false;
            }

            uint talentCost = 0;//GetTalentSpellCost(spellId);

            // cast talents with SPELL_EFFECT_LEARN_SPELL (other dependent spells will learned later as not auto-learned)
            // note: all spells with SPELL_EFFECT_LEARN_SPELL isn't passive
            if (!loading && talentCost > 0 && spellInfo.HasEffect(SpellEffects.LearnSpell))
            {
                // ignore stance requirement for talent learn spell (stance set for spell only for client spell description show)
                //CastSpell(this, spellId, true);
            }
            // also cast passive spells (including all talents without SPELL_EFFECT_LEARN_SPELL) with additional checks
            else if (spellInfo.IsPassive())
            {
                //if (IsNeedCastPassiveSpellAtLearn(spellInfo))
                //CastSpell(this, spellId, true);
            }
            else if (spellInfo.HasEffect(SpellEffects.SkillStep))
            {
                //CastSpell(this, spellId, true);
                return false;
            }

            // update used talent points count
            //SetUsedTalentCount(GetUsedTalentCount() + talentCost);

            // update free primary prof.points (if any, can be none in case GM .learn prof. learning)
            uint freeProfs = GetFreePrimaryProfessionPoints();
            if (freeProfs != 0)
            {
                if (spellInfo.IsPrimaryProfessionFirstRank())
                    SetFreePrimaryProfessions(freeProfs - 1);
            }

            // add dependent skills
            uint maxskill = GetMaxSkillValueForLevel();

            SpellLearnSkillNode spellLearnSkill = Cypher.SpellMgr.GetSpellLearnSkill(spellId);

            var skill_bounds = Cypher.SpellMgr.GetSkillLineAbility(spellId);

            if (spellLearnSkill != null)
            {
                uint skill_value = GetPureSkillValue(spellLearnSkill.skill);
                uint skill_max_value = GetPureMaxSkillValue(spellLearnSkill.skill);

                if (skill_value < spellLearnSkill.value)
                    skill_value = spellLearnSkill.value;

                uint new_skill_max_value = spellLearnSkill.maxvalue == 0 ? maxskill : spellLearnSkill.maxvalue;

                if (skill_max_value < new_skill_max_value)
                    skill_max_value = new_skill_max_value;

                SetSkill(spellLearnSkill.skill, spellLearnSkill.step, skill_value, skill_max_value);
            }
            else
            {
                // not ranked skills
                foreach (var skill in skill_bounds)
                {
                    var pSkill = DBCStorage.SkillLineStorage.LookupByKey(skill.skillId);
                    if (pSkill.id == 0)
                        continue;

                    if (HasSkill(pSkill.id))
                        continue;

                    if (skill.learnOnGetSkill == 2 || ((pSkill.id == (uint)Skill.Lockpicking ||
                        pSkill.id == (uint)Skill.Runeforging) && skill.max_value == 0))
                    {
                        switch (Cypher.SpellMgr.GetSkillType(pSkill, skill.racemask != 0))
                        {
                            case SkillType.Language:
                                SetSkill(pSkill.id, GetSkillStep(pSkill.id), 300, 300);
                                break;
                            case SkillType.Level:
                                SetSkill(pSkill.id, GetSkillStep(pSkill.id), 1, GetMaxSkillValueForLevel());
                                break;
                            case SkillType.Mono:
                                SetSkill(pSkill.id, GetSkillStep(pSkill.id), 1, 1);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // learn dependent spells
            var spell_bounds = Cypher.SpellMgr.GetSpellLearnSpellMapBounds(spellId);

            foreach (var itr2 in spell_bounds)
            {
                if (!itr2.autoLearned)
                {
                    if (!IsInWorld || !itr2.active)       // at spells loading, no output, but allow save
                        AddSpell(itr2.spell, itr2.active, true, true, false);
                    else                                            // at normal learning
                        LearnSpell(itr2.spell, true);
                }
            }

            //if (!GetSession().PlayerLoading())
            {
                // not ranked skills
                //for (SkillLineAbilityMap::const_iterator _spell_idx = skill_bounds.first; _spell_idx != skill_bounds.second; ++_spell_idx)
                {
                    //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_LEARN_SKILL_LINE, _spell_idx.second.skillId);
                    //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_LEARN_SKILLLINE_SPELLS, _spell_idx.second.skillId);
                }

                //UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_LEARN_SPELL, spellId);
            }
            // return true (for send learn packet) only if spell active (in case ranked spells) and not replace old spell
            return active && !disabled && !superceded_old;
        }
        public bool HasSpell(uint spellId)
        {
            if (Spells.Any(p => p.SpellId == spellId))
            {
                var spell = Spells.Find(s => s.SpellId == spellId);
                return (spell.State != PlayerSpellState.Removed && !spell.Disabled);
            }
            return false;
        }
        #endregion

        #region Chat
        void BuildPlayerChat(ref PacketWriter data, ChatMsg msgtype, string text, Language language, string addonPrefix = null)
        {
            data.WriteUInt8(msgtype);
            data.WriteUInt32(language);
            data.WriteUInt64(GetGUID());
            data.WriteUInt32(0);                                      // constant unknown time
            if (addonPrefix != null)
                data.WriteString(addonPrefix);
            else
                data.WriteUInt64(GetGUID());
            data.WriteUInt32(text.Length + 1);
            data.WriteCString(text);
            data.WriteUInt16(0);//GetChatTag());
        }
        public void Say(string text, Language language)
        {
            //sScriptMgr->OnPlayerChat(this, CHAT_MSG_SAY, language, _text);
            PacketWriter data = new PacketWriter(Opcodes.SMSG_MessageChat);
            BuildPlayerChat(ref data, ChatMsg.Say, text, language);
            SendMessageToSetInRange(data, 5.0f, true);//sWorld->getFloatConfig(CONFIG_LISTEN_RANGE_SAY), true); todo fixme
        }
        public void Yell(string text, Language language)
        {
            //sScriptMgr->OnPlayerChat(this, CHAT_MSG_YELL, language, _text);
            PacketWriter data = new PacketWriter(Opcodes.SMSG_MessageChat);
            BuildPlayerChat(ref data, ChatMsg.Yell, text, language);
            SendMessageToSetInRange(data, 0.0f, true);//sWorld->getFloatConfig(CONFIG_LISTEN_RANGE_YELL), true); //todo fix me
        }
        public void Whisper(string text, Language language, ulong receiver)
        {
            Player rPlayer = Cypher.ObjMgr.FindPlayer(receiver);

            //sScriptMgr->OnPlayerChat(this, CHAT_MSG_WHISPER, language, _text, rPlayer);

            // when player you are whispering to is dnd, he cannot receive your message, unless you are in gm mode
            //if (!rPlayer.isDND() || isGameMaster())
            {
                PacketWriter data = new PacketWriter(Opcodes.SMSG_MessageChat);
                BuildPlayerChat(ref data, ChatMsg.Whisper, text, language);
                rPlayer.GetSession().Send(data);

                data = new PacketWriter(Opcodes.SMSG_MessageChat);
                rPlayer.BuildPlayerChat(ref data, ChatMsg.Whisper_Inform, text, language);
                GetSession().Send(data);
            }
            //else // announce to player that player he is whispering to is dnd and cannot receive his message
            //ChatHandler(this).PSendSysMessage(LANG_PLAYER_DND, rPlayer->GetName(), rPlayer->dndMsg.c_str());

            //if (!isAcceptWhispers() && !isGameMaster() && !rPlayer->isGameMaster())
            {
                //SetAcceptWhispers(true);
                //ChatHandler(this).SendSysMessage(LANG_COMMAND_WHISPERON);
            }

            // announce to player that player he is whispering to is afk
            //if (rPlayer->isAFK())
            //ChatHandler(this).PSendSysMessage(LANG_PLAYER_AFK, rPlayer->GetName(), rPlayer->afkMsg.c_str());

            // if player whisper someone, auto turn of dnd to be able to receive an answer
            //if (isDND() && !rPlayer->isGameMaster())
                //ToggleDND();
        }
        #endregion

        //new
        public bool IsVisibleGloballyFor(Player u)
        {
            if (u == null)
                return false;

            // Always can see self
            if (u == this)
                return true;

            // Visible units, always are visible for all players
            //if (IsVisible())
               // return true;

            // GMs are visible for higher gms (or players are visible for gms)
            if (!Cypher.ObjMgr.IsPlayerAccount(u.GetSession().GetSecurity()))
                return GetSession().GetSecurity() <= u.GetSession().GetSecurity();

            // non faction visibility non-breakable for non-GMs
            //if (!IsVisible())
               // return false;

            // non-gm stealth/invisibility not hide from global player lists
            return true;
        }
        public bool IsValidPos(byte bag, byte slot, bool explicit_pos)
        {
            // post selected
            if (bag == ItemConst.NullBag && !explicit_pos)
                return true;

            if (bag == InventorySlots.Bag0)
            {
                // any post selected
                if (slot == ItemConst.NullSlot && !explicit_pos)
                    return true;

                // equipment
                if (slot < (byte)EquipmentSlot.End)
                    return true;

                // bag equip slots
                if (slot >= InventorySlots.BagStart && slot < InventorySlots.BagEnd)
                    return true;

                // backpack slots
                if (slot >= InventorySlots.ItemStart && slot < InventorySlots.ItemEnd)
                    return true;

                // bank main slots
                if (slot >= InventorySlots.BankItemStart && slot < InventorySlots.BankItemEnd)
                    return true;

                // bank bag slots
                if (slot >= InventorySlots.BankBagStart && slot < InventorySlots.BankBagEnd)
                    return true;

                return false;
            }

            // bag content slots
            // bank bag content slots
            Bag pBag = GetBagByPos(bag);
            if (pBag != null)
            {
                // any post selected
                if (slot == ItemConst.NullSlot && !explicit_pos)
                    return true;

                return slot < pBag.GetBagSize();
            }

            // where this?
            return false;
        }
        public float GetReputationPriceDiscount(Creature creature)
        {
            FactionTemplateEntry vendor_faction = creature.getFactionTemplateEntry();
            if (vendor_faction == null || vendor_faction.faction == 0)
                return 1.0f;

            ReputationRank rank = GetReputationRank(vendor_faction.faction);
            if (rank <= ReputationRank.Neutral)
                return 1.0f;

            return 1.0f - 0.05f * (rank - ReputationRank.Neutral);
        }
        public bool IsSpellFitByClassAndRace(uint spell_id)
        {
            uint racemask = getRaceMask();
            uint classmask = getClassMask();

            var bounds = Cypher.SpellMgr.GetSkillLineAbility(spell_id);
            
            //if (bounds.first == bounds.second)
                //return true;

            foreach (var _spell_idx in bounds)
            {
                // skip wrong race skills
                if (_spell_idx.racemask != 0 && (_spell_idx.racemask & racemask) == 0)
                    continue;

                // skip wrong class skills
                if (_spell_idx.classmask != 0 && (_spell_idx.classmask & classmask) == 0)
                    continue;

                return true;
            }

            return false;
        }
        //New shit
        void InitPrimaryProfessions()
        {
            SetFreePrimaryProfessions(2);//sWorld.getIntConfig(CONFIG_MAX_PRIMARY_TRADE_SKILL));
        }
        public uint GetFreePrimaryProfessionPoints() { return GetValue<uint>(PlayerFields.CharacterPoints); }
        void SetFreePrimaryProfessions(ushort profs) { SetValue<uint>(PlayerFields.CharacterPoints, profs); }
        public bool HaveAtClient(WorldObject u) { return u == this || m_clientGUIDs.FirstOrDefault(p => p == u.GetGUID()) != 0; }
        //public void RelocateToHomebind() {Position.MapId = m_homebindMapId; Position.InstanceId = 0; Position.Relocate(m_homebindX, m_homebindY, m_homebindZ); }
        bool HasTitle(int bitIndex)
        {
            if (bitIndex > (int)PlayerTitle.Max)
                return false;

            int fieldIndexOffset = bitIndex / 32;
            uint flag = (uint)(1 << (bitIndex % 32));
            return HasFlag(PlayerFields.KnownTitles + fieldIndexOffset, flag);
        }
        public bool IsInSameGroupWith(Player p)
        {
            return p == this;// || (GetGroup() != NULL &&
                //GetGroup() == p.GetGroup() &&
                //GetGroup().SameSubGroup(this, p));
        }

        void UpdateArea(uint newArea)
        {
            // FFA_PVP flags are area and not zone id dependent
            // so apply them accordingly
            m_areaUpdateId = newArea;

            AreaTableEntry area = Cypher.ObjMgr.GetAreaEntryByAreaID(newArea);
            //pvpInfo.inFFAPvPArea = area && (area->flags & AREA_FLAG_ARENA);
            //UpdatePvPState(true);

            //UpdateAreaDependentAuras(newArea);

            // previously this was in UpdateZone (but after UpdateArea) so nothing will break
            //pvpInfo.inNoPvPArea = false;
            if (area != null && area.IsSanctuary())    // in sanctuary
            {
                SetFlag<byte>(UnitFields.Bytes2, (byte)UnitPVPStateFlags.Sanctuary, 1);
                //pvpInfo.inNoPvPArea = true;
                //CombatStopWithPets();
            }
            //else
                //RemoveFlag<byte>(UnitFields.Bytes2, (byte)UnitPVPStateFlags.Sanctuary, 1);
        }
        public void UpdateZone(uint newZone, uint newArea)
        {
            if (m_zoneUpdateId != newZone)
            {
                //sOutdoorPvPMgr->HandlePlayerLeaveZone(this, m_zoneUpdateId);
                //sOutdoorPvPMgr->HandlePlayerEnterZone(this, newZone);
                //sBattlefieldMgr->HandlePlayerLeaveZone(this, m_zoneUpdateId);
                //sBattlefieldMgr->HandlePlayerEnterZone(this, newZone);
                //SendInitWorldStates(newZone, newArea);              // only if really enters to new zone, not just area change, works strange...
            }

            // group update
            //if (Group group = GetGroup())
            {
                // SetGroupUpdateFlag(GROUP_UPDATE_FULL);
                //if (GetSession() && group->isLFGGroup() && sLFGMgr->IsTeleported(GetGUID()))
                //for (GroupReference* itr = group->GetFirstMember(); itr != NULL; itr = itr->next())
                //if (Player* member = itr->getSource())
                //GetSession()->SendNameQueryOpcode(member->GetGUID());
            }

            m_zoneUpdateId = newZone;
            //m_zoneUpdateTimer = ZONE_UPDATE_INTERVAL;

            // zone changed, so area changed as well, update it
            UpdateArea(newArea);

            AreaTableEntry zone = Cypher.ObjMgr.GetAreaEntryByAreaID(newZone);
            if (zone == null)
                return;
            /*
    if (sWorld->getBoolConfig(CONFIG_WEATHER) && !HasAuraType(SPELL_AURA_FORCE_WEATHER))
    {
        if (Weather* weather = WeatherMgr::FindWeather(zone->ID))
            weather->SendWeatherUpdateToPlayer(this);
        else
        {
            if (!WeatherMgr::AddWeather(zone->ID))
            {
                // send fine weather packet to remove old zone's weather
                WeatherMgr::SendFineWeatherUpdateToPlayer(this);
            }
        }
    }


    //sScriptMgr->OnPlayerUpdateZone(this, newZone, newArea);

    // in PvP, any not controlled zone (except zone->team == 6, default case)
    // in PvE, only opposition team capital
    switch (zone.team)
    {
        case AREATEAM_ALLY:
            pvpInfo.inHostileArea = GetTeam() != Team.Alliance && (sWorld.IsPvPRealm() || zone.flags & AreaFlags.Capital);
            break;
        case AREATEAM_HORDE:
            pvpInfo.inHostileArea = GetTeam() != HORDE && (sWorld.IsPvPRealm() || zone.flags & AREA_FLAG_CAPITAL);
            break;
        case AREATEAM_NONE:
            // overwrite for battlegrounds, maybe batter some zone flags but current known not 100% fit to this
            pvpInfo.inHostileArea = sWorld->IsPvPRealm() || InBattleground() || zone->flags & AREA_FLAG_WINTERGRASP;
            break;
        default:                                            // 6 in fact
            pvpInfo.inHostileArea = false;
            break;
    }

    if (zone.flags & AREA_FLAG_CAPITAL)                     // Is in a capital city
    {
        if (!pvpInfo.inHostileArea || zone->IsSanctuary())
        {
            SetFlag(PLAYER_FLAGS, PLAYER_FLAGS_RESTING);
            SetRestType(REST_TYPE_IN_CITY);
            InnEnter(time(0), GetMapId(), 0, 0, 0);
        }
        pvpInfo.inNoPvPArea = true;
    }
    else
    {
        if (HasFlag(PlayerFields.PlayerFlags, PlayerFlags.Resting))
        {
            if (GetRestType() == REST_TYPE_IN_TAVERN)        // Still inside a tavern or has recently left
            {
                // Remove rest state if we have recently left a tavern.
                if (GetMapId() != GetInnPosMapId() || GetExactDist(GetInnPosX(), GetInnPosY(), GetInnPosZ()) > 1.0f)
                {
                    RemoveFlag(PLAYER_FLAGS, PLAYER_FLAGS_RESTING);
                    SetRestType(REST_TYPE_NO);
                }
            }
            else                                             // Recently left a capital city
            {
                RemoveFlag(PLAYER_FLAGS, PLAYER_FLAGS_RESTING);
                SetRestType(REST_TYPE_NO);
            }
        }
    }
*/
            //UpdatePvPState();

            // remove items with area/map limitations (delete only for alive player to allow back in ghost mode)
            // if player resurrected at teleport this will be applied in resurrect code
            //if (isAlive())
            //DestroyZoneLimitedItem(true, newZone);

            // check some item equip limitations (in result lost CanTitanGrip at talent reset, for example)
            AutoUnequipOffhandIfNeed();

            // recent client version not send leave/join channel packets for built-in local channels
            //UpdateLocalChannels(newZone);

            //UpdateZoneDependentAuras(newZone);
        }
        
        //OldShit soon to be removed
        public void SetCharacterFields()
        {
            //must have homeplayerrealm
            SetValue<UInt32>(PlayerFields.HomePlayerRealm, WorldConfig.RealmId);

            SetValue<Int32>(UnitFields.AttackPowerModPos, 0);
            SetValue<Int32>(UnitFields.AttackPowerModNeg, 0);
            SetValue<Int32>(UnitFields.RangedAttackPowerModPos, 0);
            SetValue<Int32>(UnitFields.RangedAttackPowerModNeg, 0);
            SetValue<Int32>(UnitFields.PowerRegenFlatModifier, 0);
            SetValue<Int32>(UnitFields.PowerRegenInterruptedFlatModifier, 0);
            SetValue<byte>(PlayerFields.Bytes3, 0, 2);
            SetValue<Byte>(UnitFields.Bytes1, 0, 0);
            SetValue<Byte>(UnitFields.Bytes1, 0, 1);
            SetValue<Byte>(UnitFields.Bytes1, 0, 2);
            SetValue<Byte>(UnitFields.Bytes1, 0, 3);
            SetValue<Single>(UnitFields.MaxHealthModifier, 1);
            //SetValue<UInt64>(UnitFields.Charm, 0);
            //SetValue<UInt64>(UnitFields.Summon, 0);
            //SetValue<UInt64>(UnitFields.Critter, 0);
            //SetValue<UInt64>(UnitFields.CharmedBy, 0);
            //SetValue<UInt64>(UnitFields.SummonedBy, 0);
            //SetValue<UInt64>(UnitFields.CreatedBy, 0);
            //SetValue<UInt64>(UnitFields.Target, 0);
        }

        #region Fields        
        public List<UInt64> m_clientGUIDs;
        public WorldObject m_seer;
        
        Team team;
        Dictionary<uint, PlayerCurrency> CurrencyStorage;
        DeathState deathState;
        ReputationMgr reputationMgr;
        public List<PlayerSpell> Spells = new List<PlayerSpell>();
        public Dictionary<uint, SkillStatusData> Skills = new Dictionary<uint, SkillStatusData>();
        public Item[] Items = new Item[(int)PlayerSlots.Count];
        public ulong CurSelection { get; set; }
        public AtLoginFlags atLoginFlags { get; set; }
        // Homebind coordinates
        uint m_homebindMapId;
        uint m_homebindZoneId;
        float m_homebindX;
        float m_homebindY;
        float m_homebindZ;
        public bool m_itemUpdateQueueBlocked { get; set; }
        public List<Item> ItemUpdateQueue = new List<Item>();
        PlayerExtraFlags m_ExtraFlags;
        uint m_zoneUpdateId;
        uint m_areaUpdateId;
        public List<WorldObject> knownObjects = new List<WorldObject>();

        //Todo  remove me
        public ObjectGuid GuildGuid { get; set; }
        public ulong GuildInvited { get; set; }

        #endregion
    }

    public class PlayerInfo
    {
        public uint MapId;
        public uint ZoneId;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float Orientation;

        public uint DisplayId_m;
        public uint DisplayId_f;

        public List<PlayerCreateInfoItem> item = new List<PlayerCreateInfoItem>();
        public List<uint> spell = new List<uint>();
        public List<PlayerCreateInfoAction> action = new List<PlayerCreateInfoAction>();

        public PlayerLevelInfo[] levelInfo = new PlayerLevelInfo[WorldConfig.MaxLevel];                             //[level-1] 0..MaxPlayerLevel-1
    }

    public class PlayerCreateInfoItem
    {
        public PlayerCreateInfoItem(uint id, uint amount)
        {
            item_id = id;
            item_amount = amount;
        }

        public uint item_id;
        public uint item_amount;
    }

    public class PlayerCreateInfoAction
    {
        public PlayerCreateInfoAction() : this(0, 0, 0) { }
        public PlayerCreateInfoAction(byte _button, uint _action, byte _type)
        {
            button = _button;
            type = _type;
            action = _action;
        }

        byte button;
        byte type;
        uint action;
    }

    public class PlayerLevelInfo
    {
        public byte[] stats = new byte[(int)Stats.Max];
    }

    public struct PlayerCurrency
    {
        public PlayerCurrencyState state;
        public uint totalCount;
        public uint weekCount;
    }

    public class PlayerTaxi
    {
        //PlayerTaxi();
        ~PlayerTaxi() { }
        // Nodes
        //void InitTaxiNodesForLevel(uint32 race, uint32 chrClass, uint8 level);
        //void LoadTaxiMask(const char* data);

        //bool IsTaximaskNodeKnown(uint nodeidx)
        //{
        //byte  field   = uint8((nodeidx - 1) / 8);
        //uint submask = 1 << ((nodeidx-1) % 8);
        //return (m_taximask[field] & submask) == submask;
        //}
        //bool SetTaximaskNode(uint nodeidx)
        //{
        //byte  field   = uint8((nodeidx - 1) / 8);
        //uint submask = 1 << ((nodeidx-1) % 8);
        //if ((m_taximask[field] & submask) != submask)
        //{
        //m_taximask[field] |= submask;
        //return true;
        //}
        // else
        //return false;
        // }
        //void AppendTaximaskTo(ByteBuffer& data, bool all);

        // Destinations
        //bool LoadTaxiDestinationsFromString(const std::string& values, uint32 team);
        //std::string SaveTaxiDestinationsToString();

        //void ClearTaxiDestinations() { m_TaxiDestinations.clear(); }
        //void AddTaxiDestination(uint dest) { m_TaxiDestinations.push_back(dest); }
        //uint GetTaxiSource() { return m_TaxiDestinations.empty() ? 0 : m_TaxiDestinations.front(); }
        //uint GetTaxiDestination() { return m_TaxiDestinations.size() < 2 ? 0 : m_TaxiDestinations[1]; }
        //uint GetCurrentTaxiPath() const;
        //uint NextTaxiDestination()
        // {
        //m_TaxiDestinations.pop_front();
        //return GetTaxiDestination();
        // }
        //bool empty() { return m_TaxiDestinations.empty(); }

        //friend std::ostringstream& operator<< (std::ostringstream& ss, PlayerTaxi const& taxi);

        //TaxiMask m_taximask;
        //std::deque<uint32> m_TaxiDestinations;
    }

    public class PlayerSpell
    {
        public uint SpellId;
        public PlayerSpellState State;
        public bool Active;
        public bool Dependent;
        public bool Disabled;
    }
}
