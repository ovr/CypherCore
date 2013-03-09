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
using WorldServer.Game.WorldEntities;
using WorldServer.Network;

namespace WorldServer.Game.Commands
{
    [CommandGroup("gm", "None", AccountTypes.Moderator)]
    public class GMCommands : CommandGroup
    {
        [Command("chat", "None", AccountTypes.Moderator)]
        public static bool Chat(string[] args, CommandGroup command)
        {
            return false;
        }

        [Command("fly", "Syntax: .gm fly [on|off] (Turn Fly Mode.)", AccountTypes.Administrator)]
        public static bool Fly(string[] args, CommandGroup command)
        {
            if (args.Count() < 1)
                return false;

            Player target = command.getSelectedPlayer();
            if (target == null)
                target = command.GetSession().GetPlayer();

            if (args[0].ToLower() == "on")
                target.SendMovementSetCanFly(true);
            else if (args[0].ToLower() == "off")
                target.SendMovementSetCanFly(false);
            else
                return command.SendSysMessage("Use [on|off]");

            command.SendSysMessage(CypherStrings.CommandFlymodeStatus, command.GetNameLink(target), args);
            return true;
        }

        [Command("ingame", "None", AccountTypes.Player)]
        public static bool InGame(string[] args, CommandGroup command)
        {
            return false;
        }

        [Command("list", "None", AccountTypes.Administrator)]
        public static bool List(string[] args, CommandGroup command)
        {
            return false;
        }

        [Command("visible", "None", AccountTypes.Moderator)]
        public static bool Visible(string[] args, CommandGroup command)
        {
            return false;
        }
    }
}
