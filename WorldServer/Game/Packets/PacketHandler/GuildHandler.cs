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
using Framework.Network;
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;
using Framework.ObjectDefines;

namespace WorldServer.Game.Packets
{
    public class GuildHandler : Cypher
    {
        [ClientOpcode(Opcodes.CMSG_GuildQuery)]
        public static void HandleGuildQuery(ref PacketReader packet, ref WorldSession session)
        {
            ulong guildGuid = packet.ReadUInt64();
            ulong playerGuid = packet.ReadUInt64();
            Guild guild = GuildMgr.GetGuildByGuid(guildGuid);
            if (guild != null)
            {
                guild.SendQuery(ref session);
                return;
            }

            SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
        }

        [ClientOpcode(Opcodes.CMSG_GuildRoster)]
        public static void HandleGuildRoster(ref PacketReader packet, ref WorldSession session)
        {
            //var mask = packet.GetGuidMask(2, 3, 0, 6, 4, 7, 5, 1);
            //var guid = packet.GetGuid(mask, 0, 4, 5, 7, 1, 3, 6, 2);

            Guild guild = GuildMgr.GetGuildByGuid(session.GetPlayer().GuildGuid);

            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            guild.SendRoster(session);
        }

        [ClientOpcode(Opcodes.CMSG_GuildQueryRanks)]
        public static void HandleGuildRanks(ref PacketReader packet, ref WorldSession session)
        {
            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);

            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            guild.SendRanks(ref session);
        }

        [ClientOpcode(Opcodes.MSG_SaveGuildEmblem)]
        public static void HandleGuildEmblem(ref PacketReader packet, ref WorldSession session)
        {
            //ulong vendorGuid = packet.ReadUInt64();
            uint _EmblemStyle = packet.ReadUInt32();
            uint _EmblemColor = packet.ReadUInt32();
            uint _BorderStyle = packet.ReadUInt32();
            uint _BorderColor = packet.ReadUInt32();
            uint _BackgroundColor = packet.ReadUInt32();

            Player pl = session.GetPlayer();

            //MSG_SAVE_GUILD_EMBLEM = 0x2404,
            PacketWriter writer = new PacketWriter();
            writer.WriteUInt16(0x2404);
            
            //Creature *pCreature = GetPlayer()->GetNPCIfCanInteractWith(vendorGuid, UNIT_NPC_FLAG_TABARDDESIGNER);
            //if (!pCreature)
            //{
                //"That's not an emblem vendor!"
                //SendSaveGuildEmblem(ERR_GUILDEMBLEM_INVALIDVENDOR);
                //return;
            //}
            
            // remove fake death
            //if (GetPlayer()->hasUnitState(UNIT_STAT_DIED))
                //GetPlayer()->RemoveSpellsCausingAura(SPELL_AURA_FEIGN_DEATH);

            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);

            if (guild == null)
            {
                writer.WriteUInt32((uint)GuildEmblem.NoGuild);
                session.Send(writer);
                return;
            }

            if (pl.GetGUIDLow() != guild.GetLeaderGuid())
            {
                writer.WriteUInt32((uint)GuildEmblem.NotGuildMaster);
                session.Send(writer);
                return;
            }
            
            //pl.ModifyMoney(-10*GOLD);
            guild.EmblemStyle = _EmblemStyle;
            guild.EmblemColor = _EmblemColor;
            guild.BorderStyle = _BorderStyle;
            guild.BorderColor = _BorderColor;
            guild.BackgroundColor = _BackgroundColor;
            
