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
    using HealthGateway.Laboratory.Models.PHSA;

    /// <summary>
    /// An instance of a COVID-19 Model.
    /// </summary>
    public class PhsaCovid19Order
    {
        /// <summary>
        /// Gets or sets the id for the COVID-19 result.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the PHN the report is for.
        /// </summary>
        [JsonPropertyName("phn")]
        public string PHN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Provider IDs for the Model.
        /// </summary>
        [JsonPropertyName("orderingProviderIds")]
        public string OrderProviderIDs { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the providers names.
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
        /// Gets or sets the message id.
        /// </summary>
        [JsonPropertyName("messageId")]
        public string MessageID { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional related data.
        /// </summary>
        [JsonPropertyName("additionalData")]
        public string AdditionalData { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether a report is available.
        /// </summary>
        [JsonPropertyName("reportAvailable")]
        public bool ReportAvailable { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of COVID-19 tests.
        /// </summary>
        [JsonPropertyName("labResults")]
        public IEnumerable<PhsaCovid19Test>? Covid19Tests { get; set; }

        /// <summary>
        /// Creates a Covid19Model object from a PHSA model.
        /// </summary>
        /// <param name="model">The laboratory result to convert.</param>
        /// <returns>The newly created laboratory object.</returns>
        public static PhsaCovid19Order FromPHSAModel(PhsaCovid19Order model)
        {
            return new PhsaCovid19Order()
            {
                Id = model.Id,
                PHN = MaskPHN(model.PHN),
                OrderProviderIDs = model.OrderProviderIDs,
                OrderingProviders = model.OrderingProviders,
                ReportingLab = model.ReportingLab,
                Location = model.Location,
                LabType = model.LabType,
                MessageDateTime = model.MessageDateTime,
                MessageID = model.MessageID,
                AdditionalData = model.AdditionalData,
                ReportAvailable = model.ReportAvailable,
                Covid19Tests = model.Covid19Tests,
            };
        }

        /// <summary>
        /// Creates a Covid19Model object from a PHSA model.
        /// </summary>
        /// <param name="models">The list of PHSA models to convert.</param>
        /// <returns>A list of Covid19Model objects.</returns>
        public static IEnumerable<PhsaCovid19Order> FromPHSAModelList(IEnumerable<PhsaCovid19Order>? models)
        {
            List<PhsaCovid19Order> objects = new();
            if (models != null)
            {
                foreach (PhsaCovid19Order phsaModel in models)
                {
                    objects.Add(PhsaCovid19Order.FromPHSAModel(phsaModel));
                }
            }

            return objects;
        }

        private static string MaskPHN(string phn)
        {
            string retPHN = "****";
            if (phn != null && phn.Length > 3)
            {
                retPHN = $"{phn.Remove(phn.Length - 5, 4)}****";
            }

            return retPHN;
        }
    }
}
