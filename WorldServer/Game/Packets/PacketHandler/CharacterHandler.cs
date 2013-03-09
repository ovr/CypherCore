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
using Framework.Constants;
using Framework.Database;
using Framework.DataStorage;
using Framework.Logging;
using Framework.Network;
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;
using Framework.Utility;
using Framework.ObjectDefines;

namespace WorldServer.Game.Packets
{
    public class CharacterHandler : Cypher
    {
        [ClientOpcode(Opcodes.CMSG_CharEnum)]
        public static void HandleCharacterEnum(ref PacketReader packet, ref WorldSession session)
        {
            //"SELECT c.guid, c.name, c.race, c.class, c.gender, c.playerBytes, c.playerBytes2, c.level, c.zone, c.map, c.position_x, c.position_y, c.position_z, gm.guildid, c.playerFlags, c.at_login, cp.entry, cp.modelid,
            //cp.level, c.equipmentCache, cb.guid, c.slot FROM characters AS c LEFT JOIN character_pet AS cp ON c.guid = cp.owner AND cp.slot = ? LEFT JOIN guild_member AS gm ON c.guid = gm.guid 
            //LEFT JOIN character_banned AS cb ON c.guid = cb.guid AND cb.active = 1 WHERE c.account = ?";
            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_SEL_ENUM);
            stmt.AddValue(0, (byte)0);
            stmt.AddValue(1, session.GetAccountId());
            SQLResult result = DB.Characters.Select(stmt);

            PacketWriter data = new PacketWriter(Opcodes.SMSG_CharEnum);

            data.WriteBits(0, 23);
            data.WriteBits(result.Count, 17);

            if (result.Count != 0)
                BuildEnumData(result, ref data);
            else
            {
                data.WriteBit(1);
                data.BitFlush();
            }

