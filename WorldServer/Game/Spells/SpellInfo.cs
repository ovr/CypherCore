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
using Framework.DataStorage;
using Framework.Utility;
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;

namespace WorldServer.Game.Spells
{
    public class SpellInfo : Cypher
    {
        public SpellInfo(SpellEntry spellEntry, SpellEffectEntry[] effects)
        {
            Id = spellEntry.Id;

            SpellMisc misc = spellEntry.Misc;
            if (misc != null)
            {
                Attributes = (SpellAttr0)misc.Attributes;
                AttributesEx = (SpellAttr1)misc.AttributesEx;
                AttributesEx2 = (SpellAttr2)misc.AttributesEx2;
                AttributesEx3 = (SpellAttr3)misc.AttributesEx3;
                AttributesEx4 = (SpellAttr4)misc.AttributesEx4;
                AttributesEx5 = (SpellAttr5)misc.AttributesEx5;
                AttributesEx6 = (SpellAttr6)misc.AttributesEx6;
                AttributesEx7 = (SpellAttr7)misc.AttributesEx7;
                AttributesEx8 = (SpellAttr8)misc.AttributesEx8;
                AttributesEx9 = (SpellAttr9)misc.AttributesEx9;
                AttributesEx10 = (SpellAttr10)misc.AttributesEx10;
            Speed = spellEntry.Misc.Speed;
            for (var i = 0; i < 2; ++i)
                SpellVisual[i] = misc.SpellVisual[i];

            CastTimeEntry = DBCStorage.SpellCastTimesStorage.LookupByKey(misc.CastingTimeIndex);
            DurationEntry = DBCStorage.SpellDurationStorage.LookupByKey(misc.DurationIndex);
            //PowerType = (Powers)misc.PowerType;
            RangeEntry = DBCStorage.SpellRangeStorage.LookupByKey(misc.RangeIndex);

            SpellIconID = misc.SpellIconID;
            ActiveIconID = misc.ActiveIconID;
            SchoolMask = misc.SchoolMask;


            }
            AttributesCu = 0;

            SpellName = spellEntry.Name;
            Rank = spellEntry.Rank;

            RuneCostID = spellEntry.RuneCostID;
            //SpellDifficultyId = spellEntry.SpellDifficultyId;
            SpellScalingId = spellEntry.SpellScalingId;
            SpellAuraOptionsId = spellEntry.SpellAuraOptionsId;
            SpellAuraRestrictionsId = spellEntry.SpellAuraRestrictionsId;
            SpellCastingRequirementsId = spellEntry.SpellCastingRequirementsId;
            SpellCategoriesId = spellEntry.SpellCategoriesId;
            SpellClassOptionsId = spellEntry.SpellClassOptionsId;
            SpellCooldownsId = spellEntry.SpellCooldownsId;
            SpellEquippedItemsId = spellEntry.SpellEquippedItemsId;
            SpellInterruptsId = spellEntry.SpellInterruptsId;
            SpellLevelsId = spellEntry.SpellLevelsId;
            SpellPowerId = spellEntry.SpellPowerId;
            SpellReagentsId = spellEntry.SpellReagentsId;
            SpellShapeshiftId = spellEntry.SpellShapeshiftId;
            SpellTargetRestrictionsId = spellEntry.SpellTargetRestrictionsId;
            SpellTotemsId = spellEntry.SpellTotemsId;

            // SpellDifficultyEntry
            if (effects != null)
            {
                for (uint i = 0; i < effects.Length; i++)
                {
                    if (effects[i] == null)
                        continue;
                    Effects[i] = new SpellEffectInfo(this, i, effects[i]);
                }
            }

            // SpellScalingEntry
            SpellScalingEntry _scaling = DBCStorage.SpellScalingStorage.LookupByKey(SpellScalingId);
            if (_scaling != null)
            {
                CastTimeMin = _scaling.CastTimeMin;
                CastTimeMax = _scaling.CastTimeMax;
                CastTimeMaxLevel = _scaling.CastTimeMaxLevel;
                ScalingClass = _scaling.ScalingClass;
                //CoefBase = _scaling.CoefBase;
                //CoefLevelBase = _scaling.CoefLevelBase;
            }

            // SpellAuraOptionsEntry
            SpellAuraOptionsEntry _options = DBCStorage.SpellAuraOptionsStorage.LookupByKey(SpellAuraOptionsId);
            if (_options != null)
            {
                ProcFlags = _options.procFlags;
                ProcChance = _options.procChance;
                ProcCharges = _options.procCharges;
                StackAmount = _options.StackAmount;
            }

            // SpellAuraRestrictionsEntry
            SpellAuraRestrictionsEntry _aura = DBCStorage.SpellAuraRestrictionsStorage.LookupByKey(SpellAuraRestrictionsId);
            if (_aura != null)
            {
                CasterAuraState = _aura.CasterAuraState;
                TargetAuraState = _aura.TargetAuraState;
                CasterAuraStateNot = _aura.CasterAuraStateNot;
                TargetAuraStateNot = _aura.TargetAuraStateNot;
                CasterAuraSpell = _aura.casterAuraSpell;
                TargetAuraSpell = _aura.targetAuraSpell;
                ExcludeCasterAuraSpell = _aura.excludeCasterAuraSpell;
                ExcludeTargetAuraSpell = _aura.excludeTargetAuraSpell;
            }

            // SpellCastingRequirementsEntry
            SpellCastingRequirementsEntry _castreq = DBCStorage.SpellCastingRequirementsStorage.LookupByKey(SpellCastingRequirementsId);
            if (_castreq != null)
            {
                RequiresSpellFocus = _castreq.RequiresSpellFocus;
                FacingCasterFlags = _castreq.FacingCasterFlags;
                AreaGroupId = _castreq.AreaGroupId != 0 ? _castreq.AreaGroupId : -1;
            }

            // SpellCategoriesEntry
            SpellCategoriesEntry _categorie = DBCStorage.SpellCategoriesStorage.LookupByKey(SpellCategoriesId);
            if (_categorie != null)
            {
                Category = _categorie.Category;
                Dispel = _categorie.Dispel;
                Mechanic = _categorie.Mechanic;
                StartRecoveryCategory = _categorie.StartRecoveryCategory;
                DmgClass = (SpellDmgClass)_categorie.DmgClass;
                PreventionType = _categorie.PreventionType;
            }

            // SpellClassOptionsEntry
            SpellClassOptionsEntry _class = DBCStorage.SpellClassOptionsStorage.LookupByKey(SpellClassOptionsId);
            if (_class != null)
            {
                SpellFamilyName = _class.SpellFamilyName;
                SpellFamilyFlags = _class.SpellFamilyFlags;
            }

            // SpellCooldownsEntry
            SpellCooldownsEntry _cooldowns = DBCStorage.SpellCooldownsStorage.LookupByKey(SpellCooldownsId);
            if (_cooldowns != null)
            {
                RecoveryTime = _cooldowns.RecoveryTime;
                CategoryRecoveryTime = _cooldowns.CategoryRecoveryTime;
                StartRecoveryTime = _cooldowns.StartRecoveryTime;
            }

            // SpellEquippedItemsEntry
            SpellEquippedItemsEntry _equipped = DBCStorage.SpellEquippedItemsStorage.LookupByKey(SpellEquippedItemsId);
            if (_equipped != null)
            {
                EquippedItemClass = _equipped.EquippedItemClass;
                EquippedItemSubClassMask = _equipped.EquippedItemSubClassMask;
                EquippedItemInventoryTypeMask = _equipped.EquippedItemInventoryTypeMask; 
            }
            else
            {
                EquippedItemClass = -1;
                EquippedItemSubClassMask = -1;
                EquippedItemInventoryTypeMask = -1;
            }

            // SpellInterruptsEntry
            SpellInterruptsEntry _interrupt = DBCStorage.SpellInterruptsEntryStorage.LookupByKey(SpellInterruptsId);
            if (_interrupt != null)
            {
                InterruptFlags = _interrupt.InterruptFlags;
                AuraInterruptFlags = _interrupt.AuraInterruptFlags;
                ChannelInterruptFlags = _interrupt.ChannelInterruptFlags;
            }

            // SpellLevelsEntry
            SpellLevelsEntry _levels = DBCStorage.SpellLevelsStorage.LookupByKey(SpellLevelsId);
            if (_levels != null)
            {
                MaxLevel = _levels.maxLevel;
                BaseLevel = _levels.baseLevel;
                SpellLevel = _levels.spellLevel;
            }

            // SpellPowerEntry
            SpellPowerEntry _power = DBCStorage.SpellPowerStorage.LookupByKey(SpellPowerId);
            if (_power != null)
            {
                ManaCost = _power.manaCost;
                ManaCostPerlevel = _power.manaCostPerlevel;
                ManaCostPercentage = _power.ManaCostPercentage;
                ManaPerSecond = _power.manaPerSecond;
            }

            // SpellReagentsEntry
            SpellReagentsEntry _reagents = DBCStorage.SpellReagentsStorage.LookupByKey(SpellReagentsId);
            for (var i = 0; i < SharedConst.MaxSpellReagents; ++i)
            {
                Reagent[i] = _reagents != null ? _reagents.Reagent[i] : 0;
                ReagentCount[i] = _reagents != null ? _reagents.ReagentCount[i] : 0;
            }

            // SpellShapeshiftEntry
            SpellShapeshiftEntry _shapeshift = DBCStorage.SpellShapeshiftStorage.LookupByKey(SpellShapeshiftId);
            if (_shapeshift != null)
            {
                Stances = _shapeshift.Stances;
                StancesNot = _shapeshift.StancesNot;
            }

            // SpellTargetRestrictionsEntry
            SpellTargetRestrictionsEntry _target = DBCStorage.SpellTargetRestrictionsStorage.LookupByKey(SpellTargetRestrictionsId);
            if (_target != null)
            {
                Targets = _target.Targets;
                TargetCreatureType = _target.TargetCreatureType;
                MaxAffectedTargets = _target.MaxAffectedTargets;
            }

            // SpellTotemsEntry
            SpellTotemsEntry _totem = DBCStorage.SpellTotemsStorage.LookupByKey(SpellTotemsId);
            for (var i = 0; i < 2; ++i)
            {
                TotemCategory[i] = _totem != null ? _totem.TotemCategory[i] : 0;
                Totem[i] = _totem != null ? _totem.Totem[i] : 0;
            }

            //ExplicitTargetMask = _GetExplicitTargetMask();
            ChainEntry = null;
        }