            //"Guild Emblem saved."
            writer.WriteUInt32((uint)GuildEmblem.Success);
            session.Send(writer);
            guild.SendQuery(ref session);
        }

        [ClientOpcode(Opcodes.CMSG_GuildNewsUpdateSticky)]
        public static void HandleGuildNews(ref PacketReader packet, ref WorldSession session)
        {
            var mask = packet.GetGuidMask(4, 2, 6, 3, 5, 0, 1, 7);
            var guid = packet.GetGuid(mask, 4, 1, 5, 6, 0, 3, 7, 2);
        }

        [ClientOpcode(Opcodes.CMSG_GuildInviteByName)]
        public static void HandleGuildInviteName(ref PacketReader packet, ref WorldSession session)
        {
            uint length = packet.GetBits<uint>(7);
            string name = packet.ReadString(length);

            Player plInvited = null;// ObjMgr.GetPlayer(name);
            if (plInvited == null)
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.PlayerNotFound_S, name);
                return;
            }

            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (plInvited.GuildGuid != 0)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.AlreadyInGuild_S, name);
                return;
            }

            plInvited.SetGuildInvited(guild.Guid);
            guild.SendInvite(pl, plInvited);

            guild.LogGuildEvent(GuildEventLogTypes.InvitePlayer, pl.GetGUIDLow(), plInvited.GetGUIDLow());
        }

        [ClientOpcode(Opcodes.CMSG_GuildRemove)]
        public static void HandleGuildRemove(ref PacketReader packet, ref WorldSession session)
        {
            var mask = packet.GetGuidMask(6, 5, 4, 0, 1, 3, 7, 2);
            var guid = packet.GetGuid(mask, 2, 6, 5, 7, 1, 4, 3, 0);
        }

        [ClientOpcode(Opcodes.CMSG_GuildAccept)]
        public static void HandleGuildAccept(ref PacketReader packet, ref WorldSession session)
        {
            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildInvited);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            guild.AddMember(pl.GetGUIDLow(), guild.GetLowestRank());

            guild.LogGuildEvent(GuildEventLogTypes.JoinGuild, pl.GetGUIDLow());
            guild.SendBroadcastEvent(GuildEvents.Joined, pl.GetGUIDLow(), pl.GetName());
        }

        [ClientOpcode(Opcodes.CMSG_GuildLeave)]
        public static void HandleGuildLeave(ref PacketReader packet, ref WorldSession session)
        {
            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (pl.GetGUIDLow() == guild.GetLeaderGuid())
            {
                if (guild.GetMemberSize() > 1)
                {
                    SendCommandResult(ref session, GuildCommandType.Quit_S, GuildCommandErrors.LeaderLeave);
                    return;
                }
                else
                {
                    SendCommandResult(ref session, GuildCommandType.Quit_S, GuildCommandErrors.Success, guild.Name);
                    guild.Disband();
                }
            }
            else
            {
                guild.DeleteMember(pl.GetGUIDLow());
                guild.LogGuildEvent(GuildEventLogTypes.LeaveGuild, pl.GetGUIDLow());
                guild.SendBroadcastEvent(GuildEvents.Left, pl.GetGUIDLow(), pl.GetName());
            }
            guild.SendRoster();
        }

        [ClientOpcode(Opcodes.CMSG_GuildDisband)]
        public static void HandleGuildDisband(ref PacketReader packet, ref WorldSession session)
        {
            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (pl.GetGUIDLow() != guild.GetLeaderGuid())
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }
            guild.Disband();
        }

        [ClientOpcode(Opcodes.CMSG_GuildDecline)]
        public static void HandleGuildDecline(ref PacketReader packet, ref WorldSession session)
        {
            Player pl = session.GetPlayer();
            pl.SetGuildInvited();
            pl.SetInGuild();
        }

        [ClientOpcode(Opcodes.CMSG_GuildMotd)]
        public static void HandleGuildSetMOTD(ref PacketReader packet, ref WorldSession session)
        {
            uint length = packet.GetBits<uint>(11);
            string motd = packet.ReadString(length);

            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (!guild.HasRankRight(guild.GetMember(pl.GetGUIDLow()).RankId, (uint)GuildRankRights.SetMOTD))
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }
        }

        [ClientOpcode(Opcodes.CMSG_GuildInfoText)]
        public static void HandleGuildSetInfo(ref PacketReader packet, ref WorldSession session)
        {
            uint length = packet.GetBits<uint>(12);
            string info = packet.ReadString(length);

            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (!guild.HasRankRight(guild.GetMember(pl.GetGUIDLow()).RankId, (uint)GuildRankRights.ModifyInfo))
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }
            guild.SetINFO(info);
        }

        [ClientOpcode(Opcodes.CMSG_GuildSetNote)]
        public static void HandleGuildSetNote(ref PacketReader packet, ref WorldSession session)
        {
            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            byte[] guid = new byte[8];
            guid[1] = packet.GetBit();
            guid[4] = packet.GetBit();
            guid[5] = packet.GetBit();
            guid[3] = packet.GetBit();
            guid[0] = packet.GetBit();
            guid[7] = packet.GetBit();

            bool Public = packet.ReadBit();

            guid[6] = packet.GetBit();

            uint noteLen = packet.GetBits<uint>(8);

            guid[2] = packet.GetBit();

            packet.ReadXORByte(guid, 4);
            packet.ReadXORByte(guid, 5);
            packet.ReadXORByte(guid, 0);
            packet.ReadXORByte(guid, 3);
            packet.ReadXORByte(guid, 1);
            packet.ReadXORByte(guid, 6);
            packet.ReadXORByte(guid, 7);

            string note = packet.ReadString(noteLen);

            packet.ReadXORByte(guid, 2);

            if (!guild.HasRankRight(guild.GetMember(pl.GetGUIDLow()).RankId, (uint)(Public ? GuildRankRights.EditPublicNote : GuildRankRights.EditOffNote)))
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }

            ulong Guid = packet.ReadGuid(guid);
            Guild.Member member = guild.GetMember(Guid);

            if (member == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }
            
            member.SetNote(Public, note);
            
            guild.SendRoster();
        }

        [ClientOpcode(Opcodes.CMSG_GuildEventLogQuery)]
        public static void HandleGuildEventLogQuery(ref PacketReader packet, ref WorldSession session)
        {
            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }
            guild.SendGuildEventLog(ref session);
        }

        //Ranks
        [ClientOpcode(Opcodes.CMSG_GuildAddRank)]
        public static void HandleGuildAddRank(ref PacketReader packet, ref WorldSession session)
        {
            uint rankId = packet.ReadUInt32();
            var count = packet.GetBits<uint>(7);
            string rankname = packet.ReadString(count);

            Player pl = session.GetPlayer();

            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (pl.GetGUIDLow() != guild.GetLeaderGuid())
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }

            guild.AddNewRank(ref session, rankname);
            
            guild.SendQuery(ref session);
            guild.SendRoster();
        }

        [ClientOpcode(Opcodes.CMSG_GuildDelRank)]
        public static void HandleGuildDelRank(ref PacketReader packet, ref WorldSession session)
        {
            uint rankId = packet.ReadUInt32();

            Player pl = session.GetPlayer();

            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (pl.GetGUIDLow() != guild.GetLeaderGuid())
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }

            guild.DeleteRank(ref session, rankId);
        }

        [ClientOpcode(Opcodes.CMSG_GuildSetRankPermissions)]
        public static void HandleGuildSetRankPermissions(ref PacketReader packet, ref WorldSession session)
        {            
            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);

            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (pl.GetGUIDLow() != guild.GetLeaderGuid())
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }
            uint rankId = packet.ReadUInt32();
            uint oldrights = packet.ReadUInt32();
            uint newrights = packet.ReadUInt32();
            
            for (uint i = 0; i < Guild.MaxBankTabs; ++i)
            { 
                uint BankSlotPerDay = packet.ReadUInt32();
                uint BankRights = packet.ReadUInt32();
                guild.SetBankRightsAndSlots(rankId, i, (ushort)(BankRights & 0xFF), (ushort)BankSlotPerDay);
            }
            uint MoneyPerDay = packet.ReadUInt32();
            packet.Skip(4);

            uint length = packet.GetBits<uint>(7);
            string rankname = packet.ReadString(length);

            guild.SetRankInfo(rankId, newrights, rankname, MoneyPerDay);
            
            guild.SendQuery(ref session);
            guild.SendRoster();                   
        }

        [ClientOpcode(Opcodes.CMSG_GuildSwitchRank)]
        public static void HandleGuildSwitchRank(ref PacketReader packet, ref WorldSession session)
        {
            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);

            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            uint rankId = packet.ReadUInt32();
            byte Direction = packet.GetBits<byte>(1);

            guild.SetRankOrder(rankId, Direction);
        }

        [ClientOpcode(Opcodes.CMSG_GuildAssignMemberRank)]
        public static void HandleGuildAssignRank(ref PacketReader packet, ref WorldSession session)
        {
            uint rankId = packet.ReadUInt32();

            var TGuid = new byte[8];
            var Guid = new byte[8];

            TGuid[1] = packet.GetBit();
            TGuid[7] = packet.GetBit();
            Guid[4] = packet.GetBit();
            Guid[2] = packet.GetBit();
            TGuid[4] = packet.GetBit();
            TGuid[5] = packet.GetBit();
            TGuid[6] = packet.GetBit();
            Guid[1] = packet.GetBit();
            Guid[7] = packet.GetBit();
            TGuid[2] = packet.GetBit();
            TGuid[3] = packet.GetBit();
            TGuid[0] = packet.GetBit();
            Guid[6] = packet.GetBit();
            Guid[3] = packet.GetBit();
            Guid[0] = packet.GetBit();
            Guid[5] = packet.GetBit();

            packet.ReadXORByte(TGuid, 0);
            packet.ReadXORByte(Guid, 1);
            packet.ReadXORByte(Guid, 3);
            packet.ReadXORByte(Guid, 5);
            packet.ReadXORByte(TGuid, 7);
            packet.ReadXORByte(TGuid, 3);
            packet.ReadXORByte(Guid, 0);
            packet.ReadXORByte(TGuid, 1);
            packet.ReadXORByte(Guid, 6);
            packet.ReadXORByte(TGuid, 2);
            packet.ReadXORByte(TGuid, 5);
            packet.ReadXORByte(TGuid, 4);
            packet.ReadXORByte(Guid, 2);
            packet.ReadXORByte(Guid, 4);
            packet.ReadXORByte(Guid, 6);
            packet.ReadXORByte(Guid, 7);

            ObjectGuid TargetGuid = new ObjectGuid(TGuid);
            ObjectGuid PlGuid = new ObjectGuid(Guid);

            Player pl = ObjMgr.FindPlayer(PlGuid);
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            Guild.Member member = guild.GetMember(TargetGuid);
            if (member == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            bool promote = false;
            if (member.RankId > rankId)
                promote = true;

            member.ChangeRank(rankId);

            guild.LogGuildEvent(promote ? GuildEventLogTypes.PromotePlayer : GuildEventLogTypes.DemotePlayer, pl.GetGUIDLow(), member.CharGuid, rankId);
            guild.SendBroadcastEvent(promote ? GuildEvents.Promotion : GuildEvents.Demotion, 0, pl.GetName(), member.Name, guild.GetRankName(rankId));

            guild.SendRoster();
        }

        [ClientOpcode(Opcodes.CMSG_GuildPromote)]
        public static void HandleGuildPromote(ref PacketReader packet, ref WorldSession session)
        {
            string name = packet.ReadString();

            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (!guild.HasRankRight(guild.GetMember(pl.GetGUIDLow()).RankId, (uint)GuildRankRights.Promote))
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }
            
            Guild.Member member = guild.GetMember(name);

            if (member == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            Guild.Member _member = guild.GetMember(pl.GetGUIDLow());
            if (_member.RankId + 1 >= member.RankId)
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooHigh_S);
                return;
            }
            
            uint newRankId = member.RankId - 1;
            
            member.ChangeRank(newRankId);

            guild.LogGuildEvent(GuildEventLogTypes.PromotePlayer, pl.GetGUIDLow(), member.CharGuid, newRankId);
            guild.SendBroadcastEvent(GuildEvents.Promotion, 0, _member.Name, member.Name, guild.GetRankName(newRankId));
        }

        [ClientOpcode(Opcodes.CMSG_GuildDemote)]
        public static void HandleGuildDemote(ref PacketReader packet, ref WorldSession session)
        {
            string name = packet.ReadString();

            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (!guild.HasRankRight(guild.GetMember(pl.GetGUIDLow()).RankId, (uint)GuildRankRights.Demote))
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }
            
            Guild.Member member = guild.GetMember(name);

            if (member == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            Guild.Member _member = guild.GetMember(pl.GetGUIDLow());
            if (_member.RankId >= member.RankId)
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooHigh_S);
                return;
            }
            
            uint newRankId = member.RankId + 1;

            member.ChangeRank(newRankId);

            guild.LogGuildEvent(GuildEventLogTypes.DemotePlayer, pl.GetGUIDLow(), member.CharGuid, newRankId);
            guild.SendBroadcastEvent(GuildEvents.Demotion, 0, _member.Name, member.Name,  guild.GetRankName(newRankId));
        }

        [ClientOpcode(Opcodes.CMSG_GuildSetGuildMaster)]
        public static void HandleGuildSetMaster(ref PacketReader packet, ref WorldSession session)
        {
            string name = packet.ReadString();

            Player pl = session.GetPlayer();
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (pl.GetGUIDLow() != guild.GetLeaderGuid())
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }

            Guild.Member oldleader = guild.GetMember(pl.GetGUIDLow());
            Guild.Member newleader = guild.GetMember(name);

            if (newleader == null)
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.PlayerNotInGuild_S, name);
                return;
            }
            
            guild.SetLeader(newleader);
            oldleader.ChangeRank((uint)guild.GetLowestRank());
            guild.SendBroadcastEvent(GuildEvents.LeaderChanged, 0, oldleader.Name, name);
}

        [ClientOpcode(Opcodes.CMSG_GuildReplaceGuildMaster)]
        public static void HandleGuildReplaceMaster(ref PacketReader packet, ref WorldSession session)
        {
            throw new NotImplementedException();
        }

        //Leveling
        [ClientOpcode(Opcodes.CMSG_QueryGuildRewards)]
        public static void HandleGuildRewards(ref PacketReader packet, ref WorldSession session)
        {
            Guild guild = GuildMgr.GetGuildByGuid(session.GetPlayer().GuildGuid);

            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }
            
            packet.Skip(4);//time?
            
            if (GuildMgr.guildRewards.Count == 0)
                return;

            uint count = (uint)GuildMgr.guildRewards.Count;

            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildRewardsList);

            writer.WriteBits(count, 21);
            writer.BitFlush();
            foreach (GuildRewards reward in GuildMgr.guildRewards)
            {
                writer.WriteUInt32(reward.Standing);
                writer.WriteUInt32(reward.Races);
                writer.WriteUInt32(reward.Entry);
                writer.WriteUInt64(reward.Price);
                writer.WriteUInt32(0);//always 0
                writer.WriteUInt32(reward.Achievement);
            }
            writer.WriteUnixTime();
            session.Send(writer);
        }

        [ClientOpcode(Opcodes.CMSG_QueryGuildXp)]
        public static void HandleGuildXP(ref PacketReader packet, ref WorldSession session)
        {
            var mask = packet.GetGuidMask(2, 1, 0, 5, 4, 7, 6, 3);
            var guid = packet.GetGuid(mask, 0, 2, 3, 6, 1, 5, 7, 4);

            Player pl = session.GetPlayer();//ObjMgr.GetPlayer(guid);
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildSendXp);
            writer.WriteUInt64(0); //Member Total XP
            writer.WriteUInt64(guild.NextLevelExperience); //Guild xp for next level
            writer.WriteUInt64(guild.TodayExperience);//Guild xp today
            writer.WriteUInt64(0); //Member Weekly xp
            writer.WriteUInt64(guild.Experience); //Guild Current xp
            session.Send(writer);
        }

        [ClientOpcode(Opcodes.CMSG_GuildRequestMaxDailyXp)]
        public static void HandleGuildRequestMaxDailyXP(ref PacketReader packet, ref WorldSession session)
        {
            var mask = packet.GetGuidMask(0, 3, 5, 1, 4, 6, 7, 2);
            var guid = packet.GetGuid(mask, 0, 4, 3, 5, 1, 2, 6, 7);

            Player pl = session.GetPlayer();//ObjMgr.GetPlayer(guid);
            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildSendMaxDailyXp);
            writer.WriteUInt64(guild.NextLevelExperience);
            session.Send(writer);
        }

        [ClientOpcode(Opcodes.CMSG_GuildRequestChallengeUpdate)]
        public static void HandleGuildChallengeUpdated(ref PacketReader packet, ref WorldSession session)
        {
            /*
            for (int i = 0; i < 4; ++i)
                packet.ReadInt32("Guild Experience Reward", i);

            for (int i = 0; i < 4; ++i)
                packet.ReadInt32("Gold Reward Unk 1", i);

            for (int i = 0; i < 4; ++i)
                packet.ReadInt32("Total Count", i);

            for (int i = 0; i < 4; ++i)
                packet.ReadInt32("Gold Reward Unk 2", i); // requires perk Cash Flow?

            for (int i = 0; i < 4; ++i)
                packet.ReadInt32("Current Count", i);
             */
        }

        [ClientOpcode(Opcodes.CMSG_RequestGuildPartyState)]
        public static void HandleRequestPartyState(ref PacketReader packet, ref WorldSession session)
        {
            var mask = packet.GetGuidMask(0, 6, 7, 3, 5, 1, 2, 4);
            var guid = packet.GetGuid(mask, 6, 3, 2, 1, 5, 0, 7, 4);
        }

        //Bank
        [ClientOpcode(Opcodes.CMSG_GuildBankBuyTab)]
        public static void HandleGuildBankBuyTab(ref PacketReader packet, ref WorldSession session)
        {
            ulong GoGuid = packet.ReadUInt64();
            
            ushort tabId = packet.ReadUInt8();

            Player pl = session.GetPlayer();

            Guild guild = GuildMgr.GetGuildByGuid(pl.GuildGuid);
            if (guild == null)
            {
                SendCommandResult(ref session, GuildCommandType.Create_S, GuildCommandErrors.PlayerNotInGuild);
                return;
            }

            if (pl.GetGUIDLow() != guild.GetLeaderGuid())
            {
                SendCommandResult(ref session, GuildCommandType.Invite_S, GuildCommandErrors.RankTooLow_S);
                return;
            }

            //if (tabId != guild.getPurchasedTabs())
                //return;
            
            //uint32 TabCost = GetGuildBankTabPrice(TabId) * GOLD;
            //if (!TabCost)
                //return;
            
            // Go on with creating tab
            //guild.CreateNewBankTab();
            //pl.ModifyMoney(-int(TabCost));
            //guild.SetBankRightsAndSlots(GetPlayer()->GetRank(), TabId, GUILD_BANK_RIGHT_FULL, WITHDRAW_SLOT_UNLIMITED, true);
            guild.SendRoster();                                       // broadcast for tab rights update
            //guild.DisplayGuildBankTabsInfo(this);
        }

        [ClientOpcode(Opcodes.SMSG_GuildBankQuery)]
        public static void HandleGuildBankList(ref PacketReader packet, ref WorldSession session)
        {

        }

        [ClientOpcode(Opcodes.CMSG_QueryGuildMembersForRecipe)]
        public static void HandleGuildMembersForRecipe(ref PacketReader packet, ref WorldSession session)
        {
            var mask = packet.GetGuidMask(4, 1, 0, 3, 6, 7, 5, 2);
            var guid = packet.GetGuid(mask, 1, 6, 5, 0, 3, 7, 2, 4);
        }

        [ClientOpcode(Opcodes.CMSG_QueryGuildRecipes)]
        public static void HandleGuildQueryMembersRecipe(ref PacketReader packet, ref WorldSession session)
        {
            var guildGuid = new byte[8];
            var guid = new byte[8];

            int SkillId = packet.ReadInt32();

            guid[2] = packet.GetBit();
            guildGuid[1] = packet.GetBit();
            guid[1] = packet.GetBit();
            guildGuid[0] = packet.GetBit();
            guildGuid[6] = packet.GetBit();
            guid[7] = packet.GetBit();
            guildGuid[4] = packet.GetBit();
            guildGuid[3] = packet.GetBit();
            guildGuid[7] = packet.GetBit();
            guid[5] = packet.GetBit();
            guid[0] = packet.GetBit();
            guildGuid[5] = packet.GetBit();
            guid[3] = packet.GetBit();
            guid[6] = packet.GetBit();
            guildGuid[2] = packet.GetBit();
            guid[4] = packet.GetBit();

            packet.ReadXORByte(guid, 2);
            packet.ReadXORByte(guid, 6);
            packet.ReadXORByte(guildGuid, 4);
            packet.ReadXORByte(guildGuid, 2);
            packet.ReadXORByte(guid, 1);
            packet.ReadXORByte(guildGuid, 7);
            packet.ReadXORByte(guildGuid, 3);
            packet.ReadXORByte(guildGuid, 1);
            packet.ReadXORByte(guid, 3);
            packet.ReadXORByte(guid, 0);
            packet.ReadXORByte(guildGuid, 0);
            packet.ReadXORByte(guid, 7);
            packet.ReadXORByte(guid, 4);
            packet.ReadXORByte(guildGuid, 5);
            packet.ReadXORByte(guildGuid, 6);
            packet.ReadXORByte(guid, 5);

            ObjectGuid guildguid = new ObjectGuid(guildGuid);
            ObjectGuid plguid = new ObjectGuid(guid);
        }

        public static void SendCommandResult(ref WorldSession session, GuildCommandType type, GuildCommandErrors errCode, string param = "")
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_GuildCommandResult2);
            writer.WriteUInt32((uint)type);
            writer.WriteCString(param);
            writer.WriteUInt32((uint)errCode);
            session.Send(writer);
        }
    }
}