            session.Send(data);
        }
        public static bool BuildEnumData(SQLResult result, ref PacketWriter data)
        {
            for (int c = 0; c < result.Count; c++)
            {
                ObjectGuid guid = new ObjectGuid(result.Read<ulong>(c, 0));
                ObjectGuid guildGuid = new ObjectGuid();//result.Read<uint>(c, 13));
                string name = result.Read<string>(c, 1);

                data.WriteBit(guid[7]);
                data.WriteBit(guid[0]);
                data.WriteBit(guid[4]);
                data.WriteBit(guildGuid[2]);
                data.WriteBit(guid[5]);
                data.WriteBit(guid[3]);
                data.WriteBits(name.Length, 7);
                data.WriteBit(guildGuid[0]);
                data.WriteBit(guildGuid[5]);
                data.WriteBit(guildGuid[3]);
                data.WriteBit(0);
                data.WriteBit(guildGuid[6]);
                data.WriteBit(guildGuid[7]);
                data.WriteBit(guid[1]);
                data.WriteBit(guildGuid[4]);
                data.WriteBit(guildGuid[1]);
                data.WriteBit(guid[2]);
                data.WriteBit(guid[6]);
            }
            data.WriteBit(1);
            data.BitFlush();

            for (int c = 0; c < result.Count; c++)
            {
                ObjectGuid guid = new ObjectGuid(result.Read<ulong>(c, 0));
                string name = result.Read<string>(c, 1);
                byte plrRace = result.Read<byte>(c, 2);
                Class plrClass = (Class)result.Read<byte>(c, 3);
                byte gender = result.Read<byte>(c, 4);
                byte skin = (byte)(result.Read<uint>(c, 5) & 0xFF);
                byte face = (byte)((result.Read<uint>(c, 5) >> 8) & 0xFF);
                byte hairStyle = (byte)((result.Read<uint>(c, 5) >> 16) & 0xFF);
                byte hairColor = (byte)((result.Read<uint>(c, 5) >> 24) & 0xFF);
                byte facialHair = (byte)(result.Read<uint>(c, 6) & 0xFF);
                byte level = result.Read<byte>(c, 7);
                uint zone = result.Read<uint>(c, 8);
                uint mapId = result.Read<uint>(c, 9);
                float x = result.Read<float>(c, 10);
                float y = result.Read<float>(c, 11);
                float z = result.Read<float>(c, 12);
                ObjectGuid guildGuid = new ObjectGuid();//result.Read<ulong>(c, 13));
                PlayerFlags playerFlags = (PlayerFlags)result.Read<uint>(c, 14);
                AtLoginFlags atLoginFlags = (AtLoginFlags)result.Read<uint>(c, 15);
                string[] equipment = result.Read<string>(c, 19).Split(' ');
                byte slot = result.Read<byte>(c, 21);

                CharacterFlags charFlags = 0;
                if (Convert.ToBoolean(playerFlags & PlayerFlags.HideHelm))
                    charFlags |= CharacterFlags.HideHelm;

                if (Convert.ToBoolean(playerFlags & PlayerFlags.HideCloak))
                    charFlags |= CharacterFlags.HideCloak;

                if (Convert.ToBoolean(playerFlags & PlayerFlags.Ghost))
                    charFlags |= CharacterFlags.Ghost;

                if (Convert.ToBoolean(atLoginFlags & AtLoginFlags.Rename))
                    charFlags |= CharacterFlags.Rename;

                //if (result.Read<uint>(c, 20) != 0)
                //charFlags |= CharacterFlags.LockedByBilling;

                //if (sWorld->getBoolConfig(CONFIG_DECLINED_NAMES_USED) && !fields[22].GetString().empty())
                //charFlags |= CHARACTER_FLAG_DECLINED;

                CharacterCustomizeFlags customizationFlag = 0;
                if (Convert.ToBoolean(atLoginFlags & AtLoginFlags.Customize))
                    customizationFlag = CharacterCustomizeFlags.Customize;
                else if (Convert.ToBoolean(atLoginFlags & AtLoginFlags.ChangeFaction))
                    customizationFlag = CharacterCustomizeFlags.Faction;
                else if (Convert.ToBoolean(atLoginFlags & AtLoginFlags.ChangeRace))
                    customizationFlag = CharacterCustomizeFlags.Race;

                uint petDisplayId = 0;
                uint petLevel = 0;
                uint petFamily = 0;
                // show pet at selection character in character list only for non-ghost character
                if (!Convert.ToBoolean(playerFlags & PlayerFlags.Ghost) && (plrClass == Class.Warlock || plrClass == Class.Hunter || plrClass == Class.Deathknight))
                {
                    //uint entry = result.Read<uint>(c, 16);
                    //var creatureInfo = ObjMgr.GetCreatureTemplate(entry);
                    //if (creatureInfo != null)
                    {
                        //petDisplayId = result.Read<uint>(c, 17);
                        //petLevel = result.Read<uint>(c, 18);
                        //petFamily = creatureInfo.Family;
                    }
                }

                data.WriteUInt32(charFlags);
                data.WriteUInt32(petFamily);
                data.WriteFloat(z);
                data.WriteByteSeq(guid[7]);
                data.WriteByteSeq(guildGuid[6]);

                for (uint i = 0; i < InventorySlots.BagEnd; ++i)
                {
                    uint index = i * 2;
                    uint itemId;
                    uint.TryParse(equipment[index], out itemId);
                    var entry = ObjMgr.GetItemTemplate(itemId);
                    if (entry == null)
                    { 
                        data.WriteInt32(0);
                        data.WriteInt8(0);
                        data.WriteInt32(0);
                        continue;
                    }

                    uint enchants;
                    SpellItemEnchantmentEntry enchant = null;
                    uint.TryParse(equipment[index + 1], out enchants);
                    for (int enchantSlot = (int)EnchantmentSlot.PERM_ENCHANTMENT_SLOT; enchantSlot <= (int)EnchantmentSlot.TEMP_ENCHANTMENT_SLOT; enchantSlot++)
                    {
                        // values stored in 2 uint16
                        uint enchantId = 0x0000FFFF & (enchants >> enchantSlot * 16);
                        if (enchantId == 0)
                            continue;

                        enchant = DBCStorage.SpellItemEnchantmentStorage.LookupByKey(enchantId);
                        if (enchant != null)
                            break;
                    }
                    data.WriteInt32(0);//enchant != null ? enchant.aura_id : 0);
                    data.WriteInt8(entry.inventoryType);
                    data.WriteInt32(entry.DisplayInfoID);
                }

                data.WriteFloat(x);
                data.WriteUInt8(plrClass);
                data.WriteByteSeq(guid[5]);
                data.WriteFloat(y);
                data.WriteByteSeq(guildGuid[3]);
                data.WriteByteSeq(guid[6]);
                data.WriteUInt32(petLevel);
                data.WriteUInt32(petDisplayId);
                data.WriteByteSeq(guid[2]);
                data.WriteByteSeq(guid[1]);
                data.WriteUInt8(hairColor);
                data.WriteUInt8(facialHair);
                data.WriteByteSeq(guildGuid[2]);
                data.WriteUInt32(zone);
                data.WriteUInt8(slot);                                    // List order
                data.WriteByteSeq(guid[0]);
                data.WriteByteSeq(guildGuid[1]);
                data.WriteUInt8(skin);
                data.WriteByteSeq(guid[4]);
                data.WriteByteSeq(guildGuid[5]);
                data.WriteString(name);
                data.WriteByteSeq(guildGuid[0]);
                data.WriteUInt8(level);
                data.WriteByteSeq(guid[3]);
                data.WriteByteSeq(guildGuid[7]);
                data.WriteUInt8(hairStyle);
                data.WriteByteSeq(guildGuid[4]);
                data.WriteUInt8(gender);
                data.WriteUInt32(mapId);
                data.WriteUInt32((uint)customizationFlag);
                data.WriteUInt8(plrRace);
                data.WriteUInt8(face);
            }
            return true;
        }

