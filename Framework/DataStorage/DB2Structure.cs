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

using System.Runtime.InteropServices;
using Framework.Constants;

namespace Framework.DataStorage
{
    public struct Db2Header
    {
        public int Signature;
        public int RecordsCount;
        public int FieldsCount;
        public int RecordSize;
        public int StringTableSize;

        public bool IsDB2
        {
            get { return Signature == 0x32424457; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ItemEntry
    {
        public uint Id;                                             // 0
        public uint Class;                                          // 1
        public uint SubClass;                                       // 2
        public int SoundOverrideSubclass;                                           // 3
        public int Material;                                       // 4
        public uint DisplayId;                                      // 5
        public uint inventoryType;                         // 6
        public uint Sheath;                                         // 7
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ItemCurrencyCostEntry
    {
        //uint32  Id;
        public uint ItemId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ItemSparse
    {
        public uint Id;                                           // 0
        public uint Quality;                                      // 1
        public uint Flags;                                        // 2
        public uint Flags2;                                       // 3
        public uint Unk510;
        public float Unk430_1;
        public float Unk430_2;
        public uint BuyCount;
        public uint BuyPrice;                                     // 4
        public uint SellPrice;                                    // 5
        public uint inventoryType;                                // 6
        public int AllowableClass;                               // 7
        public int AllowableRace;                                // 8
        public uint ItemLevel;                                    // 9
        public int RequiredLevel;                                // 10
        public uint RequiredSkill;                                // 11
        public uint RequiredSkillRank;                            // 12
        public uint RequiredSpell;                                // 13
        public uint RequiredHonorRank;                            // 14
        public uint RequiredCityRank;                             // 15
        public uint RequiredReputationFaction;                    // 16
        public uint RequiredReputationRank;                       // 17
        public uint MaxCount;                                     // 18
        public uint Stackable;                                    // 19
        public uint ContainerSlots;                               // 20
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxStats)]
        public int[] ItemStatType;           // 21 - 30
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxStats)]
        public int[] ItemStatValue;          // 31 - 40
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxStats)]
        public int[] ItemStatUnk1;           // 41 - 50
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxStats)]
        public int[] ItemStatUnk2;           // 51 - 60
        public uint ScalingStatDistribution;                      // 61
        public uint DamageType;                                   // 62
        public uint Delay;                                        // 63
        public float RangedModRange;                               // 64
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxSpells)]
        public int[] SpellId;               // 65 - 69
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxSpells)]
        public int[] SpellTrigger;          // 70 - 74
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxSpells)]
        public int[] SpellCharges;          // 75 - 79
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxSpells)]
        public int[] SpellCooldown;         // 80 - 84
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxSpells)]
        public int[] SpellCategory;         // 85 - 89
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxSpells)]
        public int[] SpellCategoryCooldown; // 90 - 94
        public uint Bonding;                                      // 95
        public uint _name;                                         // 96
        //char*      Name2;                                        // 97
        //char*      Name3;                                        // 98
        //char*      Name4;                                        // 99
        public uint _description;                                  // 100
        public uint PageText;                                     // 101
        public uint LanguageID;                                   // 102
        public uint PageMaterial;                                 // 103
        public uint StartQuest;                                   // 104
        public uint LockID;                                       // 105
        public int Material;                                     // 106
        public uint Sheath;                                       // 107
        public int RandomProperty;                               // 108
        public uint RandomSuffix;                                 // 109
        public uint ItemSet;                                      // 110
        public uint Area;                                         // 112
        public uint Map;                                          // 113
        public uint BagFamily;                                    // 114
        public uint TotemCategory;                                // 115
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxSockets)]
        public uint[] Color;                // 116 - 118
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxSockets)]
        public uint[] Content;              // 119 - 121
        public int SocketBonus;                                  // 122
        public uint GemProperties;                                // 123
        public float ArmorDamageModifier;                          // 124
        public uint Duration;                                     // 125
        public uint ItemLimitCategory;                            // 126
        public uint HolidayId;                                    // 127
        public float StatScalingFactor;                            // 128
        public int CurrencySubstitutionId;                       // 129
        public int CurrencySubstitutionCount;                    // 130

        /// <summary>
        /// Return current Name
        /// </summary>
        public string Name
        {
            get { return DB2Storage.ItemStrings[_name]; }
        }

        /// <summary>
        /// Return current Description
        /// </summary>
        public string Description
        {
            get { return DB2Storage.ItemStrings[_description]; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ItemExtendedCostEntry
    {
        public uint ID;                                         // 0 extended-cost entry id
        //uint32    reqhonorpoints;                             // 1 required honor points
        //uint32    reqarenapoints;                             // 2 required arena points
        public uint RequiredArenaSlot;                          // 3 arena slot restrictions (min slot value)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxExtCostItems)]
        public uint[] RequiredItem;      // 4-8 required item id
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxExtCostItems)]
        public uint[] RequiredItemCount; // 9-13 required count of 1st item
        public uint RequiredPersonalArenaRating;                // 14 required personal arena rating
        //uint32    ItemPurchaseGroup;                          // 15
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxExtCostCurrencies)]
        public uint[] RequiredCurrency;// 16-20 required curency id
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ItemConst.MaxExtCostCurrencies)]
        public uint[] RequiredCurrencyCount;// 21-25 required curency count
        //uint32    Unknown[5];                               // 26-30
    }
}
