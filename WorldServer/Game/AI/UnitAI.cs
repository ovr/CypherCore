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
    public class UnitAI
    {
        public UnitAI(Unit unit) { me = unit; }

        public void AttackStart(Unit victim)
        {
            if (victim != null && me.Attack(victim, true))
                me.GetMotionMaster().MoveChase(victim);
        }

        public virtual bool CanAIAttack(Unit target) { return true; }

        Unit me;
    }
}