        [ClientOpcode(Opcodes.CMSG_CharCreate)]
        public static void HandleRequestCharCreate(ref PacketReader packet, ref WorldSession session)
        {
            PacketWriter writer = new PacketWriter(Opcodes.SMSG_CharCreate);

            Player newChar = new Player(ref session);
            //newChar.GetMotionMaster()->Initialize();
            if (!newChar.Create(ObjMgr.GenerateLowGuid(HighGuidType.Player), ref packet))
            {
                // Player not create (race/class/etc problem?)
                writer.WriteUInt8((byte)ResponseCodes.CharCreateError);
                session.Send(writer);
                return;
            }

            //if ((haveSameRace && skipCinematics == 1) || skipCinematics == 2)
                //newChar.setCinematic(1);                          // not show intro

            newChar.atLoginFlags = AtLoginFlags.LoginFirst;               // First login

            // Player created, save it now
            newChar.SaveToDB(true);
            //createInfo->CharCount += 1;

            //PreparedStatement stmt = DB.Realms.GetPreparedStatement(LOGIN_DEL_REALM_CHARACTERS_BY_REALM);
            //stmt->setUInt32(0, GetAccountId());
            //stmt->setUInt32(1, realmID);
            //trans->Append(stmt);

            //stmt = LoginDatabase.GetPreparedStatement(LOGIN_INS_REALM_CHARACTERS);
            //stmt->setUInt32(0, createInfo->CharCount);
            //stmt->setUInt32(1, GetAccountId());
            //stmt->setUInt32(2, realmID);
            //trans->Append(stmt);

            // Success
            writer.WriteUInt8((byte)ResponseCodes.CharCreateSuccess);
            session.Send(writer);

            //std::string IP_str = GetRemoteAddress();
            //sLog->outInfo(LOG_FILTER_CHARACTER, "Account: %d (IP: %s) Create Character:[%s] (GUID: %u)", GetAccountId(), IP_str.c_str(), createInfo->Name.c_str(), newChar.GetGUIDLow());
            //sScriptMgr->OnPlayerCreate(&newChar);
            //sWorld->AddCharacterNameData(newChar.GetGUIDLow(), std::string(newChar.GetName()), newChar.getGender(), newChar.getRace(), newChar.getClass(), newChar.getLevel());
        }

