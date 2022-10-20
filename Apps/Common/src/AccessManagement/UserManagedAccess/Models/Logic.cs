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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Models
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>The decision strategy dictates how the policies associated with a given policy
    /// are evaluated and how a final decision is obtained.</summary>
    public enum Logic
    {
        /// <summary>Defines that this policy follows a positive logic. In other words, the final decision is the policy outcome.</summary>
        [EnumMember(Value="POSITIVE")]
        Positive,

        /// <summary>Defines that this policy uses a logical negation. In other words, the final decision would be a negative of the policy outcome.</summary>
        [EnumMember(Value="NEGATIVE")]
        Negative,
    }
}
