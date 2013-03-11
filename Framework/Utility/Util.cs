using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Utility
{
    public class flag96
    {
        uint[] part = new uint[3];

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public flag96(uint[] p)
        {
            part[0] = p[0];
            part[1] = p[1];
            part[2] = p[2];
        }

        public flag96(uint p1 = 0, uint p2 = 0, uint p3 = 0)
        {
            part[0] = p1;
            part[1] = p2;
            part[2] = p3;
        }

        public flag96(uint p1, uint p2)
        {
            part[0] = (uint)(p1 & 0x00000000FFFFFFFF);
            part[1] = (uint)((p1 >> 32) & 0x00000000FFFFFFFF);
            part[2] = p2;
        }

        public bool IsEqual(uint p1 = 0, uint p2 = 0, uint p3 = 0)
        {
            return (part[0] == p1 && part[1] == p2 && part[2] == p3);
        }

        public bool HasFlag(uint p1 = 0, uint p2 = 0, uint p3 = 0)
        {
            return Convert.ToBoolean(part[0] & p1) || Convert.ToBoolean(part[1] & p2) || Convert.ToBoolean(part[2] & p3);
        }

        public void Set(uint p1 = 0, uint p2 = 0, uint p3 = 0)
        {
            part[0] = p1;
            part[1] = p2;
            part[2] = p3;
        }

        public static bool operator <(flag96 left, flag96 right)
        {
            for (byte i = 3; i > 0; --i)
            {
                if (left.part[i - 1] < right.part[i - 1])
                    return true;
                else if (left.part[i - 1] > right.part[i - 1])
                    return false;
            }
            return false;
        }
        public static bool operator >(flag96 left, flag96 right)
        {
            for (byte i = 3; i > 0; --i)
            {
                if (left.part[i - 1] > right.part[i - 1])
                    return true;
                else if (left.part[i - 1] < right.part[i - 1])
                    return false;
            }
            return false;
        }

        public static bool operator ==(flag96 left, flag96 right)
        {
            return
            (
                left.part[0] == right.part[0] &&
                left.part[1] == right.part[1] &&
                left.part[2] == right.part[2]
            );
        }

        public static bool operator !=(flag96 left, flag96 right)
        {
            return left != right;
        }

        public static flag96 operator &(flag96 left, flag96 right)
        {
            return new flag96(left.part[0] & right.part[0], left.part[1] & right.part[1],
                left.part[2] & right.part[2]);
        }

        public static flag96 operator |(flag96 left, flag96 right)
        {
            return new flag96(left.part[0] | right.part[0], left.part[1] | right.part[1],
                left.part[2] | right.part[2]);
        }

        public static flag96 operator ^(flag96 left, flag96 right)
        {
            return new flag96(left.part[0] ^ right.part[0], left.part[1] ^ right.part[1],
                left.part[2] ^ right.part[2]);
        }

        public static implicit operator bool(flag96 left)
        {
            return (left.part[0] != 0 || left.part[1] != 0 || left.part[2] != 0);
        }

        public uint this[int i]
        {
            get
            {
                return part[i];
            }
        }

    }
}
