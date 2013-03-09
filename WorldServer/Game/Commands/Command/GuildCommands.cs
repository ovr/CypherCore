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
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;
using Framework.Constants;
using System.Text.RegularExpressions;
using WorldServer.Network;

namespace WorldServer.Game.Commands
{
    [CommandGroup("guild", "", AccountTypes.Administrator)]
    public class GuildCommands : CommandGroup
    {
        [Command("create", "", AccountTypes.GameMaster)]
        public static bool Create(string[] args, CommandGroup command)
        {
            if (args.Count() < 1)
                return false;

            Player target;
            if (!command.extractPlayerTarget(args[0], out target))
                return false;

            string guildname = command.extractQuotedArg(args[0].Contains("\"") ? args[0] : args[1]);
            if (guildname == null)
                return false;

            if (target.GuildGuid != 0)
                return command.SendErrorMessage(CypherStrings.PlayerInGuild);

            Guild guild = new Guild();
            if (!GuildMgr.CreateGuild(target, guildname))
                return command.SendErrorMessage(CypherStrings.GuildNotCreated);

            return true;
        }

        [Command("delete", "", AccountTypes.GameMaster)]
        public static bool Delete(string[] args, CommandGroup command)
        {
            string guildName = command.extractQuotedArg(args[0]);
            if (guildName == null)
                return false;

            Guild guild = GuildMgr.GetGuildByName(guildName);
            if (guild == null)
                return false;

            guild.Disband();
            return true;
        }

        [Command("invite", "", AccountTypes.GameMaster)]
        public static bool Invite(string[] args, CommandGroup command)
        {
            if (args.Count() < 1)
                return false;

            Player target;
            if (!command.extractPlayerTarget(args[0], out target))
                return false;

            string guildName = command.extractQuotedArg(args[0] == "\"" ? args[0] : args[1]);
            if (guildName == null)
                return false;

            Guild targetGuild = GuildMgr.GetGuildByName(guildName);
            if (targetGuild == null)
                return false;

            targetGuild.AddMember(target.GetGUIDLow());

            return true;
        }

        [Command("remove", "", AccountTypes.GameMaster)]
        public static bool UnInvite(string[] args, CommandGroup command)
        {
            if (args.Count() == 0)
                return false;

            Player target;
            if (!command.extractPlayerTarget(args[0], out target))
                return command.SendErrorMessage(CypherStrings.NoPlayer);

            Guild targetGuild = GuildMgr.GetGuildByGuid(target.GuildGuid);
            if (targetGuild == null)
                return false;

            targetGuild.DeleteMember(target.GetGUIDLow());

            return true;
        }

        [Command("rank", "", AccountTypes.GameMaster)]
        public static bool Rank(string[] args, CommandGroup command)
        {
            Player target;
            int rankId;

            if (!command.extractPlayerTarget(args[0], out target))
                return command.SendErrorMessage(CypherStrings.NoPlayer);               

            int.TryParse(Regex.IsMatch(args[0], @"\d") ? args[0] : args[1], out rankId);

            Guild targetGuild = GuildMgr.GetGuildByGuid(target.GuildGuid);
            if (targetGuild == null)
                return false;

            targetGuild.GetMember(target.GetGUIDLow()).ChangeRank((uint)rankId);

            return true;
        }

        [Command("level", "", AccountTypes.GameMaster)]
        public static bool Level(string[] args, CommandGroup command)
        {
            if (args.Count() < 2)
                return false;

            uint level;
            string guildName = command.extractQuotedArg(args[0]);
            if (guildName == null)
                return false;

            uint.TryParse(args[1], out level);

            Guild guild = GuildMgr.GetGuildByName(guildName);
            if (guild == null)
                return false;

            guild.SetLevel(level);
            return true;
        }
    }
}
