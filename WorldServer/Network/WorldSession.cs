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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Framework.Constants;
using Framework.Constants.Authentication;
using Framework.Cryptography;
using Framework.Logging;
using Framework.Network;
using Framework.ObjectDefines;
using Framework.Threading.Actors;
using Ionic.Zlib;
using WorldServer.Game.Misc;
using WorldServer.Game.Packets;
using WorldServer.Game.WorldEntities;
using WorldServer.Game;
using Framework.Database;

namespace WorldServer.Network
{
    public class WorldSession : Actor<WorldSession>
    {
        public WorldSession()
        {  
            ReceiveBuffer = new byte[ushort.MaxValue];
            PacketQueue = new Queue();
            Crypt = new PacketCrypt();
        }

        #region Network
        public void OnConnect(Socket client)
        {
            clientSocket = client;
            _eventArgs = SocketAsyncEventArgsPool.Acquire();

            PacketWriter TransferInitiate = new PacketWriter(Opcodes.MSG_VerifyConnectivity);
            TransferInitiate.WriteCString("RLD OF WARCRAFT CONNECTION - SERVER TO CLIENT");

            Send(TransferInitiate);
            Receive();    
        }
        public void Receive()
        {
            _eventArgs.SetBuffer(ReceiveBuffer, 0, ReceiveBuffer.Length);
            _eventArgs.Completed += OnReceivePayload;

            try
            {
                if (!clientSocket.ReceiveAsync(_eventArgs))
                    OnReceivePayload(this, _eventArgs);
            }
            catch (Exception ex)
            {
                Disconnect(); // An error occured while receiving, the connection may have been disconnected.


                if (ex is ObjectDisposedException)
                    return;

                if (ex is SocketException)
                {
                    Log.outException(ex);
                    return;
                }
            }
        }
        private void OnReceivePayload(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= OnReceivePayload;

            var error = args.SocketError;
            if (error != SocketError.Success)
            {
                Log.outWarn("Client {0} triggered a socket error ({1}) when trying to fetch a payload - disconnected.", this, error);
                Disconnect();
                return;
            }

            var bytesTransferred = args.BytesTransferred;

            if (bytesTransferred == 0)
            {
                Log.outInfo("Client {0} disconnected gracefully.", this);
                Disconnect();
                return;
            }

            if (Crypt.IsInitialized)
            {
                while (bytesTransferred > 0)
                {
                    Crypt.Decrypt(ReceiveBuffer);

                    var header = BitConverter.ToUInt32(ReceiveBuffer, 0);
                    ushort size = (ushort)(header >> 12);
                    ushort opcode = (ushort)(header & 0xFFF);

                    ReceiveBuffer[0] = (byte)(0xFF & size);
                    ReceiveBuffer[1] = (byte)(0xFF & (size >> 8));
                    ReceiveBuffer[2] = (byte)(0xFF & opcode);
                    ReceiveBuffer[3] = (byte)(0xFF & (opcode >> 8));

                    var length = BitConverter.ToUInt16(ReceiveBuffer, 0) + 4;

                    var packetData = new byte[length];
                    Array.Copy(ReceiveBuffer, packetData, length);

                    PacketReader packet = new PacketReader(packetData);
                    PacketQueue.Enqueue(packet);

                    bytesTransferred -= length;
                    Array.Copy(ReceiveBuffer, length, ReceiveBuffer, 0, bytesTransferred);
                    OnData();
                }

            }
            else
                OnData();

            Receive();
        }
        public void OnData()
        {
            try
            {
                PacketReader packet = null;
                if (PacketQueue.Count > 0)
                    packet = (PacketReader)PacketQueue.Dequeue();
                else
                    packet = new PacketReader(ReceiveBuffer);

                PacketLog.WritePacket(clientSocket.RemoteEndPoint.ToString(), null, packet);

                if (Enum.IsDefined(typeof(Opcodes), packet.Opcode))
                    PacketManager.InvokeHandler(ref packet, this, (Opcodes)packet.Opcode);
                else
                    Log.outDebug("Received unknown opcode 0x{0:X} ({0}) from AcountId:{1}", (ushort)packet.Opcode, GetAccountId());
            }
            catch (Exception ex)
            {
                Log.outException(ex);
            }
        }

