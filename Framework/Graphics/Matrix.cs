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

namespace Framework.Graphics
{
    public class Matrix
    {
        public float[,] matrix;
        public int rows;
        public int cols;

        public Matrix(int rows, int cols)
        {
            this.matrix = new float[rows, cols];
            this.rows = rows;
            this.cols = cols;
        }

        public Matrix(float[,] matrix)
        {
            this.matrix = matrix;
            this.rows = matrix.GetLength(0);
            this.cols = matrix.GetLength(1);
        }

        protected static float[,] Multiply(Matrix matrix, float scalar)
        {
            int rows = matrix.rows;
            int cols = matrix.cols;
            float[,] m1 = matrix.matrix;
            float[,] m2 = new float[rows, cols];
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    m2[i, j] = m1[i, j] * scalar;
                }
            }
            return m2;
        }

        protected static float[,] Multiply(Matrix matrix1, Matrix matrix2)
        {
            int m1rows = matrix1.rows;
            int m1cols = matrix1.cols;
            int m2rows = matrix2.rows;
            int m2cols = matrix2.cols;
            if (m1cols != m2rows)
            {
                throw new ArgumentException();
            }
            float[,] m1 = matrix1.matrix;
            float[,] m2 = matrix2.matrix;
            float[,] m3 = new float[m1rows, m2cols];
            for (int i = 0; i < m1rows; ++i)
            {
                for (int j = 0; j < m2cols; ++j)
                {
                    float sum = 0;
                    for (int it = 0; it < m1cols; ++it)
                    {
                        sum += m1[i, it] * m2[it, j];
                    }
                    m3[i, j] = sum;
                }
            }
            return m3;
        }

        public static Matrix operator *(Matrix m, float scalar)
        {
            return new Matrix(Multiply(m, scalar));
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            return new Matrix(Multiply(m1, m2));
        }

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < rows; ++i)
            {
                if (i > 0)
                {
                    res += "|";
                }
                for (int j = 0; j < cols; ++j)
                {
                    if (j > 0)
                    {
                        res += ",";
                    }
                    res += matrix[i, j];
                }
            }
            return "(" + res + ")";
        }
    }

    public class Matrix3 : Matrix
    {
        public Matrix3()
            : base(3, 3)
        {
        }

        public Matrix3(float[,] matrix)
            : base(matrix)
        {
            if (rows != 3 || cols != 3)
            {
                throw new ArgumentException();
            }
        }

        public static Matrix3 I()
        {
            return new Matrix3(new float[,] { 
        { 1.0f, 0.0f, 0.0f }, 
        { 0.0f, 1.0f, 0.0f }, 
        { 0.0f, 0.0f, 1.0f } });
        }

        public static Vector3 operator *(Matrix3 matrix3, Vector3 v)
        {
            float[,] m = matrix3.matrix;
            return new Vector3(
                m[0, 0] * v.X + m[0, 1] * v.Y + m[0, 2] * v.Z,
                m[1, 0] * v.X + m[1, 1] * v.Y + m[1, 2] * v.Z,
                m[2, 0] * v.X + m[2, 1] * v.Y + m[2, 2] * v.Z);
        }

        public static Matrix3 operator *(Matrix3 mat1, Matrix3 mat2)
        {
            float[,] m1 = mat1.matrix;
            float[,] m2 = mat2.matrix;
            float[,] m3 = new float[3, 3];
            m3[0, 0] = m1[0, 0] * m2[0, 0] + m1[0, 1] * m2[1, 0] + m1[0, 2] * m2[2, 0];
            m3[0, 1] = m1[0, 0] * m2[0, 1] + m1[0, 1] * m2[1, 1] + m1[0, 2] * m2[2, 1];
            m3[0, 2] = m1[0, 0] * m2[0, 2] + m1[0, 1] * m2[1, 2] + m1[0, 2] * m2[2, 2];
            m3[1, 0] = m1[1, 0] * m2[0, 0] + m1[1, 1] * m2[1, 0] + m1[1, 2] * m2[2, 0];
            m3[1, 1] = m1[1, 0] * m2[0, 1] + m1[1, 1] * m2[1, 1] + m1[1, 2] * m2[2, 1];
            m3[1, 2] = m1[1, 0] * m2[0, 2] + m1[1, 1] * m2[1, 2] + m1[1, 2] * m2[2, 2];
            m3[2, 0] = m1[2, 0] * m2[0, 0] + m1[2, 1] * m2[1, 0] + m1[2, 2] * m2[2, 0];
            m3[2, 1] = m1[2, 0] * m2[0, 1] + m1[2, 1] * m2[1, 1] + m1[2, 2] * m2[2, 1];
            m3[2, 2] = m1[2, 0] * m2[0, 2] + m1[2, 1] * m2[1, 2] + m1[2, 2] * m2[2, 2];
            return new Matrix3(m3);
        }

        public static Matrix3 operator *(Matrix3 m, float scalar)
        {
            return new Matrix3(Multiply(m, scalar));
        }
    }

    public class Matrix4 : Matrix
    {
        public static Matrix4 I = NewI();

        public Matrix4()
            : base(4, 4)
        {
        }

        public Matrix4(float[,] matrix)
            : base(matrix)
        {
            if (rows != 4 || cols != 4)
            {
                throw new ArgumentException();
            }
        }

        public static Matrix4 NewI()
        {
            return new Matrix4(new float[,] { 
        { 1.0f, 0.0f, 0.0f, 0.0f }, 
        { 0.0f, 1.0f, 0.0f, 0.0f }, 
        { 0.0f, 0.0f, 1.0f, 0.0f },
        { 0.0f, 0.0f, 0.0f, 1.0f } });
        }

        public static Vector3 operator *(Matrix4 matrix4, Vector3 v)
        {
            float[,] m = matrix4.matrix;
            float w = m[3, 0] * v.X + m[3, 1] * v.Y + m[3, 2] * v.Z + m[3, 3];
            return new Vector3(
                (m[0, 0] * v.X + m[0, 1] * v.Y + m[0, 2] * v.Z + m[0, 3]) / w,
                (m[1, 0] * v.X + m[1, 1] * v.Y + m[1, 2] * v.Z + m[1, 3]) / w,
                (m[2, 0] * v.X + m[2, 1] * v.Y + m[2, 2] * v.Z + m[2, 3]) / w
                );
        }
        public static Vector4 operator *(Matrix4 matrix4, Vector4 v)
        {
            float[,] m = matrix4.matrix;
            //float w = m[3, 0] * v.X + m[3, 1] * v.Y + m[3, 2] * v.Z + m[3, 3];
            return new Vector4(
                (m[0, 0] * v.X + m[0, 1] * v.Y + m[0, 2] * v.Z + m[0, 3]) * v.O,
                (m[1, 0] * v.X + m[1, 1] * v.Y + m[1, 2] * v.Z + m[1, 3]) * v.O,
                (m[2, 0] * v.X + m[2, 1] * v.Y + m[2, 2] * v.Z + m[2, 3]) * v.O,
                (m[3, 0] * v.X + m[3, 1] * v.Y + m[3, 2] * v.Z + m[3, 3]) * v.O
                );
        }

        public static Matrix4 operator *(Matrix4 mat1, Matrix4 mat2)
        {
            float[,] m1 = mat1.matrix;
            float[,] m2 = mat2.matrix;
            float[,] m3 = new float[4, 4];
            m3[0, 0] = m1[0, 0] * m2[0, 0] + m1[0, 1] * m2[1, 0] + m1[0, 2] * m2[2, 0] + m1[0, 3] * m2[3, 0];
            m3[0, 1] = m1[0, 0] * m2[0, 1] + m1[0, 1] * m2[1, 1] + m1[0, 2] * m2[2, 1] + m1[0, 3] * m2[3, 1];
            m3[0, 2] = m1[0, 0] * m2[0, 2] + m1[0, 1] * m2[1, 2] + m1[0, 2] * m2[2, 2] + m1[0, 3] * m2[3, 2];
            m3[0, 3] = m1[0, 0] * m2[0, 3] + m1[0, 1] * m2[1, 3] + m1[0, 2] * m2[2, 3] + m1[0, 3] * m2[3, 3];
            m3[1, 0] = m1[1, 0] * m2[0, 0] + m1[1, 1] * m2[1, 0] + m1[1, 2] * m2[2, 0] + m1[1, 3] * m2[3, 0];
            m3[1, 1] = m1[1, 0] * m2[0, 1] + m1[1, 1] * m2[1, 1] + m1[1, 2] * m2[2, 1] + m1[1, 3] * m2[3, 1];
            m3[1, 2] = m1[1, 0] * m2[0, 2] + m1[1, 1] * m2[1, 2] + m1[1, 2] * m2[2, 2] + m1[1, 3] * m2[3, 2];
            m3[1, 3] = m1[1, 0] * m2[0, 3] + m1[1, 1] * m2[1, 3] + m1[1, 2] * m2[2, 3] + m1[1, 3] * m2[3, 3];
            m3[2, 0] = m1[2, 0] * m2[0, 0] + m1[2, 1] * m2[1, 0] + m1[2, 2] * m2[2, 0] + m1[2, 3] * m2[3, 0];
            m3[2, 1] = m1[2, 0] * m2[0, 1] + m1[2, 1] * m2[1, 1] + m1[2, 2] * m2[2, 1] + m1[2, 3] * m2[3, 1];
            m3[2, 2] = m1[2, 0] * m2[0, 2] + m1[2, 1] * m2[1, 2] + m1[2, 2] * m2[2, 2] + m1[2, 3] * m2[3, 2];
            m3[2, 3] = m1[2, 0] * m2[0, 3] + m1[2, 1] * m2[1, 3] + m1[2, 2] * m2[2, 3] + m1[2, 3] * m2[3, 3];
            m3[3, 0] = m1[3, 0] * m2[0, 0] + m1[3, 1] * m2[1, 0] + m1[3, 2] * m2[2, 0] + m1[3, 3] * m2[3, 0];
            m3[3, 1] = m1[3, 0] * m2[0, 1] + m1[3, 1] * m2[1, 1] + m1[3, 2] * m2[2, 1] + m1[3, 3] * m2[3, 1];
            m3[3, 2] = m1[3, 0] * m2[0, 2] + m1[3, 1] * m2[1, 2] + m1[3, 2] * m2[2, 2] + m1[3, 3] * m2[3, 2];
            m3[3, 3] = m1[3, 0] * m2[0, 3] + m1[3, 1] * m2[1, 3] + m1[3, 2] * m2[2, 3] + m1[3, 3] * m2[3, 3];
            return new Matrix4(m3);
        }

        public static Matrix4 operator *(Matrix4 m, float scalar)
        {
            return new Matrix4(Multiply(m, scalar));
        }
    }
}
