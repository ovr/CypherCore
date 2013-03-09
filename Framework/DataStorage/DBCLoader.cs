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

using Framework.Logging;
using Framework.Constants;
using System.Collections.Generic;

namespace Framework.DataStorage
{
    public class DBCLoader : DBCStorage
    {
        public static void Init()
        {
            AreaTableStorage = DBCReader.ReadDBC<AreaTableEntry>(AreaTableStrings, DBCFmt.AreaTableEntryfmt, "AreaTable.dbc");
            //ArmorLocationStorage = DBCReader.ReadDBC<ArmorLocationEntry>(null, DBCFmt.ArmorLocationfmt, "ArmorLocation.dbc");

            CharStartOutfitStorage = DBCReader.ReadDBC<CharStartOutfit>(null, DBCFmt.CharStartOutfitfmt, "CharStartoutfit.dbc");
            CharClassStorage = DBCReader.ReadDBC<CharClasses>(ClassStrings, DBCFmt.CharClassesEntryfmt, "ChrClasses.dbc");
            CharPowerTypesStorage = DBCReader.ReadDBC<CharPowerTypesEntry>(null, DBCFmt.CharClassesPowerTypesfmt, "ChrClassesXPowerTypes.dbc");
            CharRaceStorage = DBCReader.ReadDBC<CharRaces>(RaceStrings, DBCFmt.CharRacesEntryfmt, "ChrRaces.dbc");
            CreatureDisplayInfoStorage = DBCReader.ReadDBC<CreatureDisplayInfoEntry>(null, DBCFmt.CreatureDisplayInfofmt, "CreatureDisplayInfo.dbc");
            CreatureModelDataStorage = DBCReader.ReadDBC<CreatureModelDataEntry>(null, DBCFmt.CreatureModelDatafmt, "CreatureModelData.dbc");
            CurrencyTypesStorage = DBCReader.ReadDBC<CurrencyTypesEntry>(null, DBCFmt.CurrencyTypesfmt, "CurrencyTypes.dbc");

            FactionStorage = DBCReader.ReadDBC<FactionEntry>(FactionStrings, DBCFmt.FactionEntryfmt, "Faction.dbc");
            FactionTemplateStorage = DBCReader.ReadDBC<FactionTemplateEntry>(null, DBCFmt.FactionTemplateEntryfmt, "FactionTemplate.dbc");

            sGtBarberShopBaseStorage = DBCReader.ReadDBC<GtBarberShopCostBaseEntry>(null, DBCFmt.GtBarberShopCostBasefmt, "gtBarberShopCostBase.dbc");
            sGtCombatRatingsStorage = DBCReader.ReadDBC<GtCombatRatingsEntry>(null, DBCFmt.GtCombatRatingsfmt, "gtCombatRatings.dbc");
            sGtChanceToMeleeCritBaseStorage = DBCReader.ReadDBC<GtChanceToMeleeCritBaseEntry>(null, DBCFmt.GtChanceToMeleeCritBasefmt, "gtChanceToMeleeCritBase.dbc");
            sGtChanceToMeleeCritStorage = DBCReader.ReadDBC<GtChanceToMeleeCritEntry>(null, DBCFmt.GtChanceToMeleeCritfmt, "gtChanceToMeleeCrit.dbc");
            sGtChanceToSpellCritBaseStorage = DBCReader.ReadDBC<GtChanceToSpellCritBaseEntry>(null, DBCFmt.GtChanceToSpellCritBasefmt, "gtChanceToSpellCritBase.dbc");
            sGtChanceToSpellCritStorage = DBCReader.ReadDBC<GtChanceToSpellCritEntry>(null, DBCFmt.GtChanceToSpellCritfmt, "gtChanceToSpellCrit.dbc");
            sGtOCTClassCombatRatingScalarStorage = DBCReader.ReadDBC<GtOCTClassCombatRatingScalarEntry>(null, DBCFmt.GtOCTClassCombatRatingScalarfmt, "gtOCTClassCombatRatingScalar.dbc");
            //sGtOCTRegenMPStore  = DBCReader.ReadDBC <GtOCTRegenMPEntry>  (null, DBCFmt, "gtOCTRegenMP.dbc")          ; -- not used currently
            sGtOCTHpPerStaminaStorage = DBCReader.ReadDBC<GtOCTHpPerStaminaEntry>(null, DBCFmt.GtOCTHpPerStaminafmt, "gtOCTHpPerStamina.dbc");
            sGtRegenMPPerSptStorage = DBCReader.ReadDBC<GtRegenMPPerSptEntry>(null, DBCFmt.GtRegenMPPerSptfmt, "gtRegenMPPerSpt.dbc");
            sGtSpellScalingStorage = DBCReader.ReadDBC<GtSpellScalingEntry>(null, DBCFmt.GtSpellScalingfmt, "gtSpellScaling.dbc");
            sGtOCTBaseHPByClassStorage = DBCReader.ReadDBC<GtOCTBaseHPByClassEntry>(null, DBCFmt.GtOCTBaseHPByClassfmt, "gtOCTBaseHPByClass.dbc");
            sGtOCTBaseMPByClassStorage = DBCReader.ReadDBC<GtOCTBaseMPByClassEntry>(null, DBCFmt.GtOCTBaseMPByClassfmt, "gtOCTBaseMPByClass.dbc");
            sGuildPerkSpellsStorage = DBCReader.ReadDBC<GuildPerkSpellsEntry>(null, DBCFmt.GuildPerkSpellsfmt, "GuildPerkSpells.dbc");

            ItemArmorQualityStorage = DBCReader.ReadDBC<ItemArmorQualityEntry>(null, DBCFmt.ItemArmorQualityfmt, "ItemArmorQuality.dbc");
            ItemArmorShieldStorage = DBCReader.ReadDBC<ItemArmorShieldEntry>(null, DBCFmt.ItemArmorShieldfmt, "ItemArmorShield.dbc");
            ItemArmorTotalStorage = DBCReader.ReadDBC<ItemArmorTotalEntry>(null, DBCFmt.ItemArmorTotalfmt, "ItemArmorTotal.dbc");
            ItemDamageAmmoStorage = DBCReader.ReadDBC<ItemDamageEntry>(null, DBCFmt.ItemDamagefmt,  "ItemDamageAmmo.dbc");
            ItemDamageOneHandStorage = DBCReader.ReadDBC<ItemDamageEntry>(null, DBCFmt.ItemDamagefmt, "ItemDamageOneHand.dbc");
            ItemDamageOneHandCasterStorage = DBCReader.ReadDBC<ItemDamageEntry>(null, DBCFmt.ItemDamagefmt, "ItemDamageOneHandCaster.dbc");
            ItemDamageRangedStorage = DBCReader.ReadDBC<ItemDamageEntry>(null, DBCFmt.ItemDamagefmt, "ItemDamageRanged.dbc");
            ItemDamageThrownStorage = DBCReader.ReadDBC<ItemDamageEntry>(null, DBCFmt.ItemDamagefmt, "ItemDamageThrown.dbc");
            ItemDamageTwoHandStorage = DBCReader.ReadDBC<ItemDamageEntry>(null, DBCFmt.ItemDamagefmt, "ItemDamageTwoHand.dbc");
            ItemDamageTwoHandCasterStorage = DBCReader.ReadDBC<ItemDamageEntry>(null, DBCFmt.ItemDamagefmt, "ItemDamageTwoHandCaster.dbc");
            ItemDamageWandStorage = DBCReader.ReadDBC<ItemDamageEntry>(null, DBCFmt.ItemDamagefmt, "ItemDamageWand.dbc");
            ItemDisenchantLootStorage = DBCReader.ReadDBC<ItemDisenchantLootEntry>(null, DBCFmt.ItemDisenchantLootfmt, "ItemDisenchantLoot.dbc");
            ItemRandomPropertiesStorage = DBCReader.ReadDBC<ItemRandomPropertiesEntry>(ItemRandomPropertiesStrings, DBCFmt.ItemRandomPropertiesfmt, "ItemRandomProperties.dbc");
            ItemRandomSuffixStorage = DBCReader.ReadDBC<ItemRandomSuffixEntry>(ItemRandomSuffixStrings, DBCFmt.ItemRandomSuffixfmt, "ItemRandomSuffix.dbc");

            LiquidTypeStorage = DBCReader.ReadDBC<LiquidTypeEntry>(null, DBCFmt.LiquidTypefmt, "LiquidType.dbc");

            NameGenStorage = DBCReader.ReadDBC<NameGen>(NameGenStrings, DBCFmt.NameGenfmt, "NameGen.dbc");
            MapStorage = DBCReader.ReadDBC<MapEntry>(MapStrings, DBCFmt.MapEntryfmt, "Map.dbc");
            MapDifficultyStorage = DBCReader.ReadDBC<MapDifficultyEntry>(MapDifficultyStrings, DBCFmt.MapDifficultyEntryfmt, "MapDifficulty.dbc");

            SkillLineAbilityStorage = DBCReader.ReadDBC<SkillLineAbilityEntry>(null, DBCFmt.SkillLineAbilityfmt, "SkillLineAbility.dbc");
            SkillLineStorage = DBCReader.ReadDBC<SkillLineEntry>(null, DBCFmt.SkillLinefmt, "SkillLine.dbc");

            SpellAuraOptionsStorage = DBCReader.ReadDBC<SpellAuraOptionsEntry>(null, DBCFmt.SpellAuraOptionsEntryfmt, "SpellAuraOptions.dbc");
            SpellAuraRestrictionsStorage = DBCReader.ReadDBC<SpellAuraRestrictionsEntry>(null, DBCFmt.SpellAuraRestrictionsEntryfmt, "SpellAuraRestrictions.dbc");
            SpellCastingRequirementsStorage = DBCReader.ReadDBC<SpellCastingRequirementsEntry>(null, DBCFmt.SpellCastingRequirementsEntryfmt, "SpellCastingRequirements.dbc");
            SpellCastTimesStorage = DBCReader.ReadDBC<SpellCastTimesEntry>(null, DBCFmt.SpellCastTimefmt, "SpellCastTimes.dbc");
            SpellCategoriesStorage = DBCReader.ReadDBC<SpellCategoriesEntry>(null, DBCFmt.SpellCategoriesEntryfmt, "SpellCategories.dbc");
            SpellClassOptionsStorage = DBCReader.ReadDBC<SpellClassOptionsEntry>(null, DBCFmt.SpellClassOptionsEntryfmt, "SpellClassOptions.dbc");
            SpellCooldownsStorage = DBCReader.ReadDBC<SpellCooldownsEntry>(null, DBCFmt.SpellCooldownsEntryfmt, "SpellCooldowns.dbc");
            //SpellDifficultyStorage = DBCReader.ReadDBC<SpellDifficultyEntry>(null, DBCFmt.SpellDifficultyfmt, "SpellDifficulty.dbc");
            SpellDurationStorage = DBCReader.ReadDBC<SpellDurationEntry>(null, DBCFmt.SpellDurationfmt, "SpellDuration.dbc");
            SpellEffectStorage = DBCReader.ReadDBC<SpellEffectEntry>(null, DBCFmt.SpellEffectEntryfmt, "SpellEffect.dbc");
            SpellStorage = DBCReader.ReadDBC<SpellEntry>(SpellStrings, DBCFmt.SpellEntryfmt, "Spell.dbc");
            SpellEquippedItemsStorage = DBCReader.ReadDBC<SpellEquippedItemsEntry>(null, DBCFmt.SpellEquippedItemsEntryfmt, "SpellEquippedItems.dbc");
            SpellFocusObjectStorage = DBCReader.ReadDBC<SpellFocusObjectEntry>(null, DBCFmt.SpellFocusObjectfmt, "SpellFocusObject.dbc");
            SpellInterruptsEntryStorage = DBCReader.ReadDBC<SpellInterruptsEntry>(null, DBCFmt.SpellInterruptsEntryfmt, "SpellInterrupts.dbc");
            SpellItemEnchantmentStorage = DBCReader.ReadDBC<SpellItemEnchantmentEntry>(null, DBCFmt.SpellItemEnchantmentfmt, "SpellItemEnchantment.dbc");
            SpellItemEnchantmentConditionStorage = DBCReader.ReadDBC<SpellItemEnchantmentConditionEntry>(null, DBCFmt.SpellItemEnchantmentConditionfmt, "SpellItemEnchantmentCondition.dbc");
            SpellLevelsStorage = DBCReader.ReadDBC<SpellLevelsEntry>(null, DBCFmt.SpellLevelsEntryfmt, "SpellLevels.dbc");
            SpellMiscStorage = DBCReader.ReadDBC<SpellMisc>(null, DBCFmt.SpellMiscfmt, "SpellMisc_internal.dbc");
            SpellPowerStorage = DBCReader.ReadDBC<SpellPowerEntry>(null, DBCFmt.SpellPowerEntryfmt, "SpellPower.dbc");
            SpellRadiusStorage = DBCReader.ReadDBC<SpellRadiusEntry>(null, DBCFmt.SpellRadiusfmt, "SpellRadius.dbc");
            SpellRangeStorage = DBCReader.ReadDBC<SpellRangeEntry>(null, DBCFmt.SpellRangefmt, "SpellRange.dbc");
            SpellReagentsStorage = DBCReader.ReadDBC<SpellReagentsEntry>(null, DBCFmt.SpellReagentsEntryfmt, "SpellReagents.dbc");
            SpellRuneCostStorage = DBCReader.ReadDBC<SpellRuneCostEntry>(null, DBCFmt.SpellRuneCostfmt, "SpellRuneCost.dbc");
            SpellScalingStorage = DBCReader.ReadDBC<SpellScalingEntry>(null, DBCFmt.SpellScalingEntryfmt, "SpellScaling.dbc");
            SpellShapeshiftStorage = DBCReader.ReadDBC<SpellShapeshiftEntry>(null, DBCFmt.SpellShapeshiftEntryfmt, "SpellShapeshift.dbc");
            SpellShapeshiftFormStorage = DBCReader.ReadDBC<SpellShapeshiftFormEntry>(null, DBCFmt.SpellShapeshiftFormfmt, "SpellShapeshiftForm.dbc");
            SpellTargetRestrictionsStorage = DBCReader.ReadDBC<SpellTargetRestrictionsEntry>(null, DBCFmt.SpellTargetRestrictionsEntryfmt, "SpellTargetRestrictions.dbc");
            SpellTotemsStorage = DBCReader.ReadDBC<SpellTotemsEntry>(null, DBCFmt.SpellTotemsEntryfmt, "SpellTotems.dbc");

            FasterLookups();

            Log.outInfo("Loaded {0} DBC files.", DBCStorage.DBCFileCount);
        }

