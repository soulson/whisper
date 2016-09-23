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

namespace Whisper.Shared.Database
{
    public abstract class DatasourceBase : IWhisperDatasource
    {
        public abstract int ExecuteNonQuery(string sql, params object[] parameters);

        public virtual T ExecuteQuery<T>(string query, ResultSetProcessor<T> processor)
        {
            return ExecuteQuery(query, new object[0], processor);
        }

        public abstract T ExecuteQuery<T>(string query, object[] parameters, ResultSetProcessor<T> processor);

        public virtual T ExecuteQuery<T>(string query, object parameter, ResultSetProcessor<T> processor)
        {
            return ExecuteQuery(query, new object[] { parameter }, processor);
        }

        public virtual void ExecuteQuery(string query, ResultSetProcessor processor)
        {
            ExecuteQuery(query, new object[0], processor);
        }

        public abstract void ExecuteQuery(string query, object[] parameters, ResultSetProcessor processor);

        public virtual void ExecuteQuery(string query, object parameter, ResultSetProcessor processor)
        {
            ExecuteQuery(query, new object[] { parameter }, processor);
        }
    }
}
