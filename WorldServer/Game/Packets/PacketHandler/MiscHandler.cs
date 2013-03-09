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
using Framework.Network;
using WorldServer.Game.Managers;
using WorldServer.Network;

namespace WorldServer.Game.Packets
{
    public class MiscHandler : Cypher
    {
        public static void SendMOTD(ref WorldSession session)
        {
            PacketWriter motd = new PacketWriter(Opcodes.SMSG_Motd);
            motd.WriteUInt32(2);

            motd.WriteCString("Your new Mists of Pandaria emulator: CypherCore!!!");
            motd.WriteCString("Welcome to our Mists of Pandaria beta test server.");
            session.Send(motd);
        }

        [ClientOpcode(Opcodes.CMSG_RequestAccountData)]
        public static void HandleRequestAccountData(ref PacketReader packet, ref WorldSession session)
        {
            uint type = packet.ReadUInt32();

            if (type > (uint)AccountDataTypes.NumAccountDataTypes)
                return;

            PacketWriter writer = new PacketWriter(Opcodes.SMSG_UpdateAccountData);
            writer.WritePackedGuid(session.GetPlayer().GetGUID());
            writer.WriteUInt32(type);
            writer.WriteUnixTime();
            writer.WriteUInt32(0);//size
            session.Send(writer);
            //data << uint32(size);                                   // decompressed length
            //data.append(dest);                                      // compressed data
        }

        [ClientOpcode(Opcodes.CMSG_UpdateAccountData)]
        public static void HandleRequestAccountUpdate(ref PacketReader packet, ref WorldSession session)
        {
            uint type = packet.ReadUInt32();
            uint timestamp = packet.ReadUInt32();
            uint decompressedSize = packet.ReadUInt32();

            if (type > (uint)AccountDataTypes.NumAccountDataTypes)
                return;
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_UpdateAccountDataComplete);

            if (decompressedSize == 0)                               // erase
            {
                //SetAccountData(AccountDataType(type), 0, "");
                writer.WriteUInt32(type);
                writer.WriteUInt32(0);
                session.Send(writer);
                return;
            }
            //std::string adata;
            //dest >> adata;
            writer.WriteUInt32(type);
            writer.WriteUInt32(0);
            session.Send(writer);
        }

        [ClientOpcode(Opcodes.CMSG_SetSelection)]
        public static void HandleSetSelection(ref PacketReader packet, ref WorldSession session)
        {
            var mask = new byte[] {3, 1, 7, 2, 6, 4, 0, 5};
            var bytes = new byte[] {4, 1, 5, 2, 6, 7, 0, 3};
            var guid = packet.GetGuid(mask, bytes);
            session.GetPlayer().SetSelection(guid);

            var blah = System.BitConverter.GetBytes(1086266846283650864);
        }

        [ClientOpcode(Opcodes.CMSG_Zoneupdate)]
        public static void HandleZoneUpdate(ref PacketReader packet, ref WorldSession session)
        {
            uint newZone = packet.ReadUInt32();

            //Log.outDebug(LOG_FILTER_NETWORKIO, "WORLD: Recvd ZONE_UPDATE: %u", newZone);

            // use server size data
            uint newzone, newarea;
            session.GetPlayer().GetZoneAndAreaId(out newzone, out newarea);
            session.GetPlayer().UpdateZone(newzone, newarea);
            //GetPlayer()->SendInitWorldStates(true, newZone);
        }
    }
}
