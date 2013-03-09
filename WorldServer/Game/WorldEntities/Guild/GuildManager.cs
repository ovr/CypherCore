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
using Framework.Logging;
using Framework.Singleton;
using Framework.ObjectDefines;

namespace WorldServer.Game.WorldEntities
{
    public struct GuildRewards
    {
        public uint Entry;
        public ulong Price;
        public uint Achievement;
        public uint Standing;
        public uint Races;
    }
    public sealed class GuildManager : SingletonBase<GuildManager>
    {
        Dictionary<UInt64, Guild> guildList;
        public List<GuildRewards> guildRewards;
        public Dictionary<uint, ulong> guildXPPerLevel;

        GuildManager()
        {
            guildList = new Dictionary<UInt64, Guild>();
            guildRewards = new List<GuildRewards>();
            guildXPPerLevel = new Dictionary<uint, ulong>();
        }
        
        public bool CreateGuild(Player leader, string name)
        {
            if (Convert.ToBoolean(GetGuildByName(name)))
                return false;

            ObjectGuid newGuid = new ObjectGuid(Cypher.ObjMgr.GenerateLowGuid(HighGuidType.Guild));
            Guild guild = new Guild()
            {
                Guid = newGuid,
                Level = 1,
                Name = name,
                LeaderGuid = leader.GetGUIDLow(),
                INFO = "",
                MOTD = "No Message Set",
                BankMoney = 0,
                CreatedDate = 0,
                Experience = 0,
                TodayExperience = 0,
                NextLevelExperience = 0,//need to add next level xp.
            };
            guildList.Add(newGuid, guild);

            guild.CreateDefaultRanks();
            guild.AddMember(leader.GetGUIDLow(), (int)GuildDefaultRanks.Master);

            return true;
        }
        public void DeleteGuild(ulong guid)
        {
            guildList.Remove(guid);
        }

        public Guild GetGuildByGuid(ulong guildGuid)
        {
            if (guildList.Any(p => p.Key == guildGuid))
                return guildList[guildGuid];

            return null;
        }
        public Guild GetGuildByName(string name)
        {
            foreach (KeyValuePair<ulong,Guild> pair in guildList)
            {
                if (pair.Value.Name.Equals(name))
                    return guildList[pair.Key];
            }
            return null;
        }

        public string GetGuildName(ulong guildGuid)
        {
            if (guildList.Keys.Any(p => p == guildGuid))
                return guildList[guildGuid].Name;

            return "";
        }

        public ulong GetXPForGuildLevel(uint level)
        {
            if (level < guildXPPerLevel.Count)
                return guildXPPerLevel[level];
            return 0;
        }

        public void LoadGuilds()
        {
            SQLResult result = DB.Characters.Select("SELECT * FROM guild ORDER BY guid");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 guilds. DB table `guild` is empty.");
                Log.outInit();
                return;
            }

            uint count = 0;
            for (int i = 0; i < result.Count; i++)
            {
                ulong guid = result.Read<ulong>(i, 0);
                Guild g = new Guild()
                {
                    Guid = new ObjectGuid(guid),
                    Name = result.Read<string>(i, "Name"),
                    LeaderGuid = result.Read<uint>(i, "leaderguid"),
                    EmblemStyle = result.Read<uint>(i, "EmblemStyle"),
                    EmblemColor = result.Read<uint>(i, "EmblemColor"),
                    BorderStyle = result.Read<uint>(i, "BorderStyle"),
                    BorderColor = result.Read<uint>(i, "BorderColor"),
                    BackgroundColor = result.Read<uint>(i, "BackgroundColor"),
                    INFO = result.Read<string>(i, "Info"),
                    MOTD = result.Read<string>(i, "Motd"),
                    CreatedDate = result.Read<ulong>(i, "Createdate"),
                    BankMoney = result.Read<uint>(i, "BankMoney"),
                    Level = result.Read<uint>(i, "Level"),
                    Experience = result.Read<uint>(i, "Experience"),
                    TodayExperience = result.Read<ulong>(i, "Today_Experience"),
                    NextLevelExperience = result.Read<ulong>(i, "Experience_Cap")
                };                
                guildList.Add(guid, g);
                count++;
            }

            LoadGuildRanks();
            LoadGuildMembers();
            LoadGuildBankTab();
            LoadGuildBankTabRights();
            LoadGuildRewards();
            LoadGuildXpForLevel();
            Validate();