        [ClientOpcode(Opcodes.CMSG_CharDelete)]
        public static void HandleRequestCharDelete(ref PacketReader packet, ref WorldSession session)
        {
            ulong guid = packet.ReadUInt64();

            PacketWriter writer = new PacketWriter(Opcodes.SMSG_CharDelete);
            writer.WriteUInt8(0x47);
            session.Send(writer);

            DB.Characters.Execute("DELETE FROM characters WHERE guid = {0}", guid);
            DB.Characters.Execute("DELETE FROM character_spell WHERE guid = {0}", guid);
        }

        [ClientOpcode(Opcodes.CMSG_RandomizeCharName)]
        public static void HandleRequestRandomCharacterName(ref PacketReader packet, ref WorldSession session)
        { 
            byte gender = packet.ReadByte();
            byte race = packet.ReadByte();

            List<string> names = DBCStorage.NameGenStorage.Where(n => n.Value.Race == race && n.Value.Gender == gender).Select(n => n.Value.Name).ToList();
            Random rand = new Random(Environment.TickCount);

            string NewName;
            SQLResult result;
            do
            {
                NewName = names[rand.Next(names.Count)];
                result = DB.Characters.Select("SELECT * FROM characters WHERE name = '{0}'", NewName);
            }
            while (result.Count != 0);

            PacketWriter writer = new PacketWriter(Opcodes.SMSG_RandomizeCharName);

            writer.WriteBits<int>(NewName.Length, 15);
            writer.WriteBit(true);
            writer.BitFlush();
            writer.WriteString(NewName);
            session.Send(writer);
        }

