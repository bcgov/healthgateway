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
namespace HealthGateway.Database.Constants
{
    /// <summary>
    /// The set of Comment entry types.
    /// </summary>
    public static class CommentEntryType
    {
        /// <summary>
        /// The code representing no entry type set.
        /// </summary>
        public const string None = "NA";

        /// <summary>
        /// The code representing Medication.
        /// </summary>
        public const string Medication = "Med";

        /// <summary>
        /// The code representing Immunization.
        /// </summary>
        public const string Immunization = "Imm";

        /// <summary>
        /// The code representing COVID-19 Laboratory orders.
        /// </summary>
        public const string Covid19Laboratory = "Lab";

        /// <summary>
        /// The code representing all Laboratory orders.
        /// </summary>
        public const string Laboratory = "ALO";

        /// <summary>
        /// The code representing Encounter.
        /// </summary>
        public const string Encounter = "Enc";

        /// <summary>
        /// The code representing Clinical Documents.
        /// </summary>
        public const string ClinicalDocuments = "CDO";

        /// <summary>
        /// The code representing Hospital Visits.
        /// </summary>
        public const string HospitalVisits = "Hos";
    }
}
