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
namespace HealthGateway.Common.Models.PHSA
{
    using System;

    /// <summary>
    /// Model object representing form parameter values when posting for an access token.
    /// </summary>
    public class PhsaConfigV2
    {
        /// <summary>
        /// The section key to use when binding this object.
        /// </summary>
        public const string ConfigurationSectionKey = "PhsaV2";

        /// <summary>
        /// Gets or sets the phsa base endpoint for tokens.
        /// </summary>
        public Uri TokenBaseUrl { get; set; } = null!;

        /// <summary>
        /// Gets or sets the phsa base endpoint for data.
        /// </summary>
        public Uri BaseUrl { get; set; } = null!;

        /// <summary>
        /// Gets or sets the phsa base endpoint for admin data.
        /// </summary>
        public Uri AdminBaseUrl { get; set; } = null!;

        /// <summary>
        /// Gets or sets the client id of the token being used to swap.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client secret of the token being used to swap.
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the grant type of the token being used to swap.
        /// </summary>
        public string GrantType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scope of the token being swapped for.
        /// </summary>
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to cache the PHSA token.
        /// </summary>
        public bool TokenCacheEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes to cache Personal Accounts.
        /// </summary>
        public int PersonalAccountsCacheTtl { get; set; } = 90;
    }
}
