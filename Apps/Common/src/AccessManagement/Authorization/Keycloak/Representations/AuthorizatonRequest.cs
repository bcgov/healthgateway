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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation
{
    using System.Collections.Generic;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation.Tokens;

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
        /// Gets or sets the Permissions.
        /// </summary>
        public PermissionTicketToken? Permissions { get; set; }

        /// <summary>
        /// Gets or sets the Metadata.
        /// </summary>       
        public Metadata? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the Audience.
        /// </summary>  
        public string? Audience { get; set; }

        /// <summary>
        /// Gets or sets the SubjectToken.
        /// </summary>
        public string? SubjectToken { get; set; }

        /// <summary>
        /// Gets or sets the SubmitRequest.
        /// </summary>      
        public bool SubmitRequest { get; set; } = false;

        /// <summary>
        /// Gets or sets the dictionary of a list of claims.
        /// </summary>  
        public Dictionary<string, List<string>>? Claims { get; set; }

        /// <summary>
        /// Gets or sets the AccessToken.
        /// </summary>          
        public AccessToken? Rpt { get; set; }

        /// <summary>
        /// Gets or sets the relying party token, RptToken.
        /// </summary>
        public string? RptToken { get; set; }
    }
}