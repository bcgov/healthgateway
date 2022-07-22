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
namespace HealthGateway.GatewayApi.Models
{
    using HealthGateway.Database.Models;

    /// <summary>
    /// Object that represents a banner change push.
    /// </summary>
    public class BannerChangeEvent
    {
        /// <summary>
        /// Gets or sets Table name the action was performed on.
        /// </summary>
        public string Table { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action that triggered the push.
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Communication object.
        /// </summary>
        public Communication? Data { get; set; }
    }
}
