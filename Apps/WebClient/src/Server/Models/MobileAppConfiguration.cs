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
namespace HealthGateway.WebClient.Server.Models
{
    /// <summary>
    /// Health Gateway mobile app overrides of the WebClient configuration, served as WebClient.Mobile.
    /// </summary>
    public class MobileAppConfiguration
    {
        /// <summary>
        /// Gets or sets the oldest app version (major.minor.build) allowed to continue; older apps must update.
        /// </summary>
        public string? MinimumVersion { get; set; }

        /// <summary>
        /// Gets or sets the mobile app timeout values.
        /// </summary>
        public MobileAppTimeoutsConfiguration? Timeouts { get; set; }

        /// <summary>
        /// Gets or sets the mobile app authentication settings.
        /// </summary>
        public MobileAppAuthenticationConfiguration? Authentication { get; set; }
    }
}
