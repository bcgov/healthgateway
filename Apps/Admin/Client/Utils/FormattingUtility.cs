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
namespace HealthGateway.Admin.Client.Utils
{
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Utility for formatting enums and booleans into human-readable strings.
    /// </summary>
    public static class FormattingUtility
    {
        /// <summary>
        /// Formats a data source for display.
        /// </summary>
        /// <param name="dataSource">The data source to format.</param>
        /// <returns>A string formatted for display.</returns>
        public static string FormatDataSource(DataSource dataSource)
        {
            return dataSource switch
            {
                DataSource.ClinicalDocument => "Clinical Documents",
                DataSource.Immunization => "Immunizations",
                DataSource.LabResult => "Laboratory Results",
                DataSource.DiagnosticImaging => "Diagnostic Imaging",
                DataSource.Medication => "Medications",
                DataSource.Note => "Notes",
                DataSource.HealthVisit => "Health Visits",
                DataSource.HospitalVisit => "Hospital Visits",
                DataSource.Covid19TestResult => "COVID-19 Test Results",
                DataSource.OrganDonorRegistration => "Organ Donor Registration",
                DataSource.SpecialAuthorityRequest => "Special Authority Requests",
                DataSource.BcCancerScreening => "BC Cancer Screenings",
                _ => dataSource.ToString(),
            };
        }
    }
}
