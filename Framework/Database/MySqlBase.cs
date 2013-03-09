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

using Framework.Logging;
using MySql.Data.MySqlClient;
using System.Text;
using System.Threading;

namespace Framework.Database
{
    public class MySqlBase
    {
        MySqlConnection Connection;

        public int RowCount { get; set; }

        public void Init(string host, string user, string password, string database, int port)
        {
            Connection = new MySqlConnection("Server=" + host + ";User Id=" + user + ";Port=" + port + ";" + 
                                             "Password=" + password + ";Database=" + database + ";Allow Zero Datetime=True");

            try
            {
                Connection.Open();
                Log.outInfo("Successfully connected to {0}:{1}:{2}", host, port, database);
            }
            catch (MySqlException ex)
            {
                Log.outError("{0}", ex.Message);

                // Try auto reconnect on error (every 5 seconds)
                Log.outInfo("Try reconnect in 5 seconds...");
                Thread.Sleep(5000);

                Init(host, user, password, database, port);
            }
        }

        public bool Execute(string sql, params object[] args)
        {
            StringBuilder sqlString = new StringBuilder();
            sqlString.AppendFormat(sql, args);

            MySqlCommand sqlCommand = new MySqlCommand(sqlString.ToString(), Connection);

            try
            {
                sqlCommand.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException ex)
            {
                Log.outError("{0}", ex.Message);
                return false;
            }
        }
        public bool Execute(PreparedStatement stmt)
        {
            try
            {
                stmt.Cmd.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException ex)
            {
                Log.outError("{0}", ex.Message);
                return false;
            }
        }

        public SQLResult Select(string sql, params object[] args)
        {
            SQLResult retData = new SQLResult();
            StringBuilder sqlString = new StringBuilder();
            sqlString.AppendFormat(sql, args);            
            try
            {
                MySqlDataAdapter dataadapter = new MySqlDataAdapter(sqlString.ToString(), Connection);
                dataadapter.Fill(retData);
                retData.Count = retData.Rows.Count;
            }
            catch (MySqlException ex)
            {
                Log.outError("{0}", ex.Message);
            }

            return retData;
        }
        public SQLResult Select(PreparedStatement stmt)
        {
            SQLResult retData = new SQLResult();
            try
            {
                MySqlDataAdapter bl = new MySqlDataAdapter(stmt.Cmd);
                bl.Fill(retData);                        
                retData.Count = retData.Rows.Count;
            }
            catch (MySqlException ex)
            {
                Log.outError("{0}", ex.Message);
            }

            return retData;
        }

        public PreparedStatement GetPreparedStatement(string stmt)
        {
            StringBuilder str = new StringBuilder();
            int index = 0;
            for (var i = 0; i < stmt.Length; i++)
            {
                if (stmt[i].Equals('?'))
                {
                    str.Append("@" + index);
                    index++;
                }
                else
                    str.Append(stmt[i]);
            }
            return new PreparedStatement(str.ToString(), Connection);
        }
    }
}
