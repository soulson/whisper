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

using System.Collections.Generic;
using Whisper.Shared.Net;
using Whisper.Daemon.Auth.Lookup;
using System;

namespace Whisper.Daemon.Auth.Net
{
    public class AuthComposer : ComposerBase<AuthSession, AuthRequest, AuthRequestOpcode, AuthStatus>
    {
        public AuthComposer(IWhisperRequestFactory<AuthRequest, AuthRequestOpcode> requestFactory, IDictionary<string, IWhisperCommand<AuthSession, AuthRequest>> commandDictionary) : base(requestFactory, commandDictionary)
        {

        }

        public override int OpcodeOffset
        {
            get
            {
                return 0;
            }
        }

        public override int OpcodeSize
        {
            get
            {
                return sizeof(byte);
            }
        }

        protected override AuthRequestOpcode GetUnimplementedOpcodeSubstitute(AuthRequestOpcode unimplementedOpcode)
        {
            // auth requests don't have a no-op, and we can fully implement this opcode set, so kill the session if we get something weird
            throw new NotImplementedException("received unimplemented opcode " + unimplementedOpcode);
        }
    }
}
