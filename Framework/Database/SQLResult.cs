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
using System.Data;

namespace Framework.Database
{
    public class SQLResult : DataTable
    {
        public int Count { get; set; }

        public T Read<T>(int row, int column)
        {
            if (Rows[row][column] != DBNull.Value)
                return (T)Convert.ChangeType(Rows[row][column], typeof(T));
            
            if (typeof(T).Name == "String")
                return (T)Convert.ChangeType("", typeof(T));
            else
                return (T)Convert.ChangeType(0, typeof(T));
        }

        public T Read<T>(int row, string columnName)
        {
            if (Rows[row][columnName] != DBNull.Value)
                return (T)Convert.ChangeType(Rows[row][columnName], typeof(T));

            if (typeof(T).Name == "String")
                return (T)Convert.ChangeType("", typeof(T));
            else
                return (T)Convert.ChangeType(0, typeof(T));
        }
    }
}
