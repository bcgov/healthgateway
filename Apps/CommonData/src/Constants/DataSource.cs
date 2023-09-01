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
    using System.Text.Json.Serialization;

    /// <summary>
    /// Data sources for blocked access.
    /// </summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum DataSource
    {
        /// <summary>
        /// Represents an unknown data source.
        /// </summary>
        Unknown,

        /// <summary>
        /// Represents a data source for clinical documents.
        /// </summary>
        ClinicalDocument,

        /// <summary>
        /// Represents a data source for covid 19 test results.
        /// </summary>
        Covid19TestResult,

        /// <summary>
        /// Represents a data source for health visits.
        /// </summary>
        HealthVisit,

        /// <summary>
        /// Represents a data source for hospitalVisits.
        /// </summary>
        HospitalVisit,

        /// <summary>
        /// Represents a data source for immunizations.
        /// </summary>
        Immunization,

        /// <summary>
        /// Represents a data source for lab results.
        /// </summary>
        LabResult,

        /// <summary>
        /// Represents a data source for medications.
        /// </summary>
        Medication,

        /// <summary>
        /// Represents a data source for notes.
        /// </summary>
        Note,

        /// <summary>
        /// Represents a data source for special authority requests.
        /// </summary>
        SpecialAuthorityRequest,

        /// <summary>
        /// Represents a data source for diagnostic imaging.
        /// </summary>
        DiagnosticImaging,

        /// <summary>
        /// Represents a data source for the Organ Donor Registration service.
        /// </summary>
        OrganDonorRegistration,

        /// <summary>
        /// Represents a data source for BC Cancer screenings.
        /// </summary>
        BcCancerScreening,
    }
}
