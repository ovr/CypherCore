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
using System.Diagnostics;
using Framework.Utility;
using Framework.Network;
using System.Text;
using Framework.Constants;
using System.Linq;
using DefaultConsole = System.Console;

namespace Framework.Logging
{ 

    /*
    public class OldLog
    {
        static Logger logger;
        static OldLog()
        {
            logger = LogManager.GetCurrentClassLogger();
        }
        public static void InitLogger(bool auth = false)
        {   
            var config = new LoggingConfiguration();    
            var consoleTarget = new ColoredConsoleTarget();
            consoleTarget.Layout = "${message}";
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Info", ConsoleOutputColor.Green, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Warn", ConsoleOutputColor.White, ConsoleOutputColor.NoChange));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule("level == LogLevel.Debug", ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange));
            config.AddTarget("console", consoleTarget);

            if (!auth)
            {
                var fileTarget = new FileTarget();
                fileTarget.FileName = @"${basedir}/Logs/Server.log";
                fileTarget.Layout = @"${message}";
                fileTarget.DeleteOldFileOnStartup = true;
                config.AddTarget("file", fileTarget);

                var rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
                config.LoggingRules.Add(rule);
            }

            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);

            LogManager.Configuration = config;
        }

        public static void outDebug(string text, params object[] args)
        {
            text = text.Insert(0, "[" + DateTime.Now.ToLongTimeString() + "] Debug: "); 
            Debug(text, args);
        }
        public static void outError(string text, params object[] args)
        {
            text = text.Insert(0, "[" + DateTime.Now.ToLongTimeString() + "] Error: "); 
            logger.Error(text, args);
        }
        public static void outInfo(string text, params object[] args)
        {
            text = text.Insert(0, "[" + DateTime.Now.ToLongTimeString() + "] System: "); 
            logger.Info(text, args);
        }
        public static void outInfo()
        {
            logger.Info("");
        }
        public static void outWarn(string text, params object[] args)
        {
            text = text.Insert(0, "[" + DateTime.Now.ToLongTimeString() + "] Warn: ");
            logger.Info(text, args);
        }
        public static void outInit()
        {
            outInit("");
        }
        public static void outInit(string text, params object[] args)
        {
            logger.Info(text, args);
        }
        
        [Conditional("DEBUG")]
        static void Debug(string text, params object[] args)
        {
            logger.Debug(text, args);
        }

        public static void outException(Exception ex, bool terminating = false)
        {
            if (terminating)
                logger.Fatal("Method:{0} Error:{1}", ex.TargetSite.Name, ex.Message);
            else
                logger.Error("Method:{0} Error:{1}", ex.TargetSite.Name, ex.Message);
        }
    }

    */

    public class Log
    {
        public static ServerType Type { get; set; }


        public static void outDebug(string text, params object[] args)
        {
            text = text.Insert(0, "[" + DateTime.Now.ToLongTimeString() + "] Debug: ");
            Debug(string.Format(text, args));
        }
        
        [Conditional("DEBUG")]
        static void Debug(string text)
        {
            DefaultConsole.ForegroundColor = ConsoleColor.White;
            DefaultConsole.WriteLine(text);
        }

        public static void outError(string text, params object[] args)
        {
            text = text.Insert(0, "[" + DateTime.Now.ToLongTimeString() + "] Error: ");
            Write(string.Format(text, args), LogType.Error);
        }
        public static void outInfo(string text, params object[] args)
        {
            text = text.Insert(0, "[" + DateTime.Now.ToLongTimeString() + "] System: ");
            Write(string.Format(text, args), LogType.Info);
        }
        public static void outInfo()
        {
            Write("", LogType.Info);
        }
        public static void outWarn(string text, params object[] args)
        {
            text = text.Insert(0, "[" + DateTime.Now.ToLongTimeString() + "] Warn: ");
            Write(string.Format(text, args), LogType.Warn);
        }
        public static void outInit(string text, params object[] args)
        {
            Write(string.Format(text, args), LogType.Init);
        }
        public static void outInit()
        {
            Write("", LogType.Init);
        }
        public static void outException(Exception ex, bool terminating = false)
        {
            if (terminating)
                Write(string.Format("Method:{0} Error:{1}", ex.TargetSite.Name, ex.Message), LogType.Fatal);
            else
                Write(string.Format("Method:{0} Error:{1}", ex.TargetSite.Name, ex.Message), LogType.Error);
        }

