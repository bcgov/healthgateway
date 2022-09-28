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
    /// Utilities for interacting with media query breakpoints.
    /// </summary>
    public static class BreakpointUtility
    {
        /// <summary>
        /// Retrieves the infix code for a particular breakpoint.
        /// </summary>
        /// <param name="breakpoint">The breakpoint to retrieve the code for.</param>
        /// <returns>A string containing the code for the breakpoint or null if the breakpoint has no associated code.</returns>
        public static string? GetCode(this Breakpoint breakpoint)
        {
            return breakpoint switch
            {
                Breakpoint.Xs => string.Empty,
                Breakpoint.Sm => "sm",
                Breakpoint.Md => "md",
                Breakpoint.Lg => "lg",
                Breakpoint.Xl => "xl",
                Breakpoint.Xxl => "xxl",
                Breakpoint.SmAndDown => string.Empty,
                Breakpoint.MdAndDown => string.Empty,
                Breakpoint.LgAndDown => string.Empty,
                Breakpoint.XlAndDown => string.Empty,
                Breakpoint.SmAndUp => "sm",
                Breakpoint.MdAndUp => "md",
                Breakpoint.LgAndUp => "lg",
                Breakpoint.XlAndUp => "xl",
                Breakpoint.None => null,
                Breakpoint.Always => string.Empty,
                _ => null,
            };
        }

        /// <summary>
        /// Retrieves the infix code that must be overridden to satisfy a particular breakpoint.
        /// </summary>
        /// <param name="breakpoint">The breakpoint to retrieve the override code for.</param>
        /// <returns>
        /// A string containing the override code for the breakpoint or null if the breakpoint has no associated override
        /// code.
        /// </returns>
        public static string? GetOverrideCode(this Breakpoint breakpoint)
        {
            return breakpoint switch
            {
                Breakpoint.SmAndDown => "md",
                Breakpoint.MdAndDown => "lg",
                Breakpoint.LgAndDown => "xl",
                Breakpoint.XlAndDown => "xx",
                _ => null,
            };
        }
    }
}
