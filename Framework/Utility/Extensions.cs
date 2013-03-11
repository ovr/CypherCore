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
using System.Diagnostics.Contracts;

namespace Framework.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// Checks if a given enum value has any of the given enum flags.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="toTest">The flags to test.</param>
        public static bool HasAnyFlag(this Enum value, Enum toTest)
        {
            Contract.Requires(value != null);
            Contract.Requires(toTest != null);

            var val = ((IConvertible)value).ToUInt64(null);
            var test = ((IConvertible)toTest).ToUInt64(null);

            return (val & test) != 0;
        }

        /// <summary>
        /// Returns the entry in this list at the given index, or the default value of the element
        /// type if the index was out of bounds.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="list">The list to retrieve from.</param>
        /// <param name="index">The index to try to retrieve at.</param>
        /// <returns>The value, or the default value of the element type.</returns>
        public static T LookupByKey<T>(this IList<T> list, int index)
        {
            Contract.Requires(list != null);
            Contract.Requires(index >= 0);

            return index >= list.Count ? default(T) : list[index];
        }

        /// <summary>
        /// Returns the entry in this dictionary at the given key, or the default value of the key
        /// if none.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="dict">The dictionary to operate on.</param>
        /// <param name="key">The key of the element to retrieve.</param>
        /// <returns>The value (if any).</returns>
        public static TValue LookupByKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            Contract.Requires(dict != null);
            Contract.Requires(key != null);

            TValue val;
            return dict.TryGetValue(key, out val) ? val : default(TValue);
        }

        public static bool empty<T>(this IList<T> list)
        {
            return list.Count != 0;
        }

        public static T ReadStruct<T>(this BinaryReader reader) where T : struct
        {
            byte[] rawData = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            T returnObject = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

            handle.Free();

            return returnObject;
        }

        public static T ReadStruct<T>(this int[] result) where T : struct
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

        public static void ReadValues<T>(this BinaryReader reader, int count, out T[] values) where T : struct
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
                temp.Add(num);

            return Encoding.UTF8.GetString(temp.ToArray());
        }

        //old shit
        public static IEnumerable<TSource> IndexRange<TSource>(this IList<TSource> source, int fromIndex)
        {
            int currIndex = fromIndex;
            while (currIndex <= source.Count)
            {
                yield return source[currIndex];
                currIndex++;
            }
        }
    }
}
