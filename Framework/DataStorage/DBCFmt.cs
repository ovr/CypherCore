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

namespace Framework.DataStorage
{
    public static class DBCFmt
    {
        public static int GetFMTCount(this string fmt)
        {
            int count = 0;
            for (var i = 0; i < fmt.Length; i++)
            {
                switch (fmt[i])
                {
                    case 'f':
                        count += sizeof(float);
                        break;
                    case 'n':
                    case 'i':
                    case 's':
                        count += sizeof(uint);
                        break;
                    case 'b':
                        count += sizeof(byte);
                        break;
                    case 'h':
                    case 'x':
                    case 'd':
                        break;
                }
            }
            return count;
        }

        // x - skip<uint32>, h - skip<uint8>, s - string, f - float, i - uint32, b - uint8, d - index (not included)
        // n - index (included), l - bool
        public const string AreaTableEntryfmt = "iiinixxxxxsxixiiiiixxxxxxxxxxx";

        public const string ArmorLocationfmt = "nfffff";

        public const string CharStartOutfitfmt = "diiiiiiiiiiiiiiiiiiiiiiiiixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxhhhh";
        public const string CharClassesEntryfmt = "nixixxxixixxxxxxxx";
        public const string CharClassesPowerTypesfmt = "nii";
        public const string CharRacesEntryfmt = "nxixiixixxxxixixxxxxxxxxxxxxxxxxxxxx";
        public const string CreatureDisplayInfofmt = "nixxfxxxxxxxxxxxxxx";
        public const string CreatureModelDatafmt = "nxxxxxxxxxxxxxffxxxxxxxxxxxxxxxxx";
        public const string CurrencyTypesfmt = "nxxxxxiiiix";

        public const string FactionEntryfmt = "niiiiiiiiiiiiiiiiiiffixsxx";
        public const string FactionTemplateEntryfmt = "niiiiiiiiiiiii";

        public const string GtBarberShopCostBasefmt = "xf";
        public const string GtCombatRatingsfmt = "xf";
        public const string GtOCTHpPerStaminafmt = "df";
        public const string GtChanceToMeleeCritBasefmt = "xf";
        public const string GtChanceToMeleeCritfmt = "xf";
        public const string GtChanceToSpellCritBasefmt = "xf";
        public const string GtChanceToSpellCritfmt = "xf";
        public const string GtOCTClassCombatRatingScalarfmt = "df";
        public const string GtOCTRegenHPfmt = "f";
        //public const string GtOCTRegenMPfmt = "f";
        public const string GtRegenMPPerSptfmt = "xf";
        public const string GtSpellScalingfmt = "df";
        public const string GtOCTBaseHPByClassfmt = "df";
        public const string GtOCTBaseMPByClassfmt = "df";
        public const string GuildPerkSpellsfmt = "dii";

        public const string ItemArmorQualityfmt = "nfffffffi";
        public const string ItemArmorShieldfmt = "nifffffff";
        public const string ItemArmorTotalfmt = "niffff";
        public const string ItemDamagefmt = "nfffffffi";
        public const string ItemDisenchantLootfmt = "niiiiii";
        public const string ItemRandomPropertiesfmt = "nxiiixxs";
        public const string ItemRandomSuffixfmt = "nsxiiiiiiiiii";

        public const string LiquidTypefmt = "nxxixixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

        public const string MapEntryfmt = "niiiisissififfiiiii";
        public const string MapDifficultyEntryfmt = "diisiix";

        public const string NameGenfmt = "nsii";

