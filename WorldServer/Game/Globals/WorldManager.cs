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

using Framework.DataStorage;
using Framework.Logging;
using Framework.Singleton;
using Framework.Utility;
using System.Globalization;
using System.Threading;
using WorldServer.Game.Misc;

namespace WorldServer.Game
{
    public class WorldManager : SingletonBase<WorldManager>
    {
        WorldManager() { }

        public void Run()
        {
            var loopThread = new Thread(_update) { IsBackground = true, CurrentCulture = CultureInfo.InvariantCulture };
            loopThread.Start();
        }
        uint WORLD_SLEEP_CONST = 50;
        long realPrevTime = Time.UnixTimeMilliseconds;
        uint prevSleepTime = 0;       

        void _update()
        { 
            //temp
            while (true)
            {
                long realCurrTime = Time.UnixTimeMilliseconds;
                long diffs =  realCurrTime - realPrevTime;
                uint diff = (uint)(realCurrTime - realPrevTime);//Time.getMSTimeDiff(realPrevTime, realCurrTime);                
                Update(diff);
                realPrevTime = realCurrTime;

                // diff (D0) include time of previous sleep (d0) + tick time (t0)
                // we want that next d1 + t1 == WORLD_SLEEP_CONST
                // we can't know next t1 and then can use (t0 + d1) == WORLD_SLEEP_CONST requirement
                // d1 = WORLD_SLEEP_CONST - t0 = WORLD_SLEEP_CONST - (D0 - d0) = WORLD_SLEEP_CONST + d0 - D0
                if (diff <= WORLD_SLEEP_CONST + prevSleepTime)
                {
                    prevSleepTime = WORLD_SLEEP_CONST + prevSleepTime - diff;
                    Thread.Sleep((int)prevSleepTime);
                }
                else
                    prevSleepTime = 0;
            }
        }

        public void Update(uint diff)
        {
            Cypher.MapMgr.Update(diff);
        }

        public void InitDBLoads()
        {
            Cypher.ObjMgr.SetHighestGuids();

            Log.outInfo("Loading Activation Class/Races...");
            Cypher.ObjMgr.LoadActivationClassRaces();

            Log.outInfo("Loading Cypher Strings...");
            Cypher.ObjMgr.LoadCypherStrings();

            Log.outInfo("Initialize data stores...");
            DBCLoader.Init();
            DB2Loader.Init();

            //SpellMgr.LoadSpellDifficulty();

            Log.outInfo("Loading SpellInfo Storage...");
            Cypher.SpellMgr.LoadSpellInfoStore();
            Log.outInfo("Loading SkillLineAbility Data...");
            Cypher.SpellMgr.LoadSkillLineAbilityStore();
            Log.outInfo("Loading Spell Rank Data...");
            Cypher.SpellMgr.LoadSpellRanks();
            Log.outInfo("Loading Spell Learn Skills...");
            Cypher.SpellMgr.LoadSpellLearnSkills();
            Log.outInfo("Loading Spell Learn Spells...");
            Cypher.SpellMgr.LoadSpellLearnSpells();

            Log.outInfo("Loading Instance Template...");
            Cypher.ObjMgr.LoadInstanceTemplate();

            //Items
            Log.outInfo("Loading Items...");                         // must be after LoadRandomEnchantmentsTable and LoadPageTexts
            Cypher.ObjMgr.LoadItemTemplates();

            //Creature
            Log.outInfo("Loading Creature Model Based Info Data...");
            Cypher.ObjMgr.LoadCreatureModelInfo();

            Log.outInfo("Loading Creature templates...");
            Cypher.ObjMgr.LoadCreatureTemplates(); 
            
            Log.outInfo("Loading Equipment templates...");
            Cypher.ObjMgr.LoadEquipmentTemplates();

            Log.outInfo("Loading Creature template addons...");
            Cypher.ObjMgr.LoadCreatureTemplateAddons();

            //Log.outInfo("Loading Creature Reputation OnKill Data...");
            //ObjMgr.LoadReputationOnKill();

            Log.outInfo("Loading Creature Base Stats...");
            Cypher.ObjMgr.LoadCreatureClassLevelStats();

            Log.outInfo("Loading Creature Data...");
            Cypher.ObjMgr.LoadCreatures();

            Log.outInfo("Loading Creature Addon Data...");
            Cypher.ObjMgr.LoadCreatureAddons();

            //Log.outInfo("Loading Creature Linked Respawn...");
            //ObjMgr.LoadLinkedRespawn();                             // must be after LoadCreatures(), LoadGameObjects()

            //Log.outInfo("Loading Npc Trainers...");
            //ObjMgr.LoadTrainerSpell();

            //Log.outInfo("Loading Vendors...");
            //ObjMgr.LoadVendors();                                   // must be after load CreatureTemplate and ItemTemplate

            //Gameobjects
            Log.outInfo("Loading GameObject Template...");
            Cypher.ObjMgr.LoadGameObjectTemplate();
            Log.outInfo("Loading GameObjects...");
            Cypher.ObjMgr.LoadGameobjects();

            //Guilds
            Log.outInfo("Loading Guilds...");
            Cypher.GuildMgr.LoadGuilds();

            Log.outInfo("Loading Player Create Data...");
            Cypher.ObjMgr.LoadPlayerInfo();

            Log.outInfo("Loading client addons...");
            Addon.LoadFromDB();
        }
    }
}
