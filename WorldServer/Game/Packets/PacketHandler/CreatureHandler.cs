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
using Framework.Constants;
using Framework.Logging;
using Framework.Network;
using WorldServer.Game.Managers;
using WorldServer.Game.Spells;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;
using Framework.Utility;
using Framework.DataStorage;
using System.IO;
using Framework.ObjectDefines;

namespace WorldServer.Game.Packets
{
    public class CreatureHandler : Cypher
    {
        [ClientOpcode(Opcodes.CMSG_TrainerList)]
        public static void HandleTrainerList(ref PacketReader packet, ref WorldSession session)
        {
            ulong guid = packet.ReadUInt64();
            string str = ObjMgr.GetCypherString(CypherStrings.NpcTrainerHello);
            
            Creature unit = session.GetPlayer().GetNPCIfCanInteractWith(guid, NPCFlags.Trainer);
            if (unit == null)
            {
                Log.outDebug("WORLD: SendTrainerList - Unit (GUID: {0}) not found or you can not interact with him.", ObjectGuid.GuidLowPart(guid));
                return;
            }
            
            // remove fake death
            //if (GetPlayer()->HasUnitState(UNIT_STATE_DIED))
            //GetPlayer()->RemoveAurasByType(SPELL_AURA_FEIGN_DEATH);

            // trainer list loaded at check;
            if (!unit.isTrainerOf(session.GetPlayer(), true))
                return;

            CreatureTemplate ci = unit.GetCreatureTemplate();

            if (ci == null)
            {
                Log.outDebug("WORLD: SendTrainerList - (GUID: {0}) NO CREATUREINFO!", ObjectGuid.GuidLowPart(guid));
                return;
            }
            TrainerSpellData trainer_spells = unit.GetTrainerSpells();
            if (trainer_spells == null)
            {
                Log.outDebug("WORLD: SendTrainerList - Training spells not found for creature (GUID: {0} Entry: {1})", ObjectGuid.GuidLowPart(guid), unit.GetEntry());
                return;
            }
            PacketWriter data = new PacketWriter(Opcodes.SMSG_TrainerList);
            data.WriteUInt64(guid);
            data.WriteUInt32(trainer_spells.trainerType);
            data.WriteUInt32(133);

            int count_pos = data.wpos();
            data.WriteUInt32(trainer_spells.spellList.Count);

            // reputation discount
            float fDiscountMod = session.GetPlayer().GetReputationPriceDiscount(unit);
            bool can_learn_primary_prof = session.GetPlayer().GetFreePrimaryProfessionPoints() > 0;

            uint count = 0;
            foreach (var spell in trainer_spells.spellList.Values)
            {
                bool valid = true;
                bool primary_prof_first_rank = false;
                for (var i = 0; i < 3; ++i)
                {
                    if (spell.learnedSpell[i] == 0)
                        continue;
                    if (!session.GetPlayer().IsSpellFitByClassAndRace(spell.learnedSpell[i]))
                    {
                        valid = false;
                        break;
                    }
                    SpellInfo spellentry = SpellMgr.GetSpellInfo(spell.learnedSpell[i]);
                    if (spellentry.IsPrimaryProfessionFirstRank())
                        primary_prof_first_rank = true;
                }
                if (!valid)
                    continue;

                TrainerSpellState state = session.GetPlayer().GetTrainerSpellState(spell);
                data.WriteUInt32(spell.spellId);                      // learned spell (or cast-spell in profession case)
                data.WriteUInt8((byte)(state == TrainerSpellState.GreenDisabled ? TrainerSpellState.Green : state));
                data.WriteUInt32((uint)Math.Floor(spell.spellCost * fDiscountMod));

                data.WriteUInt8((byte)spell.reqLevel);
                data.WriteUInt32(spell.reqSkill);
                data.WriteUInt32(spell.reqSkillValue);
                //prev + req or req + 0
                var maxReq = 0;
                for (var i = 0; i < 3; ++i)
                {
                    if (spell.learnedSpell[i] == 0)
                        continue;
                    uint prevSpellId = SpellMgr.GetPrevSpellInChain(spell.learnedSpell[i]);
                    if (prevSpellId != 0)
                    {
                        data.WriteUInt32(prevSpellId);
                        maxReq++;
                    }
                    if (maxReq == 2)
                        break;
                    
                    //SpellsRequiringSpellMapBounds spellsRequired = sSpellMgr->GetSpellsRequiredForSpellBounds(tSpell->learnedSpell[i]);
                    //for (SpellsRequiringSpellMap::const_iterator itr2 = spellsRequired.first; itr2 != spellsRequired.second && maxReq < 3; ++itr2)
                    {
                        //data.WriteUInt32(itr2->second);
                        //++maxReq;
                    }
                    //if (maxReq == 2)
                        //break;
                }
                while (maxReq < 2)
                {
                    data.WriteUInt32(0);
                    maxReq++;
                }
                
                data.WriteInt32(primary_prof_first_rank && can_learn_primary_prof ? 1 : 0);
                count++;
            }
            data.WriteCString(str);
            data.Replace<uint>(count_pos, count);
            session.Send(data);
        }

