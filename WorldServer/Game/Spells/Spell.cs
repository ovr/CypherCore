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
using Framework.Constants;
using Framework.Logging;
using Framework.Network;
using Framework.Utility;
using WorldServer.Game.Managers;
using WorldServer.Game.WorldEntities;
using System;

namespace WorldServer.Game.Spells
{
    public class Spell : Cypher
    {
        public Spell(Unit caster, SpellInfo info, TriggerCastFlags triggerFlags, ulong originalCasterGUID = 0, bool skipCheck = false)
        {

            m_spellInfo = SpellMgr.GetSpellForDifficultyFromSpell(info, caster);
            m_caster = Convert.ToBoolean(info.AttributesEx6 & SpellAttr6.CastByCharmer) ? null : caster;//&& caster->GetCharmerOrOwner()) ? caster->GetCharmerOrOwner() : caster
            m_spellValue = new SpellValue(m_spellInfo);

            m_customError = SpellCustomErrors.None;
            m_skipCheck = skipCheck;
            m_selfContainer = null;
            m_referencedFromCurrentSpell = false;
            m_executedCurrently = false;
            m_needComboPoints = m_spellInfo.NeedsComboPoints();
            m_comboPointGain = 0;
            m_delayStart = 0;
            m_delayAtDamageCount = 0;

            m_applyMultiplierMask = 0;
            m_auraScaleMask = 0;

            // Get data for type of attack
            switch (m_spellInfo.DmgClass)
            {
                case SpellDmgClass.Melee:
                    if (Convert.ToBoolean(m_spellInfo.AttributesEx3 & SpellAttr3.ReqOffhand))
                        m_attackType = WeaponAttackType.OffAttack;
                    else
                        m_attackType = WeaponAttackType.BaseAttack;
                    break;
                case SpellDmgClass.Ranged:
                    m_attackType = m_spellInfo.IsRangedWeaponSpell() ? WeaponAttackType.RangedAttack : WeaponAttackType.BaseAttack;
                    break;
                default:
                    // Wands
                    if (Convert.ToBoolean(m_spellInfo.AttributesEx2 & SpellAttr2.AutorepeatFlag))
                        m_attackType = WeaponAttackType.RangedAttack;
                    else
                        m_attackType = WeaponAttackType.BaseAttack;
                    break;
            }

            m_spellSchoolMask = info.GetSchoolMask();           // Can be override for some spell (wand shoot for example)

            if (m_attackType == WeaponAttackType.RangedAttack)
            {
                if ((m_caster.getClassMask() & SharedConst.ClassMaskWandUsers) != 0 && m_caster.GetTypeId() == ObjectType.Player)
                {
                    Item pItem = m_caster.ToPlayer().GetWeaponForAttack(WeaponAttackType.RangedAttack);
                    //if (pItem != null)
                        //m_spellSchoolMask = (SpellSchoolMask)(1 << (int)pItem.ItemInfo.ItemSparse.DamageType);
                }
            }

            if (originalCasterGUID != 0)
                m_originalCasterGUID = originalCasterGUID;
            else
                m_originalCasterGUID = m_caster.GetGUID();

            if (m_originalCasterGUID == m_caster.GetGUID())
                m_originalCaster = m_caster;
            else
            {
                m_originalCaster = ObjMgr.GetObject<Unit>(m_caster, m_originalCasterGUID);
                if (m_originalCaster != null && !m_originalCaster.IsInWorld)
                    m_originalCaster = null;
            }

            m_spellState = SpellState.None;
            _triggeredCastFlags = triggerFlags;
            if (Convert.ToBoolean(info.AttributesEx4 & SpellAttr4.Triggered))
                _triggeredCastFlags = TriggerCastFlags.FullMask;

            m_CastItem = null;
            m_castItemGUID = 0;

            unitTarget = null;
            itemTarget = null;
            gameObjTarget = null;
            focusObject = null;
            m_cast_count = 0;
            m_glyphIndex = 0;
            m_preCastSpell = 0;
            //m_triggeredByAuraSpell  = null;
            //m_spellAura = null;

            //Auto Shot & Shoot (wand)
            m_autoRepeat = m_spellInfo.IsAutoRepeatRangedSpell();

            m_runesState = 0;
            m_powerCost = 0;                                        // setup to correct value in Spell::prepare, must not be used before.
            m_casttime = 0;                                         // setup to correct value in Spell::prepare, must not be used before.
            m_timer = 0;                                            // will set to castime in prepare

            //m_channelTargetEffectMask = 0;

            // Determine if spell can be reflected back to the caster
            // Patch 1.2 notes: Spell Reflection no longer reflects abilities
            m_canReflect = m_spellInfo.DmgClass == SpellDmgClass.Magic && !Convert.ToBoolean(m_spellInfo.Attributes & SpellAttr0.Ability)
                && !Convert.ToBoolean(m_spellInfo.AttributesEx & SpellAttr1.CantBeReflected) && !Convert.ToBoolean(m_spellInfo.Attributes & SpellAttr0.UnaffectedByInvulnerability)
                && !m_spellInfo.IsPassive() && !m_spellInfo.IsPositive();

            CleanupTargetList();

            for (var i = 0; i < SharedConst.MaxSpellEffects; ++i)
                m_destTargets[i] = new SpellDestination(m_caster);
        }
        void CleanupTargetList()
        {
            m_UniqueTargetInfo.Clear();
            //m_UniqueGOTargetInfo.clear();
            //m_UniqueItemInfo.Clear();
            m_delayMoment = 0;
        }
        bool IsTriggered() { return Convert.ToBoolean(_triggeredCastFlags & TriggerCastFlags.FullMask); }
        bool IsNeedSendToClient()
        {
            return m_spellInfo.SpellVisual[0] != 0 || m_spellInfo.SpellVisual[1] != 0 || m_spellInfo.IsChanneled() ||
                Convert.ToBoolean(m_spellInfo.AttributesEx8 & SpellAttr8.AuraSendAmount) || m_spellInfo.Speed > 0.0f;// || (!m_triggeredByAuraSpell && !IsTriggered());
        }
        public void Prepare(SpellCastTargets targets, AuraEffect triggeredByAura)
        {
            if (m_CastItem != null)
                m_castItemGUID = m_CastItem.GetGUID();
            else
                m_castItemGUID = 0;

            //InitExplicitTargets(*targets);

            // Fill aura scaling information
            /*if (m_caster.IsControlledByPlayer() && !m_spellInfo.IsPassive() && m_spellInfo.SpellLevel && !m_spellInfo.IsChanneled() && !(_triggeredCastFlags & TRIGGERED_IGNORE_AURA_SCALING))
            {
                for (uint8 i = 0; i < MAX_SPELL_EFFECTS; ++i)
                {
                    if (m_spellInfo->Effects[i].Effect == SPELL_EFFECT_APPLY_AURA)
                    {
                        // Change aura with ranks only if basepoints are taken from spellInfo and aura is positive
                        if (m_spellInfo->IsPositiveEffect(i))
                        {
                            m_auraScaleMask |= (1 << i);
                            if (m_spellValue->EffectBasePoints[i] != m_spellInfo->Effects[i].BasePoints)
                            {
                                m_auraScaleMask = 0;
                                break;
                            }
                        }
                    }
                }
            }*/

            m_spellState = SpellState.Preparing;

            //if (triggeredByAura)
            //m_triggeredByAuraSpell  = triggeredByAura->GetSpellInfo();

            // create and add update event for this spell
            //SpellEvent* Event = new SpellEvent(this);
            //m_caster->m_Events.AddEvent(Event, m_caster->m_Events.CalculateTime(1));

            //Prevent casting at cast another spell (ServerSide check)
            //if (!(_triggeredCastFlags & TRIGGERED_IGNORE_CAST_IN_PROGRESS) && m_caster->IsNonMeleeSpellCasted(false, true, true) && m_cast_count)
            {
                //SendCastResult(SPELL_FAILED_SPELL_IN_PROGRESS);
                //finish(false);
                //return;
            }

            //if (DisableMgr::IsDisabledFor(DISABLE_TYPE_SPELL, m_spellInfo->Id, m_caster))
            {
                //SendCastResult(SPELL_FAILED_SPELL_UNAVAILABLE);
                //finish(false);
                //return;
            }
            //LoadScripts();

            //if (m_caster is Player)
            //m_caster.ToPlayer().SetSpellModTakingSpell(this, true);
            // Fill cost data (not use power for item casts
            //m_powerCost = m_CastItem != null ? 0 : m_spellInfo.CalcPowerCost(m_caster, m_spellSchoolMask);
            //if (m_caster->GetTypeId() == TYPEID_PLAYER)
            //m_caster->ToPlayer()->SetSpellModTakingSpell(this, false);

            // Set combo point requirement
            //if ((_triggeredCastFlags & TRIGGERED_IGNORE_COMBO_POINTS) || m_CastItem || !m_caster->m_movedPlayer)
            //m_needComboPoints = false;

            SpellCastResult result = SpellCastResult.SpellCastOk;// CheckCast(true);
            if (result != SpellCastResult.SpellCastOk && !m_autoRepeat)          //always cast autorepeat dummy for triggering
            {
                // Periodic auras should be interrupted when aura triggers a spell which can't be cast
                // for example bladestorm aura should be removed on disarm as of patch 3.3.5
                // channeled periodic spells should be affected by this (arcane missiles, penance, etc)
                // a possible alternative sollution for those would be validating aura target on unit state change
                //if (triggeredByAura && triggeredByAura->IsPeriodic() && !triggeredByAura->GetBase()->IsPassive())
                {
                    //SendChannelUpdate(0);
                    //triggeredByAura->GetBase()->SetDuration(0);
                }

                //SendCastResult(result);

                //finish(false);
                //return;
            }

            // Prepare data for triggers
            //prepareDataForTriggerSystem(triggeredByAura);

            //if (m_caster->GetTypeId() == TYPEID_PLAYER)
            //m_caster->ToPlayer()->SetSpellModTakingSpell(this, true);
            // calculate cast time (calculated after first CheckCast check to prevent charge counting for first CheckCast fail)
            m_casttime = m_spellInfo.CalcCastTime(m_caster, this);
            if (m_caster is Player)
            {
                //m_caster.ToPlayer().SetSpellModTakingSpell(this, false);

                // Set casttime to 0 if .cheat casttime is enabled.
                //if (m_caster.ToPlayer().GetCommandStatus(CHEAT_CASTTIME))
                //m_casttime = 0;
            }

            // don't allow channeled spells / spells with cast time to be casted while moving
            // (even if they are interrupted on moving, spells with almost immediate effect get to have their effect processed before movement interrupter kicks in)
            // don't cancel spells which are affected by a SPELL_AURA_CAST_WHILE_WALKING effect
            //if (((m_spellInfo.IsChanneled() || m_casttime) && (m_caster is Player) && m_caster.isMoving() &&
            //m_spellInfo.InterruptFlags & SPELL_INTERRUPT_FLAG_MOVEMENT) && !m_caster.HasAuraTypeWithAffectMask(SPELL_AURA_CAST_WHILE_WALKING, m_spellInfo))
            {
                //SendCastResult(SPELL_FAILED_MOVING);
                //finish(false);
                //return;
            }

            // set timer base at cast time
            //ReSetTimer();

            Log.outDebug("Spell::prepare: spell id {0} source {1} caster {2} customCastFlags {3} mask {4}", m_spellInfo.Id, m_caster.GetEntry(), m_originalCaster != null ? m_originalCaster.GetEntry() : 0, 0, 0);//_triggeredCastFlags, m_targets.GetTargetMask());

            //Containers for channeled spells have to be set
            //TODO:Apply this to all casted spells if needed
            // Why check duration? 29350: channelled triggers channelled
            if (Convert.ToBoolean(_triggeredCastFlags & TriggerCastFlags.CastDirectly) && (!m_spellInfo.IsChanneled() || m_spellInfo.GetMaxDuration() == 0))
                Cast(true);
            else
            {
                // stealth must be removed at cast starting (at show channel bar)
                // skip triggered spell (item equip spell casting and other not explicit character casts/item uses)
                if (!Convert.ToBoolean(_triggeredCastFlags & TriggerCastFlags.IgnoreAuraInterruptFlags) && m_spellInfo.IsBreakingStealth())
                {
                    //m_caster->RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_CAST);
                    for (var i = 0; i < SharedConst.MaxSpellEffects; ++i)
                    {
                        //if (m_spellInfo.Effects[i].GetUsedTargetObjectType() == TARGET_OBJECT_TYPE_UNIT)
                        {
                            //m_caster->RemoveAurasWithInterruptFlags(AURA_INTERRUPT_FLAG_SPELL_ATTACK);
                            //break;
                        }
                    }
                }

                //m_caster->SetCurrentCastedSpell(this);
                SendSpellStart();

                // set target for proper facing
                //if (m_casttime != 0 || m_spellInfo.IsChanneled() && !_triggeredCastFlags & (TriggerCastFlags.IgnoreSetFacing))
                //if (m_caster.GetGUID() != m_targets.GetObjectTargetGUID() && (m_caster is Unit))
                //m_caster.FocusTarget(this, m_targets.GetObjectTargetGUID());

                //if (!_triggeredCastFlags & (TriggerCastFlags.IgnoreGcd))
                //TriggerGlobalCooldown();

                //item: first cast may destroy item and second cast causes crash
                //if (m_casttime == 0 && !m_spellInfo.StartRecoveryTime && m_castItemGUID == 0 && GetCurrentContainer() == CURRENT_GENERIC_SPELL)
                //cast(true);
            }
        }
        public void SetSpellValue(SpellValueMod mod, int value)//todo fixme
        {
            switch (mod)
            {
                case SpellValueMod.BASE_POINT0:
                    //m_spellValue.EffectBasePoints[0] = m_spellInfo.Effects[0].CalcBaseValue(value);
                    break;
                case SpellValueMod.BASE_POINT1:
                    //m_spellValue.EffectBasePoints[1] = m_spellInfo.Effects[1].CalcBaseValue(value);
                    break;
                case SpellValueMod.BASE_POINT2:
                    //m_spellValue.EffectBasePoints[2] = m_spellInfo.Effects[2].CalcBaseValue(value);
                    break;
                case SpellValueMod.RADIUS_MOD:
                    m_spellValue.RadiusMod = (float)value / 10000;
                    break;
                case SpellValueMod.MAX_TARGETS:
                    m_spellValue.MaxAffectedTargets = (uint)value;
                    break;
                case SpellValueMod.AURA_STACK:
                    m_spellValue.AuraStackAmount = (byte)value;
                    break;
            }
        }
        void Cast(bool skipCheck)
        {
            // update pointers base at GUIDs to prevent access to non-existed already object
            //UpdatePointers();
            
            // cancel at lost explicit target during cast
            //if (m_targets.GetObjectTargetGUID() && !m_targets.GetObjectTarget())
            {
                //cancel();
                //return;
            }
            Player playerCaster = m_caster.ToPlayer();
            if (playerCaster != null)
            {
                // now that we've done the basic check, now run the scripts
                // should be done before the spell is actually executed
                //sScriptMgr->OnPlayerSpellCast(playerCaster, this, skipCheck);

                // As of 3.0.2 pets begin attacking their owner's target immediately
                // Let any pets know we've attacked something. Check DmgClass for harmful spells only
                // This prevents spells such as Hunter's Mark from triggering pet attack
                //if (this->GetSpellInfo()->DmgClass != SPELL_DAMAGE_CLASS_NONE)
                    //if (Pet* playerPet = playerCaster->GetPet())
                        //if (playerPet->isAlive() && playerPet->isControlled() && (m_targets.GetTargetMask() & TARGET_FLAG_UNIT))
                            //playerPet->AI()->OwnerAttacked(m_targets.GetObjectTarget()->ToUnit());
            }
            //SetExecutedCurrently(true);

            //if (!(m_caster is Player) && m_targets.GetUnitTarget() && m_targets.GetUnitTarget() != m_caster)
                //m_caster->SetInFront(m_targets.GetUnitTarget());

            // Should this be done for original caster?
            if (m_caster is Player)
            {
                // Set spell which will drop charges for triggered cast spells
                // if not successfully casted, will be remove in finish(false)
                //m_caster.ToPlayer().SetSpellModTakingSpell(this, true);
            }

            //CallScriptBeforeCastHandlers();

            // skip check if done already (for instant cast spells for example)
            if (!skipCheck)
            {
                /*
                SpellCastResult castResult = CheckCast(false);
                if (castResult != SPELL_CAST_OK)
                {
                    SendCastResult(castResult);
                    SendInterrupted(0);
                    //restore spell mods
                    if (m_caster->GetTypeId() == TYPEID_PLAYER)
                    {
                        m_caster->ToPlayer()->RestoreSpellMods(this);
                        // cleanup after mod system
                        // triggered spell pointer can be not removed in some cases
                        m_caster->ToPlayer()->SetSpellModTakingSpell(this, false);
                    }
                    finish(false);
                    SetExecutedCurrently(false);
                    return;
                }

                // additional check after cast bar completes (must not be in CheckCast)
                // if trade not complete then remember it in trade data
                if (m_targets.GetTargetMask() & TARGET_FLAG_TRADE_ITEM)
                {
                    if (m_caster->GetTypeId() == TYPEID_PLAYER)
                    {
                        if (TradeData* my_trade = m_caster->ToPlayer()->GetTradeData())
                        {
                            if (!my_trade->IsInAcceptProcess())
                            {
                                // Spell will be casted at completing the trade. Silently ignore at this place
                                my_trade->SetSpell(m_spellInfo->Id, m_CastItem);
                                SendCastResult(SPELL_FAILED_DONT_REPORT);
                                SendInterrupted(0);
                                m_caster->ToPlayer()->RestoreSpellMods(this);
                                // cleanup after mod system
                                // triggered spell pointer can be not removed in some cases
                                m_caster->ToPlayer()->SetSpellModTakingSpell(this, false);
                                finish(false);
                                SetExecutedCurrently(false);
                                return;
                            }
                        }
                    }
                }
                */
            }
            /*
            //SelectSpellTargets();

            // Spell may be finished after target map check
            if (m_spellState == SpellState.Finished)
            {
                SendInterrupted(0);
                //restore spell mods
                if (m_caster->GetTypeId() == TYPEID_PLAYER)
                {
                    m_caster->ToPlayer()->RestoreSpellMods(this);
                    // cleanup after mod system
                    // triggered spell pointer can be not removed in some cases
                    m_caster->ToPlayer()->SetSpellModTakingSpell(this, false);
                }
                finish(false);
                SetExecutedCurrently(false);
                return;
            }

    PrepareTriggersExecutedOnHit();

    CallScriptOnCastHandlers();

    // traded items have trade slot instead of guid in m_itemTargetGUID
    // set to real guid to be sent later to the client
    m_targets.UpdateTradeSlotItem();

    if (Player* player = m_caster->ToPlayer())
    {
        if (!(_triggeredCastFlags & TRIGGERED_IGNORE_CAST_ITEM) && m_CastItem)
        {
            player->StartTimedAchievement(ACHIEVEMENT_TIMED_TYPE_ITEM, m_CastItem->GetEntry());
            player->UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_USE_ITEM, m_CastItem->GetEntry());
        }

        player->UpdateAchievementCriteria(ACHIEVEMENT_CRITERIA_TYPE_CAST_SPELL, m_spellInfo->Id);
    }

    if (!(_triggeredCastFlags & TRIGGERED_IGNORE_POWER_AND_REAGENT_COST))
    {
        // Powers have to be taken before SendSpellGo
        TakePower();
        TakeReagents();                                         // we must remove reagents before HandleEffects to allow place crafted item in same slot
    }
    else if (Item* targetItem = m_targets.GetItemTarget())
    {
        /// Not own traded item (in trader trade slot) req. reagents including triggered spell case
        if (targetItem->GetOwnerGUID() != m_caster->GetGUID())
            TakeReagents();
    }

    // CAST SPELL
    SendSpellCooldown();

    PrepareScriptHitHandlers();

    HandleLaunchPhase();

    // we must send smsg_spell_go packet before m_castItem delete in TakeCastItem()...
    SendSpellGo();

    // Okay, everything is prepared. Now we need to distinguish between immediate and evented delayed spells
    if ((m_spellInfo->Speed > 0.0f && !m_spellInfo->IsChanneled()) || m_spellInfo->Id == 14157)
    {
        // Remove used for cast item if need (it can be already NULL after TakeReagents call
        // in case delayed spell remove item at cast delay start
        TakeCastItem();

        // Okay, maps created, now prepare flags
        m_immediateHandled = false;
        m_spellState = SPELL_STATE_DELAYED;
        SetDelayStart(0);

        if (m_caster->HasUnitState(UNIT_STATE_CASTING) && !m_caster->IsNonMeleeSpellCasted(false, false, true))
            m_caster->ClearUnitState(UNIT_STATE_CASTING);
    }
    else
    {
        // Immediate spell, no big deal
        handle_immediate();
    }

    CallScriptAfterCastHandlers();

    if (var spell_triggered = sSpellMgr->GetSpellLinked(m_spellInfo->Id))
    {
        for (std::vector<int32>::const_iterator i = spell_triggered->begin(); i != spell_triggered->end(); ++i)
            if (*i < 0)
                m_caster->RemoveAurasDueToSpell(-(*i));
            else
                m_caster->CastSpell(m_targets.GetUnitTarget() ? m_targets.GetUnitTarget() : m_caster, *i, true);
    }

    if (m_caster->GetTypeId() == TYPEID_PLAYER)
    {
        m_caster->ToPlayer()->SetSpellModTakingSpell(this, false);

        //Clear spell cooldowns after every spell is cast if .cheat cooldown is enabled.
        if (m_caster->ToPlayer()->GetCommandStatus(CHEAT_COOLDOWN))
            m_caster->ToPlayer()->RemoveSpellCooldown(m_spellInfo->Id, true);
    }

    SetExecutedCurrently(false);
             */
        }
        void SelectExplicitTargets()
        {
            // here go all explicit target changes made to explicit targets after spell prepare phase is finished
            Unit target = m_targets.GetUnitTarget();
            if (target != null)
            {
                // check for explicit target redirection, for Grounding Totem for example
                //if (m_spellInfo.ExplicitTargetMask & TARGET_FLAG_UNIT_ENEMY
                    //|| (m_spellInfo.GetExplicitTargetMask() & TARGET_FLAG_UNIT && !m_spellInfo->IsPositive()))
                {
                    //Unit redirect;
                    switch (m_spellInfo.DmgClass)
                    {
                        case SpellDmgClass.Magic:
                            //redirect = m_caster->GetMagicHitRedirectTarget(target, m_spellInfo);
                            break;
                        case SpellDmgClass.Melee:
                        case SpellDmgClass.Ranged:
                            //redirect = m_caster->GetMeleeHitRedirectTarget(target, m_spellInfo);
                            break;
                        default:
                            //redirect = null;
                            break;
                    }
                    //if (redirect != null && (redirect != target))
                        //m_targets.SetUnitTarget(redirect);
                }
            }
        }
        void SelectSpellTargets()
        {
            // select targets for cast phase
            SelectExplicitTargets();

            //uint processedAreaEffectsMask = 0;
            for (var i = 0; i < SharedConst.MaxSpellEffects; ++i)
            {
                // not call for empty effect.
                // Also some spells use not used effect targets for store targets for dummy effect in triggered spells
                if (!m_spellInfo.Effects[i].IsEffect())
                    continue;

                // set expected type of implicit targets to be sent to client
                //uint implicitTargetMask = GetTargetFlagMask(m_spellInfo->Effects[i].TargetA.GetObjectType()) | GetTargetFlagMask(m_spellInfo->Effects[i].TargetB.GetObjectType());
                //if (implicitTargetMask & TARGET_FLAG_UNIT)
                //m_targets.SetTargetFlag(TARGET_FLAG_UNIT);
                //if (implicitTargetMask & (TARGET_FLAG_GAMEOBJECT | TARGET_FLAG_GAMEOBJECT_ITEM))
                //m_targets.SetTargetFlag(TARGET_FLAG_GAMEOBJECT);

                //SelectEffectImplicitTargets(SpellEffIndex(i), m_spellInfo->Effects[i].TargetA, processedAreaEffectsMask);
                //SelectEffectImplicitTargets(SpellEffIndex(i), m_spellInfo->Effects[i].TargetB, processedAreaEffectsMask);

                // Select targets of effect based on effect type
                // those are used when no valid target could be added for spell effect based on spell target type
                // some spell effects use explicit target as a default target added to target map (like SPELL_EFFECT_LEARN_SPELL)
                // some spell effects add target to target map only when target type specified (like SPELL_EFFECT_WEAPON)
                // some spell effects don't add anything to target map (confirmed with sniffs) (like SPELL_EFFECT_DESTROY_ALL_TOTEMS)
                //SelectEffectTypeImplicitTargets(i);

                //if (m_targets.HasDst())
                //AddDestTarget(*m_targets.GetDst(), i);

                if (m_spellInfo.IsChanneled())
                {
                    byte mask = (byte)(1 << i);
                    foreach (var ihit in m_UniqueTargetInfo)
                    {
                        //if (ihit.effectMask & mask)
                        {
                            //m_channelTargetEffectMask |= mask;
                            //break;
                        }
                    }
                }
                //else if (m_auraScaleMask)
                {
                    // bool checkLvl = !m_UniqueTargetInfo.empty();
                    //for (std::list<TargetInfo>::iterator ihit = m_UniqueTargetInfo.begin(); ihit != m_UniqueTargetInfo.end();)
                    {
                        // remove targets which did not pass min level check
                        //if (m_auraScaleMask && ihit->effectMask == m_auraScaleMask)
                        {
                            // Do not check for selfcast
                            //if (!ihit->scaleAura && ihit->targetGUID != m_caster->GetGUID())
                            {
                                //m_UniqueTargetInfo.erase(ihit++);
                                //continue;
                            }
                        }
                        //++ihit;
                    }
                    //if (checkLvl && m_UniqueTargetInfo.empty())
                    {
                        //SendCastResult(SPELL_FAILED_LOWLEVEL);
                        //finish(false);
                    }
                }
            }

            //if (m_targets.HasDst())
            {
                //if (m_targets.HasTraj())
                {
                    //float speed = m_targets.GetSpeedXY();
                    //if (speed > 0.0f)
                    //m_delayMoment = (uint64)floor(m_targets.GetDist2d() / speed * 1000.0f);
                }
                //else if (m_spellInfo->Speed > 0.0f)
                {
                    //float dist = m_caster->GetDistance(*m_targets.GetDstPos());
                    // m_delayMoment = (uint64) floor(dist / m_spellInfo->Speed * 1000.0f);
                }
            }
        }

