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

using System.Reflection;
using System.IO;

namespace Framework.Configuration
{
    public struct WorldConfig
    {
        static Config config = new Config(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/WorldServer.conf");

        public static string AuthDBHost = config.Read("AuthDB.Host", "");
        public static int AuthDBPort = config.Read("AuthDB.Port", 3306);
        public static string AuthDBUser = config.Read("AuthDB.User", "");
        public static string AuthDBPassword = config.Read("AuthDB.Password", "");
        public static string AuthDBDataBase = config.Read("AuthDB.Database", "");

        public static string CharDBHost = config.Read("CharDB.Host", "");
        public static int CharDBPort = config.Read("CharDB.Port", 3306);
        public static string CharDBUser = config.Read("CharDB.User", "");
        public static string CharDBPassword = config.Read("CharDB.Password", "");
        public static string CharDBDataBase = config.Read("CharDB.Database", "");

        public static string WorldDBHost = config.Read("WorldDB.Host", "");
        public static int WorldDBPort = config.Read("WorldDB.Port", 3306);
        public static string WorldDBUser = config.Read("WorldDB.User", "");
        public static string WorldDBPassword = config.Read("WorldDB.Password", "");
        public static string WorldDBDataBase = config.Read("WorldDB.Database", "");

        public static uint RealmId = config.Read<uint>("RealmId", 1);

        public static string DataPath = config.Read("DataPath", "./Data/");

        public static string BindIP = config.Read("BindIP", "");
        public static int BindPort = config.Read("BindPort", 8085);

        public static uint MaxLevel = config.Read<uint>("MaxLevel", 85);
        
        //Creature
        public static float CreatureThreatRadius = config.Read<float>("ThreatRadius", 60f);
        public static float CreatureAggroRadius = config.Read<float>("Rate.Creature.Aggro", 1f);
        public static float CreatureFamilyFleeAssistanceRadius = config.Read<float>("CreatureFamilyFleeAssistanceRadius", 30f);
        public static float CreatureFamilyAssistanceRadius = config.Read<float>("CreatureFamilyAssistanceRadius", 10f);
        public static int CreatureFamilyAssistanceDelay = config.Read("CreatureFamilyAssistanceDelay", 1500);
        public static int CreatureFamilyFleeDelay = config.Read("CreatureFamilyFleeDelay", 7000);

        //Guild
        public static int GuildLevelingEnabled = config.Read("Guild.LevelingEnabled", 1);
        public static int GuildSaveInterval = config.Read("Guild.SaveInterval", 15);
        public static int GuildMaxLevel = config.Read("Guild.MaxLevel", 25);
        public static int GuildUndeletableLevel = config.Read("Guild.UndeletableLevel", 4);
        public static float GuildXPModifier = config.Read("Guild.XPModifier", 0.25f);
        public static int GuildDailyXPCap = config.Read("Guild.DailyXPCap", 7807500);
        public static int GuildWeeklyReputationCap = config.Read("Guild.WeeklyReputationCap", 4375);

        public static int GridCleanUpDelay = config.Read("GridCleanUpDelay", 300000);
        public static int MapUpdateInterval = config.Read("MapUpdateInterval", 100);
    }
}