        [ClientOpcode(Opcodes.CMSG_TrainerBuySpell)]
        public static void HandleTrainerBuySpell(ref PacketReader packet, ref WorldSession session)
        {
            ulong guid = packet.ReadUInt64();
            uint trainerId = packet.ReadUInt32();
            uint spellId = packet.ReadUInt32();

            Player pl = session.GetPlayer();

            Creature unit = pl.GetNPCIfCanInteractWith(guid, NPCFlags.Trainer);
            if (unit == null)
            {
                //sLog->outDebug(LOG_FILTER_NETWORKIO, "WORLD: HandleTrainerBuySpellOpcode - Unit (GUID: %u) not found or you can not interact with him.", uint32(GUID_LOPART(guid)));
                return;
            }

            // remove fake death
            //if (GetPlayer()->HasUnitState(UNIT_STATE_DIED))
                //GetPlayer()->RemoveAurasByType(SPELL_AURA_FEIGN_DEATH);

            if (!unit.isTrainerOf(pl, true))
            {
                SendTrainerBuyFailed(ref session, guid, spellId, 0);
                return;
            }

            var spells = unit.GetTrainerSpells();
            if (spells == null)
            {
                SendTrainerBuyFailed(ref session, guid, spellId, 0);
                return;
            }

            var learnspell = spells.spellList.LookupByKey(spellId);
            if (learnspell == null)
            {
                SendTrainerBuyFailed(ref session, guid, spellId, 0);
                return;
            }

            // can't be learn, cheat? Or double learn with lags...
            if (pl.GetTrainerSpellState(learnspell) != TrainerSpellState.Green)
            {
                SendTrainerBuyFailed(ref session, guid, spellId, 0);
                return;
            }

            // apply reputation discount
            uint nSpellCost = (uint)Math.Floor((decimal)learnspell.spellCost);// * _player->GetReputationPriceDiscount(unit)));

            // check money requirement
            if (!pl.HasEnoughMoney(nSpellCost))
            {
                SendTrainerBuyFailed(ref session, guid, spellId, 1);
                return;
            }

            pl.ModifyMoney(-nSpellCost);

            unit.SendPlaySpellVisualKit(179, 0);       // 53 SpellCastDirected
            pl.SendPlaySpellVisualKit(362, 1);    // 113 EmoteSalute

            // learn explicitly or cast explicitly
            //if (learnspell.IsCastable())
                //session.GetPlayer().CastSpell(_player, trainer_spell->spell, true);
            //else
                pl.LearnSpell(spellId, false);

                PacketWriter data = new PacketWriter(Opcodes.SMSG_TrainerBuySucceeded);
            data.WriteUInt64(guid);
            data.WriteUInt32(spellId);
            session.Send(data);
        }

        [ClientOpcode(Opcodes.CMSG_BankerActivate)]
        public static void HandleBankerActivate(ref PacketReader packet, ref WorldSession session)
        {
            ulong guid = packet.ReadUInt64();

            Creature unit = session.GetPlayer().GetNPCIfCanInteractWith(guid, NPCFlags.Banker);
            if (unit == null)
            {
                Log.outError("Banker: Unit (GUID: {0}) not found or you can not interact with him.", guid);
                return;
            }

            // remove fake death
            //if (GetPlayer()->HasUnitState(UNIT_STATE_DIED))
                //GetPlayer()->RemoveAurasByType(SPELL_AURA_FEIGN_DEATH);

            PacketWriter data = new PacketWriter(Opcodes.SMSG_ShowBank);
            data.WriteUInt64(guid);
            session.Send(data);
        }
        