        public bool IsProfession()
        {
            for (byte i = 0; i < SharedConst.MaxSpellEffects; ++i)
            {
                if (Effects[i] == null)
                    continue;

                if (Effects[i].Effect == (uint)SpellEffects.Skill)
                {
                    uint skill = (uint)Effects[i].MiscValue;

                    if (SpellMgr.IsProfessionSkill(skill))
                        return true;
                }
            }
            return false;
        }
        public bool IsPrimaryProfession()
        {
            for (var i = 0; i < SharedConst.MaxSpellEffects; ++i)
            {
                if (Effects[i] != null && Effects[i].Effect == (uint)SpellEffects.Skill)
                {
                    uint skill = (uint)Effects[i].MiscValue;

                    if (SpellMgr.IsPrimaryProfessionSkill(skill))
                        return true;
                }
            }
            return false;
        }
        public bool IsPrimaryProfessionFirstRank()
        {
            return IsPrimaryProfession() && GetRank() == 1;
        }
        byte GetRank()
        {
            if (ChainEntry.rank == 0)
                return 1;
            return ChainEntry.rank;
        }
        public bool IsLootCrafting()
        {
            return (Effects[0].Effect == (uint)SpellEffects.CreateRandomItem ||
                // different random cards from Inscription (121==Virtuoso Inking Set category) r without explicit item
                (Effects[0].Effect == (uint)SpellEffects.CreateItem2 &&
                (TotemCategory[0] != 0 || Effects[0].ItemType == 0)));
        }
        public bool IsPassive()
        {
            return Convert.ToBoolean(Attributes & SpellAttr0.Passive);
        }
        public SpellInfo GetFirstRankSpell()
        {
            if (ChainEntry == null)
                return this;
            return SpellMgr.GetSpellInfo(ChainEntry.first);
        }
        public bool HasEffect(SpellEffects effect)
        {
            for (var i = 0; i < SharedConst.MaxSpellEffects; ++i)
            {
                if (Effects[i] == null)
                    continue;

                if (Effects[i].IsEffect(effect))
                    return true;
            }
            return false;
        }
        public bool IsHighRankOf(SpellInfo spellInfo)
        {
            if (ChainEntry != null && spellInfo.ChainEntry != null)
            {
                if (ChainEntry.first == spellInfo.ChainEntry.first)
                    if (ChainEntry.rank > spellInfo.ChainEntry.rank)
                        return true;
            }
            return false;
        }
        public bool IsDifferentRankOf(SpellInfo spellInfo)
        {
            if (Id == spellInfo.Id)
                return false;
            return IsRankOf(spellInfo);
        }
        bool IsRankOf(SpellInfo spellInfo)
        {
            return GetFirstRankSpell() == spellInfo.GetFirstRankSpell();
        }
        bool IsProfessionOrRiding()
        {
            for (var i = 0; i < SharedConst.MaxSpellEffects; ++i)
            {
                if (Effects[i] == null)
                    continue;
                if (Effects[i].Effect == (uint)SpellEffects.Skill)
                {
                    uint skill = (uint)Effects[i].MiscValue;

                    if (SpellMgr.IsProfessionOrRidingSkill(skill))
                        return true;
                }
            }
            return false;
        }
        bool IsAbilityLearnedWithProfession()
        {
            var bounds = SpellMgr.GetSkillLineAbility(Id);

            foreach (var pAbility in bounds)
            {
                if (pAbility.id == 0 || pAbility.learnOnGetSkill != 1)
                    continue;

                if (pAbility.req_skill_value > 0)
                    return true;
            }

            return false;
        }
        public bool IsStackableWithRanks()
        {
            if (IsPassive())
                return false;
            if (PowerType != Powers.Mana && PowerType != Powers.Health)
                return false;
            if (IsProfessionOrRiding())
                return false;

            if (IsAbilityLearnedWithProfession())
                return false;

            // All stance spells. if any better way, change it.
            for (var i = 0; i < SharedConst.MaxSpellEffects; ++i)
            {
                if (Effects[i] == null)
                    continue;
                switch ((SpellFamilyNames)SpellFamilyName)
                {
                    case SpellFamilyNames.Paladin:
                    // Paladin aura Spell
                    if (Effects[i].Effect == (uint)SpellEffects.ApplyAreaAuraRaid)
                        return false;
                        break;
                    case SpellFamilyNames.Druid:
                    // Druid form Spell
                    if (Effects[i].Effect == (uint)SpellEffects.ApplyAura &&
                        Effects[i].ApplyAuraName == (uint)AuraType.ModShapeshift)
                        return false;
                        break;
                }
            }
            return true;
        }
        public bool IsRanked()
        {
            return ChainEntry != null;
        }
        public bool IsPositiveEffect(uint effIndex)
        {
            switch (effIndex)
            {
                default:
                case 0:
                    return !Convert.ToBoolean(AttributesCu & SpellCustomAttributes.SPELL_ATTR0_CU_NEGATIVE_EFF0);
                case 1:
                    return !Convert.ToBoolean(AttributesCu & SpellCustomAttributes.SPELL_ATTR0_CU_NEGATIVE_EFF1);
                case 2:
                    return !Convert.ToBoolean(AttributesCu & SpellCustomAttributes.SPELL_ATTR0_CU_NEGATIVE_EFF2);
            }
        }
        public SpellScalingEntry GetSpellScaling()
        {
            return SpellScalingId != 0 ? DBCStorage.SpellScalingStorage.LookupByKey(SpellScalingId) : default(SpellScalingEntry);
        }
        public bool IsPositive()
        {
            return !Convert.ToBoolean(AttributesCu & SpellCustomAttributes.SPELL_ATTR0_CU_NEGATIVE);
        }
        public bool IsRangedWeaponSpell()
        {
            return (SpellFamilyName == (uint)SpellFamilyNames.Hunter && !Convert.ToBoolean(SpellFamilyFlags[1] & 0x10000000)); // for 53352, cannot find better way
                //|| (EquippedItemSubClassMask & ITEM_SUBCLASS_MASK_WEAPON_RANGED);
        }
        public SpellSchoolMask GetSchoolMask()
        {
            return (SpellSchoolMask)SchoolMask;
        }
        public bool IsAutoRepeatRangedSpell()
        {
            return Convert.ToBoolean(AttributesEx2 & SpellAttr2.AutorepeatFlag);
        }
        public int CalcCastTime(Unit caster, Spell spell)
        {
            int castTime = 0;

            // not all spells have cast time index and this is all is pasiive abilities
            if (caster != null && CastTimeMax > 0)
            {
                castTime = CastTimeMax;
                if (CastTimeMaxLevel > (int)caster.getLevel())
                    castTime = CastTimeMin + (int)caster.getLevel() - 1 * (CastTimeMax - CastTimeMin) / (CastTimeMaxLevel - 1);
            }
            else if (CastTimeEntry.ID != 0)
                castTime = CastTimeEntry.CastTime;

            if (castTime == 0)
                return 0;

            if (caster != null)
                caster.ModSpellCastTime(this, castTime, spell);

            if (Convert.ToBoolean(Attributes & SpellAttr0.ReqAmmo) && (!IsAutoRepeatRangedSpell()))
                castTime += 500;

            return (castTime > 0) ? castTime : 0;
        }
        public bool IsChanneled()
        {
            return Convert.ToBoolean(AttributesEx & (SpellAttr1.Channeled1 | SpellAttr1.Channeled2));
        }
        public int GetMaxDuration()
        {
            if (DurationEntry.ID == 0)
                return 0;
            return (DurationEntry.Duration[2] == -1) ? -1 : Math.Abs(DurationEntry.Duration[2]);
        }
        public bool IsBreakingStealth()
        {
            return !Convert.ToBoolean(AttributesEx & SpellAttr1.NotBreakStealth);
        }

