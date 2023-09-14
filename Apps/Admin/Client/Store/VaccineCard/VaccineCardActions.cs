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
        public class MailVaccineCardAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MailVaccineCardAction"/> class.
            /// </summary>
            /// <param name="phn">The personal health number that matches the document to request to mail.</param>
            /// <param name="mailAddress">The mailing address that matches the document to request to mail.</param>
            public MailVaccineCardAction(string phn, Address mailAddress)
            {
                this.Phn = phn;
                this.MailAddress = mailAddress;
            }

            /// <summary>
            /// Gets the personal health number that matches the document that was requested to mail.
            /// </summary>
            public string Phn { get; init; }

            /// <summary>
            /// Gets the mailing address that matches the document that was requested to mail.
            /// </summary>
            public Address MailAddress { get; init; }
        }

        /// <summary>
        /// The action representing a successful request to trigger the process to physically mail the vaccine card document.
        /// </summary>
        public class MailVaccineCardSuccessAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MailVaccineCardSuccessAction"/> class.
            /// </summary>
            /// <param name="phn">The personal health number that matches the document.</param>
            /// <param name="mailAddress">The mailing address that matches the document.</param>
            public MailVaccineCardSuccessAction(string phn, Address mailAddress)
            {
            }
        }

        /// <summary>
        /// The action representing a failed request to trigger the process to physically mail the vaccine card document.
        /// </summary>
        public class MailVaccineCardFailureAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MailVaccineCardFailureAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public MailVaccineCardFailureAction(RequestError error)
                : base(error)
            {
            }
        }

        /// <summary>
        /// The action representing the request to get the COVID-19 Vaccine Record document that includes the Vaccine Card and
        /// Vaccination History.
        /// </summary>
        public class PrintVaccineCardAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PrintVaccineCardAction"/> class.
            /// </summary>
            /// <param name="phn">The personal health number that matches the document.</param>
            public PrintVaccineCardAction(string phn)
            {
                this.Phn = phn;
            }

            /// <summary>
            /// Gets the personal health number that matches the document to retrieve.
            /// </summary>
            public string Phn { get; init; }
        }

        /// <summary>
        /// The action representing a successful request to get the COVID-19 Vaccine Record document that includes the Vaccine Card
        /// and Vaccination History.
        /// </summary>
        public class PrintVaccineCardSuccessAction : BaseSuccessAction<ReportModel>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PrintVaccineCardSuccessAction"/> class.
            /// </summary>
            /// <param name="data">Result data.</param>
            /// <param name="phn">The personal health number that matches the document.</param>
            public PrintVaccineCardSuccessAction(ReportModel data, string phn)
                : base(data)
            {
            }
        }

        /// <summary>
        /// The action representing a failed request to get the COVID-19 Vaccine Record document that includes the Vaccine Card and
        /// Vaccination History.
        /// </summary>
        public class PrintVaccineCardFailureAction : BaseFailAction
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PrintVaccineCardFailureAction"/> class.
            /// </summary>
            /// <param name="error">The request error.</param>
            public PrintVaccineCardFailureAction(RequestError error)
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
