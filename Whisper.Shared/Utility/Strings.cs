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
using System.Globalization;
using System.Text;

namespace Whisper.Shared.Utility
{
    public static class Strings
    {
        public static string FromNullTerminated(byte[] input, int offset, Encoding encoding, out int length)
        {
            length = 0;
            for (int i = offset; i < input.Length; ++i, ++length)
            {
                if (input[i] == 0)
                    break;
            }

            return encoding.GetString(input, offset, length);
        }

        public static string FromNullTerminated(byte[] input, int offset, Encoding encoding)
        {
            int len;
            return FromNullTerminated(input, offset, encoding, out len);
        }

        public static string FromNullTerminated(byte[] input, int offset)
        {
            return FromNullTerminated(input, offset, Encoding.UTF8);
        }

        public static string FromNullTerminated(byte[] input, ref int offset)
        {
            int len;
            string ret = FromNullTerminated(input, offset, Encoding.UTF8, out len);
            offset += len + 1; // null terminator
            return ret;
        }

        public static string HexOf(byte[] input, int offset, int length)
        {
            StringBuilder contents = new StringBuilder();
            for (int i = offset; i < offset + length; ++i)
                contents.AppendFormat("{0:x2}", input[i]);
            return contents.ToString();
        }

        public static string HexDump(byte[] input, int offset, int length)
        {
            StringBuilder contents = new StringBuilder();

            for (int i = offset; i < offset + length; ++i)
            {
                contents.AppendFormat("{0:x2} ", input[i]);

                if ((i - offset) % 16 == 15)
                    contents.AppendLine();
            }

            return contents.ToString();
        }

        public static byte[] HexDumpToBytes(string input)
        {
            string[] tokens = input.Split(new string[] { " ", "\t", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            byte[] bytes = new byte[tokens.Length];

            for (int i = 0; i < tokens.Length; ++i)
                bytes[i] = byte.Parse(tokens[i], NumberStyles.HexNumber);

            return bytes;
        }

        public static string HexOf(byte[] input)
        {
            return HexOf(input, 0, input.Length);
        }

        public static string HexDump(byte[] input)
        {
            return HexDump(input, 0, input.Length);
        }
    }
}
