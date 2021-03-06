﻿/*
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

namespace WorldServer.Game.Spells
{
    public class AuraEffect
    {
        public AuraEffect()
        {

        }

        public bool IsAffectingSpell(SpellInfo spell)
        {
            if (spell == null)
                return false;
            // Check family name
            if (spell.SpellFamilyName != m_spellInfo.SpellFamilyName)
                return false;

            // Check EffectClassMask
            if (m_spellInfo.Effects[m_effIndex].SpellClassMask & spell.SpellFamilyFlags)
                return true;

            return false;
        }

        public SpellInfo GetSpellInfo() { return m_spellInfo; }
        public int GetAmount() { return m_amount; }
        public void SetAmount(int amount) { m_amount = amount; m_canBeRecalculated = false;}



        #region Fields
        byte m_effIndex;
        int m_amount;
        bool m_canBeRecalculated;
        SpellInfo m_spellInfo;
        #endregion
    }
}
