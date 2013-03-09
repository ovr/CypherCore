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
using Framework.Constants;
using Framework.Database;
using Framework.DataStorage;
using Framework.Logging;
using Framework.Singleton;
using Framework.Utility;
using WorldServer.Game.Packets;
using WorldServer.Game.Spells;

namespace WorldServer.Game.WorldEntities
{
    public sealed class SpellManager : SingletonBase<SpellManager>
    {
        SpellManager() { }

        public SpellInfo GetSpellInfo(uint spellId)
        {
            SpellInfo returnval;
            SpellInfoList.TryGetValue(spellId, out returnval);
            return returnval;
        }

        public bool IsPrimaryProfessionSkill(uint skill)
        {
            SkillLineEntry pSkill = DBCStorage.SkillLineStorage.LookupByKey(skill);
            if (pSkill.id == 0)
                return false;

            if (pSkill.categoryId != (uint)SkillCategory.Profession)
                return false;

            return true;
        }
        public bool IsProfessionOrRidingSkill(uint skill)
        {
            return  IsProfessionSkill(skill) || skill == (uint)Skill.Riding;
        }
        public bool IsProfessionSkill(uint skill)
        {
            return IsPrimaryProfessionSkill(skill) || skill == (uint)Skill.Fishing || skill == (uint)Skill.Cooking || skill == (uint)Skill.FirstAid;
        }
        public bool IsSpellValid(SpellInfo spellInfo, Player player = null, bool msg = true)
        {
            // not exist
            if (spellInfo.Id == 0)
                return false;

            bool need_check_reagents = false;

            // check effects
            for (byte i = 0; i < SharedConst.MaxSpellEffects; ++i)
            {
                if (spellInfo.Effects[i] == null)
                    continue;

                switch (spellInfo.Effects[i].Effect)
                {
                    case 0:
                        continue;

                    // craft spell for crafting non-existed item (break client recipes list show)
                    case (byte)SpellEffects.CreateItem:
                    case (byte)SpellEffects.CreateItem2:
                        {
                            if (spellInfo.Effects[i].ItemType == 0)
                            {
                                // skip auto-loot crafting spells, its not need explicit item info (but have special fake items sometime)
                                if (!spellInfo.IsLootCrafting())
                                {
                                    if (msg)
                                    {
                                        if (player.GetGUIDLow() != 0)
                                            ChatHandler.SendSysMessage(player.GetSession(), string.Format("Craft spell {0} not have create item entry.", spellInfo.Id));
                                        else
                                            Log.outError("Craft spell {0} not have create item entry.", spellInfo.Id);
                                    }
                                    return false;
                                }

                            }
                            // also possible IsLootCrafting case but fake item must exist anyway
                            else if (Cypher.ObjMgr.GetItemTemplate(spellInfo.Effects[i].ItemType) == null)
                            {
                                if (msg)
                                {
                                    //if (player.GetGUIDLow() != 0)
                                        //ChatHandler.SendSysMessage(player.GetSession(), string.Format("Craft spell {0} create not-exist in DB item (Entry: {1}) and then...", spellInfo.Id, spellInfo.Effects[i].ItemType));
                                    //else
                                        Log.outError("Craft spell {0} create not-exist in DB item (Entry: {1}) and then...", spellInfo.Id, spellInfo.Effects[i].ItemType);
                                }
                                return false;
                            }

                            need_check_reagents = true;
                            break;
                        }
                    case (byte)SpellEffects.LearnSpell:
                        {
                            SpellInfo spellInfo2 = Cypher.SpellMgr.GetSpellInfo(spellInfo.Effects[i].TriggerSpell);
                            if (!IsSpellValid(spellInfo2, player, msg))
                            {
                                if (msg)
                                {
                                    if (player.GetGUIDLow() != 0)
                                        ChatHandler.SendSysMessage(player.GetSession(), string.Format("Spell {0} learn to broken spell {1}, and then...", spellInfo.Id, spellInfo.Effects[i].TriggerSpell));
                                    else
                                        Log.outError("Spell {0} learn to invalid spell {1}, and then...", spellInfo.Id, spellInfo.Effects[i].TriggerSpell);
                                }
                                return false;
                            }
                            break;
                        }
                }
            }

            if (need_check_reagents)
            {
                for (int j = 0; j < SharedConst.MaxSpellReagents; ++j)
                {
                    if (spellInfo.Reagent[j] > 0 && Cypher.ObjMgr.GetItemTemplate((uint)spellInfo.Reagent[j]) == null)
                    {
                        if (msg)
                        {
                            if (player != null)
                                ChatHandler.SendSysMessage(player.GetSession(), string.Format("Craft spell {0} have not-exist reagent in DB item (Entry: {1}) and then...", spellInfo.Id, (uint)spellInfo.Reagent[j]));
                            else
                                Log.outError("Craft spell {0} have not-exist reagent in DB item (Entry: {1}) and then...", spellInfo.Id, spellInfo.Reagent[j]);
                        }
                        return false;
                    }
                }
            }

            return true;
        }

