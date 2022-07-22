// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.Constants
{
    /// <summary>
    /// A class with constants representing the various email template names.
    /// </summary>
    public static class FhirResource
    {
        /// <summary>
        /// Wildcard representing all available resources under the given context.
        /// </summary>
        public const string Wildcard = "*";

        /// <summary>
        /// Information about observations performed by a healthcare provider
        /// See <a href="http://www.hl7.org/fhir/stu3/observation.html"/>.
        /// </summary>
        public const string Observation = "Observation";

        /// <summary>
        /// Information about a patient's immunizations.
        /// See <a href="http://www.hl7.org/fhir/stu3/immunization.html"/>.
        /// </summary>
        public const string Immunization = "Immunization";

        /// <summary>
        /// Information about a medication statement.
        /// See <a href="http://www.hl7.org/fhir/stu3/medicationstatement.html"/>.
        /// </summary>
        public const string MedicationStatement = "MedicationStatement";

        /// <summary>
        /// Information about a medication request.
        /// See <a href="http://www.hl7.org/fhir/stu3/medicationrequest.html"/>.
        /// </summary>
        public const string MedicationRequest = "MedicationRequest";

        /// <summary>
        /// Demographic information about the patient.
        /// See <a href="http://www.hl7.org/fhir/stu3/patient.html"/>.
        /// </summary>
        public const string Patient = "Patient";

        /// <summary>
        /// An interaction between a patient and healthcare provider(s) for the purpose of providing healthcare service(s) or
        /// assessing the health status of a patient.
        /// See <a href="http://www.hl7.org/fhir/stu3/encounter.html"/>.
        /// </summary>
        public const string Encounter = "Encounter";

        /// <summary>
        /// The Health Gateway specific resource that represents a UserProfile.
        /// </summary>
        public const string UserProfile = "UserProfile";
    }
}
