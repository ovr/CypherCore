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
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.Database;
using Framework.Logging;
using Framework.Constants;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;
using WorldServer.Game.Packets;

namespace WorldServer.Game.Commands
{
    [CommandGroup("modify", "", AccountTypes.Moderator)]
    public class ModifyCommand : CommandGroup
    {
        [Command("all", "", AccountTypes.Moderator)]
        public static bool HandleModifyASpeedCommand(string[] args, CommandGroup cmd)
        {
            if (args.Count() < 1)
                return false;

            float ASpeed;
            float.TryParse(args[0], out ASpeed);

            if (ASpeed > 50.0f || ASpeed < 0.1f)
                return cmd.SendErrorMessage(CypherStrings.BadValue);

            Player target = cmd.GetSession().GetPlayer().GetSelection<Player>();
            if (target == null)
                return cmd.SendErrorMessage(CypherStrings.NoCharSelected);

            string targetNameLink = cmd.GetNameLink(target);

            //if (target.isInFlight())
            {
                //chat.PSendSysMessage(LANG_CHAR_IN_FLIGHT, targetNameLink.c_str());
                //return false;
            }

            if (target != cmd.GetSession().GetPlayer())
                target.SendNotification(CypherStrings.YoursAspeedChanged, cmd.GetNameLink(target), ASpeed);

            target.SetSpeed(UnitMoveType.Walk,    ASpeed, true);
            target.SetSpeed(UnitMoveType.Run,     ASpeed, true);
            target.SetSpeed(UnitMoveType.Swim,    ASpeed, true);
            target.SetSpeed(UnitMoveType.Flight,     ASpeed, true);
            return cmd.SendSysMessage(CypherStrings.YouChangeAspeed, ASpeed, target.GetName());
        }
        /*
        //Edit Player Speed
        static bool HandleModifySpeedCommand(string[] args, WorldClass session)
    {
        if (!args)
            return false;

        float Speed = (float)atof((char)args);

        if (Speed > 50.0f || Speed < 0.1f)
        {
            chat.SendSysMessage(LANG_BAD_VALUE);
            
            return false;
        }

        Player target = chat.getSelectedPlayer();
        if (!target)
        {
            chat.SendSysMessage(LANG_NO_CHAR_SELECTED);
            
            return false;
        }

        // check online security
        if (chat.HasLowerSecurity(target, 0))
            return false;

        std::string targetNameLink = chat.GetNameLink(target);

        if (target->isInFlight())
        {
            chat.PSendSysMessage(LANG_CHAR_IN_FLIGHT, targetNameLink.c_str());
            
            return false;
        }

        chat.PSendSysMessage(LANG_YOU_CHANGE_SPEED, Speed, targetNameLink.c_str());
        if (chat.needReportToTarget(target))
            (new Chat(target)).PSendSysMessage(LANG_YOURS_SPEED_CHANGED, chat.GetNameLink().c_str(), Speed);

        target->SetSpeed(MOVE_RUN, Speed, true);

        return true;
    }

        //Edit Player Swim Speed
        static bool HandleModifySwimCommand(string[] args, WorldClass session)
    {
        if (!args)
            return false;

        float Swim = (float)atof((char)args);

        if (Swim > 50.0f || Swim < 0.1f)
        {
            chat.SendSysMessage(LANG_BAD_VALUE);
            
            return false;
        }

        Player target = chat.getSelectedPlayer();
        if (!target)
        {
            chat.SendSysMessage(LANG_NO_CHAR_SELECTED);
            
            return false;
        }

        // check online security
        if (chat.HasLowerSecurity(target, 0))
            return false;

        std::string targetNameLink = chat.GetNameLink(target);

        if (target->isInFlight())
        {
            chat.PSendSysMessage(LANG_CHAR_IN_FLIGHT, targetNameLink.c_str());
            
            return false;
        }

        chat.PSendSysMessage(LANG_YOU_CHANGE_SWIM_SPEED, Swim, targetNameLink.c_str());
        if (chat.needReportToTarget(target))
            (new Chat(target)).PSendSysMessage(LANG_YOURS_SWIM_SPEED_CHANGED, chat.GetNameLink().c_str(), Swim);

        target->SetSpeed(MOVE_SWIM, Swim, true);

        return true;
    }

        //Edit Player Walk Speed
        static bool HandleModifyBWalkCommand(string[] args, WorldClass session)
    {
        if (!args)
            return false;

        float BSpeed = (float)atof((char)args);

        if (BSpeed > 50.0f || BSpeed < 0.1f)
        {
            chat.SendSysMessage(LANG_BAD_VALUE);
            
            return false;
        }

        Player target = chat.getSelectedPlayer();
        if (!target)
        {
            chat.SendSysMessage(LANG_NO_CHAR_SELECTED);
            
            return false;
        }

        // check online security
        if (chat.HasLowerSecurity(target, 0))
            return false;

        std::string targetNameLink = chat.GetNameLink(target);

        if (target->isInFlight())
        {
            chat.PSendSysMessage(LANG_CHAR_IN_FLIGHT, targetNameLink.c_str());
            
            return false;
        }

        chat.PSendSysMessage(LANG_YOU_CHANGE_BACK_SPEED, BSpeed, targetNameLink.c_str());
        if (chat.needReportToTarget(target))
            (new Chat(target)).PSendSysMessage(LANG_YOURS_BACK_SPEED_CHANGED, chat.GetNameLink().c_str(), BSpeed);

        target->SetSpeed(MOVE_RUN_BACK, BSpeed, true);

        return true;
    }

        //Edit Player Fly
        static bool HandleModifyFlyCommand(string[] args, WorldClass session)
        {
            if (!args)
                return false;

            float FSpeed = (float)atof((char)args);

            if (FSpeed > 50.0f || FSpeed < 0.1f)
            {
                chat.SendSysMessage(LANG_BAD_VALUE);

                return false;
            }

            Player target = chat.getSelectedPlayer();
            if (!target)
            {
                chat.SendSysMessage(LANG_NO_CHAR_SELECTED);

                return false;
            }

            // check online security
            if (chat.HasLowerSecurity(target, 0))
                return false;

            chat.PSendSysMessage(LANG_YOU_CHANGE_FLY_SPEED, FSpeed, chat.GetNameLink(target).c_str());
            if (chat.needReportToTarget(target))
                (new Chat(target)).PSendSysMessage(LANG_YOURS_FLY_SPEED_CHANGED, chat.GetNameLink().c_str(), FSpeed);

            target->SetSpeed(MOVE_FLIGHT, FSpeed, true);

            return true;
        }
        */
        [Command("level", "", AccountTypes.GameMaster)]
        public static bool HandleModifyLevelCommand(string[] args, CommandGroup cmd)
        {
            if (args.Count() < 1)
                return false;

            Player target;
            int level;

            if (!cmd.extractPlayerTarget(args[0], out target))
                return cmd.SendErrorMessage(CypherStrings.PlayerNotFound);

            int.TryParse(args[0] == "\"" ? args[1] : args[0], out level);

            int oldlevel = (int)(target != null ? target.getLevel() : 0);//Player::GetLevelFromDB(targetGuid);
            int newlevel = level != 0 ? level : oldlevel;

            if (newlevel < 1)
                return true;

            if (newlevel > 255)
                newlevel = 255;

            if (cmd.GetSession().GetPlayer() != target)
                target.SendNotification(CypherStrings.YouChangeLvl, cmd.GetNameLink(target), newlevel);

            if (target != null)
            {
                target.GiveLevel((uint)newlevel);
                //player->InitTalentForLevel();
                target.SetValue<uint>(PlayerFields.XP, 0);

                if (oldlevel == newlevel)
                    cmd.SendSysMessage(CypherStrings.YoursLevelProgressReset, cmd.GetNameLink(target));
                else if (oldlevel < newlevel)
                    cmd.SendSysMessage(CypherStrings.YoursLevelUp, cmd.GetNameLink(target), newlevel);
                else
                    cmd.SendSysMessage(CypherStrings.YoursLevelDown, cmd.GetNameLink(target), newlevel);
            }
            else
            {
                // Update level and reset XP, everything else will be updated at login
                PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_UPD_LEVEL);
                stmt.AddValue(0, (byte)newlevel);
                stmt.AddValue(1, target.GetGUIDLow());
                DB.Characters.Execute(stmt);                
            }
            return true;
        }

