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
using Framework.Constants;

namespace WorldServer.Game.AI
{
    public class CreatureAI : UnitAI
    {
        public CreatureAI(Creature creature) : base(creature) { me = creature; }
        
        public void MoveInLineOfSight_Safe(Unit who)
        {
            if (m_MoveInLineOfSight_locked)
                return;
            m_MoveInLineOfSight_locked = true;
            MoveInLineOfSight(who);
            m_MoveInLineOfSight_locked = false;
        }
        void MoveInLineOfSight(Unit who)
        {
            if (me.getVictim() != null)
                return;

            if (me.GetCreatureType() == CreatureType.NonCombatPet)
                return;

            if (me.canStartAttack(who, false))
                AttackStart(who);
        }
        public void MovementInform(MovementGeneratorType type, uint id) { }








        
        #region Fields
        Creature me;
        bool m_MoveInLineOfSight_locked;
        #endregion
    }
}
