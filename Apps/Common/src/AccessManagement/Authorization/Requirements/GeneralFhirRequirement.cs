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
namespace HealthGateway.Common.AccessManagement.Authorization.Requirements
{
    using HealthGateway.Common.Constants;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Asserts authorization to one or more FHIR resources.
    /// </summary>
    public class GeneralFhirRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralFhirRequirement"/> class.
        /// </summary>
        /// <param name="resourceType">The resource type (<see cref="FhirResource"/>) to validate.</param>
        /// <param name="accessType">The access type (<see cref="FhirAccessType"/>) to validate.</param>
        /// <param name="supportsSystemDelegation">Indicates if system-delegated authorization is permitted.</param>
        public GeneralFhirRequirement(string resourceType, string accessType, bool supportsSystemDelegation = true)
        {
            this.ResourceType = resourceType;
            this.AccessType = accessType;
            this.SupportsSystemDelegation = supportsSystemDelegation;
        }

        /// <summary>
        /// Gets the FHIR resource type to validate.
        /// See <see cref="FhirResource"/>.
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// Gets the access type (read or write) required for the specified resource.
        /// See <see cref="FhirAccessType"/>.
        /// </summary>
        public string AccessType { get; }

        /// <summary>
        /// Gets a value indicating whether system delegation is permitted for this requirement.
        /// </summary>
        public bool SupportsSystemDelegation { get; }
    }
}
