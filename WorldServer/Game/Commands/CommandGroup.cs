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
using Framework.Logging;
using WorldServer.Game.Managers;
using WorldServer.Game.Packets;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;

namespace WorldServer.Game.Commands
{
    public class CommandGroup : Cypher
    {
        public CommandGroupAttribute Attributes { get; private set; }
        public CommandAttribute command { get; private set; }
        public Boolean SentErrorMessage { get; private set; }

        private readonly Dictionary<CommandAttribute, MethodInfo> _commands =
            new Dictionary<CommandAttribute, MethodInfo>();
        private readonly Dictionary<CommandGroupAttribute, CommandGroup> _subgroups =
            new Dictionary<CommandGroupAttribute, CommandGroup>();

        public void Register(CommandGroupAttribute attributes)
        {
            this.Attributes = attributes;
            this.RegisterDefaultCommand();
            this.RegisterCommands();
        }

        private void RegisterCommands()
        {
            foreach (var method in this.GetType().GetMethods())
            {
                object[] attri = method.GetCustomAttributes(typeof(CommandGroup), true);

                object[] attributes = method.GetCustomAttributes(typeof(CommandAttribute), true);
                if (attributes.Length == 0) continue;

                var attribute = (CommandAttribute)attributes[0];
                if (attribute is DefaultCommand) continue;

                if (!this._commands.ContainsKey(attribute))
                    this._commands.Add(attribute, method);
                else
                    Log.outError("There exists an already registered command {0}.", attribute.Name);
            }

            foreach (var type in this.GetType().GetNestedTypes())
            {
                if (!type.IsSubclassOf(typeof(CommandGroup)))
                    continue;

                var attributes = (CommandGroupAttribute[])type.GetCustomAttributes(typeof(CommandGroupAttribute), true);
                if (attributes.Length == 0) continue;

                var groupAttribute = attributes[0];
                if (_subgroups.ContainsKey(groupAttribute))
                    Log.outError("{0} Is already defined.", groupAttribute.Name);

                var commandGroup = (CommandGroup)Activator.CreateInstance(type);
                commandGroup.Register(groupAttribute);
                _subgroups.Add(groupAttribute, commandGroup);
            }
        }

        private void RegisterDefaultCommand()
        {
            foreach (var method in this.GetType().GetMethods())
            {
                object[] attributes = method.GetCustomAttributes(typeof(DefaultCommand), true);
                if (attributes.Length == 0) continue;
                if (method.Name.ToLower() == "fallback") continue;

                this._commands.Add(new DefaultCommand(this.Attributes.MinUserLevel), method);
                return;
            }

            // set the fallback command if we couldn't find a defined DefaultCommand.
            this._commands.Add(new DefaultCommand(this.Attributes.MinUserLevel), this.GetType().GetMethod("Fallback"));
        }

        public virtual bool Handle(string parameters, WorldSession _session = null)
        {
            SetSentErrorMessage(false);
            session = _session;
            // check if the user has enough privileges to access command group.
            // check if the user has enough privileges to invoke the command.
            if (session != null && this.Attributes.MinUserLevel > session.GetSecurity())
                return SendErrorMessage(CypherStrings.NoCmd);

            string[] @params = null;
            
            if (parameters == string.Empty)
                command = this.GetDefaultSubcommand();
            else
            {
                @params = parameters.Split(' ');

                var blah = this.GetSubGroupCommand(@params[0]);
                CommandGroup group = null;
                if (blah.Key != null)
                {
                    group = blah.Value;
                    return group.Handle(string.Join(" ", @params.Skip(1)), session);
                }
                else
                    command = this.GetSubcommand(@params[0]) ?? this.GetDefaultSubcommand();

                if (command != this.GetDefaultSubcommand())
                    @params = @params.Skip(1).ToArray();
            }

            // check if the user has enough privileges to invoke the command.
            if (session != null && command.MinUserLevel > session.GetSecurity())
                return SendErrorMessage(CypherStrings.NoCmd);

            var success = (bool)this._commands[command].Invoke(this, new object[] { @params, this });

            if (!SentErrorMessage && !success)
                return SendErrorMessage(command.Help);

            return true;
        }

        public string GetHelp(string[] @params)
        {
            if (@params.Count() == 0)
                return Attributes.Help;

            foreach (var group in this._subgroups)
            {
                if (@params[0] != group.Key.Name)
                    continue;
                return group.Value.GetHelp(@params.Skip(1).ToArray());
            }

            foreach (var pair in this._commands)
            {
                if (@params[0] != pair.Key.Name)
                    continue;
                return pair.Key.Help;
            }
            return string.Empty;
        }

        [DefaultCommand]
        public virtual bool Fallback(string[] @params = null, CommandGroup cmd = null)
        {
            var output = "Available subcommands: ";
            foreach (var pair in this._subgroups)
            {
                if (pair.Key.Name.Trim() == string.Empty)
                    continue; // skip fallback command.

                if (cmd.GetSession() == null && !pair.Key.AllowConsole)
                    continue;

                if (cmd.GetSession() != null && pair.Key.MinUserLevel > cmd.GetSession().GetSecurity())
                    continue;

                output += pair.Key.Name + ", ";
            }
            foreach (var pair in this._commands)
            {
                if (pair.Key.Name.Trim() == string.Empty)
                    continue; // skip fallback command.

                if (cmd.GetSession() == null && !pair.Key.AllowConsole)
                    continue;

                if (cmd.GetSession() != null && pair.Key.MinUserLevel > cmd.GetSession().GetSecurity()) 
                    continue;

                output += pair.Key.Name + ", ";
            }
            return SendSysMessage(output.Substring(0, output.Length - 2) + ".");
        }

