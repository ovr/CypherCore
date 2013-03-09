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

using System.Linq;
using System;
using System.Text;
using System.Collections.Generic;
using Framework.Database;
using Framework.Constants;
using Framework.DataStorage;
using WorldServer.Game.Managers;
using Framework.Utility;
using Framework.Logging;
using Framework.ObjectDefines;

namespace WorldServer.Game.WorldEntities
{
    public class Item : WorldObject
    {
        public Item() : base(false)
        {
            objectTypeMask |= HighGuidMask.Item;
            objectTypeId = ObjectType.Item;

            //m_updateFlag = 0;

            SetValuesCount((int)ItemFields.End);
            Slot = 0;
            uState = ItemUpdateState.New;
            uQueuePos = -1;
            Container = null;
            LootGenerated = false;
            //mb_in_trade = false;
            //m_lastPlayedTimeUpdate = time(NULL);

            RefundRecipient = 0;
            PaidMoney = 0;
            PaidExtendedCost = 0;
        }

        bool Create(uint guidlow, uint itemid, Player owner)
        {
            CreateGuid(guidlow, 0, HighGuidType.Item);

            SetEntry(itemid);
            SetObjectScale(1.0f);

            SetValue<ulong>(ItemFields.Owner, owner != null ? owner.GetGUID() : 0);
            SetValue<ulong>(ItemFields.ContainedIn, owner != null ? owner.GetGUID() : 0);

            ItemTemplate itemProto = Cypher.ObjMgr.GetItemTemplate(itemid);
            if (itemProto == null)
                return false;

            SetValue<uint>(ItemFields.StackCount, 1);
            SetValue<uint>(ItemFields.MaxDurability, itemProto.MaxDurability);
            SetValue<uint>(ItemFields.Durability, itemProto.MaxDurability);

            //for (var i = 0; i < Defines.MaxItemSpells; ++i)
            //SetSpellCharges(i, itemProto->Spells[i].SpellCharges);

            SetValue<uint>(ItemFields.Expiration, itemProto.Duration);
            SetValue<uint>(ItemFields.CreatePlayedTime, 0);
            return true;
        }

