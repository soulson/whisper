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
using System.Runtime.InteropServices;

namespace Whisper.Game.Characters
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ActionButton
    {
        private const int ActionMask = 0x00ffffff;

        [FieldOffset(0)]
        private int action;

        [FieldOffset(3)]
        private Type type;

        public ActionButton(int action, Type type)
        {
            // important to set action first, and then type, so the type overlays onto the action
            this.action = action;
            this.type = type;
        }

        public int Action
        {
            get
            {
                return action & ActionMask;
            }
            set
            {
                action = (action & ~ActionMask) | (value & ActionMask);
            }
        }

        public Type ButtonType
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        public enum Type : byte
        {
            Spell = 0x00,
            Macro = 0x40,
            Item = 0x80,
        }
    }
}
