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
using Framework.Database;
using Framework.DataStorage;
using Framework.Logging;
using Framework.Network;
using Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldServer.Game.WorldEntities;

namespace WorldServer.Game.Managers
{
    public class ReputationMgr
    {
        public ReputationMgr(Player owner)
        {
            _player = owner;
            _visibleFactionCount = 0;
            _honoredFactionCount = 0;
            _reveredFactionCount = 0;
            _exaltedFactionCount = 0;
            _sendFactionIncreased = false;
        }

        void Initialize()
        {
            _factions.Clear();
            _visibleFactionCount = 0;
            _honoredFactionCount = 0;
            _reveredFactionCount = 0;
            _exaltedFactionCount = 0;
            _sendFactionIncreased = false;

            foreach (var factionEntry in DBCStorage.FactionStorage.Values)
            {
                if (factionEntry.reputationListID >= 0)
                {
                    FactionState newFaction = new FactionState();
                    newFaction.ID = factionEntry.ID;
                    newFaction.ReputationListID = (uint)factionEntry.reputationListID;
                    newFaction.Standing = 0;
                    newFaction.Flags = (FactionFlags)GetDefaultStateFlags(factionEntry);
                    newFaction.needSend = true;
                    newFaction.needSave = true;

                    if (Convert.ToBoolean(newFaction.Flags & FactionFlags.Visible))
                        ++_visibleFactionCount;

                    UpdateRankCounters(ReputationRank.Hostile, GetBaseRank(factionEntry));

                    if (newFaction.ReputationListID == 109)
                        continue;
                    if (newFaction.ReputationListID == 108)
                        continue;

                    _factions.Add(newFaction.ReputationListID, newFaction);
                }
            }
        }
        void UpdateRankCounters(ReputationRank old_rank, ReputationRank new_rank)
        {
            if (old_rank >= ReputationRank.Exalted)
                --_exaltedFactionCount;
            if (old_rank >= ReputationRank.Revered)
                --_reveredFactionCount;
            if (old_rank >= ReputationRank.Honored)
                --_honoredFactionCount;

            if (new_rank >= ReputationRank.Exalted)
                ++_exaltedFactionCount;
            if (new_rank >= ReputationRank.Revered)
                ++_reveredFactionCount;
            if (new_rank >= ReputationRank.Honored)
                ++_honoredFactionCount;
        }

