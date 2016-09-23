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

using Whisper.Shared.Net;

namespace Whisper.Daemon.Auth.Net
{
    public class AuthSession : SessionBase<AuthSession, AuthRequest, AuthStatus>
    {
        public AuthSession() : base(AuthStatus.Connected)
        {
            SRP = new SecureRemotePasswordArgs();
        }

        public SecureRemotePasswordArgs SRP
        {
            get;
            protected set;
        }

        public AuthServer Server
        {
            get
            {
                return (AuthServer)AppServer;
            }
        }
    }
}
