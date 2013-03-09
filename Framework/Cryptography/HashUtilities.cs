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

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Framework.Cryptography
{
    /// <summary>
    /// Provides facilities for performing common-but-specific
    /// cryptographical operations
    /// </summary>
    public static class HashUtilities
    {
        /// <summary>
        /// Brokers various data types into their integral raw
        /// form for usage by other cryptographical functions
        /// </summary>
        public class HashDataBroker
        {
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="data">the data to broker</param>
            public HashDataBroker(byte[] data)
            {
                RawData = data;
            }

            internal byte[] RawData;

            internal int Length
            {
                get { return RawData.Length; }
            }

            /// <summary>
            /// Implicit operator for byte[]->HashDataBroker casts
            /// </summary>
            /// <param name="data">the data to broker</param>
            /// <returns>a HashDataBroker object representing the original data</returns>
            public static implicit operator HashDataBroker(byte[] data)
            {
                return new HashDataBroker(data);
            }

            /// <summary>
            /// Implicit operator for string->HashDataBroker casts
            /// </summary>
            /// <param name="str">the data to broker</param>
            /// <returns>a HashDataBroker object representing the original data</returns>
            public static implicit operator HashDataBroker(string str)
            {
                return new HashDataBroker(Encoding.ASCII.GetBytes(str));
            }

            /// <summary>
            /// Implicit operator for BigInteger->HashDataBroker casts
            /// </summary>
            /// <param name="integer">the data to broker</param>
            /// <returns>a HashDataBroker object representing the original data</returns>
            public static implicit operator HashDataBroker(BigInteger integer)
            {
                return new HashDataBroker(integer.GetBytes());
            }

            /// <summary>
            /// Implicit operator for uint->HashDataBroker casts
            /// </summary>
            /// <param name="integer">the data to broker</param>
            /// <returns>a HashDataBroker object representing the original data</returns>
            public static implicit operator HashDataBroker(uint integer)
            {
                return new HashDataBroker(new BigInteger(integer).GetBytes());
            }
        }

        /// <summary>
        /// Computes a hash from hash data brokers using the given
        /// hashing algorithm
        /// </summary>
        /// <param name="algorithm">the hashing algorithm to use</param>
        /// <param name="brokers">the data brokers to hash</param>
        /// <returns>the hash result of all the data brokers</returns>
        public static byte[] FinalizeHash(HashAlgorithm algorithm, params HashDataBroker[] brokers)
        {
            MemoryStream buffer = new MemoryStream();

            foreach (HashDataBroker broker in brokers)
            {
                buffer.Write(broker.RawData, 0, broker.Length);
            }

            buffer.Position = 0;

            return algorithm.ComputeHash(buffer);
        }

        /// <summary>
        /// Computes a hash from hash data brokers using the given 
        /// hash algorithm, and generates a BigInteger from it
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="brokers"></param>
        /// <returns></returns>
        public static BigInteger HashToBigInteger(HashAlgorithm algorithm, params HashDataBroker[] brokers)
        {
            return new BigInteger(FinalizeHash(algorithm, brokers));
        }
    }
}