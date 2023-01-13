//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Common.Data.Utils
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Utility methods to convert Enums from/to strings.
    /// </summary>
    public static class EnumUtility
    {
        /// <summary>
        /// Returns a string representation of the enum instance.
        /// </summary>
        /// <typeparam name="T">The type of enum.</typeparam>
        /// <param name="instance">The instance of the enum.</param>
        /// <param name="useAttribute">If true will attempt to resolve EnumMemberAttribute otherwise returns ToString().</param>
        /// <returns>The string representation of the enum.</returns>
        public static string ToEnumString<T>(Enum instance, bool useAttribute = false)
            where T : struct, Enum
        {
            string enumString = instance.ToString();
            if (useAttribute)
            {
                FieldInfo? field = typeof(T).GetField(enumString);
                if (field != null)
                {
                    EnumMemberAttribute? attr = (EnumMemberAttribute?)field.GetCustomAttributes(typeof(EnumMemberAttribute), false).SingleOrDefault();
                    if (attr != null)
                    {
                        // if there's no EnumMember attr, use the default value
                        enumString = attr.Value !;
                    }
                }
            }

            return enumString;
        }

        /// <summary>
        /// Converts a string to the typed enum using the EnumMemberAttribute or parsing directly.
        /// </summary>
        /// <typeparam name="T">The type of enum.</typeparam>
        /// <param name="enumString">The enum string value or annotation value.</param>
        /// <param name="useAttribute">If true will attempt to resolve EnumMemberAttribute otherwise converts directly.</param>
        /// <returns>The enum instance T.</returns>
        /// <exception cref="ArgumentException"><paramref name="enumString"/> is either an empty string or only contains white space.</exception>
        /// <exception cref="ArgumentException"><paramref name="enumString"/> is a name, but not one of the named constants defined for the enumeration.</exception>
        /// <exception cref="OverflowException"><paramref name="enumString"/> is outside the range of the underlying type of <paramref name="enumString"/>.</exception>
        public static T ToEnum<T>(string enumString, bool useAttribute = false)
            where T : struct, Enum
        {
            Type enumType = typeof(T);
            if (useAttribute)
            {
                foreach (string name in Enum.GetNames(enumType))
                {
                    FieldInfo? field = enumType.GetField(name);
                    if (field != null)
                    {
                        EnumMemberAttribute attr = ((EnumMemberAttribute[])field.GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
                        if (attr.Value == enumString)
                        {
                            return (T)Enum.Parse(enumType, name);
                        }
                    }
                }
            }

            return (T)Enum.Parse(enumType, enumString);
        }

        /// <summary>
        /// Converts a string to the typed enum using the EnumMemberAttribute or parsing directly. If the string is not one of the named constants defined for the enumeration, the specified default value will be returned.
        /// </summary>
        /// <typeparam name="T">The type of enum.</typeparam>
        /// <param name="enumString">The enum string value or annotation value.</param>
        /// <param name="useAttribute">If true will attempt to resolve EnumMemberAttribute otherwise converts directly.</param>
        /// <param name="defaultValue">The default value to assign when the provided string cannot be matched to an enum member.</param>
        /// <returns>The enum instance T.</returns>
        /// <exception cref="OverflowException"><paramref name="enumString"/> is outside the range of the underlying type of <typeparamref name="T"/>.</exception>
        public static T ToEnumOrDefault<T>(string enumString, bool useAttribute = false, T defaultValue = default)
            where T : struct, Enum
        {
            try
            {
                return ToEnum<T>(enumString, useAttribute);
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }
    }
}
