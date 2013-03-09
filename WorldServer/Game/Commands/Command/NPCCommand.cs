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
using WorldServer.Game.WorldEntities;
using Framework.Constants;
using Framework.Database;
using Framework.Utility;
using WorldServer.Network;

namespace WorldServer.Game.Commands
{
    [CommandGroup("npc", "None", AccountTypes.GameMaster)]
    public class NPCCommand : CommandGroup
    {        
        [Command("info", "None", AccountTypes.Administrator)]
        public static bool HandleNpcInfoCommand(string[] args, CommandGroup cmd)
        {
            Creature target = cmd.GetSession().GetPlayer().GetSelection<Creature>();

            if (target == null)
                return cmd.SendErrorMessage(CypherStrings.SelectCreature);

            uint faction = target.GetCreatureTemplate().FactionA;
            NPCFlags npcflags = (NPCFlags)target.GetValue<uint>((int)UnitFields.NpcFlags);
            uint displayid = target.GetCreatureTemplate().ModelId[0];
            uint nativeid = target.GetCreatureTemplate().ModelId[0];
            uint Entry = target.GetEntry();
            //CreatureTemplate const* cInfo = target->GetCreatureTemplate();

            //ulong curRespawnDelay = target.->GetRespawnTimeEx()-time(NULL);
            //if (curRespawnDelay < 0)
            //curRespawnDelay = 0;
            //std::string curRespawnDelayStr = secsToTimeString(uint64(curRespawnDelay), true);
            //std::string defRespawnDelayStr = secsToTimeString(target->GetRespawnDelay(), true);

            cmd.SendSysMessage(CypherStrings.NpcinfoChar, target.GetGUIDLow(), target.GetGUIDLow(), faction, npcflags, Entry, displayid, nativeid);
            cmd.SendSysMessage(CypherStrings.NpcinfoLevel, target.GetCreatureTemplate().Maxlevel);
            //cmd.SendSysMessage(CypherStrings.NPCINFO_HEALTH, target.CurHealth);
            //cmd.SendSysMessage(CypherStrings.NPCINFO_FLAGS, target.GetValue<uint>((int)UnitFields.Flags), target.GetValue<uint>((int)UnitFields.DynamicFlags), target.Template.FactionA);
            //cmd.SendSysMessage(CypherStrings.COMMAND_RAWPAWNTIMES, defRespawnDelayStr.c_str(), curRespawnDelayStr.c_str());
            //cmd.SendSysMessage(CypherStrings.NPCINFO_LOOT,  cInfo->lootid, cInfo->pickpocketLootId, cInfo->SkinLootId);
            //cmd.SendSysMessage(CypherStrings.NPCINFO_DUNGEON_ID, target->GetInstanceId());
            //cmd.SendSysMessage(CypherStrings.NPCINFO_PHASEMASK, target->GetPhaseMask());
            //cmd.SendSysMessage(CypherStrings.NPCINFO_ARMOR, target->GetArmor());
            cmd.SendSysMessage(CypherStrings.NpcinfoPosition, target.Position.X, target.Position.Y, target.Position.Z);
            //cmd.SendSysMessage(CypherStrings.NPCINFO_AIINFO, target->GetAIName().c_str(), target->GetScriptName().c_str());

            if (Convert.ToBoolean(npcflags & NPCFlags.Vendor))
                cmd.SendSysMessage(CypherStrings.NpcinfoVendor);

            if (Convert.ToBoolean(npcflags & NPCFlags.Trainer))
                cmd.SendSysMessage(CypherStrings.NpcinfoTrainer);

            return true;
        }

        [CommandGroup("set", "Syntax: .npc set $subcommand", AccountTypes.Administrator, true)]
        public class SetCommands : CommandGroup
        {
            [Command("flag", "None", AccountTypes.GameMaster)]
            public static bool HandleNpcSetFlagCommand(string[] args, CommandGroup cmd)
            {
                if (args.Count() < 1)
                    return false;

                uint npcFlags;
                uint.TryParse(args[0], out npcFlags);

                Creature creature = cmd.GetSession().GetPlayer().GetSelection<Creature>();

                if (creature == null)
                    return cmd.SendErrorMessage(CypherStrings.SelectCreature);

                creature.SetValue<uint>(UnitFields.NpcFlags, npcFlags);

                PreparedStatement stmt = DB.World.GetPreparedStatement(WorldStatements.Upd_CreatureNPCFlag);

                stmt.AddValue(0, npcFlags);
                stmt.AddValue(1, creature.GetEntry());
                DB.World.Execute(stmt);

                return cmd.SendSysMessage(CypherStrings.ValueSavedRejoin);
            }
        }

    }
}