        public SpellChainNode GetSpellChainNode(uint spell_id)
        {
            return SpellChainList.LookupByKey(spell_id);
        }
        public uint GetFirstSpellInChain(uint spell_id)
        {
            var node = GetSpellChainNode(spell_id);
            if (node != null)
                return node.first;
            
            return spell_id;
        }
        public uint GetLastSpellInChain(uint spell_id)
        {
            var node = GetSpellChainNode(spell_id);
            if (node != null)
                return node.last;

            return spell_id;
        }
        public uint GetNextSpellInChain(uint spell_id)
        {
            var node = GetSpellChainNode(spell_id);
            if (node != null)
                if (node.next != 0)
                    return node.next;

            return 0;
        }
        public uint GetPrevSpellInChain(uint spell_id)
        {
            var node = GetSpellChainNode(spell_id);
            if (node != null)
                if (node.prev != 0)
                    return node.prev;

            return 0;
        }
        public SpellLearnSkillNode GetSpellLearnSkill(uint spell_id)
        {
            return SpellLearnSkillDic.LookupByKey(spell_id);
        }
        public List<SkillLineAbilityEntry> GetSkillLineAbility(uint spell_id)
        {
            return SkillLineAbilityList.Where(p => p.Value.spellId == spell_id).Select(p => p.Value).ToList();
        }
        public List<SpellLearnSpellNode> GetSpellLearnSpellMapBounds(uint spell_id)
        {
            return SpellLearnSpellList.Where(p => p.Key == spell_id).Select(p => p.Value).ToList();
        }
        uint GetSpellDifficultyId(uint spellId)
        {
            return SpellDifficultyDic.FindByKey(spellId);
        }
        uint GetSpellIdForDifficulty(uint spellId, Unit caster)
        {
            if (GetSpellInfo(spellId) == null)
                return spellId;

            if (caster == null || caster.GetMap() == null)// || caster.GetMap().IsDungeon())
                return spellId;

            uint mode = (uint)caster.GetMap().GetSpawnMode();
            if (mode >= SharedConst.MaxDifficulty)
            {
                Log.outError("SpellMgr::GetSpellIdForDifficulty: Incorrect Difficulty for spell {0}.", spellId);
                return spellId; //return source spell
            }

            uint difficultyId = GetSpellDifficultyId(spellId);
            if (difficultyId == 0)
                return spellId;

            var difficultyEntry = DBCStorage.SpellDifficultyStorage.LookupByKey(difficultyId);
            if (difficultyEntry.ID == 0)
            {
                Log.outError("SpellMgr::GetSpellIdForDifficulty: SpellDifficultyEntry not found for spell {0}. This should never happen.", spellId);
                return spellId; //return source spell
            }

            if (difficultyEntry.SpellID[mode] <= 0 && mode > (uint)Difficulty.DungeonHeroic)
            {
                Log.outError("SpellMgr::GetSpellIdForDifficulty: spell {0} mode {1} spell is NULL, using mode {2}", spellId, mode, mode - 2);
                mode -= 2;
            }

            if (difficultyEntry.SpellID[mode] <= 0)
            {
                Log.outError("SpellMgr::GetSpellIdForDifficulty: spell {0} mode {1} spell is 0. Check spelldifficulty_dbc!", spellId, mode);
                return spellId;
            }

            Log.outDebug("SpellMgr::GetSpellIdForDifficulty: spellid for spell %u in mode %u is %d", spellId, mode, difficultyEntry.SpellID[mode]);
            return (uint)difficultyEntry.SpellID[mode];
        }
        public SpellInfo GetSpellForDifficultyFromSpell(SpellInfo spell, Unit caster)
        {
            uint newSpellId = GetSpellIdForDifficulty(spell.Id, caster);
            var newSpell = GetSpellInfo(newSpellId);
            if (newSpell == null)
            {
                Log.outDebug("SpellMgr::GetSpellForDifficultyFromSpell: spell {0} not found. Check spelldifficulty_dbc!", newSpellId);
                return spell;
            }

            Log.outDebug("SpellMgr::GetSpellForDifficultyFromSpell: Spell id for instance mode is {0} (original {1})", newSpell.Id, spell.Id);
            return newSpell;
        }