        void SendSpellStart()
        {
            if (!IsNeedSendToClient())
                return;

            SpellCastFlags castFlags = SpellCastFlags.HasTrajectory;

            if ((IsTriggered() && !m_spellInfo.IsAutoRepeatRangedSpell()) )//|| m_triggeredByAuraSpell)
                castFlags |= SpellCastFlags.Pending;

            if ((m_caster is Player) || (m_caster is Unit) && m_caster.ToCreature().isPet()
                && m_spellInfo.PowerType != Powers.Health)
                castFlags |= SpellCastFlags.PowerLeftSelf;

            if (m_spellInfo.RuneCostID != 0 && m_spellInfo.PowerType == Powers.Runes)
                castFlags |= SpellCastFlags.Unk19;

            PacketWriter data = new PacketWriter(Opcodes.SMSG_SpellStart);
            if (m_CastItem != null)
                data.WriteUInt64(m_CastItem.GetPackGUID());
            else
                data.WriteUInt64(m_caster.GetPackGUID());

            data.WriteUInt64(m_caster.GetPackGUID());
            data.WriteUInt8((byte)m_cast_count);                            // pending spell cast?
            data.WriteUInt32(m_spellInfo.Id);                        // spellId
            data.WriteUInt32((uint)castFlags);                              // cast flags
            data.WriteUInt32(0);//m_timer);                                // delay?
            data.WriteInt32(m_casttime);

            m_targets.Write(ref data);

            //if (castFlags & (SpellCastFlags.PowerLeftSelf))
            //data.WriteUInt32(m_caster.GetPower((Powers)m_spellInfo.PowerType));

            if (Convert.ToBoolean(castFlags & SpellCastFlags.RuneList))                   // rune cooldowns list
            {
                //TODO: There is a crash caused by a spell with CAST_FLAG_RUNE_LIST casted by a creature
                //The creature is the mover of a player, so HandleCastSpellOpcode uses it as the caster
                Player player = m_caster.ToPlayer();
                if (player != null)
                {
                    data.WriteUInt8(m_runesState);                     // runes state before
                    //data.WriteUInt8(player.GetRunesState());          // runes state after

                    //for (var i = 0; i < MAX_RUNES; ++i)
                    {
                        // float casts ensure the division is performed on floats as we need float result
                        //float baseCd = float(player->GetRuneBaseCooldown(i));
                        //data.WriteUInt8((baseCd - float(player->GetRuneCooldown(i))) / baseCd * 255); // rune cooldown passed
                    }
                }
                else
                {
                    data.WriteUInt8(0);
                    data.WriteUInt8(0);
                }
            }

            if (Convert.ToBoolean(castFlags & SpellCastFlags.Projectile))
            {
                data.WriteUInt32(0); // Ammo display ID
                data.WriteUInt32(0); // Inventory Type
            }

            if (Convert.ToBoolean(castFlags & SpellCastFlags.Immunity))
            {
                data.WriteUInt32(0);
                data.WriteUInt32(0);
            }

            if (Convert.ToBoolean(castFlags & SpellCastFlags.HealPrediction))
            {
                data.WriteUInt32(0);
                data.WriteUInt8(0); // unkByte
            }
            m_caster.SendMessageToSet(data, true);
        }
        void SendSpellGo()
        {
            // not send invisible spell casting
            if (!IsNeedSendToClient())
                return;
            
            SpellCastFlags castFlags = SpellCastFlags.Unk9;

            // triggered spells with spell visual != 0
            if ((IsTriggered() && !m_spellInfo.IsAutoRepeatRangedSpell()))// || m_triggeredByAuraSpell)
                castFlags |= SpellCastFlags.Pending;

            if ((m_caster.GetTypeId() == ObjectType.Player ||
                (m_caster.GetTypeId() == ObjectType.Unit && m_caster.ToCreature().isPet()))
                && m_spellInfo.PowerType != Powers.Health)
                castFlags |= SpellCastFlags.PowerLeftSelf; // should only be sent to self, but the current messaging doesn't make that possible

            if ((m_caster.GetTypeId() == ObjectType.Player)
                && (m_caster.getClass() == (byte)Class.Deathknight)
                && m_spellInfo.RuneCostID != 0
                && m_spellInfo.PowerType == Powers.Runes)
            {
                castFlags |= SpellCastFlags.Unk19;                   // same as in SMSG_SPELL_START
                castFlags |= SpellCastFlags.RuneList;                    // rune cooldowns list
            }

            if (m_spellInfo.HasEffect(SpellEffects.ActivateRune))
            {
                castFlags |= SpellCastFlags.RuneList;                    // rune cooldowns list
                castFlags |= SpellCastFlags.Unk19;                   // same as in SMSG_SPELL_START
            }

            if (m_targets.HasTraj())
                castFlags |= SpellCastFlags.AdjustMissile;

            PacketWriter data = new PacketWriter(Opcodes.SMSG_SpellGo);

            if (m_CastItem != null)
                data.WriteUInt64(m_CastItem.GetPackGUID());
            else
                data.WriteUInt64(m_caster.GetPackGUID());

            data.WriteUInt64(m_caster.GetPackGUID());
            data.WriteUInt8(m_cast_count);                            // pending spell cast?
            data.WriteUInt32(m_spellInfo.Id);                        // spellId
            data.WriteUInt32((uint)castFlags);                              // cast flags
            data.WriteUInt32(0);//m_timer);
            data.WriteUnixTime();                            // timestamp

            WriteSpellGoTargets(ref data);

            m_targets.Write(ref data);
            
            //if (castFlags & CAST_FLAG_POWER_LEFT_SELF)
                //data.WriteUInt32(m_caster.GetPower((Powers)m_spellInfo->PowerType));

            if (Convert.ToBoolean(castFlags & SpellCastFlags.RuneList))                   // rune cooldowns list
            {
                //TODO: There is a crash caused by a spell with CAST_FLAG_RUNE_LIST casted by a creature
                //The creature is the mover of a player, so HandleCastSpellOpcode uses it as the caster
                Player player = m_caster.ToPlayer();
                if (player != null)
                {
                    data.WriteUInt8(m_runesState);                     // runes state before
                    //data.WriteUInt8(player.GetRunesState());          // runes state after
                    //for (uint8 i = 0; i < MAX_RUNES; ++i)
                    {
                        // float casts ensure the division is performed on floats as we need float result
                        //float baseCd = float(player->GetRuneBaseCooldown(i));
                        //data << uint8((baseCd - float(player->GetRuneCooldown(i))) / baseCd * 255); // rune cooldown passed
                    }
                }
            }

            if (Convert.ToBoolean(castFlags & SpellCastFlags.AdjustMissile))
            {
                //data << m_targets.GetElevation();
                data.WriteUInt32((uint)m_delayMoment);
            }

            if (Convert.ToBoolean(castFlags & SpellCastFlags.Projectile))
            {
                data.WriteUInt32(0); // Ammo display ID
                data.WriteUInt32(0); // Inventory Type
            }

            if (Convert.ToBoolean(castFlags & SpellCastFlags.VisualChain))
            {
                data.WriteUInt32(0);
                data.WriteUInt32(0);
            }

            //if (m_targets.GetTargetMask() & TARGET_FLAG_DEST_LOCATION)
            {
                //data << uint8(0);
            }

            //if (m_targets.GetTargetMask() & TARGET_FLAG_EXTRA_TARGETS)
            {
                //data.WriteUInt32(0); // Extra targets count
                /*
                 * for (uint8 i = 0; i < count; ++i)
                 * {
                 * data << float(0);   // Target Position X
                 * data << float(0);   // Target Position Y
                 * data << float(0);   // Target Position Z
                 * data << uint64(0);  // Target Guid
                 * }
                 * */
            }

            m_caster.SendMessageToSet(data, true);
        }
        /// Writes miss and hit targets for a SMSG_SPELL_GO packet
        void WriteSpellGoTargets(ref PacketWriter data)
        {
            // This function also fill data for channeled spells:
            // m_needAliveTargetMask req for stop channelig if one target die
            foreach (var ihit in m_UniqueTargetInfo)
            {
                if (ihit.effectMask == 0)                  // No effect apply - all immuned add state
                    // possibly SPELL_MISS_IMMUNE2 for this??
                    ihit.missCondition = SpellMissInfo.Immune2;
            }

            // Hit and miss target counts are both uint8, that limits us to 255 targets for each
            // sending more than 255 targets crashes the client (since count sent would be wrong)
            // Spells like 40647 (with a huge radius) can easily reach this limit (spell might need
            // target conditions but we still need to limit the number of targets sent and keeping
            // correct count for both hit and miss).

            uint hit = 0;
            int hitPos = data.wpos();
            data.WriteUInt8(0); // placeholder
            foreach (var ihit in m_UniqueTargetInfo)
            {
                if (ihit.missCondition == SpellMissInfo.None)       // Add only hits
                {
                    data.WriteUInt64(ihit.targetGUID);
                    //m_channelTargetEffectMask |= ihit.effectMask;
                    hit++;
                }
            }

            foreach (var ighit in m_UniqueTargetInfo)
            {
                data.WriteUInt64(ighit.targetGUID);                 // Always hits
                hit++;
            }

            uint miss = 0;
            int missPos = data.wpos();
            data.WriteUInt8(0); // placeholder
            foreach (var ihit in m_UniqueTargetInfo)
            {
                if (ihit.missCondition != SpellMissInfo.None)        // Add only miss
                {
                    data.WriteUInt64(ihit.targetGUID);
                    data.WriteUInt8((byte)ihit.missCondition);
                    if (ihit.missCondition == SpellMissInfo.Reflect)
                        data.WriteUInt8((byte)ihit.reflectResult);
                    miss++;
                }
            }
            // Reset m_needAliveTargetMask for non channeled spell
            //if (!m_spellInfo.IsChanneled())
            //m_channelTargetEffectMask = 0;

            data.Replace(hitPos, (byte)hit);
            data.Replace(missPos, (byte)miss);
        }

