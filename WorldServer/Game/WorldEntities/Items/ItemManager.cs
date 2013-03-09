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
using System.Data;
using System.Linq;
using Framework.Constants;
using Framework.Database;
using Framework.DataStorage;
using Framework.Logging;
using Framework.Singleton;
using Framework.Utility;

namespace WorldServer.Game.WorldEntities
{
    public sealed class ItemManager : SingletonBase<ItemManager>
    {
        ItemManager() { }

        public Item NewItemOrBag(ItemTemplate proto) { return (proto.inventoryType == InventoryType.Bag) ? new Bag() : new Item(); }

        Item LoadItem(DataRow row, Player pl, uint timeDiff)
        {
            Item item = null;
            uint itemGuid = row.Field<uint>(13);
            uint itemEntry = row.Field<uint>(14);
            ItemTemplate proto = Cypher.ObjMgr.GetItemTemplate(itemEntry);
            if (proto != null)
            {
                bool remove = false;
                item = NewItemOrBag(proto);

                if (item.LoadFromDB(itemGuid, pl.GetGUID(), itemEntry))
                {
                    PreparedStatement stmt = null;

                    // Do not allow to have item limited to another map/zone in alive state
                    //if (isAlive() && item.IsLimitedToAnotherMapOrZone(pl.GetMapId(), pl.Zone))
                    {
                        //Log.outDebug("LoadInventory: player (GUID: %u, name: '%s', map: %u) has item (GUID: %u, entry: %u) limited to another map (%u). Deleting item.",
                            //pl.GetGUIDLow(), pl.Name, pl.GetMapId(), item.GetGUIDLow(), item.GetEntry(), pl.Zone);
                        //remove = true;
                    }
                    // "Conjured items disappear if you are logged out for more than 15 minutes"
                    //else if (timeDiff > 15 * MINUTE && proto.Flags & (ItemFlags.Conjured))
                    {
                       // Log.outDebug("LoadInventory: player (GUID: {0}, name: {1}, diff: {2}) has conjured item (GUID: {3}, entry: {4}) with expired lifetime (15 minutes). Deleting item.",
                           // pl.GetGUIDLow(), pl.Name, timeDiff, item.GetGUIDLow(), item.GetEntry());
                        //remove = true;
                    }
                    //else if (item.HasFlag((int)ItemFields.Flags, ItemFieldFlags.Refundable))
                    {
                        //if (item.GetPlayedTime() > (2 * HOUR))
                        {
                            //Log.outDebug("LoadInventory: player (GUID: {0}, name: {1}) has item (GUID: {2}, entry: {3}) with expired refund time ({4}). Deleting refund data and removing " +
                            //"efundable flag.", pl.GetGUIDLow(), pl.Name, item.GetGUIDLow(), item.GetEntry(), item.GetPlayedTime());

                            //stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_DEL_ITEM_REFUND_INSTANCE);
                            //stmt.AddValue(0, item.GetGUIDLow());
                            //DB.Characters.Execute(stmt);

                            //item.RemoveFlag(ITEM_FIELD_FLAGS, ITEM_FLAG_REFUNDABLE);
                        }
                        //else
                        {
                            stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_ITEM_REFUNDS);
                            stmt.AddValue(0, item.GetGUIDLow());
                            stmt.AddValue(1, pl.GetGUIDLow());
                            if (DB.Characters.Execute(stmt))
                            {
                                //item.SetRefundRecipient((*result)[0].GetUInt32());
                                //item.SetPaidMoney((*result)[1].GetUInt32());
                                //item.SetPaidExtendedCost((*result)[2].GetUInt16());
                                //AddRefundReference(item->GetGUIDLow());
                            }
                            else
                            {
                                Log.outDebug("LoadInventory: player (GUID: {0}, name: {1}) has item (GUID: {2}, entry: {3}) with refundable flags, but without data in item_refund_instance. Removing flag.",
                                    pl.GetGUIDLow(), pl.GetName(), item.GetGUIDLow(), item.GetEntry());
                                //item.RemoveFlag(ITEM_FIELD_FLAGS, ITEM_FLAG_REFUNDABLE);
                            }
                        }
                    }
                    //else if (item.HasFlag((int)ItemFields.Flags, ItemFieldFlags.BopTradeable))
                    {
                        stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_ITEM_BOP_TRADE);
                        stmt.AddValue(0, item.GetGUIDLow());
                        if (DB.Characters.Execute(stmt))
                        {
                            //string strGUID = (*result)[0].GetString();
                            //Tokens GUIDlist(strGUID, ' ');
                            //AllowedLooterSet looters;
                            //for (Tokens::iterator itr = GUIDlist.begin(); itr != GUIDlist.end(); ++itr)
                            //looters.insert(atol(*itr));
                            //item.SetSoulboundTradeable(looters);
                            //AddTradeableItem(item);
                        }
                        else
                        {
                            Log.outDebug("LoadInventory: player (GUID: {0}, name: {1}) has item (GUID: {2}, entry: {3}) with ITEM_FLAG_BOP_TRADEABLE flag, " +
                                "but without data in item_soulbound_trade_data. Removing flag.", pl.GetGUIDLow(), pl.GetName(), item.GetGUIDLow(), item.GetEntry());
                            //item.RemoveFlag(ITEM_FIELD_FLAGS, ITEM_FLAG_BOP_TRADEABLE);
                        }
                    }
                    //else if (proto.HolidayId != 0)
                    {
                        remove = true;
                        //GameEventMgr::GameEventDataMap const& events = sGameEventMgr->GetEventMap();
                        //GameEventMgr::ActiveEvents const& activeEventsList = sGameEventMgr->GetActiveEventList();
                        //for (GameEventMgr::ActiveEvents::const_iterator itr = activeEventsList.begin(); itr != activeEventsList.end(); ++itr)
                        {
                            //if (uint32(events[*itr].holiday_id) == proto->HolidayId)
                            {
                                //remove = false;
                                //break;
                            }
                        }
                    }
                }
                else
                {
                    Log.outError("LoadInventory: player (GUID: {0}, name: {1}) has broken item (GUID: {2}, entry: {3}) in inventory. Deleting item.",
                        pl.GetGUIDLow(), pl.GetName(), itemGuid, itemEntry);
                    remove = true;
                }
                // Remove item from inventory if necessary
                if (remove)
                {
                    //Item::DeleteFromInventoryDB(trans, itemGuid);
                    //item.FSetState(ITEM_REMOVED);
                    //item.SaveToDB(trans);                           // it also deletes item object!
                    //item = null;
                }
            }
            else
            {
                Log.outError("LoadInventory: player (GUID: {0}, name: {1}) has unknown item (entry: {2}) in inventory. Deleting item.",
                    pl.GetGUIDLow(), pl.GetName(), itemEntry);
                //Item::DeleteFromInventoryDB(trans, itemGuid);
                //Item::DeleteFromDB(trans, itemGuid);
            }
            return item;
        }
    }
}
