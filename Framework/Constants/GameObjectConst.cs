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
    public struct GameObjectConst
    {
        public const int MaxGOData = 32;
        public const int MaxQuestItems = 6;
    }
    public enum GameObjectTypes
    {
        Door = 0,
        Button = 1,
        QuestGiver = 2,
        Chest = 3,
        Binder = 4,
        Generic = 5,
        Trap = 6,
        Chair = 7,
        SpellFocus = 8,
        Text = 9,
        Goober = 10,
        Transport = 11,
        AreaDamage = 12,
        Camera = 13,
        MapObject = 14,
        MoTransport = 15,
        DuelArbiter = 16,
        FishingNode = 17,
        Ritual = 18,
        Mailbox = 19,
        AuctionHouse = 20,
        GuardPost = 21,
        SpellCaster = 22,
        MeetingStone = 23,
        FlagStand = 24,
        FishingHole = 25,
        FlagDrop = 26,
        MiniGame = 27,
        LotteryKiosk = 28,
        CapturePoint = 29,
        AuraGenerator = 30,
        DungeonDifficulty = 31,
        BarberChair = 32,
        DestructibleBuilding = 33,
        GuildBank = 34,
        TrapDoor = 35,
        Max = 36
    }
    public enum GameObjectState
    {
        Active = 0,
        Ready = 1,
        ActiveAlternative = 2,
        Max = 3
    }
    public enum GameObjectDynamicLowFlags
    {
        Activate = 0x01,
        Animate = 0x02,
        NoInteract = 0x04,
        Sparkle = 0x08,
    }
    public enum GameObjectFlags
    {
        InUse = 0x00000001,                   // Disables Interaction While Animated
        Locked = 0x00000002,                   // Require Key, Spell, Event, Etc To Be Opened. Makes "Locked" Appear In Tooltip
        InteractCond = 0x00000004,                   // Cannot Interact (Condition To Interact)
        Transport = 0x00000008,                   // Any Kind Of Transport? Object Can Transport (Elevator, Boat, Car)
        NotSelectable = 0x00000010,                   // Not Selectable Even In Gm Mode
        Nodespawn = 0x00000020,                   // Never Despawn, Typically For Doors, They Just Change State
        Triggered = 0x00000040,                   // Typically, Summoned Objects. Triggered By Spell Or Other Events
        Damaged = 0x00000200,
        Destroyed = 0x00000400
    }
    public enum LootState
    {
        NotReady = 0,
        Ready,                                               // can be ready but despawned, and then not possible activate until spawn
        Activated,
        JustDeactivated
    }
}