        //Static
        public static Item CreateItem(uint item, uint count, Player player)
        {
            if (count < 1)
                return null;                                        //don't create item at zero count

            var pProto = Cypher.ObjMgr.GetItemTemplate(item);
            if (pProto != null)
            {
                if (count > pProto.GetMaxStackSize())
                    count = pProto.GetMaxStackSize();

                Item pItem = Cypher.ItemMgr.NewItemOrBag(pProto);
                if (pItem.Create(Cypher.ObjMgr.GenerateLowGuid(HighGuidType.Item), item, player))
                {
                    pItem.SetStackCount(count);
                    return pItem;
                }
            }

            return null;
        }
        public static bool ItemCanGoIntoBag(ItemTemplate pProto, ItemTemplate pBagProto)
        {
            if (pProto == null || pBagProto == null)
                return false;

            switch (pBagProto.Class)
            {
                case ItemClass.Container:
                    switch ((ItemSubClassContainer)pBagProto.SubClass)
                    {
                        case ItemSubClassContainer.Container:
                            return true;
                        case ItemSubClassContainer.SoulContainer:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.SOUL_SHARDS))
                                return false;
                            return true;
                        case ItemSubClassContainer.HerbContainer:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.HERBS))
                                return false;
                            return true;
                        case ItemSubClassContainer.EnchantingContainer:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.ENCHANTING_SUPP))
                                return false;
                            return true;
                        case ItemSubClassContainer.MiningContainer:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.MINING_SUPP))
                                return false;
                            return true;
                        case ItemSubClassContainer.EngineeringContainer:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.ENGINEERING_SUPP))
                                return false;
                            return true;
                        case ItemSubClassContainer.GemContainer:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.GEMS))
                                return false;
                            return true;
                        case ItemSubClassContainer.LeatherworkingContainer:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.LEATHERWORKING_SUPP))
                                return false;
                            return true;
                        case ItemSubClassContainer.InscriptionContainer:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.INSCRIPTION_SUPP))
                                return false;
                            return true;
                        case ItemSubClassContainer.TackleContainer:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.FISHING_SUPP))
                                return false;
                            return true;
                        default:
                            return false;
                    }
                //can remove?
                case ItemClass.Quiver:
                    switch ((ItemSubClassQuiver)pBagProto.SubClass)
                    {
                        case ItemSubClassQuiver.Quiver:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.ARROWS))
                                return false;
                            return true;
                        case ItemSubClassQuiver.AmmoPouch:
                            if (!Convert.ToBoolean(pProto.BagFamily & BagFamilyMask.BULLETS))
                                return false;
                            return true;
                        default:
                            return false;
                    }
            }
            return false;
        }
        public void DeleteFromInventoryDB()
        {
            DeleteFromInventoryDB(GetGUIDLow());
        }
        void DeleteFromInventoryDB(uint itemGuid)
        {
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_CHAR_INVENTORY_BY_ITEM);
            stmt.AddValue(0, itemGuid);
            DB.Characters.Execute(stmt);
        }

        bool IsInUpdateQueue() { return uQueuePos != -1; }
        int GetQueuePos() { return uQueuePos; }
        public void FSetState(ItemUpdateState state)// forced
        {
            uState = state;
        }
        void AddToUpdateQueueOf(Player player)
        {
            if (IsInUpdateQueue())
                return;

            //ASSERT(player != NULL);

            if (player.GetGUID() != GetOwnerGUID())
            {
                Log.outError("Item->AddToUpdateQueueOf - Owner's guid ({0}) and player's guid ({1}) don't match!", GetOwnerGUID(), player.GetGUIDLow());
                return;
            }

            if (player.m_itemUpdateQueueBlocked)
                return;

            player.ItemUpdateQueue.Add(this);
            uQueuePos = player.ItemUpdateQueue.IndexOf(this);
        }
        public void RemoveFromUpdateQueueOf(Player player)
        {
            if (!IsInUpdateQueue())
                return;

            //ASSERT(player != NULL)

            if (player.GetGUID() != GetOwnerGUID())
            {
                Log.outError("Item->RemoveFromUpdateQueueOf - Owner's guid ({0}) and player's guid ({1}) don't match!", ObjectGuid.GuidLowPart(GetOwnerGUID()), player.GetGUIDLow());
                return;
            }

            if (player.m_itemUpdateQueueBlocked)
                return;

            player.ItemUpdateQueue[uQueuePos] = null;
            uQueuePos = -1;
        }

        public Item CloneItem(uint count, Player player)
        {
            Item newItem = CreateItem(GetEntry(), count, player);
            if (newItem == null)
                return null;

            newItem.SetValue<uint>(ItemFields.Creator, GetValue<uint>(ItemFields.Creator));
            newItem.SetValue<uint>(ItemFields.GiftCreator, GetValue<uint>(ItemFields.GiftCreator));
            newItem.SetValue<uint>(ItemFields.Flags, GetValue<uint>(ItemFields.Flags));
            newItem.SetValue<uint>(ItemFields.Expiration, GetValue<uint>(ItemFields.Expiration));
            // player CAN be NULL in which case we must not update random properties because that accesses player's item update queue
            //if (player != null)
            //newItem.SetItemRandomProperties(GetItemRandomPropertyId());
            return newItem;
        }

        //Gets
        public ItemUpdateState GetState() { return uState; }
        public byte GetSlot() { return Slot; }
        public Bag GetContainer() { return Container; }
        public ItemTemplate GetTemplate()
        {
            return Cypher.ObjMgr.GetItemTemplate(GetEntry());
        }
        public ulong GetOwnerGUID() { return GetValue<ulong>(ItemFields.Owner); }
        public ushort GetPos() { return (ushort)(GetBagSlot() << 8 | GetSlot()); }
        public byte GetBagSlot() { return Container != null ? Container.GetSlot() : InventorySlots.Bag0; }
        public bool IsBag() { return GetTemplate().inventoryType == InventoryType.Bag; }
        public uint GetPlayedTime()
        {
            DateTime curtime = DateTime.Now;
            //uint elapsed = uint32(curtime - m_lastPlayedTimeUpdate);
            //return GetUInt32Value(ITEM_FIELD_CREATE_PLAYED_TIME) + elapsed;
            return 0;
        }
        public uint GetSkill()
        {
            Skill[] item_weapon_skills = new Skill[]
            {
                Skill.Axes,     Skill.Axes2h,  Skill.Bows,          Skill.Guns,      Skill.Maces,
                Skill.Maces2h, Skill.Polearms, Skill.Swords,        Skill.Swords2h, 0,
                Skill.Staves,   0,              0,                   Skill.FistWeapons,   0,
                Skill.Daggers,  Skill.Thrown,   Skill.Assassination, Skill.Crossbows, Skill.Wands,
                Skill.Fishing
            };

            Skill[] item_armor_skills = new Skill[]
            {
                0, Skill.Cloth, Skill.Leather, Skill.Mail, Skill.PlateMail, 0, Skill.Shield, 0, 0, 0, 0
            };

            ItemTemplate proto = GetTemplate();

            switch (proto.Class)
            {
                case ItemClass.Weapon:
                    if (proto.SubClass >= (uint)ItemSubClassWeapon.Max)
                        return 0;
                    else
                        return (uint)item_weapon_skills[proto.SubClass];

                case ItemClass.Armor:
                    if (proto.SubClass >= (uint)ItemSubClassArmor.Max)
                        return 0;
                    else
                        return (uint)item_armor_skills[proto.SubClass];

                default:
                    return 0;
            }
        }
        public uint GetEnchantmentId(EnchantmentSlot slot) { return GetValue<uint>(ItemFields.Enchantment + (int)slot * (int)EnchantmentOffset.Max + (int)EnchantmentOffset.Id); }
        uint GetEnchantmentDuration(EnchantmentSlot slot) { return GetValue<uint>(ItemFields.Enchantment + (int)slot * (int)EnchantmentOffset.Max + (int)EnchantmentOffset.Duration); }
        uint GetEnchantmentCharges(EnchantmentSlot slot) { return GetValue<uint>(ItemFields.Enchantment + (int)slot * (int)EnchantmentOffset.Max + (int)EnchantmentOffset.Charges); }
        bool IsInBag() { return Container != null; }
        public bool IsEquipped() { return !IsInBag() && Slot < (byte)EquipmentSlot.End; }
        public byte GetGemCountWithID(uint GemID)
        {
            byte count = 0;
            for (var enchant_slot = EnchantmentSlot.SOCK_ENCHANTMENT_SLOT; enchant_slot < EnchantmentSlot.SOCK_ENCHANTMENT_SLOT + ItemConst.MaxSockets; enchant_slot++)
            {
                uint enchant_id = GetEnchantmentId(enchant_slot);
                if (enchant_id == 0)
                    continue;

                SpellItemEnchantmentEntry enchantEntry = DBCStorage.SpellItemEnchantmentStorage.LookupByKey(enchant_id);
                if (enchantEntry == null)
                    continue;

                if (GemID == enchantEntry.GemID)
                    ++count;
            }
            return count;
        }
        public byte GetGemCountWithLimitCategory(uint limitCategory)
        {
            byte count = 0;
            for (var enchant_slot = EnchantmentSlot.SOCK_ENCHANTMENT_SLOT; enchant_slot < EnchantmentSlot.SOCK_ENCHANTMENT_SLOT + ItemConst.MaxSockets; enchant_slot++)
            {
                uint enchant_id = GetEnchantmentId(enchant_slot);
                if (enchant_id == 0)
                    continue;

                SpellItemEnchantmentEntry enchantEntry = DBCStorage.SpellItemEnchantmentStorage.LookupByKey(enchant_id);
                if (enchantEntry == null)
                    continue;

                ItemTemplate gemProto = Cypher.ObjMgr.GetItemTemplate(enchantEntry.GemID);
                if (gemProto == null)
                    continue;

                if (gemProto.ItemLimitCategory == limitCategory)
                    count++;
            }
            return count;
        }
        public bool IsSoulBound() { return HasFlag((int)ItemFields.Flags, ItemFieldFlags.Soulbound); }
        bool IsBoundAccountWide() { return (GetTemplate().Flags & ItemFlags.BindToAccount) != 0; }
        public bool IsBindedNotWith(Player player)
        {
            // not binded item
            if (!IsSoulBound())
                return false;

            // own item
            if (GetOwnerGUID() == player.GetGUID())
                return false;

            //if (HasFlag((int)ItemFields.Flags, ItemFieldFlags.BopTradeable))
            //if (allowedGUIDs.find(player->GetGUIDLow()) != allowedGUIDs.end())
            //return false;

            // BOA item case
            if (IsBoundAccountWide())
                return false;

            return true;
        }
        public bool IsNotEmptyBag()
        {
            Bag bag = ToBag();
            if (bag != null)
                return !bag.IsEmpty();

            return false;
        }
        public uint GetVisibleEntry()
        {
            //if (uint32 transmogrification = GetEnchantmentId(TRANSMOGRIFY_ENCHANTMENT_SLOT))
            //return transmogrification;
            return GetEntry();
        }
        public uint GetStackCount() { return GetValue<uint>(ItemFields.StackCount); }
        uint GetRefundRecipient() { return RefundRecipient; }
        uint GetPaidMoney() { return PaidMoney; }
        uint GetPaidExtendedCost() { return PaidExtendedCost; }


        //Sets
        public void SetSlot(byte slot) { Slot = slot; }
        public void SetContainer(Bag container) { Container = container; }
        public void SetOwnerGUID(ulong guid) { SetValue<ulong>(ItemFields.Owner, guid); }
        public void SetState(ItemUpdateState state, Player forplayer = null)
        {
            if (uState == ItemUpdateState.New && state == ItemUpdateState.Removed)
            {
                // pretend the item never existed
                RemoveFromUpdateQueueOf(forplayer);
                //forplayer->DeleteRefundReference(GetGUIDLow());
                return;
            }
            if (state != ItemUpdateState.Unchanged)
            {
                // new items must stay in new state until saved
                if (uState != ItemUpdateState.New)
                    uState = state;

                AddToUpdateQueueOf(forplayer);
            }
            else
            {
                // unset in queue
                // the item must be removed from the queue manually
                uQueuePos = -1;
                uState = ItemUpdateState.Unchanged;
            }
        }
        public void SetBinding(bool val) { ApplyModFlag(ItemFields.Flags, (uint)ItemFieldFlags.Soulbound, val); }
        public void SetStackCount(uint value) { SetValue<uint>(ItemFields.StackCount, value); }
        void SetRefundRecipient(uint pGuidLow) { RefundRecipient = pGuidLow; }
        void SetPaidMoney(uint money) { PaidMoney = money; }
        void SetPaidExtendedCost(uint iece) { PaidExtendedCost = iece; }
        public void SetNotRefundable(Player owner, bool changestate = true)
        {
            if (!HasFlag(ItemFields.Flags, ItemFlags.Refundable))
                return;

            RemoveFlag(ItemFields.Flags, ItemFlags.Refundable);
            // Following is not applicable in the trading procedure
            if (changestate)
                SetState(ItemUpdateState.Changed, owner);

            SetRefundRecipient(0);
            SetPaidMoney(0);
            SetPaidExtendedCost(0);
            //DeleteRefundDataFromDB(trans);

            //owner.DeleteRefundReference(GetGUIDLow());
        }

        string GetText() { return m_text; }
        void SetText(string text) { m_text = text; }

        public int GetItemRandomPropertyId() { return GetValue<int>(ItemFields.RandomPropertiesID); }
        public uint GetItemSuffixFactor() { return GetValue<uint>(ItemFields.PropertySeed); }
        int GetSpellCharges(byte index/*0..5*/ = 0) { return GetValue<int>(ItemFields.SpellCharges + index); }
        void SetSpellCharges(byte index/*0..5*/, int value) { SetValue<int>(ItemFields.SpellCharges + index, value); }

        public Bag ToBag()
        {
            if (IsBag())
                return (this as Bag);
            else
                return null;
        }
        public void ClearSoulboundTradeable(Player currentOwner)
        {
            //RemoveFlag(ItemFields.Flags, ItemFlags.BopTradeable);
            //if (allowedGUIDs.empty())
            //return;

            //allowedGUIDs.clear();
            SetState(ItemUpdateState.Changed, currentOwner);
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_ITEM_BOP_TRADE);
            stmt.AddValue(0, GetGUIDLow());
            DB.Characters.Execute(stmt);
        }
        public InventoryResult CanBeMergedPartlyWith(ItemTemplate proto)
        {
            // not allow merge looting currently items
            if (LootGenerated)
                return InventoryResult.LootGone;

            // check item type
            if (GetEntry() != proto.ItemId)
                return InventoryResult.CantStack;

            // check free space (full stacks can't be target of merge
            if (GetStackCount() >= proto.GetMaxStackSize())
                return InventoryResult.CantStack;

            return InventoryResult.Ok;
        }
        void UpdateItemSuffixFactor()
        {
            uint suffixFactor = ItemEnchantment.GenerateEnchSuffixFactor(GetEntry());
            if (GetItemSuffixFactor() == suffixFactor)
                return;
            SetValue<uint>(ItemFields.PropertySeed, suffixFactor);
        }

        public static int GenerateItemRandomPropertyId(uint item_id)
        {
            ItemTemplate itemProto = Cypher.ObjMgr.GetItemTemplate(item_id);

            if (itemProto == null)
                return 0;

            // item must have one from this field values not null if it can have random enchantments
            if (itemProto.RandomProperty == 0 && itemProto.RandomSuffix == 0)
                return 0;

            // item can have not null only one from field values
            if (itemProto.RandomProperty != 0 && itemProto.RandomSuffix != 0)
            {
                Log.outError("Item template {0} have RandomProperty == {1} and RandomSuffix == {2}, but must have one from field =0", itemProto.ItemId, itemProto.RandomProperty, itemProto.RandomSuffix);
                return 0;
            }

            // RandomProperty case
            if (itemProto.RandomProperty != 0)
            {
                uint randomPropId = ItemEnchantment.GetItemEnchantMod((int)itemProto.RandomProperty);
                ItemRandomPropertiesEntry random_id = DBCStorage.ItemRandomPropertiesStorage.LookupByKey(randomPropId);
                if (random_id == null)
                {
                    Log.outError("Enchantment id #{0} used but it doesn't have records in 'ItemRandomProperties.dbc'", randomPropId);
                    return 0;
                }

                return (int)random_id.ID;
            }
            // RandomSuffix case
            else
            {
                uint randomPropId = ItemEnchantment.GetItemEnchantMod((int)itemProto.RandomSuffix);
                ItemRandomSuffixEntry random_id = DBCStorage.ItemRandomSuffixStorage.LookupByKey(randomPropId);
                if (random_id == null)
                {
                    Log.outError("Enchantment id #{0} used but it doesn't have records in sItemRandomSuffixStore.", randomPropId);
                    return 0;
                }

                return -(int)random_id.ID;
            }
        }

        //DB
        public void SaveToDB()
        {
            PreparedStatement stmt;
            uint guid = GetGUIDLow();
            switch (uState)
            {
                case ItemUpdateState.New:
                case ItemUpdateState.Changed:
                    {
                        byte index = 0;
                        stmt = DB.Characters.GetPreparedStatement(uState == ItemUpdateState.New ? CharStatements.CHAR_REP_ITEM_INSTANCE : CharStatements.CHAR_UPD_ITEM_INSTANCE);
                        stmt.AddValue(index, GetEntry());
                        stmt.AddValue(++index, ObjectGuid.GuidLowPart(GetOwnerGUID()));
                        stmt.AddValue(++index, ObjectGuid.GuidLowPart(GetValue<ulong>(ItemFields.Creator)));
                        stmt.AddValue(++index, ObjectGuid.GuidLowPart(GetValue<ulong>(ItemFields.GiftCreator)));
                        stmt.AddValue(++index, GetStackCount());
                        stmt.AddValue(++index, GetValue<uint>(ItemFields.Expiration));


                        StringBuilder ss = new StringBuilder();
                        for (byte i = 0; i < ItemConst.MaxSpells; ++i)
                            ss.AppendFormat("{0} ", GetSpellCharges(i));
                        stmt.AddValue(++index, ss.ToString());

                        stmt.AddValue(++index, GetValue<uint>(ItemFields.Flags));

                        ss.Clear();
                        for (var i = 0; i < (int)EnchantmentSlot.MAX_ENCHANTMENT_SLOT; ++i)
                        {
                            ss.AppendFormat("{0} ", GetEnchantmentId((EnchantmentSlot)i));
                            ss.AppendFormat("{0} ", GetEnchantmentDuration((EnchantmentSlot)i));
                            ss.AppendFormat("{0} ", GetEnchantmentCharges((EnchantmentSlot)i));
                        }
                        stmt.AddValue(++index, ss.ToString());

                        stmt.AddValue(++index, GetItemRandomPropertyId());
                        stmt.AddValue(++index, GetValue<uint>(ItemFields.Durability));
                        stmt.AddValue(++index, GetValue<uint>(ItemFields.CreatePlayedTime));
                        stmt.AddValue(++index, m_text);
                        stmt.AddValue(++index, guid);

                        DB.Characters.Execute(stmt);

                        if ((uState == ItemUpdateState.Changed) && HasFlag(ItemFields.Flags, ItemFieldFlags.Wrapped))
                        {
                            stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_UPD_GIFT_OWNER);
                            stmt.AddValue(0, ObjectGuid.GuidLowPart(GetOwnerGUID()));
                            stmt.AddValue(1, guid);
                            DB.Characters.Execute(stmt);
                        }
                        break;
                    }
                case ItemUpdateState.Removed:
                    {
                        stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_ITEM_INSTANCE);
                        stmt.AddValue(0, guid);
                        DB.Characters.Execute(stmt);

                        if (HasFlag(ItemFields.Flags, ItemFieldFlags.Wrapped))
                        {
                            stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_GIFT);
                            stmt.AddValue(0, guid);
                            DB.Characters.Execute(stmt);
                        }
                        return;
                    }
                case ItemUpdateState.Unchanged:
                    break;
            }

            SetState(ItemUpdateState.Unchanged);
        }
        public override bool LoadFromDB(uint guid, Maps.Map map)
        {
            throw new NotImplementedException();
        }
        public bool LoadFromDB(ulong guid, ulong owner_guid, uint itemEntry)
        {
            //                                              0            1                2      3         4        5      6             7                 8 
            SQLResult result = DB.Characters.Select("SELECT creatorGuid, giftCreatorGuid, count, duration, charges, flags, enchantments, randomPropertyId, durability, " +
                //9           10
                "playedTime, text FROM item_instance WHERE guid = {0}", guid);
            if (result.Count == 0)
                return false;
            // create item before any checks for store correct guid
            // and allow use "FSetState(ITEM_REMOVED); SaveToDB();" for deleting item from DB
            CreateGuid(guid, 0, HighGuidType.Item);

            SetEntry(itemEntry);
            SetObjectScale(1.0f);

            ItemTemplate proto = GetTemplate();
            if (proto == null)
                return false;

            // set owner (not if item is only loaded for gbank/auction/mail
            if (owner_guid != 0)
                SetOwnerGUID(owner_guid);

            bool need_save = false;
            SetValue<ulong>(ItemFields.Creator, MakeNewGuid(result.Read<uint>(0, 0), 0, HighGuidType.Player));
            SetValue<ulong>(ItemFields.GiftCreator, MakeNewGuid(result.Read<uint>(0, 1), 0, HighGuidType.Player));

            ulong blah = GetPackGUID();
            ulong vlah = GetGUID();

            ulong bl = ObjectGuid.GuidLowPart(vlah);

            SetStackCount(result.Read<uint>(0, 2));

            uint duration = result.Read<uint>(0, 3);
            SetValue<uint>(ItemFields.Expiration, duration);
            // update duration if need, and remove if not need
            if (proto.Duration != duration)
            {
                SetValue<uint>(ItemFields.Expiration, proto.Duration);
                need_save = true;
            }

            //Tokens tokens(fields[4].GetString(), ' ', MAX_ITEM_PROTO_SPELLS);
            //if (tokens.size() == MAX_ITEM_PROTO_SPELLS)
            //for (uint8 i = 0; i < MAX_ITEM_PROTO_SPELLS; ++i)
            //SetSpellCharges(i, atoi(tokens[i]));

            SetValue<uint>(ItemFields.Flags, result.Read<uint>(0, 5));
            // Remove bind flag for items vs NO_BIND set
            if (IsSoulBound() && proto.Bonding == ItemBondingType.None)
            {
                ApplyModFlag(ItemFields.Flags, ItemFieldFlags.Soulbound, false);
                need_save = true;
            }

            string enchants = result.Read<string>(0, 6);
            LoadIntoDataField(enchants, (uint)ItemFields.Enchantment, (uint)EnchantmentSlot.MAX_ENCHANTMENT_SLOT * (uint)EnchantmentOffset.Max);
            SetValue<int>(ItemFields.RandomPropertiesID, result.Read<int>(0, 7));
            // recalculate suffix factor
            if (GetItemRandomPropertyId() < 0)
                UpdateItemSuffixFactor();

            uint durability = result.Read<uint>(0, 8);
            SetValue<uint>(ItemFields.Durability, durability);
            // update max durability (and durability) if need
            SetValue<uint>(ItemFields.MaxDurability, proto.MaxDurability);
            if (durability > proto.MaxDurability)
            {
                SetValue<uint>(ItemFields.Durability, proto.MaxDurability);
                need_save = true;
            }

            SetValue<uint>(ItemFields.CreatePlayedTime, result.Read<uint>(0, 9));
            SetText(result.Read<string>(0, 10));

            if (need_save)                                           // normal item changed state set not work at loading
            {
                PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_UPD_ITEM_INSTANCE_ON_LOAD);
                stmt.AddValue(0, GetValue<uint>(ItemFields.Expiration));
                stmt.AddValue(1, GetValue<uint>(ItemFields.Flags));
                stmt.AddValue(2, GetValue<uint>(ItemFields.Durability));
                stmt.AddValue(3, guid);
                DB.Characters.Execute(stmt);
            }
            return true;
        }
        public void DeleteFromDB(uint itemGuid)
        {
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_ITEM_INSTANCE);
            stmt.AddValue(0, itemGuid);
            DB.Characters.Execute(stmt);
        }
        public void DeleteFromDB()
        {
            DeleteFromDB(GetGUIDLow());
        }
        public void SaveRefundDataToDB()
        {
            DeleteRefundDataFromDB();

            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_INS_ITEM_REFUND_INSTANCE);
            stmt.AddValue(0, GetGUIDLow());
            stmt.AddValue(1, GetRefundRecipient());
            stmt.AddValue(2, GetPaidMoney());
            stmt.AddValue(3, (ushort)GetPaidExtendedCost());
            DB.Characters.Execute(stmt);
        }
        public void DeleteRefundDataFromDB()
        {
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_ITEM_REFUND_INSTANCE);
            stmt.AddValue(0, GetGUIDLow());
            DB.Characters.Execute(stmt);
        }

        #region Fields
        ItemUpdateState uState;
        uint PaidExtendedCost;
        uint PaidMoney;
        uint RefundRecipient;
        public bool LootGenerated { get; set; }
        byte Slot;
        Bag Container;
        int uQueuePos;
        string m_text;
        #endregion
    }

    public class ItemPosCount
    {
        public ItemPosCount(ushort _pos, uint _count)
        {
            pos = _pos;
            count = _count;
        }
        public bool isContainedIn(ref List<ItemPosCount> vec)
        {
            foreach (var itr in vec)
                if (itr.pos == pos)
                    return true;
            return false;
        }
        public ushort pos;
        public uint count;
    }
    public enum EnchantmentOffset
    {
        Id = 0,
        Duration = 1,
        Charges = 2,                         // now here not only charges, but something new in wotlk
        Max = 3
    }
}