        bool isDelayableNoMore()
        {
            if (m_delayAtDamageCount >= 2)
                return true;

            m_delayAtDamageCount++;
            return false;
        }

        #region Fields
        SpellInfo m_spellInfo;
        public Item m_CastItem;
        ulong m_castItemGUID;
        byte m_cast_count;
        uint m_glyphIndex;
        uint m_preCastSpell;
        SpellCastTargets m_targets;
        sbyte m_comboPointGain;
        SpellCustomErrors m_customError;
        bool m_skipCheck;

        byte m_auraScaleMask;

        //UsedSpellMods m_appliedMods;
        Unit m_caster;

        SpellState m_spellState;

        SpellValue m_spellValue;

        SpellDestination[] m_destTargets = new SpellDestination[SharedConst.MaxSpellEffects];

        ulong m_originalCasterGUID;                        // real source of cast (aura caster/etc), used for spell targets selection
        // e.g. damage around area spell trigered by victim aura and damage enemies of aura caster
        Unit m_originalCaster;                             // cached pointer for m_originalCaster, updated at Spell::UpdatePointers()

        Spell m_selfContainer;                            // pointer to our spell container (if applicable)

        //Spell data
        SpellSchoolMask m_spellSchoolMask;                  // Spell school (can be overwrite for some spells (wand shoot for example)
        WeaponAttackType m_attackType;                      // For weapon based attack
        int m_powerCost;                                  // Calculated spell cost initialized only in Spell::prepare
        int m_casttime;                                   // Calculated spell cast time initialized only in Spell::prepare
        bool m_canReflect;                                  // can reflect this spell?
        bool m_autoRepeat;
        byte m_runesState;

