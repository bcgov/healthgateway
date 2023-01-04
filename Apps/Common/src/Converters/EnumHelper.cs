// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------

namespace HealthGateway.Common.Converters
{
    using System;

    /// <summary>
    /// Helper class for dealing with enums.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Enum parsing with null value support.
        /// </summary>
        /// <param name="value">the value to parse.</param>
        /// <typeparam name="T">the enum type.</typeparam>
        /// <returns>parsed enum or default is value is null.</returns>
        public static T ParseEnum<T>(string? value)
            where T : struct, Enum
        {
            return string.IsNullOrWhiteSpace(value)
                ? default
                : Enum.Parse<T>(value, true);
        }
    }
}
