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
using WorldServer.Game.Managers;

namespace WorldServer.Game.WorldEntities
{
    public class Bag : Item
    {
        public Bag()
        {
            objectTypeMask |= HighGuidMask.Container;
            objectTypeId = ObjectType.Container;

            SetValuesCount((int)ContainerFields.End);
        }

        bool Create(uint guidlow, uint itemid, Player owner)
        {
            var itemProto = Cypher.ObjMgr.GetItemTemplate(itemid);

            if (itemProto == null || itemProto.ContainerSlots > ItemConst.MaxBagSize)
                return false;
            CreateGuid(guidlow, 0, HighGuidType.Container);

            SetValue<uint>(ObjectFields.Entry, itemid);
            SetValue<float>(ObjectFields.Scale, 1.0f);

            SetValue<ulong>(ItemFields.Owner, owner != null ? owner.GetGUID() : 0);
            SetValue<ulong>(ItemFields.ContainedIn, owner != null ? owner.GetGUID() : 0);

            SetValue<uint>(ItemFields.MaxDurability, itemProto.MaxDurability);
            SetValue<uint>(ItemFields.Durability, itemProto.MaxDurability);
            SetValue<uint>(ItemFields.StackCount, 1);

            // Setting the number of Slots the Container has
            SetValue<uint>(ContainerFields.NumSlots, itemProto.ContainerSlots);

            // Cleaning 20 slots
            BagSlot = new Item[ItemConst.MaxBagSize];
            return true;
        }

        //Gets
        public uint GetBagSize() { return GetValue<uint>(ContainerFields.NumSlots); }
        public bool IsEmpty()
        {
            for (var i = 0; i < GetBagSize(); ++i)
                if (BagSlot[i] != null)
                    return false;

            return true;
        }
        public uint GetItemCount(uint item, Item eItem)
        {
            Item pItem;
            uint count = 0;
            for (var i = 0; i < GetBagSize(); ++i)
            {
                pItem = BagSlot[i];
                if (pItem != null && pItem != eItem && pItem.GetEntry() == item)
                    count += pItem.GetStackCount();
            }

            if (eItem != null && eItem.GetTemplate().GemProperties != 0)
            {
                for (var i = 0; i < GetBagSize(); ++i)
                {
                    pItem = BagSlot[i];
                    if (pItem != null && pItem != eItem && pItem.GetTemplate().Socket[0].Color != 0)
                        count += pItem.GetGemCountWithID(item);
                }
            }

            return count;
        }
        public Item GetItemByPos(byte slot)
        {
            if (slot < GetBagSize())
                return BagSlot[slot];

            return null;
        }

        public void RemoveItem(byte slot, bool update)
        {
            if (BagSlot[slot] != null)
                BagSlot[slot].SetContainer(null);
            
            BagSlot[slot] = null;
            SetValue<ulong>(ContainerFields.Slot + (slot * 2), 0);
        }
        public void StoreItem(byte slot, Item pItem, bool update)
        {
            if (pItem != null && pItem.GetGUID() != GetGUID())
            {
                BagSlot[slot] = pItem;
                SetValue<ulong>(ContainerFields.Slot + (slot * 2), pItem.GetGUID());
                pItem.SetValue<ulong>(ItemFields.ContainedIn, GetGUID());
                pItem.SetValue<ulong>(ItemFields.Owner, GetOwnerGUID());
                pItem.SetContainer(this);
                pItem.SetSlot(slot);
            }
        }


        //DB
        public bool LoadFromDB(uint guid, ulong owner_guid, uint entry)
        {
            if (!LoadFromDB(guid, owner_guid, entry))
                return false;

            ItemTemplate itemProto = GetTemplate(); // checked in Item::LoadFromDB
            SetValue<uint>(ContainerFields.NumSlots, itemProto.ContainerSlots);
            // cleanup bag content related item value fields (its will be filled correctly from `character_inventory`)
            for (byte i = 0; i < ItemConst.MaxBagSize; ++i)
            {
                SetValue<ulong>(ContainerFields.Slot + (i * 2), 0);
                BagSlot[i] = null;
            }
            return true;
        }


        #region Fields
        Item[] BagSlot = new Item[36];
        #endregion
    }
}