        //Gets
        ReputationRank GetBaseRank(FactionEntry factionEntry)
        {
            int reputation = GetBaseReputation(factionEntry);
            return ReputationToRank(reputation);
        }
        byte GetVisibleFactionCount() { return _visibleFactionCount; }
        byte GetHonoredFactionCount() { return _honoredFactionCount; }
        byte GetReveredFactionCount() { return _reveredFactionCount; }
        byte GetExaltedFactionCount() { return _exaltedFactionCount; }
        SortedDictionary<uint, FactionState> GetStateList() { return _factions; }
        FactionState GetState(FactionEntry factionEntry) { return factionEntry.CanHaveReputation() ? GetState(factionEntry.reputationListID) : null; }
        FactionState GetState(int id)
        {
            return _factions.LookupByKey((uint)id);
        }
        //uint GetReputationRankStrIndex(FactionEntry factionEntry)
        //{
            //return ReputationRankStrIndex[GetRank(factionEntry)];
        //}
        public bool IsAtWar(FactionEntry factionEntry)
        {
            if (factionEntry == null)
                return false;

            FactionState factionState = GetState(factionEntry);
            if (factionState != null)
                return Convert.ToBoolean(factionState.Flags & FactionFlags.AtWar);
            return false;
        }
        public ReputationRank GetForcedRankIfAny(FactionTemplateEntry factionTemplateEntry) { return _forcedReactions.LookupByKey(factionTemplateEntry.faction); }
        public ReputationRank GetRank(FactionEntry factionEntry)
        {
            int reputation = GetReputation(factionEntry);
            return ReputationToRank(reputation);
        }
        int GetReputation(uint faction_id)
        {
            var factionEntry = DBCStorage.FactionStorage.LookupByKey(faction_id);

            if (factionEntry.ID == 0)
            {
                Log.outError("ReputationMgr->GetReputation: Can't get reputation of {0} for unknown faction (faction id) #{1}.", _player.GetName(), faction_id);
                return 0;
            }

            return GetReputation(factionEntry);
        }
        int GetReputation(FactionEntry factionEntry)
        {
            // Faction without recorded reputation. Just ignore.
            if (factionEntry.ID == 0)
                return 0;

            FactionState state = GetState(factionEntry);
            if (state != null)
                return GetBaseReputation(factionEntry) + state.Standing;

            return 0;
        }
        int GetBaseReputation(FactionEntry factionEntry)
        {
            if (factionEntry.ID == 0)
                return 0;

            uint raceMask = _player.getRaceMask();
            uint classMask = _player.getClassMask();
            for (var i = 0; i < 4; i++)
            {
                if (Convert.ToBoolean(factionEntry.BaseRepRaceMask[i] & raceMask) ||
                    (factionEntry.BaseRepRaceMask[i] == 0 && factionEntry.BaseRepClassMask[i] != 0) &&
                    Convert.ToBoolean(factionEntry.BaseRepClassMask[i] & classMask) ||
                    factionEntry.BaseRepClassMask[i] == 0)

                    return factionEntry.BaseRepValue[i];
            }

            // in faction.dbc exist factions with (RepListId >=0, listed in character reputation list) with all BaseRepRaceMask[i] == 0
            return 0;
        }
        ReputationRank ReputationToRank(int standing)
        {
            int limit = Reputation_Cap + 1;
            for (int i = (int)ReputationRank.Max - 1; i >= (int)ReputationRank.Min; --i)
            {
                limit -= PointsInRank[i];
                if (standing >= limit)
                    return (ReputationRank)i;
            }
            return ReputationRank.Min;
        }
        uint GetDefaultStateFlags(FactionEntry factionEntry)
        {
            if (factionEntry == null)
                return 0;

            uint raceMask = _player.getRaceMask();
            uint classMask = _player.getClassMask();
            for (int i = 0; i < 4; i++)
            {
                if (Convert.ToBoolean(factionEntry.BaseRepRaceMask[i] & raceMask) ||
                    (factionEntry.BaseRepRaceMask[i] == 0 &&
                     factionEntry.BaseRepClassMask[i] != 0 &&
                    Convert.ToBoolean(factionEntry.BaseRepClassMask[i] & classMask) ||
                     factionEntry.BaseRepClassMask[i] == 0))

                    return factionEntry.ReputationFlags[i];
            }
            return 0;
        }

