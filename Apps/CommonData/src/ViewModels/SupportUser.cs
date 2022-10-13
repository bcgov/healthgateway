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
namespace HealthGateway.Common.Data.ViewModels
{
    using System;

    /// <summary>
    /// Represents a Support User.
    /// </summary>
    public class SupportUser
    {
        /// <summary>
        /// Gets or sets the patient's PHN.
        /// </summary>
        public string PersonalHealthNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's hdid.
        /// </summary>
        public string Hdid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's last login date time.
        /// </summary>
        public DateTime LastLoginDateTime { get; set; }
    }
}
