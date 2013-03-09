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
using Framework.Database;
using Framework.Logging;
using Framework.Utility;
using System.Diagnostics;

namespace WorldServer.Game.Misc
{
    public class Addon
    {
        public const int StandardAddonCRC = 0x4c1c776d;
        public static List<SavedAddon> knownAddons = new List<SavedAddon>();

        public static void LoadFromDB()
        {
            var time = Time.getMSTime();
            SQLResult result = DB.Characters.Select("SELECT name, crc FROM addons");
            if (result.Count == 0)
            {
                Log.outError("Loaded 0 known addons. DB table `addons` is empty!");
                return;
            }

            uint count = 0;
            for (var i = 0; i < result.Count; i++)
            {
                string name = result.Read<string>(i, 0);
                uint crc = result.Read<uint>(i, 1);

                knownAddons.Add(new SavedAddon(name, crc));
                ++count;
            }

            Log.outInfo("Loaded {0} known addons in {1} ms", count, Time.getMSTimeDiffNow(time));
            Log.outInfo();
        }
        public static void SaveAddon(AddonInfo addon)
        {
            string name = addon.Name;

            PreparedStatement stmt = DB.Characters.GetPreparedStatement(CharStatements.CHAR_INS_ADDON);

            stmt.AddValue(0, name);
            stmt.AddValue(1, addon.CRC);

            DB.Characters.Execute(stmt);

            knownAddons.Add(new SavedAddon(addon.Name, addon.CRC));
        }
        public static SavedAddon GetAddonInfo(string name)
        {
            return knownAddons.Find(p => p.Name == name);
        }
    }
    public class AddonInfo
    {
        public AddonInfo(string name, byte enabled, uint crc, byte state, bool crcOrPubKey)
        {
            Name = name;
            Enabled = enabled;
            CRC = crc;
            State = state;
            UsePublicKeyOrCRC = crcOrPubKey;
        }

        public string Name;
        public byte Enabled;
        public uint CRC;
        public byte State;
        public bool UsePublicKeyOrCRC;
    }
    public class SavedAddon
    {
        public SavedAddon(string name, uint crc)
        {
            Name = name;
            CRC = crc;
        }

        public string Name;
        public uint CRC;
    }
}
