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

using Framework.Constants;
using Framework.Network;
using WorldServer.Game.Commands;
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;
using Framework.Logging;

namespace WorldServer.Game.Packets
{

    public class ChatHandler : Cypher
    {
        [ClientOpcode(Opcodes.CMSG_MessageChatSay)]
        [ClientOpcode(Opcodes.CMSG_MessageChatWhisper)]
        [ClientOpcode(Opcodes.CMSG_MessageChatYell)]
        [ClientOpcode(Opcodes.CMSG_MessageChatGuild)]
        [ClientOpcode(Opcodes.CMSG_MessageChatOfficer)]
        [ClientOpcode(Opcodes.CMSG_MessageChatParty)]
        //[Opcode(Message.CMSG_MessageChatPartyLeader)]
        [ClientOpcode(Opcodes.CMSG_MessageChatRaid)]
        public static void HandleChatMessage(ref PacketReader packet, ref WorldSession session)
        {
            ChatMsg type;
            switch (packet.Opcode)
            {
                case Opcodes.CMSG_MessageChatGuild:
                    type = ChatMsg.Guild;
                    break;
                case Opcodes.CMSG_MessageChatOfficer:
                    type = ChatMsg.Officer;
                    break;
                case Opcodes.CMSG_MessageChatParty:
                    type = ChatMsg.Party;
                    break;
                case Opcodes.CMSG_MessageChatWhisper:
                    type = ChatMsg.Whisper;
                    break;
                case Opcodes.CMSG_MessageChatYell:
                    type = ChatMsg.Yell;
                    break;
                case Opcodes.CMSG_MessageChatSay:
                default:
                    type = ChatMsg.Say;
                    break;
            }

            if (type >= ChatMsg.Max)
            {
                Log.outError("CHAT: Wrong message type received: {0}", type);
                return;
            }
            Player sender = session.GetPlayer();

            Language lang;

            if (type != ChatMsg.Emote && type != ChatMsg.Afk && type != ChatMsg.Dnd)
            {
                lang = (Language)packet.ReadUInt32();
                /*
                // prevent talking at unknown language (cheating)
                LanguageDesc const* langDesc = GetLanguageDescByID(lang);
                if (!langDesc)
                {
                    SendNotification(LANG_UNKNOWN_LANGUAGE);
                    recvData.rfinish();
                    return;
                }

                if (langDesc->skill_id != 0 && !sender->HasSkill(langDesc->skill_id))
                {
                    // also check SPELL_AURA_COMPREHEND_LANGUAGE (client offers option to speak in that language)
                    Unit::AuraEffectList const& langAuras = sender->GetAuraEffectsByType(SPELL_AURA_COMPREHEND_LANGUAGE);
                    bool foundAura = false;
                    for (Unit::AuraEffectList::const_iterator i = langAuras.begin(); i != langAuras.end(); ++i)
                    {
                        if ((*i)->GetMiscValue() == int32(lang))
                        {
                            foundAura = true;
                            break;
                        }
                    }
                    if (!foundAura)
                    {
                        SendNotification(LANG_NOT_LEARNED_LANGUAGE);
                        recvData.rfinish();
                        return;
                    }
                }
                */
                if (lang == Language.Addon)
                {
                    //if (sWorld->getBoolConfig(CONFIG_CHATLOG_ADDON))
                    {
                        //std::string msg = "";
                        //recvData >> msg;

                        //if (msg.empty())
                        //return;

                        // sScriptMgr->OnPlayerChat(sender, uint32(CHAT_MSG_ADDON), lang, msg);
                    }

                    // Disabled addon channel?
                    //if (!sWorld->getBoolConfig(CONFIG_ADDON_CHANNEL))
                    //return;
                }
                // LANG_ADDON should not be changed nor be affected by flood control
                else
                {
                    // send in universal language if player in .gm on mode (ignore spell effects)
                    if (sender.isGameMaster())
                        lang = Language.Universal;
                    /*
                else
                {
                    // send in universal language in two side iteration allowed mode
                    //if (sWorld->getBoolConfig(CONFIG_ALLOW_TWO_SIDE_INTERACTION_CHAT))
                    //lang = LANG_UNIVERSAL;
                    //else
                    //{
                    switch (type)
                    {
                        case CHAT_MSG_PARTY:
                        case CHAT_MSG_PARTY_LEADER:
                        case CHAT_MSG_RAID:
                        case CHAT_MSG_RAID_LEADER:
                        case CHAT_MSG_RAID_WARNING:
                        // allow two side chat at group channel if two side group allowed
                        if (sWorld->getBoolConfig(CONFIG_ALLOW_TWO_SIDE_INTERACTION_GROUP))
                            lang = LANG_UNIVERSAL;
                            break;
                        case CHAT_MSG_GUILD:
                        case CHAT_MSG_OFFICER:
                        // allow two side chat at guild channel if two side guild allowed
                        if (sWorld->getBoolConfig(CONFIG_ALLOW_TWO_SIDE_INTERACTION_GUILD))
                            lang = LANG_UNIVERSAL;
                        break;
                    }
                    

            // but overwrite it by SPELL_AURA_MOD_LANGUAGE auras (only single case used)
            Unit::AuraEffectList const& ModLangAuras = sender->GetAuraEffectsByType(SPELL_AURA_MOD_LANGUAGE);
            if (!ModLangAuras.empty())
                lang = ModLangAuras.front()->GetMiscValue();
                }

        //if (!sender->CanSpeak())
        {
            //std::string timeStr = secsToTimeString(m_muteTime - time(NULL));
            //SendNotification(GetCypherString(LANG_WAIT_BEFORE_SPEAKING), timeStr.c_str());
            //recvData.rfinish(); // Prevent warnings
            //return;
        }
                */
                }
            }
            else
                lang = Language.Universal;

            //if (sender->HasAura(1852) && type != CHAT_MSG_WHISPER)
            {
                //recvData.rfinish();
                //SendNotification(GetCypherString(LANG_GM_SILENCE), sender->GetName());
                //return;
            }
            uint textLength = 0;
            uint receiverLength = 0;
            string to = string.Empty;
            string channel = string.Empty;
            string msg = string.Empty;
            bool ignoreChecks = false;
            switch (type)
            {
                case ChatMsg.Say:
                case ChatMsg.Emote:
                case ChatMsg.Yell:
                case ChatMsg.Party:
                case ChatMsg.Guild:
                case ChatMsg.Officer:
                case ChatMsg.Raid:
                case ChatMsg.Raid_Warning:
                case ChatMsg.Battleground:
                    textLength = packet.GetBits<uint>(9);
                    msg = packet.ReadString(textLength);
                    break;
                case ChatMsg.Whisper:
                    receiverLength = packet.GetBits<uint>(10);
                    textLength = packet.GetBits<uint>(9);
                    to = packet.ReadString(receiverLength);
                    msg = packet.ReadString(textLength);
                    break;
                case ChatMsg.Channel:
                    receiverLength = packet.GetBits<uint>(10);
                    textLength = packet.GetBits<uint>(9);
                    msg = packet.ReadString(textLength);
                    channel = packet.ReadString(receiverLength);
                    break;
                case ChatMsg.Afk:
                case ChatMsg.Dnd:
                    textLength = packet.GetBits<uint>(9);
                    msg = packet.ReadString(textLength);
                    ignoreChecks = true;
                    break;
            }
            if (!ignoreChecks)
            {
                if (msg == string.Empty)
                    return;

                if (CommandManager.TryParse(msg, session))
                    return;

                if (msg == string.Empty)
                    return;
            }

            switch (type)
            {
                case ChatMsg.Say:
                case ChatMsg.Emote:
                case ChatMsg.Yell:
                    //if (sender->getLevel() < sWorld->getIntConfig(CONFIG_CHAT_SAY_LEVEL_REQ))
                    {
                        //SendNotification(GetCypherString(LANG_SAY_REQ), sWorld->getIntConfig(CONFIG_CHAT_SAY_LEVEL_REQ));
                        //return;
                    }

                    if (type == ChatMsg.Say)
                        sender.Say(msg, lang);
                    //else if (type == ChatMsg.EMOTE)
                    //sender.TextEmote(msg);
                    else if (type == ChatMsg.Yell)
                        sender.Yell(msg, lang);
                    break;
                case ChatMsg.Whisper:
                        //if (sender->getLevel() < sWorld->getIntConfig(CONFIG_CHAT_WHISPER_LEVEL_REQ))
                        {
                            //SendNotification(GetCypherString(LANG_WHISPER_REQ), sWorld->getIntConfig(CONFIG_CHAT_WHISPER_LEVEL_REQ));
                            //return;
                        }

                        //if (!normalizePlayerName(to))
                        {
                            //SendPlayerNotFoundNotice(to);
                            //break;
                        }

                        Player receiver = ObjMgr.FindPlayerByName(to);
                        bool senderIsPlayer = ObjMgr.IsPlayerAccount(session.GetSecurity());
                        bool receiverIsPlayer = ObjMgr.IsPlayerAccount(receiver != null ? receiver.GetSession().GetSecurity() : AccountTypes.Player);
                        if (receiver == null || (senderIsPlayer && !receiverIsPlayer))// && !receiver.isAcceptWhispers() && !receiver.IsInWhisperWhiteList(sender->GetGUID())))todo fixme
                        {
                            SendPlayerNotFoundNotice(session, to);
                            return;
                        }

                        //if (!sWorld->getBoolConfig(CONFIG_ALLOW_TWO_SIDE_INTERACTION_CHAT) && senderIsPlayer && receiverIsPlayer)
                        //if (GetPlayer()->GetTeam() != receiver->GetTeam())
                        {
                            //SendWrongFactionNotice();
                            //return;
                        }

                        //if (GetPlayer()->HasAura(1852) && !receiver->isGameMaster())
                        {
                            //SendNotification(GetCypherString(LANG_GM_SILENCE), GetPlayer()->GetName());
                            //return;
                        }

                        // If player is a Gamemaster and doesn't accept whisper, we auto-whitelist every player that the Gamemaster is talking to
                        //if (!senderIsPlayer && !sender->isAcceptWhispers() && !sender->IsInWhisperWhiteList(receiver->GetGUID()))
                        //sender->AddWhisperWhiteList(receiver->GetGUID());

                        session.GetPlayer().Whisper(msg, lang, receiver.GetGUID());
                        break;
                /*
                case ChatMsg.Party:
                case ChatMsg.PartyLEADER:
                {
                    // if player is in battleground, he cannot say to battleground members by /p
                    Group* group = GetPlayer()->GetOriginalGroup();
                    if (!group)
                    {
                        group = _player->GetGroup();
                        if (!group || group->isBGGroup())
                            return;
                    }

                    if (group->IsLeader(GetPlayer()->GetGUID()))
                        type = CHAT_MSG_PARTY_LEADER;

                    sScriptMgr->OnPlayerChat(GetPlayer(), type, lang, msg, group);

                    WorldPacket data;
                    ChatHandler::FillMessageData(&data, this, uint8(type), lang, NULL, 0, msg.c_str(), NULL);
                    group->BroadcastPacket(&data, false, group->GetMemberGroup(GetPlayer()->GetGUID()));
                } break;
                case CHAT_MSG_GUILD:
                {
                    if (GetPlayer()->GetGuildId())
                    {
                        if (Guild* guild = sGuildMgr->GetGuildById(GetPlayer()->GetGuildId()))
                        {
                            sScriptMgr->OnPlayerChat(GetPlayer(), type, lang, msg, guild);

                            guild->BroadcastToGuild(this, false, msg, lang == LANG_ADDON ? LANG_ADDON : LANG_UNIVERSAL);
                        }
                    }
                } break;
                case CHAT_MSG_OFFICER:
                {
                    if (GetPlayer()->GetGuildId())
                    {
                        if (Guild* guild = sGuildMgr->GetGuildById(GetPlayer()->GetGuildId()))
                        {
                            sScriptMgr->OnPlayerChat(GetPlayer(), type, lang, msg, guild);

                            guild->BroadcastToGuild(this, true, msg, lang == LANG_ADDON ? LANG_ADDON : LANG_UNIVERSAL);
                        }
                    }
                } break;
                case CHAT_MSG_RAID:
                case CHAT_MSG_RAID_LEADER:
                {
                    // if player is in battleground, he cannot say to battleground members by /ra
                    Group* group = GetPlayer()->GetOriginalGroup();
                    if (!group)
                    {
                        group = GetPlayer()->GetGroup();
                        if (!group || group->isBGGroup() || !group->isRaidGroup())
                            return;
                    }

                    if (group->IsLeader(GetPlayer()->GetGUID()))
                        type = CHAT_MSG_RAID_LEADER;

                    sScriptMgr->OnPlayerChat(GetPlayer(), type, lang, msg, group);

                    WorldPacket data;
                    ChatHandler::FillMessageData(&data, this, uint8(type), lang, "", 0, msg.c_str(), NULL);
                    group->BroadcastPacket(&data, false);
                } break;
                case CHAT_MSG_RAID_WARNING:
                {
                    Group* group = GetPlayer()->GetGroup();
                    if (!group || !group->isRaidGroup() || !(group->IsLeader(GetPlayer()->GetGUID()) || group->IsAssistant(GetPlayer()->GetGUID())) || group->isBGGroup())
                        return;

                    sScriptMgr->OnPlayerChat(GetPlayer(), type, lang, msg, group);

                    WorldPacket data;
                    //in battleground, raid warning is sent only to players in battleground - code is ok
                    ChatHandler::FillMessageData(&data, this, CHAT_MSG_RAID_WARNING, lang, "", 0, msg.c_str(), NULL);
                    group->BroadcastPacket(&data, false);
                } break;
                case CHAT_MSG_BATTLEGROUND:
                case CHAT_MSG_BATTLEGROUND_LEADER:
                {
                    // battleground raid is always in Player->GetGroup(), never in GetOriginalGroup()
                    Group* group = GetPlayer()->GetGroup();
                    if (!group || !group->isBGGroup())
                        return;

                    if (group->IsLeader(GetPlayer()->GetGUID()))
                        type = CHAT_MSG_BATTLEGROUND_LEADER;

                    sScriptMgr->OnPlayerChat(GetPlayer(), type, lang, msg, group);

                    WorldPacket data;
                    ChatHandler::FillMessageData(&data, this, uint8(type), lang, "", 0, msg.c_str(), NULL);
                    group->BroadcastPacket(&data, false);
                } break;
                case CHAT_MSG_CHANNEL:
                {
                    if (AccountMgr::IsPlayerAccount(GetSecurity()))
                    {
                        if (_player->getLevel() < sWorld->getIntConfig(CONFIG_CHAT_CHANNEL_LEVEL_REQ))
                        {
                            SendNotification(GetCypherString(LANG_CHANNEL_REQ), sWorld->getIntConfig(CONFIG_CHAT_CHANNEL_LEVEL_REQ));
                            return;
                        }
                    }

                    if (ChannelMgr* cMgr = channelMgr(_player->GetTeam()))
                    {

                        if (Channel* chn = cMgr->GetChannel(channel, _player))
                        {
                            sScriptMgr->OnPlayerChat(_player, type, lang, msg, chn);

                            chn->Say(_player->GetGUID(), msg.c_str(), lang);
                        }
                    }
                } break;
                case CHAT_MSG_AFK:
                {
                    if ((msg.empty() || !_player->isAFK()) && !_player->isInCombat())
                    {
                        if (!_player->isAFK())
                        {
                            if (msg.empty())
                                msg  = GetCypherString(LANG_PLAYER_AFK_DEFAULT);
                            _player->afkMsg = msg;
                        }

                        sScriptMgr->OnPlayerChat(_player, type, lang, msg);

                        _player->ToggleAFK();
                        if (_player->isAFK() && _player->isDND())
                            _player->ToggleDND();
                    }
                } break;
                case CHAT_MSG_DND:
                {
                    if (msg.empty() || !_player->isDND())
                    {
                        if (!_player->isDND())
                        {
                            if (msg.empty())
                                msg = GetCypherString(LANG_PLAYER_DND_DEFAULT);
                            _player->dndMsg = msg;
                        }

                        sScriptMgr->OnPlayerChat(_player, type, lang, msg);

                        _player->ToggleDND();
                        if (_player->isDND() && _player->isAFK())
                            _player->ToggleAFK();
                    }
                } break;
                */
                default:
                    Log.outError("CHAT: unknown message type {0}, lang: {1}", type, lang);
                    break;

            }
        }