        [ClientOpcode(Opcodes.CMSG_PlayerLogin)]
        public static void HandlePlayerLogin(ref PacketReader packet, ref WorldSession session)
        {
            var mask = new byte[] {5, 7, 6, 1, 2, 3, 4, 0};
            var bytes = new byte[] {6, 4, 3, 5, 0, 2, 7, 1};
            var guid = packet.GetGuid(mask, bytes);

            Player pChar = new Player(ref session);
            session.SetPlayer(pChar);

            if (!pChar.LoadCharacter(guid))
            {
                session.SetPlayer(null);
                session.KickPlayer();                                       // disconnect client, player no set to session and it will not deleted or saved at kick
                //m_playerLoading = false;
                return;
            }

            //pCurrChar->GetMotionMaster()->Initialize();
            //pCurrChar->SendDungeonDifficulty(false);            

            PacketWriter world = new PacketWriter(Opcodes.SMSG_NewWorld);
            world.WriteUInt32(pChar.GetMapId());
            world.WriteFloat(pChar.Position.Y);
            world.WriteFloat(pChar.Position.Orientation);
            world.WriteFloat(pChar.Position.X);
            world.WriteFloat(pChar.Position.Z);
            session.Send(world);

            //LoadAccountData(holder->GetPreparedResult(PLAYER_LOGIN_QUERY_LOADACCOUNTDATA), PER_CHARACTER_CACHE_MASK);
            //SendAccountDataTimes(PER_CHARACTER_CACHE_MASK);
            session.SendAccountDataTimes(AccountDataMasks.PerCharacterCacheMask);

            PacketWriter feature = new PacketWriter(Opcodes.SMSG_FeatureSystemStatus);

            feature.WriteUInt8(2);     // SystemStatus
            feature.WriteUInt32(1);    // Unknown, Mostly 1
            feature.WriteUInt32(1);    // Unknown, Mostly 1
            feature.WriteUInt32(2);    // Unknown, Mostly same as SystemStatus, but seen other values
            feature.WriteUInt32(0);    // Unknown, Hmm???

            feature.WriteBit(true);  // Unknown
            feature.WriteBit(true);  // Unknown
            feature.WriteBit(false); // Unknown
            feature.WriteBit(true);  // Unknown
            feature.WriteBit(false); // EnableVoiceChat, not sure
            feature.WriteBit(false); // Unknown
            feature.BitFlush();

            feature.WriteUInt32(1);    // Only seen 1
            feature.WriteUInt32(0);    // Unknown, like random values
            feature.WriteUInt32(0xA);  // Only seen 10
            feature.WriteUInt32(0x3C); // Only seen 60

            //session.Send(feature);

            MiscHandler.SendMOTD(ref session);

            //if (pChar.GuildGuid != 0)
            {
                //Guild guild = GuildMgr.GetGuildByGuid(pChar.GuildGuid);
                //if (guild != null)
                {
                    //var member = guild.GetMember(pChar.Guid);
                    //pChar.SetInGuild(pChar.GuildGuid);
                    //pChar.SetGuildRank(member.RankId);
                    //pChar.SetGuildLevel(guild.GetLevel());
                    //guild.HandleMemberLogin(pChar);
                }
                //else
                {
                    //pChar.SetInGuild(0);
                    //pChar.SetGuildRank(0);
                    //pChar.SetGuildLevel(0);
                }
            }

            PacketWriter data = new PacketWriter(Opcodes.SMSG_LearnedDanceMoves);
            data.WriteUInt64(0);
            //session.Send(data);

            //hotfix

            pChar.SendInitialPacketsBeforeAddToMap();
            /*
             //Show cinematic at the first time that player login
            if (!pCurrChar->getCinematic())
            {
                pCurrChar->setCinematic(1);

                if (ChrClassesEntry cEntry = sChrClassesStore.LookupEntry(pCurrChar->getClass()))
                {
                    if (cEntry->CinematicSequence)
                        pCurrChar->SendCinematicStart(cEntry->CinematicSequence);
                    else if (ChrRacesEntry rEntry = sChrRacesStore.LookupEntry(pCurrChar->getRace()))
                    pCurrChar->SendCinematicStart(rEntry->CinematicSequence);

                    // send new char string if not empty
                    if (!sWorld->GetNewCharString().empty())
                        chH.PSendSysMessage("%s", sWorld->GetNewCharString().c_str());
                }
            }
            if (Group* group = pCurrChar->GetGroup())
            {
                if (group->isLFGGroup())
                {
                    LfgDungeonSet Dungeons;
                    Dungeons.insert(sLFGMgr->GetDungeon(group->GetGUID()));
                    sLFGMgr->SetSelectedDungeons(pCurrChar->GetGUID(), Dungeons);
                    sLFGMgr->SetState(pCurrChar->GetGUID(), sLFGMgr->GetState(group->GetGUID()));
                }
            }
            */
            if (!pChar.GetMap().AddPlayer(pChar))//|| !pCurrChar.CheckInstanceLoginValid())
            {
                //AreaTrigger at = sObjectMgr->GetGoBackTrigger(pCurrChar->GetMapId());
                //if (at)
                    //pCurrChar->TeleportTo(at->target_mapId, at->target_X, at->target_Y, at->target_Z, pCurrChar->GetOrientation());
                //else
                    //pCurrChar->TeleportTo(pCurrChar->m_homebindMapId, pCurrChar->m_homebindX, pCurrChar->m_homebindY, pCurrChar->m_homebindZ, pCurrChar->GetOrientation());
            }
            ObjMgr.AddObject(pChar);

            pChar.SendInitialPacketsAfterAddToMap();

            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_UPD_CHAR_ONLINE);
            stmt.AddValue(0, pChar.GetGUIDLow());
            DB.Characters.Execute(stmt);

            stmt = DB.Auth.GetPreparedStatement(LoginStatements.Upd_AccountOnline);
            stmt.AddValue(0, session.GetAccountId());
            DB.Auth.Execute(stmt);

            //pCurrChar->SetInGameTime(getMSTime());

