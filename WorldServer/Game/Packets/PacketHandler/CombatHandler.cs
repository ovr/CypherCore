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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework.Network;
using WorldServer.Game.WorldEntities;
using Framework.ObjectDefines;
using Framework.Logging;
using WorldServer.Network;
using Framework.Constants;

namespace WorldServer.Game.Packets.PacketHandler
{
    public class CombatHandler
    {
        [ClientOpcode(Opcodes.CMSG_Attackswing)]
        public static void HandleAttackSwingOpcode(ref PacketReader packet, ref WorldSession session)
        {
            ulong guid = packet.ReadUInt64();

            Log.outDebug("WORLD: Recvd CMSG_ATTACKSWING Message guidlow:{0} guidhigh:{1}", ObjectGuid.GuidLowPart(guid), ObjectGuid.GuidHiPart(guid));

            var pl = session.GetPlayer();

            Unit pEnemy = Cypher.ObjMgr.GetUnit(pl, guid);

            if (pEnemy == null)
            {
                // stop attack state at client
                SendAttackStop(null, pl);
                return;
            }

            //if (!pl.IsValidAttackTarget(pEnemy))
            {
                // stop attack state at client
                //SendAttackStop(pEnemy, pl);
                //return;
            }

            //! Client explicitly checks the following before sending CMSG_ATTACKSWING packet,
            //! so we'll place the same check here. Note that it might be possible to reuse this snippet
            //! in other places as well.
            //if (Vehicle* vehicle = _player->GetVehicle())
            {
                //VehicleSeatEntry const* seat = vehicle->GetSeatForPassenger(_player);
                //ASSERT(seat);
                //if (!(seat->m_flags & VEHICLE_SEAT_FLAG_CAN_ATTACK))
                {
                    //SendAttackStop(pEnemy);
                    //return;
                }
            }

            pl.Attack(pEnemy, true);
        }

        [ClientOpcode(Opcodes.CMSG_Attackstop)]
        public static void HandleAttackStopOpcode(ref PacketReader packet, ref WorldSession session)
        {
            session.GetPlayer().AttackStop();
        }

        static void SendAttackStop(Unit enemy, Player pl)
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_Attackstop);
            data.WritePackedGuid(pl.GetPackGUID());
            data.WritePackedGuid(enemy != null ? enemy.GetPackGUID() : 0);          // must be packed guid
            data.WriteUInt32(0);                                      // unk, can be 1 also
            pl.GetSession().Send(data);
        }

    }
}