        //Sets
        bool SetReputation(FactionEntry factionEntry, int standing) { return SetReputation(factionEntry, standing, false); }
        bool ModifyReputation(FactionEntry factionEntry, int standing) { return SetReputation(factionEntry, standing, true); }
        public bool SetReputation(FactionEntry factionEntry, int standing, bool incremental = true)
        {
            //sScriptMgr->OnPlayerReputationChange(_player, factionEntry->ID, standing, incremental);
            bool res = false;
            // if spillover definition exists in DB, override DBC
            var repTemplate = Cypher.ObjMgr.GetRepSpillover(factionEntry.ID);
            if (repTemplate != null)
            {
                for (var i = 0; i < 5; ++i)
                {
                    if (repTemplate.faction[i] != 0)
                    {
                        if (_player.GetReputationRank(repTemplate.faction[i]) <= (ReputationRank)repTemplate.faction_rank[i])
                        {
                            // bonuses are already given, so just modify standing by rate
                            int spilloverRep = (int)(standing * repTemplate.faction_rate[i]);
                            SetOneFactionReputation(DBCStorage.FactionStorage.LookupByKey(repTemplate.faction[i]), spilloverRep, incremental);
                        }
                    }
                }
            }
            else
            {
                float spillOverRepOut = (float)standing;
                // check for sub-factions that receive spillover

                //might be slow
                var flist = DBCStorage.FactionStorage.Where(p => p.Value.team == factionEntry.team);
                // if has no sub-factions, check for factions with same parent
                if (flist.Count() == 0 && factionEntry.team != 0 && factionEntry.spilloverRateOut != 0.0f)
                {
                    spillOverRepOut *= factionEntry.spilloverRateOut;
                    FactionEntry parent = DBCStorage.FactionStorage.LookupByKey(factionEntry.team);
                    if (parent.ID != 0)
                    {
                        var parentState = _factions.LookupByKey((uint)parent.reputationListID);
                        // some team factions have own reputation standing, in this case do not spill to other sub-factions
                        if (parentState != null && Convert.ToBoolean(parentState.Flags & FactionFlags.Special))
                        {
                            SetOneFactionReputation(parent, (int)spillOverRepOut, incremental);
                        }
                        else    // spill to "sister" factions
                        {
                            flist = DBCStorage.FactionStorage.Where(p => p.Value.team == factionEntry.team);
                        }
                    }
                }
                if (flist.Count() != 0)
                {
                    // Spillover to affiliated factions
                    foreach (var itr in flist)
                    {
                        var factionEntryCalc = DBCStorage.FactionStorage.LookupByKey(itr.Key);
                        if (factionEntryCalc.ID != 0)
                        {
                            if (factionEntryCalc.ID == factionEntry.ID || GetRank(factionEntryCalc) > (ReputationRank)factionEntryCalc.spilloverMaxRankIn)
                                continue;
                            int spilloverRep = (int)(spillOverRepOut * factionEntryCalc.spilloverRateIn);
                            if (spilloverRep != 0 || !incremental)
                                res = SetOneFactionReputation(factionEntryCalc, spilloverRep, incremental);
                        }
                    }
                }
            }

            // spillover done, update faction itself
            var faction = _factions.LookupByKey((uint)factionEntry.reputationListID);
            if (faction != null)
            {
                res = SetOneFactionReputation(factionEntry, standing, incremental);
                // only this faction gets reported to client, even if it has no own visible standing
                SendState(faction);
            }
            return res;
        }
        bool SetOneFactionReputation(FactionEntry factionEntry, int standing, bool incremental)
        {
            var itr = _factions.LookupByKey((uint)factionEntry.reputationListID);
            if (itr != null)
            {
                int BaseRep = GetBaseReputation(factionEntry);

                if (incremental)
                {
                    // int32 *= float cause one point loss?
                    standing = (int)(Math.Floor((float)standing * 1 + 0.5f));//sWorld->getRate(RATE_REPUTATION_GAIN) + 0.5f));
                    standing += itr.Standing + BaseRep;
                }

                if (standing > Reputation_Cap)
                    standing = Reputation_Cap;
                else if (standing < Reputation_Bottom)
                    standing = Reputation_Bottom;

                ReputationRank old_rank = ReputationToRank(itr.Standing + BaseRep);
                ReputationRank new_rank = ReputationToRank(standing);

                itr.Standing = standing - BaseRep;
                itr.needSend = true;
                itr.needSave = true;

                SetVisible(itr);

                if (new_rank <= ReputationRank.Hostile)
                    SetAtWar(itr, true);

                if (new_rank > old_rank)
                    _sendFactionIncreased = true;

                UpdateRankCounters(old_rank, new_rank);

                //_player->ReputationChanged(factionEntry);
                //_player->UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_KNOWN_FACTIONS,          factionEntry->ID);
                //_player->UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_GAIN_REPUTATION,         factionEntry->ID);
                //_player->UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_GAIN_EXALTED_REPUTATION, factionEntry->ID);
                //_player->UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_GAIN_REVERED_REPUTATION, factionEntry->ID);
                //_player->UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_GAIN_HONORED_REPUTATION, factionEntry->ID);

                return true;
            }
            return false;
        }
        public void SetVisible(FactionTemplateEntry factionTemplateEntry)
        {
            if (factionTemplateEntry.faction == 0)
                return;

            var factionEntry = DBCStorage.FactionStorage.LookupByKey(factionTemplateEntry.faction);
            if (factionEntry.ID != 0)
                // Never show factions of the opposing team
                if (!Convert.ToBoolean(factionEntry.BaseRepRaceMask[1] & _player.getRaceMask()) && factionEntry.BaseRepValue[1] == Reputation_Bottom)
                    SetVisible(factionEntry);
        }
        void SetVisible(FactionEntry factionEntry)
        {
            if (factionEntry.reputationListID < 0)
                return;

            var itr = _factions.LookupByKey((uint)factionEntry.reputationListID);
            if (itr == null)
                return;

            SetVisible(itr);
        }
        void SetVisible(FactionState faction)
        {
            // always invisible or hidden faction can't be make visible
            // except if faction has FACTION_FLAG_SPECIAL
            if (Convert.ToBoolean(faction.Flags & (FactionFlags.InvisibleForced | FactionFlags.Hidden)) && !Convert.ToBoolean(faction.Flags & FactionFlags.Special))
                return;

            // already set
            if (Convert.ToBoolean(faction.Flags & FactionFlags.Visible))
                return;

            faction.Flags |= FactionFlags.Visible;
            faction.needSend = true;
            faction.needSave = true;

            _visibleFactionCount++;

            SendVisible(faction);
        }
        void SetAtWar(uint repListID, bool on)
        {
            var itr = _factions.LookupByKey(repListID);
            if (itr == null)
                return;

            // always invisible or hidden faction can't change war state
            if (Convert.ToBoolean(itr.Flags & (FactionFlags.InvisibleForced | FactionFlags.Hidden)))
                return;

            SetAtWar(itr, on);
        }
        void SetAtWar(FactionState faction, bool atWar)
        {
            // not allow declare war to own faction
            if (atWar && Convert.ToBoolean(faction.Flags & FactionFlags.PeaceForced))
                return;

            // already set
            if (((faction.Flags & FactionFlags.AtWar) != 0) == atWar)
                return;

            if (atWar)
                faction.Flags |= FactionFlags.AtWar;
            else
                faction.Flags &= ~FactionFlags.AtWar;

            faction.needSend = true;
            faction.needSave = true;
        }
        void SetInactive(uint repListID, bool on)
        {
            var itr = _factions.LookupByKey(repListID);
            if (itr == null)
                return;

            SetInactive(itr, on);
        }
        void SetInactive(FactionState faction, bool inactive)
        {
            // always invisible or hidden faction can't be inactive
            if (inactive && Convert.ToBoolean(faction.Flags & (FactionFlags.InvisibleForced | FactionFlags.Hidden)) || !Convert.ToBoolean(faction.Flags & FactionFlags.Visible))
                return;

            // already set
            if (((faction.Flags & FactionFlags.Inactive) != 0) == inactive)
                return;

            if (inactive)
                faction.Flags |= FactionFlags.Inactive;
            else
                faction.Flags &= ~FactionFlags.Inactive;

            faction.needSend = true;
            faction.needSave = true;
        }


