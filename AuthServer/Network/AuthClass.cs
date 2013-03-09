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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Framework.Constants.Authentication;
using Framework.Cryptography;
using Framework.Database;
using Framework.Logging;
using Framework.Network;
using Framework.ObjectDefines;
using Framework.Utility;
using System.Linq;

namespace AuthServer.Network
{
    public class AuthClass
    {
        public static Account account { get; set; }
        public static List<Realm> Realms;
        public static AuthNetwork realm;
        public Socket clientSocket;
        byte[] DataBuffer;
        public SRP6 SRP { get; set; }
        ushort ClientBuild { get; set; }

        static AuthClass()
        {
            account = new Account();
            realm = new AuthNetwork();
            Realms = new List<Realm>();
        }

        void HandleRealmData(byte[] data)
        {
            PacketReader reader = new PacketReader(data, false);

            switch ((ClientLink)reader.ReadUInt8())
            {
                case ClientLink.AuthLogonChallenge:
                case ClientLink.AuthReconnectChallenge:
                    HandleAuthLogonChallenge(this, reader);
                    break;
                case ClientLink.AuthLogonProof:
                case ClientLink.AuthReconnectProof:
                    HandleAuthLogonProof(this, reader);
                    break;
                case ClientLink.RealmList:
                    HandleRealmList(this, reader);
                    break;
            }
        }

        public void HandleAuthLogonChallenge(AuthClass session, PacketReader data)
        {
            data.Skip(10);
            ClientBuild = data.ReadUInt16();
            data.Skip(8);
            account.Locale = data.ReadStringFromBytes(4);
            data.Skip(4);

            byte[] ip = new byte[4];

            for (int i = 0; i < 4; ++i)
                ip[i] = data.ReadUInt8();

            account.LastIP = ip[0] + "." + ip[1] + "." + ip[2] + "." + ip[3];
       
            StringBuilder nameBuilder = new StringBuilder();
            byte nameLength = data.ReadUInt8();
            char[] name = new char[nameLength];

            for (int i = 0; i < nameLength; i++)
            {
                name[i] = data.ReadChar();
                nameBuilder.Append(name[i]);
            }

            account.Name = nameBuilder.ToString().Trim(new char[] {'@'}).ToUpper();
            //             0,            1,       2,       3,          4
            //"SELECT a.sha_pass_hash, a.id, a.locked, a.last_ip, aa.gmlevel FROM account a LEFT JOIN account_access aa ON (a.id = aa.id) WHERE a.username = ?"
            PreparedStatement stmt = DB.Auth.GetPreparedStatement(LoginStatements.Sel_LogonChallenge);
            stmt.AddValue(0, account.Name);
            
            SQLResult result = DB.Auth.Select(stmt);

            PacketWriter logonChallenge = new PacketWriter();
            logonChallenge.WriteUInt8((byte)ClientLink.AuthLogonChallenge);
            logonChallenge.WriteUInt8(0);

            if (result.Count != 0)
            {
                account.Id = result.Read<uint>(0, 1);
                SRP = new SRP6(account.Name, result.Read<string>(0, 0).ToByteArray(), true);

                logonChallenge.WriteUInt8((byte)AuthResults.Success);
                logonChallenge.WriteBytes(SRP.PublicEphemeralValueB.GetBytes());
                logonChallenge.WriteUInt8(1);
                logonChallenge.WriteBytes(SRP.Generator.GetBytes());
                logonChallenge.WriteUInt8(0x20);
                logonChallenge.WriteBytes(SRP.Modulus.GetBytes());
                logonChallenge.WriteBytes(SRP.Salt.GetBytes());
                logonChallenge.WriteBytes(SRP6.RandomNumber(0x10).GetBytes());
                // GMLevel
                logonChallenge.WriteUInt8(0);

            }
            else
                logonChallenge.WriteUInt8((byte)AuthResults.FailUnknownAccount);

            session.Send(logonChallenge);
        }