        static void FasterLookups()
        {
            PowersByClass = new int[(int)Class.Max][];
            for (var i = 0; i < (int)Class.Max; ++i)
            {
                for (var j = 0; j < (int)Powers.Max; ++j)
                {
                    if (PowersByClass[i] == null)
                        PowersByClass[i] = new int[(int)Powers.Max];
                    PowersByClass[i][j] = (int)Powers.Max;
                }
            }

            foreach (var power in CharPowerTypesStorage.Values)
            {
                int index = 0;
                for (var j = 0; j < (int)Powers.Max; ++j)
                    if (PowersByClass[power.classId][j] != (int)Powers.Max)
                        ++index;

                PowersByClass[power.classId][power.power] = index;

            }
            
            // must be after sAreaStore loading
            foreach (var area in AreaTableStorage.Values)
            {
                // fill AreaId->DBC records
                AreaFlagByAreaID.Add(area.ID, area.exploreFlag);
                
                // fill MapId->DBC records (skip sub zones and continents)
                if (area.zone == 0 && area.mapid != 0 && area.mapid != 1 && area.mapid != 530 && area.mapid != 571)
                    AreaFlagByMapID.Add(new KeyValuePair<uint,uint>(area.mapid, area.exploreFlag));                
            }
        }
    }
}
