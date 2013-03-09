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
using Framework.Configuration;
using Framework.Constants;
using Framework.Database;
using Framework.Network;
using WorldServer.Game.Managers;
using WorldServer.Network;
using Framework.ObjectDefines;

namespace WorldServer.Game.WorldEntities
{
    public class Guild : Cypher
    {
        internal const int MaxBankTabs = 8;
        internal const int MaxBankSlots = 98;
        internal const int BankMoneyLogsTab = 100;
        internal const int MinRanks = 5;
        internal const int MaxRanks = 10;
        internal const int RankNone = 0xFF;
        internal const uint WithdrawMoneyUnlimited = 0xFFFFFFFF;
        internal const uint WithdrawSlotUnlimited = 0xFFFFFFFF;
        internal const uint EventLogGuidUndefined = 0xFFFFFFFF;
        internal const int ExperienceUncappedLevel = 20;

        public Guild() { }

        //classes
        public class Member
        {
            public string Name { get; set; }
            public uint ZoneId { get; set; }
            public uint Level { get; set; }
            public Byte Class { get; set; }
            public uint Flags { get; set; }
            public ulong LogoutTime { get; set; }
            public ObjectGuid CharGuid { get; set; }
            public uint RankId { get; set; }
            public string PublicNote { get; set; }
            public string OfficerNote { get; set; }
            public uint BankResetTimeMoney { get; set; }
            public uint BankRemainingMoney { get; set; }
            public uint AchievementPoints { get; set; }
            public List<BankTabs> BanksTabs = new List<BankTabs>(Guild.MaxBankTabs);
            public List<Profession> ProfessionList = new List<Profession>(2);