        public void HandleAuthLogonProof(AuthClass session, PacketReader data)
        {
            PacketWriter logonProof = new PacketWriter();

            BigInteger a = data.ReadBytes(32);
            BigInteger m1 = data.ReadBytes(20);

            SRP.PublicEphemeralValueA = a;

            if (!SRP.IsClientProofValid(m1))
                return;

            foreach (var b in SRP.SessionKey.GetBytes())
                if (b < 0x10)
                    account.SessionKey += "0" + String.Format("{0:X}", b);
                else
                    account.SessionKey += String.Format("{0:X}", b);

            logonProof.WriteUInt8((byte)ClientLink.AuthLogonProof);
            logonProof.WriteUInt8(0);
            logonProof.WriteBytes(SRP.ServerSessionKeyProof.GetBytes(), 20);
            logonProof.WriteUInt32(0x00800000);
            logonProof.WriteUInt32(0);
            logonProof.WriteUInt16(0);

            //"UPDATE account SET sessionkey = ?, last_ip = ?, last_login = NOW(), locale = ?, failed_logins = 0 WHERE username = ?"
            PreparedStatement stmt = DB.Auth.GetPreparedStatement(LoginStatements.Upd_LogonProof);
            stmt.AddValue(0, account.SessionKey);
            stmt.AddValue(1, account.LastIP);
            stmt.AddValue(2, 0);//GetLocaleByName(_localizationName));
            stmt.AddValue(3, account.Name);
            DB.Auth.Execute(stmt);

            Log.outInfo("Accepted Client Connection IP: {0}", clientSocket.LocalEndPoint);
            session.Send(logonProof);
        }

        public void HandleRealmList(AuthClass session, PacketReader data)
        {
            PacketWriter realmData = new PacketWriter();

            foreach (var r in Realms)
            {
                realmData.WriteUInt8(r.Icon);
                realmData.WriteUInt8(0); //Status Lock
                realmData.WriteUInt8(r.Flags);
                realmData.WriteCString(r.Name);

                //if (account.LastIP == "127.0.0.1")
                    //realmData.WriteCString("127.0.0.1:" + r.Port);
                //else
                    realmData.WriteCString(r.IP + ":" + r.Port);
                realmData.WriteUInt32(r.Population);
                realmData.WriteUInt8(0); //AmountOfCharacters
                realmData.WriteUInt8(r.TimeZone); //Category timezone
                realmData.WriteUInt8(0x2C);
            }

            PacketWriter realmList = new PacketWriter();
            realmList.WriteUInt8((byte)ClientLink.RealmList);

            realmList.WriteUInt16((ushort)(realmData.BaseStream.Length + 8));
            realmList.WriteUInt32(0);
            realmList.WriteUInt16((ushort)Realms.Count);
            realmList.WriteBytes(realmData.ReadDataToSend(true));            
            realmList.WriteUInt8(0);
            realmList.WriteUInt8(0x10);

            Send(realmList);
        }

        public void Recieve()
        {
            while (realm.listenSocket)
            {
                Thread.Sleep(1);
                if (clientSocket.Available > 0)
                {
                    DataBuffer = new byte[clientSocket.Available];
                    clientSocket.Receive(DataBuffer, DataBuffer.Length, SocketFlags.None);

                    HandleRealmData(DataBuffer);
                }
            }

            clientSocket.Close();
        }

        public void Send(PacketWriter packet)
        {
            DataBuffer = packet.ReadDataToSend(true);

            try
            {
                clientSocket.BeginSend(DataBuffer, 0, DataBuffer.Length, SocketFlags.None, new AsyncCallback(FinishSend), clientSocket);
                packet.Flush();
            }
            catch (Exception ex)
            {
                Log.outError("{0}", ex.Message);
                Log.outInit();
                clientSocket.Close();
            }
        }

        public void FinishSend(IAsyncResult result)
        {
            clientSocket.EndSend(result);
        }
    }
}
