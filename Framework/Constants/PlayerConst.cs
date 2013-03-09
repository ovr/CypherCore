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
    public struct PlayerConst
    {
        public const int MaxPowersPerClass = 5;
        public const int MinTalentSpec = 0;
        public const int MaxTalentSpec = 1;
        public const int MinTalentSpecs = 1;
        public const int MaxTalentSpecs = 2;
        public const int MaxGlyphSlotIndex = 9;
        public const int ReqPrimaryTreeTalents = 31;
        public const int ExploredZonesSize = 156;
        public const long MaxMoneyAmount = long.MaxValue;
    }


    public enum PlayerFlags : uint
    {
        None = 0x00000000,
        GroupLeader = 0x00000001,
        AFK = 0x00000002,
        DND = 0x00000004,
        GM = 0x00000008,
        Ghost = 0x00000010,
        Resting = 0x00000020,
        Unk7 = 0x00000040,
        Unk8 = 0x00000080,
        ContestedPVP = 0x00000100,
        InPVP = 0x00000200,
        HideHelm = 0x00000400,
        HideCloak = 0x00000800,
        PartialPlayTime = 0x00001000,
        NoPlayTime = 0x00002000,
        IsOutOfBounds = 0x00004000,
        Developer = 0x00008000,
        EnableLowLevelRaid = 0x00010000,
        TaxiBenchmark = 0x00020000,
        PVPTimer = 0x00040000,
        Commentator = 0x00080000,
        Unk21 = 0x00100000,
        Unk22 = 0x00200000,
        CommentatorUber = 0x00400000,
        AllowOnlyAbility = 0x00800000,
        Unk25 = 0x01000000,
        XPDisabled = 0x02000000,
        Unk27 = 0x04000000,
        Unk28 = 0x08000000,
        GuildLevelEnabled = 0x10000000
    }
    public enum CharacterFlags : uint
    {
        None = 0x00000000,
        Unk1 = 0x00000001,
        Unk2 = 0x00000002,
        CharacterLockedForTransfer = 0x00000004,
        Unk4 = 0x00000008,
        Unk5 = 0x00000010,
        Unk6 = 0x00000020,
        Unk7 = 0x00000040,
        Unk8 = 0x00000080,
        Unk9 = 0x00000100,
        Unk10 = 0x00000200,
        HideHelm = 0x00000400,
        HideCloak = 0x00000800,
        Unk13 = 0x00001000,
        Ghost = 0x00002000,
        Rename = 0x00004000,
        Unk16 = 0x00008000,
        Unk17 = 0x00010000,
        Unk18 = 0x00020000,
        Unk19 = 0x00040000,
        Unk20 = 0x00080000,
        Unk21 = 0x00100000,
        Unk22 = 0x00200000,
        Unk23 = 0x00400000,
        Unk24 = 0x00800000,
        LockedByBilling = 0x01000000,
        Declined = 0x02000000,
        Unk27 = 0x04000000,
        Unk28 = 0x08000000,
        Unk29 = 0x10000000,
        Unk30 = 0x20000000,
        Unk31 = 0x40000000,
        Unk32 = 0x80000000
    }
    public enum PlayerRestState
    {
        Rested = 0x01,
        NotRAFLinked = 0x02,
        RAFLinked = 0x06
    }
    public enum CharacterCustomizeFlags
    {
        None = 0x00000000,
        Customize = 0x00000001,       // Name, Gender, Etc...
        Faction = 0x00010000,       // Name, Gender, Faction, Etc...
        Race = 0x00100000        // Name, Gender, Race, Etc...
    }
    public enum AtLoginFlags
    {
        None = 0x00,
        Rename = 0x01,
        ResetSpells = 0x02,
        ResetTalents = 0x04,
        Customize = 0x08,
        ResetPetTalents = 0x10,
        LoginFirst = 0x20,
        ChangeFaction = 0x40,
        ChangeRace = 0x80
    }
    public enum TradeSlots
    {
        Count = 7,
        TradedCount = 6,
        Nontraded = 6,
        Invalid = -1
    }
    public enum PlayerSlots
    {
        // first slot for item stored (in any way in player items data)
        Start = 0,
        // last+1 slot for item stored (in any way in player items data)
        End = 86,
        Count = (End - Start)
    }
    public enum PlayerTitle : ulong
    {
        Disabled = 0x0000000000000000,
        None = 0x0000000000000001,
        Private = 0x0000000000000002, // 1
        Corporal = 0x0000000000000004, // 2
        SergeantA = 0x0000000000000008, // 3
        MasterSergeant = 0x0000000000000010, // 4
        SergeantMajor = 0x0000000000000020, // 5
        Knight = 0x0000000000000040, // 6
        KnightLieutenant = 0x0000000000000080, // 7
        KnightCaptain = 0x0000000000000100, // 8
        KnightChampion = 0x0000000000000200, // 9
        LieutenantCommander = 0x0000000000000400, // 10
        Commander = 0x0000000000000800, // 11
        Marshal = 0x0000000000001000, // 12
        FieldMarshal = 0x0000000000002000, // 13
        GrandMarshal = 0x0000000000004000, // 14
        Scout = 0x0000000000008000, // 15
        Grunt = 0x0000000000010000, // 16
        SergeantH = 0x0000000000020000, // 17
        SeniorSergeant = 0x0000000000040000, // 18
        FirstSergeant = 0x0000000000080000, // 19
        StoneGuard = 0x0000000000100000, // 20
        BloodGuard = 0x0000000000200000, // 21
        Legionnaire = 0x0000000000400000, // 22
        Centurion = 0x0000000000800000, // 23
        Champion = 0x0000000001000000, // 24
        LieutenantGeneral = 0x0000000002000000, // 25
        General = 0x0000000004000000, // 26
        Warlord = 0x0000000008000000, // 27
        HighWarlord = 0x0000000010000000, // 28
        Gladiator = 0x0000000020000000, // 29
        Duelist = 0x0000000040000000, // 30
        Rival = 0x0000000080000000, // 31
        Challenger = 0x0000000100000000, // 32
        ScarabLord = 0x0000000200000000, // 33
        Conqueror = 0x0000000400000000, // 34
        Justicar = 0x0000000800000000, // 35
        ChampionOfTheNaaru = 0x0000001000000000, // 36
        MercilessGladiator = 0x0000002000000000, // 37
        OfTheShatteredSun = 0x0000004000000000, // 38
        HandOfAdal = 0x0000008000000000, // 39
        VengefulGladiator = 0x0000010000000000, // 40

        KnowTitlesSize = 3,
        Max = KnowTitlesSize * 64
    }
    public enum PlayerExtraFlags
    {
        // gm abilities
        GM_On = 0x0001,
        AcceptWhispers = 0x0004,
        TaxiCheat = 0x0008,
        GMInvisible = 0x0010,
        GMChat = 0x0020,               // Show GM badge in chat messages

        // other states
        PVPDeath = 0x0100                // store PvP death status until corpse creating.
    }
}
