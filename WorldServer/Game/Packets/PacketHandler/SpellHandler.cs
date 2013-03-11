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
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;
using WorldServer.Network;
using Framework.Logging;
using WorldServer.Game.Spells;
using Framework.Utility;

namespace WorldServer.Game.Packets
{


    public class SpellHandler : Cypher
    {
        /*
        [ClientOpcode(Opcodes.CMSG_CastSpell)]
        public static void HandleCastSpell(ref PacketReader packet, ref WorldSession session)
        {
            uint spellId, glyphIndex;
            uint castCount, castFlags;

            castCount = packet.ReadUInt32();
            spellId = packet.ReadUInt32();
            glyphIndex = packet.ReadUInt32();
            castFlags = packet.ReadUInt32();

            //Log.outDebug(LOG_FILTER_NETWORKIO, "WORLD: got cast spell packet, castCount: %u, spellId: %u, castFlags: %u, data length = %u", castCount, spellId, castFlags, (uint32)recvPacket.size());
            Player _player = session.GetPlayer();
            // ignore for remote control state (for player case)
            Unit mover = _player.m_mover;
            if (mover != _player && mover.GetTypeId() == ObjectType.Player)
                return;

            SpellInfo spellInfo = Cypher.SpellMgr.GetSpellInfo(spellId);
            if (spellInfo == null)
            {
                //sLog->outError(LOG_FILTER_NETWORKIO, "WORLD: unknown spell id %u", spellId);
                return;
            }

            if (mover.GetTypeId() == ObjectType.Player)
            {
                // not have spell in spellbook or spell passive and not casted by client
                if (!mover.ToPlayer().HasActiveSpell(spellId) || spellInfo.IsPassive())
                {
                    //cheater? kick? ban?
                    return;
                }
            }
            else
            {
                // not have spell in spellbook or spell passive and not casted by client
                if ((mover.GetTypeId() == ObjectType.Unit && !mover.ToCreature().HasSpell(spellId)) || spellInfo.IsPassive())
                {
                    //cheater? kick? ban?
                    return;
                }
            }

            var swaps = mover.GetAuraEffectsByType(AuraType.OverrideActionbarSpells);
            var swaps2 = mover.GetAuraEffectsByType(AuraType.OverrideActionbarSpells2);
            if (!swaps2.empty())
                swaps.AddRange(swaps2);

            if (!swaps.empty())
            {
                foreach (var itr in swaps)
                {
                    if (itr.IsAffectingSpell(spellInfo))
                    {
                        SpellInfo newInfo = Cypher.SpellMgr.GetSpellInfo(itr.GetAmount());
                        if (newInfo != null)
                        {
                            spellInfo = newInfo;
                            spellId = newInfo.Id;
                        }
                        break;
                    }
                }
            }

            // Client is resending autoshot cast opcode when other spell is casted during shoot rotation
            // Skip it to prevent "interrupt" message
            if (spellInfo.IsAutoRepeatRangedSpell() && _player.GetCurrentSpell(CURRENT_AUTOREPEAT_SPELL)
                && _player.GetCurrentSpell(CURRENT_AUTOREPEAT_SPELL).m_spellInfo == spellInfo)
            {
                return;
            }

            // can't use our own spells when we're in possession of another unit,
            if (_player.isPossessing())
            {
                return;
            }

            // client provided targets
            SpellCastTargets targets;
            targets.Read(ref packet, mover);
            //HandleClientCastFlags(recvPacket, castFlags, targets);

            // auto-selection buff level base at target level (in spellInfo)
            if (targets.GetUnitTarget() != null)
            {
                SpellInfo actualSpellInfo = spellInfo.GetAuraRankForLevel(targets.GetUnitTarget()->getLevel());

                // if rank not found then function return NULL but in explicit cast case original spell can be casted and later failed with appropriate error message
                if (actualSpellInfo != null)
                    spellInfo = actualSpellInfo;
            }

            Spell spell = new Spell(mover, spellInfo, TriggerCastFlags.None, 0, false);
            spell.m_cast_count = (byte)castCount;                       // set count of casts
            spell.m_glyphIndex = glyphIndex;
            spell.prepare(targets);
        }
        */
    }
}
