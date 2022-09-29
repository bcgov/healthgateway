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
namespace HealthGateway.Common.UserManagedAccess.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>An interface representing a Policy.</summary>
    public interface IPolicy
    {
        /// <summary>Gets or sets the policy ID.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>Gets or sets the policy name.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>Gets or sets the policy description.</summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>Gets or sets the policy type.</summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>Gets the list of policies.</summary>
        [JsonPropertyName("policies")]
        public List<string> Policies { get; }

        /// <summary>Gets the list of resources.</summary>
        [JsonPropertyName("resources")]
        public List<string> Resources { get; }

        /// <summary>Gets the list of scopes.</summary>
        [JsonPropertyName("scopes")]
        public List<string> Scopes { get;  }

        /// <summary>Gets or sets the Logic setting.</summary>
        [JsonPropertyName("logic")]
        public Logic Logic { get; set; }

        /// <summary>Gets or sets the DecisionStrategy setting.</summary>
        [JsonPropertyName("decision_strategy")]
        public DecisionStrategy DecisionStrategy { get; set; }

        /// <summary>Gets or sets the policy owner.</summary>
        [JsonPropertyName("owner")]
        public string? Owner { get; set; }
    }
}