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
    using System.Collections.Generic;

    /// <summary>
    /// Model object representing form parameter values when posting for an access token.
    /// </summary>
    public class TokenSwapRequest
    {
        /// <summary>
        /// Gets or sets the phsa base endpoint.
        /// </summary>
        public Uri BaseUrl { get; set; } = null!;

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
        /// Gets or sets the token being swapped for.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets the form parameters to swap tokens.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> FormParameters
        {
            get
            {
                IEnumerable<KeyValuePair<string, string>> formParameters = new[]
                {
                    new KeyValuePair<string, string>("client_id", this.ClientId),
                    new KeyValuePair<string, string>("client_secret", this.ClientSecret),
                    new KeyValuePair<string, string>("grant_type", this.GrantType),
                    new KeyValuePair<string, string>("scope", this.Scope),
                    new KeyValuePair<string, string>("token", this.Token),
                };
                return formParameters;
            }
        }
    }
}
