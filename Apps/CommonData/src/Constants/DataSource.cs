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
    /// <summary>
    /// Data sources for blocked access.
    /// </summary>
    public enum DataSource
    {
        /// <summary>
        /// Represents a data source for clinical document.
        /// </summary>
        ClinicalDocument,

        /// <summary>
        /// Represents a data source for covid 19 test result.
        /// </summary>
        Covid19TestResult,

        /// <summary>
        /// Represents a data source for health visit.
        /// </summary>
        HealthVisit,

        /// <summary>
        /// Represents a data source for hospitalVisit.
        /// </summary>
        HospitalVisit,

        /// <summary>
        /// Represents a data source for immunization.
        /// </summary>
        Immunization,

        /// <summary>
        /// Represents a data source for lab result.
        /// </summary>
        LabResult,

        /// <summary>
        /// Represents a data source for medication.
        /// </summary>
        Medication,

        /// <summary>
        /// Represents a data source for note.
        /// </summary>
        Note,

        /// <summary>
        /// Represents a data source for special authority request.
        /// </summary>
        SpecialAuthorityRequest,

        /// <summary>
        /// Represents a data source for diagnostic imaging.
        /// </summary>
        DiagnosticImaging,

        /// <summary>
        /// Represents a data source for Organ Donor Registration service.
        /// </summary>
        OrganDonorRegistration,
    }
}