        public void Send(PacketWriter packet)
        {
            if (packet.Opcode == 0)
                return;

            var buffer = packet.ReadDataToSend();

            if (Crypt.IsInitialized)
            {
                uint totalLength = (uint)packet.Size - 2;
                totalLength <<= 12;
                totalLength |= ((uint)packet.Opcode & 0xFFF);

                var header = BitConverter.GetBytes(totalLength);

                Crypt.Encrypt(header);

                buffer[0] = header[0];
                buffer[1] = header[1];
                buffer[2] = header[2];
                buffer[3] = header[3];
            }

            var args = SocketAsyncEventArgsPool.Acquire();
            args.SetBuffer(buffer, 0, buffer.Length);
            args.Completed += OnSend;

            try
            {
                if (!clientSocket.SendAsync(args))
                    OnSend(this, args);
            }
            catch (Exception ex)
            {
                args.Completed -= OnSend;
                SocketAsyncEventArgsPool.Release(args);
                Disconnect();

                if (ex is ObjectDisposedException)
                    return;

                if (ex is SocketException)
                {
                    Log.outException(ex);
                    return;
                }

                throw;
            }

            Log.outDebug("Sent Opcode: 0x{0:X} ({0}) to Player: {1}", packet.Opcode, GetAccount() == null ? 0 : GetAccountId());
            PacketLog.WritePacket(clientSocket.RemoteEndPoint.ToString(), packet);
        }
        private void OnSend(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= OnSend;
            SocketAsyncEventArgsPool.Release(args);
        }

        public void Disconnect()
        {
            try
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Disconnect(false);
                clientSocket.Close();
            }
            catch (Exception) { }
        }
        public void KickPlayer()
        {
            if (clientSocket != null)
                clientSocket.Close();
        }
        #endregion

