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
namespace HealthGateway.Common.Constants
{
    /// <summary>
    /// Change feed configuration constants.
    /// </summary>
    public class ChangeFeedOptions
    {
        /// <summary>
        /// The configuration section key.
        /// </summary>
        public const string ChangeFeed = "ChangeFeed";

        /// <summary>
        /// Gets or sets the configuration for users events.
        /// </summary>
        public ChangeFeedConfiguration Dependents { get; set; } = new(false);

        /// <summary>
        /// Gets or sets the configuration for accounts events.
        /// </summary>
        public ChangeFeedConfiguration Accounts { get; set; } = new(false);

        /// <summary>
        /// Gets or sets the configuration for notifications events.
        /// </summary>
        public ChangeFeedConfiguration Notifications { get; set; } = new(false);

        /// <summary>
        /// Gets or sets the configuration for blocked data sources events.
        /// </summary>
        public ChangeFeedConfiguration BlockedDataSources { get; set; } = new(false);
    }

    /// <summary>
    /// Change feed event configuration.
    /// </summary>
    public record ChangeFeedConfiguration(bool Enabled);
}