            internal void ChangeRank(uint newRank)
            {
                RankId = newRank;
                PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildUpdMemberRank);
                stmt.AddValue(0, newRank);
                stmt.AddValue(1, CharGuid);
                DB.Characters.Execute(stmt);
            }
            internal void SetNote(bool Public, string Note)
            {
                PreparedStatement stmt;
                if (Public)
                {
                    PublicNote = Note;
                    stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildUpdMemberPnote);
                    stmt.AddValue(0, Note);
                    stmt.AddValue(1, CharGuid);
                    //DB.Characters.Execute("UPDATE guild_members SET pnote = '{0}' WHERE CharGuid = {1}", Note, CharGuid);
                }
                else
                {
                    OfficerNote = Note;
                    stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildUpdMemberOffnote);
                    stmt.AddValue(0, Note);
                    stmt.AddValue(1, CharGuid);
                    //DB.Characters.Execute("UPDATE guild_members SET offnote = '{0}' WHERE CharGuid = {1}", Note, CharGuid);
                }
                DB.Characters.Execute(stmt);
            }
            internal void UpdateStats(Player pChar)
            {
                Name = pChar.Name;
                Level = pChar.getLevel();
                Class = pChar.getClass();
                ZoneId = pChar.GetZoneId();
            }
            internal void SaveToDB(ulong guid)
            {
                PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildInsMember);
                stmt.AddValue(0, guid);
                stmt.AddValue(1, CharGuid);
                stmt.AddValue(2, RankId);
                stmt.AddValue(3, PublicNote);
                stmt.AddValue(4, OfficerNote);
                DB.Characters.Execute(stmt);
            }

            public class Profession
            {
                public uint SkillId { get; set; }
                public uint Rank { get; set; }
                public uint Level { get; set; }
            }
            public class BankTabs
            {
                public uint ResetTime { get; set; }
                public uint SlotsLeft { get; set; }
            }
        }
        public class Rank
        {
            public uint RankId { get; set; }
            public string Name { get; set; }
            public uint Rights { get; set; }
            public uint BankMoneyPerDay { get; set; }
            public uint[] TabSlotPerDay = new uint[MaxBankTabs];// { get; set; }
            public uint[] TabRight = new uint[MaxBankTabs];// { get; set; }
            public uint Order { get; set; }

            internal void SaveToDB(ulong GuildGuid)
            {
                PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildInsRank);
                stmt.AddValue(0, GuildGuid);
                stmt.AddValue(1, RankId);
                stmt.AddValue(2, Name);
                stmt.AddValue(3, Rights);
                DB.Characters.Execute(stmt);
            }
        }
        public class EventLog
        {
            public byte EventType { get; set; }
            public ObjectGuid PlayerGuid1 { get; set; }
            public ObjectGuid PlayerGuid2 { get; set; }
            public byte NewRank { get; set; }
            public ulong TimeStamp { get; set; }
        }
        public class BankTab
        {
            public uint TabId { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public string Text { get; set; }
            List<Items> ItemList = new List<Items>();

            class Items
            {
                public uint itemId { get; set; }
                public uint Slot { get; set; }
                public uint Count { get; set; }
            }
        }
        public class BankEventLog
        {
            public GuildBankEventLogTypes EventType { get; set; }
            public ulong PlayerGuid { get; set; }
            public uint ItemOrMoney { get; set; }
            public uint ItemStackCount { get; set; }
            public uint DestTabId { get; set; }
            public ulong TimeStamp { get; set; }

            bool isMoneyEvent()
            {
                return EventType == GuildBankEventLogTypes.DepositMoney ||
                    EventType == GuildBankEventLogTypes.WithdrawMoney ||
                    EventType == GuildBankEventLogTypes.RepairMoney;
            }
        }

        //Members
        public bool AddMember(UInt64 plGuid, int rankId = -1)
        {
            Player pl = ObjMgr.FindPlayer(plGuid);
            if (pl == null || pl.GuildGuid != 0)
                return false;

            //remove all signs from petitions

            if (rankId == -1)
                rankId = GetLowestRank();

            Member member = new Member()
            {
                CharGuid = new ObjectGuid(pl.GetGUIDLow()),
                Name = pl.Name,
                Level = pl.getLevel(),
                Class = pl.getClass(),
                ZoneId = pl.GetZoneId(),
                Flags = (byte)GuildMemberFlags.Online,
                LogoutTime = 0,
                RankId = (uint)rankId,
                PublicNote = "",
                OfficerNote = "",
                AchievementPoints = 0,
                BankRemainingMoney = 0,
                BankResetTimeMoney = 0,
            };
            for (int i = 0; i < 2; i++)
            {
                Member.Profession prof = new Member.Profession();
                prof.Level = 0;
                prof.SkillId = 0;
                prof.Rank = 0;
                member.ProfessionList.Add(prof);
            }

            MemberList.Add(member);

            member.SaveToDB(Guid);

            pl.SetInGuild(Guid);
            pl.SetGuildRank(member.RankId);
            pl.SetGuildLevel(GetLevel());
            pl.SetGuildInvited(0);
            /*
            pl.GetReputationMgr().SetReputation(sFactionStore.LookupEntry(1168), 1);
            if (sWorld->getBoolConfig(CONFIG_GUILD_LEVELING_ENABLED))
            {
                for (uint32 i = 0; i < sGuildPerkSpellsStore.GetNumRows(); ++i)
                    if (GuildPerkSpellsEntry const* entry = sGuildPerkSpellsStore.LookupEntry(i))
                if (entry->Level >= GetLevel())
                    player->learnSpell(entry->SpellId, true);
            }
            */
            return true;
        }
        public void DeleteMember(ulong guid)
        {
            Player player = ObjMgr.FindPlayer(guid);
            if (GetLeaderGuid() == guid)
            {
                Member oldLeader = null;
                Member newLeader = null;
                foreach (var member in MemberList.OrderBy(p => p.RankId))
                {
                    if (member.CharGuid == guid)
                        oldLeader = member;
                    else if (newLeader == null || newLeader.RankId > member.RankId)
                        newLeader = member;
                }

                if (newLeader == null)
                {
                    Disband();
                    return;
                }

                SetLeader(newLeader);

                Player newLeaderPlayer = ObjMgr.FindPlayer(newLeader.CharGuid);
                if (newLeaderPlayer != null)
                    newLeaderPlayer.SetGuildRank((uint)GuildDefaultRanks.Master);

                if (oldLeader != null)
                {
                    SendBroadcastEvent(GuildEvents.LeaderChanged, 0, oldLeader.Name, newLeader.Name);
                    SendBroadcastEvent(GuildEvents.Left, guid, oldLeader.Name);
                }
            }
            Member m = GetMember(guid);
            if (m == null)
                return;
            MemberList.Remove(m);

            if (player != null)
            {
                player.SetInGuild(0);
                player.SetGuildRank(0);
                player.SetGuildLevel(0);
                /*
                for (uint32 i = 0; i < sGuildPerkSpellsStore.GetNumRows(); ++i)
                    if (GuildPerkSpellsEntry const* entry = sGuildPerkSpellsStore.LookupEntry(i))
                if (entry->Level >= GetLevel())
                    player->removeSpell(entry->SpellId, false, false);
                */
            }
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildDelMember);
            stmt.AddValue(0, guid);
            DB.Characters.Execute(stmt);
        }

        public Member GetMember(ulong guid) { return MemberList.FirstOrDefault(m => m.CharGuid == guid); }
        public Member GetMember(string name)
        {
            foreach (Member member in MemberList)
                if (member.Name == name)
                    return member;

            return null;
        }
        public int GetMemberSize() { return MemberList.Count(); }

        //Ranks
        public void CreateDefaultRanks()
        {
            PreparedStatement stmt = null;

            stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildDelRanks);
            stmt.AddValue(0, Guid);
            DB.Characters.Execute(stmt);

            stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildDelBankRights);
            stmt.AddValue(0, Guid);
            DB.Characters.Execute(stmt);

            CreateRank(GuildDefaultRanks.Master.ToString(), GuildRankRights.All);
            CreateRank(GuildDefaultRanks.Officer.ToString(), GuildRankRights.All);
            CreateRank(GuildDefaultRanks.Veteran.ToString(), GuildRankRights.ChatListen | GuildRankRights.ChatSpeak);
            CreateRank(GuildDefaultRanks.Member.ToString(), GuildRankRights.ChatListen | GuildRankRights.ChatSpeak);
            CreateRank(GuildDefaultRanks.Initiate.ToString(), GuildRankRights.ChatListen | GuildRankRights.ChatSpeak);
        }
        public void AddNewRank(ref WorldSession session, string name) //, uint32 rankId)
        {
            if (GetRankSize() >= MaxRanks)
                return;
            
            if (GetLeaderGuid() != session.GetPlayer().GetGUIDLow())
               SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
            else
            {
                CreateRank(name, GuildRankRights.ChatListen | GuildRankRights.ChatSpeak);
                SendQuery(ref session);
                SendRoster();
            }
        }
        public void CreateRank(string name, GuildRankRights rights)
        {
            if (RankList.Count() == MaxRanks)
                return;

            uint newRankId = (uint)RankList.Count();
            Rank rank = new Rank()
            {
                RankId = newRankId,
                Name = name,
                Rights = (uint)rights,
                BankMoneyPerDay = 0,
                Order = newRankId
            };
            RankList.Add(rank);

            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildInsBankRightDefault);
            for (var i = 0; i < GetBankTabSize(); ++i)
            {
                stmt.AddValue(0, Guid);
                stmt.AddValue(1, i);
                stmt.AddValue(2, newRankId);
                DB.Characters.Execute(stmt);
                stmt.Clear();
            }
            rank.SaveToDB(Guid);
        }
        public void DeleteRank(ref WorldSession session, uint rankId)
        {
            if (GetRankSize() <= Guild.MinRanks || rankId >= GetRankSize())
                return;

            //todo: Delete bank rights for rank

            DB.Characters.Execute("DELETE FROM guild_ranks WHERE RankId = {0} && GuildGuid = {1}", rankId, Guid);

            RankList.RemoveAll(r => r.RankId == rankId);

            SendQuery(ref session);
            SendRoster();
        }
        public void SendRanks(ref WorldSession session)
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildRanks);

            int count = GetRankSize();
            writer.WriteBits(count, 18);

            foreach (Rank rank in RankList)
                writer.WriteBits(rank.Name.Length, 7);

            writer.BitFlush();
            for (int i = 0; i < count; i++)
            {
                writer.WriteUInt32((uint)i);//Creation Order
                for (int j = 0; j < MaxBankTabs; ++j)
                {
                    writer.WriteUInt32(0);//RankList[i].TabSlotPerDay[j]);
                    writer.WriteUInt32(0);//RankList[i].TabRight[j]);
                }
                writer.WriteUInt32(RankList[i].BankMoneyPerDay);
                writer.WriteUInt32(RankList[i].Rights);
                writer.WriteString(RankList[i].Name);
                writer.WriteUInt32(RankList[i].Order);//order
            }
            session.Send(writer);
        }

        public void SetRankInfo(uint rankId, uint newRights, string Name, uint MoneyPerDay)
        {
            if (rankId == (uint)GuildDefaultRanks.Master)
                return;

            RankList[(int)rankId].Name = Name;
            RankList[(int)rankId].Rights = newRights;
            RankList[(int)rankId].BankMoneyPerDay = MoneyPerDay;

            foreach (Member member in MemberList)
                if (member.RankId == rankId)
                    member.BankResetTimeMoney = 0;

            DB.Characters.Execute("UPDATE guild_ranks SET Name = '{0}', Rights = {1}, BankMoneyPerDay = {2} WHERE GuildGuid = {3} AND RankId = {4}", Name, newRights, MoneyPerDay, Guid, rankId);
            DB.Characters.Execute("UPDATE guild_members SET ResetTimeMoney = 0 WHERE GuildGuid = {0} AND RankId = {1}", Guid, rankId);
        }
        public void SetRankOrder(uint rankId, byte Direction)
        {
            Rank changerank = GetRank(rankId);
            uint neworder = changerank.Order - 1;

            foreach (Rank rank in RankList)
            {
                if (rank.Order == neworder)
                {
                    rank.Order = rank.Order + 1;
                    //DB.Characters.Execute("UPDATE guild_ranks SET rankOrder = {0} WHERE RankId = {1}", rank.Order, rank.RankId);
                    break;
                }
            }
            changerank.Order = neworder;
            //DB.Characters.Execute("UPDATE guild_ranks SET rankOrder = {0} WHERE RankId = {1}", changerank.Order, changerank.RankId);
            //SendQuery();
            //SendRanks();
        }
        public Rank GetRank(uint RankId) { return RankList.First(r => r.RankId == RankId); }
        public string GetRankName(uint rankId) { return RankList.First(r => r.RankId == rankId).Name; }
        public int GetRankSize() { return RankList.Count(); }
        public bool HasRank(uint rankId) { return RankList.Any(p => p.RankId == rankId); }
        public bool HasRankRight(uint rankId, uint rights) { return Convert.ToBoolean(RankList[(int)rankId].Rights & rights); }
        public int GetLowestRank() { return RankList.Count - 1; }

        //Bank
        public void SendBankQuery()
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildBankQuery);

            uint itemcount = 0; //temp
            uint tabcount = (uint)BankTabList.Count();

            writer.WriteBit(false); //unk
            writer.WriteBits(itemcount, 20);
            writer.WriteBits(tabcount, 22);

            //for (var i = 0; i < itemcount; ++i)
            //enchants[i] = packet.ReadBits(24); // Number of Enchantments ?
            foreach (BankTab tab in BankTabList)
            {
                writer.WriteBits(tab.Icon.Length, 9);
                writer.WriteBits(tab.Text.Length, 7);
            }

            foreach (BankTab tab in BankTabList)
            {
                writer.WriteString(tab.Icon);
                writer.WriteUInt32(tab.TabId);//index
                writer.WriteString(tab.Text);
            }

            writer.WriteUInt64(BankMoney);

            for (var i = 0; i < itemcount; ++i)
            {
                for (var j = 0; j < 0; ++j) //enchants number
                {
                    //packet.ReadUInt32("Enchantment Slot Id?", i, j);
                    //packet.ReadUInt32("Enchantment Id?", i, j);
                }
                //packet.ReadUInt32("Unk UInt32 1", i); // Only seen 0
                //packet.ReadUInt32("Unk UInt32 2", i); // Only seen 0
                //packet.ReadUInt32("Unk UInt32 3", i); // Only seen 0
                //packet.ReadUInt32("Stack Count", i);
                //packet.ReadUInt32("Slot Id", i);
                //packet.ReadEnum<UnknownFlags>("Unk mask", TypeCode.UInt32, i);
                //packet.ReadEntryWithName<Int32>(StoreNameType.Item, "Item Entry", i);
                //packet.ReadInt32("Random Item Property Id", i);
                //packet.ReadUInt32("Spell Charges", i);
                //packet.ReadUInt32("Item Suffix Factor", i);
            }
            writer.WriteUInt32(0);//Tab ?
            writer.WriteUInt32(0);//remaining withdraw for the member
        }

        public void AddBankTab()
        {

        }
        public void SetBankRightsAndSlots(uint rankId, uint TabId, ushort BankRights, ushort BankSlotPerDay)
        {
            if (rankId == (uint)GuildDefaultRanks.Master)
                return;

            RankList[(int)rankId].TabRight[TabId] = BankRights;
            RankList[(int)rankId].TabSlotPerDay[TabId] = BankSlotPerDay;

            foreach (Member member in MemberList)
                if (member.RankId == rankId)
                    for (int i = 0; i < MaxBankTabs; i++)
                        member.BanksTabs[i].ResetTime = 0;

            DB.Characters.Execute("DELETE FROM guild_bank_rights WHERE GuildGuid = {0} AND TabId = {1} AND RankId = {2}", Guid, TabId, rankId);
            DB.Characters.Execute("INSERT INTO guild_bank_rights (GuildGuid,TabId,RankId,Rights,SlotPerDay) VALUES ({0}, {1}, {2}, {3}, {4})",
                Guid, TabId, rankId, RankList[(int)rankId].TabRight[TabId], RankList[(int)rankId].TabSlotPerDay[TabId]);
            DB.Characters.Execute("UPDATE guild_members SET ResetTimeTab{0} = 0 WHERE GuildGuid = {1} AND RankId = {2}", TabId, Guid, rankId);

        }
        public int GetBankTabSize() { return BankTabList.Count(); }

        //Leveling
        void GiveXP(uint xp, Player source)
        {
            if (WorldConfig.GuildLevelingEnabled == 0)
                return;

            // @TODO: Award reputation and count activity for player

            if (GetLevel() >= WorldConfig.GuildMaxLevel)
                xp = 0;
            if (GetLevel() >= ExperienceUncappedLevel)
                xp = Math.Min(xp, 7807500 - (uint)TodayExperience);

            PacketWriter data = new PacketWriter(Opcodes.SMSG_GuildXpEarned);
            data.WriteUInt64(xp);
            source.GetSession().Send(data);

            Experience += xp;
            TodayExperience += xp;

            if (xp == 0)
                return;

            // Ding, mon!
            while (Experience >= GuildMgr.GetXPForGuildLevel(GetLevel()) && GetLevel() < WorldConfig.GuildMaxLevel)
            {
                Experience -= GuildMgr.GetXPForGuildLevel(GetLevel());
                SetLevel(GetLevel() + 1);
            }
        }
        public void SetLevel(uint NewLevel)
        {
            uint oldLevel = GetLevel();
            Level = NewLevel;
            /*
            // Find all guild perks to learn
            std::vector<uint32> perksToLearn;
            for (uint32 i = 0; i < sGuildPerkSpellsStore.GetNumRows(); ++i)
                if (GuildPerkSpellsEntry const* entry = sGuildPerkSpellsStore.LookupEntry(i))
            if (entry->Level > oldLevel && entry->Level <= GetLevel())
                perksToLearn.push_back(entry->SpellId);
            */
            // Notify all online players that guild level changed and learn perks
            Player pl;
            foreach (var member in MemberList)
            {
                pl = ObjMgr.FindPlayer(member.CharGuid);
                if (pl != null)
                {
                    pl.SetGuildLevel(GetLevel());
                    //for (size_t i = 0; i < perksToLearn.size(); ++i)
                        //pl->learnSpell(perksToLearn[i], true);
                }
            }

            //GetNewsLog().AddNewEvent(GUILD_NEWS_LEVEL_UP, time(NULL), 0, 0, _level);
            //GetAchievementMgr().UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_REACH_GUILD_LEVEL, GetLevel(), 0, NULL, source);
        }

        //Event
        public void SendBroadcastEvent(GuildEvents guildEvent, ulong guid = 0, params object[] str)
        {
            int strCount = str.Count();
            if (strCount > 3)
                return;

            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildEvent);
            writer.WriteUInt8((byte)guildEvent);
            writer.WriteUInt8((byte)strCount);

            foreach (string text in str)
                writer.WriteCString(text);

            if (guid != 0)
                writer.WriteUInt64(guid);

            BroadcastPacket(writer);
        }
        public void SendGuildEventLog(ref WorldSession session)
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildEventLogQuery);

            writer.WriteBits(EventLogList.Count(), 23);

            foreach (EventLog log in EventLogList)
            {
                writer.WriteBit(log.PlayerGuid1[2]);
                writer.WriteBit(log.PlayerGuid1[4]);
                writer.WriteBit(log.PlayerGuid2[7]);
                writer.WriteBit(log.PlayerGuid2[6]);
                writer.WriteBit(log.PlayerGuid1[3]);
                writer.WriteBit(log.PlayerGuid2[3]);
                writer.WriteBit(log.PlayerGuid2[5]);
                writer.WriteBit(log.PlayerGuid1[7]);
                writer.WriteBit(log.PlayerGuid1[5]);
                writer.WriteBit(log.PlayerGuid1[0]);
                writer.WriteBit(log.PlayerGuid2[4]);
                writer.WriteBit(log.PlayerGuid2[2]);
                writer.WriteBit(log.PlayerGuid2[0]);
                writer.WriteBit(log.PlayerGuid2[1]);
                writer.WriteBit(log.PlayerGuid1[1]);
                writer.WriteBit(log.PlayerGuid1[6]);
            }
            writer.BitFlush();

            foreach (EventLog log in EventLogList)
            {
                writer.WriteByteSeq(log.PlayerGuid2[3]);
                writer.WriteByteSeq(log.PlayerGuid2[2]);
                writer.WriteByteSeq(log.PlayerGuid2[5]);
                writer.WriteUInt8(log.NewRank);
                writer.WriteByteSeq(log.PlayerGuid2[4]);
                writer.WriteByteSeq(log.PlayerGuid1[0]);
                writer.WriteByteSeq(log.PlayerGuid1[4]);
                writer.WriteUnixTime();//(uint)log.TimeStamp);
                writer.WriteByteSeq(log.PlayerGuid1[7]);
                writer.WriteByteSeq(log.PlayerGuid1[3]);
                writer.WriteByteSeq(log.PlayerGuid2[0]);
                writer.WriteByteSeq(log.PlayerGuid2[6]);
                writer.WriteByteSeq(log.PlayerGuid2[7]);
                writer.WriteByteSeq(log.PlayerGuid1[5]);
                writer.WriteUInt8(log.EventType);
                writer.WriteByteSeq(log.PlayerGuid2[1]);
                writer.WriteByteSeq(log.PlayerGuid1[2]);
                writer.WriteByteSeq(log.PlayerGuid1[6]);
                writer.WriteByteSeq(log.PlayerGuid1[1]);
            }
            session.Send(writer);
        }
        public void LogGuildEvent(GuildEventLogTypes EventType, ulong Guid1, ulong Guid2 = 0, uint newRank = 0)
        {
            EventLog NewEvent = new EventLog()
            {
                EventType = (byte)EventType,
                PlayerGuid1 = new ObjectGuid(Guid1),
                PlayerGuid2 = new ObjectGuid(Guid2),
                NewRank = (byte)newRank,
                TimeStamp = (ulong)DateTime.UtcNow.Ticks
            };

            EventLogList.Add(NewEvent);

            // Save event to DB
            //CharacterDatabase.PExecute("DELETE FROM guild_eventlog WHERE guildid='%u' AND LogGuid='%u'", m_Id, m_GuildEventLogNextGuid);
            //CharacterDatabase.PExecute("INSERT INTO guild_eventlog (guildid, LogGuid, EventType, PlayerGuid1, PlayerGuid2, NewRank, TimeStamp) VALUES ('%u','%u','%u','%u','%u','%u','" UI64FMTD "')",
            //m_Id, m_GuildEventLogNextGuid, uint32(NewEvent.EventType), NewEvent.PlayerGuid1, NewEvent.PlayerGuid2, uint32(NewEvent.NewRank), NewEvent.TimeStamp);
        }

        //Guild
        public void Disband()
        {
            SendBroadcastEvent(GuildEvents.Disbanded);

            foreach (var member in MemberList)
                DeleteMember(member.CharGuid);

            PreparedStatement stmt = null;

            stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildDel);
            stmt.AddValue(0, Guid);
            DB.Characters.Execute(stmt);

            stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildDelRanks);
            stmt.AddValue(0, Guid);
            DB.Characters.Execute(stmt);

            stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildDelBankTabs);
            stmt.AddValue(0, Guid);
            DB.Characters.Execute(stmt);

            // Free bank tab used memory and delete items stored in them
            //_DeleteBankItems(trans, true);

            //stmt = DB.Characters.GetPreparedStatement(CharStatements CHAR_DEL_GUILD_BANK_ITEMS);
            //stmt.AddValue(0, Guid);

            stmt = DB.Characters.GetPreparedStatement(CharStatements.GuildDelBankRights);
            stmt.AddValue(0, Guid);
            DB.Characters.Execute(stmt);

            //stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_GUILD_BANK_EVENTLOGS);
            //stmt.AddValue(0, Guid);
            //DB.Characters.Execute(stmt);

            //stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_GUILD_EVENTLOGS);
            //stmt.AddValue(0, Guid);
            //DB.Characters.Execute(stmt);

            //GuildFinderMgr.DeleteGuild(Guid);

            GuildMgr.DeleteGuild(Guid);
        }
        public void HandleMemberLogout(Player pChar)
        {
            Member member = GetMember(pChar.GetGUIDLow());
            if (member != null)
            {
                member.UpdateStats(pChar);
                member.LogoutTime = (ulong)DateTime.UtcNow.Ticks;
            }
            SendBroadcastEvent(GuildEvents.SignedOff, pChar.GetGUIDLow(), pChar.Name);
            //SaveToDB();
        }
        public void HandleMemberLogin(Player pChar)
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildEvent);
            writer.WriteUInt8((byte)GuildEvents.MOTD);
            writer.WriteUInt8((byte)1);
            writer.WriteCString(MOTD);
            pChar.GetSession().Send(writer);

            //HandleGuildRanks(session);

            SendBroadcastEvent(GuildEvents.SignedOn, pChar.GetGUIDLow(), pChar.Name);

            // Send to self separately, player is not in world yet and is not found by _BroadcastEvent
            //data.Initialize(SMSG_GUILD_EVENT, 1 + 1 + strlen(session->GetPlayer()->GetName()) + 8);
            //data << uint8(GE_SIGNED_ON);
            //data << uint8(1);
            //data << session->GetPlayer()->GetName();
            //data << uint64(session->GetPlayer()->GetGUID());
            //session->SendPacket(&data);

            //for (uint32 i = 0; i < sGuildPerkSpellsStore.GetNumRows(); ++i)
            //if (GuildPerkSpellsEntry const* entry = sGuildPerkSpellsStore.LookupEntry(i))
            //if (entry->Level >= GetLevel())
            //session->GetPlayer()->learnSpell(entry->SpellId, false);

            //SendGuildReputationWeeklyCap(session);

            //GetAchievementMgr().SendAllAchievementData(session->GetPlayer());

            writer = new PacketWriter(Opcodes.SMSG_GuildMemberDailyReset);
            pChar.GetSession().Send(writer);
        }
        public void SendInvite(Player pl, Player plInvited)
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildInvite);

            writer.WriteUInt32(GetLevel());
            writer.WriteUInt32(BorderStyle);
            writer.WriteUInt32(BorderColor);
            writer.WriteUInt32(EmblemStyle);
            writer.WriteUInt32(BackgroundColor);
            writer.WriteUInt32(EmblemColor);

            writer.WriteBit(Guid[3]);
            writer.WriteBit(Guid[2]);
            writer.WriteBits(0, 8); //old guild name

            writer.WriteBit(Guid[1]);
            writer.WriteBit(plInvited.GuildGuid[6]);
            writer.WriteBit(plInvited.GuildGuid[4]);
            writer.WriteBit(plInvited.GuildGuid[1]);
            writer.WriteBit(plInvited.GuildGuid[5]);
            writer.WriteBit(plInvited.GuildGuid[7]);
            writer.WriteBit(plInvited.GuildGuid[2]);
            writer.WriteBit(Guid[7]);
            writer.WriteBit(Guid[0]);
            writer.WriteBit(Guid[6]);
            writer.WriteBits(Name.Length, 8);
            writer.WriteBit(plInvited.GuildGuid[3]);
            writer.WriteBit(plInvited.GuildGuid[0]);
            writer.WriteBit(Guid[5]);
            writer.WriteBits(pl.Name.Length, 7);
            writer.WriteBit(Guid[4]);

            writer.BitFlush();

            writer.WriteByteSeq(Guid[1]);
            writer.WriteByteSeq(plInvited.GuildGuid[3]);
            writer.WriteByteSeq(Guid[6]);
            writer.WriteByteSeq(plInvited.GuildGuid[2]);
            writer.WriteByteSeq(plInvited.GuildGuid[1]);
            writer.WriteByteSeq(Guid[0]);

            //Old guild that you will lose rep with?
            //if (!GetLevel plInvited->GetGuildName().empty())
            //data.WriteString(pInvitee->GetGuildName());

            writer.WriteByteSeq(Guid[7]);
            writer.WriteByteSeq(plInvited.GuildGuid[2]);
            writer.WriteString(pl.Name);
            writer.WriteByteSeq(plInvited.GuildGuid[7]);
            writer.WriteByteSeq(plInvited.GuildGuid[6]);
            writer.WriteByteSeq(plInvited.GuildGuid[5]);
            writer.WriteByteSeq(plInvited.GuildGuid[0]);
            writer.WriteByteSeq(Guid[4]);
            writer.WriteString(Name);
            writer.WriteByteSeq(Guid[5]);
            writer.WriteByteSeq(plInvited.GuildGuid[3]);
            writer.WriteByteSeq(plInvited.GuildGuid[4]);

            plInvited.GetSession().Send(writer);
        }
        public void SendQuery(ref WorldSession session)
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildQueryResponse);

            writer.WriteUInt64(Guid);
            writer.WriteCString(Name);

            for (int i = 0; i < MaxRanks; ++i)
            {
                if (i < GetRankSize())
                    writer.WriteCString(RankList[i].Name);
                else
                    writer.WriteUInt8(0);
            }
            for (uint i = 0; i < MaxRanks; i++)
                writer.WriteUInt32(i);
            for (uint i = 0; i < MaxRanks; i++)
            {
                if (HasRank(i))
                    writer.WriteUInt32(RankList[(int)i].Order);//Rights Order - needs changed.
                else
                    writer.WriteUInt32(i);
            }

            writer.WriteUInt32(EmblemStyle);
            writer.WriteUInt32(EmblemColor);
            writer.WriteUInt32(BorderStyle);
            writer.WriteUInt32(BorderColor);
            writer.WriteUInt32(BackgroundColor);
            writer.WriteUInt32((uint)GetRankSize());

            session.Send(writer);
        }
        public void SendRoster(WorldSession session = null)
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildRoster);

            int size = GetMemberSize();
            writer.WriteBits(MOTD.Length, 11);
            writer.WriteBits(size, 18);

            foreach (Member pmember in MemberList)
            {
                var guid = pmember.CharGuid;

                writer.WriteBit(guid[3]);
                writer.WriteBit(guid[4]);
                writer.WriteBit(false); //Has Authenticator
                writer.WriteBit(false); //Can SoR
                writer.WriteBits(pmember.PublicNote.Length, 8);
                writer.WriteBits(pmember.OfficerNote.Length, 8);
                writer.WriteBit(guid[0]);
                writer.WriteBits(pmember.Name.Length, 7);
                writer.WriteBit(guid[1]);
                writer.WriteBit(guid[2]);
                writer.WriteBit(guid[6]);
                writer.WriteBit(guid[5]);
                writer.WriteBit(guid[7]);
            }
            writer.WriteBits(INFO.Length, 12);
            writer.BitFlush();
            foreach (Member pmember in MemberList)
            {
                Player pl = ObjMgr.FindPlayer(pmember.CharGuid);

                if (pl != null)
                {
                    var guid = new ObjectGuid(pl.GetGUIDLow());
                    writer.WriteUInt8(pl.getClass());
                    writer.WriteUInt32(0);//Guild Reputation
                    writer.WriteByteSeq(guid[0]);
                    writer.WriteUInt64(0);//week activity
                    writer.WriteUInt32(pmember.RankId);
                    writer.WriteUInt32(0);//GetAchievementPoints());

                    for (int j = 0; j < 2; j++)
                    {
                        writer.WriteUInt32(pmember.ProfessionList[j].Rank);
                        writer.WriteUInt32(pmember.ProfessionList[j].Level);
                        writer.WriteUInt32(pmember.ProfessionList[j].SkillId);
                    }

                    writer.WriteByteSeq(guid[2]);
                    writer.WriteUInt8((byte)GuildMemberFlags.Online);
                    writer.WriteUInt32(pl.GetZoneId());
                    writer.WriteUInt64(0);//total activity
                    writer.WriteByteSeq(guid[7]);
                    writer.WriteUInt32(0);// remaining guild week rep
                    writer.Write(pmember.PublicNote.ToCharArray());
                    writer.WriteByteSeq(guid[3]);
                    writer.WriteUInt8((byte)pl.getLevel());
                    writer.WriteUInt32(0);//unk
                    writer.WriteByteSeq(guid[5]);
                    writer.WriteByteSeq(guid[4]);
                    writer.WriteUInt8(0);//unk
                    writer.WriteByteSeq(guid[1]);
                    writer.WriteFloat(0);
                    writer.Write(pmember.OfficerNote.ToCharArray());
                    writer.WriteByteSeq(guid[6]);
                    writer.Write(pl.Name.ToCharArray());
                }
                else
                {
                    var guid = pmember.CharGuid;

                    writer.WriteUInt8(pmember.Class);
                    writer.WriteUInt32(0);//unk
                    writer.WriteByteSeq(guid[0]);
                    writer.WriteUInt64(0);//week activity
                    writer.WriteUInt32(pmember.RankId);//rank
                    writer.WriteUInt32(0);//GetAchievementPoints());

                    for (int j = 0; j < 2; j++)
                    {
                        writer.WriteUInt32(pmember.ProfessionList[j].Rank);
                        writer.WriteUInt32(pmember.ProfessionList[j].Level);
                        writer.WriteUInt32(pmember.ProfessionList[j].SkillId);
                    }
                    writer.WriteByteSeq(guid[2]);
                    writer.WriteUInt8((byte)GuildMemberFlags.Offline);
                    writer.WriteUInt32(pmember.ZoneId);
                    writer.WriteUInt64(0);//total activity
                    writer.WriteByteSeq(guid[7]);
                    writer.WriteUInt32(0);// remaining guild rep
                    writer.Write(pmember.PublicNote.ToCharArray());
                    writer.WriteByteSeq(guid[3]);
                    writer.WriteUInt8((byte)pmember.Level);
                    writer.WriteUInt32(0);//unk
                    writer.WriteByteSeq(guid[5]);
                    writer.WriteByteSeq(4);
                    writer.WriteUInt8(0);//unk
                    writer.WriteByteSeq(guid[1]);
                    writer.WriteFloat(Convert.ToSingle(new TimeSpan((DateTime.UtcNow.Ticks - (long)pmember.LogoutTime) / (int)TimeConstants.Day).Minutes));// ?? time(NULL)-itr2->second.LogoutTime) / DAY);
                    writer.Write(pmember.OfficerNote.ToCharArray());
                    writer.WriteByteSeq(guid[6]);
                    writer.Write(pmember.Name.ToCharArray());
                }
            }
            writer.Write(INFO.ToCharArray());
            writer.Write(MOTD.ToCharArray());
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);

            if (session != null)
                session.Send(writer);
            else
                BroadcastPacket(writer);
        }
        public void BroadcastPacket(PacketWriter writer)
        {
            foreach (Member member in MemberList)
            {
                Player pl = ObjMgr.FindPlayer(member.CharGuid);
                if (pl != null)
                    pl.GetSession().Send(writer);
            }
        }
        void SendCommandResult(ref WorldSession session, GuildCommandType type, GuildCommandErrors errCode, string param = "")
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildCommandResult2);
            writer.WriteUInt32((uint)type);
            writer.WriteCString(param);
            writer.WriteUInt32((uint)errCode);
            session.Send(writer);
        }

        public void SaveToDB()
        {

        }

        public void SetLeader(Member member)
        {
            this.LeaderGuid = member.CharGuid;
            member.ChangeRank((uint)GuildDefaultRanks.Master);
            DB.Characters.Execute("UPDATE guild SET leaderguid = {0} WHERE GuildGuid = {1}", member.CharGuid, Guid);
        }
        public void SetMOTD(string motd)
        {
            MOTD = motd;

            DB.Characters.Execute("UPDATE guild SET motd = '{0}' WHERE Guid = {1}", motd, Guid);

            SendBroadcastEvent(GuildEvents.MOTD, Guid, motd);
        }
        public void SetINFO(string info)
        {
            INFO = info;

            DB.Characters.Execute("UPDATE guild SET info = '{0}' WHERE Guid = {1}", info, Guid);
        }
        public ulong GetLeaderGuid() { return MemberList.First(p => p.RankId == (uint)GuildDefaultRanks.Master).CharGuid; }
        public uint GetLevel() { return Level; }
        public string GetGuildName() { return Name; }

        #region Fields
        //Private
        internal List<Member> MemberList = new List<Member>();
        internal List<Rank> RankList = new List<Rank>();
        internal List<EventLog> EventLogList = new List<EventLog>();
        internal List<BankTab> BankTabList = new List<BankTab>();

        public ObjectGuid Guid { get; set; }
        public uint Level { get; set; }
        public string Name { get; set; }
        public ulong LeaderGuid { get; set; }
        public string MOTD { get; set; }
        public string INFO { get; set; }
        public ulong BankMoney { get; set; }
        public ulong CreatedDate { get; set; }
        public ulong Experience { get; set; }
        public ulong TodayExperience { get; set; }
        public ulong NextLevelExperience { get; set; }
        public uint EmblemStyle { get; set; }
        public uint EmblemColor { get; set; }
        public uint BorderStyle { get; set; }
        public uint BorderColor { get; set; }
        public uint BackgroundColor { get; set; }
        #endregion
    }
}
