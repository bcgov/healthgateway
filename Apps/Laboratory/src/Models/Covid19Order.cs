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
namespace HealthGateway.Laboratory.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using HealthGateway.Laboratory.Models.PHSA;

    /// <summary>
    /// A model for a COVID-19 order.
    /// </summary>
    public class Covid19Order
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Covid19Order"/> class.
        /// </summary>
        public Covid19Order()
        {
            this.Covid19Tests = new List<Covid19Test>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Covid19Order"/> class.
        /// </summary>
        /// <param name="covid19Tests">The list of COVID-19 Test records.</param>
        [JsonConstructor]
        public Covid19Order(IList<Covid19Test> covid19Tests)
        {
            this.Covid19Tests = covid19Tests;
        }

        /// <summary>
        /// Gets or sets the id for the COVID-19 order.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the PHN the order is for.
        /// </summary>
        [JsonPropertyName("phn")]
        public string Phn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Provider IDs for the order.
        /// </summary>
        [JsonPropertyName("orderingProviderIds")]
        public string OrderProviderIds { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the providers' names.
        /// </summary>
        [JsonPropertyName("orderingProviders")]
        public string OrderingProviders { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the reporting lab.
        /// </summary>
        [JsonPropertyName("reportingLab")]
        public string ReportingLab { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets if this is an Order or Result.
        /// </summary>
        [JsonPropertyName("labType")]
        public string LabType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Message datetime.
        /// </summary>
        [JsonPropertyName("messageDateTime")]
        public DateTime MessageDateTime { get; set; }

        /// <summary>
        /// Gets or sets the message id.
        /// </summary>
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional related data.
        /// </summary>
        [JsonPropertyName("additionalData")]
        public string AdditionalData { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether a report is available.
        /// </summary>
        [JsonPropertyName("reportAvailable")]
        public bool ReportAvailable { get; set; }

        /// <summary>
        /// Gets the list of COVID-19 tests.
        /// </summary>
        [JsonPropertyName("labResults")]
        public IList<Covid19Test> Covid19Tests { get; }

        /// <summary>
        /// Creates a COVID-19 order model from a PHSA model.
        /// </summary>
        /// <param name="model">The laboratory result to convert.</param>
        /// <returns>The newly created laboratory object.</returns>
        public static Covid19Order FromPhsaModel(PhsaCovid19Order model)
        {
            IList<Covid19Test> covid19Tests =
                model.Covid19Tests != null ? model.Covid19Tests.Select(Covid19Test.FromPhsaModel).ToList() : new List<Covid19Test>();

            return new Covid19Order(covid19Tests)
            {
                Id = model.Id,
                Phn = MaskPhn(model.Phn),
                OrderProviderIds = model.OrderProviderIds,
                OrderingProviders = model.OrderingProviders,
                ReportingLab = model.ReportingLab,
                Location = model.Location,
                LabType = model.LabType,
                MessageDateTime = model.MessageDateTime,
                MessageId = model.MessageId,
                AdditionalData = model.AdditionalData,
                ReportAvailable = model.ReportAvailable,
            };
        }

        /// <summary>
        /// Creates a collection of <see cref="Covid19Order"/> models from a collection of PHSA models.
        /// </summary>
        /// <param name="phsaOrders">The collection of PHSA models to convert.</param>
        /// <returns>A collection of <see cref="Covid19Order"/> models.</returns>
        public static IEnumerable<Covid19Order> FromPhsaModelCollection(IEnumerable<PhsaCovid19Order>? phsaOrders)
        {
            return phsaOrders != null ? phsaOrders.Select(FromPhsaModel) : Enumerable.Empty<Covid19Order>();
        }

        private static string MaskPhn(string phn)
        {
            string retVal = "****";
            if (phn.Length > 3)
            {
                retVal = $"{phn.Remove(phn.Length - 5, 4)}****";
            }

            return retVal;
        }
    }
}
