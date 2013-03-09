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
using WorldServer.Game.WorldEntities;

namespace WorldServer.Game.AI
{
    class AIFunctions
    {
        /*
        public static CreatureAI selectAI(Creature creature)
    {
        CreatureAICreator* ai_factory = NULL;
        CreatureAIRegistry& ai_registry(*CreatureAIRepository::instance());

        if (creature.isPet())
            ai_factory = ai_registry.GetRegistryItem("PetAI");

        //scriptname in db
        if (!ai_factory)
            if (CreatureAI* scriptedAI = sScriptMgr->GetCreatureAI(creature))
                return scriptedAI;

        // AIname in db
        std::string ainame=creature->GetAIName();
        if (!ai_factory && !ainame.empty())
            ai_factory = ai_registry.GetRegistryItem(ainame);

        // select by NPC flags
        if (!ai_factory)
        {
            if (creature->IsVehicle())
                ai_factory = ai_registry.GetRegistryItem("VehicleAI");
            else if (creature->HasUnitTypeMask(UNIT_MASK_CONTROLABLE_GUARDIAN) && ((Guardian*)creature)->GetOwner()->GetTypeId() == TYPEID_PLAYER)
                ai_factory = ai_registry.GetRegistryItem("PetAI");
            else if (creature->HasFlag(UNIT_NPC_FLAGS, UNIT_NPC_FLAG_SPELLCLICK))
                ai_factory = ai_registry.GetRegistryItem("NullCreatureAI");
            else if (creature->isGuard())
                ai_factory = ai_registry.GetRegistryItem("GuardAI");
            else if (creature->HasUnitTypeMask(UNIT_MASK_CONTROLABLE_GUARDIAN))
                ai_factory = ai_registry.GetRegistryItem("PetAI");
            else if (creature->isTotem())
                ai_factory = ai_registry.GetRegistryItem("TotemAI");
            else if (creature->isTrigger())
            {
                if (creature->m_spells[0])
                    ai_factory = ai_registry.GetRegistryItem("TriggerAI");
                else
                    ai_factory = ai_registry.GetRegistryItem("NullCreatureAI");
            }
            else if (creature->GetCreatureType() == CREATURE_TYPE_CRITTER && !creature->HasUnitTypeMask(UNIT_MASK_GUARDIAN))
                ai_factory = ai_registry.GetRegistryItem("CritterAI");
        }

        // select by permit check
        if (!ai_factory)
        {
            int best_val = -1;
            typedef CreatureAIRegistry::RegistryMapType RMT;
            RMT const& l = ai_registry.GetRegisteredItems();
            for (RMT::const_iterator iter = l.begin(); iter != l.end(); ++iter)
            {
                const CreatureAICreator* factory = iter->second;
                const SelectableAI* p = dynamic_cast<const SelectableAI*>(factory);
                ASSERT(p);
                int val = p->Permit(creature);
                if (val > best_val)
                {
                    best_val = val;
                    ai_factory = p;
                }
            }
        }

        // select NullCreatureAI if not another cases
        ainame = (ai_factory == NULL) ? "NullCreatureAI" : ai_factory->key();

        sLog->outDebug(LOG_FILTER_TSCR, "Creature %u used AI is %s.", creature->GetGUIDLow(), ainame.c_str());
        return (ai_factory == NULL ? new NullCreatureAI(creature) : ai_factory->Create(creature));
    }
         */
    }
}