        public void ReadAddonsInfo(byte[] data, int size)
        {
            uint Size = BitConverter.ToUInt32(data, 0);

            byte[] Data = new byte[size];
            Array.Copy(data, 4, Data, 0, size);

            if (Size == 0)
                return;

            if (Size > 0xFFFFF)
            {
                Log.outError("ReadAddonsInfo addon info too big, size {0}", size);
                return;
            }
            PacketReader addonInfo = new PacketReader(ZlibStream.UncompressBuffer(Data), false);
            if (addonInfo != null)
            {
                uint addonsCount = addonInfo.ReadUInt32();

                for (uint i = 0; i < addonsCount; ++i)
                {
                    // check next addon data format correctness
                    if ((addonInfo.BaseStream.Position) + 1 > addonInfo.BaseStream.Length)
                        return;

                    string addonName = addonInfo.ReadCString();
                    byte enabled = addonInfo.ReadByte();
                    uint crc = addonInfo.ReadUInt32();
                    uint unk1 = addonInfo.ReadUInt32();

                    Log.outDebug("ADDON: Name: {0}, Enabled: 0x{1}, CRC: 0x{2}, Unknown2: 0x{3}", addonName, enabled, crc, unk1);

                    AddonInfo addon = new AddonInfo(addonName, enabled, crc, 2, true);

                    SavedAddon savedAddon = Addon.GetAddonInfo(addonName);
                    if (savedAddon != null)
                    {
                        bool match = true;

                        if (addon.CRC != savedAddon.CRC)
                            match = false;

                        if (!match)
                            Log.outWarn("ADDON: {0} was known, but didn't match known CRC (0x{1})!", addon.Name, savedAddon.CRC);
                    }
                    else
                    {
                        Addon.SaveAddon(addon);

                        Log.outWarn("ADDON: {0} (0x{1}) was not known, saving...", addon.Name, addon.CRC);
                    }

                    // TODO: Find out when to not use CRC/pubkey, and other possible states.
                    addonsList.Add(addon);
                }

                uint currentTime = addonInfo.ReadUInt32();
                Log.outDebug("ADDON: CurrentTime: {0}", currentTime);

                //if (addonInfo.rpos() != addonInfo.size())
                //sLog->outDebug(LOG_FILTER_NETWORKIO, "packet under-read!");
            }
            else
                Log.outError("Addon packet uncompress error!");
        }
        public void SendAddonsInfo()
        {
            byte[] addonPublicKey = new byte[256]
            {
                0xC3, 0x5B, 0x50, 0x84, 0xB9, 0x3E, 0x32, 0x42, 0x8C, 0xD0, 0xC7, 0x48, 0xFA, 0x0E, 0x5D, 0x54,
                0x5A, 0xA3, 0x0E, 0x14, 0xBA, 0x9E, 0x0D, 0xB9, 0x5D, 0x8B, 0xEE, 0xB6, 0x84, 0x93, 0x45, 0x75,
                0xFF, 0x31, 0xFE, 0x2F, 0x64, 0x3F, 0x3D, 0x6D, 0x07, 0xD9, 0x44, 0x9B, 0x40, 0x85, 0x59, 0x34,
                0x4E, 0x10, 0xE1, 0xE7, 0x43, 0x69, 0xEF, 0x7C, 0x16, 0xFC, 0xB4, 0xED, 0x1B, 0x95, 0x28, 0xA8,
                0x23, 0x76, 0x51, 0x31, 0x57, 0x30, 0x2B, 0x79, 0x08, 0x50, 0x10, 0x1C, 0x4A, 0x1A, 0x2C, 0xC8,
                0x8B, 0x8F, 0x05, 0x2D, 0x22, 0x3D, 0xDB, 0x5A, 0x24, 0x7A, 0x0F, 0x13, 0x50, 0x37, 0x8F, 0x5A,
                0xCC, 0x9E, 0x04, 0x44, 0x0E, 0x87, 0x01, 0xD4, 0xA3, 0x15, 0x94, 0x16, 0x34, 0xC6, 0xC2, 0xC3,
                0xFB, 0x49, 0xFE, 0xE1, 0xF9, 0xDA, 0x8C, 0x50, 0x3C, 0xBE, 0x2C, 0xBB, 0x57, 0xED, 0x46, 0xB9,
                0xAD, 0x8B, 0xC6, 0xDF, 0x0E, 0xD6, 0x0F, 0xBE, 0x80, 0xB3, 0x8B, 0x1E, 0x77, 0xCF, 0xAD, 0x22,
                0xCF, 0xB7, 0x4B, 0xCF, 0xFB, 0xF0, 0x6B, 0x11, 0x45, 0x2D, 0x7A, 0x81, 0x18, 0xF2, 0x92, 0x7E,
                0x98, 0x56, 0x5D, 0x5E, 0x69, 0x72, 0x0A, 0x0D, 0x03, 0x0A, 0x85, 0xA2, 0x85, 0x9C, 0xCB, 0xFB,
                0x56, 0x6E, 0x8F, 0x44, 0xBB, 0x8F, 0x02, 0x22, 0x68, 0x63, 0x97, 0xBC, 0x85, 0xBA, 0xA8, 0xF7,
                0xB5, 0x40, 0x68, 0x3C, 0x77, 0x86, 0x6F, 0x4B, 0xD7, 0x88, 0xCA, 0x8A, 0xD7, 0xCE, 0x36, 0xF0,
                0x45, 0x6E, 0xD5, 0x64, 0x79, 0x0F, 0x17, 0xFC, 0x64, 0xDD, 0x10, 0x6F, 0xF3, 0xF5, 0xE0, 0xA6,
                0xC3, 0xFB, 0x1B, 0x8C, 0x29, 0xEF, 0x8E, 0xE5, 0x34, 0xCB, 0xD1, 0x2A, 0xCE, 0x79, 0xC3, 0x9A,
                0x0D, 0x36, 0xEA, 0x01, 0xE0, 0xAA, 0x91, 0x20, 0x54, 0xF0, 0x72, 0xD8, 0x1E, 0xC7, 0x89, 0xD2
            };

            PacketWriter data = new PacketWriter(Opcodes.SMSG_AddonInfo);

            foreach (var itr in addonsList)
            {
                data.WriteUInt8(itr.State);

                bool crcpub = itr.UsePublicKeyOrCRC;
                data.WriteUInt8(crcpub);
                if (crcpub)
                {
                    bool usepk = itr.CRC != Addon.StandardAddonCRC; // If addon is Standard addon CRC
                    data.WriteUInt8(usepk);
                    if (usepk)                                      // if CRC is wrong, add public key (client need it)
                    {
                        Log.outDebug("ADDON: CRC (0x{0}) for addon {1} is wrong (does not match expected 0x{2}), sending pubkey",
                            itr.CRC, itr.Name, Addon.StandardAddonCRC);

                        data.WriteBytes(addonPublicKey);
                    }

                    data.WriteUInt32(0);                              // TODO: Find out the meaning of this.
                }

                byte unk3 = 0;                                     // 0 is sent here
                data.WriteUInt8(unk3);
                if (unk3 != 0)
                {
                    // String, length 256 (null terminated)
                    data.WriteUInt8(0);
                }
            }
            addonsList.Clear();

            data.WriteUInt32(0); // count for an unknown for loop

            Send(data);
        }

