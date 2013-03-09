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

namespace WorldServer.Game.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandGroupAttribute : Attribute
    {
        /// <summary>
        /// Command group's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Help text for command group.
        /// </summary>
        public string Help { get; private set; }

        /// <summary>
        /// Allow Console?
        /// </summary>
        public bool AllowConsole { get; private set; }

        /// <summary>
        /// Minimum user level required to invoke the command.
        /// </summary>
        public AccountTypes MinUserLevel { get; private set; }

        public CommandGroupAttribute(string name, string help, AccountTypes minUserLevel = AccountTypes.Player, bool allowconsole = false)
        {
            this.Name = name.ToLower();
            this.Help = help;
            this.MinUserLevel = minUserLevel;
            this.AllowConsole = allowconsole;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Command's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Help text for command.
        /// </summary>
        public string Help { get; private set; }

        /// <summary>
        /// Allow Console?
        /// </summary>
        public bool AllowConsole { get; private set; }

        /// <summary>
        /// Minimum user level required to invoke the command.
        /// </summary>
        public AccountTypes MinUserLevel { get; private set; }

        public CommandAttribute(string command, string help, AccountTypes minUserLevel = AccountTypes.Player, bool allowconsole = false)
        {
            this.Name = command.ToLower();
            this.Help = help;
            this.MinUserLevel = minUserLevel;
            this.AllowConsole = allowconsole;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DefaultCommand : CommandAttribute
    {
        public DefaultCommand(AccountTypes minUserLevel = AccountTypes.Player)
            : base("", "", minUserLevel)
        {
        }
    }
}