        public const string SkillLinefmt = "nisxiixxx";
        public const string SkillLineAbilityfmt = "niiiiiiiiixxx";
        public const string SpellAuraOptionsEntryfmt = "nixiiii";
        public const string SpellAuraRestrictionsEntryfmt = "nixiiiiiiii";
        public const string SpellCastingRequirementsEntryfmt = "nixxixi";
        public const string SpellCastTimefmt = "nixx";
        public const string SpellCategoriesEntryfmt = "nixiiiiiix";
        public const string SpellClassOptionsEntryfmt = "nxiiiix";
        public const string SpellCooldownsEntryfmt = "nixiii";
        public const string SpellDifficultyfmt = "niiii";
        public const string SpellDurationfmt = "niii";
        public const string SpellEffectEntryfmt = "nxifiiiffiiiiiifiifiiixixiiiix";
        public const string SpellEntryfmt = "nssxxixxfiiiiiiiiiiiiiiii";
        public const string SpellEquippedItemsEntryfmt = "nixiii";
        public const string SpellFocusObjectfmt = "nx";
        public const string SpellInterruptsEntryfmt = "nixixixi";
        public const string SpellItemEnchantmentConditionfmt = "nbbbbbxxxxxbbbbbbbbbbiiiiihhhhh";
        public const string SpellItemEnchantmentfmt = "nxiiiiiiiiisiiiiiiixxxxxx";
        public const string SpellLevelsEntryfmt = "nixiii";
        public const string SpellMiscfmt = "diiiiiiiiiiiiiiiiifiiiii";
        public const string SpellPowerEntryfmt = "nixiiiixxxxxx";
        public const string SpellRadiusfmt = "nfxxx";
        public const string SpellRangefmt = "nffffixx";
        public const string SpellReagentsEntryfmt = "nixiiiiiiiiiiiiiiii";
        public const string SpellRuneCostfmt = "niiiix";
        public const string SpellScalingEntryfmt = "niiiifxxx";
        public const string SpellShapeshiftEntryfmt = "nixixx";
        public const string SpellShapeshiftFormfmt = "nxxiixiiixxiiiiiiiixx";
        public const string SpellTargetRestrictionsEntryfmt = "nixfxiiii";
        public const string SpellTotemsEntryfmt = "niiii";

