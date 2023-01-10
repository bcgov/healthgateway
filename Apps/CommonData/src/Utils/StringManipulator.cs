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
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utilities for performing manipulations on strings.
    /// </summary>
    public static class StringManipulator
    {
        private static readonly Regex PlaceholderRegex = new(@"\$\{(.*?)\}");
        private static readonly Regex WhitespaceRegex = new(@"\s");

        /// <summary>
        /// Replaces any occurrences of ${key} in the string with the value.
        /// The dictionary should only have the name of the key as in KEY and NOT ${KEY}.
        /// </summary>
        /// <param name="inStr">The string to scan and replace.</param>
        /// <param name="data">The dictionary of key/value pairs.</param>
        /// <returns>The string with the key replaced by the supplied values.</returns>
        public static string Replace(string inStr, Dictionary<string, string> data)
        {
            // The regex will find all instances of ${ANYTHING} and will evaluate if the keys between
            // the mustaches match one of those in the dictionary.  If so it then replaces the match
            // with the value in the dictionary.
            return PlaceholderRegex.Replace(
                inStr,
                m => m.Groups.Count > 1 && data.TryGetValue(m.Groups[1].Value, out string? replacement) ? replacement : m.Value);
        }

        /// <summary>
        /// Removes any whitespace from the provided string.
        /// </summary>
        /// <param name="target">A string that may contain whitespace.</param>
        /// <returns>The string with whitespace removed.</returns>
        public static string? StripWhitespace(string? target)
        {
            return target is null ? target : WhitespaceRegex.Replace(target, string.Empty);
        }

        /// <summary>
        /// Returns bool indicating if a string is all positive numeric.
        /// </summary>
        /// <param name="target">The string to check.</param>
        /// <returns>The bool indicating if string is all numeric.</returns>
        public static bool IsPositiveNumeric(string target)
        {
            return Regex.IsMatch(target, @"^\d+$");
        }
    }
}
