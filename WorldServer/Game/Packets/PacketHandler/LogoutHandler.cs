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

namespace WorldServer.Game.Packets
{
    public class LogoutHandler : Cypher
    {
        [ClientOpcode(Opcodes.CMSG_LogoutRequest)]
        public static void HandleLogout(ref PacketReader packet, ref WorldSession session)
        {
            var pl = session.GetPlayer();
            LogoutPlayer(pl, true);
        }

        [ClientOpcode(Opcodes.CMSG_LogDisconnect)]
        public static void HandleLogDisconnect(ref PacketReader packet, ref WorldSession session)
        {
            //var pl = session.GetPlayer();
            //LogoutPlayer(pl);
        }

        public static void LogoutPlayer(Player pChar, bool send = false)
        {
            ///- If the player is in a guild, update the guild roster and broadcast a logout message to other guild members
            //Guild guild = GuildMgr.GetGuildByGuid(pChar.GuildGuid);
            //if (guild != null)
                //guild.HandleMemberLogout(pChar);

            pChar.SaveToDB();
            //ObjMgr.RemovePlayer(pChar);
            pChar.GetMap().RemovePlayer(pChar);
            if (send)
            {
                PacketWriter data = new PacketWriter(Opcodes.SMSG_LogoutComplete);
                pChar.GetSession().Send(data);
            }
        }
    }
}
