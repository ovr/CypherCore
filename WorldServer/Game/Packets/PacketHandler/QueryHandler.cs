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
using WorldServer.Game.WorldEntities;
using WorldServer.Network;
using Framework.Database;
using Framework.Configuration;

namespace WorldServer.Game.Packets
{
    public class QueryHandler : Cypher
    {
        [ClientOpcode(Opcodes.CMSG_NameQuery)]
        public static void HandleNameQueryRequest(ref PacketReader packet, ref WorldSession session)
        {
            ulong guid = packet.ReadUInt64();
            uint realmId = packet.ReadUInt32();//Sometimes it reads 0?  maybe not realmid?

            Player pl = ObjMgr.FindPlayer(guid);

            if (pl == null)
                return;

            PacketWriter writer = new PacketWriter(Opcodes.SMSG_NameQueryResponse);
            writer.WritePackedGuid(guid);
            writer.WriteUInt8(0);//result?
            writer.WriteCString(pl.GetName());
            writer.WriteUInt32(WorldConfig.RealmId);
            writer.WriteUInt8(pl.getRace());
            writer.WriteUInt8(pl.getGender());
            writer.WriteUInt8(pl.getClass());
            writer.WriteUInt8(0);

            session.Send(writer);
        }

        [ClientOpcode(Opcodes.CMSG_RealmQuery)]
        public static void HandleRealmCache(ref PacketReader packet, ref WorldSession session)
        {
            Player pl = session.GetPlayer();

            uint realmId = packet.ReadUInt32();

            SQLResult result = DB.Auth.Select("SELECT name FROM realmList WHERE id = {0}", realmId);
            string realmName = result.Read<string>(0, "Name");

            PacketWriter nameCache = new PacketWriter(Opcodes.SMSG_RealmQuery);

            nameCache.WriteUInt32(realmId);
            nameCache.WriteUInt8(0);              // < 0 => End of packet
            nameCache.WriteUInt8(1);              // Unknown
            nameCache.WriteCString(realmName);
            nameCache.WriteCString(realmName);

            session.Send(nameCache);
        }

        [ClientOpcode(Opcodes.CMSG_GameobjectQuery)]
        public static void HandleGameObjectQueryRequest(ref PacketReader packet, ref WorldSession session)
        {
            uint entry = packet.ReadUInt32();
            ulong guid = packet.ReadUInt64();

            GameObjectTemplate go = Cypher.ObjMgr.GetGameObjectTemplate(entry);
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GameobjectQueryResponse);
            writer.WriteUInt32(entry);
            writer.WriteInt32(go.type);
            writer.WriteUInt32(go.displayId);
            writer.WriteCString(go.name);
            writer.WriteUInt8(0);// name2
            writer.WriteUInt8(0);// name3
            writer.WriteUInt8(0);// name 4
            writer.WriteCString(go.IconName);
            writer.WriteCString(go.castBarCaption);
            writer.WriteCString(go.unk1);

            for (int i = 0; i < 32; i++)
                writer.WriteUInt32(go.RawData[i]);

            writer.WriteFloat(go.size);

            for (int i = 0; i < 6; i++)
                writer.WriteUInt32(go.questItems[i]);

            writer.WriteInt32(go.unkInt32); //expansion req

            session.Send(writer);
        }

        [ClientOpcode(Opcodes.CMSG_CreatureQuery)]
        public static void HandleCreatureQueryRequest(ref PacketReader packet, ref WorldSession session)
        {
            uint id = packet.ReadUInt32();
            ulong guid = packet.ReadUInt64();

            //var data = Globals.ObjMgr.GetCreatureData(Globals.ObjMgr.GuidLowPart(guid));//.GetCreatureTemplate(entry);
            var creature = Cypher.ObjMgr.GetCreatureTemplate(id);

            PacketWriter writer = new PacketWriter(Opcodes.SMSG_CreatureQueryResponse);
            writer.WriteUInt32(creature.Entry);
            writer.WriteCString(creature.Name);

            for (int i = 0; i < 7; i++)
                writer.WriteCString("");

            writer.WriteCString(creature.SubName);
            writer.WriteCString("");
            writer.WriteCString(creature.IconName);

            writer.WriteUInt32(creature.TypeFlags);
            writer.WriteUInt32(creature.TypeFlags2);

            writer.WriteUInt32(creature.CreatureType);
            writer.WriteUInt32(creature.Family);
            writer.WriteUInt32(creature.Rank);

            writer.WriteUInt32(creature.KillCredit[0]);
            writer.WriteUInt32(creature.KillCredit[1]);

            for (int i = 0; i < 4; ++i)
                writer.WriteUInt32(creature.ModelId[i]);

            writer.WriteFloat(creature.HeathMod);
            writer.WriteFloat(creature.ManaMod);

            writer.WriteUInt8((byte)(creature.RacialLeader ? 1 : 0));

            for (int i = 0; i < 6; ++i)
                writer.WriteUInt32(creature.QuestItems[i]);

            writer.WriteUInt32(creature.MovementId);
            writer.WriteUInt32(creature.expansion);

            session.Send(writer);
        }
    }
}