        public void LoadTutorialsData()
        {            
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_TUTORIALS);
            stmt.AddValue(0, GetAccountId());

            SQLResult result = DB.Characters.Select(stmt);
            if (result.Count > 0)
                for (var i = 0; i < SharedConst.MaxAccountTutorialValues; i++)
                    m_Tutorials[i] = result.Read<uint>(0, i);

            m_TutorialsChanged = false;
        }
        public void SaveTutorialsData()
        {
            if (!m_TutorialsChanged)
                return;
            
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_HAS_TUTORIALS);
            stmt.AddValue(0, GetAccountId());
            bool hasTutorials = !DB.Characters.Execute(stmt);
            // Modify data in DB
            stmt = DB.Characters.GetPreparedStatement(hasTutorials ? CharStatements.CHAR_UPD_TUTORIALS : CharStatements.CHAR_INS_TUTORIALS);
            for (var i = 0; i < SharedConst.MaxAccountTutorialValues; ++i)
                stmt.AddValue(i, m_Tutorials[i]);
            stmt.AddValue(SharedConst.MaxAccountTutorialValues, GetAccountId());
            DB.Characters.Execute(stmt);

            m_TutorialsChanged = false;
        }

        /*
         void WorldSession::LoadGlobalAccountData()
{
    PreparedStatement* stmt = CharacterDatabase.GetPreparedStatement(CHAR_SEL_ACCOUNT_DATA);
    stmt->setUInt32(0, GetAccountId());
    LoadAccountData(CharacterDatabase.Query(stmt), GLOBAL_CACHE_MASK);
}

void WorldSession::LoadAccountData(PreparedQueryResult result, uint32 mask)
{
    for (uint32 i = 0; i < NUM_ACCOUNT_DATA_TYPES; ++i)
        if (mask & (1 << i))
            m_accountData[i] = AccountData();

    if (!result)
        return;

    do
    {
        Field* fields = result->Fetch();
        uint32 type = fields[0].GetUInt8();
        if (type >= NUM_ACCOUNT_DATA_TYPES)
        {
            sLog->outError(LOG_FILTER_GENERAL, "Table `%s` have invalid account data type (%u), ignore.",
                mask == GLOBAL_CACHE_MASK ? "account_data" : "character_account_data", type);
            continue;
        }

        if ((mask & (1 << type)) == 0)
        {
            sLog->outError(LOG_FILTER_GENERAL, "Table `%s` have non appropriate for table  account data type (%u), ignore.",
                mask == GLOBAL_CACHE_MASK ? "account_data" : "character_account_data", type);
            continue;
        }

        m_accountData[type].Time = time_t(fields[1].GetUInt32());
        m_accountData[type].Data = fields[2].GetString();
    }
    while (result->NextRow());
}

void WorldSession::SetAccountData(AccountDataType type, time_t tm, std::string data)
{
    uint32 id = 0;
    uint32 index = 0;
    if ((1 << type) & GLOBAL_CACHE_MASK)
    {
        id = GetAccountId();
        index = CHAR_REP_ACCOUNT_DATA;
    }
    else
    {
        // _player can be NULL and packet received after logout but m_GUID still store correct guid
        if (!m_GUIDLow)
            return;

        id = m_GUIDLow;
        index = CHAR_REP_PLAYER_ACCOUNT_DATA;
    }

    PreparedStatement* stmt = CharacterDatabase.GetPreparedStatement(index);
    stmt->setUInt32(0, id);
    stmt->setUInt8 (1, type);
    stmt->setUInt32(2, uint32(tm));
    stmt->setString(3, data);
    CharacterDatabase.Execute(stmt);

    m_accountData[type].Time = tm;
    m_accountData[type].Data = data;
}
         */
        public void SendAccountDataTimes(AccountDataMasks mask)
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_AccountDataTimes);
            data.WriteUnixTime();
            data.WriteUInt8(0);
            data.WriteUInt32((uint)mask);

            for (int i = 0; i <= (int)AccountDataTypes.NumAccountDataTypes; ++i)
                if (((int)mask & (1 << i)) != 0)
                    if (i == 1 && mask == AccountDataMasks.GlobalCacheMask)
                        data.WriteUnixTime();
                    else
                        data.WriteUInt32(0);

            Send(data);
        }
        public void SendTutorialsData()
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_TutorialFlags);
            for (int i = 0; i < 8; i++)
                data.WriteUInt32(m_Tutorials[i]);

            Send(data);
        }
        public void SendAuthResponse()
        {
            PacketWriter authResponse = new PacketWriter(Opcodes.SMSG_AuthResponse);

            var Class = Cypher.ObjMgr.GetActivationClasses();
            var Races = Cypher.ObjMgr.GetActivationRaces();
            
            authResponse.WriteBit(true);
            authResponse.WriteBits(Class.Count, 25); // Activation count for classes
            authResponse.WriteBit(false);             // Unknown, 5.0.4
            authResponse.WriteBit(false);
            authResponse.WriteBits(0, 22);            // Activate character template windows/button
            authResponse.WriteBits(Races.Count, 25);  // Activation count for races
            authResponse.WriteBit(false);             // IsInQueue
            authResponse.BitFlush();

            foreach (var r in Races)
            {
                authResponse.WriteUInt8(r.Value);
                authResponse.WriteUInt8(r.Key);                
            }

            authResponse.WriteUInt32(0);
            authResponse.WriteUInt32(0);
            authResponse.WriteUInt8(0);
            authResponse.WriteUInt8(GetExpansion());
            authResponse.WriteUInt8(GetExpansion());

            foreach (var c in Class)
            {
                authResponse.WriteUInt8(c.Key);
                authResponse.WriteUInt8(c.Value);
            }

            authResponse.WriteUInt32(0);

            authResponse.WriteUInt8((byte)AuthCodes.Ok);

            Send(authResponse);
        }

        #region Gets/Sets
        public void SetPlayer(Player pl) { player = pl; }
        public void SetAccount(Account acct) { account = acct; }
        public Player GetPlayer() { return player; }
        public Account GetAccount() { return account; }
        public AccountTypes GetSecurity() { return account.GMLevel; }
        public uint GetAccountId() { return (uint)account.Id; }
        public uint GetExpansion() { return account.Expansion; }
        uint GetTutorialInt(byte index) { return m_Tutorials[index]; }
        void SetTutorialInt(byte index, uint value)
        {
            if (m_Tutorials[index] != value)
            {
                m_Tutorials[index] = value;
                m_TutorialsChanged = true;
            }
        }
        #endregion

        #region Fields
        internal PacketCrypt Crypt;
        public List<AddonInfo> addonsList = new List<AddonInfo>();
        uint[] m_Tutorials = new uint[SharedConst.MaxAccountTutorialValues];
        bool m_TutorialsChanged;

        private Socket clientSocket;
        private Account account { get; set; }
        private Player player { get; set; }
        private Queue PacketQueue;
        private SocketAsyncEventArgs _eventArgs;
        private byte[] ReceiveBuffer;
        #endregion
    }
}
