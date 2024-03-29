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
namespace HealthGateway.Admin.Client.Store.VaccineCard
{
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class VaccineCardActions
    {
        /// <summary>
        /// The action representing the request to trigger the process to physically mail the vaccine card document.
        /// </summary>
        public record MailVaccineCardAction
        {
            /// <summary>
            /// Gets the personal health number that matches the document that was requested to mail.
            /// </summary>
            public required string Phn { get; init; }

            /// <summary>
            /// Gets the mailing address that matches the document that was requested to mail.
            /// </summary>
            public required Address MailAddress { get; init; }
        }

        /// <summary>
        /// The action representing a successful request to trigger the process to physically mail the vaccine card document.
        /// </summary>
        public record MailVaccineCardSuccessAction;

        /// <summary>
        /// The action representing a failed request to trigger the process to physically mail the vaccine card document.
        /// </summary>
        public record MailVaccineCardFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing the request to get the COVID-19 Vaccine Record document that includes the Vaccine Card and
        /// Vaccination History.
        /// </summary>
        public record PrintVaccineCardAction
        {
            /// <summary>
            /// Gets the personal health number that matches the document to retrieve.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing a successful request to get the COVID-19 Vaccine Record document that includes the Vaccine Card
        /// and Vaccination History.
        /// </summary>
        public record PrintVaccineCardSuccessAction : BaseSuccessAction<ReportModel>;

        /// <summary>
        /// The action representing a failed request to get the COVID-19 Vaccine Record document that includes the Vaccine Card and
        /// Vaccination History.
        /// </summary>
        public record PrintVaccineCardFailureAction : BaseFailureAction;

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public record ResetStateAction;
    }
}
