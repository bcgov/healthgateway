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
    /// FhirRequirement asserts authorization to a fhir resource.
    /// </summary>
    public class FhirRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FhirRequirement"/> class.
        /// </summary>
        /// <param name="fhirResource">The fhir resource (<see cref="FhirResource"/>) to validate.</param>
        /// <param name="fhirAccessType">The accessType (<see cref="FhirAccessType"/>) to validate.</param>
        /// <param name="fhirLookup">The mechanism to find the fhir resource identifer.</param>
        /// <param name="supportsSystemDelegation">Indicates if system delegated authorization may occur.</param>
        /// <param name="supportsUserDelegation">Indicates if user delegated authorization may occur.</param>
        public FhirRequirement(
            string fhirResource,
            string fhirAccessType,
            FhirResourceLookup fhirLookup = FhirResourceLookup.Route,
            bool supportsSystemDelegation = true,
            bool supportsUserManagedAccess = true,
            bool supportsUserDelegation = false)
        {
            this.Resource = fhirResource;
            this.AccessType = fhirAccessType;
            this.Lookup = fhirLookup;
            this.SupportsSystemDelegation = supportsSystemDelegation;
            this.SupportsUserDelegation = supportsUserDelegation;
            this.SupportsUserManagedAccess = supportsUserManagedAccess;
        }

        /// <summary>
        /// Gets the fhir resource to validate.
        /// See <see cref="FhirResource"/>.
        /// </summary>
        public string Resource { get; }

        /// <summary>
        /// Gets the access type (read or write) required for the specified resource.
        /// See <see cref="FhirAccessType"/>.
        /// </summary>
        public string AccessType { get; }

        /// <summary>
        /// Gets the mechanism to find the fhir resource identifer.
        /// </summary>
        public FhirResourceLookup Lookup { get; }

        /// <summary>
        /// Gets a value indicating whether system delegation is supported for this requirement.
        /// </summary>
        public bool SupportsSystemDelegation { get; }

        /// <summary>
        /// Gets a value indicating whether user delegation is supported for this requirement.
        /// </summary>
        public bool SupportsUserDelegation { get; }
    }
}