        protected CommandAttribute GetDefaultSubcommand()
        {
            return this._commands.Keys.First();
        }

        protected CommandAttribute GetSubcommand(string name)
        {
            return this._commands.Keys.FirstOrDefault(command => command.Name == name);
        }

        protected KeyValuePair<CommandGroupAttribute, CommandGroup> GetSubGroupCommand(string name)
        {
            return this._subgroups.FirstOrDefault(group => group.Key.Name == name);
        }

        public string extractQuotedArg(string str)
        {
            if (str == "")
                return null;

            if (!str.Contains("\""))
                return null;

            return str.Replace("\"", String.Empty);
        }
        public bool extractPlayerTarget(string arg, out Player player)
        {
            if (arg.Contains("\"") || !Regex.IsMatch(arg, @"\d"))
                player = ObjMgr.FindPlayerByName(arg);
            else
                player = getSelectedPlayer();// session.GetPlayer().GetSelection<Player>();

            return player != null;
        }
        public Player getSelectedPlayer()
        {
            if (session == null)
                return null;

            Player pl = session.GetPlayer().GetSelection<Player>();

            if (pl == null)
                return session.GetPlayer();

            return pl;
        }
        public string playerLink(string name, bool console = false) { return console ? name : "|cffffffff|Hplayer:" + name + "|h[" + name + "]|h|r"; }
        public string GetNameLink() { return GetNameLink(session.GetPlayer()); }
        public string GetNameLink(Player obj) { return playerLink(obj.GetName()); }
        public bool needReportToTarget(Player chr)
        {
            Player pl = session.GetPlayer();
            return pl != chr && pl.IsVisibleGloballyFor(chr);
        }
        public bool HasLowerSecurity(Player target, ulong guid, bool strong = false)
        {
            WorldSession target_session = null;
            uint target_account = 0;

            if (target != null)
                target_session = target.GetSession();
            else if (guid != 0)
                target_account = ObjMgr.GetPlayerAccountIdByGUID(guid);

            if (target_session == null && target_account == 0)
            {
                SendSysMessage(CypherStrings.PlayerNotFound);
                SetSentErrorMessage(true);
                return true;
            }

            return HasLowerSecurityAccount(target_session, target_account, strong);
        }
        bool HasLowerSecurityAccount(WorldSession target, uint target_account, bool strong = false)
        {
            uint target_sec;

            // allow everything from console and RA console
            if (session == null)
                return false;

            // ignore only for non-players for non strong checks (when allow apply command at least to same sec level)
            if (!ObjMgr.IsPlayerAccount(session.GetSecurity()) && !strong)// && !sWorld->getBoolConfig(CONFIG_GM_LOWER_SECURITY))
                return false;

            if (target != null)
                target_sec = (uint)target.GetSecurity();
            else if (target_account != 0)
                target_sec = AcctMgr.GetSecurity(target_account);
            else
                return true;                                        // caller must report error for (target == NULL && target_account == 0)

            AccountTypes target_ac_sec = (AccountTypes)target_sec;
            if (session.GetSecurity() < target_ac_sec || (strong && session.GetSecurity() <= target_ac_sec))
            {
                SendSysMessage(CypherStrings.YoursSecurityIsLow);
                SetSentErrorMessage(true);
                return true;
            }
            return false;
        }

        public WorldSession GetSession() { return session; }

        public bool SendErrorMessage(CypherStrings str, params object[] args)
        {
            SetSentErrorMessage(true);
            string input = ObjMgr.GetCypherString(str);
            string pattern = @"%(\d+(\.\d+)?)?(d|f|s|u)";

            int count = 0;
            string result = Regex.Replace(input, pattern, m =>
            {
                return String.Concat("{", count++, "}");
            });

            return SendSysMessage(result, args);
        }
        public bool SendErrorMessage(string str, params object[] args)
        {
            SetSentErrorMessage(true);
            string msg = string.Format(str, args);

            if (session == null)
                Log.outError(msg);
            else
                ChatHandler.SendSysMessage(session, msg);

            return true;
        }

        public bool SendSysMessage(CypherStrings str, params object[] args)
        {
            string input = ObjMgr.GetCypherString(str);
            string pattern = @"%(\d+(\.\d+)?)?(d|f|s|u)";
            
            int count = 0;
            string result = Regex.Replace(input, pattern, m =>
            {
                return String.Concat("{", count++, "}");
            });

            return SendSysMessage(result, args);
        }
        public bool SendSysMessage(string str, params object[] args)
        {
            string msg = string.Format(str, args);

            if (session == null)
                Log.outInfo(msg);
            else
                ChatHandler.SendSysMessage(session, msg);

            return true;
        }

        void SetSentErrorMessage(bool arg) { SentErrorMessage = arg; }

        WorldSession session { get; set; }
    }
}