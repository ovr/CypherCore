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
    public struct UnitConst
    {
        public const float BASE_MINDAMAGE = 1.0f;
        public const float BASE_MAXDAMAGE = 2.0f;
        public const int BASE_ATTACK_TIME = 2000;
    }
    public enum WeaponDamageRange
    {
        MINDAMAGE,
        MAXDAMAGE
    }
    public enum UnitMods
    {
        STAT_STRENGTH,                                 // STAT_STRENGTH..STAT_SPIRIT must be in existed order, it's accessed by index values of Stats enum.
        STAT_AGILITY,
        STAT_STAMINA,
        STAT_INTELLECT,
        STAT_SPIRIT,
        HEALTH,
        MANA,                                          // MANA..RUNIC_POWER must be in existed order, it's accessed by index values of Powers enum.
        RAGE,
        FOCUS,
        ENERGY,
        UNUSED,                                        // Old HAPPINESS
        RUNE,
        RUNIC_POWER,
        SOUL_SHARDS,
        ECLIPSE,
        HOLY_POWER,
        ALTERNATIVE,
        ARMOR,                                         // ARMOR..RESISTANCE_ARCANE must be in existed order, it's accessed by index values of SpellSchools enum.
        RESISTANCE_HOLY,
        RESISTANCE_FIRE,
        RESISTANCE_NATURE,
        RESISTANCE_FROST,
        RESISTANCE_SHADOW,
        RESISTANCE_ARCANE,
        ATTACK_POWER,
        ATTACK_POWER_RANGED,
        DAMAGE_MAINHAND,
        DAMAGE_OFFHAND,
        DAMAGE_RANGED,
        END,
        // synonyms
        STAT_START = STAT_STRENGTH,
        STAT_END = STAT_SPIRIT + 1,
        RESISTANCE_START = ARMOR,
        RESISTANCE_END = RESISTANCE_ARCANE + 1,
        POWER_START = MANA,
        POWER_END = ALTERNATIVE + 1
    }
    public enum UnitModifierType
    {
        BASE_VALUE = 0,
        BASE_PCT = 1,
        TOTAL_VALUE = 2,
        TOTAL_PCT = 3,
        MODIFIER_TYPE_END = 4
    }
    public enum VictimState
    {
        INTACT = 0, // set when attacker misses
        HIT = 1, // victim got clear/blocked hit
        DODGE = 2,
        PARRY = 3,
        INTERRUPT = 4,
        BLOCKS = 5, // unused? not set when blocked, even on full block
        EVADES = 6,
        IS_IMMUNE = 7,
        DEFLECTS = 8
    }

    public enum HitInfo
    {
        NORMALSWING = 0x00000000,
        UNK1 = 0x00000001,               // req correct packet structure
        AFFECTS_VICTIM = 0x00000002,
        OFFHAND = 0x00000004,
        UNK2 = 0x00000008,
        MISS = 0x00000010,
        FULL_ABSORB = 0x00000020,
        PARTIAL_ABSORB = 0x00000040,
        FULL_RESIST = 0x00000080,
        PARTIAL_RESIST = 0x00000100,
        CRITICALHIT = 0x00000200,               // critical hit
        // 0x00000400
        // 0x00000800
        // 0x00001000
        BLOCK = 0x00002000,               // blocked damage
        // 0x00004000                                           // Hides worldtext for 0 damage
        // 0x00008000                                           // Related to blood visual
        GLANCING = 0x00010000,
        CRUSHING = 0x00020000,
        NO_ANIMATION = 0x00040000,
        // 0x00080000
        // 0x00100000
        SWINGNOHITSOUND = 0x00200000,               // unused?
        // 0x00400000
        RAGE_GAIN = 0x00800000
    }

    public enum ReactStates
    {
        PASSIVE = 0,
        DEFENSIVE = 1,
        AGGRESSIVE = 2
    }

    // high byte (3 from 0..3) of UNIT_FIELD_BYTES_2
    public enum ShapeshiftForm
    {
        None = 0x00,
        Cat = 0x01,
        Tree = 0x02,
        Travel = 0x03,
        Aqua = 0x04,
        Bear = 0x05,
        Ambient = 0x06,
        Ghoul = 0x07,
        Direbear = 0x08, // Removed In 4.0.1
        StevesGhoul = 0x09,
        TharonjaSkeleton = 0x0a,
        TestOfStrength = 0x0b,
        BlbPlayer = 0x0c,
        ShadowDance = 0x0d,
        Creaturebear = 0x0e,
        Creaturecat = 0x0f,
        Ghostwolf = 0x10,
        Battlestance = 0x11,
        Defensivestance = 0x12,
        Berserkerstance = 0x13,
        Test = 0x14,
        Zombie = 0x15,
        Metamorphosis = 0x16,
        Undead = 0x19,
        MasterAngler = 0x1a,
        FlightEpic = 0x1b,
        Shadow = 0x1c,
        Flight = 0x1d,
        Stealth = 0x1e,
        Moonkin = 0x1f,
        Spiritofredemption = 0x20
    };
    // byte (1 from 0..3) of UNIT_FIELD_BYTES_2
    public enum UnitPVPStateFlags
    {
        PVP = 0x01,
        Unk1 = 0x02,
        FFAPVP = 0x04,
        Sanctuary = 0x08,
        Unk4 = 0x10,
        Unk5 = 0x20,
        Unk6 = 0x40,
        Unk7 = 0x80
    }
    // byte (2 from 0..3) of UNIT_FIELD_BYTES_2
    public enum UnitRename
    {
        CanBeRenamed = 0x01,
        CanBeAbandoned = 0x02
    }

    // byte flag value (UNIT_FIELD_BYTES_1, 2)
    public enum UnitStandFlags
    {
        Unk1 = 0x01,
        Creep = 0x02,
        Untrackable = 0x04,
        Unk4 = 0x08,
        Unk5 = 0x10,
        All = 0xFF
    }


    public enum CombatRating
    {
        WeaponSkill = 0,
        DefenseSkill = 1, // Removed In 4.0.1
        Dodge = 2,
        Parry = 3,
        Block = 4,
        HitMelee = 5,
        HitRanged = 6,
        HitSpell = 7,
        CritMelee = 8,
        CritRanged = 9,
        CritSpell = 10,
        HitTakenMelee = 11,
        HitTakenRanged = 12,
        HitTakenSpell = 13,
        CritTakenMelee = 14,
        CritTakenRanged = 15,
        CritTakenSpell = 16,
        HasteMelee = 17,
        HasteRanged = 18,
        HasteSpell = 19,
        WeaponSkillMainhand = 20,
        WeaponSkillOffhand = 21,
        WeaponSkillRanged = 22,
        Expertise = 23,
        ArmorPenetration = 24,
        Max = 25
    }

    public enum DeathState
    {
        Alive = 0,
        JustDied = 1,
        Corpse = 2,
        Dead = 3,
        JustRespawned = 4
    }
    public enum UnitState : uint
    {
        Died = 0x00000001,                     // Player Has Fake Death Aura
        Melee_Attacking = 0x00000002,                     // Player Is Melee Attacking Someone
        //Melee_Attack_By = 0x00000004,                     // Player Is Melee Attack By Someone
        Stunned = 0x00000008,
        Roaming = 0x00000010,
        Chase = 0x00000020,
        //Searching       = 0x00000040,
        Fleeing = 0x00000080,
        In_Flight = 0x00000100,                     // Player Is In Flight Mode
        Follow = 0x00000200,
        Root = 0x00000400,
        Confused = 0x00000800,
        Distracted = 0x00001000,
        Isolated = 0x00002000,                     // Area Auras Do Not Affect Other Players
        Attack_Player = 0x00004000,
        Casting = 0x00008000,
        Possessed = 0x00010000,
        Charging = 0x00020000,
        Jumping = 0x00040000,
        Onvehicle = 0x00080000,
        Move = 0x00100000,
        Rotating = 0x00200000,
        Evade = 0x00400000,
        Roaming_Move = 0x00800000,
        Confused_Move = 0x01000000,
        Fleeing_Move = 0x02000000,
        Chase_Move = 0x04000000,
        Follow_Move = 0x08000000,
        Unattackable = (In_Flight | Onvehicle),
        // For Real Move Using Movegen Check And Stop (Except Unstoppable Flight)
        Moving = Roaming_Move | Confused_Move | Fleeing_Move | Chase_Move | Follow_Move,
        Controlled = (Confused | Stunned | Fleeing),
        Lost_Control = (Controlled | Jumping | Charging),
        Sightless = (Lost_Control | Evade),
        Cannot_Autoattack = (Lost_Control | Casting),
        Cannot_Turn = (Lost_Control | Rotating),
        // Stay By Different Reasons
        Not_Move = Root | Stunned | Died | Distracted,
        All_State = 0xffffffff                      //(Stopped | Moving | In_Combat | In_Flight)
    }

    public enum UnitMoveType
    {
        Walk = 0,
        Run = 1,
        RunBack = 2,
        Swim = 3,
        SwimBack = 4,
        TurnRate = 5,
        Flight = 6,
        FlightBack = 7,
        PitchRate = 8
    }
    public enum WeaponAttackType
    {
        BaseAttack = 0,
        OffAttack = 1,
        RangedAttack = 2,
        MaxAttack
    }

    public enum UnitTypeMask
    {
        None = 0x00000000,
        Summon = 0x00000001,
        Minion = 0x00000002,
        Guardian = 0x00000004,
        Totem = 0x00000008,
        Pet = 0x00000010,
        Vehicle = 0x00000020,
        Puppet = 0x00000040,
        HunterPet = 0x00000080,
        ControlableGuardian = 0x00000100,
        Accessory = 0x00000200
    }

    public enum UnitClass
    {
        Warrior = 1,
        Paladin = 2,
        Rogue = 4,
        Mage = 8
    }
}
