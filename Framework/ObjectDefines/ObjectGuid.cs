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
using Framework.Constants;

namespace Framework.ObjectDefines
{
    public class ObjectGuid
    {
        public ObjectGuid() { guid = 0L; }
        public ObjectGuid(ulong _guid)
        {
            guid = _guid;
            guidbytes = BitConverter.GetBytes(guid);
        }
        public ObjectGuid(ObjectGuid other)
        {
            guid = other.guid;
            guidbytes = BitConverter.GetBytes(guid);
        }
        public ObjectGuid(byte[] _guid, int startindex = 0)
        {
            guidbytes = _guid;
            guid = BitConverter.ToUInt64(_guid, startindex);
        }

        public static uint GuidHiPart(ulong guid)
        {
            uint t = (uint)(guid >> 48) & 0x0000FFFF;
            return (t == (uint)HighGuidType.Guild || t == (uint)HighGuidType.Corpse) ? t : (t >> 4) & 0x00000FFF;
        }
        public static uint GuidLowPart(ulong guid)
        {
            return (uint)(guid & 0x00000000FFFFFFFF);
        }

        public uint ToHiPart()
        {
            uint t = (uint)(guid >> 48) & 0x0000FFFF;
            return (t == (uint)HighGuidType.Guild || t == (uint)HighGuidType.Corpse) ? t : (t >> 4) & 0x00000FFF;
        }
        public uint ToLowPart()
        {
            return (uint)(guid & 0x00000000FFFFFFFF);
        }

        public static implicit operator double(ObjectGuid Guid) { return Guid.guid; }
        public static implicit operator long(ObjectGuid Guid) { return (long)Guid.guid; }
        public static implicit operator ulong(ObjectGuid Guid) { return Guid.guid; }
        public byte this[int i] { get { return guidbytes[i]; } set { guidbytes[i] = value; guid = BitConverter.ToUInt64(guidbytes, 0); } }

        ulong guid;
        byte[] guidbytes = new byte[8];
    }
}
