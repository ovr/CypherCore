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
using System.Reflection;
using Framework.Constants;
using WorldServer.Network;
using Framework.Logging;
using Framework.Network;

namespace WorldServer.Game.Packets
{
    public static class PacketManager
    {
        static Dictionary<Opcodes, HandlePacket> OpcodeHandlers = new Dictionary<Opcodes, HandlePacket>();
        delegate void HandlePacket(ref PacketReader packet, ref WorldSession session);

        public static void DefineOpcodeHandler()
        {
            Assembly currentAsm = Assembly.GetExecutingAssembly();
            foreach (var type in currentAsm.GetTypes())
            {
                foreach (var methodInfo in type.GetMethods())
                {
                    foreach (var opcodeAttr in methodInfo.GetCustomAttributes<ClientOpcodeAttribute>())
                    {
                        OpcodeHandlers[opcodeAttr.Opcode] = (HandlePacket)Delegate.CreateDelegate(typeof(HandlePacket), methodInfo);
                    }
                }
            }
        }

        public static bool InvokeHandler(ref PacketReader reader, WorldSession session, Opcodes opcode)
        {
            if (OpcodeHandlers.ContainsKey(opcode))
            {
                OpcodeHandlers[opcode].Invoke(ref reader, ref session);
                return true;
            }
            else
            {
                Log.outDebug("Received unhandled opcode {0} from AcountId:{1}", opcode, session.GetAccountId());
                return false;

            }
        }
    }
}
