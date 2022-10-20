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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.ResourceServer.Requirements
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// UserRequirement asserts authorization that the user is a Patient (has an HDID).
    /// </summary>
    public class UmaRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmaRequirement"/> class.
        /// </summary>
        /// <param name="validateRequestingPartyToken">If true validates the user HDID matches the resource HDID.</param>
        public UmaRequirement(bool validateRequestingPartyToken)
        {
            this.ValidateRequestingPartyToken = validateRequestingPartyToken;
        }

        /// <summary>
        /// Gets a value indicating whether the ownership of the resource should be confirmed.
        /// </summary>
        public bool ValidateRequestingPartyToken { get; }
    }
}
