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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utilities for performing manipulations on strings.
    /// </summary>
    public static partial class StringManipulator
    {
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
            return PlaceholderRegex()
                .Replace(
                    inStr,
                    m => m.Groups.Count > 1 && data.TryGetValue(m.Groups[1].Value, out string? replacement) ? replacement : m.Value);
        }

        /// <summary>
        /// Removes any whitespace from the provided string.
        /// </summary>
        /// <param name="target">A string that may contain whitespace.</param>
        /// <returns>The string with whitespace removed.</returns>
        [return: NotNullIfNotNull(nameof(target))]
        public static string? StripWhitespace(string? target)
        {
            return target is null ? target : WhitespaceRegex().Replace(target, string.Empty);
        }

        /// <summary>
        /// Returns bool indicating if a string is all positive numeric.
        /// </summary>
        /// <param name="target">The string to check.</param>
        /// <returns>The bool indicating if string is all positive numeric.</returns>
        public static bool IsPositiveNumeric(string target)
        {
            return OnlyDigitsRegex().IsMatch(target);
        }

        /// <summary>
        /// Joins together a collection of strings with a given separator,
        /// excluding any that are null, empty, or contain only whitespace.
        /// </summary>
        /// <param name="values">A collection that contains the strings to concatenate.</param>
        /// <param name="separator">The string to use as a separator.</param>
        /// <returns>
        /// A string that consists of the elements of values delimited by the separator string
        /// -or- <see cref="string.Empty"/> if values has zero non-blank elements.
        /// </returns>
        public static string JoinWithoutBlanks(IEnumerable<string?> values, string separator = " ")
        {
            return string.Join(separator, ExcludeBlanks(values));
        }

        /// <summary>
        /// Filters a collection of strings to exclude those that are null, empty, or consisting only of whitespace.
        /// </summary>
        /// <param name="strings">The collection of strings to filter.</param>
        /// <returns>A collection containing the elements that are neither null, nor empty, nor consisting only of whitespace.</returns>
        public static IEnumerable<string> ExcludeBlanks(IEnumerable<string?> strings)
        {
            return strings.Where(s => !string.IsNullOrWhiteSpace(s)).OfType<string>();
        }

        [GeneratedRegex(@"\$\{(.*?)\}")]
        private static partial Regex PlaceholderRegex();

        [GeneratedRegex(@"\s")]
        private static partial Regex WhitespaceRegex();

        [GeneratedRegex(@"^\d+$")]
        private static partial Regex OnlyDigitsRegex();
    }
}
