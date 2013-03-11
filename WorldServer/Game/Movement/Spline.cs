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
using WorldServer.Game.WorldEntities;
using Framework.Graphics;
using Framework.Network;
using Framework.Logging;
using System.Reflection;
using Framework.Utility;

namespace WorldServer.Game.Movement
{
    public class Spline
    {
        public int getPointCount() { return points.Length; }
        public Vector3 getPoint(int i) { return points[i]; }
        public Vector3[] getPoints() { return points; }

        public void clear()
        {
           Array.Clear(points, 0, points.Length);
        }
        public int first() { return index_lo; }
        public int last() { return index_hi; }

        public bool isCyclic() { return cyclic;}
        
        #region Evaluate
        public void Evaluate_Percent(int Idx, float u, out Vector3 c) 
        {            
            switch (m_mode)
            {
                case EvaluationMode.Linear:
                    EvaluateLinear(Idx, u, out c);
                    break;
                case EvaluationMode.Catmullrom:
                    EvaluateCatmullRom(Idx, u, out c);
                    break;
                case EvaluationMode.Bezier3_Unused:
                    EvaluateBezier3(Idx, u, out c);
                    break;
                default:
                    c = new Vector3();
                    break;
            }
        }
        void EvaluateLinear(int index, float u, out Vector3 result)
        {
            result = points[index] + (points[index + 1] - points[index]) * u;
        }
        void EvaluateCatmullRom(int index, float t, out Vector3 result)
        {
            C_Evaluate(points.IndexRange(index - 1).ToArray(), t, s_catmullRomCoeffs, out result);
        }
        void EvaluateBezier3(int index, float t, out Vector3 result)
        {
            index *= (int)3u;
            C_Evaluate(points.IndexRange(index).ToArray(), t, s_Bezier3Coeffs, out result);
        }
        #endregion

        #region Init
        public void Init_Spline(Vector3[] controls, int count, EvaluationMode m, int cyclic_point = 0)
        {
            m_mode = m;
            cyclic = cyclic_point != 0;

            switch (m_mode)
            {
                case EvaluationMode.Linear:
                case EvaluationMode.Catmullrom:
                    InitCatmullRom(controls, count, cyclic, cyclic_point);
                    break;
                case EvaluationMode.Bezier3_Unused:
                    InitBezier3(controls, count, cyclic, cyclic_point);
                    break;
                default:
                    break;
            }
        }
        void InitLinear(Vector3[] controls, int count, bool cyclic, int cyclic_point)
        {
            int real_size = count + 1;

            Array.Resize(ref points, real_size);
            Array.Copy(controls, points, count);

            // first and last two indexes are space for special 'virtual points'
            // these points are required for proper C_Evaluate and C_Evaluate_Derivative methtod work
            if (cyclic)
                points[count] = controls[cyclic_point];
            else
                points[count] = controls[count - 1];

            index_lo = 0;
            index_hi = cyclic ? count : (count - 1);
        }
        void InitCatmullRom(Vector3[] controls, int count, bool cyclic, int cyclic_point)
        {
            int real_size = count + (cyclic ? (1 + 2) : (1 + 1));

            Array.Resize(ref points, real_size);//todo find faster way

            int lo_index = 1;
            int high_index = lo_index + count - 1;

            //points = controls;
            Array.Copy(controls, 0, points, lo_index, count);

            // first and last two indexes are space for special 'virtual points'
            // these points are required for proper C_Evaluate and C_Evaluate_Derivative methtod work
            if (cyclic)
            {
                if (cyclic_point == 0)
                    points[0] = controls[count - 1];
                else
                    points[0] = controls[0].lerp(controls[1], -1);

                points[high_index + 1] = controls[cyclic_point];
                points[high_index + 2] = controls[cyclic_point + 1];
            }
            else
            {
                points[0] = controls[0].lerp(controls[1], -1);
                points[high_index + 1] = controls[count - 1];
            }

            index_lo = lo_index;
            index_hi = high_index + (cyclic ? 1 : 0);
        }
        void InitBezier3(Vector3[] controls, int count, bool cyclic, int cyclic_point)
        {
            int c = (int)(count / 3u * 3u);
            int t = (int)(c / 3u);

            Array.Resize(ref points, c);
            Array.Copy(controls, points, c);

            index_lo = 0;
            index_hi = t - 1;
        }
        #endregion

        #region EvaluateDerivative
        public void Evaluate_Derivative(int Idx, float u, out Vector3 hermite)
        {
            switch (m_mode)
            {
                case EvaluationMode.Linear:
                    EvaluateDerivativeLinear(Idx, u, out hermite);
                    break;
                case EvaluationMode.Catmullrom:
                    EvaluateDerivativeCatmullRom(Idx, u, out hermite);
                    break;
                case EvaluationMode.Bezier3_Unused:
                    EvaluateDerivativeBezier3(Idx, u, out hermite);
                    break;
                default:
                    hermite = new Vector3();
                    break;
            }
        }
        void EvaluateDerivativeLinear(int index, float t, out Vector3 result)
        {
            result = points[index + 1] - points[index];
        }
        void EvaluateDerivativeCatmullRom(int index, float t, out Vector3 result)
        {
            C_Evaluate_Derivative(points.Skip(index - 1).ToArray(), t, s_catmullRomCoeffs, out result);//needs checked
        }
        void EvaluateDerivativeBezier3(int index, float t, out Vector3 result)
        {
            index *= (int)3u;
            C_Evaluate_Derivative(points.IndexRange(index).ToArray(), t, s_Bezier3Coeffs, out result);
        }
        #endregion
        
