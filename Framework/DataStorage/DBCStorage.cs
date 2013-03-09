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

using System.Collections.Generic;

namespace Framework.DataStorage
{
    public class DBCStorage
    {
        internal static int DBCFileCount = 0;

        public static Dictionary<uint, AreaTableEntry> AreaTableStorage;
        public static Dictionary<uint, ArmorLocationEntry> ArmorLocationStorage;

        public static Dictionary<uint, CharStartOutfit> CharStartOutfitStorage;
        public static Dictionary<uint, CharClasses> CharClassStorage;
        public static Dictionary<uint, CharPowerTypesEntry> CharPowerTypesStorage;
        public static Dictionary<uint, CharRaces> CharRaceStorage;
        public static Dictionary<uint, CreatureDisplayInfoEntry> CreatureDisplayInfoStorage;
        public static Dictionary<uint, CreatureModelDataEntry> CreatureModelDataStorage;
        public static Dictionary<uint, CurrencyTypesEntry> CurrencyTypesStorage;

        public static Dictionary<uint, FactionEntry> FactionStorage;
        public static Dictionary<uint, FactionTemplateEntry> FactionTemplateStorage;

        public static Dictionary<uint, GtBarberShopCostBaseEntry> sGtBarberShopBaseStorage;
        public static Dictionary<uint, GtCombatRatingsEntry> sGtCombatRatingsStorage;
        public static Dictionary<uint, GtChanceToMeleeCritBaseEntry> sGtChanceToMeleeCritBaseStorage;
        public static Dictionary<uint, GtChanceToMeleeCritEntry> sGtChanceToMeleeCritStorage;
        public static Dictionary<uint, GtChanceToSpellCritBaseEntry> sGtChanceToSpellCritBaseStorage;
        public static Dictionary<uint, GtChanceToSpellCritEntry> sGtChanceToSpellCritStorage;
        public static Dictionary<uint, GtOCTClassCombatRatingScalarEntry> sGtOCTClassCombatRatingScalarStorage;
        //public static Dictionary<uint, GtOCTRegenMPEntry > sGtOCTRegenMPStore;
        public static Dictionary<uint, GtOCTHpPerStaminaEntry> sGtOCTHpPerStaminaStorage;
        public static Dictionary<uint, GtRegenMPPerSptEntry> sGtRegenMPPerSptStorage;
        public static Dictionary<uint, GtSpellScalingEntry> sGtSpellScalingStorage;
        public static Dictionary<uint, GtOCTBaseHPByClassEntry> sGtOCTBaseHPByClassStorage;
        public static Dictionary<uint, GtOCTBaseMPByClassEntry> sGtOCTBaseMPByClassStorage;
        public static Dictionary<uint, GuildPerkSpellsEntry> sGuildPerkSpellsStorage;

        public static Dictionary<uint, ItemArmorQualityEntry> ItemArmorQualityStorage;
        public static Dictionary<uint, ItemArmorShieldEntry> ItemArmorShieldStorage;
        public static Dictionary<uint, ItemArmorTotalEntry> ItemArmorTotalStorage;
        public static Dictionary<uint, ItemDamageEntry> ItemDamageAmmoStorage;
        public static Dictionary<uint, ItemDamageEntry> ItemDamageOneHandStorage;
        public static Dictionary<uint, ItemDamageEntry> ItemDamageOneHandCasterStorage;
        public static Dictionary<uint, ItemDamageEntry> ItemDamageRangedStorage;
        public static Dictionary<uint, ItemDamageEntry> ItemDamageThrownStorage;
        public static Dictionary<uint, ItemDamageEntry> ItemDamageTwoHandStorage;
        public static Dictionary<uint, ItemDamageEntry> ItemDamageTwoHandCasterStorage;
        public static Dictionary<uint, ItemDamageEntry> ItemDamageWandStorage;
        public static Dictionary<uint, ItemDisenchantLootEntry> ItemDisenchantLootStorage;
        public static Dictionary<uint, ItemRandomPropertiesEntry> ItemRandomPropertiesStorage;
        public static Dictionary<uint, ItemRandomSuffixEntry> ItemRandomSuffixStorage;

