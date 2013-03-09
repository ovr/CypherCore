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
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Framework.Constants;
using Framework.Graphics;

namespace Framework.Network
{
    public class PacketWriter : BinaryWriter
    {
        public Opcodes Opcode { get; set; }
        public uint Size { get; set; }
        byte BitPosition;
        byte BitValue;
        public byte[] Storage
        {
            get
            {
                byte[] data = new byte[this.BaseStream.Length - 4];
                Seek(4, SeekOrigin.Begin);

                for (int i = 0; i < this.BaseStream.Length - 4; i++)
                    data[i] = (byte)BaseStream.ReadByte();
                return data;
            }
        }

        public PacketWriter() : base(new MemoryStream()) { BitPosition = 8; }
        public PacketWriter(Opcodes opcode, bool isWorldPacket = false)
            : base(new MemoryStream())
        { 
            WritePacketHeader(opcode, isWorldPacket);
        }

        protected void WritePacketHeader(Opcodes _opcode, bool isWorldPacket = false)
        {
            BitPosition = 8;
            Opcode = _opcode;
            uint opcode = (uint)_opcode;

            WriteUInt8(0);
            WriteUInt8(0);
            WriteUInt8((byte)(0xFF & opcode));
            WriteUInt8((byte)(0xFF & (opcode >> 8)));

            if (isWorldPacket)
            {
                WriteUInt8((byte)(0xFF & (opcode >> 16)));
                WriteUInt8((byte)(0xFF & (opcode >> 24)));
            }
        }

        public byte[] ReadDataToSend(bool isAuthPacket = false)
        {
            byte[] data = new byte[BaseStream.Length];
            Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < BaseStream.Length; i++)
                data[i] = (byte)BaseStream.ReadByte();


            Size = (uint)(data.Length - 2);
            if (!isAuthPacket)
            {
                data[0] = (byte)(0xFF & Size);
                data[1] = (byte)(0xFF & (Size >> 8));
            }
            return data;
        }

        public byte[] GetContents()
        {
            byte[] data = new byte[BaseStream.Length];
            Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < BaseStream.Length; i++)
                data[i] = (byte)BaseStream.ReadByte();

            return data;
        }

        public void WriteInt8<T>(T data)
        {
            base.Write((sbyte)Convert.ChangeType(data, typeof(sbyte)));
        }

        public void WriteInt16<T>(T data)
        {
            base.Write((short)Convert.ChangeType(data, typeof(short)));
        }

        public void WriteInt32<T>(T data)
        {
            base.Write((int)Convert.ChangeType(data, typeof(int)));
        }

        public void WriteInt64(long data)
        {
            base.Write(data);
        }

        public void WriteUInt8<T>(T data)
        {
            base.Write((byte)Convert.ChangeType(data, typeof(byte)));
        }

        public void WriteUInt16<T>(T data)
        {
            base.Write((ushort)Convert.ChangeType(data, typeof(ushort)));
        }

        public void WriteUInt32<T>(T data)
        {
            base.Write((uint)Convert.ChangeType(data, typeof(uint)));
        }

        public void WriteUInt64(ulong data)
        {
            base.Write(data);
        }

        public void WriteFloat(float data)
        {
            base.Write(data);
        }

        public void WriteDouble(double data)
        {
            base.Write(data);
        }

        public void WriteCString(string data)
        {
            byte[] sBytes = Encoding.ASCII.GetBytes(data);
            this.WriteBytes(sBytes);
            base.Write((byte)0);    // String null terminated
        }

        public void WriteString(string data)
        {
            byte[] sBytes = Encoding.ASCII.GetBytes(data);
            this.WriteBytes(sBytes);
        }

        public void WriteUnixTime()
        {
            DateTime baseDate = new DateTime(1970, 1, 1);
            DateTime currentDate = DateTime.Now;
            TimeSpan ts = currentDate - baseDate;

            this.Write(Convert.ToUInt32(ts.TotalSeconds));
        }

        public void WritePackedGuid(ulong guid)
        {
            byte[] packedGuid = new byte[9];
            packedGuid[0] = 0;
            byte length = 1;

            for (byte i = 0; guid != 0; i++)
            {
                if ((guid & 0xFF) != 0)
                {
                    packedGuid[0] |= (byte)(1 << i);
                    packedGuid[length] = (byte)(guid & 0xFF);
                    ++length;
                }

                guid >>= 8;
            }

            WriteBytes(packedGuid, length);
        }

        public void WriteBytes(byte[] data, int count = 0)
        {
            if (count == 0)
                base.Write(data);
            else
                base.Write(data, 0, count);
        }

        public void WriteBytes(PacketWriter data, int count = 0)
        {
            if (count == 0)
                base.Write(data.GetContents());
            else
                base.Write(data.GetContents(), 0, count);
        }

        public void WriteBitArray(BitArray buffer, int Len)
        {
            byte[] bufferarray = new byte[Convert.ToByte((buffer.Length + 8) / 8) + 2];
            buffer.CopyTo(bufferarray, 0);

            WriteBytes(bufferarray.ToArray(), Len);
        }

        public void WriteByteSeq(byte b)
        {
            if (b != 0)
            {
                base.Write((byte)(b ^ 1));
            }
        }

        public int wpos()
        {
            return (int)base.BaseStream.Position;
        }

        public void Replace<T>(int pos, T value) where T : struct
        {
            int retpos = (int)base.BaseStream.Position;

            Seek(pos, SeekOrigin.Begin);            
            switch (typeof(T).Name)
            {
                case "Byte":
                case "SByte":
                    base.Write((byte)Convert.ChangeType(value, typeof(byte)));
                    break;
                case "Float":
                    base.Write((float)Convert.ChangeType(value, typeof(float)));
                    break;
                case "Int16":
                case "UInt16":
                    base.Write((ushort)Convert.ChangeType(value, typeof(ushort)));
                    break;
                case "Int32":
                case "UInt32":
                    base.Write((uint)Convert.ChangeType(value, typeof(uint)));
                    break;
                case "Int64":
                case "UInt64":
                    base.Write((ulong)Convert.ChangeType(value, typeof(ulong)));
                    break;
            }
            Seek(retpos, SeekOrigin.Begin);
        }

        public void WriteVec3(Vector3 pos)
        {
            base.Write(pos.X);
            base.Write(pos.Y);
            base.Write(pos.Z);
        }

        public void WritePackXYZ(float x, float y, float z)
        {
            uint packed = 0;
            packed |= ((uint)(x / 0.25f) & 0x7FF);
            packed |= ((uint)(y / 0.25f) & 0x7FF) << 11;
            packed |= ((uint)(z / 0.25f) & 0x3FF) << 22;
            base.Write(packed);
        }

        //BitPack
        public void WriteBit<T>(T bit)
        {
            --BitPosition;

            if (Convert.ToBoolean(bit))
                BitValue |= (byte)(1 << (BitPosition));

            if (BitPosition == 0)
            {
                BitPosition = 8;
                WriteUInt8(BitValue);
                BitValue = 0;
            }
        }

        public void WriteBits<T>(T bit, int count)
        {
            for (int i = count - 1; i >= 0; --i)
                WriteBit<T>((T)Convert.ChangeType(((Convert.ToInt32(bit) >> i) & 1), typeof(T)));
        }

        public void BitFlush()
        {
            if (BitPosition == 8)
                return;

            WriteUInt8(BitValue);
            BitValue = 0;
            BitPosition = 8;
        }
    }
}
