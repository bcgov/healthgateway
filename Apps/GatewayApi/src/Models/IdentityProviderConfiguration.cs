﻿//-------------------------------------------------------------------------
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
namespace HealthGateway.WebClient.Models
{
    /// <summary>
    /// An object representing a configured Health Gateway IdentityProvider.
    /// </summary>
    public class IdentityProviderConfiguration
    {
        /// <summary>
        /// Gets or sets the Id of the Identity Provider.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the Identity Provider.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Font Awesome Icon that we recommend to use.
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Identity Provider hint.
        /// </summary>
        public string Hint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this identity provider
        /// should be used.
        /// </summary>
        public bool Disabled { get; set; }
    }
}
