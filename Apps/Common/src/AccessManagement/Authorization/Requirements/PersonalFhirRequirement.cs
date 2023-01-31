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

    /// <summary>
    /// Asserts authorization to one or more FHIR resources associated with a particular patient.
    /// </summary>
    public class PersonalFhirRequirement : GeneralFhirRequirement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalFhirRequirement"/> class.
        /// </summary>
        /// <param name="resourceType">The resource type (<see cref="FhirResource"/>) to validate.</param>
        /// <param name="accessType">The access type (<see cref="FhirAccessType"/>) to validate.</param>
        /// <param name="subjectLookupMethod">
        /// The mechanism with which to retrieve the subject identifier for the requested health data resource(s).
        /// </param>
        /// <param name="supportsSystemDelegation">Indicates if system-delegated authorization may occur.</param>
        /// <param name="supportsUserDelegation">Indicates if user-delegated authorization may occur.</param>
        public PersonalFhirRequirement(
            string resourceType,
            string accessType,
            FhirSubjectLookupMethod subjectLookupMethod = FhirSubjectLookupMethod.Route,
            bool supportsSystemDelegation = true,
            bool supportsUserDelegation = false)
            : base(resourceType, accessType, supportsSystemDelegation)
        {
            this.SubjectLookupMethod = subjectLookupMethod;
            this.SupportsUserDelegation = supportsUserDelegation;
        }

        /// <summary>
        /// Gets the mechanism with which to retrieve the subject identifier for the requested health data resource(s).
        /// </summary>
        public FhirSubjectLookupMethod SubjectLookupMethod { get; }

        /// <summary>
        /// Gets a value indicating whether user delegation is supported for this requirement.
        /// </summary>
        public bool SupportsUserDelegation { get; }
    }
}
