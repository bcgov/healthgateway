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
namespace HealthGateway.Database.Constants
{
    /// <summary>
    /// The application type being recorded in the DB.
    /// </summary>
    public static class ApplicationType
    {
        /// <summary>
        /// Represents the Configuration application.
        /// </summary>
        public const string Configuration = "CFG";

        /// <summary>
        /// Represents the WebClient application.
        /// </summary>
        public const string WebClient = "WEB";

        /// <summary>
        /// Represents the Immunization application.
        /// </summary>
        public const string Immunization = "IMM";

        /// <summary>
        /// Represents the Patient application.
        /// </summary>
        public const string Patient = "PAT";

        /// <summary>
        /// Represents the Medication application.
        /// </summary>
        public const string Medication = "MED";

        /// <summary>
        /// Represents the Laboratory application.
        /// </summary>
        public const string Laboratory = "LAB";

        /// <summary>
        /// Represents the Admin Client application.
        /// </summary>
        public const string AdminWebClient = "ADMIN";

        /// <summary>
        /// Represents the Job Scheduler application.
        /// </summary>
        public const string JobScheduler = "JOBS";

        /// <summary>
        /// Represents the Encounter application.
        /// </summary>
        public const string Encounter = "ENC";

        /// <summary>
        /// Represents the Clinical document application.
        /// </summary>
        public const string ClinicalDocument = "CDO";
    }
}