            // announce group about member online (must be after add to player list to receive announce to self)
            //if (Group* group = pCurrChar->GetGroup())
            {
                //pCurrChar->groupInfo.group->SendInit(this); // useless
                //group->SendUpdate();
                //group->ResetMaxEnchantingLevel();
            }

            // friend status
            //sSocialMgr->SendFriendStatus(pCurrChar, FRIEND_ONLINE, pCurrChar->GetGUIDLow(), true);

            // Place character in world (and load zone) before some object loading
            //pCurrChar->LoadCorpse();

            // setting Ghost+speed if dead
            //if (pCurrChar->m_deathState != ALIVE)
            {
                // not blizz like, we must correctly save and load player instead...
                //if (pCurrChar->getRace() == RACE_NIGHTELF)
                //pCurrChar->CastSpell(pCurrChar, 20584, true, 0);// auras SPELL_AURA_INCREASE_SPEED(+speed in wisp form), SPELL_AURA_INCREASE_SWIM_SPEED(+swim speed in wisp form), SPELL_AURA_TRANSFORM (to wisp form)
                //pCurrChar->CastSpell(pCurrChar, 8326, true, 0);     // auras SPELL_AURA_GHOST, SPELL_AURA_INCREASE_SPEED(why?), SPELL_AURA_INCREASE_SWIM_SPEED(why?)

                //pCurrChar->SendMovementSetWaterWalking(true);
            }

            //pCurrChar->ContinueTaxiFlight();

            // reset for all pets before pet loading
            //if (pChar.HasAtLoginFlag(AtLoginFlags.ResetPetTalents))
            //resetTalentsForAllPetsOf(pCurrChar);

            // Load pet if any (if player not alive and in taxi flight or another then pet will remember as temporary unsummoned)
            //pCurrChar->LoadPet();

            // Set FFA PvP for non GM in non-rest mode
            //if (sWorld->IsFFAPvPRealm() && !pCurrChar->isGameMaster() && !pCurrChar->HasFlag(PLAYER_FLAGS, PLAYER_FLAGS_RESTING))
            //pCurrChar->SetByteFlag(UNIT_FIELD_BYTES_2, 1, UNIT_BYTE2_FLAG_FFA_PVP);

            //if (pChar.HasFlag(PlayerFields.PlayerFlags, PlayerFlags.ContestedPVP))
            //pChar->SetContestedPvP();

            // Apply at_login requests
            if (pChar.HasAtLoginFlag(AtLoginFlags.ResetSpells))
            {
                //pChar.resetSpells();
                pChar.SendNotification(CypherStrings.ResetSpells);
            }

            if (pChar.HasAtLoginFlag(AtLoginFlags.ResetTalents))
            {
                //pChar.ResetTalents(true);
                //pChar.SendTalentsInfoData(false);              // original talents send already in to SendInitialPacketsBeforeAddToMap, resend reset state
                pChar.SendNotification(CypherStrings.ResetTalents);
            }

            if (pChar.HasAtLoginFlag(AtLoginFlags.LoginFirst))
                pChar.RemoveAtLoginFlag(AtLoginFlags.LoginFirst);

            // show time before shutdown if shutdown planned.
            //if (sWorld->IsShuttingDown())
            //sWorld->ShutdownMsg(true, pChar);

            //if (sWorld->getBoolConfig(CONFIG_ALL_TAXI_PATHS))
            //pChar->SetTaxiCheater(true);

            if (pChar.isGameMaster())
                pChar.SendNotification(CypherStrings.GmOn);

            //string IP_str = GetRemoteAddress();
            Log.outDebug("Account: {0} (IP: {1}) Login Character:[{2}] (GUID: {3}) Level: {4}",
                session.GetAccountId(), 0, pChar.GetName(), pChar.GetGUIDLow(), pChar.getLevel());

            //if (!pChar->IsStandState() && !pChar->HasUnitState(UNIT_STATE_STUNNED))
            //pChar->SetStandState(UNIT_STAND_STATE_STAND);

            //m_playerLoading = false;

            //sScriptMgr->OnPlayerLogin(pCurrChar);
        }
    }
}
