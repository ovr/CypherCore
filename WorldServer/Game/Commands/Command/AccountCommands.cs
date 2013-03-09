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
using Framework.Cryptography;
using Framework.Database;
using Framework.Utility;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;
using System;
using System.Linq;
using Framework.Configuration;

namespace WorldServer.Game.Commands
{
    [CommandGroup("account", "Syntax: .account (Display access level of your account.)", AccountTypes.Player)]
    public class AccountCommands : CommandGroup
    {
        [Command("create", "Syntax: .account create $name $password (Create account and set password to it.)", AccountTypes.Console, true)]
        public static bool Create(string[] args, CommandGroup command)
        {
            if (args.Length < 2)
                return false;

            var accountName = args[0].ToUpper();
            var password = args[1];

            if (accountName == string.Empty || password == string.Empty)
                return false;

            if (!accountName.Contains("@"))
                return command.SendErrorMessage("Account name requires an email address");

            if (accountName.Length > 16)
                return command.SendErrorMessage(CypherStrings.TooLongName);

            PreparedStatement stmt = DB.Auth.GetPreparedStatement(LoginStatements.Get_AccountIDByUsername);
            stmt.AddValue(0, accountName);
            SQLResult result = DB.Auth.Select(stmt);

            if (result.Count != 0)
                return command.SendSysMessage(CypherStrings.AccountAlreadyExist, accountName);

            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Ins_Account);
            stmt.AddValue(0, accountName);
            stmt.AddValue(1, SRP6.GenerateCredentialsHash(accountName, password).ToHexString());
            DB.Auth.Execute(stmt);

            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Ins_RealmCharactersInit);
            DB.Auth.Execute(stmt);

