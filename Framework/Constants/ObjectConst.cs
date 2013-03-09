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

namespace Framework.Constants
{
    public static class ObjectConst
    {
        public const float DefaultWorldObjectSize = 0.388999998569489f;      // player size, also currently used (correctly?) for any non Unit world objects
        public const float AttackDistance = 5.0f;
        public const float DefaultCombatReach = 1.5f;
        public const float MinMeleeReach = 2.0f;
        public const float NominalMeleeRange = 5.0f;
        public const float MeleeRange = NominalMeleeRange - MinMeleeReach * 2; //center to center for players

        public const float CONTACT_DISTANCE = 0.5f;
        public const float INTERACTION_DISTANCE = 5.0f;
        public const float ATTACK_DISTANCE = 5.0f;
        public const float MAX_VISIBILITY_DISTANCE = MapConst.SizeOfGrids;        // max distance for visible objects
        public const float SIGHT_RANGE_UNIT = 50.0f;
        public const float DEFAULT_VISIBILITY_DISTANCE = 90.0f;                  // default visible distance, 90 yards on continents
        public const float DEFAULT_VISIBILITY_INSTANCE = 170.0f;                 // default visible distance in instances, 170 yards
        public const float DEFAULT_VISIBILITY_BGARENAS = 533.0f;             // default visible distance in BG/Arenas, roughly 533 yards
    }
    public enum NotifyFlags
    {
        NONE = 0x00,
        AI_RELOCATION = 0x01,
        VISIBILITY_CHANGED = 0x02,
        ALL = 0xFF
    }
}
