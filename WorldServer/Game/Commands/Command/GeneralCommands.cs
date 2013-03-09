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
using WorldServer.Network;

namespace WorldServer.Game.Commands
{
    [CommandGroup("commands", "Lists available commands for your user-level.")]
    public class CommandsCommandGroup : CommandGroup
    {
        public override bool Fallback(string[] parameters = null, CommandGroup command = null)
        {
            var output = "Available commands: ";
            foreach (var pair in CommandManager.GetCommandList())
            {
                if (command != null && command.GetSession() != null && pair.Key.MinUserLevel > command.GetSession().GetSecurity())
                    continue;
                output += pair.Key.Name + ", ";
            }

            return SendSysMessage(output.Substring(0, output.Length - 2) + ".\nType 'help <command>' to get help.");
        }
    }

    [CommandGroup("help", "")]
    public class HelpCommandGroup : CommandGroup
    {
        public override bool Fallback(string[] parameters = null, CommandGroup command = null)
        {
            return SendSysMessage("usage: help <command>");
        }

        public override bool Handle(string parameters, WorldSession session = null)
        {
            if (parameters == string.Empty)
                return this.Fallback();

            string output = string.Empty;
            bool found = false;
            var @params = parameters.Split(' ');
            var group = @params[0];

            foreach (var pair in CommandManager.GetCommandList())
            {
                if (group != pair.Key.Name)
                    continue;

                if (@params.Count() <= 1)
                    return SendSysMessage(pair.Key.Help);

                output = pair.Value.GetHelp(@params.Skip(1).ToArray());
                found = true;
            }

            if (!found)
                output = string.Format("Unknown command: {0} {1}", group, command);

            return SendSysMessage(output);
        }
    }
}
