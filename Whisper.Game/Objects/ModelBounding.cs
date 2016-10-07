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

namespace Whisper.Game.Objects
{
    public class ModelBounding
    {
        private static readonly ModelBounding defaultValue = new ModelBounding(0, 0.0f, 0.0f);

        public static ModelBounding Default
        {
            get
            {
                return defaultValue;
            }
        }

        public ModelBounding(int id, float boundingRadius, float combatReach)
        {
            ModelID = id;
            BoundingRadius = boundingRadius;
            CombatReach = combatReach;
        }

        public int ModelID
        {
            get;
            private set;
        }

        public float BoundingRadius
        {
            get;
            private set;
        }

        public float CombatReach
        {
            get;
            private set;
        }
    }
}
