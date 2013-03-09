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
using System.IO;
using System.Text;
using Framework.Logging;
using System.Collections.Generic;

namespace Framework.Configuration
{
    public class Config
    {
        string[] ConfigContent;
        public string ConfigFile { get; set; }
        Dictionary<string, string> ConfigList = new Dictionary<string, string>();

        public Config(string config)
        {
            ConfigFile = config;
            if (!File.Exists(config))
            {
                Log.outError("{0} doesn't exist!", config);
                Environment.Exit(0);
            }
            else
                ConfigContent = File.ReadAllLines(config, Encoding.UTF8);

            int lineCounter = 0;
            try
            {
                string name = string.Empty;
                foreach (var line in ConfigContent)
                {
                    if (line.StartsWith("#") || line.StartsWith("-") || line == string.Empty)
                        continue;

                    var configOption = line.Split(new char[] { '=' }, StringSplitOptions.None);
                    ConfigList.Add(configOption[0].Trim(), configOption[1].Replace("\"", "").Trim());
                    lineCounter++;
                }
            }
            catch
            {
                Log.outError("Error in {0} on Line {1}", ConfigFile, lineCounter);
            }
        }

        public T Read<T>(string name, T value)
        {
            string temp = null;
            ConfigList.TryGetValue(name, out temp);

            if (temp == null)
                return (T)Convert.ChangeType(value, typeof(T));
            else
                return (T)Convert.ChangeType(temp, typeof(T));
        }
    }
}
