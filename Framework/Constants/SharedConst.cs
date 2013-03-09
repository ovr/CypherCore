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
    public struct SharedConst
    {
        public const int MaxAccountTutorialValues = 8;


        //Spells
        public const int MaxSpellEffects = 21;
        public const int MaxEffectMask = 7;
        public const int MaxSpellReagents = 8;
        public const int MaxSpellTotems = 2;
        public const int MaxShapeshiftSpells = 8;

        //Map
        public const int RaidDifficultyMask25Man = 1;    // since 25man difficulties are 1 and 3, we can check them like that
        public const int MaxDungeonDifficulty = 3;
        public const int MaxRaidDifficulty = 4;
        public const int MaxDifficulty = 4;

        //
        public const int ClassMaskAllCreatures = (1 << ((int)UnitClass.Warrior - 1)) | (1 << ((int)UnitClass.Paladin - 1)) | (1 << ((int)UnitClass.Rogue - 1)) | (1 << ((int)UnitClass.Mage - 1));
        public const int ClassMaskWandUsers = (1 << ((int)Class.Priest - 1)) | (1 << ((int)Class.Mage - 1)) | (1 << ((int)Class.Warlock - 1));


        //Unit
        public const float DefaultWorldObjectSize = 0.388999998569489f;


        // All Gt* DBC store data for 100 levels, some by 100 per class/race
        public const int GTMaxLevel = 100;
        // gtOCTClassCombatRatingScalar.dbc stores data for 32 ratings, look at MAX_COMBAT_RATING for real used amount
        public const int GTMaxRating = 32;
    }

    public struct TimeConst
    {
        public const int MINUTE = 60;
        public const int HOUR = MINUTE * 60;
        public const int DAY = HOUR * 24;
        public const int WEEK = DAY * 7;
        public const int MONTH = DAY * 30;
        public const int YEAR = MONTH * 12;
        public const int IN_MILLISECONDS = 1000;
    }

    public enum TeamId
    {
        Alliance = 0,
        Horde,
        Neutral
    }
    public enum Team
    {
        Horde = 67,
        Alliance = 469,
        //TEAM_STEAMWHEEDLE_CARTEL = 169,                       // not used in code
        //TEAM_ALLIANCE_FORCES     = 891,
        //TEAM_HORDE_FORCES        = 892,
        //TEAM_SANCTUARY           = 936,
        //TEAM_OUTLAND             = 980,
        TeamOther = 0                            // if ReputationListId > 0 && Flags != FACTION_FLAG_TEAM_HEADER
    }

    public enum ResponseCodes
    {
        ResponseSuccess = 0,
        ResponseFailure = 1,
        ResponseCancelled = 2,
        ResponseDisconnected = 3,
        ResponseFailedToConnect = 4,
        ResponseConnected = 5,
        ResponseVersionMismatch = 6,

        CstatusConnecting = 7,
        CstatusNegotiatingSecurity = 8,
        CstatusNegotiationComplete = 9,
        CstatusNegotiationFailed = 10,
        CstatusAuthenticating = 11,

        AuthOk = 12,
        AuthFailed = 13,
        AuthReject = 14,
        AuthBadServerProof = 15,
        AuthUnavailable = 16,
        AuthSystemError = 17,
        AuthBillingError = 18,
        AuthBillingExpired = 19,
        AuthVersionMismatch = 20,
        AuthUnknownAccount = 21,
        AuthIncorrectPassword = 22,
        AuthSessionExpired = 23,
        AuthServerShuttingDown = 24,
        AuthAlreadyLoggingIn = 25,
        AuthLoginServerNotFound = 26,
        AuthWaitQueue = 27,
        AuthBanned = 28,
        AuthAlreadyOnline = 29,
        AuthNoTime = 30,
        AuthDbBusy = 31,
        AuthSuspended = 32,
        AuthParentalControl = 33,
        AuthLockedEnforced = 34,

        RealmListInProgress = 35,
        RealmListSuccess = 36,
        RealmListFailed = 37,
        RealmListInvalid = 38,
        RealmListRealmNotFound = 39,

        AccountCreateInProgress = 40,
        AccountCreateSuccess = 41,
        AccountCreateFailed = 42,

        CharListRetrieving = 43,
        CharListRetrieved = 44,
        CharListFailed = 45,

        CharCreateInProgress = 46,
        CharCreateSuccess = 47,
        CharCreateError = 48,
        CharCreateFailed = 49,
        CharCreateNameInUse = 50,
        CharCreateDisabled = 51,
        CharCreatePvpTeamsViolation = 52,
        CharCreateServerLimit = 53,
        CharCreateAccountLimit = 54,
        CharCreateServerQueue = 55,
        CharCreateOnlyExisting = 56,
        CharCreateExpansion = 57,
        CharCreateExpansionClass = 58,
        CharCreateLevelRequirement = 59,
        CharCreateUniqueClassLimit = 60,
        CharCreateCharacterInGuild = 61,
        CharCreateRestrictedRaceclass = 62,
        CharCreateCharacterChooseRace = 63,
        CharCreateCharacterArenaLeader = 64,
        CharCreateCharacterDeleteMail = 65,
        CharCreateCharacterSwapFaction = 66,
        CharCreateCharacterRaceOnly = 67,
        CharCreateCharacterGoldLimit = 68,
        CharCreateForceLogin = 69,

        CharDeleteInProgress = 70,
        CharDeleteSuccess = 71,
        CharDeleteFailed = 72,
        CharDeleteFailedLockedForTransfer = 73,
        CharDeleteFailedGuildLeader = 74,
        CharDeleteFailedArenaCaptain = 75,
        CharDeleteFailedHasHeirloomOrMail = 76,

        CharLoginInProgress = 77,
        CharLoginSuccess = 78,
        CharLoginNoWorld = 79,
        CharLoginDuplicateCharacter = 80,
        CharLoginNoInstances = 81,
        CharLoginFailed = 82,
        CharLoginDisabled = 83,
        CharLoginNoCharacter = 84,
        CharLoginLockedForTransfer = 85,
        CharLoginLockedByBilling = 86,
        CharLoginLockedByMobileAh = 87,
        CharLoginTemporaryGmLock = 88,

        CharNameSuccess = 89,
        CharNameFailure = 90,
        CharNameNoName = 91,
        CharNameTooShort = 92,
        CharNameTooLong = 93,
        CharNameInvalidCharacter = 94,
        CharNameMixedLanguages = 95,
        CharNameProfane = 96,
        CharNameReserved = 97,
        CharNameInvalidApostrophe = 98,
        CharNameMultipleApostrophes = 99,
        CharNameThreeConsecutive = 100,
        CharNameInvalidSpace = 101,
        CharNameConsecutiveSpaces = 102,
        CharNameRussianConsecutiveSilentCharacters = 103,
        CharNameRussianSilentCharacterAtBeginningOrEnd = 104,
        CharNameDeclensionDoesntMatchBaseName = 105,
    }
    public enum FactionMasks
    {
        Player = 1,                              // any player
        Alliance = 2,                              // player or creature from alliance team
        Horde = 4,                              // player or creature from horde team
        Monster = 8                               // aggressive creature from monster team
        // if none flags set then non-aggressive creature
    }
    public enum FactionTemplateFlags
    {
        PVP = 0x00000800,   // flagged for PvP
        ContestedGuard = 0x00001000,   // faction will attack players that were involved in PvP combats
        HostileByDefault = 0x00002000
    }
    public enum ReputationRank
    {
        None = -1,
        Hated = 0,
        Hostile = 1,
        Unfriendly = 2,
        Neutral = 3,
        Friendly = 4,
        Honored = 5,
        Revered = 6,
        Exalted = 7,
        Max = 8,
        Min = Hated
    }
    public enum FactionFlags
    {
        None = 0x00,                 // no faction flag
        Visible = 0x01,                 // makes visible in client (set or can be set at interaction with target of this faction)
        AtWar = 0x02,                 // enable AtWar-button in client. player controlled (except opposition team always war state), Flag only set on initial creation
        Hidden = 0x04,                 // hidden faction from reputation pane in client (player can gain reputation, but this update not sent to client)
        InvisibleForced = 0x08,                 // always overwrite FACTION_FLAG_VISIBLE and hide faction in rep.list, used for hide opposite team factions
        PeaceForced = 0x10,                 // always overwrite FACTION_FLAG_AT_WAR, used for prevent war with own team factions
        Inactive = 0x20,                 // player controlled, state stored in characters.data (CMSG_SET_FACTION_INACTIVE)
        Rival = 0x40,                 // flag for the two competing outland factions
        Special = 0x80                  // horde and alliance home cities and their northrend allies have this flag
    };
    public enum Gender
    {
        Male = 0,
        Female = 1,
        None = 2
    }
    public enum Class
    {
        None = 0,
        Warrior = 1,
        Paladin = 2,
        Hunter = 3,
        Rogue = 4,
        Priest = 5,
        Deathknight = 6,
        Shaman = 7,
        Mage = 8,
        Warlock = 9,
        Monk = 10,
        Druid = 11,
        Max = 12
    }
    public enum Race
    {
        None = 0,
        Human = 1,
        Orc = 2,
        Dwarf = 3,
        NightElf = 4,
        Undead = 5,
        Tauren = 6,
        Gnome = 7,
        Troll = 8,
        Goblin = 9,
        BloodElf = 10,
        Draenei = 11,
        //FelOrc = 12,
        //Naga = 13,
        //Broken = 14,
        //Skeleton = 15,
        //Vrykul = 16,
        //Tuskarr = 17,
        //ForestTroll = 18,
        //Taunka = 19,
        //NorthrendSkeleton = 20,
        //IceTroll = 21,
        Worgen = 22,
        //HumanGilneas = 23,
        PandarenNeutral = 24,
        PandarenAlliance = 25,
        PandarenHorde = 26,
        Max = 27
    }
    public enum Expansion
    {
        Classic = 0,
        BurningCrusade = 1,
        LichKing = 2,
        Cataclysm = 3,
        Pandaria = 4
    }
    public enum Powers : uint
    {
        Mana = 0,
        Rage = 1,
        Focus = 2,
        Energy = 3,
        Unused = 4,
        Runes = 5,
        RunicPower = 6,
        SoulShards = 7,
        Eclipse = 8,
        HolyPower = 9,
        AlternatePower = 10,           // Used in some quests
        DarkForce = 11,
        LightForce = 12,
        ShadowOrbs = 13,
        BurningEmbers = 14,
        DemonicFury = 15,
        Chii = 16,
        Max = 17,
        All = 127,          // default for class?
        Health = 0xFFFFFFFE,    // (-2 as signed value)
        MaxPerClass = 5
    }
    public enum Stats
    {
        Strength = 0,
        Agility = 1,
        Stamina = 2,
        Intellect = 3,
        Spirit = 4,
        Max = 5
    }

    public enum TrainerType
    {
        Class = 0,
        Mounts = 1,
        Tradeskills = 2,
        Pets = 3
    }
    public enum TrainerSpellState
    {
        Gray = 0,
        Green = 1,
        Red = 2,
        GreenDisabled = 10
    }

    public enum TimeConstants
    {
        Minute = 60,
        Hour = Minute * 60,
        Day = Hour * 24,
        Week = Day * 7,
        Month = Day * 30,
        Year = Month * 12,
        InMilliseconds = 1000
    }
    public enum ChatMsg : uint
    {
        Addon = 0xffffffff, // -1
        System = 0x00,
        Say = 0x01,
        Party = 0x02,
        Raid = 0x03,
        Guild = 0x04,
        Officer = 0x05,
        Yell = 0x06,
        Whisper = 0x07,
        Whisper_Foreign = 0x08,
        Whisper_Inform = 0x09,
        Emote = 0x0a,
        Text_Emote = 0x0b,
        Monster_Say = 0x0c,
        Monster_Party = 0x0d,
        Monster_Yell = 0x0e,
        Monster_Whisper = 0x0f,
        Monster_Emote = 0x10,
        Channel = 0x11,
        Channel_Join = 0x12,
        Channel_Leave = 0x13,
        Channel_List = 0x14,
        Channel_Notice = 0x15,
        Channel_Notice_User = 0x16,
        // Targeticons
        Afk = 0x17,
        Dnd = 0x18,
        Ignored = 0x19,
        Skill = 0x1a,
        Loot = 0x1b,
        Money = 0x1c,
        Opening = 0x1d,
        Tradeskills = 0x1e,
        Pet_Info = 0x1f,
        Combat_Misc_Info = 0x20,
        Combat_Xp_Gain = 0x21,
        Combat_Honor_Gain = 0x22,
        Combat_Faction_Change = 0x23,
        Bg_System_Neutral = 0x24,
        Bg_System_Alliance = 0x25,
        Bg_System_Horde = 0x26,
        Raid_Leader = 0x27,
        Raid_Warning = 0x28,
        Raid_Boss_Emote = 0x29,
        Raid_Boss_Whisper = 0x2a,
        Filtered = 0x2b,
        Battleground = 0x2c,
        Battleground_Leader = 0x2d,
        Restricted = 0x2e,
        Battlenet = 0x2f,
        Achievement = 0x30,
        Guild_Achievement = 0x31,
        Arena_Points = 0x32,
        Party_Leader = 0x33,
        Unk52 = 0x34,     // 4.0.1
        BNetWhisper = 0x35,     // 4.0.1
        BNetWhisperInform = 0x36,     // 4.0.1
        BNetConversation = 0x37,      // 4.0.1
        Max = 0x38,
    }

    public enum Difficulty
    {
        Regular = 0,

        DungeonNormal = 0,
        DungeonHeroic = 1,
        DungeonEpic = 2,

        Raid10manNormal = 0,
        Raid25manNormal = 1,
        Raid10manHeroic = 2,
        Raid25manHeroic = 3
    }

    public enum SpawnMask
    {
        SpawnmaskContinent = (1 << Difficulty.Regular), // Any Maps Without Spawn Modes

        SpawnmaskDungeonNormal = (1 << Difficulty.DungeonNormal),
        SpawnmaskDungeonHeroic = (1 << Difficulty.DungeonHeroic),
        SpawnmaskDungeonAll = (SpawnmaskDungeonNormal | SpawnmaskDungeonHeroic),

        SpawnmaskRaid10manNormal = (1 << Difficulty.Raid10manNormal),
        SpawnmaskRaid25manNormal = (1 << Difficulty.Raid25manNormal),
        SpawnmaskRaidNormalAll = (SpawnmaskRaid10manNormal | SpawnmaskRaid25manNormal),

        SpawnmaskRaid10manHeroic = (1 << Difficulty.Raid10manHeroic),
        SpawnmaskRaid25manHeroic = (1 << Difficulty.Raid25manHeroic),
        SpawnmaskRaidHeroicAll = (SpawnmaskRaid10manHeroic | SpawnmaskRaid25manHeroic),

        SpawnmaskRaidAll = (SpawnmaskRaidNormalAll | SpawnmaskRaidHeroicAll)
    }
}
