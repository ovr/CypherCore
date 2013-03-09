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
using AuthServer.Network;
using Framework.Configuration;
using Framework.Database;
using Framework.Logging;
using Framework.ObjectDefines;

namespace AuthServer
{
    class AuthServer
    {
        static void Main()
        {
            Log.Type = ServerType.Auth;

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

            Log.outInfo("Starting Cypher AuthServer...");

            DB.Auth.Init(AuthConfig.AuthDBHost, AuthConfig.AuthDBUser, AuthConfig.AuthDBPassword, AuthConfig.AuthDBDataBase, AuthConfig.AuthDBPort);
            
            Log.outInfo("Updating Realm List...");
            Log.outInit();

            PreparedStatement stmt = DB.Auth.GetPreparedStatement(LoginStatements.Sel_RealmList);
            SQLResult result = DB.Auth.Select(stmt);

            for (int i = 0; i < result.Count; i++)
            {
                AuthClass.Realms.Add(new Realm()
                {
                    Id = result.Read<uint>(i, 0),
                    Name = result.Read<string>(i, 1),
                    IP = result.Read<string>(i, 2),
                    Port = result.Read<uint>(i, 3),
                    Icon = result.Read<byte>(i, 4),
                    Flags = result.Read<byte>(i, 5),
                    TimeZone = result.Read<byte>(i, 6),
                    SecurityLevel = result.Read<uint>(i, 7),
                    Population = result.Read<uint>(i, 8),
                    GameBuild = result.Read<uint>(i, 9)
                });
                Log.outInfo("Added Realm \"{0}\"", result.Read<string>(i, 1));
            }
            Log.outInit();

            if (AuthClass.realm.Start(AuthConfig.BindIP, AuthConfig.BindPort))
            {
                AuthClass.realm.AcceptConnectionThread();
                Log.outInfo("AuthServer listening on {0} port {1}.", AuthConfig.BindIP, AuthConfig.BindPort);
                Log.outInfo("AuthServer successfully started!");
            }
            else
                Log.outError("AuthServer couldn't be started: ");
        }
    }
}