        public bool NeedsComboPoints()
        {
            return Convert.ToBoolean(AttributesEx & (SpellAttr1.ReqComboPoints1 | SpellAttr1.ReqComboPoints2));
        }


        #region Fields
        public uint Id { get; set; }
        public uint Category { get; set; }
        public uint Dispel { get; set; }
        public uint Mechanic { get; set; }
        public SpellAttr0 Attributes { get; set; }
        public SpellAttr1 AttributesEx { get; set; }
        public SpellAttr2 AttributesEx2 { get; set; }
        public SpellAttr3 AttributesEx3 { get; set; }
        public SpellAttr4 AttributesEx4 { get; set; }
        public SpellAttr5 AttributesEx5 { get; set; }
        public SpellAttr6 AttributesEx6 { get; set; }
        public SpellAttr7 AttributesEx7 { get; set; }
        public SpellAttr8 AttributesEx8 { get; set; }
        public SpellAttr9 AttributesEx9 { get; set; }
        public SpellAttr10 AttributesEx10 { get; set; }
        public SpellCustomAttributes AttributesCu { get; set; }
        public uint Stances { get; set; }
        public uint StancesNot { get; set; }
        public uint Targets { get; set; }
        public uint TargetCreatureType { get; set; }
        public uint RequiresSpellFocus { get; set; }
        public uint FacingCasterFlags { get; set; }
        public uint CasterAuraState { get; set; }
        public uint TargetAuraState { get; set; }
        public uint CasterAuraStateNot { get; set; }
        public uint TargetAuraStateNot { get; set; }
        public uint CasterAuraSpell { get; set; }
        public uint TargetAuraSpell { get; set; }
        public uint ExcludeCasterAuraSpell { get; set; }
        public uint ExcludeTargetAuraSpell { get; set; }
        public SpellCastTimesEntry CastTimeEntry { get; set; }
        public uint RecoveryTime { get; set; }
        public uint CategoryRecoveryTime { get; set; }
        public uint StartRecoveryCategory { get; set; }
        public uint StartRecoveryTime { get; set; }
        public uint InterruptFlags { get; set; }
        public uint AuraInterruptFlags { get; set; }
        public uint ChannelInterruptFlags { get; set; }
        public uint ProcFlags { get; set; }
        public uint ProcChance { get; set; }
        public uint ProcCharges { get; set; }
        public uint MaxLevel { get; set; }
        public uint BaseLevel { get; set; }
        public uint SpellLevel { get; set; }
        public SpellDurationEntry DurationEntry { get; set; }
        public Powers PowerType { get; set; }
        public uint ManaCost { get; set; }
        public uint ManaCostPerlevel { get; set; }
        public uint ManaPerSecond { get; set; }
        public uint ManaCostPercentage { get; set; }
        public uint RuneCostID { get; set; }
        public SpellRangeEntry RangeEntry { get; set; }
        public float Speed { get; set; }
        public uint StackAmount { get; set; }
        public uint[] Totem = new uint[2];
        public int[] Reagent = new int[SharedConst.MaxSpellReagents];
        public uint[] ReagentCount = new uint[SharedConst.MaxSpellReagents];
        public int EquippedItemClass { get; set; }
        public int EquippedItemSubClassMask { get; set; }
        public int EquippedItemInventoryTypeMask { get; set; }
        public uint[] TotemCategory = new uint[2];
        public uint[] SpellVisual = new uint[2];
        public uint SpellIconID { get; set; }
        public uint ActiveIconID { get; set; }
        public string SpellName { get; set; }
        public string Rank { get; set; }
        //uint MaxTargetLevel;
        public uint MaxAffectedTargets { get; set; }
        public uint SpellFamilyName { get; set; }
        public uint[] SpellFamilyFlags { get; set; }
        public SpellDmgClass DmgClass { get; set; }
        public uint PreventionType { get; set; }
        public int AreaGroupId { get; set; }
        public uint SchoolMask { get; set; }
        public uint SpellDifficultyId { get; set; }
        public uint SpellScalingId { get; set; }
        public uint SpellAuraOptionsId { get; set; }
        public uint SpellAuraRestrictionsId { get; set; }
        public uint SpellCastingRequirementsId { get; set; }
        public uint SpellCategoriesId { get; set; }
        public uint SpellClassOptionsId { get; set; }
        public uint SpellCooldownsId { get; set; }
        public uint SpellEquippedItemsId { get; set; }
        public uint SpellInterruptsId { get; set; }
        public uint SpellLevelsId { get; set; }
        public uint SpellPowerId { get; set; }
        public uint SpellReagentsId { get; set; }
        public uint SpellShapeshiftId { get; set; }
        public uint SpellTargetRestrictionsId { get; set; }
        public uint SpellTotemsId { get; set; }
        //SpellScalingEntry
        public int CastTimeMin { get; set; }
        public int CastTimeMax { get; set; }
        public int CastTimeMaxLevel { get; set; }
        public int ScalingClass { get; set; }
        public float CoefBase { get; set; }
        public int CoefLevelBase { get; set; }
        public SpellEffectInfo[] Effects = new SpellEffectInfo[SharedConst.MaxSpellEffects];
        public uint ExplicitTargetMask { get; set; }
        public SpellChainNode ChainEntry { get; set; }
        #endregion

