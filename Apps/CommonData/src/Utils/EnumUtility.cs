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
        /// Returns a string representation of the Enum instance.
        /// </summary>
        /// <typeparam name="T">The type of enum.</typeparam>
        /// <param name="instance">The instance of the enum.</param>
        /// <param name="useAttribute">If true will attempt to resolve EnumMemberAttribute otherwise returns toString().</param>
        /// <returns>The string representation of the Enum.</returns>
        public static string ToEnumString<T>(Enum instance, bool useAttribute = false)
            where T : Enum
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
        /// Converts a string to the typed Enum using the Attribute or downgrading to parsing directly.
        /// </summary>
        /// <typeparam name="T">The type of enum.</typeparam>
        /// <param name="enumStr">The enum string value or annotation value.</param>
        /// <param name="useAttribute">If true will attempt to resolve EnumMemberAttribute otherwise converts directly.</param>
        /// <returns>The enum instance T.</returns>
        public static T ToEnum<T>(string enumStr, bool useAttribute = false)
            where T : Enum
        {
            if (useAttribute)
            {
                Type enumType = typeof(T);
                foreach (string name in Enum.GetNames(enumType))
                {
                    FieldInfo? field = enumType.GetField(name);
                    if (field != null)
                    {
                        EnumMemberAttribute attr = ((EnumMemberAttribute[])field.GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
                        if (attr.Value == enumStr)
                        {
                            return (T)Enum.Parse(enumType, name);
                        }
                    }
                }
            }

            return (T)Enum.Parse(typeof(T), enumStr);
        }
    }
}
