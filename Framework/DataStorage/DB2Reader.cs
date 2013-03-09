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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Framework.Configuration;
using Framework.Logging;
using Framework.Utility;

namespace Framework.DataStorage
{
    static class DB2Reader
    {
        public static Dictionary<uint, T> ReadDB2<T>(Dictionary<uint, string> strDict, string _fmt, string FileName) where T : class
        {
            Dictionary<uint, T> dict = new Dictionary<uint, T>();

            string path = WorldConfig.DataPath + "/dbc/" + FileName;

            if (!File.Exists(path))
            {
                Log.outError("Cant Find File {0}", FileName);
                return null;
            }
            using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.UTF8))
            {

                Db2Header header = reader.ReadStruct<Db2Header>();

                if (!header.IsDB2)
                {
                    Log.outError("{0} is not DB2 File", FileName);
                    return null;
                }

                if (header.FieldsCount != _fmt.Length)
                {
                    Log.outError("Size of '{0}' setted by format string ({1}) not equal size of C# structure ({2}).", FileName, _fmt.Length, header.FieldsCount);
                    return null;
                }

                int structsize = Marshal.SizeOf(typeof(T));
                if (structsize != _fmt.GetFMTCount())
                {
                    Log.outError("Size of '{0}' setted by format string ({1}) not equal size of C# Structure ({2}).", FileName, _fmt.GetFMTCount(), structsize);
                    return null;
                }

                // WDB2 specific fields
                uint tableHash = reader.ReadUInt32();   // new field in WDB2
                uint build = reader.ReadUInt32();       // new field in WDB2
                uint unk1 = reader.ReadUInt32();        // new field in WDB2

                if (build > 12880) // new extended header
                {
                    int MinId = reader.ReadInt32();     // new field in WDB2
                    int MaxId = reader.ReadInt32();     // new field in WDB2
                    int locale = reader.ReadInt32();    // new field in WDB2
                    int unk5 = reader.ReadInt32();      // new field in WDB2

                    if (MaxId != 0)
                    {
                        int diff = MaxId - MinId + 1;   // blizzard is weird people...
                        reader.ReadBytes(diff * 4);     // an index for rows
                        reader.ReadBytes(diff * 2);     // a memory allocation bank
                    }
                }

                for (int r = 0; r < header.RecordsCount; ++r)
                {
                    byte[] rawData = reader.ReadBytes(header.RecordSize);

                    byte[] data = new byte[Marshal.SizeOf(typeof(T))];

                    var offset = 0;
                    var index = 0;
                    for (var x = 0; x < header.FieldsCount; ++x)
                    {
                        switch (_fmt[x])
                        {
                            case 'f':
                                Array.Copy(rawData, index, data, offset, sizeof(float));
                                offset += sizeof(float);
                                index += sizeof(float);
                                break;
                            case 'n':
                            case 'i':
                            case 's':
                                Array.Copy(rawData, index, data, offset, sizeof(uint));
                                offset += sizeof(uint);
                                index += sizeof(uint);
                                break;
                            case 'b':
                                Array.Copy(rawData, index, data, offset, sizeof(byte));
                                offset += sizeof(byte);
                                index += sizeof(byte);
                                break;
                            case 'h':
                                index += sizeof(byte);
                                break;
                            case 'x':
                            case 'd':
                                index += sizeof(uint);
                                break;
                            default:
                                Log.outError("Unknown field format character in DBCfmt");
                                break;
                        }
                    }

                    GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    T T_entry = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                    handle.Free();

                    uint key = BitConverter.ToUInt32(rawData, 0);
                    dict.Add(key, T_entry);
                }

                int StartStringPosition = (int)reader.BaseStream.Position;

                if (strDict != null)
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        var offset = (uint)(reader.BaseStream.Position - StartStringPosition);
                        var str = reader.ReadCString();
                        strDict.Add(offset, str);
                    }
                }
            }
            DB2Storage.DB2FileCount += 1;
            return dict;
        }
    }
}
