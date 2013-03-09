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
    public enum AccountDataTypes
    {
        GlobalConfigCache          = 0x00,   // 0x01 G
        PerCharacterConfigCache    = 0x01,   // 0x02 P
        GlobalBindingsCache        = 0x02,   // 0x04 G
        PerCharacterBindingsCache  = 0x03,   // 0x08 P
        GlobalMacrosCache          = 0x04,   // 0x10 G
        PerCharacterMacrosCache    = 0x05,   // 0x20 P
        PerCharacterLayoutCache    = 0x06,   // 0x40 P
        PerCharacterChatCache      = 0x07,   // 0x80 P
        NumAccountDataTypes        = 0x08
    }

    public enum AccountDataMasks
    {
        GlobalCacheMask            = 0x15,
        PerCharacterCacheMask      = 0xAA
    }

    public enum AccountTypes
    {
        Player = 0,
        Moderator = 1,
        GameMaster = 2,
        Administrator = 3,
        Console = 4
    }
}
