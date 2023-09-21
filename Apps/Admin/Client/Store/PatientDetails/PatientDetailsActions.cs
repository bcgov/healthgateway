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
        public class LoadAction
        {
            /// <summary>
            /// Gets or sets query string.
            /// </summary>
            public required string Hdid { get; set; }
        }

        /// <summary>
        /// The action representing a failed load.
        /// </summary>
        public class LoadFailAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LoadFailAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public LoadFailAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action representing a successful load.
        /// </summary>
        public class LoadSuccessAction : BaseSuccessAction<PatientSupportDetails>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LoadSuccessAction"/> class.
            /// </summary>
            /// <param name="data">Result data.</param>
            public LoadSuccessAction(PatientSupportDetails data)
                : base(data)
            {
            }
        }

        /// <summary>
        /// The action representing the configuring of a patient's level of access.
        /// </summary>
        public class BlockAccessAction
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
        public class BlockAccessSuccessAction
        {
            /// <summary>
            /// Gets the patient's HDID which had access configured.
            /// </summary>
            public required string Hdid { get; init; }
        }

        /// <summary>
        /// The action representing a failed update of a patient's blocked access.
        /// </summary>
        public class BlockAccessFailureAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BlockAccessFailureAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public BlockAccessFailureAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action representing the initiation of a COVID-19 treatment assessment submission.
        /// </summary>
        public class SubmitCovid19TreatmentAssessmentAction
        {
            /// <summary>
            /// Gets the COVID-19 therapy assessment request.
            /// </summary>
            public required CovidAssessmentRequest Request { get; init; }

            /// <summary>
            /// Gets the HDID associated with the patient.
            /// </summary>
            public required string Hdid { get; init; }
        }

        /// <summary>
        /// The action representing a successful COVID-19 treatment assessment submission.
        /// </summary>
        public class SubmitCovid19TreatmentAssessmentSuccessAction
        {
            /// <summary>
            /// Gets the HDID associated with the patient.
            /// </summary>
            public required string Hdid { get; init; }
        }

        /// <summary>
        /// The action representing a failed COVID-19 treatment assessment submission.
        /// </summary>
        public class SubmitCovid19TreatmentAssessmentFailAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SubmitCovid19TreatmentAssessmentFailAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public SubmitCovid19TreatmentAssessmentFailAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public class ResetStateAction
        {
        }
    }
}
