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
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Whisper.Daemon.Auth.Lookup;
using Whisper.Daemon.Auth.Net;
using Whisper.Shared.Net;
using Whisper.Shared.Security;
using Whisper.Shared.Utility;

namespace Whisper.Daemon.Auth.Commands
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct LogonProof
    {
        public AuthRequestOpcode Opcode;
        public fixed byte A[32];
        public fixed byte M1[20];
        public fixed byte CrcHash[20];
        public byte KeyCount;
        public byte SecurityFlags;
    }

    public sealed class LogonProofCommand : CommandBase<LogonProof, AuthSession, AuthRequest, AuthStatus>
    {
        private readonly ILog log = LogManager.GetLogger(typeof(LogonProofCommand));

        public override string Name
        {
            get
            {
                return AuthRequestOpcode.LogonProof.ToString();
            }
        }

        public override AuthStatus PermissibleStatus
        {
            get
            {
                return AuthStatus.Connected;
            }
        }

        public unsafe override void ExecuteCommand(AuthSession session, AuthRequest request, LogonProof header)
        {
            log.DebugFormat("handling {0} opcode. securityflags = {1}, keycount = {2}", header.Opcode, header.SecurityFlags, header.KeyCount);
            
            using (Digester sha1 = new Digester(new SHA1CryptoServiceProvider()))
            {
                // client sends public ephemeral value A
                session.SRP.A = BigIntegers.FromUnsignedByteArray(FixedBuffers.ToArray(header.A, 32));

                log.DebugFormat("server N = {0:X}", session.SRP.N);
                log.DebugFormat("client A = {0:X}", session.SRP.A);
                log.DebugFormat("server B = {0:X}", session.SRP.B);
                log.DebugFormat("server s = {0:X}", session.SRP.s);
                log.DebugFormat("server v = {0:X}", session.SRP.v);

                // srp is required to abort if A or B is 0
                if (session.SRP.A.IsZero)
                    throw new ArgumentException("security abort: SRP6.A = 0");

                session.SRP.u = BigIntegers.FromUnsignedByteArray(sha1.CalculateDigest(session.SRP.A, session.SRP.B));
                session.SRP.S = BigInteger.ModPow(session.SRP.A * BigInteger.ModPow(session.SRP.v, session.SRP.u, session.SRP.N), session.SRP.b, session.SRP.N);

                log.DebugFormat("server u = {0:X}", session.SRP.u);
                log.DebugFormat("server S = {0:X}", session.SRP.S);

                // BigIntever.ToByteArrays are chopped/padded on the right (taking the 'Left' of the array) because of little endian. we're either killing the sign byte or padding with leading 0's in case of small number
                byte[] t = Arrays.Left(session.SRP.S.ToByteArray(), 32);
                byte[] t1 = new byte[16];
                byte[] vK = new byte[40];

                // modified srp - uses a session key (vK, K) created by interleaving two hashes (tK) created from half (t1) of the shared secret (S)
                for (int i = 0; i < 16; ++i)
                    t1[i] = t[i * 2];
                byte[] tK = sha1.CalculateDigest(t1);
                for (int i = 0; i < 20; ++i)
                    vK[i * 2] = tK[i];

                for (int i = 0; i < 16; ++i)
                    t1[i] = t[i * 2 + 1];
                tK = sha1.CalculateDigest(t1);
                for (int i = 0; i < 20; ++i)
                    vK[i * 2 + 1] = tK[i];

                session.SRP.K = BigIntegers.FromUnsignedByteArray(vK);
                log.DebugFormat("server K = {0:X}", session.SRP.K);

                byte[] Nghash = sha1.CalculateDigest(Arrays.Left(session.SRP.N.ToByteArray(), 32));
                byte[] ghash = sha1.CalculateDigest(Arrays.Left(session.SRP.g.ToByteArray(), 1));

                for (int i = 0; i < sha1.DigestSize; ++i)
                    Nghash[i] ^= ghash[i];

                // calculating M1
                byte[] M1S = sha1.CalculateDigest(new byte[][]
                {
                    Nghash,
                    sha1.CalculateDigest(session.SRP.I),
                    Arrays.Left(session.SRP.s.ToByteArray(), 32),
                    Arrays.Left(session.SRP.A.ToByteArray(), 32),
                    Arrays.Left(session.SRP.B.ToByteArray(), 32),
                    vK,
                });

                byte[] M1C = FixedBuffers.ToArray(header.M1, 20);

                BigInteger M1s = BigIntegers.FromUnsignedByteArray(M1S);
                BigInteger M1c = BigIntegers.FromUnsignedByteArray(M1C);

                log.DebugFormat("client M1 = {0:X}", M1c);
                log.DebugFormat("server M1 = {0:X}", M1s);

                // if client M1 matches server M1, then client and server have agreed on shared session key (vK, K), but only server knows that right now
                if (M1s == M1c)
                {
                    session.Status = AuthStatus.Authenticated;

                    // need to send M2 so client knows session key is shared also
                    byte[] M2S = sha1.CalculateDigest(new byte[][] {
                        Arrays.Left(session.SRP.A.ToByteArray(), 32),
                        M1C,
                        Arrays.Left(session.SRP.K.ToByteArray(), 40),
                    });
                    BigInteger M2s = BigIntegers.FromUnsignedByteArray(M2S);

                    log.InfoFormat("authentication successful for user {0}", session.SRP.I);
                    log.DebugFormat("server M2 = {0:X}", M2s);

                    int result = session.Server.AuthDB.ExecuteNonQuery("update account set last_login = now(), last_ip = ?, session_key = ? where name = ?", session.RemoteEndPoint.Address.ToString(), string.Format("{0:X}", session.SRP.K), session.SRP.I);
                    if (result != 1)
                        log.ErrorFormat("expected 1 result when updating account for login, but got {0}", result);

                    using (ByteBuffer response = new ByteBuffer())
                    {
                        response.Append((byte)AuthRequestOpcode.LogonProof);
                        response.Append((byte)AuthResponseOpcode.Success);
                        response.Append(Arrays.Left(M2S, 20));
                        response.Append(0);
                        session.Send(response.GetArraySegment());
                    }
                }
                else
                {
                    log.InfoFormat("authentication failed for user {0}", session.SRP.I);
                    using (ByteBuffer response = new ByteBuffer())
                    {
                        response.Append((byte)AuthRequestOpcode.LogonProof);
                        response.Append((byte)AuthResponseOpcode.FailBadCredentials);
                        session.Send(response.GetArraySegment());
                    }
                }
            }
        }
    }
}
