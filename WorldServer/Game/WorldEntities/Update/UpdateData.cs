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

using System.Collections.Generic;
using Framework.Constants;
using Framework.Network;
using WorldServer.Network;
using System;

namespace WorldServer.Game.WorldEntities
{
    public class UpdateData
    {
        public UpdateData(uint mapId)
        {
            MapId = mapId;
            BlockCount = 0;
            data = new PacketWriter();
            outOfRangeGUIDs = new List<ulong>();
        }

        public void AddOutOfRangeGUID(List<ulong> guids)
        {
            outOfRangeGUIDs.AddRange(guids);
        }

        public void AddOutOfRangeGUID(ulong guid)
        {
            outOfRangeGUIDs.Add(guid);
        }

        public void AddUpdateBlock(PacketWriter block)
        {
            data.WriteBytes(block);
            BlockCount++;
        }

        public void SendPackets(WorldSession session)
        {
            PacketWriter packet;
            if (HasData())
            {
                packet = new PacketWriter(Opcodes.SMSG_UpdateObject);
                packet.WriteUInt16(MapId);
                packet.WriteUInt32(outOfRangeGUIDs.Count != 0 ? BlockCount + 1 : BlockCount);
                if (outOfRangeGUIDs.Count != 0)
                {
                    packet.WriteUInt8((byte)UpdateType.DestroyObjects);
                    packet.WriteUInt32(outOfRangeGUIDs.Count);

                    foreach (var guid in outOfRangeGUIDs)
                        packet.WritePackedGuid(guid);
                }

                packet.WriteBytes(data);           
                session.Send(packet);
            }
        }
        public void SendPackets(ref Player pl)
        {
            var session = pl.GetSession();
            SendPackets(session);
        }

        public void Clear()
        {
            data = new PacketWriter();
            outOfRangeGUIDs.Clear();
            BlockCount = 0;
            MapId = 0;
        }

        public bool HasData() { return BlockCount > 0 || outOfRangeGUIDs.Count != 0; }

        public List<ulong> GetOutOfRangeGUIDs() { return outOfRangeGUIDs; }

        public void SetMapId(ushort mapId) { MapId = mapId; }

        public bool BuildPacket(ref PacketWriter packet)
        {
            packet = new PacketWriter(Opcodes.SMSG_UpdateObject);

            packet.WriteUInt16(MapId);
            packet.WriteUInt32(BlockCount + (outOfRangeGUIDs.Count == 0 ? 0 : 1));

            if (outOfRangeGUIDs.Count != 0)
            {
                packet.WriteUInt8((byte)UpdateType.DestroyObjects);
                packet.WriteUInt32(outOfRangeGUIDs.Count);

                foreach (var guid in outOfRangeGUIDs)
                    packet.WritePackedGuid(guid);
            }

            packet.WriteBytes(data);
            return true;
        }

        uint MapId;
        uint BlockCount;
        List<ulong> outOfRangeGUIDs;
        PacketWriter data;
    }
}
