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
    class SpellConst
    {
    }

    public enum ProcFlags
    {
        NONE = 0x00000000,

        KILLED = 0x00000001,    // 00 Killed by agressor - not sure about this flag
        KILL = 0x00000002,    // 01 Kill target (in most cases need XP/Honor reward)

        DONE_MELEE_AUTO_ATTACK = 0x00000004,    // 02 Done melee auto attack
        TAKEN_MELEE_AUTO_ATTACK = 0x00000008,    // 03 Taken melee auto attack

        DONE_SPELL_MELEE_DMG_CLASS = 0x00000010,    // 04 Done attack by Spell that has dmg class melee
        TAKEN_SPELL_MELEE_DMG_CLASS = 0x00000020,    // 05 Taken attack by Spell that has dmg class melee

        DONE_RANGED_AUTO_ATTACK = 0x00000040,    // 06 Done ranged auto attack
        TAKEN_RANGED_AUTO_ATTACK = 0x00000080,    // 07 Taken ranged auto attack

        DONE_SPELL_RANGED_DMG_CLASS = 0x00000100,    // 08 Done attack by Spell that has dmg class ranged
        TAKEN_SPELL_RANGED_DMG_CLASS = 0x00000200,    // 09 Taken attack by Spell that has dmg class ranged

        DONE_SPELL_NONE_DMG_CLASS_POS = 0x00000400,    // 10 Done positive spell that has dmg class none
        TAKEN_SPELL_NONE_DMG_CLASS_POS = 0x00000800,    // 11 Taken positive spell that has dmg class none

        DONE_SPELL_NONE_DMG_CLASS_NEG = 0x00001000,    // 12 Done negative spell that has dmg class none
        TAKEN_SPELL_NONE_DMG_CLASS_NEG = 0x00002000,    // 13 Taken negative spell that has dmg class none

        DONE_SPELL_MAGIC_DMG_CLASS_POS = 0x00004000,    // 14 Done positive spell that has dmg class magic
        TAKEN_SPELL_MAGIC_DMG_CLASS_POS = 0x00008000,    // 15 Taken positive spell that has dmg class magic

        DONE_SPELL_MAGIC_DMG_CLASS_NEG = 0x00010000,    // 16 Done negative spell that has dmg class magic
        TAKEN_SPELL_MAGIC_DMG_CLASS_NEG = 0x00020000,    // 17 Taken negative spell that has dmg class magic

        DONE_PERIODIC = 0x00040000,    // 18 Successful do periodic (damage / healing)
        TAKEN_PERIODIC = 0x00080000,    // 19 Taken spell periodic (damage / healing)

        TAKEN_DAMAGE = 0x00100000,    // 20 Taken any damage
        DONE_TRAP_ACTIVATION = 0x00200000,    // 21 On trap activation (possibly needs name change to ON_GAMEOBJECT_CAST or USE)

        DONE_MAINHAND_ATTACK = 0x00400000,    // 22 Done main-hand melee attacks (spell and autoattack)
        DONE_OFFHAND_ATTACK = 0x00800000,    // 23 Done off-hand melee attacks (spell and autoattack)

        DEATH = 0x01000000,    // 24 Died in any way

        // flag masks
        AUTO_ATTACK_MASK = DONE_MELEE_AUTO_ATTACK | TAKEN_MELEE_AUTO_ATTACK
                                                    | DONE_RANGED_AUTO_ATTACK | TAKEN_RANGED_AUTO_ATTACK,

        MELEE_MASK = DONE_MELEE_AUTO_ATTACK | TAKEN_MELEE_AUTO_ATTACK
                                                    | DONE_SPELL_MELEE_DMG_CLASS | TAKEN_SPELL_MELEE_DMG_CLASS
                                                    | DONE_MAINHAND_ATTACK | DONE_OFFHAND_ATTACK,

        RANGED_MASK = DONE_RANGED_AUTO_ATTACK | TAKEN_RANGED_AUTO_ATTACK
                                                    | DONE_SPELL_RANGED_DMG_CLASS | TAKEN_SPELL_RANGED_DMG_CLASS,

        SPELL_MASK = DONE_SPELL_MELEE_DMG_CLASS | TAKEN_SPELL_MELEE_DMG_CLASS
                                                    | DONE_SPELL_RANGED_DMG_CLASS | TAKEN_SPELL_RANGED_DMG_CLASS
                                                    | DONE_SPELL_NONE_DMG_CLASS_POS | TAKEN_SPELL_NONE_DMG_CLASS_POS
                                                    | DONE_SPELL_NONE_DMG_CLASS_NEG | TAKEN_SPELL_NONE_DMG_CLASS_NEG
                                                    | DONE_SPELL_MAGIC_DMG_CLASS_POS | TAKEN_SPELL_MAGIC_DMG_CLASS_POS
                                                    | DONE_SPELL_MAGIC_DMG_CLASS_NEG | TAKEN_SPELL_MAGIC_DMG_CLASS_NEG,

        SPELL_CAST_MASK = SPELL_MASK | DONE_TRAP_ACTIVATION | RANGED_MASK,

        PERIODIC_MASK = DONE_PERIODIC | TAKEN_PERIODIC,

        DONE_HIT_MASK = DONE_MELEE_AUTO_ATTACK | DONE_RANGED_AUTO_ATTACK
                                                     | DONE_SPELL_MELEE_DMG_CLASS | DONE_SPELL_RANGED_DMG_CLASS
                                                     | DONE_SPELL_NONE_DMG_CLASS_POS | DONE_SPELL_NONE_DMG_CLASS_NEG
                                                     | DONE_SPELL_MAGIC_DMG_CLASS_POS | DONE_SPELL_MAGIC_DMG_CLASS_NEG
                                                     | DONE_PERIODIC | DONE_MAINHAND_ATTACK | DONE_OFFHAND_ATTACK,

        TAKEN_HIT_MASK = TAKEN_MELEE_AUTO_ATTACK | TAKEN_RANGED_AUTO_ATTACK
                                                     | TAKEN_SPELL_MELEE_DMG_CLASS | TAKEN_SPELL_RANGED_DMG_CLASS
                                                     | TAKEN_SPELL_NONE_DMG_CLASS_POS | TAKEN_SPELL_NONE_DMG_CLASS_NEG
                                                     | TAKEN_SPELL_MAGIC_DMG_CLASS_POS | TAKEN_SPELL_MAGIC_DMG_CLASS_NEG
                                                     | TAKEN_PERIODIC | TAKEN_DAMAGE,

        REQ_SPELL_PHASE_MASK = SPELL_MASK & DONE_HIT_MASK
    }
    public enum ProcFlagsExLegacy
    {
        NONE = 0x0000000,                 // If none can tigger on Hit/Crit only (passive spells MUST defined by SpellFamily flag)
        NORMAL_HIT = 0x0000001,                 // If set only from normal hit (only damage spells)
        CRITICAL_HIT = 0x0000002,
        MISS = 0x0000004,
        RESIST = 0x0000008,
        DODGE = 0x0000010,
        PARRY = 0x0000020,
        BLOCK = 0x0000040,
        EVADE = 0x0000080,
        IMMUNE = 0x0000100,
        DEFLECT = 0x0000200,
        ABSORB = 0x0000400,
        REFLECT = 0x0000800,
        INTERRUPT = 0x0001000,                 // Melee hit result can be Interrupt (not used)
        FULL_BLOCK = 0x0002000,                 // block al attack damage
        RESERVED2 = 0x0004000,
        NOT_ACTIVE_SPELL = 0x0008000,                 // Spell mustn't do damage/heal to proc
        EX_TRIGGER_ALWAYS = 0x0010000,                 // If set trigger always no matter of hit result
        EX_ONE_TIME_TRIGGER = 0x0020000,                 // If set trigger always but only one time (not implemented yet)
        ONLY_ACTIVE_SPELL = 0x0040000,                 // Spell has to do damage/heal to proc

        // Flags for internal use - do not use these in db!
        INTERNAL_CANT_PROC = 0x0800000,
        INTERNAL_DOT = 0x1000000,
        INTERNAL_HOT = 0x2000000,
        INTERNAL_TRIGGERED = 0x4000000,
        INTERNAL_REQ_FAMILY = 0x8000000
    }
    // Spell aura states
    public enum AuraStateType
    {   // (C) used in caster aura state     (T) used in target aura state
        // (c) used in caster aura state-not (t) used in target aura state-not
        NONE = 0,            // C   |
        DEFENSE = 1,            // C   |
        HEALTHLESS_20_PERCENT = 2,            // CcT |
        BERSERKING = 3,            // C T |
        FROZEN = 4,            //  c t| frozen target
        JUDGEMENT = 5,            // C   |
        //UNKNOWN6                   = 6,            //     | not used
        HUNTER_PARRY = 7,            // C   |
        //UNKNOWN7                   = 7,            //  c  | creature cheap shot / focused bursts spells
        //UNKNOWN8                   = 8,            //    t| test spells
        //UNKNOWN9                   = 9,            //     |
        WARRIOR_VICTORY_RUSH = 10,           // C   | warrior victory rush
        //UNKNOWN11                  = 11,           // C  t| 60348 - Maelstrom Ready!, test spells
        FAERIE_FIRE = 12,           //  c t|
        HEALTHLESS_35_PERCENT = 13,           // C T |
        CONFLAGRATE = 14,           //   T |
        SWIFTMEND = 15,           //   T |
        DEADLY_POISON = 16,           //   T |
        ENRAGE = 17,           // C   |
        BLEEDING = 18,           //    T|
        UNKNOWN19 = 19,           //     |
        //UNKNOWN20                  = 20,           //  c  | only (45317 Suicide)
        //UNKNOWN21                  = 21,           //     | not used
        UNKNOWN22 = 22,           // C  t| varius spells (63884, 50240)
        HEALTH_ABOVE_75_PERCENT = 23,            // C   |

        PER_CASTER_AURA_STATE_MASK = (1<<(CONFLAGRATE-1))|(1<<(DEADLY_POISON-1))
    }

}