        byte m_delayAtDamageCount;
        int m_timer;

        // Delayed spells system
        ulong m_delayStart;                                // time of spell delay start, filled by event handler, zero = just started
        ulong m_delayMoment;                               // moment of next delay call, used internally
        bool m_immediateHandled;                            // were immediate actions handled? (used by delayed spells only)

        // These vars are used in both delayed spell system and modified immediate spell system
        bool m_referencedFromCurrentSpell;                  // mark as references to prevent deleted and access by dead pointers
        bool m_executedCurrently;                           // mark as executed to prevent deleted and access by dead pointers
        bool m_needComboPoints;
        byte m_applyMultiplierMask;
        float[] m_damageMultipliers = new float[3];

        // Current targets, to be used in SpellEffects (MUST BE USED ONLY IN SPELL EFFECTS)
        Unit unitTarget;
        Item itemTarget;
        GameObject gameObjTarget;
        ObjectPosition destTarget;
        int damage;
        //SpellEffectHandleMode effectHandleMode;
        // used in effects handlers
        //Aura m_spellAura;

        // this is set in Spell Hit, but used in Apply Aura handler
        //DiminishingLevels m_diminishLevel;
        //DiminishingGroup m_diminishGroup;
        TriggerCastFlags _triggeredCastFlags;