        public enum SpellCustomAttributes
        {
            SPELL_ATTR0_CU_ENCHANT_PROC = 0x00000001,
            SPELL_ATTR0_CU_CONE_BACK = 0x00000002,
            SPELL_ATTR0_CU_CONE_LINE = 0x00000004,
            SPELL_ATTR0_CU_SHARE_DAMAGE = 0x00000008,
            SPELL_ATTR0_CU_NO_INITIAL_THREAT = 0x00000010,
            SPELL_ATTR0_CU_NONE2 = 0x00000020,   // UNUSED
            SPELL_ATTR0_CU_AURA_CC = 0x00000040,
            SPELL_ATTR0_CU_DIRECT_DAMAGE = 0x00000100,
            SPELL_ATTR0_CU_CHARGE = 0x00000200,
            SPELL_ATTR0_CU_PICKPOCKET = 0x00000400,
            SPELL_ATTR0_CU_NEGATIVE_EFF0 = 0x00001000,
            SPELL_ATTR0_CU_NEGATIVE_EFF1 = 0x00002000,
            SPELL_ATTR0_CU_NEGATIVE_EFF2 = 0x00004000,
            SPELL_ATTR0_CU_IGNORE_ARMOR = 0x00008000,
            SPELL_ATTR0_CU_REQ_TARGET_FACING_CASTER = 0x00010000,
            SPELL_ATTR0_CU_REQ_CASTER_BEHIND_TARGET = 0x00020000,

