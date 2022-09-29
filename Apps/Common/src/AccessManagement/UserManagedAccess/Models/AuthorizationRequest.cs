//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
namespace namespace HealthGateway.Common.UserManagedAccess.Models
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json.Serialization;

    using namespace HealthGateway.Common.UserManagedAccess.Models.Tokens;

    /// <summary>
    /// UMA 2.0 AuthorizationRequest.
    /// </summary>
    public class AuthorizationRequest
    {
        /// <summary>
        /// Gets or sets the Ticket.
        /// </summary>
        public string? Ticket { get; set; }

        /// <summary>
        /// Gets or sets the ClaimToken.
        /// </summary>
        public string? ClaimToken { get; set; }

        /// <summary>
        /// Gets or sets the ClaimTokenFormat.
        /// </summary>
        public string? ClaimTokenFormat { get; set; }

        /// <summary>
        /// Gets or sets the Pct.
        /// </summary>
        public string? Pct { get; set; }

        /// <summary>
        /// Gets or sets the Scope.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// Gets the Permissions.
        /// </summary>
        public PermissionTicketToken? Permissions { get; } = new PermissionTicketToken();

        /// <summary>
        /// Gets or sets the Metadata.
        /// </summary>
        public RequestMetadata? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the Audience.
        /// </summary>
        [JsonPropertyName("audience")]
        public string? Audience { get; set; }

        /// <summary>
        /// Gets or sets the SubjectToken.
        /// </summary>
        public string? SubjectToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SubmitRequest happened.
        /// </summary>
        public bool SubmitRequest { get; set; }

        /// <summary>
        /// Gets the dictionary of a list of claims.
        /// </summary>
        public Dictionary<string, List<string>>? Claims { get; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// Gets the AccessToken.
        /// </summary>
        public AccessToken Rpt { get; } = new AccessToken();

        /// <summary>
        /// Gets or sets the relying party token, RptToken.
        /// </summary>
        public string? RptToken { get; set; }
    }
}