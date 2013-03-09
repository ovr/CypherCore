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
    public enum SpellGroup
    {
        None = 0,
        ElixirBattle = 1,
        ElixirGuardian = 2,
        ElixirUnstable = 3,
        ElixirShattrath = 4,
        CoreRangeMax = 5
    }
    public enum SpellGroupStackRule
    {
        Default = 0,
        Exclusive = 1,
        ExclusiveFromSameCaster = 2,
        ExclusiveSameEffect = 3
    }

    public enum PlayerSpellState
    {
        Unchanged = 0,
        Changed = 1,
        New = 2,
        Removed = 3,
        Temporary = 4
    }

    public enum SpellState
    {
        None = 0,
        Preparing = 1,
        Casting = 2,
        Finished = 3,
        Idle = 4,
        Delayed = 5
    }

    public enum SpellSchools
    {
        Normal = 0,
        Holy = 1,
        Fire = 2,
        Nature = 3,
        Frost = 4,
        Shadow = 5,
        Arcane = 6,
        Max = 7
    }

    //16309
    public enum SpellCastResult
    {
        Success = 0,
        AffectingCombat = 1,
        AlreadyAtFullHealth = 2,
        AlreadyAtFullMana = 3,
        AlreadyAtFullPower = 4,
        AlreadyBeingTamed = 5,
        AlreadyHaveCharm = 6,
        AlreadyHaveSummon = 7,
        AlreadyHavePet = 8,
        AlreadyOpen = 9,
        AuraBounced = 10,
        AutotrackInterrupted = 11,
        BadImplicitTargets = 12,
        BadTargets = 13,
        CantBeCharmed = 14,
        CantBeDisenchanted = 15,
        CantBeDisenchantedSkill = 16,
        CantBeMilled = 17,
        CantBeProspected = 18,
        CantCastOnTapped = 19,
        CantDuelWhileInvisible = 20,
        CantDuelWhileStealthed = 21,
        CantStealth = 22,
        CasterAurastate = 23,
        CasterDead = 24,
        Charmed = 25,
        ChestInUse = 26,
        Confused = 27,
        DontReport = 28,
        EquippedItem = 29,
        EquippedItemClass = 30,
        EquippedItemClassMainhand = 31,
        EquippedItemClassOffhand = 32,
        Error = 33,
        Falling = 34,
        Fizzle = 35,
        Fleeing = 36,
        FoodLowlevel = 37,
        Highlevel = 38,
        HungerSatiated = 39,
        Immune = 40,
        IncorrectArea = 41,
        Interrupted = 42,
        InterruptedCombat = 43,
        ItemAlreadyEnchanted = 44,
        ItemGone = 45,
        ItemNotFound = 46,
        ItemNotReady = 47,
        LevelRequirement = 48,
        LineOfSight = 49,
        Lowlevel = 50,
        LowCastlevel = 51,
        MainhandEmpty = 52,
        Moving = 53,
        NeedAmmo = 54,
        NeedAmmoPouch = 55,
        NeedExoticAmmo = 56,
        NeedMoreItems = 57,
        Nopath = 58,
        NotBehind = 59,
        NotFishable = 60,
        NotFlying = 61,
        NotHere = 62,
        NotInfront = 63,
        NotInControl = 64,
        NotKnown = 65,
        NotMounted = 66,
        NotOnTaxi = 67,
        NotOnTransport = 68,
        NotReady = 69,
        NotShapeshift = 70,
        NotStanding = 71,
        NotTradeable = 72,
        NotTrading = 73,
        NotUnsheathed = 74,
        NotWhileGhost = 75,
        NotWhileLooting = 76,
        NoAmmo = 77,
        NoChargesRemain = 78,
        NoChampion = 79,
        NoComboPoints = 80,
        NoDueling = 81,
        NoEndurance = 82,
        NoFish = 83,
        NoItemsWhileShapeshifted = 84,
        NoMountsAllowed = 85,
        NoPet = 86,
        NoPower = 87,
        NothingToDispel = 88,
        NothingToSteal = 89,
        OnlyAbovewater = 90,
        OnlyDaytime = 91,
        OnlyIndoors = 92,
        OnlyMounted = 93,
        OnlyNighttime = 94,
        OnlyOutdoors = 95,
        OnlyShapeshift = 96,
        OnlyStealthed = 97,
        OnlyUnderwater = 98,
        OutOfRange = 99,
        Pacified = 100,
        Possessed = 101,
        Reagents = 102,
        RequiresArea = 103,
        RequiresSpellFocus = 104,
        Rooted = 105,
        Silenced = 106,
        SpellInProgress = 107,
        SpellLearned = 108,
        SpellUnavailable = 109,
        Stunned = 110,
        TargetsDead = 111,
        TargetAffectingCombat = 112,
        TargetAurastate = 113,
        TargetDueling = 114,
        TargetEnemy = 115,
        TargetEnraged = 116,
        TargetFriendly = 117,
        TargetInCombat = 118,
        TargetInPetBattle = 119,
        TargetIsPlayer = 120,
        TargetIsPlayerControlled = 121,
        TargetNotDead = 122,
        TargetNotInParty = 123,
        TargetNotLooted = 124,
        TargetNotPlayer = 125,
        TargetNoPockets = 126,
        TargetNoWeapons = 127,
        TargetNoRangedWeapons = 128,
        TargetUnskinnable = 129,
        ThirstSatiated = 130,
        TooClose = 131,
        TooManyOfItem = 132,
        TotemCategory = 133,
        Totems = 134,
        TryAgain = 135,
        UnitNotBehind = 136,
        UnitNotInfront = 137,
        VisionObscured = 138,
        WrongPetFood = 139,
        NotWhileFatigued = 140,
        TargetNotInInstance = 141,
        NotWhileTrading = 142,
        TargetNotInRaid = 143,
        TargetFreeforall = 144,
        NoEdibleCorpses = 145,
        OnlyBattlegrounds = 146,
        TargetNotGhost = 147,
        TransformUnusable = 148,
        WrongWeather = 149,
        DamageImmune = 150,
        PreventedByMechanic = 151,
        PlayTime = 152,
        Reputation = 153,
        MinSkill = 154,
        NotInRatedBattleground = 155,
        NotOnShapeshift = 156,
        NotOnStealthed = 157,
        NotOnDamageImmune = 158,
        NotOnMounted = 159,
        TooShallow = 160,
        TargetNotInSanctuary = 161,
        TargetIsTrivial = 162,
        BmOrInvisgod = 163,
        ApprenticeRidingRequirement = 164,
        JourneymanRidingRequirement = 165,
        ExpertRidingRequirement = 166,
        ArtisanRidingRequirement = 167,
        MasterRidingRequirement = 168,
        ColdRidingRequirement = 169,
        FlightMasterRidingRequirement = 170,
        CSRidingRequirement = 171,
        PanadaRidingRequirement = 172,
        NotIdle = 173,
        NotInactive = 174,
        PartialPlaytime = 175,
        NoPlaytime = 176,
        NotInBattleground = 177,
        NotInRaidInstance = 178,
        OnlyInArena = 179,
        TargetLockedToRaidInstance = 180,
        OnUseEnchant = 181,
        NotOnGround = 182,
        CustomError = 183,
        CantDoThatRightNow = 184,
        TooManySockets = 185,
        InvalidGlyph = 186,
        UniqueGlyph = 187,
        GlyphSocketLocked = 188,
        NoValidTargets = 189,
        ItemAtMaxCharges = 190,
        NotInBarbershop = 191,
        FishingTooLow = 192,
        ItemEnchantTradeWindow = 193,
        SummonPending = 194,
        MaxSockets = 195,
        PetCanRename = 196,
        TargetCannotBeResurrected = 197,
        NoActions = 198,
        CurrencyWeightMismatch = 199,
        WeightNotEnough = 200,
        WeightTooMuch = 201,
        NoVacantSeat = 202,
        NoLiquid = 203,
        OnlyNotSwimming = 204,
        ByNotMoving = 205,
        InCombatResLimitReached = 206,
        NotInArena = 207,
        TargetNotGrounded = 208,
        ExceededWeeklyUsage = 209,
        NotInLfgDungeon = 210,
        BadTargetFilter = 211,
        NotEnoughTargets = 212,
        NoSpec = 213,
        CantAddBattlePet = 214,
        CantUpgradeBattlePet = 215,
        WrongBattlePetType = 216,
        Unknown = 254, // Custom Value, Default Case
        SpellCastOk = 255, // Custom Value, Must Not Be Sent To Client
    }

    public enum SpellCustomErrors
    {
        None = 0,
        CustomMsg = 1, // Something Bad Happened, And We Want To Display A Custom Message!
        AlexBrokeQuest = 2, // Alex Broke Your Quest! Thank Him Later!
        NeedHelplessVillager = 3, // This Spell May Only Be Used On Helpless Wintergarde Villagers That Have Not Been Rescued.
        NeedWarsongDisguise = 4, // Requires That You Be Wearing The Warsong Orc Disguise.
        RequiresPlagueWagon = 5, // You Must Be Closer To A Plague Wagon In Order To Drop Off Your 7th Legion Siege Engineer.
        CantTargetFriendlyNonparty = 6, // You Cannot Target Friendly Units Outside Your Party.
        NeedChillNymph = 7, // You Must Target A Weakened Chill Nymph.
        MustBeInEnkilah = 8, // The Imbued Scourge Shroud Will Only Work When Equipped In The Temple City Of En'Kilah.
        RequiresCorpseDust = 9, // Requires Corpse Dust
        CantSummonGargoyle = 10, // You Cannot Summon Another Gargoyle Yet.
        NeedCorpseDustIfNoTarget = 11, // Requires Corpse Dust If The Target Is Not Dead And Humanoid.
        MustBeAtShatterhorn = 12, // Can Only Be Placed Near Shatterhorn
        MustTargetProtoDrakeEgg = 13, // You Must First Select A Proto-Drake Egg.
        MustBeCloseToTree = 14, // You Must Be Close To A Marked Tree.
        MustTargetTurkey = 15, // You Must Target A Fjord Turkey.
        MustTargetHawk = 16, // You Must Target A Fjord Hawk.
        TooFarFromBouy = 17, // You Are Too Far From The Bouy.
        MustBeCloseToOilSlick = 18, // Must Be Used Near An Oil Slick.
        MustBeCloseToBouy = 19, // You Must Be Closer To The Buoy!
        WyrmrestVanquisher = 20, // You May Only Call For The Aid Of A Wyrmrest Vanquisher In Wyrmrest Temple, The Dragon Wastes, Galakrond'S Rest Or The Wicked Coil.
        MustTargetIceHeartJormungar = 21, // That Can Only Be Used On A Ice Heart Jormungar Spawn.
        MustBeCloseToSinkhole = 22, // You Must Be Closer To A Sinkhole To Use Your Map.
        RequiresHaroldLane = 23, // You May Only Call Down A Stampede On Harold Lane.
        RequiresGammothMagnataur = 24, // You May Only Use The Pouch Of Crushed Bloodspore On Gammothra Or Other Magnataur In The Bloodspore Plains And Gammoth.
        MustBeInResurrectionChamber = 25, // Requires The Magmawyrm Resurrection Chamber In The Back Of The Maw Of Neltharion.
        CantCallWintergardeHere = 26, // You May Only Call Down A Wintergarde Gryphon In Wintergarde Keep Or The Carrion Fields.
        MustTargetWilhelm = 27, // What Are You Doing? Only Aim That Thing At Wilhelm!
        NotEnoughHealth = 28, // Not Enough Health!
        NoNearbyCorpses = 29, // There Are No Nearby Corpses To Use
        TooManyGhouls = 30, // You'Ve Created Enough Ghouls. Return To Gothik The Harvester At Death'S Breach.
        GoFurtherFromSunderedShard = 31, // Your Companion Does Not Want To Come Here.  Go Further From The Sundered Shard.
        MustBeInCatForm = 32, // Must Be In Cat Form
        MustBeDeathKnight = 33, // Only Death Knights May Enter Ebon Hold.
        MustBeInFeralForm = 34, // Must Be In Cat Form, Bear Form, Or Dire Bear Form
        MustBeNearHelplessVillager = 35, // You Must Be Within Range Of A Helpless Wintergarde Villager.
        CantTargetElementalMechanical = 36, // You Cannot Target An Elemental Or Mechanical Corpse.
        MustHaveUsedDalaranCrystal = 37, // This Teleport Crystal Cannot Be Used Until The Teleport Crystal In Dalaran Has Been Used At Least Once.
        YouAlreadyHoldSomething = 38, // You Are Already Holding Something In Your Hand. You Must Throw The Creature In Your Hand Before Picking Up Another.
        YouDontHoldAnything = 39, // You Don'T Have Anything To Throw! Find A Vargul And Use Gymer Grab To Pick One Up!
        MustBeCloseToValduran = 40, // Bouldercrag'S War Horn Can Only Be Used Within 10 Yards Of Valduran The Stormborn.
        NoPassenger = 41, // You Are Not Carrying A Passenger. There Is Nobody To Drop Off.
        CantBuildMoreVehicles = 42, // You Cannot Build Any More Siege Vehicles.
        AlreadyCarryingCrusader = 43, // You Are Already Carrying A Captured Argent Crusader. You Must Return To The Argent Vanguard Infirmary And Drop Off Your Passenger Before You May Pick Up Another.
        CantDoWhileRooted = 44, // You Can'T Do That While Rooted.
        RequiresNearbyTarget = 45, // Requires A Nearby Target.
        NothingToDiscover = 46, // Nothing Left To Discover.
        NotEnoughTargets = 47, // No Targets Close Enough To Bluff.
        ConstructTooFar = 48, // Your Iron Rune Construct Is Out Of Range.
        RequiresGrandMasterEngineer = 49, // Requires Grand Master Engineer
        CantUseThatMount = 50, // You Can'T Use That Mount.
        NooneToEject = 51, // There Is Nobody To Eject!
        TargetMustBeBound = 52, // The Target Must Be Bound To You.
        TargetMustBeUndead = 53, // Target Must Be Undead.
        TargetTooFar = 54, // You Have No Target Or Your Target Is Too Far Away.
        MissingDarkMatter = 55, // Missing Reagents: Dark Matter
        CantUseThatItem = 56, // You Can'T Use That Item
        CantDoWhileCycyloned = 57, // You Can'T Do That While Cycloned
        TargetHasScroll = 58, // Target Is Already Affected By A Scroll
        PoisonTooStrong = 59, // That Anti-Venom Is Not Strong Enough To Dispel That Poison
        MustHaveLanceEquipped = 60, // You Must Have A Lance Equipped.
        MustBeCloseToMaiden = 61, // You Must Be Near The Maiden Of Winter'S Breath Lake.
        LearnedEverything = 62, // You Have Learned Everything From That Book
        PetIsDead = 63, // Your Pet Is Dead
        NoValidTargets = 64, // There Are No Valid Targets Within Range.
        GmOnly = 65, // Only Gms May Use That. Your Account Has Been Reported For Investigation.
        RequiresLevel58 = 66, // You Must Reach Level 58 To Use This Portal.
        AtHonorCap = 67, // You Already Have The Maximum Amount Of Honor.
        HaveHotRod = 68, // You Already Have A Hot Rod.
        PartygoerMoreBubbly = 69, // This Partygoer Wants Some More Bubbly
        PartygoerNeedBucket = 70, // This Partygoer Needs A Bucket!
        PartygoerWantToDance = 71, // This Partygoer Wants To Dance With You.
        PartygoerWantFireworks = 72, // This Partygoer Wants To See Some Fireworks.
        PartygoerWantAppetizer = 73, // This Partygoer Wants Some More Hors D'Oeuvres.
        GoblinBatteryDepleted = 74, // The Goblin All-In-1-Der Belt'S Battery Is Depleted.
        MustHaveDemonicCircle = 75, // You Must Have A Demonic Circle Active.
        AtMaxRage = 76, // You Already Have Maximum Rage
        Requires350Engineering = 77, // Requires Engineering (350)
        SoulBelongsToLichKing = 78, // Your Soul Belongs To The Lich King
        AttendantHasPony = 79, // Your Attendant Already Has An Argent Pony
        GoblinStartingMission = 80, // First, Overload The Defective Generator, Activate The Leaky Stove, And Drop A Cigar On The Flammable Bed.
        GasbotAlreadySent = 81, // You'Ve Already Sent In The Gasbot And Destroyed Headquarters!
        GoblinIsPartiedOut = 82, // This Goblin Is All Partied Out!
        MustHaveFireTotem = 83, // You Must Have A Fire Totem Active.
        CantTargetVampires = 84, // You May Not Bite Other Vampires.
        PetAlreadyAtYourLevel = 85, // Your Pet Is Already At Your Level.
        MissingItemRequiremens = 86, // You Do Not Meet The Level Requirements For This Item.
        TooManyAbominations = 87, // There Are Too Many Mutated Abominations.
        AllPotionsUsed = 88, // The Potions Have All Been Depleted By Professor Putricide.
        DefeatedEnoughAlready = 89, // You Have Already Defeated Enough Of Them.
        RequiresLevel65 = 90, // Requires Level 65
        DestroyedKtcOilPlatform = 91, // You Have Already Destroyed The Ktc Oil Platform.
        LaunchedEnoughCages = 92, // You Have Already Launched Enough Cages.
        RequiresBoosterRockets = 93, // Requires Single-Stage Booster Rockets. Return To Hobart Grapplehammer To Get More.
        EnoughWildCluckers = 94, // You Have Already Captured Enough Wild Cluckers.
        RequiresControlFireworks = 95, // Requires Remote Control Fireworks. Return To Hobart Grapplehammer To Get More.
        MaxNumberOfRecruits = 96, // You Already Have The Max Number Of Recruits.
        MaxNumberOfVolunteers = 97, // You Already Have The Max Number Of Volunteers.
        FrostmourneRenderedRessurect = 98, // Frostmourne Has Rendered You Unable To Ressurect.
        CantMountWithShapeshift = 99, // You Can'T Mount While Affected By That Shapeshift.
        FawnsAlreadyFollowing = 100, // Three Fawns Are Already Following You!
        AlreadyHaveRiverBoat = 101, // You Already Have A River Boat.
        NoActiveEnchantment = 102, // You Have No Active Enchantment To Unleash.
        EnoughHighbourneSouls = 103, // You Have Bound Enough Highborne Souls. Return To Arcanist Valdurian.
        Atleast40ydFromOilDrilling = 104, // You Must Be At Least 40 Yards Away From All Other Oil Drilling Rigs.
        AboveEnslavedPearlMiner = 106, // You Must Be Above The Enslaved Pearl Miner.
        MustTargetCorpseSpecial1 = 107, // You Must Target The Corpse Of A Seabrush Terrapin, Scourgut Remora, Or Spinescale Hammerhead.
        SlaghammerAlreadyPrisoner = 108, // Ambassador Slaghammer Is Already Your Prisoner.
        RequireAttunedLocation1 = 109, // Requires A Location That Is Attuned With The Naz'Jar Battlemaiden.
        NeedToFreeDrakeFirst = 110, // Free The Drake From The Net First!
        DragonmawAlliesAlreadyFollow = 111, // You Already Have Three Dragonmaw Allies Following You.
        RequireOpposableThumbs = 112, // Requires Opposable Thumbs.
        NotEnoughHealth2 = 113, // Not Enough Health
        EnoughForsakenTroopers = 114, // You Already Have Enough Forsaken Troopers.
        CannotJumpToBoulder = 115, // You Cannot Jump To Another Boulder Yet.
        SkillTooHigh = 116, // Skill Too High.
        Already6SurvivorsRescued = 117, // You Have Already Rescued 6 Survivors.
        MustFaceShipsFromBalloon = 118, // You Need To Be Facing The Ships From The Rescue Balloon.
        CannotSuperviseMoreCultists = 119, // You Cannot Supervise More Than 5 Arrested Cultists At A Time.
        RequiresLevel85 = 120, // You Must Reach Level 85 To Use This Portal.
        MustBeBelow35Health = 121, // Your Target Must Be Below 35% Health.
        MustSelectTalentSpecial = 122, // You Must Select A Talent Specialization First.
        TooWiseAndPowerful = 123, // You Are Too Wise And Powerful To Gain Any Benefit From That Item.
        TooCloseArgentLightwell = 124, // You Are Within 10 Yards Of Another Argent Lightwell.
        NotWhileShapeshifted = 125, // You Can'T Do That While Shapeshifted.
        ManaGemInBank = 126, // You Already Have A Mana Gem In Your Bank.
        FlameShockNotActive = 127, // You Must Have At Least One Flame Shock Active.
        CantTransform = 128, // You Cannot Transform Right Now
        PetMustBeAttacking = 129, // Your Pet Must Be Attacking A Target.
        GnomishEngineering = 130, // Requires Gnomish Engineering
        GoblinEngineering = 131, // Requires Goblin Engineering
        NoTarget = 132, // You Have No Target.
        PetOutOfRange = 133, // Your Pet Is Out Of Range Of The Target.
        HoldingFlag = 134, // You Can'T Do That While Holding The Flag.
        TargetHoldingFlag = 135, // You Can'T Do That To Targets Holding The Flag.
        PortalNotOpen = 136, // The Portal Is Not Yet Open. Continue Helping The Druids At The Sanctuary Of Malorne.
        AggraAirTotem = 137, // You Need To Be Closer To Aggra'S Air Totem, In The West.
        AggraWaterTotem = 138, // You Need To Be Closer To Aggra'S Water Totem, In The North.
        AggraEarthTotem = 139, // You Need To Be Closer To Aggra'S Earth Totem, In The East.
        AggraFireTotem = 140, // You Need To Be Closer To Aggra'S Fire Totem, Near Thrall.
        TargetHasStartdust2 = 148, // Target Is Already Affected By Stardust No. 2.
        ElementiumGemClusters = 149  // You Cannot Deconstruct Elementium Gem Clusters While Collecting Them!
    }

    public enum SpellMissInfo
    {
        None = 0,
        Miss = 1,
        Resist = 2,
        Dodge = 3,
        Parry = 4,
        Block = 5,
        Evade = 6,
        Immune = 7,
        Immune2 = 8, // One Of These 2 Is Miss_Tempimmune
        Deflect = 9,
        Absorb = 10,
        Reflect = 11
    }

    public enum SpellHitType
    {
        Unk1 = 0x00001,
        Crit = 0x00002,
        Unk3 = 0x00004,
        Unk4 = 0x00008,
        Unk5 = 0x00010,                          // Replace Caster?
        Unk6 = 0x00020
    }

    public enum SpellDmgClass
    {
        None = 0,
        Magic = 1,
        Melee = 2,
        Ranged = 3
    }

    public enum SpellPreventionType
    {
        None = 0,
        Silence = 1,
        Pacify = 2
    }

    public enum SpellCastTargetFlags
    {
        None = 0x00000000,
        Unused1 = 0x00000001,               // Not Used
        Unit = 0x00000002,               // Pguid
        UnitRaid = 0x00000004,               // Not Sent, Used To Validate Target (If Raid Member)
        UnitParty = 0x00000008,               // Not Sent, Used To Validate Target (If Party Member)
        Item = 0x00000010,               // Pguid
        SourceLocation = 0x00000020,               // Pguid, 3 Float
        DestLocation = 0x00000040,               // Pguid, 3 Float
        UnitEnemy = 0x00000080,               // Not Sent, Used To Validate Target (If Enemy)
        UnitAlly = 0x00000100,               // Not Sent, Used To Validate Target (If Ally)
        CorpseEnemy = 0x00000200,               // Pguid
        UnitDead = 0x00000400,               // Not Sent, Used To Validate Target (If Dead Creature)
        Gameobject = 0x00000800,               // Pguid, Used With TargetGameobjectTarget
        TradeItem = 0x00001000,               // Pguid
        String = 0x00002000,               // String
        GameobjectItem = 0x00004000,               // Not Sent, Used With TargetGameobjectItemTarget
        CorpseAlly = 0x00008000,               // Pguid
        UnitMinipet = 0x00010000,               // Pguid, Used To Validate Target (If Non Combat Pet)
        GlyphSlot = 0x00020000,               // Used In Glyph Spells
        DestTarget = 0x00040000,               // Sometimes Appears With DestTarget Spells (May Appear Or Not For A Given Spell)
        ExtraTargets = 0x00080000,               // Uint32 Counter, Loop { Vec3 - Screen Position (?), Guid }, Not Used So Far
        UnitPassenger = 0x00100000,               // Guessed, Used To Validate Target (If Vehicle Passenger)

        UnitMask = Unit | UnitRaid | UnitParty
            | UnitEnemy | UnitAlly | UnitDead | UnitMinipet | UnitPassenger,
        GameobjectMask = Gameobject | GameobjectItem,
        CorpseMask = CorpseAlly | CorpseEnemy,
        ItemMask = TradeItem | Item | GameobjectItem
    }

    public enum SpellFamilyNames
    {
        Generic = 0,
        Unk1 = 1,                            // Events, Holidays
        // 2 - Unused
        Mage = 3,
        Warrior = 4,
        Warlock = 5,
        Priest = 6,
        Druid = 7,
        Rogue = 8,
        Hunter = 9,
        Paladin = 10,
        Shaman = 11,
        Unk2 = 12,                           // 2 Spells (Silence Resistance)
        Potion = 13,
        // 14 - Unused
        Deathknight = 15,
        // 16 - Unused
        Pet = 17,
        Unk3 = 50,
    }

    public enum TriggerCastFlags : uint
    {
        None = 0x00000000,   //! Not Triggered
        IgnoreGcd = 0x00000001,   //! Will Ignore Gcd
        IgnoreSpellAndCategoryCd = 0x00000002,   //! Will Ignore Spell And Category Cooldowns
        IgnorePowerAndReagentCost = 0x00000004,   //! Will Ignore Power And Reagent Cost
        IgnoreCastItem = 0x00000008,   //! Will Not Take Away Cast Item Or Update Related Achievement Criteria
        IgnoreAuraScaling = 0x00000010,   //! Will Ignore Aura Scaling
        IgnoreCastInProgress = 0x00000020,   //! Will Not Check If A Current Cast Is In Progress
        IgnoreComboPoints = 0x00000040,   //! Will Ignore Combo Point Requirement
        CastDirectly = 0x00000080,   //! In Spell::Prepare, Will Be Cast Directly Without Setting Containers For Executed Spell
        IgnoreAuraInterruptFlags = 0x00000100,   //! Will Ignore Interruptible Aura'S At Cast
        IgnoreSetFacing = 0x00000200,   //! Will Not Adjust Facing To Target (If Any)
        IgnoreShapeshift = 0x00000400,   //! Will Ignore Shapeshift Checks
        IgnoreCasterAurastate = 0x00000800,   //! Will Ignore Caster Aura States Including Combat Requirements And Death State
        IgnoreCasterMountedOrOnVehicle = 0x00002000,   //! Will Ignore Mounted/On Vehicle Restrictions
        IgnoreCasterAuras = 0x00010000,   //! Will Ignore Caster Aura Restrictions Or Requirements
        DisallowProcEvents = 0x00020000,   //! Disallows Proc Events From Triggered Spell (Default)
        DontReportCastError = 0x00040000,   //! Will Return SpellFailedDontReport In Checkcast Functions
        FullMask = 0xffffffff
    }

    public enum SpellSchoolMask
    {
        SpellSchoolMaskNone = 0x00,                       // Not Exist
        SpellSchoolMaskNormal = (1 << SpellSchools.Normal), // Physical (Armor)
        SpellSchoolMaskHoly = (1 << SpellSchools.Holy),
        SpellSchoolMaskFire = (1 << SpellSchools.Fire),
        SpellSchoolMaskNature = (1 << SpellSchools.Nature),
        SpellSchoolMaskFrost = (1 << SpellSchools.Frost),
        SpellSchoolMaskShadow = (1 << SpellSchools.Shadow),
        SpellSchoolMaskArcane = (1 << SpellSchools.Arcane),

        // 124, Not Include Normal And Holy Damage
        SpellSchoolMaskSpell = (SpellSchoolMaskFire |
                                      SpellSchoolMaskNature | SpellSchoolMaskFrost |
                                      SpellSchoolMaskShadow | SpellSchoolMaskArcane),
        // 126
        SpellSchoolMaskMagic = (SpellSchoolMaskHoly | SpellSchoolMaskSpell),

        // 127
        SpellSchoolMaskAll = (SpellSchoolMaskNormal | SpellSchoolMaskMagic)
    }

    public enum SpellCastFlags : uint
    {
        None = 0x00000000,
        Pending = 0x00000001,              // Aoe Combat Log?
        HasTrajectory = 0x00000002,
        Unk3 = 0x00000004,
        Unk4 = 0x00000008,              // Ignore Aoe Visual
        Unk5 = 0x00000010,
        Projectile = 0x00000020,
        Unk7 = 0x00000040,
        Unk8 = 0x00000080,
        Unk9 = 0x00000100,
        Unk10 = 0x00000200,
        Unk11 = 0x00000400,
        PowerLeftSelf = 0x00000800,
        Unk13 = 0x00001000,
        Unk14 = 0x00002000,
        Unk15 = 0x00004000,
        Unk16 = 0x00008000,
        Unk17 = 0x00010000,
        AdjustMissile = 0x00020000,
        Unk19 = 0x00040000,
        VisualChain = 0x00080000,
        Unk21 = 0x00100000,
        RuneList = 0x00200000,
        Unk23 = 0x00400000,
        Unk24 = 0x00800000,
        Unk25 = 0x01000000,
        Unk26 = 0x02000000,
        Immunity = 0x04000000,
        Unk28 = 0x08000000,
        Unk29 = 0x10000000,
        Unk30 = 0x20000000,
        HealPrediction = 0x40000000,
        Unk32 = 0x80000000
    }

    #region Spell Attributes
    public enum SpellAttr0 : uint
    {
        Unk0 = 0x00000001, //  0
        ReqAmmo = 0x00000002, //  1 On Next Ranged
        OnNextSwing = 0x00000004, //  2
        IsReplenishment = 0x00000008, //  3 Not Set In 3.0.3
        Ability = 0x00000010, //  4 Client Puts 'Ability' Instead Of 'Spell' In Game Strings For These Spells
        Tradespell = 0x00000020, //  5 Trade Spells (Recipes), Will Be Added By Client To A Sublist Of Profession Spell
        Passive = 0x00000040, //  6 Passive Spell
        HiddenClientside = 0x00000080, //  7 Spells With This Attribute Are Not Visible In Spellbook Or Aura Bar
        HideInCombatLog = 0x00000100, //  8 This Attribite Controls Whether Spell Appears In Combat Logs
        TargetMainhandItem = 0x00000200, //  9 Client Automatically Selects Item From Mainhand Slot As A Cast Target
        OnNextSwing2 = 0x00000400, // 10
        Unk11 = 0x00000800, // 11
        DaytimeOnly = 0x00001000, // 12 Only Useable At Daytime, Not Set In 2.4.2
        NightOnly = 0x00002000, // 13 Only Useable At Night, Not Set In 2.4.2
        IndoorsOnly = 0x00004000, // 14 Only Useable Indoors, Not Set In 2.4.2
        OutdoorsOnly = 0x00008000, // 15 Only Useable Outdoors.
        NotShapeshift = 0x00010000, // 16 Not While Shapeshifted
        OnlyStealthed = 0x00020000, // 17 Must Be In Stealth
        DontAffectSheathState = 0x00040000, // 18 Client Won'T Hide Unit Weapons In Sheath On Cast/Channel
        LevelDamageCalculation = 0x00080000, // 19 Spelldamage Depends On Caster Level
        StopAttackTarget = 0x00100000, // 20 Stop Attack After Use This Spell (And Not Begin Attack If Use)
        ImpossibleDodgeParryBlock = 0x00200000, // 21 Cannot Be Dodged/Parried/Blocked
        CastTrackTarget = 0x00400000, // 22 Client Automatically Forces Player To Face Target When Casting
        CastableWhileDead = 0x00800000, // 23 Castable While Dead?
        CastableWhileMounted = 0x01000000, // 24 Castable While Mounted
        DisabledWhileActive = 0x02000000, // 25 Activate And Start Cooldown After Aura Fade Or Remove Summoned Creature Or Go
        Negative1 = 0x04000000, // 26 Many Negative Spells Have This Attr
        CastableWhileSitting = 0x08000000, // 27 Castable While Sitting
        CantUsedInCombat = 0x10000000, // 28 Cannot Be Used In Combat
        UnaffectedByInvulnerability = 0x20000000, // 29 Unaffected By Invulnerability (Hmm Possible Not...)
        BreakableByDamage = 0x40000000, // 30
        CantCancel = 0x80000000  // 31 Positive Aura Can'T Be Canceled

    }
    public enum SpellAttr1 : uint
    {
        DismissPet = 0x00000001, //  0 For Spells Without This Flag Client Doesn'T Allow To Summon Pet If Caster Has A Pet
        DrainAllPower = 0x00000002, //  1 Use All Power (Only Paladin Lay Of Hands And Bunyanize)
        Channeled1 = 0x00000004, //  2 Clientside Checked? Cancelable?
        CantBeRedirected = 0x00000008, //  3
        Unk4 = 0x00000010, //  4 Stealth And Whirlwind
        NotBreakStealth = 0x00000020, //  5 Not Break Stealth
        Channeled2 = 0x00000040, //  6
        CantBeReflected = 0x00000080, //  7
        CantTargetInCombat = 0x00000100, //  8 Can Target Only Out Of Combat Units
        MeleeCombatStart = 0x00000200, //  9 Player Starts Melee Combat After This Spell Is Cast
        NoThreat = 0x00000400, // 10 No Generates Threat On Cast 100% (Old NoInitialAggro)
        Unk11 = 0x00000800, // 11 Aura
        IsPickpocket = 0x00001000, // 12 Pickpocket
        Farsight = 0x00002000, // 13 Client Removes Farsight On Aura Loss
        ChannelTrackTarget = 0x00004000, // 14 Client Automatically Forces Player To Face Target When Channeling
        DispelAurasOnImmunity = 0x00008000, // 15 Remove Auras On Immunity
        UnaffectedBySchoolImmune = 0x00010000, // 16 On Immuniy
        UnautocastableByPet = 0x00020000, // 17
        Unk18 = 0x00040000, // 18 Stun, Polymorph, Daze, Hex
        CantTargetSelf = 0x00080000, // 19
        ReqComboPoints1 = 0x00100000, // 20 Req Combo Points On Target
        Unk21 = 0x00200000, // 21
        ReqComboPoints2 = 0x00400000, // 22 Req Combo Points On Target
        Unk23 = 0x00800000, // 23
        IsFishing = 0x01000000, // 24 Only Fishing Spells
        Unk25 = 0x02000000, // 25
        Unk26 = 0x04000000, // 26 Works Correctly With [Target=Focus] And [Target=Mouseover] Macros?
        Unk27 = 0x08000000, // 27 Melee Spell?
        DontDisplayInAuraBar = 0x10000000, // 28 Client Doesn'T Display These Spells In Aura Bar
        ChannelDisplaySpellName = 0x20000000, // 29 Spell Name Is Displayed In Cast Bar Instead Of 'Channeling' Text
        EnableAtDodge = 0x40000000, // 30 Overpower
        Unk31 = 0x80000000  // 31
    }
    public enum SpellAttr2 : uint
    {
        CanTargetDead = 0x00000001, //  0 Can Target Dead Unit Or Corpse
        Unk1 = 0x00000002, //  1 Vanish, Shadowform, Ghost Wolf And Other
        CanTargetNotInLos = 0x00000004, //  2 26368 4.0.1 Dbc Change
        Unk3 = 0x00000008, //  3
        DisplayInStanceBar = 0x00000010, //  4 Client Displays Icon In Stance Bar When Learned, Even If Not Shapeshift
        AutorepeatFlag = 0x00000020, //  5
        CantTargetTapped = 0x00000040, //  6 Target Must Be Tapped By Caster
        Unk7 = 0x00000080, //  7
        Unk8 = 0x00000100, //  8 Not Set In 3.0.3
        Unk9 = 0x00000200, //  9
        Unk10 = 0x00000400, // 10 Related To Tame
        HealthFunnel = 0x00000800, // 11
        Unk12 = 0x00001000, // 12 Cleave, Heart Strike, Maul, Sunder Armor, Swipe
        PreserveEnchantInArena = 0x00002000, // 13 Items Enchanted By Spells With This Flag Preserve The Enchant To Arenas
        Unk14 = 0x00004000, // 14
        Unk15 = 0x00008000, // 15 Not Set In 3.0.3
        TameBeast = 0x00010000, // 16
        NotResetAutoActions = 0x00020000, // 17 Don'T Reset Timers For Melee Autoattacks (Swings) Or Ranged Autoattacks (Autoshoots)
        ReqDeadPet = 0x00040000, // 18 Only Revive Pet And Heart Of The Pheonix
        NotNeedShapeshift = 0x00080000, // 19 Does Not Necessarly Need Shapeshift
        Unk20 = 0x00100000, // 20
        DamageReducedShield = 0x00200000, // 21 For Ice Blocks, Pala Immunity Buffs, Priest Absorb Shields, But Used Also For Other Spells -> Not Sure!
        Unk22 = 0x00400000, // 22 Ambush, Backstab, Cheap Shot, Death Grip, Garrote, Judgements, Mutilate, Pounce, Ravage, Shiv, Shred
        IsArcaneConcentration = 0x00800000, // 23 Only Mage Arcane Concentration Have This Flag
        Unk24 = 0x01000000, // 24
        Unk25 = 0x02000000, // 25
        Unk26 = 0x04000000, // 26 Unaffected By School Immunity
        Unk27 = 0x08000000, // 27
        Unk28 = 0x10000000, // 28
        CantCrit = 0x20000000, // 29 Spell Can'T Crit
        TriggeredCanTriggerProc = 0x40000000, // 30 Spell Can Trigger Even If Triggered
        FoodBuff = 0x80000000  // 31 Food Or Drink Buff (Like Well Fed)
    }
    public enum SpellAttr3 : uint
    {
        Unk0 = 0x00000001, //  0
        Unk1 = 0x00000002, //  1
        Unk2 = 0x00000004, //  2
        BlockableSpell = 0x00000008, //  3 Only Dmg Class Melee In 3.1.3
        IgnoreResurrectionTimer = 0x00000010, //  4 You Don'T Have To Wait To Be Resurrected With These Spells
        Unk5 = 0x00000020, //  5
        Unk6 = 0x00000040, //  6
        StackForDiffCasters = 0x00000080, //  7 Separate Stack For Every Caster
        OnlyTargetPlayers = 0x00000100, //  8 Can Only Target Players
        TriggeredCanTriggerProc2 = 0x00000200, //  9 Triggered From Effect?
        MainHand = 0x00000400, // 10 Main Hand Weapon Required
        Battleground = 0x00000800, // 11 Can Casted Only On Battleground
        OnlyTargetGhosts = 0x00001000, // 12
        Unk13 = 0x00002000, // 13
        IsHonorlessTarget = 0x00004000, // 14 "Honorless Target" Only This Spells Have This Flag
        Unk15 = 0x00008000, // 15 Auto Shoot, Shoot, Throw,  - This Is Autoshot Flag
        CantTriggerProc = 0x00010000, // 16 Confirmed With Many Patchnotes
        NoInitialAggro = 0x00020000, // 17 Soothe Animal, 39758, Mind Soothe
        IgnoreHitResult = 0x00040000, // 18 Spell Should Always Hit Its Target
        DisableProc = 0x00080000, // 19 During Aura Proc No Spells Can Trigger (20178, 20375)
        DeathPersistent = 0x00100000, // 20 Death Persistent Spells
        Unk21 = 0x00200000, // 21 Unused
        ReqWand = 0x00400000, // 22 Req Wand
        Unk23 = 0x00800000, // 23
        ReqOffhand = 0x01000000, // 24 Req Offhand Weapon
        Unk25 = 0x02000000, // 25 No Cause Spell Pushback ?
        CanProcWithTriggered = 0x04000000, // 26 Auras With This Attribute Can Proc From Triggered Spell Casts With TriggeredCanTriggerProc2 (67736 + 52999)
        DrainSoul = 0x08000000, // 27 Only Drain Soul Has This Flag
        Unk28 = 0x10000000, // 28
        NoDoneBonus = 0x20000000, // 29 Ignore Caster Spellpower And Done Damage Mods?  Client Doesn'T Apply Spellmods For Those Spells
        DontDisplayRange = 0x40000000, // 30 Client Doesn'T Display Range In Tooltip For Those Spells
        Unk31 = 0x80000000  // 31
    }
    public enum SpellAttr4 : uint
    {
        IgnoreResistances = 0x00000001, //  0 Spells With This Attribute Will Completely Ignore The Target'S Resistance (These Spells Can'T Be Resisted)
        ProcOnlyOnCaster = 0x00000002, //  1 Proc Only On Effects With TargetUnitCaster?
        Unk2 = 0x00000004, //  2
        Unk3 = 0x00000008, //  3
        Unk4 = 0x00000010, //  4 This Will No Longer Cause Guards To Attack On Use??
        Unk5 = 0x00000020, //  5
        NotStealable = 0x00000040, //  6 Although Such Auras Might Be Dispellable, They Cannot Be Stolen
        Triggered = 0x00000080, //  7 Spells Forced To Be Triggered
        Unk8 = 0x00000100, //  8 Ignores Taken Percent Damage Mods?
        TriggerActivate = 0x00000200, //  9 Initially Disabled / Trigger Activate From Event (Execute, Riposte, Deep Freeze End Other)
        SpellVsExtendCost = 0x00000400, // 10 Rogue Shiv Have This Flag
        Unk11 = 0x00000800, // 11
        Unk12 = 0x00001000, // 12
        Unk13 = 0x00002000, // 13
        DamageDoesntBreakAuras = 0x00004000, // 14 Doesn'T Break Auras By Damage From These Spells
        Unk15 = 0x00008000, // 15
        NotUsableInArenaOrRatedBg = 0x00010000, // 16 Cannot Be Used In Both Arenas Or Rated Battlegrounds
        UsableInArena = 0x00020000, // 17
        AreaTargetChain = 0x00040000, // 18 (Nyi)Hits Area Targets One After Another Instead Of All At Once
        Unk19 = 0x00080000, // 19 Proc Dalayed, After Damage Or Don'T Proc On Absorb?
        NotCheckSelfcastPower = 0x00100000, // 20 Supersedes Message "More Powerful Spell Applied" For Self Casts.
        Unk21 = 0x00200000, // 21 Pally Aura, Dk Presence, Dudu Form, Warrior Stance, Shadowform, Hunter Track
        Unk22 = 0x00400000, // 22 Seal Of Command (42058,57770) And Gymer'S Smash 55426
        Unk23 = 0x00800000, // 23
        Unk24 = 0x01000000, // 24 Some Shoot Spell
        IsPetScaling = 0x02000000, // 25 Pet Scaling Auras
        CastOnlyInOutland = 0x04000000, // 26 Can Only Be Used In Outland.
        Unk27 = 0x08000000, // 27
        Unk28 = 0x10000000, // 28 Aimed Shot
        Unk29 = 0x20000000, // 29
        Unk30 = 0x40000000, // 30
        Unk31 = 0x80000000  // 31 Polymorph (Chicken) 228 And Sonic Boom (38052,38488)
    }
    public enum SpellAttr5 : uint
    {
        Unk0 = 0x00000001, //  0
        NoReagentWhilePrep = 0x00000002, //  1 Not Need Reagents If UnitFlagPreparation
        Unk2 = 0x00000004, //  2
        UsableWhileStunned = 0x00000008, //  3 Usable While Stunned
        Unk4 = 0x00000010, //  4
        SingleTargetSpell = 0x00000020, //  5 Only One Target Can Be Apply At A Time
        Unk6 = 0x00000040, //  6
        Unk7 = 0x00000080, //  7
        Unk8 = 0x00000100, //  8
        StartPeriodicAtApply = 0x00000200, //  9 Begin Periodic Tick At Aura Apply
        HideDuration = 0x00000400, // 10 Do Not Send Duration To Client
        AllowTargetOfTargetAsTarget = 0x00000800, // 11 (Nyi) Uses Target'S Target As Target If Original Target Not Valid (Intervene For Example)
        Unk12 = 0x00001000, // 12 Cleave Related?
        HasteAffectDuration = 0x00002000, // 13 Haste Effects Decrease Duration Of This
        Unk14 = 0x00004000, // 14
        Unk15 = 0x00008000, // 15 Inflits On Multiple Targets?
        SpecialItemClassCheck = 0x00010000, // 16 This Allows Spells With Equippeditemclass To Affect Spells From Other Items If The Required Item Is Equipped
        UsableWhileFeared = 0x00020000, // 17 Usable While Feared
        UsableWhileConfused = 0x00040000, // 18 Usable While Confused
        DontTurnDuringCast = 0x00080000, // 19 Blocks Caster'S Turning When Casting (Client Does Not Automatically Turn Caster'S Model To Face UnitFieldTarget)
        Unk20 = 0x00100000, // 20
        Unk21 = 0x00200000, // 21
        Unk22 = 0x00400000, // 22
        Unk23 = 0x00800000, // 23
        Unk24 = 0x01000000, // 24
        Unk25 = 0x02000000, // 25
        Unk26 = 0x04000000, // 26 Aoe Related - Boulder, Cannon, Corpse Explosion, Fire Nova, Flames, Frost Bomb, Living Bomb, Seed Of Corruption, Starfall, Thunder Clap, Volley
        Unk27 = 0x08000000, // 27
        Unk28 = 0x10000000, // 28
        Unk29 = 0x20000000, // 29
        Unk30 = 0x40000000, // 30
        Unk31 = 0x80000000  // 31 Forces All Nearby Enemies To Focus Attacks Caster
    }
    public enum SpellAttr6 : uint
    {
        DontDisplayCooldown = 0x00000001, //  0 Client Doesn'T Display Cooldown In Tooltip For These Spells
        OnlyInArena = 0x00000002, //  1 Only Usable In Arena
        IgnoreCasterAuras = 0x00000004, //  2
        Unk3 = 0x00000008, //  3
        Unk4 = 0x00000010, //  4
        Unk5 = 0x00000020, //  5
        Unk6 = 0x00000040, //  6
        Unk7 = 0x00000080, //  7
        CantTargetCrowdControlled = 0x00000100, //  8
        Unk9 = 0x00000200, //  9
        CanTargetPossessedFriends = 0x00000400, // 10 Nyi!
        NotInRaidInstance = 0x00000800, // 11 Not Usable In Raid Instance
        CastableWhileOnVehicle = 0x00001000, // 12 Castable While Caster Is On Vehicle
        CanTargetInvisible = 0x00002000, // 13 Ignore Visibility Requirement For Spell Target (Phases, Invisibility, Etc.)
        Unk14 = 0x00004000, // 14
        Unk15 = 0x00008000, // 15 Only 54368, 67892
        Unk16 = 0x00010000, // 16
        Unk17 = 0x00020000, // 17 Mount Spell
        CastByCharmer = 0x00040000, // 18 Client Won'T Allow To Cast These Spells When Unit Is Not Possessed && Charmer Of Caster Will Be Original Caster
        Unk19 = 0x00080000, // 19 Only 47488, 50782
        Unk20 = 0x00100000, // 20 Only 58371, 62218
        ClientUiTargetEffects = 0x00200000, // 21 It'S Only Client-Side Attribute
        Unk22 = 0x00400000, // 22 Only 72054
        Unk23 = 0x00800000, // 23
        CanTargetUntargetable = 0x01000000, // 24
        Unk25 = 0x02000000, // 25 Exorcism, Flash Of Light
        Unk26 = 0x04000000, // 26 Related To Player Castable Positive Buff
        Unk27 = 0x08000000, // 27
        Unk28 = 0x10000000, // 28 Death Grip
        NoDonePctDamageMods = 0x20000000, // 29 Ignores Done Percent Damage Mods?
        Unk30 = 0x40000000, // 30
        Unk31 = 0x80000000  // 31 Some Special Cooldown Calc? Only 2894
    }
    public enum SpellAttr7 : uint
    {
        Unk0 = 0x00000001, //  0 Shaman'S New Spells (Call Of The ...), Feign Death.
        Unk1 = 0x00000002, //  1 Not Set In 3.2.2a.
        ReactivateAtResurrect = 0x00000004, //  2 Paladin'S Auras And 65607 Only.
        IsCheatSpell = 0x00000008, //  3 Cannot Cast If Caster Doesn'T Have Unitflag2 & UnitFlag2AllowCheatSpells
        Unk4 = 0x00000010, //  4 Only 47883 (Soulstone Resurrection) And Test Spell.
        SummonPlayerTotem = 0x00000020, //  5 Only Shaman Player Totems.
        Unk6 = 0x00000040, //  6 Dark Surge, Surge Of Light, Burning Breath Triggers (Boss Spells).
        Unk7 = 0x00000080, //  7 66218 (Launch) Spell.
        HordeOnly = 0x00000100, //  8 Teleports, Mounts And Other Spells.
        AllianceOnly = 0x00000200, //  9 Teleports, Mounts And Other Spells.
        DispelCharges = 0x00000400, // 10 Dispel And Spellsteal Individual Charges Instead Of Whole Aura.
        InterruptOnlyNonplayer = 0x00000800, // 11 Only Non-Player Casts Interrupt, Though Feral Charge - Bear Has It.
        Unk12 = 0x00001000, // 12 Not Set In 3.2.2a.
        Unk13 = 0x00002000, // 13 Not Set In 3.2.2a.
        Unk14 = 0x00004000, // 14 Only 52150 (Raise Dead - Pet) Spell.
        Unk15 = 0x00008000, // 15 Exorcism. Usable On Players? 100% Crit Chance On Undead And Demons?
        Unk16 = 0x00010000, // 16 Druid Spells (29166, 54833, 64372, 68285).
        Unk17 = 0x00020000, // 17 Only 27965 (Suicide) Spell.
        HasChargeEffect = 0x00040000, // 18 Only Spells That Have Charge Among Effects.
        ZoneTeleport = 0x00080000, // 19 Teleports To Specific Zones.
        Unk20 = 0x00100000, // 20 Blink, Divine Shield, Ice Block
        Unk21 = 0x00200000, // 21 Not Set
        Unk22 = 0x00400000, // 22
        Unk23 = 0x00800000, // 23 Motivate, Mutilate, Shattering Throw
        Unk24 = 0x01000000, // 24 Motivate, Mutilate, Perform Speech, Shattering Throw
        Unk25 = 0x02000000, // 25
        Unk26 = 0x04000000, // 26
        Unk27 = 0x08000000, // 27 Not Set
        Unk28 = 0x10000000, // 28 Related To Player Positive Buff
        Unk29 = 0x20000000, // 29 Only 69028, 71237
        Unk30 = 0x40000000, // 30 Burning Determination, Divine Sacrifice, Earth Shield, Prayer Of Mending
        Unk31 = 0x80000000  // 31 Only 70769
    }
    public enum SpellAttr8 : uint
    {
        Unk0 = 0x00000001, // 0
        Unk1 = 0x00000002, // 1
        Unk2 = 0x00000004, // 2
        Unk3 = 0x00000008, // 3
        Unk4 = 0x00000010, // 4
        Unk5 = 0x00000020, // 5
        Unk6 = 0x00000040, // 6
        Unk7 = 0x00000080, // 7
        Unk8 = 0x00000100, // 8
        DontResetPeriodicTimer = 0x00000200, // 9 Periodic Auras With This Flag Keep Old Periodic Timer When Refreshing At Close To One Tick Remaining (Kind Of Anti Dot Clipping)
        Unk10 = 0x00000400, // 10
        Unk11 = 0x00000800, // 11
        AuraSendAmount = 0x00001000, // 12 Aura Must Have Flag AflagAnyEffectAmountSent To Send Amount
        Unk13 = 0x00002000, // 13
        Unk14 = 0x00004000, // 14
        Unk15 = 0x00008000, // 15
        Unk16 = 0x00010000, // 16
        Unk17 = 0x00020000, // 17
        Unk18 = 0x00040000, // 18
        Unk19 = 0x00080000, // 19
        ArmorSpecialization = 0x00100000, // 20
        Unk21 = 0x00200000, // 21
        Unk22 = 0x00400000, // 22
        Unk23 = 0x00800000, // 23
        Unk24 = 0x01000000, // 24
        Unk25 = 0x02000000, // 25
        RaidMarker = 0x04000000, // 26 Probably Spell No Need Learn To Cast
        Unk27 = 0x08000000, // 27
        GuildPerks = 0x10000000, // 28
        Mastery = 0x20000000, // 29
        Unk30 = 0x40000000, // 30
        Unk31 = 0x80000000  // 31
    }
    public enum SpellAttr9 : uint
    {
        Unk0 = 0x00000001, // 0
        Unk1 = 0x00000002, // 1
        Unk2 = 0x00000004, // 2
        Unk3 = 0x00000008, // 3
        Unk4 = 0x00000010, // 4
        Unk5 = 0x00000020, // 5
        Unk6 = 0x00000040, // 6
        Unk7 = 0x00000080, // 7
        Unk8 = 0x00000100, // 8
        NotUsableInArena = 0x00000200, // 9 Cannot Be Used In Arenas
        Unk10 = 0x00000400, // 10
        Unk11 = 0x00000800, // 11
        Unk12 = 0x00001000, // 12
        Unk13 = 0x00002000, // 13
        UsableInRatedBattlegrounds = 0x00004000, // 14 Can Be Used In Rated Battlegrounds
        Unk15 = 0x00008000, // 15
        Unk16 = 0x00010000, // 16
        Unk17 = 0x00020000, // 17
        Unk18 = 0x00040000, // 18
        Unk19 = 0x00080000, // 19
        Unk20 = 0x00100000, // 20
        Unk21 = 0x00200000, // 21
        Unk22 = 0x00400000, // 22
        Unk23 = 0x00800000, // 23
        Unk24 = 0x01000000, // 24
        Unk25 = 0x02000000, // 25
        Unk26 = 0x04000000, // 26
        Unk27 = 0x08000000, // 27
        Unk28 = 0x10000000, // 28
        Unk29 = 0x20000000, // 29
        Unk30 = 0x40000000, // 30
        Unk31 = 0x80000000  // 31
    }
    public enum SpellAttr10 : uint
    {
        Unk0 = 0x00000001, // 0
        Unk1 = 0x00000002, // 1
        Unk2 = 0x00000004, // 2
        Unk3 = 0x00000008, // 3
        Unk4 = 0x00000010, // 4
        Unk5 = 0x00000020, // 5
        Unk6 = 0x00000040, // 6
        Unk7 = 0x00000080, // 7
        Unk8 = 0x00000100, // 8
        Unk9 = 0x00000200, // 9
        Unk10 = 0x00000400, // 10
        Unk11 = 0x00000800, // 11
        Unk12 = 0x00001000, // 12
        Unk13 = 0x00002000, // 13
        Unk14 = 0x00004000, // 14
        Unk15 = 0x00008000, // 15
        Unk16 = 0x00010000, // 16
        Unk17 = 0x00020000, // 17
        Unk18 = 0x00040000, // 18
        Unk19 = 0x00080000, // 19
        Unk20 = 0x00100000, // 20
        Unk21 = 0x00200000, // 21
        Unk22 = 0x00400000, // 22
        Unk23 = 0x00800000, // 23
        Unk24 = 0x01000000, // 24
        Unk25 = 0x02000000, // 25
        Unk26 = 0x04000000, // 26
        Unk27 = 0x08000000, // 27
        Unk28 = 0x10000000, // 28
        Unk29 = 0x20000000, // 29
        Unk30 = 0x40000000, // 30
        Unk31 = 0x80000000  // 31
    }
    #endregion

    //Effects
    public enum SpellEffects
    {
        Instakill = 1,
        SchoolDamage = 2,
        Dummy = 3,
        PortalTeleport = 4, // Unused (4.3.4)
        TeleportUnits = 5,
        ApplyAura = 6,
        EnvironmentalDamage = 7,
        PowerDrain = 8,
        HealthLeech = 9,
        Heal = 10,
        Bind = 11,
        Portal = 12,
        RitualBase = 13, // Unused (4.3.4)
        RitualSpecialize = 14, // Unused (4.3.4)
        RitualActivatePortal = 15, // Unused (4.3.4)
        QuestComplete = 16,
        WeaponDamageNoschool = 17,
        Resurrect = 18,
        AddExtraAttacks = 19,
        Dodge = 20,
        Evade = 21,
        Parry = 22,
        Block = 23,
        CreateItem = 24,
        Weapon = 25,
        Defense = 26,
        PersistentAreaAura = 27,
        Summon = 28,
        Leap = 29,
        Energize = 30,
        WeaponPercentDamage = 31,
        TriggerMissile = 32,
        OpenLock = 33,
        SummonChangeItem = 34,
        ApplyAreaAuraParty = 35,
        LearnSpell = 36,
        SpellDefense = 37,
        Dispel = 38,
        Language = 39,
        DualWield = 40,
        Jump = 41,
        JumpDest = 42,
        TeleportUnitsFaceCaster = 43,
        SkillStep = 44,
        AddHonor = 45,
        Spawn = 46,
        TradeSkill = 47,
        Stealth = 48,
        Detect = 49,
        TransDoor = 50,
        ForceCriticalHit = 51, // Unused (4.3.4)
        GuaranteeHit = 52, // Unused (4.3.4)
        EnchantItem = 53,
        EnchantItemTemporary = 54,
        Tamecreature = 55,
        SummonPet = 56,
        LearnPetSpell = 57,
        WeaponDamage = 58,
        CreateRandomItem = 59,
        Proficiency = 60,
        SendEvent = 61,
        PowerBurn = 62,
        Threat = 63,
        TriggerSpell = 64,
        ApplyAreaAuraRaid = 65,
        CreateManaGem = 66,
        HealMaxHealth = 67,
        InterruptCast = 68,
        Distract = 69,
        Pull = 70,
        Pickpocket = 71,
        AddFarsight = 72,
        UntrainTalents = 73,
        ApplyGlyph = 74,
        HealMechanical = 75,
        SummonObjectWild = 76,
        ScriptEffect = 77,
        Attack = 78,
        Sanctuary = 79,
        AddComboPoints = 80,
        CreateHouse = 81,
        BindSight = 82,
        Duel = 83,
        Stuck = 84,
        SummonPlayer = 85,
        ActivateObject = 86,
        GameobjectDamage = 87,
        GameobjectRepair = 88,
        GameobjectSetDestructionState = 89,
        KillCredit = 90,
        ThreatAll = 91,
        EnchantHeldItem = 92,
        ForceDeselect = 93,
        SelfResurrect = 94,
        Skinning = 95,
        Charge = 96,
        CastButton = 97,
        KnockBack = 98,
        Disenchant = 99,
        Inebriate = 100,
        FeedPet = 101,
        DismissPet = 102,
        Reputation = 103,
        SummonObjectSlot1 = 104,
        SummonObjectSlot2 = 105,
        SummonObjectSlot3 = 106,
        SummonObjectSlot4 = 107,
        DispelMechanic = 108,
        SummonDeadPet = 109,
        DestroyAllTotems = 110,
        DurabilityDamage = 111,
        Effect112 = 112,
        ResurrectNew = 113,
        AttackMe = 114,
        DurabilityDamagePct = 115,
        SkinPlayerCorpse = 116,
        SpiritHeal = 117,
        Skill = 118,
        ApplyAreaAuraPet = 119,
        TeleportGraveyard = 120,
        NormalizedWeaponDmg = 121,
        Effect122 = 122, // Unused (4.3.4)
        SendTaxi = 123,
        PullTowards = 124,
        ModifyThreatPercent = 125,
        StealBeneficialBuff = 126,
        Prospecting = 127,
        ApplyAreaAuraFriend = 128,
        ApplyAreaAuraEnemy = 129,
        RedirectThreat = 130,
        Effect131 = 131,
        PlayMusic = 132,
        UnlearnSpecialization = 133,
        KillCredit2 = 134,
        CallPet = 135,
        HealPct = 136,
        EnergizePct = 137,
        LeapBack = 138,
        ClearQuest = 139,
        ForceCast = 140,
        ForceCastWithValue = 141,
        TriggerSpellWithValue = 142,
        ApplyAreaAuraOwner = 143,
        KnockBackDest = 144,
        PullTowardsDest = 145,
        ActivateRune = 146,
        QuestFail = 147,
        TriggerMissileSpellWithValue = 148,
        ChargeDest = 149,
        QuestStart = 150,
        TriggerSpell2 = 151,
        SummonRafFriend = 152,
        CreateTamedPet = 153,
        DiscoverTaxi = 154,
        TitanGrip = 155,
        EnchantItemPrismatic = 156,
        CreateItem2 = 157,
        Milling = 158,
        AllowRenamePet = 159,
        Effect160 = 160,
        TalentSpecCount = 161,
        TalentSpecSelect = 162,
        Effect163 = 163, // Unused (4.3.4)
        RemoveAura = 164,
        Effect165 = 165,
        Effect166 = 166,
        Effect167 = 167,
        Effect168 = 168,
        Effect169 = 169,
        Effect170 = 170,
        Effect171 = 171, // Summons Gamebject
        Effect172 = 172, // Aoe Ressurection
        UnlockGuildVaultTab = 173, // Guild Tab Unlocked (Guild Perk)
        Effect174 = 174,
        Effect175 = 175, // Unused (4.3.4)
        Effect176 = 176, // Some Kind Of Sanctuary Effect (Vanish)
        Effect177 = 177,
        Effect178 = 178, // Unused (4.3.4)
        Effect179 = 179,
        Effect180 = 180, // Unused (4.3.4)
        Effect181 = 181, // Unused (4.3.4)
        Effect182 = 182,
        TotalSpellEffects = 183,
    }

    public enum SpellEffectHandle
    {
        Launch,
        LaunchTarget,
        Hit,
        HitTarget
    }
}
