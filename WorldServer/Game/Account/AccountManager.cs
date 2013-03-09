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
using System.Threading.Tasks;
using Framework.Singleton;
using Framework.Database;
using Framework.Constants;

namespace WorldServer.Game
{
    public sealed class AccountManager : SingletonBase<AccountManager>
    {
        AccountManager() { }

        public void CreateAccount(string name, string password, string email)
        {
        }

        public uint GetId(string username)
        {
            PreparedStatement stmt = DB.Auth.GetPreparedStatement(LoginStatements.Get_AccountIDByUsername);
            stmt.AddValue(0, username);
            SQLResult result = DB.Auth.Select(stmt);

            return result.Count != 0 ? result.Read<uint>(0, 0) : 0;
        }

        public uint GetSecurity(uint accountId)
        {
            PreparedStatement stmt = DB.Auth.GetPreparedStatement(LoginStatements.Get_account_accessGMLevel);
            stmt.AddValue(0, accountId);
            SQLResult result = DB.Auth.Select(stmt);

            return (result.Count != 0) ? result.Read<byte>(0, 0) : (byte)AccountTypes.Player;
        }
        public uint GetSecurity(uint accountId, int realmId)
        {
            PreparedStatement stmt = DB.Auth.GetPreparedStatement(LoginStatements.Get_GMLevelByRealmID);
            stmt.AddValue(0, accountId);
            stmt.AddValue(1, realmId);
            SQLResult result = DB.Auth.Select(stmt);

            return result.Count != 0 ? result.Read<uint>(0, 0) : (uint)AccountTypes.Player;
        }
        public bool IsConsoleAccount(AccountTypes gmlevel)
        {
            return gmlevel == AccountTypes.Console;
        }

    }
}
