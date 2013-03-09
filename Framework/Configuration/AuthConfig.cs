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

using System.IO;
using System.Reflection;

namespace Framework.Configuration
{
    public static class AuthConfig
    {
        static Config config = new Config(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/AuthServer.conf");

        public static string AuthDBHost = config.Read("AuthDB.Host", "");
        public static int AuthDBPort = config.Read("AuthDB.Port", 3306);
        public static string AuthDBUser = config.Read("AuthDB.User", "");
        public static string AuthDBPassword = config.Read("AuthDB.Password", "");
        public static string AuthDBDataBase = config.Read("AuthDB.Database", "");

        public static string BindIP = config.Read("BindIP", "");
        public static int BindPort = config.Read("BindPort", 3724);
    }
}
