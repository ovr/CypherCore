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
    public struct SkillConst
    {
        public const int MaxPlayerSkills = 128;
    }

    public enum Skill
    {
        None = 0,

        Frost = 6,
        Fire = 8,
        Arms = 26,
        Combat = 38,
        Subtlety = 39,
        Swords = 43,
        Axes = 44,
        Bows = 45,
        Guns = 46,
        BeastMastery = 50,
        Survival = 51,
        Maces = 54,
        Swords2h = 55,
        Holy = 56,
        Shadow = 78,
        Defense = 95,
        LangCommon = 98,
        RacialDwarven = 101,
        LangOrcish = 109,
        LangDwarven = 111,
        LangDarnassian = 113,
        LangTaurahe = 115,
        DualWield = 118,
        RacialTauren = 124,
        OrcRacial = 125,
        RacialNightElf = 126,
        FirstAid = 129,
        FeralCombat = 134,
        Staves = 136,
        LangThalassian = 137,
        LangDraconic = 138,
        LangDemonTongue = 139,
        LangTitan = 140,
        LangOldTongue = 141,
        Survival2 = 142,
        RidingHorse = 148,
        RidingWolf = 149,
        RidingTiger = 150,
        RidingRam = 152,
        Swiming = 155,
        Maces2h = 160,
        Unarmed = 162,
        Marksmanship = 163,
        Blacksmithing = 164,
        Leatherworking = 165,
        Alchemy = 171,
        Axes2h = 172,
        Daggers = 173,
        Thrown = 176,
        Herbalism = 182,
        GenericDnd = 183,
        Retribution = 184,
        Cooking = 185,
        Mining = 186,
        PetImp = 188,
        PetFelhunter = 189,
        Tailoring = 197,
        Engineering = 202,
        PetSpider = 203,
        PetVoidwalker = 204,
        PetSuccubus = 205,
        PetInfernal = 206,
        PetDoomguard = 207,
        PetWolf = 208,
        PetCat = 209,
        PetBear = 210,
        PetBoar = 211,
        PetCrocilisk = 212,
        PetCarrionBird = 213,
        PetCrab = 214,
        PetGorilla = 215,
        PetRaptor = 217,
        PetTallstrider = 218,
        RacialUnded = 220,
        Crossbows = 226,
        Wands = 228,
        Polearms = 229,
        PetScorpid = 236,
        Arcane = 237,
        PetTurtle = 251,
        Assassination = 253,
        Fury = 256,
        Protection = 257,
        Protection2 = 267,
        PetTalents = 270,
        PlateMail = 293,
        LangGnomish = 313,
        LangTroll = 315,
        Enchanting = 333,
        Demonology = 354,
        Affliction = 355,
        Fishing = 356,
        Enhancement = 373,
        Restoration = 374,
        ElementalCombat = 375,
        Skinning = 393,
        Mail = 413,
        Leather = 414,
        Cloth = 415,
        Shield = 433,
        FistWeapons = 473,
        RidingRaptor = 533,
        RidingMechanostrider = 553,
        RidingUndeadHorse = 554,
        Restoration2 = 573,
        Balance = 574,
        Destruction = 593,
        Holy2 = 594,
        Discipline = 613,
        Lockpicking = 633,
        PetBat = 653,
        PetHyena = 654,
        PetBirdOfPrey = 655,
        PetWindSerpent = 656,
        LangGutterspeak = 673,
        RidingKodo = 713,
        RacialTroll = 733,
        RacialGnome = 753,
        RacialHuman = 754,
        Jewelcrafting = 755,
        RacialBloodelf = 756,
        PetEventRc = 758,   // Skillcategory = -1
        LangDraenei = 759,
        RacialDraenei = 760,
        PetFelguard = 761,
        Riding = 762,
        PetDragonhawk = 763,
        PetNetherRay = 764,
        PetSporebat = 765,
        PetWarpStalker = 766,
        PetRavager = 767,
        PetSerpent = 768,
        Internal = 769,
        DkBlood = 770,
        DkFrost = 771,
        DkUnholy = 772,
        Inscription = 773,
        PetMoth = 775,
        Runeforging = 776,
        Mounts = 777,
        Companions = 778,
        PetExoticChimaera = 780,
        PetExoticDevilsaur = 781,
        PetGhoul = 782,
        PetExoticSilithid = 783,
        PetExoticWorm = 784,
        PetWasp = 785,
        PetExoticRhino = 786,
        PetExoticCoreHound = 787,
        PetExoticSpiritBeast = 788,
        RacialWorgen = 789,
        RacialGoblin = 790,
        LangWorgen = 791,
        LangGoblin = 792,
        Archaeology = 794,
        GeneralHunter = 795,
        GeneralDeathKnight = 796,
        GeneralRogue = 797,
        GeneralDruid = 798,
        GeneralMage = 799,
        GeneralPaladin = 800,
        GeneralShaman = 801,
        GeneralWarlock = 802,
        GeneralWarrior = 803,
        GeneralPriest = 804,
        PetWaterElemental = 805,
        PetFox = 808,
        AllGlyphs = 810,
        PetDog = 811,
        PetMonkey = 815,
        PetShaleSpider = 817,
        PetBeetle = 818,
        AllGuildPerks = 821,
        PetHydra = 824,
    }

    public enum SkillState
    {
        Unchanged = 0,
        Changed = 1,
        New = 2,
        Deleted = 3
    }

    public enum SkillCategory
    {
        Unk = 0,
        Attributes = 5,
        Weapon = 6,
        Class = 7,
        Armor = 8,
        Secondary = 9,
        Languages = 10,
        Profession = 11,
        Generic = 12
    }

    public enum SkillType
    {
        Language,                                   // 300..300
        Level,                                      // 1..max skill for level
        Mono,                                       // 1..1, grey monolite bar
        Rank,                                       // 1..skill for known rank
        None                                        // 0..0 always
    }
}
