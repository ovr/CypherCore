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
using Framework.Network;
using WorldServer.Game.Managers;
using WorldServer.Network;

namespace WorldServer.Game.Packets
{
    public class TimeHandler : Cypher
    {
        [ClientOpcode(Opcodes.CMSG_ReadyForAccountDataTimes)]
        public static void HandleReadyForAccountDataTimes(ref PacketReader packet, ref WorldSession session)
        {
            session.SendAccountDataTimes(AccountDataMasks.GlobalCacheMask);
        }

        [ClientOpcode(Opcodes.CMSG_WorldStateUiTimerUpdate)]
        public static void HandleRequestUITime(ref PacketReader packet, ref WorldSession session)
        {
            PacketWriter uiTime = new PacketWriter(Opcodes.SMSG_WorldStateUiTimerUpdate);

            uiTime.WriteUnixTime();
            session.Send(uiTime);
        }

        [ClientOpcode(Opcodes.CMSG_Ping)]
        public static void HandlePing(ref PacketReader packet, ref WorldSession session)
        {
            uint sequence = packet.ReadUInt32();
            uint latency = packet.ReadUInt32();
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_Pong);

            writer.WriteUInt32(sequence);
            session.Send(writer);
        }

        public static void HandleTimeSpeed(ref WorldSession session)
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_GamespeedSet);

            DateTime time = DateTime.Now;
            uint packedtime = (uint)(((uint)time.Year - 100) << 24 | (uint)time.Month << 20 | (uint)(time.Day - 1) << 14 | (uint)(time.DayOfWeek) << 11 | (uint)time.Hour << 6 | (uint)time.Minute);

            data.WriteUInt32(packedtime);
            data.WriteUInt32(packedtime);
            data.WriteFloat(0.01666667f);                             // game speed
            data.WriteUInt32(0);                                      // added in 3.1.2
            data.WriteUInt32(0);
            session.Send(data);
        }

        [ClientOpcode(Opcodes.CMSG_RealmSplit)]
        public static void HandleRealmSplitStateResponse(ref PacketReader packet, ref WorldSession session)
        {
            uint realmSplitState = 0;

            PacketWriter realmSplitStateResp = new PacketWriter(Opcodes.SMSG_RealmSplit);

            realmSplitStateResp.WriteUInt32(packet.ReadUInt32());
            realmSplitStateResp.WriteUInt32(realmSplitState);
            realmSplitStateResp.WriteCString("01/01/01");

            session.Send(realmSplitStateResp);
        }
    }
}