        public SkillType GetSkillType(SkillLineEntry pSkill, bool racial)
        {
            switch ((SkillCategory)pSkill.categoryId)
            {
                case SkillCategory.Languages:
                    return SkillType.Language;

                case SkillCategory.Weapon:
                    return SkillType.Level;

                case SkillCategory.Armor:
                case SkillCategory.Class:
                    if (pSkill.id != (uint)Skill.Lockpicking)
                        return SkillType.Mono;
                    else
                        return SkillType.Level;

                case SkillCategory.Secondary:
                case SkillCategory.Profession:
                    // not set skills for professions and racial abilities
                    if (IsProfessionSkill(pSkill.id))
                        return SkillType.Rank;
                    else if (racial)
                        return SkillType.None;
                    else
                        return SkillType.Mono;

                default:
                case SkillCategory.Attributes:                     //not found in dbc
                case SkillCategory.Generic:                        //only GENERIC(DND)
                    return SkillType.None;
            }
        }

        public void LoadSpellRanks()
        {
            // cleanup core data before reload - remove reference to ChainNode from SpellInfo
            foreach (var chain in SpellChainList)
                SpellInfoList[chain.Key].ChainEntry = new SpellChainNode();

            SpellChainList.Clear();
            
            SQLResult result = DB.World.Select("SELECT first_spell_id, spell_id, rank from spell_ranks ORDER BY first_spell_id, rank");

            if (result.Count == 0)
            {
                Log.outError("Loaded 0 spell rank records. DB table `spell_ranks` is empty.");
                return;
            }

            bool finished = false;
            int i = 0;
            int count = 0;
            do 
            {
                List<Tuple<uint, uint>> rankChain = new List<Tuple<uint,uint>>();
                int currentSpell = -1;
                int lastSpell = -1;
                while (currentSpell == lastSpell && !finished)
                {             
                    currentSpell = result.Read<int>(i, 0);
                    if (lastSpell == -1)
                        lastSpell = currentSpell;
                    uint spell_id = result.Read<uint>(i, 1);
                    uint rank = result.Read<uint>(i, 2);

                    // don't drop the row if we're moving to the next rank
                    if (currentSpell == lastSpell)
                    {
                        rankChain.Add(new Tuple<uint,uint>(spell_id, rank));
                        if (i == result.Count - 1)
                            finished = true;
                    }
                    else
                        break;
                    i++;
                }
                
                // check if chain is made with valid first spell        
                SpellInfo first = GetSpellInfo((uint)lastSpell);
                if (first.Id == 0)
                {
                    Log.outError("Spell rank identifier(first_spell_id) {0} listed in `spell_ranks` does not exist!", lastSpell);            
                    continue;
                    
                }
                // check if chain is long enough
                if (rankChain.Count < 2)
                {
                    Log.outError("There is only 1 spell rank for identifier(first_spell_id) {0} in `spell_ranks`, entry is not needed!", lastSpell);
                    continue;
                }
                int curRank = 0;
                bool valid = true;
                // check spells in chain
                foreach (var chain in rankChain)
                {
                    SpellInfo spell = GetSpellInfo(chain.Item1);
                    if (spell.Id == 0)
                    {
                        Log.outError("Spell {0} (rank {1}) listed in `spell_ranks` for chain {2}%u does not exist!", chain.Item1, chain.Item2, lastSpell);
                        valid = false;
                        break;
                    }
                    ++curRank;
                    if (chain.Item2 != curRank)
                    {
                        Log.outError("Spell {0} (rank {1}) listed in `spell_ranks` for chain {2} does not have proper rank value(should be {3})!", chain.Item1, chain.Item2, lastSpell, curRank);
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                    continue;
                int prevRank = 0;
                // insert the chain
                for (var b = 0; b < rankChain.Count; b++)
                {
                    count++;
                    uint addedSpell = rankChain[b].Item1;
                    SpellChainNode node = new SpellChainNode()
                    {
                        first = (uint)lastSpell,
                        rank = (byte)rankChain[b].Item2,
                        prev = (uint)prevRank,
                    };

                    if (b != 0)
                        node.last =  rankChain[b - 1].Item1;

                    prevRank = (int)addedSpell;
                    if (b == rankChain.Count - 1)
                    {
                        node.next = 0;
                        break;
                    }
                    else
                        node.next = rankChain[b].Item1;
                    SpellChainList.Add(addedSpell, node);
                    SpellInfoList[addedSpell].ChainEntry = node;
                }
            } while (!finished);

            Log.outInfo("Loaded {0} spell rank records", count);
            Log.outInit();
        }
        public void LoadSpellLearnSkills()
        {
            SpellLearnSkillDic.Clear();

            // search auto-learned skills and add its to map also for use in unlearn spells/talents
            uint dbc_count = 0;
            foreach (var spell in SpellInfoList.Values)
            {
                for (int i = 0; i < SharedConst.MaxSpellEffects; ++i)
                {
                    if (spell.Effects[i] == null)
                        continue;
                    if (spell.Id == 668)
                        continue;
                    if (spell.Effects[i].Effect == (uint)SpellEffects.Skill)
                    {
                        var dbc_node = new SpellLearnSkillNode();
                        dbc_node.skill = (uint)spell.Effects[i].MiscValue;
                        dbc_node.step = (uint)spell.Effects[i].CalcValue();
                        if (dbc_node.skill != (uint)Skill.Riding)
                            dbc_node.value = 1;
                        else
                            dbc_node.value = dbc_node.step * 75;
                        dbc_node.maxvalue = dbc_node.step * 75;
                        SpellLearnSkillDic.Add(spell.Id, dbc_node);
                        ++dbc_count;
                        break;
                    }
                }
            }
            Log.outInfo("Loaded {0} Spell Learn Skills from DBC", dbc_count);
            Log.outInit();
        }
        public void LoadSpellLearnSpells()
        {
            SpellLearnSpellList.Clear();

            //                                         0      1        2
            SQLResult result = DB.World.Select("SELECT entry, SpellID, Active FROM spell_learn_spell");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 spell learn spells. DB table `spell_learn_spell` is empty.");        
                return;                
            }
            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                uint spell_id = result.Read<uint>(i, 0);

                var node = new SpellLearnSpellNode();
                node.spell = result.Read<uint>(i, 1);
                node.active = result.Read<bool>(i, 2);
                node.autoLearned = false;

                if (GetSpellInfo(spell_id) == null)
                {
                    Log.outError("Spell {0} listed in `spell_learn_spell` does not exist", spell_id);
                    continue;
                }

                if (GetSpellInfo(node.spell).Id == 0)
                {
                    Log.outError("Spell {0} listed in `spell_learn_spell` learning not existed spell {1}", spell_id, node.spell);
                    continue;
                }

                //if (GetTalentSpellCost(node.spell))
                {
                    //Log.outError("Spell %u listed in `spell_learn_spell` attempt learning talent spell %u, skipped", spell_id, node.spell);
                    //continue;
                }
                SpellLearnSpellList.Add(new KeyValuePair<uint,SpellLearnSpellNode>(spell_id, node));

                ++count;
            }
            
            // search auto-learned spells and add its to map also for use in unlearn spells/talents
            uint dbc_count = 0;
            foreach (var spell in SpellInfoList)
            {
                var entry = spell.Value;
                for (byte i = 0; i < SharedConst.MaxSpellEffects; ++i)
                {
                    if (entry.Effects[i] == null)
                        continue;

                    if (entry.Effects[i].Effect == (uint)SpellEffects.LearnSpell)
                    {
                        var dbc_node = new SpellLearnSpellNode();
                        dbc_node.spell = entry.Effects[i].TriggerSpell;
                        dbc_node.active = true;                     // all dbc based learned spells is active (show in spell book or hide by client itself)

                        // ignore learning not existed spells (broken/outdated/or generic learnig spell 483
                        if (GetSpellInfo(dbc_node.spell) == null)
                            continue;

                        // talent or passive spells or skill-step spells auto-casted and not need dependent learning,
                        // pet teaching spells must not be dependent learning (casted)
                        // other required explicit dependent learning
                        //dbc_node.autoLearned = entry.Effects[i].TargetA.GetTarget() == TARGET_UNIT_PET || GetTalentSpellCost(spell) > 0 || entry->IsPassive() || entry->HasEffect(SPELL_EFFECT_SKILL_STEP);

                        var db_node_bounds = GetSpellLearnSpellMapBounds(spell.Key);

                        bool found = false;
                        foreach (var bound in db_node_bounds)
                        //for (SpellLearnSpellMap::const_iterator itr = db_node_bounds.first; itr != db_node_bounds.second; ++itr)
                        {
                            if (bound.spell == dbc_node.spell)
                            {
                                Log.outError("Spell {0} auto-learn spell {1} in spell.dbc then the record in `spell_learn_spell` is redundant, please fix DB.",
                                    spell, dbc_node.spell);
                                found = true;
                                break;
                            }
                        }

                        if (!found)                                  // add new spell-spell pair if not found
                        {
                            SpellLearnSpellList.Add(new KeyValuePair<uint,SpellLearnSpellNode>(spell.Key, dbc_node));
                            ++dbc_count;
                        }
                    }
                }
            }
            Log.outInfo("Loaded {0} spell learn spells + {1} found in DBC.", count, dbc_count);
        }
        public void LoadSpellDifficulty()
        {
            // Create Spelldifficulty searcher
            foreach (var spellDiff in DBCStorage.SpellDifficultyStorage.Values)
            {
                SpellDifficultyEntry newEntry = new SpellDifficultyEntry();
                newEntry.SpellID = new int[SharedConst.MaxDifficulty];
                for (int x = 0; x < SharedConst.MaxDifficulty; ++x)
                {
                    if (spellDiff.SpellID[x] <= 0 || DBCStorage.SpellStorage.LookupByKey((uint)spellDiff.SpellID[x]) == null)
                    {
                        if (spellDiff.SpellID[x] > 0)//don't show error if spell is <= 0, not all modes have spells and there are unknown negative values
                            Log.outError("spelldifficulty_dbc: spell {0} at field id:{1} at spellid{2} does not exist in SpellStorage (spell.dbc), loaded as 0", spellDiff.SpellID[x], spellDiff.ID, x);
                        newEntry.SpellID[x] = 0;//spell was <= 0 or invalid, set to 0
                    }
                    else
                        newEntry.SpellID[x] = spellDiff.SpellID[x];
                }
                if (newEntry.SpellID[0] <= 0 || newEntry.SpellID[1] <= 0)//id0-1 must be always set!
                    continue;

                for (int x = 0; x < SharedConst.MaxDifficulty; ++x)
                {
                    if (newEntry.SpellID[x] == 0)
                        continue;
                    SpellDifficultyDic.Add((uint)newEntry.SpellID[x], spellDiff.ID);
                }
            }
        }

