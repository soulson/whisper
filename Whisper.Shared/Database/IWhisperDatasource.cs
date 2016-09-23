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

namespace Whisper.Shared.Database
{
    public interface IWhisperDatasource
    {
        T ExecuteQuery<T>(string query, ResultSetProcessor<T> processor);
        T ExecuteQuery<T>(string query, object parameter, ResultSetProcessor<T> processor);
        T ExecuteQuery<T>(string query, object[] parameters, ResultSetProcessor<T> processor);
        void ExecuteQuery(string query, ResultSetProcessor processor);
        void ExecuteQuery(string query, object parameter, ResultSetProcessor processor);
        void ExecuteQuery(string query, object[] parameters, ResultSetProcessor processor);
        int ExecuteNonQuery(string sql, params object[] parameters);
    }
}