        #region SegLength
        public float SegLength(int i)
        {
            switch (m_mode)
            {
                case EvaluationMode.Linear:
                    return SegLengthLinear(i);
                case EvaluationMode.Catmullrom:
                    return SegLengthCatmullRom(i);
                case EvaluationMode.Bezier3_Unused:
                    return SegLengthBezier3(i);
                default:
                    return 0;
            }
        }
        float SegLengthLinear(int index)
        {
            var blah = points[index] - points[index + 1];
            return (points[index] - points[index + 1]).Length();
        }
        float SegLengthCatmullRom(int index)
        {
            Vector3 curPos, nextPos;
            var p = points;
            curPos = nextPos = p[index - 1];

            int i = 1;
            double length = 0;
            while (i <= 3)
            {
                C_Evaluate(p, (float)i / (float)3, s_catmullRomCoeffs, out nextPos);
                length += (nextPos - curPos).Length();
                curPos = nextPos;
                ++i;
            }
            return (float)length;
        }
        float SegLengthBezier3(int index)
        {
            index *= (int)3u;

            Vector3 curPos, nextPos;
            var p = points;

            C_Evaluate(p, 0.0f, s_Bezier3Coeffs, out nextPos);
            curPos = nextPos;

            int i = 1;
            double length = 0;
            while (i <= 3)
            {
                C_Evaluate(p, (float)i / (float)3, s_Bezier3Coeffs, out nextPos);
                length += (nextPos - curPos).Length();
                curPos = nextPos;
                ++i;
            }
            return (float)length;
        }
        #endregion

        Matrix4 s_catmullRomCoeffs = new Matrix4(new float[,] {
        {-0.5f, 1.5f,-1.5f, 0.5f},
        {1.0f, -2.5f, 2.0f, -0.5f},
        {-0.5f, 0.0f,  0.5f, 0.0f},
        {0.0f,  1.0f,  0.0f,  0.0f}
        });

        Matrix4 s_Bezier3Coeffs = new Matrix4(new float[,] {
        {-1.0f,  3.0f, -3.0f, 1.0f},
        {3.0f, -6.0f, 3.0f, 0.0f},
        {-3.0f,  3.0f,  0.0f, 0.0f},
        {1.0f,  0.0f,  0.0f, 0.0f}
        });

        void C_Evaluate(Vector3[] vertice, float t, Matrix4 matr, out Vector3 result)
        {
            Vector4 tvec = new Vector4(t * t * t, t * t, t, 1.0f);
            Vector4 weights = (matr * tvec);

            result = vertice[0] * weights.X + vertice[1] * weights.Y
                + vertice[2] * weights.Z + vertice[3] * weights.O;
        }
        void C_Evaluate_Derivative(Vector3[] vertice, float t, Matrix4 matr, out Vector3 result)
        {
            Vector4 tvec = new Vector4(3.0f*t*t, 2.0f*t, 1.0f, 0.0f);
            Vector4 weights = (matr * tvec);

            result = vertice[0] * weights.X + vertice[1] * weights.Y
                + vertice[2] * weights.Z + vertice[3] * weights.O;
        }

        public int length() { return lengths[index_hi];}
        public int length(int first, int last) { return lengths[last]-lengths[first];}
        public int length(int Idx) { return lengths[Idx];}
        public void set_length(int i, int length) { lengths[i] = length; }
        public void initLengths(MoveSpline.CommonInitializer cacher)
        {
            int i = index_lo;
            Array.Resize(ref lengths, index_hi+1);
            int prev_length = 0, new_length = 0;
            while (i < index_hi)
            {
                new_length = cacher.SetGetTime(this, i);
                lengths[++i] = new_length;

                prev_length = new_length;
            }
        }
        public bool empty() { return index_lo == index_hi;}

        int[] lengths = new int[0];
        Vector3[] points = new Vector3[0];
        public EvaluationMode m_mode;
        bool cyclic;
        int index_lo;
        int index_hi;
        public enum EvaluationMode
        {
            Linear,
            Catmullrom,
            Bezier3_Unused,
            UninitializedMode,
            ModesEnd
        }
    }

    public class FacingInfo
    {
        public FacingInfo(float o)
        {
            angle = o;
        }
        public FacingInfo(ulong t)
        {
            target = t;
        }
        FacingInfo() { }

        public float x;
        public float y;
        public float z;
        public ulong target;
        public float angle;
    }
}