        public void SendVisible(FactionState faction)
        {
            if (!_player.IsInWorld)//GetSession()->PlayerLoading())
                return;

            // make faction visible in reputation list at client
            PacketWriter data = new PacketWriter(Opcodes.SMSG_SetFactionVisible);
            data.WriteUInt32(faction.ReputationListID);
            _player.SendDirectMessage(data);
        }
        public void SendState(FactionState faction)
        {
            uint count = 1;

            PacketWriter data = new PacketWriter(Opcodes.SMSG_SetFactionStanding);
            data.WriteFloat(0.0f);
            data.WriteUInt8((byte)(_sendFactionIncreased ? 1 : 0));
            _sendFactionIncreased = false; // Reset

            int p_count = data.wpos();
            data.WriteUInt32(count);

            data.WriteUInt32(faction.ReputationListID);
            data.WriteInt32(faction.Standing);

            foreach (var itr in _factions.Values)
            {
                if (itr.needSend)
                {
                    itr.needSend = false;
                    if (itr.ReputationListID != faction.ReputationListID)
                    {
                        data.WriteUInt32(itr.ReputationListID);
                        data.WriteInt32(itr.Standing);
                        ++count;
                    }
                }
            }

            data.Replace<uint>(p_count, count);
            _player.SendDirectMessage(data);
        }
        public void SendInitialReputations()
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_InitializeFactions);
            data.WriteUInt32(256);    // count

            uint a = 0;
            foreach (var itr in _factions)
            {
                // fill in absent fields
                for (; a != itr.Key; ++a)
                {
                    data.WriteUInt8(0);
                    data.WriteUInt32(0);
                }

                // fill in encountered data
                data.WriteUInt8((byte)itr.Value.Flags);
                data.WriteInt32(itr.Value.Standing);

                itr.Value.needSend = false;

                a++;
            }
            
