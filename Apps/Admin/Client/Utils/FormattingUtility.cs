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
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Utility for formatting enums and booleans into human-readable strings for display.
    /// </summary>
    public static class FormattingUtility
    {
        /// <summary>
        /// Formats a broadcast action type for display.
        /// </summary>
        /// <param name="actionType">The broadcast action type to format.</param>
        /// <returns>A string formatted for display.</returns>
        public static string FormatBroadcastActionType(BroadcastActionType actionType)
        {
            return actionType switch
            {
                BroadcastActionType.None => "None",
                BroadcastActionType.InternalLink => "Internal Link",
                BroadcastActionType.ExternalLink => "External Link",
                _ => actionType.ToString(),
            };
        }

        /// <summary>
        /// Formats a broadcast's enabled status for display.
        /// </summary>
        /// <param name="enabled">The enabled status to format.</param>
        /// <returns>A string formatted for display.</returns>
        public static string FormatBroadcastEnabled(bool enabled)
        {
            return enabled ? "Publish" : "Draft";
        }

        /// <summary>
        /// Formats a communication status for display.
        /// </summary>
        /// <param name="status">The communication status to format.</param>
        /// <returns>A string formatted for display.</returns>
        [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
        public static string FormatCommunicationStatus(CommunicationStatus status)
        {
            return status switch
            {
                CommunicationStatus.New => "Publish",
                _ => status.ToString(),
            };
        }

        /// <summary>
        /// Formats a communication type for display.
        /// </summary>
        /// <param name="communicationType">The communication type to format.</param>
        /// <returns>A string formatted for display.</returns>
        [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
        public static string FormatCommunicationType(CommunicationType? communicationType)
        {
            return communicationType switch
            {
                CommunicationType.Banner => "Public Banner",
                CommunicationType.InApp => "In-App Banner",
                CommunicationType.Mobile => "Mobile Communication",
                _ => string.Empty,
            };
        }

        /// <summary>
        /// Formats a data source for display.
        /// </summary>
        /// <param name="dataSource">The data source to format.</param>
        /// <returns>A string formatted for display.</returns>
        [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
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

        /// <summary>
        /// Formats an agent's identity provider for display.
        /// </summary>
        /// <param name="identityProvider">The identity provider to format.</param>
        /// <returns>A string formatted for display.</returns>
        [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
        public static string FormatKeycloakIdentityProvider(KeycloakIdentityProvider identityProvider)
        {
            return identityProvider switch
            {
                KeycloakIdentityProvider.Idir => "IDIR",
                KeycloakIdentityProvider.PhsaAzure => "PHSA",
                _ => identityProvider.ToString(),
            };
        }

        /// <summary>
        /// Formats a patient status for display.
        /// </summary>
        /// <param name="status">The patient status to format.</param>
        /// <returns>A string formatted for display.</returns>
        [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
        public static string FormatPatientStatus(PatientStatus status)
        {
            return status switch
            {
                PatientStatus.NotFound => "Patient not found",
                PatientStatus.Deceased => "Patient is deceased",
                PatientStatus.NotUser => "Patient is not a user",
                _ => string.Empty,
            };
        }

        /// <summary>
        /// Formats a patient query type for display.
        /// </summary>
        /// <param name="queryType">The query type to format.</param>
        /// <returns>A string formatted for display.</returns>
        [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
        public static string FormatPatientQueryType(PatientQueryType queryType)
        {
            return queryType switch
            {
                PatientQueryType.Hdid => "HDID",
                PatientQueryType.Phn => "PHN",
                PatientQueryType.Sms => "SMS",
                _ => queryType.ToString(),
            };
        }
    }
}
