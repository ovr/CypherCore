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
using System.Threading;
using Framework.Configuration;
using Framework.Database;
using Framework.Logging;
using Framework.Network;
using WorldServer.Game;
using WorldServer.Game.Commands;
using WorldServer.Game.Managers;
using WorldServer.Network;
using WorldServer.Game.Packets;
using Framework.Utility;
using System.Diagnostics;

namespace WorldServer
{
    class WorldServer
    {
        static void Main()
        {
            //AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler; // Watch for any unhandled exceptions.
            var time = Time.getMSTime();
            //Log.InitLogger();
            Log.Type = ServerType.World;

            Log.outInit(@" ____                    __                      ");
            Log.outInit(@"/\  _`\                 /\ \                     ");
            Log.outInit(@"\ \ \/\_\  __  __  _____\ \ \___      __   _ __  ");
            Log.outInit(@" \ \ \/_/_/\ \/\ \/\ '__`\ \  _ `\  /'__`\/\`'__\");
            Log.outInit(@"  \ \ \L\ \ \ \_\ \ \ \L\ \ \ \ \ \/\  __/\ \ \/ ");
            Log.outInit(@"   \ \____/\/`____ \ \ ,__/\ \_\ \_\ \____\\ \_\ ");
            Log.outInit(@"    \/___/  `/___/> \ \ \/  \/_/\/_/\/____/ \/_/ ");
            Log.outInit(@"               /\___/\ \_\                       ");
            Log.outInit(@"               \/__/  \/_/                   Core");
            Log.outInit();

            Log.outInfo("Starting Cypher WorldServer...");

            Cypher.Initialize();

            DB.Characters.Init(WorldConfig.CharDBHost, WorldConfig.CharDBUser, WorldConfig.CharDBPassword, WorldConfig.CharDBDataBase, WorldConfig.CharDBPort);
            DB.Auth.Init(WorldConfig.AuthDBHost, WorldConfig.AuthDBUser, WorldConfig.AuthDBPassword, WorldConfig.AuthDBDataBase, WorldConfig.AuthDBPort);
            DB.World.Init(WorldConfig.WorldDBHost, WorldConfig.WorldDBUser, WorldConfig.WorldDBPassword, WorldConfig.WorldDBDataBase, WorldConfig.WorldDBPort);
            Log.outInit();

            Cypher.WorldMgr.InitDBLoads();

            if (!new WorldNetwork().Start(WorldConfig.BindIP, WorldConfig.BindPort))
            {
                Log.outError("Server couldn't be started.");
                WorldServer.ShutDownServer();
            }
            PacketManager.DefineOpcodeHandler();

            Log.outInfo("WorldServer listening on {0} port {1}.", WorldConfig.BindIP, WorldConfig.BindPort);
            Log.outInfo("WorldServer successfully started in {0}s", Time.getMSTimeDiff(time, Time.getMSTime()) / 1000);

            //starts World Update.
            Cypher.WorldMgr.Run();

            CommandManager.InitConsole();
        }

        /// <summary>
        /// Unhandled exception emitter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Log.outError(ex.Message);

            Console.ReadLine();
        }

        public static void ShutDownServer(int delay = 5000)
        {
            Log.outError("Shutting down.....");
            Thread.Sleep(delay);
            Environment.Exit(0);
        }
    }
}
