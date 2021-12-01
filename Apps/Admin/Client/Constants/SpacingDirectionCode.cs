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
namespace HealthGateway.Admin.Client.Constants
{
    /// <summary>
    /// A class with constants representing the various codes for spacing directions.
    /// </summary>
    public static class SpacingDirectionCode
    {
        /// <summary>
        /// The code for left spacing.
        /// </summary>
        public const char Left = 'l';

        /// <summary>
        /// The code for right spacing.
        /// </summary>
        public const char Right = 'r';

        /// <summary>
        /// The code for top spacing.
        /// </summary>
        public const char Top = 't';

        /// <summary>
        /// The code for bottom spacing.
        /// </summary>
        public const char Bottom = 'b';

        /// <summary>
        /// The code for left and right spacing.
        /// </summary>
        public const char Horizontal = 'x';

        /// <summary>
        /// The code for top and bottom spacing.
        /// </summary>
        public const char Vertical = 'y';

        /// <summary>
        /// The code for left, right, top, and bottom spacing.
        /// </summary>
        public const char All = 'a';
    }
}
