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

namespace Whisper.Game.Units
{
    public class MovementSpeed
    {
        private float walking;
        private float running;
        private float runningBack;
        private float swimming;
        private float swimmingBack;
        private float turning;

        public MovementSpeed()
        {
            walking = 2.5f;
            running = 7.0f;
            runningBack = 4.5f;
            swimming = 4.722222f;
            swimmingBack = 2.5f;
            turning = 3.141594f;

            IsChanged = false;
        }

        public float Walking
        {
            get
            {
                return walking;
            }
            set
            {
                IsChanged |= walking != value;
                walking = value;
            }
        }

        public float Running
        {
            get
            {
                return running;
            }
            set
            {
                IsChanged |= running != value;
                running = value;
            }
        }

        public float RunningBack
        {
            get
            {
                return runningBack;
            }
            set
            {
                IsChanged |= runningBack != value;
                runningBack = value;
            }
        }

        public float Swimming
        {
            get
            {
                return swimming;
            }
            set
            {
                IsChanged |= swimming != value;
                swimming = value;
            }
        }

        public float SwimmingBack
        {
            get
            {
                return swimmingBack;
            }
            set
            {
                IsChanged |= swimmingBack != value;
                swimmingBack = value;
            }
        }

        public float Turning
        {
            get
            {
                return turning;
            }
            set
            {
                IsChanged |= turning != value;
                turning = value;
            }
        }

        public bool IsChanged
        {
            get;
            private set;
        }

        public void ClearChangeState()
        {
            IsChanged = false;
        }
    }
}
