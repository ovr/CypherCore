﻿/*
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
using System.Runtime.InteropServices;

namespace Framework.Graphics
{
	/// <summary>
	/// Defines a vector with four components.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	public struct Vector4 : IEquatable<Vector4>
	{
		public static readonly Vector4 Zero = new Vector4(0, 0, 0, 0);

		/// <summary>
		/// The X component of the vector.
		/// </summary>
		[FieldOffset(0)]
		public float X;

		/// <summary>
		/// The Y component of the vector.
		/// </summary>
		[FieldOffset(4)]
		public float Y;

		/// <summary>
		/// The Z component of the vector.
		/// </summary>
		[FieldOffset(8)]
		public float Z;

		/// <summary>
		/// The O component of the vector.
		/// </summary>
		[FieldOffset(12)]
		public float O;

		/// <summary>
		/// Creates a new <see cref="Vector3" /> with the given X, Y, Z and W components.
		/// </summary>
		/// <param name="x">the X component</param>
		/// <param name="y">the Y component</param>
		/// <param name="z">the Z component</param>
		/// <param name="w">the O component</param>
		public Vector4(float x, float y, float z, float o)
		{
			X = x;
			Y = y;
			Z = z;
			O = o;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3" /> with the given X, Y, Z and W components.
		/// </summary>
		/// <param name="xy">the XY component</param>
		/// <param name="z">the Z component</param>
		/// <param name="w">the O component</param>
		public Vector4(Vector2 xy, float z, float o)
		{
			X = xy.X;
			Y = xy.Y;
			Z = z;
			O = o;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3" /> with the given X, Y, Z and W components.
		/// </summary>
		/// <param name="xyz">the XYZ component</param>
		/// <param name="w">the O component</param>
		public Vector4(Vector3 xyz, float o)
		{
			X = xyz.X;
			Y = xyz.Y;
			Z = xyz.Z;
			O = o;
		}

		/// <summary>
		/// Clamps the values of the vector to be within a specified range.
		/// </summary>
		/// <param name="min">the minimum value</param>
		/// <param name="max">the maximum value</param>
		/// <returns>a new <see cref="Vector4" /> that has been clamped within the specified range</returns>
		public Vector4 Clamp(ref Vector4 min, ref Vector4 max)
		{
			float x = X;
			x = (x > max.X) ? max.X : x;
			x = (x < min.X) ? min.X : x;
			float y = Y;
			y = (y > max.Y) ? max.Y : y;
			y = (y < min.Y) ? min.Y : y;
			float z = Z;
			z = (z > max.Z) ? max.Z : z;
			z = (z < min.Z) ? min.Z : z;
			float o = O;
			o = (o > max.O) ? max.O : o;
			o = (o < min.O) ? min.O : o;

			return new Vector4(x, y, z, o);
		}

		/// <summary>
		/// Calculates the distance from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector4" /></param>
		/// <returns>the distance between the vectors</returns>
		public float GetDistance(ref Vector4 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;
			float z = point.Z - Z;
			float w = point.O - O;
			float dist = (((x * x) + (y * y)) + (z * z)) + (w * w);

			return (float)Math.Sqrt(dist);
		}

		/// <summary>
		/// Calculates the distance squared from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector4" /></param>
		/// <returns>the distance squared between the vectors</returns>
		public float GetDistanceSquared(ref Vector4 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;
			float z = point.Z - Z;
			float w = point.O - O;

			return (((x * x) + (y * y)) + (z * z)) + (w * w);
		}

		/// <summary>
		/// Turns the current vector into a unit vector.
		/// </summary>
		/// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
		public void Normalize()
		{
			float length = (((X * X) + (Y * Y)) + (Z * Z)) + (O * O);
			float normFactor = 1f / ((float)Math.Sqrt(length));

			X *= normFactor;
			Y *= normFactor;
			Z *= normFactor;
			O *= normFactor;
		}

		/// <summary>
		/// Checks equality of two vectors.
		/// </summary>
		/// <param name="other">the other vector to compare with</param>
		/// <returns>true if both vectors are equal; false otherwise</returns>
		public bool Equals(Vector4 other)
		{
			return ((X == other.X) && (Y == other.Y) && (Z == other.Z) && (O == other.O));
		}

		/// <summary>
		/// Checks equality with another object.
		/// </summary>
		/// <param name="obj">the object to compare</param>
		/// <returns>true if the object is <see cref="Vector4" /> and is equal; false otherwise</returns>
		public override bool Equals(object obj)
		{
			return obj is Vector4 && Equals((Vector4) obj);
		}

		public override int GetHashCode()
		{
			return (X.GetHashCode() + Y.GetHashCode() + Y.GetHashCode() + O.GetHashCode());
		}

		public static bool operator ==(Vector4 a, Vector4 b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Vector4 a, Vector4 b)
		{
			return (a.X != b.X) || (a.Y != b.Y) || (a.Z != b.Z) || a.O != b.O;
		}

		public override string ToString()
		{
			return string.Format(@"(X:{0}, Y:{1}, Z:{2}, W:{3})", X, Y, Z, O);
		}
	}
}