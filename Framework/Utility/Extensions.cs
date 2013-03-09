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

namespace Framework.Utility
{
    public static class Extensions
    {
        public static IEnumerable<TSource> IndexRange<TSource>(this IList<TSource> source, int fromIndex)
        {
            int currIndex = fromIndex;
            while (currIndex <= source.Count)
            {
                yield return source[currIndex];
                currIndex++;
            }
        }

        public static T ReadStruct<T>(this BinaryReader reader) where T : struct
        {
            byte[] rawData = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            T returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

            handle.Free();

            return returnObject;
        }

        public static T ReadGameObjectData<T>(this int[] result) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T)) / 4;
            int[] rawData = new int[size];
            Array.Copy(result, rawData, size);

            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            T returnObject = new T();
            returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return returnObject;
        }

        public unsafe static void ReadValues<T>(this BinaryReader reader, int count, out T[] values) where T : struct
        {
            values = new T[count];
            for (int i = 0; i < count; i++)
            {
                switch (values.GetType().Name)
                {
                    case "Float":
                        values[i] = (T)(object)reader.ReadSingle();
                        break;
                    case "Byte":
                        values[i] = (T)(object)reader.ReadByte();
                        break;
                    case "UInt16":
                        values[i] = (T)(object)reader.ReadUInt16();
                        break;
                }
            }
        }

        public static string ReadCString(this BinaryReader reader)
        {
            byte num;
            List<byte> temp = new List<byte>();

            while ((num = reader.ReadByte()) != 0)
            {
                temp.Add(num);
            }

            return Encoding.UTF8.GetString(temp.ToArray());
        }

        public static int GetFMTCount(this string fmt)
        {
            int count = 0;
            for (var i = 0; i < fmt.Length; i++)
            {
                switch (fmt[i])
                {
                    case 'f':
                        count += sizeof(float);
                        break;
                    case 'n':
                    case 'i':
                    case 's':
                        count += sizeof(uint);
                        break;
                    case 'b':
                        count += sizeof(byte);
                        break;
                    case 'h':
                    case 'x':
                    case 'd':
                        break;
                }
            }
            return count;
        }

        public static T LookupByKey<T>(this Dictionary<uint, T> dictionary, uint key) where T : class
        {
            T value;
            dictionary.TryGetValue(key, out value);
            return value;
        }
        public static T LookupByKey<T>(this SortedDictionary<uint, T> dictionary, uint key) where T : class
        {
            T value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static T LookupByKey<T>(this Dictionary<ulong, T> dictionary, ulong key) where T : class
        {
            T value;
            dictionary.TryGetValue(key, out value);
            return value;
        }
        public static T FindByKey<T>(this Dictionary<uint, T> dictionary, uint key) where T : struct
        {
            T value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            else
                return default(T);
        }

        public static Tuple<T, D> LookupByKey<T, D>(this List<Tuple<T, D>> blah, int id)
        {
            return blah.Find(p => Convert.ToInt32(p.Item1) == id);
        }
    }
}