        static void SendPlayerNotFoundNotice(WorldSession session, string name)
        {
            PacketWriter data = new PacketWriter(Opcodes.SMSG_ChatPlayerNotFound);
            data.WriteString(name);
            session.Send(data);
        }

        public static void SendSysMessage(WorldObject obj, CypherStrings message, params object[] args)
        {
            SendSysMessage(obj.GetSession(), ObjMgr.GetCypherString(message), args);
        }
        public static void SendSysMessage(WorldObject obj, string message, params object[] args)
        {
            SendSysMessage(obj.GetSession(), message, args);
        }
        public static void SendSysMessage(WorldSession session, string message, params object[] args)
        {
            string msg = string.Format(message, args);
            SendChatMessage(session, ChatMsg.System, Language.Universal, msg);
        }
        public static void SendSysMessage(WorldSession session, CypherStrings message, params object[] args)
        {
            string msg = string.Format(ObjMgr.GetCypherString(message), args);
            SendChatMessage(session, ChatMsg.System, Language.Universal, msg);
        }
        public static void SendChatMessage(WorldSession session, ChatMsg type, Language language, string chatMessage)
        {
            PacketWriter messageChat = new PacketWriter(Opcodes.SMSG_MessageChat);
            ulong guid = session.GetPlayer().GetGUIDLow();

            messageChat.WriteUInt8(type);
            messageChat.WriteUInt32(language);
            messageChat.WriteUInt64(guid);
            messageChat.WriteUInt32(0);//time?
            messageChat.WriteUInt64(guid);
            messageChat.WriteUInt32(chatMessage.Length + 1);
            messageChat.WriteCString(chatMessage);
            messageChat.WriteUInt16(0);

            session.Send(messageChat);
        }

    }
}
