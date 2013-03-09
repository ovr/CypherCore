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

namespace Framework.Database
{
    public struct LoginStatements
    {
        public const string Sel_RealmList = "SELECT id, name, address, port, icon, flag, timezone, allowedSecurityLevel, population, gamebuild FROM realmlist WHERE flag <> 3 ORDER BY name";
        public const string Del_ExpiredIPBans = "DELETE FROM ipBanned WHERE unbandate<>bandate AND unbandate<=UNIX_TIMESTAMP()";
        public const string Upd_ExpiredAccountBans = "UPDATE accountBanned SET active = 0 WHERE active = 1 AND unbandate<>bandate AND unbandate<=UNIX_TIMESTAMP()";
        public const string Sel_IPBanned = "SELECT * FROM ipBanned WHERE ip = ?";
        public const string Ins_IPAutoBanned = "INSERT INTO ipBanned VALUES (?, UNIX_TIMESTAMP(), UNIX_TIMESTAMP()+?, 'Cypher realmd', 'Failed login autoban')";
        public const string Sel_IPBannedAll = "SELECT ip, bandate, unbandate, bannedby, banreason FROM ipBanned WHERE (bandate = unbandate OR unbandate > UNIX_TIMESTAMP()) ORDER BY unbandate";
        public const string Sel_IPBannedByIP = "SELECT ip, bandate, unbandate, bannedby, banreason FROM ipBanned WHERE (bandate = unbandate OR unbandate > UNIX_TIMESTAMP()) AND ip LIKE CONCAT('%%', ?, '%%') ORDER BY unbandate";
        public const string Sel_AccountBanned = "SELECT bandate, unbandate FROM accountBanned WHERE id = ? AND active = 1";
        public const string Sel_AccountBannedAll = "SELECT account.id, username FROM account, accountBanned WHERE account.id = accountBanned.id AND active = 1 GROUP BY account.id";
        public const string Sel_AccountBannedByUsername = "SELECT account.id, username FROM account, accountBanned WHERE account.id = accountBanned.id AND active = 1 AND username LIKE CONCAT('%%', ?, '%%') GROUP BY account.id";
        public const string Ins_AccountAutoBanned = "INSERT INTO accountBanned VALUES (?, UNIX_TIMESTAMP(), UNIX_TIMESTAMP()+?, 'Cypher realmd', 'Failed login autoban', 1)";
        public const string Del_AccountBanned = "DELETE FROM accountBanned WHERE id = ?";
        public const string Sel_SessionKey = "SELECT a.sessionkey, a.id, aa.gmlevel  FROM account a LEFT JOIN account_access aa ON (a.id = aa.id) WHERE username = ?";
        //public const string Upd_VS = "UPDATE account SET v = ?, s = ? WHERE username = ?";
        public const string Upd_LogonProof = "UPDATE account SET sessionkey = ?, last_ip = ?, last_login = NOW(), locale = ?, failed_logins = 0 WHERE username = ?";
        public const string Sel_LogonChallenge = "SELECT a.sha_pass_hash, a.id, a.locked, a.last_ip, aa.gmlevel FROM account a LEFT JOIN account_access aa ON (a.id = aa.id) WHERE a.username = ?";
        public const string Upd_FailedLogins = "UPDATE account SET failed_logins = failed_logins + 1 WHERE username = ?";
        public const string Sel_FailedLogins = "SELECT id, failed_logins FROM account WHERE username = ?";
        public const string Sel_AccountIDByName = "SELECT id FROM account WHERE username = ?";
        public const string Sel_AccountListByName = "SELECT id, username FROM account WHERE username = ?";
        public const string Sel_AccountInfoByName = "SELECT id, sha_pass_hash, sessionkey, last_ip, locked, expansion, mutetime, locale, recruiter FROM account WHERE username = ?";
        public const string Sel_AccountListByEmail = "SELECT id, username FROM account WHERE email = ?";
        public const string Sel_NumCharsOnRealm = "SELECT numchars FROM realmcharacters WHERE realmid = ? AND acctid= ?";
        public const string Sel_AccountByIP = "SELECT id, username FROM account WHERE last_ip = ?";
        public const string Sel_AccountByID = "SELECT 1 FROM account WHERE id = ?";
        public const string Ins_IPBanned = "INSERT INTO ipBanned VALUES (?, UNIX_TIMESTAMP(), UNIX_TIMESTAMP()+?, ?, ?)";
        public const string Del_IPNotBanned = "DELETE FROM ipBanned WHERE ip = ?";
        public const string Ins_AccountBanned = "INSERT INTO accountBanned VALUES (?, UNIX_TIMESTAMP(), UNIX_TIMESTAMP()+?, ?, ?, 1)";
        public const string Upd_AccountNotBanned = "UPDATE accountBanned SET active = 0 WHERE id = ? AND active != 0";
        public const string Del_RealmCharactersByRealm = "DELETE FROM realmcharacters WHERE acctid = ? AND realmid = ?";
        public const string Del_RealmCharacters = "DELETE FROM realmcharacters WHERE acctid = ?";
        public const string Ins_RealmCharacters = "INSERT INTO realmcharacters (numchars, acctid, realmid) VALUES (?, ?, ?)";
        public const string Sel_SumRealmCharacters = "SELECT SUM(numchars) FROM realmcharacters WHERE acctid = ?";
        public const string Ins_Account = "INSERT INTO account(username, sha_pass_hash, joindate) VALUES(?, ?, NOW())";
        public const string Ins_RealmCharactersInit = "INSERT INTO realmcharacters (realmid, acctid, numchars) SELECT realmlist.id, account.id, 0 FROM realmlist, account LEFT JOIN realmcharacters ON acctid=account.id WHERE acctid IS NULL";
        public const string Upd_Expansion = "UPDATE account SET expansion = ? WHERE id = ?";
        public const string Upd_AccountLock = "UPDATE account SET locked = ? WHERE id = ?";
        public const string Ins_Log = "INSERT INTO logs (time, realm, type, level, string) VALUES (?, ?, ?, ?, ?)";
        public const string Upd_Username = "UPDATE account SET v = 0, s = 0, username = ?, sha_pass_hash = ? WHERE id = ?";
        public const string Upd_Password = "UPDATE account SET v = 0, s = 0, sha_pass_hash = ? WHERE id = ?";
        public const string Upd_MuteTime = "UPDATE account SET mutetime = ? WHERE id = ?";
        public const string Upd_LastIP = "UPDATE account SET last_ip = ? WHERE username = ?";
        public const string Upd_AccountOnline = "UPDATE account SET online = 1 WHERE id = ?";
        public const string Upd_UptimePlayers = "UPDATE uptime SET uptime = ?, maxplayers = ? WHERE realmid = ? AND starttime = ?";
        public const string Del_OldLogs = "DELETE FROM logs WHERE (time + ?) < ?";
        public const string Del_account_access = "DELETE FROM account_access WHERE id = ?";
        public const string Del_account_accessByRealm = "DELETE FROM account_access WHERE id = ? AND (RealmID = ? OR RealmID = -1)";
        public const string Ins_account_access = "INSERT INTO account_access (id,gmlevel,RealmID) VALUES (?, ?, ?)";
        public const string Get_AccountIDByUsername = "SELECT id FROM account WHERE username = ?";
        public const string Get_account_accessGMLevel = "SELECT gmlevel FROM account_access WHERE id = ?";
        public const string Get_GMLevelByRealmID = "SELECT gmlevel FROM account_access WHERE id = ? AND (RealmID = ? OR RealmID = -1)";
        public const string Get_UsernameByID = "SELECT username FROM account WHERE id = ?";
        public const string Sel_CheckPassword = "SELECT 1 FROM account WHERE id = ? AND sha_pass_hash = ?";
        public const string Sel_CheckPasswordByName = "SELECT 1 FROM account WHERE username = ? AND sha_pass_hash = ?";
        public const string Sel_PInfo = "SELECT a.username, aa.gmlevel, a.email, a.last_ip, DATE_FORMAT(a.last_login, '%Y-%m-%d %T'), a.mutetime FROM account a LEFT JOIN account_access aa ON (a.id = aa.id AND (aa.RealmID = ? OR aa.RealmID = -1)) WHERE a.id = ?";
        public const string Sel_PInfoBans = "SELECT unbandate, bandate = unbandate, bannedby, banreason FROM accountBanned WHERE id = ? AND active ORDER BY bandate ASC LIMIT 1";
        public const string Sel_GMAccountS = "SELECT a.username, aa.gmlevel FROM account a, account_access aa WHERE a.id=aa.id AND aa.gmlevel >= ? AND (aa.realmid = -1 OR aa.realmid = ?)";
        public const string Sel_AccountInfo = "SELECT a.username, a.last_ip, aa.gmlevel, a.expansion FROM account a LEFT JOIN account_access aa ON (a.id = aa.id) WHERE a.id = ?";
        public const string Sel_account_accessGMLevelTest = "SELECT 1 FROM account_access WHERE id = ? AND gmlevel > ?";
        public const string Sel_account_access = "SELECT a.id, aa.gmlevel, aa.RealmID FROM account a LEFT JOIN account_access aa ON (a.id = aa.id) WHERE a.username = ?";
        public const string Sel_AccountRecruiter = "SELECT 1 FROM account WHERE recruiter = ?";
        public const string Sel_Bans = "SELECT 1 FROM accountBanned WHERE id = ? AND active = 1 UNION SELECT 1 FROM ipBanned WHERE ip = ?";
        public const string Sel_AccountWhoIs = "SELECT username, email, last_ip FROM account WHERE id = ?";
        public const string Sel_RealmListSecurityLevel = "SELECT allowedSecurityLevel from realmlist WHERE id = ?";
        public const string Del_Account = "DELETE FROM account WHERE id = ?";
    }
}
