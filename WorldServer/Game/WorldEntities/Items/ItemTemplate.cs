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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Framework.Constants;

namespace WorldServer.Game.WorldEntities
{
    public class ItemTemplate
    {
        public uint GetMaxStackSize()
        {
            return (Stackable == 2147483647 || Stackable <= 0) ? (uint)0x7FFFFFFF - 1 : Stackable;
        }


        #region Fields
        public uint ItemId { get; set; }
        public ItemClass Class { get; set; }                                           // id from ItemClass.dbc
        public uint SubClass { get; set; }                                        // id from ItemSubClass.dbc
        public int SoundOverrideSubclass { get; set; }                           // < 0: id from ItemSubClass.dbc, used to override weapon sound from actual SubClass
        public string Name1 { get; set; }
        public uint DisplayInfoID { get; set; }                                   // id from ItemDisplayInfo.dbc
        public ItemQuality Quality { get; set; }
        public ItemFlags Flags { get; set; }
        public ItemFlags2 Flags2 { get; set; }
        public float Unk430_1 { get; set; }
        public float Unk430_2 { get; set; }
        public uint BuyCount { get; set; }
        public uint BuyPrice { get; set; }
        public uint SellPrice { get; set; }
        public InventoryType inventoryType { get; set; }
        public int AllowableClass { get; set; }
        public int AllowableRace { get; set; }
        public uint ItemLevel { get; set; }
        public int RequiredLevel { get; set; }
        public uint RequiredSkill { get; set; }                                   // id from SkillLine.dbc
        public uint RequiredSkillRank { get; set; }
        public uint RequiredSpell { get; set; }                                   // id from Spell.dbc
        public uint RequiredHonorRank { get; set; }
        public uint RequiredCityRank { get; set; }
        public uint RequiredReputationFaction { get; set; }                       // id from Faction.dbc
        public uint RequiredReputationRank { get; set; }
        public uint MaxCount { get; set; }                                        // <= 0: no limit
        public uint Stackable { get; set; }                                       // 0: not allowed, -1: put in player coin info tab and don't limit stacking (so 1 slot)
        public uint ContainerSlots { get; set; }
        public itemstat[] ItemStat = new itemstat[ItemConst.MaxStats];
        public uint ScalingStatDistribution { get; set; }                         // id from ScalingStatDistribution.dbc
        public uint DamageType { get; set; }                                      // id from Resistances.dbc
        public uint Delay { get; set; }
        public float RangedModRange { get; set; }
        public ItemSpell[] Spells = new ItemSpell[ItemConst.MaxSpells];
        public ItemBondingType Bonding { get; set; }
        public string Description { get; set; }
        public uint PageText { get; set; }
        public uint LanguageID { get; set; }
        public uint PageMaterial { get; set; }
        public uint StartQuest { get; set; }                                      // id from QuestCache.wdb
        public uint LockID { get; set; }
        public int Material { get; set; }                                        // id from Material.dbc
        public uint Sheath { get; set; }
        public int RandomProperty { get; set; }                                  // id from ItemRandomProperties.dbc
        public uint RandomSuffix { get; set; }                                    // id from ItemRandomSuffix.dbc
        public uint ItemSet { get; set; }                                         // id from ItemSet.dbc
        public uint MaxDurability { get; set; }
        public uint Area { get; set; }                                            // id from AreaTable.dbc
        public uint Map { get; set; }                                             // id from Map.dbc
        public BagFamilyMask BagFamily { get; set; }                                       // bit mask (1 << id from ItemBagFamily.dbc)
        public uint TotemCategory { get; set; }                                   // id from TotemCategory.dbc
        public ItemSocket[] Socket = new ItemSocket[ItemConst.MaxSockets];
        public int socketBonus { get; set; }                                     // id from SpellItemEnchantment.dbc
        public uint GemProperties { get; set; }                                   // id from GemProperties.dbc
        public float ArmorDamageModifier { get; set; }
        public uint Duration { get; set; }
        public uint ItemLimitCategory { get; set; }                               // id from ItemLimitCategory.dbc
        public uint HolidayId { get; set; }                                       // id from Holidays.dbc
        public float StatScalingFactor { get; set; }
        public int CurrencySubstitutionId { get; set; }                          // May be used instead of a currency
        public int CurrencySubstitutionCount { get; set; }

        // extra fields, not part of db2 files
        public float DamageMin { get; set; }
        public float DamageMax { get; set; }
        public float DPS { get; set; }
        public uint Armor { get; set; }
        public float SpellPPMRate { get; set; }
        public uint ScriptId { get; set; }
        public uint DisenchantID { get; set; }
        public uint RequiredDisenchantSkill { get; set; }
        public uint FoodType { get; set; }
        public uint MinMoneyLoot { get; set; }
        public uint MaxMoneyLoot { get; set; }
        public uint FlagsCu { get; set; }
        #endregion
        public struct itemstat
        {
            public int ItemStatType;
            public int ItemStatValue;
            public int ItemStatUnk1;
            public int ItemStatUnk2;
        }
        public struct ItemSpell
        {
            public int SpellId;                                         // id from Spell.dbc
            public int SpellTrigger;
            public int SpellCharges;
            public int SpellCooldown;
            public int SpellCategory;                                   // id from SpellCategory.dbc
            public int SpellCategoryCooldown;
        }
        public struct ItemSocket
        {
            public uint Color;
            public uint Content;
        }
    }
}
