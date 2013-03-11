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
using Framework.Constants;
using Framework.Constants.Authentication;
using Framework.Database;
using Framework.Network;
using Framework.ObjectDefines;
using WorldServer.Game.Managers;
using WorldServer.Network;
using Framework.Logging;
using Framework.Utility;
using Framework.Configuration;

namespace WorldServer.Game.Packets
{
    public class AuthenticationHandler : Cypher
    {
        [ClientOpcode(Opcodes.MSG_VerifyConnectivity)]
        public static void HandleTransferInitiate(ref PacketReader packet, ref WorldSession session)
        {
            PacketWriter authChallenge = new PacketWriter(Opcodes.SMSG_AuthChallenge, true);
            
            authChallenge.WriteUInt32((uint)new Random(DateTime.Now.Second).Next(1, 0xFFFFFFF));

            for (int i = 0; i < 8; i++)
                authChallenge.WriteUInt32(0);
            
            authChallenge.WriteUInt8(1);

            session.Send(authChallenge);
        }

        [ClientOpcode(Opcodes.CMSG_AuthSession)]
        public static void HandleAuthSession(ref PacketReader packet, ref WorldSession session)
        {
            byte[] digest = new byte[20];

            packet.Skip(6);
            digest[5] = packet.ReadByte();
            byte minorVersion = packet.ReadByte();
            digest[2] = packet.ReadByte();
            ushort clientBuild = packet.ReadUInt16();
            digest[18] = packet.ReadByte();
            digest[10] = packet.ReadByte();
            uint clientSeed = packet.ReadUInt32();//????
            digest[9] = packet.ReadByte();
            digest[8] = packet.ReadByte();
            digest[11] = packet.ReadByte();
            digest[13] = packet.ReadByte();
            digest[4] = packet.ReadByte();
            digest[7] = packet.ReadByte();
            digest[16] = packet.ReadByte();
            digest[1] = packet.ReadByte();
            digest[0] = packet.ReadByte();
            digest[14] = packet.ReadByte();
            digest[12] = packet.ReadByte();
            byte majorVersion = packet.ReadByte();
            digest[17] = packet.ReadByte();
            digest[19] = packet.ReadByte();
            packet.Skip(12);
            digest[3] = packet.ReadByte();
            packet.Skip(8);
            digest[6] = packet.ReadByte();
            digest[15] = packet.ReadByte();

            //


            int addonSize = packet.ReadInt32();
            byte[] addondata = packet.ReadBytes(addonSize);

            packet.GetBit();
            uint namelength = packet.GetBits<uint>(12);
            string accountName = packet.ReadString(namelength);

            //"SELECT id, sha_pass_hash, sessionkey, last_ip, locked, expansion, mutetime, locale, recruiter, FROM account WHERE username = ?"
            PreparedStatement stmt = DB.Auth.GetPreparedStatement(LoginStatements.Sel_AccountInfoByName);
            stmt.AddValue(0, accountName);
            SQLResult result = DB.Auth.Select(stmt);

            if (result.Count == 0)
            {
                PacketWriter data = new PacketWriter(Opcodes.SMSG_AuthResponse);
                data.WriteBit(0); // has queue info
                data.WriteBit(0); // has account info
                data.WriteUInt8(AuthCodes.UnknownAccount);
                session.Send(data);

                Log.outError("HandleAuthSession: Sent Auth Response (unknown account).");
                session.Disconnect();
                return;
            }

            var account = new Account()
            {
                Id = result.Read<uint>(0, 0),
                Name = accountName,
                Password = result.Read<string>(0, 1),
                SessionKey = result.Read<string>(0, 2),
                LastIP = result.Read<string>(0, 3),
                Expansion = result.Read<byte>(0, "expansion"),
                Locale = result.Read<String>(0, 7),
            };
            //uint world_expansion = sWorld->getIntConfig(CONFIG_EXPANSION);
            //if (account.expansion > world_expansion)
            //account.expansion = world_expansion;

            ///- Re-check ip locking (same check as in realmd).
            if (result.Read<byte>(0, 4) == 1) // if ip is locked
            {
                //if (strcmp (fields[2].GetCString(), GetRemoteAddress().c_str()))
                {
                    PacketWriter data = new PacketWriter(Opcodes.SMSG_AuthResponse);
                    data.WriteBit(0); // has queue info
                    data.WriteBit(0); // has account info
                    data.WriteUInt8(AuthCodes.Failed);
                    session.Send(data);

                    Log.outDebug("HandleAuthSession: Sent Auth Response (Account IP differs).");
                    return;
                }
            }

            byte[] kBytes = account.SessionKey.ToByteArray();

            uint recruiter = result.Read<uint>(0, 8);
            //string os = result.Read<string>(0, 9);

            // Checks gmlevel per Realm
            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Get_GMLevelByRealmID);
            stmt.AddValue(0, account.Id);
            stmt.AddValue(1, WorldConfig.RealmId);

            result = DB.Auth.Select(stmt);

            if (result.Count == 0)
                account.GMLevel = 0;
            else
                account.GMLevel = (AccountTypes)result.Read<byte>(0, 0);

            //Ban Shit here

            // Check if this user is by any chance a recruiter
            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Sel_AccountRecruiter);
            stmt.AddValue(0, account.Id);
            result = DB.Auth.Select(stmt);

            bool isRecruiter = false;
            if (result.Count > 0)
                isRecruiter = true;


            // Update the last_ip in the database

            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Upd_LastIP);

            stmt.AddValue(0, account.LastIP);
            stmt.AddValue(1, account.Name);

            DB.Auth.Execute(stmt);

            session.SetAccount(account);

            session.Crypt.Initialize(kBytes);

            //session.LoadGlobalAccountData();
            session.LoadTutorialsData();
            session.ReadAddonsInfo(addondata, addonSize - 4);

            session.SendAuthResponse();
            session.SendAddonsInfo();

            uint version = 0;//todo fixme
            PacketWriter data1 = new PacketWriter(Opcodes.SMSG_ClientcacheVersion);
            data1.WriteUInt32(version);
            //session.Send(data1);

            session.SendTutorialsData();
        }
    }
}
