﻿// -------------------------------------------------------------------------
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
namespace HealthGateway.WebClient.Models
{
    /// <summary>
    /// The user profile model.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string Hdid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user accepted the terms of service.
        /// </summary>
        public bool AcceptedTermsOfService { get; set; }

        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        public string Email { get; set; }
    }
}
