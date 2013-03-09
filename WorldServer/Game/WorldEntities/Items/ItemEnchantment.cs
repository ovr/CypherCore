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
using Framework.Logging;
using Framework.Database;
using Framework.Utility;
using WorldServer.Game.Managers;
using Framework.DataStorage;
using Framework.Constants;

namespace WorldServer.Game.WorldEntities
{
    public class ItemEnchantment
    {
        public class EnchStoreItem
        {
            public uint ench;
            public float chance;

            public EnchStoreItem()
            {
                ench = 0;
                chance = 0;
            }
            public EnchStoreItem(uint _ench, float _chance)
            {
                ench = _ench;
                chance = _chance;
            }
        }

        //typedef std::vector<EnchStoreItem> EnchStoreList;
        //typedef UNORDERED_MAP<uint32, EnchStoreList> EnchantmentStore;

        static Dictionary<uint, EnchStoreItem> RandomItemEnch = new Dictionary<uint, EnchStoreItem>();

        //static EnchantmentStore RandomItemEnch;

        public static void LoadRandomEnchantmentsTable()
        {
            RandomItemEnch.Clear();                                 // for reload case

            //                                         0      1     2
            SQLResult result = DB.World.Select("SELECT entry, ench, chance FROM item_enchantment_template");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 Item Enchantment definitions. DB table `item_enchantment_template` is empty.");
            }
            uint count = 0;

            for (int i = 0; i < result.Count; i++)
            {
                uint entry = result.Read<uint>(i, 0);
                uint ench = result.Read<uint>(i, 1);
                float chance = result.Read<float>(i, 2);

                if (chance > 0.000001f && chance <= 100.0f)
                    RandomItemEnch.Add(entry, new EnchStoreItem(ench, chance));

                ++count;
            }

            Log.outInfo("Loaded {0} Item Enchantment definitions", count);
        }

        public static uint GetItemEnchantMod(int entry)
        {
            if (entry == 0)
                return 0;

            if (entry == -1)
                return 0;

            var tab = RandomItemEnch.LookupByKey((uint)entry);
            if (tab == null)
            {
                Log.outError("Item RandomProperty / RandomSuffix id #{0} used in `item_template` but it does not have records in `item_enchantment_template` table.", entry);
                return 0;
            }

            //double dRoll = rand_chance();
            //float fCount = 0;

            //for (EnchStoreList::const_iterator ench_iter = tab->second.begin(); ench_iter != tab->second.end(); ++ench_iter)
            {
                //fCount += ench_iter.chance;

                //if (fCount > dRoll)
                //return ench_iter.ench;
            }

            //we could get here only if sum of all enchantment chances is lower than 100%
            // dRoll = (irand(0, (int)floor(fCount * 100) + 1)) / 100;
            //fCount = 0;

            //for (EnchStoreList::const_iterator ench_iter = tab->second.begin(); ench_iter != tab->second.end(); ++ench_iter)
            {
                //fCount += ench_iter->chance;

                //if (fCount > dRoll)
                // return ench_iter->ench;
            }

            return 0;
        }

        public static uint GenerateEnchSuffixFactor(uint item_id)
        {
            ItemTemplate itemProto = Cypher.ObjMgr.GetItemTemplate(item_id);

            if (itemProto == null)
                return 0;
            if (itemProto.RandomSuffix == 0)
                return 0;

            RandomPropertiesPointsEntry randomProperty = DBCStorage.RandomPropertiesPointsStorage.LookupByKey(itemProto.ItemLevel);
            if (randomProperty == null)
                return 0;

            uint suffixFactor;
            switch (itemProto.inventoryType)
            {
                // Items of that type don`t have points
                case InventoryType.NonEquip:
                case InventoryType.Bag:
                case InventoryType.Tabard:
                case InventoryType.Ammo:
                case InventoryType.Quiver:
                case InventoryType.Relic:
                    return 0;
                // Select point coefficient
                case InventoryType.Head:
                case InventoryType.Body:
                case InventoryType.Chest:
                case InventoryType.Legs:
                case InventoryType.Weapon2Hand:
                case InventoryType.Robe:
                    suffixFactor = 0;
                    break;
                case InventoryType.Shoulders:
                case InventoryType.Waist:
                case InventoryType.Feet:
                case InventoryType.Hands:
                case InventoryType.Trinket:
                    suffixFactor = 1;
                    break;
                case InventoryType.Neck:
                case InventoryType.Wrists:
                case InventoryType.Finger:
                case InventoryType.Shield:
                case InventoryType.Cloak:
                case InventoryType.Holdable:
                    suffixFactor = 2;
                    break;
                case InventoryType.Weapon:
                case InventoryType.WeaponMainhand:
                case InventoryType.WeaponOffhand:
                    suffixFactor = 3;
                    break;
                case InventoryType.Ranged:
                case InventoryType.Thrown:
                case InventoryType.RangedRight:
                    suffixFactor = 4;
                    break;
                default:
                    return 0;
            }
            // Select rare/epic modifier
            switch (itemProto.Quality)
            {
                case ItemQuality.Uncommon:
                    return randomProperty.UncommonPropertiesPoints[suffixFactor];
                case ItemQuality.Rare:
                    return randomProperty.RarePropertiesPoints[suffixFactor];
                case ItemQuality.Epic:
                    return randomProperty.EpicPropertiesPoints[suffixFactor];
                case ItemQuality.Legendary:
                case ItemQuality.Artifact:
                    return 0;                                       // not have random properties
                default:
                    break;
            }
            return 0;
        }
    }
}
