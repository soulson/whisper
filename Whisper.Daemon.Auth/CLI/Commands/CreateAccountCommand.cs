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

using log4net;
using System.Numerics;
using System.Security.Cryptography;
using Whisper.Daemon.Auth.Net;
using Whisper.Shared.Security;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Auth.CLI.Commands
{
    public sealed class CreateAccountCommand : AuthConsoleCommandBase
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CreateAccountCommand));

        public override string Name
        {
            get
            {
                return "account.create";
            }
        }

        public override string Help
        {
            get
            {
                return "login_name password\n\tCreates a new account with the specified login name and password.";
            }
        }

        public override void ExecuteCommand(AuthServer server, string[] arguments)
        {
            if (arguments.Length < 3)
                log.ErrorFormat("{0} requires 2 arguments", Name);
            else
            {
                string identity = arguments[1].ToUpperInvariant();
                string password = arguments[2].ToUpperInvariant();

                log.InfoFormat("creating new account '{0}'", identity);

                SecureRemotePasswordArgs srp = new SecureRemotePasswordArgs();
                using (Digester sha1 = new Digester(new SHA1CryptoServiceProvider()))
                {
                    BigInteger s = server.SecureRandom.GetRandomBigInteger(32);
                    BigInteger x = BigIntegers.FromUnsignedByteArray(sha1.CalculateDigest(new byte[][] { Arrays.Left(s.ToByteArray(), 32), sha1.CalculateDigest(identity + ':' + password) }));
                    BigInteger v = BigInteger.ModPow(srp.g, x, srp.N);

                    if (server.AuthDB.ExecuteQuery("select 1 from account where name = ?", identity, reader =>
                    {
                        if (reader.Read())
                        {
                            log.ErrorFormat("cannot create account '{0}': an account with this name already exists", identity);
                            return false;
                        }
                        return true;
                    }))
                    {
                        int result = server.AuthDB.ExecuteNonQuery("insert into account (name, salt, verifier) values (?, ?, ?)", new object[] { identity, string.Format("{0:X}", s), string.Format("{0:X}", v) });
                        if (result == 1)
                            log.InfoFormat("successfully created account '{0}'", identity);
                        else
                            log.ErrorFormat("failed to create account '{0}'. expected 1 update, got {1}", identity, result);
                    }
                }
            }
        }
    }
}
