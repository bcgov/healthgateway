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

    /// <summary>An interface representing a Policy.</summary>
    public interface IPolicy
    {
        /// <summary>Gets or sets the policy ID.</summary>
        public string? Id { get; set; }

        /// <summary>Gets or sets the policy name.</summary>
        public string? Name { get; set; }

        /// <summary>Gets or sets the policy description.</summary>
        public string? Description { get; set; }

        /// <summary>Gets or sets the policy type.</summary>
        public string? Type { get; set; }

        /// <summary>Gets or sets the list of policies.</summary>
        public List<string>? Policies { get; set; }

        /// <summary>Gets or sets the list of resources.</summary>
        public List<string>? Resources { get; set; }

        /// <summary>Gets or sets the list of scopes.</summary>
        public List<string>? Scopes { get; set; }

        /// <summary>Gets or sets the Logic setting.</summary>
        public Logic Logic { get; set; }

        /// <summary>Gets or sets the DecisionStrategy setting.</summary>
        public DecisionStrategy DecisionStrategy { get; set; }

        /// <summary>Gets or sets the policy owner.</summary>
        public string? Owner { get; set; }
    }
}