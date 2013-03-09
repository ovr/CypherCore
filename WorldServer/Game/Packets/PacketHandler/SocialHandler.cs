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

using System.Linq;
using Framework.Constants;
using Framework.Network;
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;


namespace WorldServer.Game.Packets
{
    public class SocialHandler : Cypher
    {
        [ClientOpcode(Opcodes.CMSG_Who)]
        public static void HandleWhoRequest(ref PacketReader packet, ref WorldSession session)
        {
            uint minLevel = packet.ReadUInt32();
            uint maxLevel = packet.ReadUInt32();
            string playerName = packet.ReadCString();
            string guildName = packet.ReadCString();
            int raceMask = packet.ReadInt32();
            int classMask = packet.ReadInt32();

            uint[] zones = new uint[packet.ReadUInt32()];
            for (int i = 0; i < zones.Length; ++i)
                zones[i] = packet.ReadUInt32();

            string[] patterns = new string[packet.ReadUInt32()];
            for (int i = 0; i < patterns.Length; ++i)
                patterns[i] = packet.ReadCString();

            //temp till more is added  just going to show all that is online
            uint count = 0;// (uint)ObjMgr.PlayerList.Count;

            PacketWriter writer = new PacketWriter(Opcodes.SMSG_Who);
            writer.WriteUInt32(count); //match count?
            writer.WriteUInt32(count); //online

            //foreach (Player pl in ObjMgr.PlayerList.ToList())
            {
                //writer.WriteCString(pl.GetName());
                //writer.WriteCString(GuildMgr.GetGuildName(0));
                //writer.WriteUInt32(pl.getLevel());
                //writer.WriteUInt32(pl.getClass());
                //writer.WriteUInt32(pl.getRace());
                //writer.WriteUInt8(pl.getGender());
                //writer.WriteUInt32(pl.GetZoneId());
            }
            session.Send(writer);
        }

        [ClientOpcode(Opcodes.CMSG_Whois)]
        public static void HandleWhoIsRequest(ref PacketReader packet, ref WorldSession session)
        {

        }
    }
}