        // -------------------------------------------
        GameObject focusObject;

        // Damage and healing in effects need just calculate
        int m_damage;           // Damge   in effects count here
        int m_healing;          // Healing in effects count here

        // ******************************************
        // Spell trigger system
        // ******************************************
        uint m_procAttacker;                // Attacker trigger flags
        uint m_procVictim;                  // Victim   trigger flags
        uint m_procEx;

        List<TargetInfo> m_UniqueTargetInfo;
        #endregion
    }

    public class SkillStatusData
    {
        public SkillStatusData(uint _pos, SkillState state)
        {
            Pos = (byte)_pos;
            State = state;
        }
        public byte Pos;
        public SkillState State;
    }

    public class SpellChainNode
    {
        public uint prev;
        public uint next;
        public uint first;
        public uint last;
        public byte rank;
    }

    public class SpellLearnSkillNode
    {
        public uint skill;
        public uint step;
        public uint value;                                           // 0  - max skill value for player level
        public uint maxvalue;                                        // 0  - max skill value for player level
    }

    public class SpellLearnSpellNode
    {
        public uint spell;
        public bool active;                                            // show in spellbook or not
        public bool autoLearned;
    }

    public class SpellDestination
    {
        public SpellDestination()
        {
            Position = new ObjectPosition();
            TransportGUID = 0;
            TransportOffset = new ObjectPosition();
        }

