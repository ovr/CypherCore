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
using Framework.Constants;
using Framework.ObjectDefines;

namespace Framework.Network
{
    public class PacketReader : BinaryReader
    {
        public Opcodes Opcode { get; set; }
        public ushort Length { get; set; }
        public byte[] Storage { get; set; }

        public PacketReader(byte[] data, bool worldPacket = true)
            : base(new MemoryStream(data))
        {
            Position = 8;
            Value = 0;

            if (worldPacket)
            {
                ushort value = this.ReadUInt16();
                Opcode = (Opcodes)this.ReadUInt16();

                if (Opcode == Opcodes.MSG_VerifyConnectivity)
                    Length = (ushort)((value % 0x100) + (value / 0x100));
                else
                    Length = value;

                Storage = new byte[Length];
                Array.Copy(data, 4, Storage, 0, Length);
            }
        }

        public sbyte ReadInt8()
        {
            return base.ReadSByte();
        }

        public new short ReadInt16()
        {
            return base.ReadInt16();
        }

        public new int ReadInt32()
        {
            return base.ReadInt32();
        }

        public new long ReadInt64()
        {
            return base.ReadInt64();
        }

        public byte ReadUInt8()
        {
            return base.ReadByte();
        }

        public new ushort ReadUInt16()
        {
            return base.ReadUInt16();
        }

        public new uint ReadUInt32()
        {
            return base.ReadUInt32();
        }

        public new ulong ReadUInt64()
        {
            return base.ReadUInt64();
        }

        public float ReadFloat()
        {
            return base.ReadSingle();
        }

        public new double ReadDouble()
        {
            return base.ReadDouble();
        }

        public string ReadCString()
        {
            StringBuilder tmpString = new StringBuilder();
            char tmpChar = base.ReadChar();
            char tmpEndChar = Convert.ToChar(Encoding.UTF8.GetString(new byte[] { 0 }));

            while (tmpChar != tmpEndChar)
            {
                tmpString.Append(tmpChar);
                tmpChar = base.ReadChar();
            }

            return tmpString.ToString();
        }

        public string ReadString(uint count)
        {
            byte[] stringArray = ReadBytes(count);
            return Encoding.ASCII.GetString(stringArray);
        }

        public byte[] ReadBytes(uint count)
        {
            return base.ReadBytes((int)count);
        }

        public string ReadStringFromBytes(uint count)
        {
            byte[] stringArray = ReadBytes(count);
            Array.Reverse(stringArray);

            return Encoding.ASCII.GetString(stringArray);
        }

        public ObjectGuid ReadGuid(byte[] stream)
        {
            return new ObjectGuid(stream);
        }

        public byte ReadXORByte(byte[] stream, byte value)
        {
            if (stream[value] != 0)
                return stream[value] ^= ReadByte();

            return 0;
        }

        public byte ReadByteSeq(byte b)
        {
            if (b != 0)
                return b ^= ReadByte();

            return 0;
        }

        public void Skip(int count)
        {
            base.BaseStream.Position += count;
        }

        public ObjectGuid ReadPackedGuid()
        {
            byte mask = ReadByte();

            ulong guid = 0;
            int i = 0;
            while (i < 8)
            {
                if ((mask & 1 << i) != 0)
                    guid += (ulong)ReadByte() << (i * 8);

                i++;
            }
            return new ObjectGuid(guid);
        }

        //BitUnpack
        int Position;
        byte Value;

        public byte GetBit()
        {
            if (Position == 8)
            {
                Value = ReadUInt8();
                Position = 0;
            }
            int returnValue = Value;
            Value = (byte)(2 * returnValue);
            ++Position;

            return (byte)(returnValue >> 7);
        }
        public bool ReadBit()
        {
            if (Position == 8)
            {
                Value = ReadUInt8();
                Position = 0;
            }
            int returnValue = Value;
            Value = (byte)(2 * returnValue);
            ++Position;

            return Convert.ToBoolean(returnValue >> 7);
        }

        public T GetBits<T>(int bitCount)
        {
            int returnValue = 0;

            for (var i = bitCount - 1; i >= 0; --i)
                returnValue = ReadBit() ? (1 << i) | returnValue : returnValue;

            return (T)Convert.ChangeType(returnValue, typeof(T));
        }

        public byte[] GetGuidMask(params int[] values)
        {
            var bytes = new byte[values.Length];

            foreach (var value in values)
                bytes[value] = (byte)(ReadBit() ? 1 : 0);

            return bytes;
        }

        public ObjectGuid GetGuid(byte[] mask, params byte[] bytes)
        {
            bool[] guidMask = new bool[mask.Length];
            byte[] guidBytes = new byte[bytes.Length];

            for (int i = 0; i < guidMask.Length; i++)
                guidMask[i] = ReadBit();

            for (byte i = 0; i < bytes.Length; i++)
                if (guidMask[mask[i]])
                    guidBytes[bytes[i]] = (byte)(ReadUInt8() ^ 1);

            return new ObjectGuid(guidBytes);

            //return tempBytes);
        }
    }
}