        public void LoadSkillLineAbilityStore()
        {
            SkillLineAbilityList.Clear();

            foreach (var skill in DBCStorage.SkillLineAbilityStorage.Values)
                SkillLineAbilityList.Add(new KeyValuePair<uint,SkillLineAbilityEntry>(skill.spellId, skill));

            Log.outInfo("Loaded {0} SkillLineAbility MultiMap Data.", SkillLineAbilityList.Count);
            Log.outInit();
        }
        public void LoadSpellInfoStore()
        {
            SpellInfoList.Clear();

            var effectsBySpell = new SpellEffectEntry[DBCStorage.SpellEffectStorage.Keys.Max()][];

            foreach (var effect in DBCStorage.SpellEffectStorage.Values)
            {
                if (effectsBySpell[effect.EffectSpellId] == null)
                    effectsBySpell[effect.EffectSpellId] = new SpellEffectEntry[21];
                effectsBySpell[effect.EffectSpellId][effect.EffectIndex] = effect;
            }

            foreach (var spell in DBCStorage.SpellStorage.Values)
                SpellInfoList.Add(spell.Id, new SpellInfo(spell, effectsBySpell[spell.Id]));

            Log.outInfo("Loaded {0} SpellInfos", SpellInfoList.Count);
            Log.outInit();
        }
        List<KeyValuePair<uint, SpellGroup>> SpellSpellGroupMapBounds;
        Dictionary<SpellGroup, SpellGroupStackRule> mSpellGroupStack;
        //Dictionary<uint, SpellGroup> SpellSpellGroupMap;
        public List<SpellGroup> GetSpellSpellGroupMapBounds(uint spell_id)
        {
            spell_id = GetFirstSpellInChain(spell_id);
            return SpellSpellGroupMapBounds.Where(p => p.Key == spell_id).Select(p => p.Value).ToList();
        }
        public bool AddSameEffectStackRuleSpellGroups(SpellInfo spellInfo, int amount, out Dictionary<SpellGroup, int> groups)
        {
            groups = new Dictionary<SpellGroup, int>();
            uint spellId = spellInfo.GetFirstRankSpell().Id;
            var spellGroup = GetSpellSpellGroupMapBounds(spellId);
            // Find group with SPELL_GROUP_STACK_RULE_EXCLUSIVE_SAME_EFFECT if it belongs to one
            foreach (var group in spellGroup)
            {
                //SpellGroup group = itr->second;
                var found = mSpellGroupStack.FirstOrDefault(p => p.Key == group);
                if (found.Key != SpellGroup.None)
                {
                    if (found.Value == SpellGroupStackRule.ExclusiveSameEffect)
                    {
                        // Put the highest amount in the map
                        if (groups.FirstOrDefault(p => p.Key == group).Key == SpellGroup.None)
                            groups.Add(group, amount);
                        else
                        {
                            int curr_amount = groups[group];
                            // Take absolute value because this also counts for the highest negative aura
                            if (Math.Abs(curr_amount) < Math.Abs(amount))
                                groups[group] = amount;
                        }
                        // return because a spell should be in only one SPELL_GROUP_STACK_RULE_EXCLUSIVE_SAME_EFFECT group
                        return true;
                    }
                }
            }
            // Not in a SPELL_GROUP_STACK_RULE_EXCLUSIVE_SAME_EFFECT group, so return false
            return false;
        }
        
        #region Fields
        Dictionary<uint,SpellInfo> SpellInfoList = new Dictionary<uint,SpellInfo>();
        Dictionary<uint, SpellChainNode> SpellChainList = new Dictionary<uint,SpellChainNode>();
        Dictionary<uint, SpellLearnSkillNode> SpellLearnSkillDic = new Dictionary<uint, SpellLearnSkillNode>();
        List<KeyValuePair<uint, SkillLineAbilityEntry>> SkillLineAbilityList = new List<KeyValuePair<uint, SkillLineAbilityEntry>>();
        List<KeyValuePair<uint, SpellLearnSpellNode>> SpellLearnSpellList = new List<KeyValuePair<uint,SpellLearnSpellNode>>();
        Dictionary<uint, uint> SpellDifficultyDic = new Dictionary<uint, uint>();
        #endregion
    }
}
