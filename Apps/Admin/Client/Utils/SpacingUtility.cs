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
namespace HealthGateway.Admin.Client.Utils
{
    using MudBlazor;

    /// <summary>
    /// A class with utility methods relating to spacing in the UI.
    /// </summary>
    public static class SpacingUtility
    {
        /// <summary>
        /// Generates the MudBlazor class(es) associated with a margin definition.
        /// </summary>
        /// <param name="directionCode">The code for the the margin direction.</param>
        /// <param name="breakpoint">The breakpoint where the margin should be applied.</param>
        /// <param name="size">The amount of spacing for the margin.</param>
        /// <returns>A string containing the generated MudBlazor class(es) or empty if the breakpoint is invalid.</returns>
        public static string GenerateMarginClasses(char directionCode, Breakpoint breakpoint, uint size)
        {
            string retVal = string.Empty;

            string? breakpointCode = breakpoint.GetCode();
            string? breakpointOverrideCode = breakpoint.GetOverrideCode();

            if (breakpointCode != null)
            {
                retVal = FormatMarginClass(directionCode, breakpointCode, size);

                if (breakpointOverrideCode != null)
                {
                    retVal += $" {FormatMarginClass(directionCode, breakpointOverrideCode, 0)}";
                }
            }

            return retVal;
        }

        private static string FormatMarginClass(char directionCode, string breakpointCode, uint size)
        {
            string breakpointSection = string.IsNullOrEmpty(breakpointCode) ? string.Empty : $"{breakpointCode}-";
            return $"m{directionCode}-{breakpointSection}{size}";
        }
    }
}
