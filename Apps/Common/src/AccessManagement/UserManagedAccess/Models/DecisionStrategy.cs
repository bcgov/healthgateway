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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Models
{
    using System.Text.Json.Serialization;
    using System.Runtime.Serialization;

    /// <summary>The decision strategy dictates how the policies associated with a given policy
    /// are evaluated and how a final decision is obtained.</summary>
    public enum DecisionStrategy
    {

        /// <summary>Defines that at least one policy must evaluate to a positive decision
        /// in order to the overall decision be also positive.</summary>
        [EnumMember(Value="AFFIRMATIVE")]
        Affirmative,

        /// <summary>Defines that all policies must evaluate to a positive
        /// decision in order to the overall decision be also positive.</summary>
        [EnumMember(Value="UNANIMOUS")]
        Unanimous,

        /// <summary>Defines that the number of positive decisions must be greater than the
        /// number of negative decisions. If the number of positive and negative is the same,
        /// the final decision will be negative.</summary>
        [EnumMember(Value="CONSENSUS")]
        Consensus,
    }
}