            return command.SendSysMessage(CypherStrings.AccountCreated, accountName);
        }

        [Command("delete", "Syntax: .account delete $account (Delete account with all characters.)", AccountTypes.Console, true)]
        public static bool Delete(string[] args, CommandGroup command)
        {
            if (args.Length < 1)
                return false;

            string account = args[0];

            if (account == string.Empty)
                return false;

            if (!account.Contains("@"))
                return command.SendSysMessage("Account name requires an email address");

            PreparedStatement stmt = DB.Auth.GetPreparedStatement(LoginStatements.Get_AccountIDByUsername);
            stmt.AddValue(0, account);
            SQLResult result = DB.Auth.Select(stmt);


            if (result.Count == 0)
                command.SendErrorMessage(CypherStrings.AccountNotExist, account);

            uint accountId = result.Read<uint>(0, 0);

            stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_CHARS_BY_ACCOUNT_ID);
            stmt.AddValue(0, accountId);
            result = DB.Characters.Select(stmt);

            if (result.Count != 0)
            {
                for (var i = 0; i < result.Count; i++)
                {
                    uint guidLow = result.Read<uint>(i, 0);

                    // Kick if player is online
                    Player pl = ObjMgr.FindPlayer(guidLow);
                    if (pl != null)
                    {
                        WorldSession s = pl.GetSession();
                        s.KickPlayer();                            // mark session to remove at next session list update
                        //s.LogoutPlayer(false);                     // logout player without waiting next session list update
                    }

                    //Player::DeleteFromDB(guid, accountId, false);       // no need to update realm characters todo fixme
                }
            }

            stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_TUTORIALS);
            stmt.AddValue(0, accountId);
            DB.Characters.Execute(stmt);

            stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_ACCOUNT_DATA);
            stmt.AddValue(0, accountId);
            DB.Characters.Execute(stmt);

            stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_CHARACTER_BAN);
            stmt.AddValue(0, accountId);
            DB.Characters.Execute(stmt);

            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Del_Account);
            stmt.AddValue(0, accountId);
            DB.Auth.Execute(stmt);

            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Del_account_access);
            stmt.AddValue(0, accountId);
            DB.Auth.Execute(stmt);

            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Del_RealmCharacters);
            stmt.AddValue(0, accountId);
            DB.Auth.Execute(stmt);

            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Del_AccountBanned);
            stmt.AddValue(0, accountId);
            DB.Auth.Execute(stmt);

            return command.SendSysMessage(CypherStrings.AccountDeleted, account);
        }

        [Command("addon", "", AccountTypes.Moderator)]
        public static bool Addon(string[] args, CommandGroup command)
        {
            return false;
        }

        [Command("lock", "Syntax: .account lock [on|off]\r\n(Allow login from account only from current used IP or remove this requirement.)", AccountTypes.Player)]
        public static bool Lock(string[] args, CommandGroup command)
        {
            return false;
        }

        [Command("online", "Syntax: .account onlinelist (Show list of online accounts.)", AccountTypes.Console, true)]
        public static bool OnlineList(string[] args, CommandGroup command)
        {
            return false;
        }

        [Command("password", "Syntax: .account password $old_password $new_password $new_password (Change your account password.)", AccountTypes.Player)]
        public static bool Password(string[] args, CommandGroup command)
        {
            return false;
        }

        [CommandGroup("set", "Syntax: .account set $subcommand Type .account set to see the list of possible subcommands or .help account set $subcommand to see info on subcommands", AccountTypes.Administrator, true)]
        public class SetCommands : CommandGroup
        {
            [Command("password", "Testing set password help", AccountTypes.Console, true)]
            public static bool SetPassword(string[] args, CommandGroup command)
            {
                return false;
            }

            [Command("addon", "", AccountTypes.Administrator, true)]
            public static bool SetAddon(string[] args, CommandGroup command)
            {
                return false;
            }
            [Command("gmlevel", "", AccountTypes.Console, true)]
            public static bool SetGMLevel(string[] args, CommandGroup command)
            {
                if (args.Length < 3)
                    return false;

                string targetAccountName = "";
                uint targetAccountId = 0;
                uint targetSecurity = 0;
                uint gm = 0;
                string arg1 = args[0];
                string arg2 = args[1];
                string arg3 = args[2];
                bool isAccountNameGiven = true;

                if (!string.IsNullOrEmpty(arg1) && string.IsNullOrEmpty(arg3))
                {
                    if (command.getSelectedPlayer() == null)
                        return false;
                    isAccountNameGiven = false;
                }

                // Check for second parameter
                if (!isAccountNameGiven && !string.IsNullOrEmpty(arg2))
                    return false;

                // Check for account
                if (isAccountNameGiven)
                {
                    targetAccountName = arg1;
                    if (!targetAccountName.IsNormalized())//need checked
                    {
                        command.SendErrorMessage(CypherStrings.AccountNotExist, targetAccountName);
                        return false;
                    }
                }

                // Check for invalid specified GM level.
                gm = isAccountNameGiven ? uint.Parse(arg2) : uint.Parse(arg1);
                if (gm > (uint)AccountTypes.Console)
                {
                    command.SendErrorMessage(CypherStrings.BadValue);
                    return false;
                }

                // handler->getSession() == NULL only for console
                targetAccountId = (isAccountNameGiven) ? AcctMgr.GetId(targetAccountName) : command.getSelectedPlayer().GetSession().GetAccountId();
                int gmRealmID = (isAccountNameGiven) ? int.Parse(arg3) : int.Parse(arg2);
                uint playerSecurity;
                if (command.GetSession() != null)
                    playerSecurity = AcctMgr.GetSecurity(command.GetSession().GetAccountId(), gmRealmID);
                else
                    playerSecurity = (uint)AccountTypes.Console;

                // can set security level only for target with less security and to less security that we have
                // This is also reject self apply in fact
                targetSecurity = AcctMgr.GetSecurity(targetAccountId, gmRealmID);
                if (targetSecurity >= playerSecurity || gm >= playerSecurity)
                {
                    command.SendErrorMessage(CypherStrings.YoursSecurityIsLow);
                    return false;
                }
                PreparedStatement stmt;
                // Check and abort if the target gm has a higher rank on one of the realms and the new realm is -1
                if (gmRealmID == -1 && !AcctMgr.IsConsoleAccount((AccountTypes)playerSecurity))
                {
                    stmt = DB.Auth.GetPreparedStatement(LoginStatements.Sel_account_accessGMLevelTest);
                    stmt.AddValue(0, targetAccountId);
                    stmt.AddValue(1, gm);

                    SQLResult result = DB.Auth.Select(stmt);

                    if (result.Count > 0)
                    {
                        command.SendErrorMessage(CypherStrings.YoursSecurityIsLow);
                        return false;
                    }
                }

                // Check if provided realmID has a negative value other than -1
                if (gmRealmID < -1)
                {
                    command.SendErrorMessage(CypherStrings.InvalidRealmid);
                    return false;
                }

                // If gmRealmID is -1, delete all values for the account id, else, insert values for the specific realmID
                if (gmRealmID == -1)
                {
                    stmt = DB.Auth.GetPreparedStatement(LoginStatements.Del_account_access);
                    stmt.AddValue(0, targetAccountId);
                }
                else
                {
                    stmt = DB.Auth.GetPreparedStatement(LoginStatements.Del_account_accessByRealm);

                    stmt.AddValue(0, targetAccountId);
                    stmt.AddValue(1, WorldConfig.RealmId);
                }
                DB.Auth.Execute(stmt);

                if (gm != 0)
                {
                    stmt = DB.Auth.GetPreparedStatement(LoginStatements.Ins_account_access);

                    stmt.AddValue(0, targetAccountId);
                    stmt.AddValue(1, gm);
                    stmt.AddValue(2, gmRealmID);

                    DB.Auth.Execute(stmt);
                }
                command.SendSysMessage(CypherStrings.YouChangeSecurity, targetAccountName, gm);
                return true;
            }
        }
    }
}