        public SpellDestination(float x, float y, float z, float orientation, uint mapId)
            : this()
        {
            Position.Relocate(x, y, z, orientation);
            TransportGUID = 0;
            //Position.MapId = mapId;
        }

        public SpellDestination(ObjectPosition pos)
            : this()
        {
            Position.Relocate(pos);
            TransportGUID = 0;
        }

        public SpellDestination(WorldObject wObj)
            : this()
        {
            //TransportGUID = wObj.GetTransGUID();
            //TransportOffset.Relocate(wObj.GetTransOffsetX(), wObj.GetTransOffsetY(), wObj.GetTransOffsetZ(), wObj.GetTransOffsetO());
            //Position.Relocate(wObj);
            Position.SetOrientation(wObj.GetOrientation());
        }



        public ObjectPosition Position;
        public ulong TransportGUID;
        public ObjectPosition TransportOffset;
    }

    public class TargetInfo
    {
        public uint targetGUID;
        public uint timeDelay;
        public SpellMissInfo missCondition = SpellMissInfo.Immune2;
        public SpellMissInfo reflectResult = SpellMissInfo.Immune2;
        public uint effectMask = 8;
        public bool processed = true;
        public bool alive = true;
        public bool crit = true;
        public bool scaleAura = true;
        public int damage;
    }

    public class SpellValue
    {
        public SpellValue(SpellInfo proto)
        {
            for (var i = 0; i < SharedConst.MaxSpellEffects; ++i)
                EffectBasePoints[i] = proto.Effects[i].BasePoints;

            MaxAffectedTargets = proto.MaxAffectedTargets;
            RadiusMod = 1.0f;
            AuraStackAmount = 1;
        }
        public int[] EffectBasePoints = new int[SharedConst.MaxSpellEffects];
        public uint MaxAffectedTargets;
        public float RadiusMod;
        public byte AuraStackAmount;
    }
}
