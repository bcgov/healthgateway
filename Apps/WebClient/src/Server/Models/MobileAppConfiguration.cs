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
        /// Gets or sets the lowest app build number allowed to continue (the build in v3.0.0.0057, the same on iOS and
        /// Android); an app with a lower build must update. Unset: no check.
        /// </summary>
        public int? MinimumBuild { get; set; }

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
