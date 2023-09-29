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
    public static class ChangeFeedConfiguration
    {
        /// <summary>
        /// The configuration section key.
        /// </summary>
        public static readonly string ConfigurationSectionKey = "ChangeFeed";

        /// <summary>
        /// The internal change feed configuration key for dependent events.
        /// </summary>
        public static readonly string DependentsKey = "Dependents";

        /// <summary>
        /// The internal change feed configuration key for account events.
        /// </summary>
        public static readonly string AccountsKey = "Accounts";

        /// <summary>
        /// The internal change feed configuration key for notification verification events.
        /// </summary>
        public static readonly string NotificationChannelVerifiedKey = "NotificationChannelVerified";
    }
}
