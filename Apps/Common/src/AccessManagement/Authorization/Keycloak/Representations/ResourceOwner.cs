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
    using System.Text.Json.Serialization;

    /// <summary>
    /// OAuth 2.0 UMA Resource Owner. Information about a user protected resource owner.
    /// </summary>
    public class ResourceOwner
    {
        /// <summary>Gets or sets the resource owner identifier.</summary>
        string? Id { get; set; } 

        /// <summary>Gets or sets the resource owner name.</summary>
        string? Name { get; set; }
    }
}