        static void Write(string text, LogType logtype)
        {
            DefaultConsole.OutputEncoding = UTF8Encoding.UTF8;
            switch (logtype)
            { 
                case LogType.Error:
                    DefaultConsole.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.Info:
                case LogType.Init:
                    DefaultConsole.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogType.Warn:
                    DefaultConsole.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogType.Fatal: 
                    DefaultConsole.ForegroundColor = ConsoleColor.DarkBlue;
                    break;
                default:
                    DefaultConsole.ForegroundColor = ConsoleColor.White;
                    break;
            }
            DefaultConsole.WriteLine(text);
        }

    }
    enum LogFilterType
    {
        LOG_FILTER_GENERAL = 0,     // This one should only be used inside Log.cpp
        LOG_FILTER_UNITS = 1,     // Anything related to units that doesn't fit in other categories. ie. creature formations
        LOG_FILTER_PETS = 2,
        LOG_FILTER_VEHICLES = 3,
        LOG_FILTER_TSCR = 4,     // C++ AI, instance scripts, etc.
        LOG_FILTER_DATABASE_AI = 5,     // SmartAI, EventAI, Creature* * AI
        LOG_FILTER_MAPSCRIPTS = 6,
        LOG_FILTER_NETWORKIO = 7,
        LOG_FILTER_SPELLS_AURAS = 8,
        LOG_FILTER_ACHIEVEMENTSYS = 9,
        LOG_FILTER_CONDITIONSYS = 10,
        LOG_FILTER_POOLSYS = 11,
        LOG_FILTER_AUCTIONHOUSE = 12,
        LOG_FILTER_BATTLEGROUND = 13,
        LOG_FILTER_OUTDOORPVP = 14,
        LOG_FILTER_CHATSYS = 15,
        LOG_FILTER_LFG = 16,
        LOG_FILTER_MAPS = 17,
        LOG_FILTER_PLAYER = 18,     // Any player log that does not fit in other player filters
        LOG_FILTER_PLAYER_LOADING = 19,     // Debug output from Player::_Load functions
        LOG_FILTER_PLAYER_ITEMS = 20,
        LOG_FILTER_PLAYER_SKILLS = 21,
        LOG_FILTER_PLAYER_CHATLOG = 22,
        LOG_FILTER_LOOT = 23,
        LOG_FILTER_GUILD = 24,
        LOG_FILTER_TRANSPORTS = 25,
        LOG_FILTER_SQL = 26,
        LOG_FILTER_GMCOMMAND = 27,
        LOG_FILTER_REMOTECOMMAND = 28,
        LOG_FILTER_WARDEN = 29,
        LOG_FILTER_AUTHSERVER = 30,
        LOG_FILTER_WORLDSERVER = 31,
        LOG_FILTER_GAMEEVENTS = 32,
        LOG_FILTER_CALENDAR = 33,
        LOG_FILTER_CHARACTER = 34,
        LOG_FILTER_ARENAS = 35,
        LOG_FILTER_SQL_DRIVER = 36,
        LOG_FILTER_SQL_DEV = 37,
        LOG_FILTER_PLAYER_DUMP = 38,
        LOG_FILTER_BATTLEFIELD = 39,
        LOG_FILTER_SERVER_LOADING = 40,
        LOG_FILTER_OPCODES = 41,
        LOG_FILTER_SOAP = 42,
        LOG_FILTER_RBAC = 43
    };
    enum LogType
    {
        Debug,
        Error,
        Info,
        Warn,
        Init,
        Fatal
    }
    public enum ServerType
    {
        World,
        Auth
    }
}
