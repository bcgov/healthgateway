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
namespace HealthGateway.Admin.Client.Store.PatientDetails
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class PatientDetailsActions
    {
        /// <summary>
        /// The action representing the initiation of a load.
        /// </summary>
        public record LoadAction
        {
            /// <summary>
            /// Gets the query type.
            /// </summary>
            public required ClientRegistryType QueryType { get; init; }

            /// <summary>
            /// Gets the query string.
            /// </summary>
            public required string QueryString { get; init; }

            /// <summary>
            /// Gets or sets a value indicating whether the call should force cached vaccine validation details data to be refreshed.
            /// </summary>
            public bool RefreshVaccineDetails { get; set; }
        }

        /// <summary>
        /// The action representing a failed load.
        /// </summary>
        public record LoadFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing a successful load.
        /// </summary>
        public record LoadSuccessAction : BaseSuccessAction<PatientSupportDetails>;

        /// <summary>
        /// The action representing the configuring of a patient's level of access.
        /// </summary>
        public record BlockAccessAction
        {
            /// <summary>
            /// Gets the patient's HDID to configure access for.
            /// </summary>
            public required string Hdid { get; init; }

            /// <summary>
            /// Gets the list of data sources to block.
            /// </summary>
            public required IEnumerable<DataSource> DataSources { get; init; }

            /// <summary>
            /// Gets the audit reason.
            /// </summary>
            public required string Reason { get; init; }
        }

        /// <summary>
        /// The action representing a successful update of a patient's blocked access.
        /// </summary>
        public record BlockAccessSuccessAction
        {
            /// <summary>
            /// Gets the patient's HDID which had access configured.
            /// </summary>
            public required string Hdid { get; init; }
        }

        /// <summary>
        /// The action representing a failed update of a patient's blocked access.
        /// </summary>
        public record BlockAccessFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing the initiation of a COVID-19 treatment assessment submission.
        /// </summary>
        public record SubmitCovid19TreatmentAssessmentAction
        {
            /// <summary>
            /// Gets the COVID-19 therapy assessment request.
            /// </summary>
            public required CovidAssessmentRequest Request { get; init; }

            /// <summary>
            /// Gets the PHN associated with the patient.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing a successful COVID-19 treatment assessment submission.
        /// </summary>
        public record SubmitCovid19TreatmentAssessmentSuccessAction
        {
            /// <summary>
            /// Gets the PHN associated with the patient.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing a failed COVID-19 treatment assessment submission.
        /// </summary>
        public record SubmitCovid19TreatmentAssessmentFailureAction : BaseFailureAction;

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public record ResetStateAction;
    }
}
