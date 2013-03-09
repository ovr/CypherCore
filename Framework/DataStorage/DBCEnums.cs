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

namespace Framework.DataStorage
{
    public enum MapTypes
    {
        Common = 0,
        Instance = 1,
        Raid = 2,
        Battleground = 3,
        Arena = 4
    }

    public enum AreaFlags
    {
        Snow = 0x00000001,                // Snow (Only Dun Morogh, Naxxramas, Razorfen Downs And Winterspring)
        Unk1 = 0x00000002,                // Razorfen Downs, Naxxramas And Acherus: The Ebon Hold (3.3.5a)
        Unk2 = 0x00000004,                // Only Used For Areas On Map 571 (Development Before)
        SlaveCapital = 0x00000008,                // City And City Subsones
        Unk3 = 0x00000010,                // Can'T Find Common Meaning
        SlaveCapital2 = 0x00000020,                // Slave Capital City Flag?
        AllowDuels = 0x00000040,                // Allow To Duel Here
        Arena = 0x00000080,                // Arena, Both Instanced And World Arenas
        Capital = 0x00000100,                // Main Capital City Flag
        City = 0x00000200,                // Only For One Zone Named "City" (Where It Located?)
        Outland = 0x00000400,                // Expansion Zones? (Only Eye Of The Storm Not Have This Flag, But Have 0x00004000 Flag)
        Sanctuary = 0x00000800,                // Sanctuary Area (Pvp Disabled)
        NeedFly = 0x00001000,                // Unknown
        Unused1 = 0x00002000,                // Unused In 3.3.5a
        Outland2 = 0x00004000,                // Expansion Zones? (Only Circle Of Blood Arena Not Have This Flag, But Have 0x00000400 Flag)
        OutdoorPvp = 0x00008000,                // Pvp Objective Area? (Death'S Door Also Has This Flag Although It'S No Pvp Object Area)
        ArenaInstance = 0x00010000,                // Used By Instanced Arenas Only
        Unused2 = 0x00020000,                // Unused In 3.3.5a
        ContestedArea = 0x00040000,                // On Pvp Servers These Areas Are Considered Contested, Even Though The Zone It Is Contained In Is A Horde/Alliance Territory.
        Unk6 = 0x00080000,                // Valgarde And Acherus: The Ebon Hold
        Lowlevel = 0x00100000,                // Used For Some Starting Areas With AreaLevel <= 15
        Town = 0x00200000,                // Small Towns With Inn
        Unk7 = 0x00400000,                // Warsong Hold, Acherus: The Ebon Hold, New Agamand Inn, Vengeance Landing Inn, Sunreaver Pavilion (Something To Do With Team?)
        Unk8 = 0x00800000,                // Valgarde, Acherus: The Ebon Hold, Westguard Inn, Silver Covenant Pavilion (Something To Do With Team?)
        Wintergrasp = 0x01000000,                // Wintergrasp And It'S Subzones
        Inside = 0x02000000,                // Used For Determinating Spell Related Inside/Outside Questions In Map::Isoutdoors
        Outside = 0x04000000,                // Used For Determinating Spell Related Inside/Outside Questions In Map::Isoutdoors
        Wintergrasp2 = 0x08000000,                // Same As Wintergrasp Except For The Sunken Ring And Western Bridge.
        NoFlyZone = 0x20000000,                // Marks Zones Where You Cannot Fly
        Unk9 = 0x40000000,
    }
}