        static void SendTrainerBuyFailed(ref WorldSession session, ulong guid, uint spellId, uint reason)
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_TrainerBuyFailed);
            data.WriteUInt64(guid);
            data.WriteUInt32(spellId);
            data.WriteUInt32(reason);         // 1 == "Not enough money for trainer service." 0 == "Trainer service %d unavailable."
            session.Send(data);
        }

        [ClientOpcode(Opcodes.CMSG_ListInventory)]
        public static void HandleListInventoryOpcode(ref PacketReader packet, ref WorldSession session)
        {
            ulong guid = packet.ReadUInt64();
            
            if (!session.GetPlayer().isAlive())
                return;

            Log.outDebug("WORLD: Recvd CMSG_LIST_INVENTORY");

            SendListInventory(guid, ref session);
        }

        static void SendListInventory(ulong vendorGuid, ref WorldSession session)
        {
            Log.outDebug("WORLD: Sent SMSG_LIST_INVENTORY");

            Player pl = session.GetPlayer();

            Creature vendor = pl.GetNPCIfCanInteractWith(vendorGuid, NPCFlags.Vendor);
            if (vendor == null)
            {
                Log.outDebug("WORLD: SendListInventory - Unit (GUID: %u) not found or you can not interact with him.", ObjectGuid.GuidLowPart(vendorGuid));
                session.GetPlayer().SendSellError(SellResult.CantFindVendor, null, 0);
                return;
            }

            // remove fake death
            //if (GetPlayer()->HasUnitState(UNIT_STATE_DIED))
            //GetPlayer()->RemoveAurasByType(SPELL_AURA_FEIGN_DEATH);

            // Stop the npc if moving
            //if (vendor.HasUnitState(UNIT_STATE_MOVING))
            //vendor->StopMoving();

            VendorItemData vendorItems = vendor.GetVendorItems();
            int rawItemCount = vendorItems != null ? vendorItems.GetItemCount() : 0;

            List<bool> enablers = new List<bool>();
            PacketWriter itemsData = new PacketWriter();

            float discountMod = session.GetPlayer().GetReputationPriceDiscount(vendor);
            ushort count = 0;
            for (ushort slot = 0; slot < rawItemCount; ++slot)
            {
                VendorItem vendorItem = vendorItems.GetItem(slot);
                if (vendorItem == null)
                    continue;

                if (vendorItem.Type == (byte)ItemVendorType.Item)
                {
                    ItemTemplate itemTemplate = ObjMgr.GetItemTemplate(vendorItem.item);
                    if (itemTemplate == null)
                        continue;

                    uint leftInStock = vendorItem.maxcount == 0 ? 0xFFFFFFFF : vendor.GetVendorItemCurrentCount(vendorItem);
                    if (!pl.isGameMaster()) // ignore conditions if GM on
                    {
                        // Respect allowed class
                        if (!Convert.ToBoolean(itemTemplate.AllowableClass & pl.getClassMask()) && itemTemplate.Bonding == ItemBondingType.PickedUp)
                            continue;

                        // Only display items in vendor lists for the team the player is on
                        if ((Convert.ToBoolean(itemTemplate.Flags2 & ItemFlags2.HordeOnly) && pl.GetTeam() == Team.Alliance) ||
                            (Convert.ToBoolean(itemTemplate.Flags2 & ItemFlags2.AllianceOnly) && pl.GetTeam() == Team.Horde))
                            continue;

                        // Items sold out are not displayed in list
                        if (leftInStock == 0)
                            continue;
                    }

                    int price = vendorItem.IsGoldRequired(itemTemplate) ? (int)(Math.Floor(itemTemplate.BuyPrice * discountMod)) : 0;

                    //int priceMod = pl.GetTotalAuraModifier(SPELL_AURA_MOD_VENDOR_ITEMS_PRICES)
                    //if (priceMod != 0)
                    //price -= CalculatePctN(price, priceMod);

                    itemsData.WriteUInt32(count++ + 1);        // client expects counting to start at 1
                    itemsData.WriteUInt32(itemTemplate.MaxDurability);

                    if (vendorItem.ExtendedCost != 0)
                    {
                        enablers.Add(false);
                        itemsData.WriteUInt32(vendorItem.ExtendedCost);
                    }
                    else
                        enablers.Add(true);
                    enablers.Add(true);                 // unk bit

                    itemsData.WriteUInt32(vendorItem.item);
                    itemsData.WriteUInt32(vendorItem.Type);     // 1 is items, 2 is currency
                    itemsData.WriteUInt32(price);
                    itemsData.WriteUInt32(itemTemplate.DisplayInfoID);
                    // if (!unk "enabler") data << uint32(something);
                    itemsData.WriteUInt32(leftInStock);
                    itemsData.WriteUInt32(itemTemplate.BuyCount);
                }
                else if (vendorItem.Type == (byte)ItemVendorType.Currency)
                {
                    CurrencyTypesEntry currencyTemplate = DBCStorage.CurrencyTypesStorage.LookupByKey(vendorItem.item);
                    if (currencyTemplate == null)
                        continue;

                    if (vendorItem.ExtendedCost == 0)
                        continue; // there's no price defined for currencies, only extendedcost is used

                    uint precision = (uint)(Convert.ToBoolean(currencyTemplate.Flags & (uint)CurrencyFlags.HighPrecision) ? 100 : 1);

                    itemsData.WriteUInt32(count++ + 1);        // client expects counting to start at 1
                    itemsData.WriteUInt32(0);                  // max durability

                    if (vendorItem.ExtendedCost != 0)
                    {
                        enablers.Add(false);
                        itemsData.WriteUInt32(vendorItem.ExtendedCost);
                    }
                    else
                        enablers.Add(true);

                    enablers.Add(true);                    // unk bit

                    itemsData.WriteUInt32(vendorItem.item);
                    itemsData.WriteUInt32(vendorItem.Type);    // 1 is items, 2 is currency
                    itemsData.WriteUInt32(0);                   // price, only seen currency types that have Extended cost
                    itemsData.WriteUInt32(0);                   // displayId
                    // if (!unk "enabler") data << uint32(something);
                    itemsData.WriteInt32(-1);
                    itemsData.WriteUInt32(vendorItem.maxcount * precision);
                }
                // else error
            }

            ObjectGuid guid = new ObjectGuid(vendorGuid);

            PacketWriter data = new PacketWriter(Opcodes.SMSG_ListInventory);

            data.WriteBit(guid[1]);
            data.WriteBit(guid[0]);

            data.WriteBits(count, 21); // item count

            data.WriteBit(guid[3]);
            data.WriteBit(guid[6]);
            data.WriteBit(guid[5]);
            data.WriteBit(guid[2]);
            data.WriteBit(guid[7]);

            foreach (var itr in enablers)
                data.WriteBit(itr);

            data.WriteBit(guid[4]);

            data.BitFlush();
            data.WriteBytes(itemsData);

            data.WriteByteSeq(guid[5]);
            data.WriteByteSeq(guid[4]);
            data.WriteByteSeq(guid[1]);
            data.WriteByteSeq(guid[0]);
            data.WriteByteSeq(guid[6]);

            data.WriteUInt8(count == 0); // unk byte, item count 0: 1, item count != 0: 0 or some "random" value below 300

            data.WriteByteSeq(guid[2]);
            data.WriteByteSeq(guid[3]);
            data.WriteByteSeq(guid[7]);

            session.Send(data);
        }

        [ClientOpcode(Opcodes.CMSG_GossipHello)]
        public static void HandleGossipHello(ref PacketReader packet, ref WorldSession session)
        {
            Log.outDebug("WORLD: Received CMSG_GOSSIP_HELLO");

            ulong guid = packet.ReadUInt64();

            Creature unit = session.GetPlayer().GetNPCIfCanInteractWith(guid, NPCFlags.Gossip);
            if (unit == null)
            {
                Log.outDebug("WORLD: HandleGossipHelloOpcode - Unit (GUID: {0}) not found or you can not interact with him.", ObjectGuid.GuidLowPart(guid));
                return;
            }

            // set faction visible if needed
            FactionTemplateEntry factionTemplateEntry = DBCStorage.FactionTemplateStorage.LookupByKey(unit.getFaction());
            if (factionTemplateEntry != null)
                session.GetPlayer().GetReputationMgr().SetVisible(factionTemplateEntry);

            //GetPlayer()->RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_TALK);

            //if (unit.isArmorer() || unit.isCivilian() || unit.isQuestGiver() || unit.isServiceProvider() || unit.isGuard())
            //unit.StopMoving();

            // If spiritguide, no need for gossip menu, just put player into resurrect queue
            //if (unit.isSpiritGuide())
            {
                //Battleground* bg = _player->GetBattleground();
                //if (bg)
                {
                    //bg->AddPlayerToResurrectQueue(unit->GetGUID(), _player->GetGUID());
                    //sBattlegroundMgr->SendAreaSpiritHealerQueryOpcode(_player, bg, unit->GetGUID());
                    //return;
                }
            }

            //if (!sScriptMgr->OnGossipHello(_player, unit))
            {
                //        _player->TalkedToCreature(unit->GetEntry(), unit->GetGUID());
                //_player->PrepareGossipMenu(unit, unit->GetCreatureTemplate()->GossipMenuId, true);
                //_player->SendPreparedGossip(unit);
            }
            //unit.AI()->sGossipHello(_player);
        }
    }
}
