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
using System.Reflection;
using System.Text.RegularExpressions;
using Framework.Constants;
using Framework.Database;
using Framework.Logging;
using WorldServer.Game.Managers;
using WorldServer.Game.Packets;
using WorldServer.Network;
using System.Threading;

namespace WorldServer.Game.Commands
{
    public class CommandManager
    {
        protected static Dictionary<CommandGroupAttribute, CommandGroup> CommandGroups = new Dictionary<CommandGroupAttribute, CommandGroup>();

        static CommandManager()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsSubclassOf(typeof(CommandGroup)) || Convert.ToBoolean(type.Attributes & TypeAttributes.NestedPublic))
                    continue;

                var attributes = (CommandGroupAttribute[])type.GetCustomAttributes(typeof(CommandGroupAttribute), true);
                if (attributes.Length == 0) continue;

                var groupAttribute = attributes[0];
                if (CommandGroups.ContainsKey(groupAttribute))
                    Log.outError("{0} Is already defined.", groupAttribute.Name);

                var commandGroup = (CommandGroup)Activator.CreateInstance(type);
                commandGroup.Register(groupAttribute);
                CommandGroups.Add(groupAttribute, commandGroup);
            }

            SQLResult result = DB.World.Select("SELECT * FROM command ORDER BY name ASC");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 Commands. DB table `command` is empty. Will use default.");
                return;
            }

            //todo  add support to modify commands from db.
        }

        public static void InitConsole()
        {
            while (true)
            {
                Thread.Sleep(1);
                Log.outInit("Cypher >> ");

                TryParse(System.Console.ReadLine());
            }
        }

        /// <summary>
        /// Tries to parse given line as a server command.
        /// </summary>
        /// <param name="line">The line to be parsed.</param>
        /// <param name="invokerClient">The invoker client if any.</param>
        /// <returns><see cref="bool"/></returns>
        public static bool TryParse(string line, WorldSession session = null)
        {
            string command;
            string parameters;
            var success = false;
            CommandGroup group = null;

            if (line == null)
                return false;
            if (line.Trim() == string.Empty)
                return false;

            if (!ExtractCommandAndParameters(line, session == null, out command, out parameters))
                return false;

            foreach (var pair in CommandGroups)
            {
                if (pair.Key.Name != command)
                    continue;

                group = pair.Value;
                success = group.Handle(parameters, session);
                break;
            }

            if (group == null)
                return SendErrorMessage(CypherStrings.NoCmd, session);

            if (!group.SentErrorMessage && !success)
                return SendErrorMessage(group.command.Help, session);

            return true;
        }

        public static bool ExtractCommandAndParameters(string line, bool console, out string command, out string parameters)
        {
            line = line.Trim();
            command = string.Empty;
            parameters = string.Empty;

            if (line == string.Empty)
                return false;

            if (!console)
            {
                if (line[0] != '.') // if line does not start with command-prefix
                    return false;
                line = line.Substring(1); // advance to actual command.
            }

            command = line.Split(' ')[0].ToLower(); // get command
            parameters = String.Empty;
            if (line.Contains(' ')) parameters = line.Substring(line.IndexOf(' ') + 1).Trim(); // get parameters if any.

            return true;
        }

        public static bool SendErrorMessage(CypherStrings str, WorldSession session, params object[] args)
        {
            string input = Cypher.ObjMgr.GetCypherString(str);
            string pattern = @"%(\d+(\.\d+)?)?(d|f|s|u)";

            int count = 0;
            string result = Regex.Replace(input, pattern, m =>
            {
                return String.Concat("{", count++, "}");
            });

            return SendErrorMessage(result, session, args);
        }

        public static bool SendErrorMessage(string str, WorldSession session, params object[] args)
        {
            string msg = string.Format(str, args);
            if (session == null)
                Log.outError(msg);
            else
                ChatHandler.SendSysMessage(session, msg);

            return true;
        }

        public static Dictionary<CommandGroupAttribute, CommandGroup> GetCommandList()
        {
            return CommandGroups;
        }
    }
}