        [Command("money", "", AccountTypes.GameMaster)]
        public static bool HandleModifyMoneyCommand(string[] args, CommandGroup handler)
        {
            if (args.Count() < 1)
                return false;

            Player target = handler.getSelectedPlayer();
            if (target == null)
                return handler.SendErrorMessage(CypherStrings.NoCharSelected);

            // check online security
            if (handler.HasLowerSecurity(target, 0))
                return false;

            long addmoney;
            long.TryParse(args[0], out addmoney);

            long moneyuser = (long)target.GetMoney();

            if (addmoney < 0)
            {
                ulong newmoney = (ulong)(moneyuser + addmoney);

                Log.outDebug(ObjMgr.GetCypherString(CypherStrings.CurrentMoney), moneyuser, addmoney, newmoney);
                if (newmoney <= 0)
                {
                    handler.SendSysMessage(CypherStrings.YouTakeAllMoney, handler.GetNameLink(target));
                    if (handler.needReportToTarget(target))
                       ChatHandler.SendSysMessage(target, CypherStrings.YoursAllMoneyGone, handler.GetNameLink());

                    target.SetMoney(0);
                }
                else
                {
                    if (newmoney > PlayerConst.MaxMoneyAmount)
                        newmoney = PlayerConst.MaxMoneyAmount;

                    handler.SendSysMessage(CypherStrings.YouTakeMoney, Math.Abs(addmoney), handler.GetNameLink(target));
                    if (handler.needReportToTarget(target))
                        ChatHandler.SendSysMessage(target, CypherStrings.YoursMoneyTaken, handler.GetNameLink(), Math.Abs(addmoney));
                    target.SetMoney(newmoney);
                }
            }
            else
            {
                handler.SendSysMessage( CypherStrings.YouGiveMoney, addmoney, handler.GetNameLink(target));
                if (handler.needReportToTarget(target))
                    ChatHandler.SendSysMessage(target, CypherStrings.YoursMoneyGiven, handler.GetNameLink(), addmoney);

                if (addmoney >= PlayerConst.MaxMoneyAmount)
                    target.SetMoney(PlayerConst.MaxMoneyAmount);
                else
                    target.ModifyMoney(addmoney);
            }

            Log.outDebug(ObjMgr.GetCypherString(CypherStrings.NewMoney), moneyuser, addmoney, target.GetMoney());
            return true;
        }
    }
}