        public static Dictionary<uint, LiquidTypeEntry> LiquidTypeStorage;

        public static Dictionary<uint, NameGen> NameGenStorage;
        public static Dictionary<uint, MapEntry> MapStorage;
        public static Dictionary<uint, MapDifficultyEntry> MapDifficultyStorage;

        public static Dictionary<uint, RandomPropertiesPointsEntry> RandomPropertiesPointsStorage;

        public static Dictionary<uint, SkillLineAbilityEntry> SkillLineAbilityStorage;
        public static Dictionary<uint, SkillLineEntry> SkillLineStorage;

        public static Dictionary<uint, SpellAuraOptionsEntry> SpellAuraOptionsStorage;
        public static Dictionary<uint, SpellAuraRestrictionsEntry> SpellAuraRestrictionsStorage;
        public static Dictionary<uint, SpellCastingRequirementsEntry> SpellCastingRequirementsStorage;
        public static Dictionary<uint, SpellCastTimesEntry> SpellCastTimesStorage;
        public static Dictionary<uint, SpellCategoriesEntry> SpellCategoriesStorage;
        public static Dictionary<uint, SpellClassOptionsEntry> SpellClassOptionsStorage;
        public static Dictionary<uint, SpellCooldownsEntry> SpellCooldownsStorage;
        public static Dictionary<uint, SpellDifficultyEntry> SpellDifficultyStorage;
        public static Dictionary<uint, SpellDurationEntry> SpellDurationStorage;
        public static Dictionary<uint, SpellEffectEntry> SpellEffectStorage;
        public static Dictionary<uint, SpellEntry> SpellStorage;
        public static Dictionary<uint, SpellEquippedItemsEntry> SpellEquippedItemsStorage;
        public static Dictionary<uint, SpellFocusObjectEntry> SpellFocusObjectStorage;
        public static Dictionary<uint, SpellInterruptsEntry> SpellInterruptsEntryStorage;
        public static Dictionary<uint, SpellItemEnchantmentConditionEntry> SpellItemEnchantmentConditionStorage;
        public static Dictionary<uint, SpellItemEnchantmentEntry> SpellItemEnchantmentStorage;
        public static Dictionary<uint, SpellLevelsEntry> SpellLevelsStorage;
        public static Dictionary<uint, SpellMisc> SpellMiscStorage;
        public static Dictionary<uint, SpellPowerEntry> SpellPowerStorage;
        public static Dictionary<uint, SpellRadiusEntry> SpellRadiusStorage;
        public static Dictionary<uint, SpellRangeEntry> SpellRangeStorage;
        public static Dictionary<uint, SpellReagentsEntry> SpellReagentsStorage;
        public static Dictionary<uint, SpellRuneCostEntry> SpellRuneCostStorage;
        public static Dictionary<uint, SpellScalingEntry> SpellScalingStorage;
        public static Dictionary<uint, SpellShapeshiftEntry> SpellShapeshiftStorage;
        public static Dictionary<uint, SpellShapeshiftFormEntry> SpellShapeshiftFormStorage;
        public static Dictionary<uint, SpellTargetRestrictionsEntry> SpellTargetRestrictionsStorage;
        public static Dictionary<uint, SpellTotemsEntry> SpellTotemsStorage;
        public static Dictionary<uint, GtSpellScalingEntry> GtSpellScalingStorage;


        //Strings
        internal static Dictionary<uint, string> AreaTableStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> ClassStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> FactionStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> ItemRandomPropertiesStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> ItemRandomSuffixStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> MapStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> MapDifficultyStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> NameGenStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> RaceStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> SkillLineStrings = new Dictionary<uint, string>();
        internal static Dictionary<uint, string> SpellStrings = new Dictionary<uint, string>();

        public static int[][] PowersByClass;
        public static Dictionary<uint, uint> AreaFlagByAreaID = new Dictionary<uint,uint>();
        public static List<KeyValuePair<uint, uint>> AreaFlagByMapID = new List<KeyValuePair<uint, uint>>();
    }
}
