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
namespace HealthGateway.Laboratory.Models.PHSA
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// An instance of a COVID-19 Order.
    /// </summary>
    public class PhsaCovid19Order
    {
        /// <summary>
        /// Gets or sets the id for the COVID-19 order.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the sourceSystemId for the lab order.
        /// </summary>
        [JsonPropertyName("sourceSystemId")]
        public string SourceSystemId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the PHN the order is for.
        /// </summary>
        [JsonPropertyName("phn")]
        public string Phn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Provider IDs for the Order.
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
        [JsonPropertyName("ormOrOru")]
        public string LabType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Message datetime.
        /// </summary>
        [JsonPropertyName("messageDateTime")]
        public DateTime MessageDateTime { get; set; }

        /// <summary>
        /// Gets or sets the message ID.
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
        /// Gets or sets the list of COVID-19 tests.
        /// </summary>
        [JsonPropertyName("labResults")]
        public IEnumerable<PhsaCovid19Test>? Covid19Tests { get; set; }
    }
}