            SPELL_ATTR0_CU_NEGATIVE = SPELL_ATTR0_CU_NEGATIVE_EFF0 | SPELL_ATTR0_CU_NEGATIVE_EFF1 | SPELL_ATTR0_CU_NEGATIVE_EFF2
        };
    }

    public class SpellEffectInfo
    {
        public SpellEffectInfo(SpellInfo _spellInfo, uint effIndex, SpellEffectEntry _effect)
        {
            SpellScalingEntry scaling = DBCStorage.SpellScalingStorage.LookupByKey(_spellInfo.SpellScalingId);

            spellInfo = _spellInfo;
            _effIndex = _effect != null ? _effect.EffectIndex : effIndex;
            Effect = _effect.Effect;
            ApplyAuraName = _effect.EffectApplyAuraName;
            Amplitude = _effect.EffectAmplitude;
            DieSides = _effect.EffectDieSides;
            RealPointsPerLevel = _effect.EffectRealPointsPerLevel;
            BasePoints = _effect.EffectBasePoints;
            PointsPerComboPoint = _effect.EffectPointsPerComboPoint;
            ValueMultiplier = _effect.EffectValueMultiplier;
            DamageMultiplier = _effect.EffectDamageMultiplier;
            BonusMultiplier = _effect.EffectBonusMultiplier;
            MiscValue = _effect.EffectMiscValue;
            MiscValueB = _effect.EffectMiscValueB;
            //Mechanic = Mechanics(_effect ? _effect.EffectMechanic : 0);
            //TargetA = SpellImplicitTargetInfo(_effect ? _effect.EffectImplicitTargetA : 0);
            //TargetB = SpellImplicitTargetInfo(_effect ? _effect.EffectImplicitTargetB : 0);
            RadiusEntry = DBCStorage.SpellRadiusStorage.LookupByKey(_effect.EffectRadiusIndex);
            ChainTarget = _effect.EffectChainTarget;
            ItemType = _effect.EffectItemType;
            TriggerSpell = _effect.EffectTriggerSpell;
            //SpellClassMask = _effect.EffectSpellClassMask;
            //ImplicitTargetConditions = null;
            //ScalingMultiplier = scaling != null ? scaling.Multiplier[_effIndex] : 0.0f;
            //DeltaScalingMultiplier = scaling != null ? scaling.RandomMultiplier[_effIndex] : 0.0f;
            //ComboScalingMultiplier = scaling != null ? scaling.OtherMultiplier[_effIndex] : 0.0f;
        }

