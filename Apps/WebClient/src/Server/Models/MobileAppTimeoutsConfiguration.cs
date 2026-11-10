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
    /// Timeout values used by the Health Gateway mobile app.
    /// </summary>
    public class MobileAppTimeoutsConfiguration
    {
        /// <summary>
        /// Gets or sets the idle time in milliseconds before the app logs out, or locks when device security is on.
        /// </summary>
        public int? Idle { get; set; }
    }
}