        /*
        const char Achievementfmt[]="niixsxiixixxii";
//const std::string CustomAchievementfmt="pppaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaapapaaaaaaaaaaaaaaaaaapp";
//const std::string CustomAchievementIndex = "ID";
const char AchievementCriteriafmt[]="niiiiiiiisiiiiixxiiiiii";
const char AreaTableEntryfmt[]="iiinixxxxxisiiiiixxxxxxxxx";
const char AreaGroupEntryfmt[]="niiiiiii";
const char AreaPOIEntryfmt[]="niiiiiiiiiiiffixixxixx";
const char AreaTriggerEntryfmt[]="nifffxxxfffff";
const char ArmorLocationfmt[]="nfffff";
const char AuctionHouseEntryfmt[]="niiix";
const char BankBagSlotPricesEntryfmt[]="ni";
const char BarberShopStyleEntryfmt[]="nixxxiii";
const char BattlemasterListEntryfmt[]="niiiiiiiiixsiiiixxxx";
const char CharStartOutfitEntryfmt[]="diiiiiiiiiiiiiiiiiiiiiiiiixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
const char CharTitlesEntryfmt[]="nxsxix";
const char ChatChannelsEntryfmt[]="nixsx";

const char ChrClassesEntryfmt[]="nixsxxxixiiiix";
const char ChrRacesEntryfmt[]="nxixiixixxxxixsxxxxxixxx";
const char ChrClassesXPowerTypesfmt[]="nii";

const char CinematicSequencesEntryfmt[]="nxxxxxxxxx";
const char CreatureDisplayInfofmt[]="nixxfxxxxxxxxxxxx";
const char CreatureModelDatafmt[]="nxxxxxxxxxxxxxffxxxxxxxxxxxxxxx";
const char CreatureFamilyfmt[]="nfifiiiiixsx";
const char CreatureSpellDatafmt[]="niiiixxxx";
const char CreatureTypefmt[]="nxx";
const char CurrencyTypesfmt[]="nxxxxxiiiix";

const char DestructibleModelDatafmt[]="ixxixxxixxxixxxixxxxxxxx";
const char DungeonEncounterfmt[]="iiixisxx";
const char DurabilityCostsfmt[]="niiiiiiiiiiiiiiiiiiiiiiiiiiiii";
const char DurabilityQualityfmt[]="nf";
const char EmotesEntryfmt[]="nxxiiixx";
const char EmotesTextEntryfmt[]="nxixxxxxxxxxxxxxxxx";
const char FactionEntryfmt[]="niiiiiiiiiiiiiiiiiiffixsxx";
const char FactionTemplateEntryfmt[]="niiiiiiiiiiiii";
const char GameObjectDisplayInfofmt[]="nsxxxxxxxxxxffffffxxx";

const char GemPropertiesEntryfmt[]="nixxix";
const char GlyphPropertiesfmt[]="niii";
const char GlyphSlotfmt[]="nii";

const char GuildPerkSpellsfmt[]="dii";
const char Holidaysfmt[]="niiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiixxsiix";
const char ImportPriceArmorfmt[]="nffff";
const char ImportPriceQualityfmt[]="nf";
const char ImportPriceShieldfmt[]="nf";
const char ImportPriceWeaponfmt[]="nf";
const char ItemPriceBasefmt[]="diff";
const char ItemReforgefmt[]="nifif";
const char ItemBagFamilyfmt[]="nx";
const char ItemClassfmt[]="dixxfx";
const char ItemDisenchantLootfmt[]="niiiiii";
//const char ItemDisplayTemplateEntryfmt[]="nxxxxxxxxxxixxxxxxxxxxx";
const char ItemLimitCategoryEntryfmt[]="nxii";
const char ItemRandomPropertiesfmt[]="nxiiixxs";
const char ItemRandomSuffixfmt[]="nsxiiiiiiiiii";
const char ItemSetEntryfmt[]="dsiiiiiiiiiixxxxxxxiiiiiiiiiiiiiiiiii";
const char LFGDungeonEntryfmt[]="nxiiiiiiixixxixixxxxx";
const char LiquidTypefmt[]="nxxixixxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
const char LockEntryfmt[]="niiiiiiiiiiiiiiiiiiiiiiiixxxxxxxx";
const char PhaseEntryfmt[]="nsi";
const char MailTemplateEntryfmt[]="nxs";
const char MapEntryfmt[]="nxixxxsixxixiffxiixi";
const char MapDifficultyEntryfmt[]="diisiix";
const char MovieEntryfmt[]="nxxx";
const char MountCapabilityfmt[]="niiiiiii";
const char MountTypefmt[]="niiiiiiiiiiiiiiiiiiiiiiii";
const char NameGenfmt[] = "dsii";
const char NumTalentsAtLevelfmt[]="df";
const char OverrideSpellDatafmt[]="niiiiiiiiiixx";
const char QuestSortEntryfmt[]="nx";
const char QuestXPfmt[]="niiiiiiiiii";
const char QuestFactionRewardfmt[]="niiiiiiiiii";
const char PvPDifficultyfmt[]="diiiii";
const char RandomPropertiesPointsfmt[]="niiiiiiiiiiiiiii";
const char ScalingStatDistributionfmt[]="niiiiiiiiiiiiiiiiiiiixi";
const char ScalingStatValuesfmt[]="iniiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii";
const char SoundEntriesfmt[]="nxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
const char StableSlotPricesfmt[] = "ni";
const char SummonPropertiesfmt[] = "niiiii";
const char TalentEntryfmt[]="niiiiiiiiixxixxxxxx";
const char TalentTabEntryfmt[]="nxxiiixxxxx";
const char TalentTreePrimarySpellsfmt[]="diix";
const char TaxiNodesEntryfmt[]="nifffsiixxx";
const char TaxiPathEntryfmt[]="niii";
const char TaxiPathNodeEntryfmt[]="diiifffiiii";
const char TeamContributionPointsfmt[]="df";
const char TotemCategoryEntryfmt[]="nxii";
const char VehicleEntryfmt[]="niffffiiiiiiiifffffffffffffffssssfifiixx";
const char VehicleSeatEntryfmt[]="niiffffffffffiiiiiifffffffiiifffiiiiiiiffiiiiixxxxxxxxxxxxxxxxxxxx";
const char WMOAreaTableEntryfmt[]="niiixxxxxiixxxx";
const char WorldMapAreaEntryfmt[]="xinxffffixxxxx";
const char WorldMapOverlayEntryfmt[]="nxiiiixxxxxxxxx";
const char WorldSafeLocsEntryfmt[]="nifffx";
         */

    }
}
