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

namespace Framework.Constants
{
    public enum GuildRankRights
    {
        Empty = 0x00000040,
        ChatListen = 0x00000041,
        ChatSpeak = 0x00000042,
        OffChatListen = 0x00000044,
        OffChatSpeak = 0x00000048,
        Promote = 0x000000C0,
        Demote = 0x00000140,
        Invite = 0x00000050,
        Remove = 0x00000060,
        SetMOTD = 0x00001040,
        EditPublicNote = 0x00002040,
        ViewOffNote = 0x00004040,
        EditOffNote = 0x00008040,
        ModifyInfo = 0x00010040,
        WithdrawGoldLock = 0x00020000,               // remove money withdraw capacity
        WithdrawRepair = 0x00040040,               // withdraw for repair
        WithdrawGold = 0x00080000,               // withdraw gold
        CreateEvent = 0x00100040,               // wotlk
        RequireAuthenticator = 0x00200040,
        RemoveEvent = 0x00800040,
        All = 0x00DDFFBF
    }
    public enum GuildDefaultRanks
    {
        Master = 0,
        Officer = 1,
        Veteran = 2,
        Member = 3,
        Initiate = 4,
    }
    public enum GuildMemberFlags
    {
        Offline = 0,
        Online = 1,
        AFK = 2,
        DND = 3,
        Mobile = 4 //not used
    }
    public enum GuildCommandType
    {
        Create_S = 0,
        Invite_S = 1,
        Quit_S = 3,
        GetRoster = 5,
        PromotePlayer = 6,
        DemotePlayer = 7,
        RemovePlayer = 8,
        ChangeLeader = 10,
        EditMOTD = 11,
        GuildChat = 13,
        Founder = 14,
        ChangeRank = 16,
        EditPublicNote = 19,
        Unk = 20,
        ViewTab = 21,
        MoveItem = 22,
        Repair = 25
    }
    public enum GuildCommandErrors
    {
        Success = 0x00,
        GuildInternal = 0x01,
        AlreadyInGuild = 0x02,
        AlreadyInGuild_S = 0x03,
        InvitedToGuild = 0x04,
        AlreadyInvitedToGuild_S = 0x05,
        NameInvalid = 0x06,
        NameExists_S = 0x07,
        LeaderLeave = 0x08,
        PlayerNotInGuild = 0x09,
        PlayerNotInGuild_S = 0x0A,
        PlayerNotFound_S = 0x0B,
        NotAllied = 0x0C,
        RankTooHigh_S = 0x0D,
        RankTooLow_S = 0x0E,
        RanksLocked = 0x11,
        RankInUse = 0x12,
        IgnoringYou_S = 0x13,
        Unk1 = 0x14,
        WithdrawLimit = 0x19,
        NotEnoughMoney = 0x1A,
        BankFull = 0x1C,
        ItemNotFound = 0x1D,
        TooMuchMoney = 0x1F,
        WrongTab = 0x20,
        ReqAuthenticator = 0x22,
        BankVoucherFailed = 0x23
    }
    public enum GuildEvents
    {
        Promotion = 1,
        Demotion = 2,
        MOTD = 3,
        Joined = 4,
        Left = 5,
        Removed = 6,
        LeaderIs = 7,
        LeaderChanged = 8,
        Disbanded = 9,
        TabardChange = 10,
        RankUpdated = 11,
        RankCreated = 12,
        RankDeleted = 13,
        RankOrderChanged = 14,
        Founder = 15,
        SignedOn = 16,
        SignedOff = 17,
        BankBagsSlotsChanged = 18,
        BankTabPurchased = 19,
        BankTabUpdated = 20,
        BankUpdateMoney = 21,
        BankMoneyWithdrawn = 22,
        BankTextChanged = 23
    }
    public enum GuildEventLogTypes
    {
        InvitePlayer = 1,
        JoinGuild = 2,
        PromotePlayer = 3,
        DemotePlayer = 4,
        UninvitePlayer = 5,
        LeaveGuild = 6,
    }
    public enum GuildBankEventLogTypes
    {
        DepositItem = 1,
        WithdrawItem = 2,
        MoveItem = 3,
        DepositMoney = 4,
        WithdrawMoney = 5,
        RepairMoney = 6,
        MoveItem2 = 7,
        BuySlot = 9,
    }
    public enum GuildEmblem
    {
        Success = 0,
        InvalidTabardColors = 1,
        NoGuild = 2,
        NotGuildMaster = 3,
        NotEnoughMoney = 4,
        InvalidVendor = 5
    }
}
