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
    using System.Text.Json.Serialization;

    /// <summary>
    /// An instance of a Laboratory Model.
    /// </summary>
    public class LaboratoryModel
    {
        /// <summary>
        /// Gets or sets the id for the lab result.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

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
        /// Gets or sets the list of lab results.
        /// </summary>
        [JsonPropertyName("labResults")]
        public IEnumerable<LaboratoryResult>? LabResults { get; set; }

        /// <summary>
        /// Creates a LaboratoryModel object from a PHSA model.
        /// </summary>
        /// <param name="model">The laboratory result to convert.</param>
        /// <returns>The newly created laboratory object.</returns>
        public static LaboratoryModel FromPHSAModel(LaboratoryOrder model)
        {
            return new LaboratoryModel()
            {
                Id = model.SourceSystemId,
                PHN = model.PHN,
                OrderProviderIDs = model.OrderProviderIDs,
                OrderingProviders = model.OrderingProviders,
                ReportingLab = model.ReportingLab,
                Location = model.Location,
                LabType = model.LabType,
                MessageDateTime = model.MessageDateTime,
                MessageID = model.MessageID,
                AdditionalData = model.AdditionalData,
                ReportAvailable = model.ReportAvailable,
                LabResults = model.LabResults,
            };
        }

        /// <summary>
        /// Creates a LaboratoryModel object from a PHSA model.
        /// </summary>
        /// <param name="models">The list of PHSA models to convert.</param>
        /// <returns>A list of LaboratoryModel objects.</returns>
        public static IEnumerable<LaboratoryModel> FromPHSAModelList(IEnumerable<LaboratoryOrder>? models)
        {
            List<LaboratoryModel> objects = new List<LaboratoryModel>();
            if (models != null)
            {
                foreach (LaboratoryOrder immunizationModel in models)
                {
                    objects.Add(LaboratoryModel.FromPHSAModel(immunizationModel));
                }
            }

            return objects;
        }
    }
}