            Log.outInfo("Loaded {0} Guilds", count);
            Log.outInit();
        }
        private void LoadGuildRanks()
        {
            DB.Characters.Execute("DELETE gr FROM guild_ranks gr LEFT JOIN guild g ON gr.GuildGuid = g.Guid WHERE g.Guid IS NULL");

            SQLResult result = DB.Characters.Select("SELECT GuildGuid, RankId, Name, Rights, BankMoneyPerDay, rankOrder FROM guild_ranks ORDER BY guildGuid ASC, RankId ASC");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 guild ranks. DB table `guild_ranks` is empty.");
                return;
            }

            for (int i = 0; i < result.Count; i++)
            {
                ulong guildGuid = result.Read<ulong>(i, 0);
                Guild guild = GetGuildByGuid(guildGuid);
                if (guild == null)
                    continue;

                Guild.Rank r = new Guild.Rank();
                uint rankId = result.Read<uint>(i, 1);
                r.RankId = rankId;
                r.Name = result.Read<string>(i, 2);
                r.Rights = result.Read<uint>(i, 3);
                r.BankMoneyPerDay = result.Read<uint>(i, 4);
                r.Order = result.Read<uint>(i, 5);

                if (r.RankId == (uint)GuildDefaultRanks.Master)
                    r.Rights |= (uint)GuildRankRights.All;

                guild.RankList.Add(r);
            }
        }
        private void LoadGuildMembers()
        {
            //guid, rank, pnote, offnote, BankResetTimeMoney, BankRemMoney, BankResetTimeTab0, BankRemSlotsTab0, BankResetTimeTab1, BankRemSlotsTab1, BankResetTimeTab2, BankRemSlotsTab2,
            //BankResetTimeTab3, BankRemSlotsTab3, BankResetTimeTab4, BankRemSlotsTab4, BankResetTimeTab5, BankRemSlotsTab5, BankResetTimeTab6, BankRemSlotsTab6, BankResetTimeTab7, BankRemSlotsTab7,
            //FirstProffLevel, FirstProffSkill, FirstProffRank, SecondProffLevel, SecondProffSkill, SecondProffRank
            SQLResult result = DB.Characters.Select("SELECT gm.*, c.name, c.level, c.class, c.zone FROM guild_members gm LEFT JOIN characters c ON c.guid = gm.CharGuid ORDER by gm.GuildGuid ASC");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 guild_members. DB table `guild_members` is empty.");
                return;
            }

            for (int i = 0; i < result.Count; i++)
            {
                Guild g = GetGuildByGuid(result.Read<ulong>(i, "GuildGuid"));
                if (g == null)
                    continue;

                Guild.Member m = new Guild.Member();
                m.CharGuid = new ObjectGuid(result.Read<ulong>(i, 1));
                m.RankId = result.Read<uint>(i, 2);
                m.PublicNote = result.Read<string>(i, 3);
                m.OfficerNote = result.Read<string>(i, 4);
                m.BankResetTimeMoney = result.Read<uint>(i, 5);
                m.BankRemainingMoney = result.Read<uint>(i, 6);

                for (int j = 0; j < Guild.MaxBankTabs; j++)
                {
                    Guild.Member.BankTabs tab = new Guild.Member.BankTabs();
                    tab.ResetTime = result.Read<uint>(i, (byte)(7 + (j * 2)));
                    tab.SlotsLeft = result.Read<uint>(i, (byte)(8 + (j * 2)));
                    m.BanksTabs.Add(tab);
                }

                for (int k = 0; k < 2; k++)
                {
                    Guild.Member.Profession prof = new Guild.Member.Profession();
                    prof.Level = result.Read<uint>(i, (byte)(23 + (k * 3)));
                    prof.SkillId = result.Read<uint>(i, (byte)(24 + (k * 3)));
                    prof.Rank = result.Read<uint>(i, (byte)(25 + (k * 3)));
                    m.ProfessionList.Add(prof);
                }

                m.Name = result.Read<string>(i, 29);
                m.Level = result.Read<Byte>(i, 30);
                m.Class = result.Read<Byte>(i, 31);
                m.ZoneId = result.Read<uint>(i, 32);
                //m.LogoutTime = result.Read<ulong>(i, 33);
                m.Flags = (uint)GuildMemberFlags.Online;

                g.MemberList.Add(m);
            }
        }
        private void LoadGuildBankTab()
        {
            SQLResult result = DB.Characters.Select("SELECT GuildGuid, TabId, Name, Icon, Text FROM guild_bank_tab ORDER BY GuildGuid, TabId ASC");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 guild banks tabs. DB table `guild_bank_tab` is empty.");
                return;
            }

            for (int i = 0; i < result.Count; i++)
            {
                ulong guildGuid = result.Read<ulong>(i, "GuildGuid");
                Guild guild = GetGuildByGuid(guildGuid);
                if (guild == null)
                    continue;

                Guild.BankTab tab = new Guild.BankTab()
                {
                    TabId = result.Read<uint>(i, "TabId"),
                    Name = result.Read<string>(i, "Name"),
                    Icon = result.Read<string>(i, "Icon"),
                    Text = result.Read<string>(i, "Text")
                
                };
                guild.BankTabList.Add(tab);
            }
        }
        private void LoadGuildBankTabRights()
        {
            SQLResult result = DB.Characters.Select("SELECT GuildGuid, TabId, RankId, Rights, SlotPerDay FROM guild_bank_rights ORDER BY GuildGuid ASC, TabId ASC");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 guild bank rights. DB table `guild_bank_rights` is empty.");
                return;
            }

            for (int i = 0; i < result.Count; i++)
            {
                ulong guildGuid = result.Read<ulong>(i, "GuildGuid");
                Guild guild = GuildManager.GetInstance().GetGuildByGuid(guildGuid);
                if (guild == null)
                    continue;

                uint TabId = result.Read<uint>(i, "TabId");
                uint rankId = result.Read<uint>(i, "RankId");
                ushort right = result.Read<ushort>(i, "Rights");
                ushort SlotPerDay = result.Read<ushort>(i, "SlotPerDay");

                guild.SetBankRightsAndSlots(rankId, TabId, right, SlotPerDay);
            }
        }
        private void Validate()
        {
            foreach (Guild guild in guildList.Values)
            {
                if (guild.GetRankSize() < Guild.MinRanks || guild.GetRankSize() > Guild.MaxRanks)
                {
                    Log.outError("Guild {0} has invalid number of ranks, Creating new...", guild.Guid);
                    guild.RankList.Clear();
                    guild.CreateDefaultRanks();
                }

                List<Guild.Member> brokenrank = guild.MemberList.Where(m => m.RankId > guild.GetRankSize()).ToList();
                foreach (Guild.Member member in brokenrank)
                    member.ChangeRank((uint)(guild.GetLowestRank()));

                Guild.Member leader = guild.GetMember(guild.GetLeaderGuid());
                if (leader == null)
                {
                    guild.DeleteMember(guild.GetLeaderGuid());

                    if (guild.MemberList.Count == 0)
                    {
                        guild.Disband();
                        return;
                    }
                }
                else if (leader.RankId != (uint)GuildDefaultRanks.Master)
                    guild.SetLeader(leader);
            }
        }

        private void LoadGuildRewards()
        {
            guildRewards.Clear();
            SQLResult result = DB.World.Select("SELECT item_entry, price, achievement, standing, races  FROM guild_rewards ORDER by achievement ASC");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 guild_rewards. DB table `guild_rewards` is empty.");
                return;
            }

            for (int i = 0; i < result.Count; i++)
            {
                GuildRewards reward = new GuildRewards()
                {
                    Entry = result.Read<uint>(i, "item_entry"),
                    Price = result.Read<ulong>(i, "price"),
                    Achievement = result.Read<uint>(i, "achievement"),
                    Standing = result.Read<uint>(i, "standing"),
                    Races = result.Read<uint>(i, "races")
                };

                if (Cypher.ObjMgr.GetItemTemplate(reward.Entry) == null)
                {
                    Log.outError("Guild rewards constains not existing item entry {0}", reward.Entry);
                    continue;
                }

                //if (reward.Achievement != 0 && (!sAchievementStore.LookupEntry(reward.AchievementId)))
                //{
                    //Log.outError("Guild rewards constains not existing achievement entry {0}", reward.Achievement);
                    //continue;
                //}

                if (reward.Standing >= 8)
                {
                    Log.outError("Guild rewards contains wrong reputation standing {0}, max is {1}", reward.Standing, 8 - 1);
                    continue;
                }

                guildRewards.Add(reward);
            }
        }
        private void LoadGuildXpForLevel()
        {
            guildXPPerLevel.Clear();
            SQLResult result = DB.World.Select("SELECT level, xp_next_level FROM guild_xp_for_level ORDER by level ASC");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 Guild level definitions. DB table `guild_xp_for_level` is empty.");
                return;
            }

            for (int i = 0; i < result.Count; i++)
            {
                uint level = result.Read<uint>(i, "level");
                if (level >= WorldConfig.GuildMaxLevel)
                {
                    Log.outError("Unused (> Guild.MaxLevel in worldserver.conf) level {0} in `guild_xp_for_level` table, ignoring.", level);
                    continue;
                }
                guildXPPerLevel.Add(level, result.Read<ulong>(i, "xp_next_level"));
            }
        }
    }
}
