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
    public static class CreatureConst
    {
        public const int MaxBaseHp = 4;
        public const int MaxQuestItems = 6;
        public const int MaxEquipmentItems = 3;
        public const int MaxSpells = 8;
        public const int MaxVendorItems = 300;
        public const int AttackRangeZ = 3;
    }

    public enum CreatureEliteType
    {
        NORMAL = 0,
        ELITE = 1,
        RAREELITE = 2,
        WORLDBOSS = 3,
        RARE = 4,
        UNKNOWN = 5                      // found in 2.2.3 for 2 mobs
    }

    public enum UnitFlags : uint
    {
        ServerControlled = 0x00000001,
        NonAttackable = 0x00000002,
        DisableMove = 0x00000004,
        PvpAttackable = 0x00000008,
        Rename = 0x00000010,
        Preparation = 0x00000020,
        Unk6 = 0x00000040,
        NotAttackable1 = 0x00000080,
        ImmuneToPc = 0x00000100,
        ImmuneToNpc = 0x00000200,
        Looting = 0x00000400,
        PetInCombat = 0x00000800,
        Pvp = 0x00001000,
        Silenced = 0x00002000,
        Unk14 = 0x00004000,
        Unk15 = 0x00008000,
        Unk16 = 0x00010000,
        Pacified = 0x00020000,
        Stunned = 0x00040000,
        InCombat = 0x00080000,
        TaxiFlight = 0x00100000,
        Disarmed = 0x00200000,
        Confused = 0x00400000,
        Fleeing = 0x00800000,
        PlayerControlled = 0x01000000,
        NotSelectable = 0x02000000,
        Skinnable = 0x04000000,
        Mount = 0x08000000,
        Unk28 = 0x10000000,
        Unk29 = 0x20000000,
        Sheathe = 0x40000000,
        Unk31 = 0x80000000
    }

    public enum UnitFlags2
    {
        FeignDeath = 0x00000001,
        Unk1 = 0x00000002,
        IgnoreReputation = 0x00000004,
        ComprehendLang = 0x00000008,
        MirrorImage = 0x00000010,
        ForceMove = 0x00000040,
        DisarmOffhand = 0x00000080,
        DisarmRanged = 0x00000400,
        RegeneratePower = 0x00000800,
        AllowEnemyInteract = 0x00004000,
        AllowCheatSpells = 0x00040000,
    }

    public enum NPCFlags
    {
        None = 0x00000000,
        Gossip = 0x00000001,
        QuestGiver = 0x00000002,
        Unk1 = 0x00000004,
        Unk2 = 0x00000008,
        Trainer = 0x00000010,
        TrainerClass = 0x00000020,
        TrainerProfession = 0x00000040,
        Vendor = 0x00000080,
        VendorGeneral = 0x00000100,
        VendorFood = 0x00000200,
        VendorPoison = 0x00000400,
        VendorReagent = 0x00000800,
        Repair = 0x00001000,
        FlightMaster = 0x00002000,
        SpiritHealer = 0x00004000,
        SpiritGuide = 0x00008000,
        Innkeeper = 0x00010000,
        Banker = 0x00020000,
        Petitioner = 0x00040000,
        TabardDesigner = 0x00080000,
        BattleMaster = 0x00100000,
        Auctioneer = 0x00200000,
        StableMaster = 0x00400000,
        GuildBanker = 0x00800000,
        SpellClick = 0x01000000,
        PlayerVehicle = 0x02000000,
        Reforger = 0x08000000,
        Transmogrifier = 0x10000000,
        VaultKeeper = 0x20000000,
    }

    public enum CreatureTypeFlags : uint
    {
        CREATURE_TYPEFLAGS_TAMEABLE = 0x00000001,         // Tameable by any hunter
        CREATURE_TYPEFLAGS_GHOST = 0x00000002,         // Creature are also visible for not alive player. Allow gossip interaction if npcflag allow?
        CREATURE_TYPEFLAGS_BOSS = 0x00000004,
        CREATURE_TYPEFLAGS_UNK3 = 0x00000008,
        CREATURE_TYPEFLAGS_UNK4 = 0x00000010,
        CREATURE_TYPEFLAGS_UNK5 = 0x00000020,
        CREATURE_TYPEFLAGS_UNK6 = 0x00000040,
        CREATURE_TYPEFLAGS_DEAD_INTERACT = 0x00000080,         // Player can interact with the creature if its dead (not player dead)
        CREATURE_TYPEFLAGS_HERBLOOT = 0x00000100,         // Can be looted by herbalist
        CREATURE_TYPEFLAGS_MININGLOOT = 0x00000200,         // Can be looted by miner
        CREATURE_TYPEFLAGS_UNK10 = 0x00000400,
        CREATURE_TYPEFLAGS_MOUNTED_COMBAT = 0x00000800,         // Creature can remain mounted when entering combat
        CREATURE_TYPEFLAGS_AID_PLAYERS = 0x00001000,         // ? Can aid any player in combat if in range?
        CREATURE_TYPEFLAGS_UNK13 = 0x00002000,
        CREATURE_TYPEFLAGS_UNK14 = 0x00004000,         // ? Possibly not in use
        CREATURE_TYPEFLAGS_ENGINEERLOOT = 0x00008000,         // Can be looted by engineer
        CREATURE_TYPEFLAGS_EXOTIC = 0x00010000,         // Can be tamed by hunter as exotic pet
        CREATURE_TYPEFLAGS_UNK17 = 0x00020000,         // ? Related to vehicles/pvp?
        CREATURE_TYPEFLAGS_UNK18 = 0x00040000,         // ? Related to vehicle/siege weapons?
        CREATURE_TYPEFLAGS_UNK19 = 0x00080000,
        CREATURE_TYPEFLAGS_UNK20 = 0x00100000,
        CREATURE_TYPEFLAGS_UNK21 = 0x00200000,
        CREATURE_TYPEFLAGS_UNK22 = 0x00400000,
        CREATURE_TYPEFLAGS_UNK23 = 0x00800000,         // ? First seen in 3.2.2. Related to banner/backpack of creature/companion?
        CREATURE_TYPEFLAGS_UNK24 = 0x01000000,
        CREATURE_TYPEFLAGS_UNK25 = 0x02000000,
        CREATURE_TYPEFLAGS_PARTY_MEMBER = 0x04000000,         //! Creature can be targeted by spells that require target to be in caster's party/raid
        CREATURE_TYPEFLAGS_UNK27 = 0x08000000,
        CREATURE_TYPEFLAGS_UNK28 = 0x10000000,
        CREATURE_TYPEFLAGS_UNK29 = 0x20000000,
        CREATURE_TYPEFLAGS_UNK30 = 0x40000000,
        CREATURE_TYPEFLAGS_UNK31 = 0x80000000
    }

    public enum CreatureFlagsExtra
    {
        InstanceBind = 0x00000001,       // Creature Kill Bind Instance With Killer And Killer'S Group
        Civilian = 0x00000002,       // Not Aggro (Ignore Faction/Reputation Hostility)
        NoParry = 0x00000004,       // Creature Can'T Parry
        NoParryHasten = 0x00000008,       // Creature Can'T Counter-Attack At Parry
        NoBlock = 0x00000010,       // Creature Can'T Block
        NoCrush = 0x00000020,       // Creature Can'T Do Crush Attacks
        NoXpAtKill = 0x00000040,       // Creature Kill Not Provide Xp
        Trigger = 0x00000080,       // Trigger Creature
        NoTaunt = 0x00000100,       // Creature Is Immune To Taunt Auras And Effect Attack Me
        Worldevent = 0x00004000,       // Custom Flag For World Event Creatures (Left Room For Merging)
        Guard = 0x00008000,       // Creature Is Guard
        NoCrit = 0x00020000,       // Creature Can'T Do Critical Strikes
        NoSkillgain = 0x00040000,       // Creature Won'T Increase Weapon Skills
        TauntDiminish = 0x00080000,       // Taunt Is A Subject To Diminishing Returns On This Creautre
        AllDiminish = 0x00100000,       // Creature Is Subject To All Diminishing Returns As Player Are
        DungeonBoss = 0x10000000        // Creature Is A Dungeon Boss (Set Dynamically, Do Not Add In Db)
    }

    public enum CreatureType
    {
        Beast = 1,
        Dragonkin = 2,
        Demon = 3,
        Elemental = 4,
        Giant = 5,
        Undead = 6,
        Humanoid = 7,
        Critter = 8,
        Mechanical = 9,
        NotSpecified = 10,
        Totem = 11,
        NonCombatPet = 12,
        GasCloud = 13
    }

    public enum InhabitType
    {
        Ground = 1,
        Water = 2,
        Air = 4,
        Anywhere = Ground | Water | Air
    }

}
