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
    public struct ItemConst
    {
        public const int MaxDamages = 2;                           // changed in 3.1.0
        public const int MaxSockets = 3;
        public const int MaxSpells = 5;
        public const int MaxStats = 10;
        public const int MaxBagSize = 36;
        public const byte NullBag = 0;
        public const byte NullSlot = 255;
        public const int MaxOutfitItems = 24;
        public const int MaxExtCostItems = 5;
        public const int MaxExtCostCurrencies = 5;
        public const int MaxEnchantmentEffects = 3;
    }
    public struct InventorySlots
    {
        public const byte BagStart = 19;
        public const byte BagEnd = 23;
        public const byte ItemStart = 23;
        public const byte ItemEnd = 39;

        public const byte BankItemStart = 39;
        public const byte BankItemEnd = 67;
        public const byte BankBagStart = 67;
        public const byte BankBagEnd = 74;

        public const byte BuyBackStart = 74;
        public const byte BuyBackEnd = 86;
        public const byte KeyRingStart = 86;
        public const byte KeyRingEnd = 118;
        public const byte CurrencyTokenStart = 118;
        public const byte CurrencyTokenEnd = 150;

        public const byte Bag0 = 255;
    }

    public enum BagFamilyMask
    {
        NONE = 0x00000000,
        ARROWS = 0x00000001,
        BULLETS = 0x00000002,
        SOUL_SHARDS = 0x00000004,
        LEATHERWORKING_SUPP = 0x00000008,
        INSCRIPTION_SUPP = 0x00000010,
        HERBS = 0x00000020,
        ENCHANTING_SUPP = 0x00000040,
        ENGINEERING_SUPP = 0x00000080,
        KEYS = 0x00000100,
        GEMS = 0x00000200,
        MINING_SUPP = 0x00000400,
        SOULBOUND_EQUIPMENT = 0x00000800,
        VANITY_PETS = 0x00001000,
        CURRENCY_TOKENS = 0x00002000,
        QUEST_ITEMS = 0x00004000,
        FISHING_SUPP = 0x00008000,
    }

    public enum InventoryType
    {
        NonEquip = 0,
        Head = 1,
        Neck = 2,
        Shoulders = 3,
        Body = 4,
        Chest = 5,
        Waist = 6,
        Legs = 7,
        Feet = 8,
        Wrists = 9,
        Hands = 10,
        Finger = 11,
        Trinket = 12,
        Weapon = 13,
        Shield = 14,
        Ranged = 15,
        Cloak = 16,
        Weapon2Hand = 17,
        Bag = 18,
        Tabard = 19,
        Robe = 20,
        WeaponMainhand = 21,
        WeaponOffhand = 22,
        Holdable = 23,
        Ammo = 24,
        Thrown = 25,
        RangedRight = 26,
        Quiver = 27,
        Relic = 28
    }
    public enum VisibleEquipmentSlot
    {
        Head = 0,
        Shoulder = 2,
        Shirt = 3,
        Chest = 4,
        Belt = 5,
        Pants = 6,
        Boots = 7,
        Wrist = 8,
        Gloves = 9,
        Back = 14,
        Tabard = 18
    }
    public enum EquipmentSlot
    {
        Start = 0,
        Head = 0,
        Neck = 1,
        Shoulders = 2,
        Shirt = 3,
        Chest = 4,
        Waist = 5,
        Legs = 6,
        Feet = 7,
        Wrist = 8,
        Hands = 9,
        Finger1 = 10,
        Finger2 = 11,
        Trinket1 = 12,
        Trinket2 = 13,
        Cloak = 14,
        MainHand = 15,
        OffHand = 16,
        Ranged = 17,
        Tabard = 18,
        End = 19
    }

    public enum ItemBondingType
    {
        None = 0,
        PickedUp = 1,
        Equiped = 2,
        Use = 3,
        QuestItem = 4,
        QuestItem1 = 5         // not used in game
    }

    public enum ItemClass
    {
        Consumable = 0,
        Container = 1,
        Weapon = 2,
        Gem = 3,
        Armor = 4,
        Reagent = 5,
        Projectile = 6,
        Trade_Goods = 7,
        Generic = 8,  // Obsolete
        Recipe = 9,
        Money = 10, // Obsolete
        Quiver = 11,
        Quest = 12,
        Key = 13,
        Permanent = 14, // Obsolete
        Miscellaneous = 15,
        Glyph = 16,
        Max = 17
    }

    public enum ItemSubClassConsumable
    {
        Consumable = 0,
        Potion = 1,
        Elixir = 2,
        Flask = 3,
        Scroll = 4,
        FoodDrink = 5,
        ItemEnhancement = 6,
        Bandage = 7,
        ConsumableOther = 8,
        Max = 9
    }

    public enum ItemSubClassContainer
    {
        Container = 0,
        SoulContainer = 1,
        HerbContainer = 2,
        EnchantingContainer = 3,
        EngineeringContainer = 4,
        GemContainer = 5,
        MiningContainer = 6,
        LeatherworkingContainer = 7,
        InscriptionContainer = 8,
        TackleContainer = 9,
        Max = 10
    }

    public enum ItemSubClassWeapon
    {
        Axe = 0,  // One-Handed Axes
        Axe2 = 1,  // Two-Handed Axes
        Bow = 2,
        Gun = 3,
        Mace = 4,  // One-Handed Maces
        Mace2 = 5,  // Two-Handed Maces
        Polearm = 6,
        Sword = 7,  // One-Handed Swords
        Sword2 = 8,  // Two-Handed Swords
        Obsolete = 9,
        Staff = 10,
        Exotic = 11, // One-Handed Exotics
        Exotic2 = 12, // Two-Handed Exotics
        Fist = 13,
        Miscellaneous = 14,
        Dagger = 15,
        Thrown = 16,
        Spear = 17,
        Crossbow = 18,
        Wand = 19,
        FishingPole = 20,

        MaskRanged = (1 << Bow) | (1 << Gun) | (1 << Crossbow) | (1 << Thrown),

        Max = 12

    }

    public enum ItemSubClassGem
    {
        Red = 0,
        Blue = 1,
        Yellow = 2,
        Purple = 3,
        Green = 4,
        Orange = 5,
        Meta = 6,
        Simple = 7,
        Prismatic = 8,
        Hydraulic = 9,
        Cogwheel = 10,
        Max = 11
    }

    public enum ItemSubClassArmor
    {
        Miscellaneous = 0,
        Cloth = 1,
        Leather = 2,
        Mail = 3,
        Plate = 4,
        Buckler = 5, // Obsolete
        Shield = 6,
        Libram = 7,
        Idol = 8,
        Totem = 9,
        Sigil = 10,
        Relic = 11,
        Max = 12
    }

    public enum ItemSubClassReagent
    {
        Reagent = 0,
        Max = 1
    }

    public enum ItemSubClassProjectile
    {
        Wand = 0, // Obsolete
        Bolt = 1, // Obsolete
        Arrow = 2,
        Bullet = 3,
        Thrown = 4,  // Obsolete
        Max = 5
    }

    public enum ItemSubClassTradeGoods
    {
        TradeGoods = 0,
        Parts = 1,
        Explosives = 2,
        Devices = 3,
        Jewelcrafting = 4,
        Cloth = 5,
        Leather = 6,
        MetalStone = 7,
        Meat = 8,
        Herb = 9,
        Elemental = 10,
        TradeGoodsOther = 11,
        Enchanting = 12,
        Material = 13,
        Enchantment = 14,
        Max = 15
    }

    public enum ItemSubClassGeneric
    {
        Generic = 0,  // Obsolete
        Max = 1
    }

    public enum ItemSubClassRecipe
    {
        Book = 0,
        LeatherworkingPattern = 1,
        TailoringPattern = 2,
        EngineeringSchematic = 3,
        Blacksmithing = 4,
        CookingRecipe = 5,
        AlchemyRecipe = 6,
        FirstAidManual = 7,
        EnchantingFormula = 8,
        FishingManual = 9,
        JewelcraftingRecipe = 10,
        InscriptionTechnique = 11,
        Max = 12
    }

    public enum ItemSubClassMoney
    {
        Money = 0,  // Obsolete
        Unk7 = 7,  // Obsolete, 1 Item (41749)
        Max = 8
    }

    public enum ItemSubClassQuiver
    {
        Quiver0 = 0, // Obsolete
        Quiver1 = 1, // Obsolete
        Quiver = 2,
        AmmoPouch = 3,
        Max = 4,
    }

    public enum ItemSubClassQuest
    {
        Quest = 0,
        Unk3 = 3, // 1 Item (33604)
        Unk8 = 8, // 2 Items (37445, 49700)
        Max = 9
    }

    public enum ItemSubClassKey
    {
        Key = 0,
        Lockpick = 1,
        Max = 2
    }

    public enum ItemSubClassPermanent
    {
        Permanent = 0,
        Max = 1
    }

    public enum ItemSubClassJunk
    {
        Junk = 0,
        Reagent = 1,
        Pet = 2,
        Holiday = 3,
        Other = 4,
        Mount = 5,
        Unk12 = 12, // 1 Item (37677)
        Max = 13
    }

    public enum ItemSubClassGlyph
    {
        Warrior = 1,
        Paladin = 2,
        Hunter = 3,
        Rogue = 4,
        Priest = 5,
        DeathKnight = 6,
        Shaman = 7,
        Mage = 8,
        Warlock = 9,
        Druid = 11,
        Max = 12
    }

    public enum ItemQuality
    {
        Poor = 0,                 //Grey
        Normal = 1,                 //White
        Uncommon = 2,                 //Green
        Rare = 3,                 //Blue
        Epic = 4,                 //Purple
        Legendary = 5,                 //Orange
        Artifact = 6,                 //Light Yellow
        Heirloom = 7
    }

    public enum ItemFieldFlags : uint
    {
        Soulbound = 0x00000001, // Item Is Soulbound And Cannot Be Traded <<--
        Unk1 = 0x00000002, // ?
        Unlocked = 0x00000004, // Item Had Lock But Can Be Opened Now
        Wrapped = 0x00000008, // Item Is Wrapped And Contains Another Item
        Unk2 = 0x00000010, // ?
        Unk3 = 0x00000020, // ?
        Unk4 = 0x00000040, // ?
        Unk5 = 0x00000080, // ?
        BopTradeable = 0x00000100, // Allows Trading Soulbound Items
        Readable = 0x00000200, // Opens Text Page When Right Clicked
        Unk6 = 0x00000400, // ?
        Unk7 = 0x00000800, // ?
        Refundable = 0x00001000, // Item Can Be Returned To Vendor For Its Original Cost (Extended Cost)
        Unk8 = 0x00002000, // ?
        Unk9 = 0x00004000, // ?
        Unk10 = 0x00008000, // ?
        Unk11 = 0x00010000, // ?
        Unk12 = 0x00020000, // ?
        Unk13 = 0x00040000, // ?
        Unk14 = 0x00080000, // ?
        Unk15 = 0x00100000, // ?
        Unk16 = 0x00200000, // ?
        Unk17 = 0x00400000, // ?
        Unk18 = 0x00800000, // ?
        Unk19 = 0x01000000, // ?
        Unk20 = 0x02000000, // ?
        Unk21 = 0x04000000, // ?
        Unk22 = 0x08000000, // ?
        Unk23 = 0x10000000, // ?
        Unk24 = 0x20000000, // ?
        Unk25 = 0x40000000, // ?
        Unk26 = 0x80000000, // ?

        MailTextMask = Readable | Unk13 | Unk14
    }

    public enum ItemFlags : long
    {
        Unk1 = 0x00000001, // ?
        Conjured = 0x00000002, // Conjured Item
        Openable = 0x00000004, // Item Can Be Right Clicked To Open For Loot
        Heroic = 0x00000008, // Makes Green "Heroic" Text Appear On Item
        Deprecated = 0x00000010, // Cannot Equip Or Use
        Indestructible = 0x00000020, // Item Can Not Be Destroyed, Except By Using Spell (Item Can Be Reagent For Spell)
        Unk2 = 0x00000040, // ?
        NoEquipCooldown = 0x00000080, // No Default 30 Seconds Cooldown When Equipped
        Unk3 = 0x00000100, // ?
        Wrapper = 0x00000200, // Item Can Wrap Other Items
        Unk4 = 0x00000400, // ?
        PartyLoot = 0x00000800, // Looting This Item Does Not Remove It From Available Loot
        Refundable = 0x00001000, // Item Can Be Returned To Vendor For Its Original Cost (Extended Cost)
        Charter = 0x00002000, // Item Is Guild Or Arena Charter
        Unk5 = 0x00004000, // Only Readable Items Have This (But Not All)
        Unk6 = 0x00008000, // ?
        Unk7 = 0x00010000, // ?
        Unk8 = 0x00020000, // ?
        Prospectable = 0x00040000, // Item Can Be Prospected
        UniqueEquipped = 0x00080000, // You Can Only Equip One Of These
        Unk9 = 0x00100000, // ?
        UseableInArena = 0x00200000, // Item Can Be Used During Arena Match
        Throwable = 0x00400000, // Some Thrown Weapons Have It (And Only Thrown) But Not All
        UsableWhenShapeshifted = 0x00800000, // Item Can Be Used In Shapeshift Forms
        Unk10 = 0x01000000, // ?
        SmartLoot = 0x02000000, // Profession Recipes: Can Only Be Looted If You Meet Requirements And Don'T Already Know It
        NotUseableInArena = 0x04000000, // Item Cannot Be Used In Arena
        BindToAccount = 0x08000000, // Item Binds To Account And Can Be Sent Only To Your Own Characters
        TriggeredCast = 0x10000000, // Spell Is Cast With Triggered Flag
        Millable = 0x20000000, // Item Can Be Milled
        Unk11 = 0x40000000, // ?
        BopTradeable = 0x80000000  // Bound Item That Can Be Traded
    }

    public enum ItemFlags2
    {
        HordeOnly = 0x00000001,
        AllianceOnly = 0x00000002,
        ExtCostRequiresGold = 0x00000004, // When Item Uses Extended Cost, Gold Is Also Required
        NeedRollDisabled = 0x00000100,
        CasterWeapon = 0x00000200,
        HasNormalPrice = 0x00004000,
        BnetAccountBound = 0x00020000,
        CannotBeTransmog = 0x00200000,
        CannotTransmog = 0x00400000,
        CanTransmog = 0x00800000,
    }

    public enum InventoryResult
    {
        Ok = 0,
        CantEquipLevelI = 1,  // You Must Reach Level %D To Use That Item.
        CantEquipSkill = 2,  // You Aren'T Skilled Enough To Use That Item.
        WrongSlot = 3,  // That Item Does Not Go In That Slot.
        BagFull = 4,  // That Bag Is Full.
        BagInBag = 5,  // Can'T Put Non-Empty Bags In Other Bags.
        TradeEquippedBag = 6,  // You Can'T Trade Equipped Bags.
        AmmoOnly = 7,  // Only Ammo Can Go There.
        ProficiencyNeeded = 8,  // You Do Not Have The Required Proficiency For That Item.
        NoSlotAvailable = 9,  // No Equipment Slot Is Available For That Item.
        CantEquipEver = 10, // You Can Never Use That Item.
        CantEquipEver2 = 11, // You Can Never Use That Item.
        NoSlotAvailable2 = 12, // No Equipment Slot Is Available For That Item.
        Equipped2handed = 13, // Cannot Equip That With A Two-Handed Weapon.
        skillnotfound2h = 14, // You Cannot Dual-Wield
        WrongBagType = 15, // That Item Doesn'T Go In That Container.
        WrongBagType2 = 16, // That Item Doesn'T Go In That Container.
        ItemMaxCount = 17, // You Can'T Carry Any More Of Those Items.
        NoSlotAvailable3 = 18, // No Equipment Slot Is Available For That Item.
        CantStack = 19, // This Item Cannot Stack.
        NotEquippable = 20, // This Item Cannot Be Equipped.
        CantSwap = 21, // These Items Can'T Be Swapped.
        SlotEmpty = 22, // That Slot Is Empty.
        ItemNotFound = 23, // The Item Was Not Found.
        DropBoundItem = 24, // You Can'T Drop A Soulbound Item.
        OutOfRange = 25, // Out Of Range.
        TooFewToSplit = 26, // Tried To Split More Than Number In Stack.
        SplitFailed = 27, // Couldn'T Split Those Items.
        SpellFailedReagentsGeneric = 28, // Missing Reagent
        NotEnoughMoney = 29, // You Don'T Have Enough Money.
        NotABag = 30, // Not A Bag.
        DestroyNonemptyBag = 31, // You Can Only Do That With Empty Bags.
        NotOwner = 32, // You Don'T Own That Item.
        OnlyOneQuiver = 33, // You Can Only Equip One Quiver.
        NoBankSlot = 34, // You Must Purchase That Bag Slot First
        NoBankHere = 35, // You Are Too Far Away From A Bank.
        ItemLocked = 36, // Item Is Locked.
        GenericStunned = 37, // You Are Stunned
        PlayerDead = 38, // You Can'T Do That When You'Re Dead.
        ClientLockedOut = 39, // You Can'T Do That Right Now.
        InternalBagError = 40, // Internal Bag Error
        OnlyOneBolt = 41, // You Can Only Equip One Quiver.
        OnlyOneAmmo = 42, // You Can Only Equip One Ammo Pouch.
        CantWrapStackable = 43, // Stackable Items Can'T Be Wrapped.
        CantWrapEquipped = 44, // Equipped Items Can'T Be Wrapped.
        CantWrapWrapped = 45, // Wrapped Items Can'T Be Wrapped.
        CantWrapBound = 46, // Bound Items Can'T Be Wrapped.
        CantWrapUnique = 47, // Unique Items Can'T Be Wrapped.
        CantWrapBags = 48, // Bags Can'T Be Wrapped.
        LootGone = 49, // Already Looted
        InvFull = 50, // Inventory Is Full.
        BankFull = 51, // Your Bank Is Full
        VendorSoldOut = 52, // That Item Is Currently Sold Out.
        BagFull2 = 53, // That Bag Is Full.
        ItemNotFound2 = 54, // The Item Was Not Found.
        CantStack2 = 55, // This Item Cannot Stack.
        BagFull3 = 56, // That Bag Is Full.
        VendorSoldOut2 = 57, // That Item Is Currently Sold Out.
        ObjectIsBusy = 58, // That Object Is Busy.
        CantBeDisenchanted = 59,
        NotInCombat = 60, // You Can'T Do That While In Combat
        NotWhileDisarmed = 61, // You Can'T Do That While Disarmed
        BagFull4 = 62, // That Bag Is Full.
        CantEquipRank = 63, // You Don'T Have The Required Rank For That Item
        CantEquipReputation = 64, // You Don'T Have The Required Reputation For That Item
        TooManySpecialBags = 65, // You Cannot Equip Another Bag Of That Type
        LootCantLootThatNow = 66, // You Can'T Loot That Item Now.
        ItemUniqueEquippable = 67, // You Cannot Equip More Than One Of Those.
        VendorMissingTurnins = 68, // You Do Not Have The Required Items For That Purchase
        NotEnoughHonorPoints = 69, // You Don'T Have Enough Honor Points
        NotEnoughArenaPoints = 70, // You Don'T Have Enough Arena Points
        ItemMaxCountSocketed = 71, // You Have The Maximum Number Of Those Gems In Your Inventory Or Socketed Into Items.
        MailBoundItem = 72, // You Can'T Mail Soulbound Items.
        InternalBagError2 = 73, // Internal Bag Error
        BagFull5 = 74, // That Bag Is Full.
        ItemMaxCountEquippedSocketed = 75, // You Have The Maximum Number Of Those Gems Socketed Into Equipped Items.
        ItemUniqueEquippableSocketed = 76, // You Cannot Socket More Than One Of Those Gems Into A Single Item.
        TooMuchGold = 77, // At Gold Limit
        NotDuringArenaMatch = 78, // You Can'T Do That While In An Arena Match
        TradeBoundItem = 79, // You Can'T Trade A Soulbound Item.
        CantEquipRating = 80, // You Don'T Have The Personal, Team, Or Battleground Rating Required To Buy That Item
        NoOutput = 81,
        NotSameAccount = 82, // Account-Bound Items Can Only Be Given To Your Own Characters.
        ItemMaxLimitCategoryCountExceededIs = 84, // You Can Only Carry %D %S
        ItemMaxLimitCategorySocketedExceededIs = 85, // You Can Only Equip %D |4item:Items In The %S Category
        ScalingStatItemLevelExceeded = 86, // Your Level Is Too High To Use That Item
        PurchaseLevelTooLow = 87, // You Must Reach Level %D To Purchase That Item.
        CantEquipNeedTalent = 88, // You Do Not Have The Required Talent To Equip That.
        ItemMaxLimitCategoryEquippedExceededIs = 89, // You Can Only Equip %D |4item:Items In The %S Category
        ShapeshiftFormCannotEquip = 90, // Cannot Equip Item In This Form
        ItemInventoryFullSatchel = 91, // Your Inventory Is Full. Your Satchel Has Been Delivered To Your Mailbox.
        ScalingStatItemLevelTooLow = 92, // Your Level Is Too Low To Use That Item
        CantBuyQuantity = 93, // You Can'T Buy The Specified Quantity Of That Item.
    }

    public enum BuyResult
    {
        CantFindItem = 0,
        ItemAlreadySold = 1,
        NotEnoughtMoney = 2,
        SellerDontLikeYou = 4,
        DistanceTooFar = 5,
        ItemSoldOut = 7,
        CantCarryMore = 8,
        RankRequire = 11,
        ReputationRequire = 12
    }

    public enum SellResult
    {
        CantFindItem = 1,
        CantSellItem = 2,       // Merchant Doesn'T Like That Item
        CantFindVendor = 3,       // Merchant Doesn'T Like You
        YouDontOwnThatItem = 4,       // You Don'T Own That Item
        Unk = 5,       // Nothing Appears...
        OnlyEmptyBag = 6        // Can Only Do With Empty Bags
    }

    public enum EnchantmentSlot
    {
        PERM_ENCHANTMENT_SLOT = 0,
        TEMP_ENCHANTMENT_SLOT = 1,
        SOCK_ENCHANTMENT_SLOT = 2,
        SOCK_ENCHANTMENT_SLOT_2 = 3,
        SOCK_ENCHANTMENT_SLOT_3 = 4,
        BONUS_ENCHANTMENT_SLOT = 5,
        PRISMATIC_ENCHANTMENT_SLOT = 6,                    // added at apply special permanent enchantment
        //TODO: 7,
        REFORGE_ENCHANTMENT_SLOT = 8,
        TRANSMOGRIFY_ENCHANTMENT_SLOT = 9,
        MAX_INSPECTED_ENCHANTMENT_SLOT = 10,

        PROP_ENCHANTMENT_SLOT_0 = 10,                   // used with RandomSuffix
        PROP_ENCHANTMENT_SLOT_1 = 11,                   // used with RandomSuffix
        PROP_ENCHANTMENT_SLOT_2 = 12,                   // used with RandomSuffix and RandomProperty
        PROP_ENCHANTMENT_SLOT_3 = 13,                   // used with RandomProperty
        PROP_ENCHANTMENT_SLOT_4 = 14,                   // used with RandomProperty
        MAX_ENCHANTMENT_SLOT = 15
    }

    public enum ItemUpdateState
    {
        Unchanged = 0,
        Changed = 1,
        New = 2,
        Removed = 3
    }

    public enum ItemVendorType
    {
        Item = 1,
        Currency = 2,
    }

    public enum CurrencyFlags
    {
        Tradeable = 0x01,
        // ...
        HighPrecision = 0x08,
        // ...
    }

    public enum PlayerCurrencyState
    {
        Unchanged = 0,
        Changed = 1,
        New = 2,
        Removed = 3     //not removed just set count == 0
    }
}
