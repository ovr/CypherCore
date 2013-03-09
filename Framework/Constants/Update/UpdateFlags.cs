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

namespace Framework.Constants
{
    [Flags]
    public enum UpdateFlag
    {
        None                = 0x0000,
        Self                = 0x0001,
        Living              = 0x0002,
        Rotation            = 0x0004,
        StationaryPosition  = 0x0008,
        HasTarget           = 0x0010,
        Transport           = 0x0020,
        GoTransportPosition = 0x0040,
        AnimKits            = 0x0080,
        Vehicle             = 0x0100,
        Unknown             = 0x0200,
        Unknown2            = 0x0400,
        Unknown3            = 0x0800,
        Unknown4            = 0x1000,
    }
}
