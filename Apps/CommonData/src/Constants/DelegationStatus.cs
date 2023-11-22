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
namespace HealthGateway.Common.Data.Constants
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Invitation status for a Delegation.
    /// </summary>
    public enum DelegationStatus
    {
        /// <summary>
        /// Represents active status for a delegation.
        /// </summary>
        [EnumMember(Value = "Active")]
        Active,

        /// <summary>
        /// Represents access expired status for a delegation.
        /// </summary>
        [EnumMember(Value = "AccessExpired")]
        AccessExpired,

        /// <summary>
        /// Represents declined status for a delegation.
        /// </summary>
        [EnumMember(Value = "Declined")]
        Declined,

        /// <summary>
        /// Represents invite expired status for a delegation.
        /// </summary>
        [EnumMember(Value = "InviteExpired")]
        InviteExpired,

        /// <summary>
        /// Represents locked status for a delegation.
        /// </summary>
        [EnumMember(Value = "Locked")]
        Locked,

        /// <summary>
        /// Represents pending status for a delegation.
        /// </summary>
        [EnumMember(Value = "Pending")]
        Pending,
    }
}
