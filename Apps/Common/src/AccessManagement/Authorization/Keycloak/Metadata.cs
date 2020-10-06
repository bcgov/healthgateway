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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak
{
    /// <summary>Metadata, a component of <cref name="AuthorizationRequest"/>.</summary>
    public class Metadata
    {
        /// <summary>Gets or sets whether to include Resource Name.</summary>
        public bool IncludeResourceName { get; set; }

        /// <summary>Gets or sets the limit.</summary>
        public int Limit { get; set; }

        /// <summary>Gets or sets the response mode.</summary>
        public string? ResponseMode { get; set; }
    }
}