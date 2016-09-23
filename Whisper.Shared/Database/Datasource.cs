/* 
 * This file is part of the whisper project.
 * Copyright (C) 2016  soulson (a.k.a. foxic)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Whisper.Shared.Database
{
    public class Datasource : DatasourceBase
    {
        private readonly string connectionString;

        public Datasource(string address, int port, string database, string username, string password)
        {
            connectionString = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};", address, port, database, username, password);
        }

        protected virtual IDbConnection GetConnection()
        {
            MySqlConnection sql = new MySqlConnection(connectionString);
            sql.Open();
            return sql;
        }

        public override T ExecuteQuery<T>(string query, object[] parameters, ResultSetProcessor<T> processor)
        {
            using (IDbConnection cn = GetConnection())
            {
                using (IDbCommand command = cn.CreateCommand())
                {
                    command.CommandText = query;

                    foreach (var entry in parameters)
                    {
                        IDataParameter param = command.CreateParameter();
                        param.Direction = ParameterDirection.Input;
                        param.Value = entry;
                        command.Parameters.Add(param);
                    }

                    using (IDataReader reader = command.ExecuteReader())
                        return processor(reader);
                }
            }
        }

        public override int ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (IDbConnection cn = GetConnection())
            {
                using (IDbCommand command = cn.CreateCommand())
                {
                    command.CommandText = sql;

                    foreach (var entry in parameters)
                    {
                        IDataParameter param = command.CreateParameter();
                        param.Direction = ParameterDirection.Input;
                        param.Value = entry;
                        command.Parameters.Add(param);
                    }

                    return command.ExecuteNonQuery();
                }
            }
        }

        public override void ExecuteQuery(string query, object[] parameters, ResultSetProcessor processor)
        {
            using (IDbConnection cn = GetConnection())
            {
                using (IDbCommand command = cn.CreateCommand())
                {
                    command.CommandText = query;

                    foreach (var entry in parameters)
                    {
                        IDataParameter param = command.CreateParameter();
                        param.Direction = ParameterDirection.Input;
                        param.Value = entry;
                        command.Parameters.Add(param);
                    }

                    using (IDataReader reader = command.ExecuteReader())
                        processor(reader);
                }
            }
        }
    }
}
