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
    using System.Text;
    using System.Text.Json.Serialization;

    ///<summary>A bounded extent of access that is possible to perform on a resource set. In authorization policy terminology,
    /// a scope is one of the potentially many "verbs" that can logically apply to a resource set ("object").
    /// For more details, see "https://docs.kantarainitiative.org/uma/draft-oauth-resource-reg.html#rfc.section.2.1".</summary>.
    public class Scope
    {
        /// <summary>Gets or sets the scope ID.</summary>

        public string? id { get; set; }

        /// <summary>Gets or sets the scope name.</summary>
        public string? name { get; set; }

        /// <summary>Gets or sets the scope icon URI.</summary>
        public string? iconUri { get; set; }

        /// <summary>Gets or sets the scope policies.</summary>
        public List<Policy>? policies { get; set; }

        /// <summary>Gets or sets the scope resources.</summary>
        public List<Resource>? resources { get; set; }

        /// <summary>Gets or sets the scope displayName.</summary>
        public string? displayName { get; set; }
    }
}