            // fill in absent fields
            for (; a != 256; ++a)
            {
                data.WriteUInt8(0);
                data.WriteUInt32(0);
            }
            //_player.SendDirectMessage(data);
        }
        public void SendForceReactions()
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_SetForcedReactions);
            data.WriteInt32(_forcedReactions.Count);
            foreach (var itr in _forcedReactions)
            {
                data.WriteUInt32(itr.Key);                         // faction_id (Faction.dbc)
                data.WriteUInt32((uint)itr.Value);                        // reputation rank
            }
            _player.SendDirectMessage(data);
        }

        public void LoadFromDB()
        {
            // Set initial reputations (so everything is nifty before DB data load)
            Initialize();

            SQLResult result = DB.Characters.Select("SELECT faction, standing, flags FROM character_reputation WHERE guid = {0}", _player.GetGUIDLow());

            if (result.Count != 0)
            {
                for (var i = 0; i < result.Count; i++)
                {
                    var factionEntry = DBCStorage.FactionStorage.LookupByKey(result.Read<uint>(i, 0));
                    if (factionEntry != null && factionEntry.reputationListID >= 0)
                    {
                        var faction = _factions.LookupByKey((uint)factionEntry.reputationListID);

                        // update standing to current
                        faction.Standing = result.Read<int>(i, 1);

                        // update counters
                        int BaseRep = GetBaseReputation(factionEntry);
                        ReputationRank old_rank = ReputationToRank(BaseRep);
                        ReputationRank new_rank = ReputationToRank(BaseRep + faction.Standing);
                        UpdateRankCounters(old_rank, new_rank);

                        FactionFlags dbFactionFlags = (FactionFlags)result.Read<uint>(i, 2);

                        if (Convert.ToBoolean(dbFactionFlags & FactionFlags.Visible))
                            SetVisible(faction);                    // have internal checks for forced invisibility

                        if (Convert.ToBoolean(dbFactionFlags & FactionFlags.Inactive))
                            SetInactive(faction, true);              // have internal checks for visibility requirement

                        if (Convert.ToBoolean(dbFactionFlags & FactionFlags.AtWar))  // DB at war
                            SetAtWar(faction, true);                 // have internal checks for FACTION_FLAG_PEACE_FORCED
                        else                                        // DB not at war
                        {
                            // allow remove if visible (and then not FACTION_FLAG_INVISIBLE_FORCED or FACTION_FLAG_HIDDEN)
                            if (Convert.ToBoolean(faction.Flags & FactionFlags.Visible))
                                SetAtWar(faction, false);            // have internal checks for FACTION_FLAG_PEACE_FORCED
                        }

                        // set atWar for hostile
                        if (GetRank(factionEntry) <= ReputationRank.Hostile)
                            SetAtWar(faction, true);

                        // reset changed flag if values similar to saved in DB
                        if (faction.Flags == dbFactionFlags)
                        {
                            faction.needSend = false;
                            faction.needSave = false;
                        }
                    }
                }
            }
        }
        public void SaveToDB()
        {
            foreach (var itr in _factions.Values)
            {
                if (itr.needSave)
                {
                    PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_CHAR_REPUTATION_BY_FACTION);
                    stmt.AddValue(0, _player.GetGUIDLow());
                    stmt.AddValue(1, (ushort)itr.ID);
                    DB.Characters.Execute(stmt);

                    stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_INS_CHAR_REPUTATION_BY_FACTION);
                    stmt.AddValue(0, _player.GetGUIDLow());
                    stmt.AddValue(1, (ushort)(itr.ID));
                    stmt.AddValue(2, itr.Standing);
                    stmt.AddValue(3, (ushort)(itr.Flags));
                    DB.Characters.Execute(stmt);

                    itr.needSave = false;
                }
            }
        }



        #region Fields
        Player _player;
        byte _visibleFactionCount;// :8;
        byte _honoredFactionCount;// :8;
        byte _reveredFactionCount;// :8;
        byte _exaltedFactionCount;// :8;
        bool _sendFactionIncreased; //! Play visual effect on next SMSG_SET_FACTION_STANDING sent
        #endregion

        int[] PointsInRank = new int[] { 36000, 3000, 3000, 3000, 6000, 12000, 21000, 1000 };
        //static uint[] ReputationRankStrIndex = new uint[(int)ReputationRank.Max]
        //{
        //LANG_REP_HATED,    LANG_REP_HOSTILE, LANG_REP_UNFRIENDLY, LANG_REP_NEUTRAL,
        //LANG_REP_FRIENDLY, LANG_REP_HONORED, LANG_REP_REVERED,    LANG_REP_EXALTED
        //};
        const int Reputation_Cap = 42999;
        const int Reputation_Bottom = -42000;
        SortedDictionary<uint, FactionState> _factions = new SortedDictionary<uint, FactionState>();
        Dictionary<uint, ReputationRank> _forcedReactions = new Dictionary<uint,ReputationRank>();
         
        public class FactionState
        {
            public uint ID;
            public uint ReputationListID;
            public int Standing;
            public FactionFlags Flags;
            public bool needSend;
            public bool needSave;
        }
    }

    public class RepSpilloverTemplate
    {
        public uint[] faction = new uint[5];
        public float[] faction_rate = new float[5];
        public uint[] faction_rank = new uint[5];
    }
}
