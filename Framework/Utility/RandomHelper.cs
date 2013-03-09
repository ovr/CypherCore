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

namespace Framework.Utility
{
    public class RandomHelper
    {
        /* Return a random double from 0.0 to 1.0 (exclusive). Floats support only 7 valid decimal digits.
         * A double supports up to 15 valid decimal digits and is used internally (RAND32_MAX has 10 digits).
         * With an FPU, there is usually no difference in performance between float and double.
         */
        public static double rand_norm()
        {
            return new Random().NextDouble();
        }

        /* Return a random double from 0.0 to 99.9999999999999. Floats support only 7 valid decimal digits.
         * A double supports up to 15 valid decimal digits and is used internally (RAND32_MAX has 10 digits).
         * With an FPU, there is usually no difference in performance between float and double.
        */
        public static double rand_chance()
        {
            return new Random().Next(0, 100);
        }

        // Return a random number in the range min..max; (max-min) must be smaller than 32768.
        public static int irand(int min, int max)
        {
            return new Random().Next(min, max);
        }
        
        //Return a random number in the range min..max (inclusive). For reliable results, the difference
        //between max and min should be less than RAND32_MAX.
        public static uint urand(int min, int max)
        {
            return (uint)new Random().Next(min, max);
        }

        //Return a random number in the range 0 .. RAND32_MAX.
        public static int rand32()
        {
            return new Random().Next(0, Int32.MaxValue);
        }

        // Return a random number in the range min..max
        public static float frand(int min, int max)
        {
            return (float)new Random().Next(min, max);
        }
    }
}
