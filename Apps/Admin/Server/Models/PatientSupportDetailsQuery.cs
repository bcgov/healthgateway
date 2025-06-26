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
namespace HealthGateway.Admin.Server.Models
{
    using HealthGateway.Admin.Common.Constants;

    /// <summary>
    /// Query to retrieve patient support details.
    /// </summary>
    public record PatientSupportDetailsQuery
    {
        /// <summary>
        /// Gets the type of query.
        /// </summary>
        public ClientRegistryType QueryType { get; init; }

        /// <summary>
        /// Gets the query parameter.
        /// </summary>
        public string QueryParameter { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether messaging verifications should be included in the result.
        /// </summary>
        public bool IncludeMessagingVerifications { get; init; }

        /// <summary>
        /// Gets a value indicating whether blocked data sources should be included in the result.
        /// </summary>
        public bool IncludeBlockedDataSources { get; init; }

        /// <summary>
        /// Gets a value indicating whether agent actions should be included in the result.
        /// </summary>
        public bool IncludeAgentActions { get; init; }

        /// <summary>
        /// Gets a value indicating whether dependents should be included in the result.
        /// </summary>
        public bool IncludeDependents { get; init; }

        /// <summary>
        /// Gets a value indicating whether COVID-19 details should be included in the result.
        /// </summary>
        public bool IncludeCovidDetails { get; init; }

        /// <summary>
        /// Gets a value indicating whether Api Registration status should be included in the result.
        /// </summary>
        public bool IncludeApiRegistration { get; init; }

        /// <summary>
        /// Gets a value indicating whether imaging refresh data should be included in the result.
        /// </summary>
        public bool IncludeImagingRefresh { get; init; }

        /// <summary>
        /// Gets a value indicating whether labs refresh data should be included in the result.
        /// </summary>
        public bool IncludeLabsRefresh { get; init; }

        /// <summary>
        /// Gets a value indicating whether the query should force cached vaccine validation details data to be refreshed.
        /// </summary>
        public bool RefreshVaccineDetails { get; init; }
    }
}