        public bool IsEffect()
        {
            return Effect != 0;
        }
        public bool IsEffect(SpellEffects effectName)
        {
            return Effect == (uint)effectName;
        }

        //needs fixed
        public int CalcValue(Unit caster = null, int? bp = null, Unit target = null)
        {
            float basePointsPerLevel = RealPointsPerLevel;
            int basePoints = bp ?? BasePoints;
            float comboDamage = PointsPerComboPoint;

            // base amount modification based on spell lvl vs caster lvl
            if (ScalingMultiplier != 0.0f)
            {
                if (caster != null)
                {
                    uint level = caster.getLevel();
                    if (target != null && spellInfo.IsPositiveEffect(_effIndex) && (Effect == (uint)SpellEffects.ApplyAura))
                        level = target.getLevel();

                    var gtScaling = DBCStorage.GtSpellScalingStorage.LookupByKey((uint)(spellInfo.ScalingClass != -1 ? spellInfo.ScalingClass - 1 : (int)Class.Max - 1) * 100 + level - 1);
                    if (gtScaling.value != 0)
                    {
                        float multiplier = gtScaling.value;
                        if (spellInfo.CastTimeMax > 0 && spellInfo.CastTimeMaxLevel > level)
                            multiplier *= (float)(spellInfo.CastTimeMin + (level - 1) * (spellInfo.CastTimeMax - spellInfo.CastTimeMin) / (spellInfo.CastTimeMaxLevel - 1)) / (float)spellInfo.CastTimeMax;
                        if (spellInfo.CoefLevelBase > level)
                            multiplier *= (1.0f - spellInfo.CoefBase) * (float)(level - 1) / (float)(spellInfo.CoefLevelBase - 1) + spellInfo.CoefBase;

                        float preciseBasePoints = ScalingMultiplier * multiplier;
                        if (DeltaScalingMultiplier != 0.0f)
                        {
                            float delta = DeltaScalingMultiplier * ScalingMultiplier * multiplier * 0.5f;
                            preciseBasePoints += RandomHelper.frand((int)-delta, (int)delta);
                        }

                        basePoints = (int)preciseBasePoints;

                        if (ComboScalingMultiplier != 0.0f)
                            comboDamage = ComboScalingMultiplier * multiplier;
                    }
                }
            }
            else
            {
                if (caster != null)
                {
                    uint level = caster.getLevel();
                    if (level > spellInfo.MaxLevel && spellInfo.MaxLevel > 0)
                        level = spellInfo.MaxLevel;
                    else if (level < spellInfo.BaseLevel)
                        level = spellInfo.BaseLevel;

                    level -= spellInfo.SpellLevel;
                    basePoints += (int)(level * basePointsPerLevel);
                }

                // roll in a range <1;EffectDieSides> as of patch 3.3.3
                int randomPoints = DieSides;
                switch (randomPoints)
                {
                    case 0: 
                        break;
                    case 1: basePoints += 1; 
                        break;                     // range 1..1
                    default:
                        {
                            // range can have positive (1..rand) and negative (rand..1) values, so order its for irand
                            int randvalue = (randomPoints >= 1)
                                ? new Random(1).Next(randomPoints)
                                : new Random(randomPoints).Next(1);

                            basePoints += randvalue;
                            break;
                        }
                }
            }

            float value = (float)basePoints;

            // random damage
            if (caster != null)
            {
                // bonus amount from combo points
                //if (caster.m_movedPlayer && comboDamage)
                //if (uint8 comboPoints = caster.m_movedPlayer.GetComboPoints())
                //value += comboDamage * comboPoints;

                value = caster.ApplyEffectModifiers(spellInfo, _effIndex, value);

                // amount multiplication based on caster's level
                if (spellInfo.GetSpellScaling().Id == 0 && basePointsPerLevel == 0.0f && Convert.ToBoolean(spellInfo.Attributes & SpellAttr0.LevelDamageCalculation) &&
                    spellInfo.SpellLevel != 0 &&
                    Effect != (uint)SpellEffects.WeaponPercentDamage &&
                    Effect != (uint)SpellEffects.KnockBack &&
                    Effect != (uint)SpellEffects.AddExtraAttacks &&
                    ApplyAuraName != (uint)AuraType.ModSpeedAlways &&
                    ApplyAuraName != (uint)AuraType.ModSpeedNotStack &&
                    ApplyAuraName != (uint)AuraType.ModIncreaseSpeed &&
                    ApplyAuraName != (uint)AuraType.ModDecreaseSpeed)
                    //there are many more: slow speed, -healing pct
                    value *= (float)(0.25f * Math.Exp(caster.getLevel() * (70 - spellInfo.SpellLevel) / 1000.0f));
                //value = int32(value * (int32)getLevel() / (int32)(_spellInfo->spellLevel ? _spellInfo->spellLevel : 1));
            }
            return (int)value;
        }

        #region Fields
        SpellInfo spellInfo;
        uint _effIndex;
        public uint Effect { get; private set; }
        public uint ApplyAuraName { get; private set; }
        public uint Amplitude { get; private set; }
        public int DieSides { get; private set; }
        public float RealPointsPerLevel { get; private set; }
        public int BasePoints { get; private set; }
        public float PointsPerComboPoint { get; private set; }
        public float ValueMultiplier { get; private set; }
        public float DamageMultiplier { get; private set; }
        public float BonusMultiplier { get; private set; }
        public int MiscValue { get; private set; }
        public int MiscValueB { get; private set; }
        //Mechanics Mechanic;
        //SpellImplicitTargetInfo TargetA;
        //SpellImplicitTargetInfo TargetB;
        public SpellRadiusEntry RadiusEntry { get; private set; }
        public uint ChainTarget { get; private set; }
        public uint ItemType { get; private set; }
        public uint TriggerSpell { get; private set; }
        //flag96    SpellClassMask;
        //std::list<Condition*>* ImplicitTargetConditions;
        public float ScalingMultiplier { get; private set; }
        public float DeltaScalingMultiplier { get; private set; }
        public float ComboScalingMultiplier { get; private set; }
        #endregion
    }
}
