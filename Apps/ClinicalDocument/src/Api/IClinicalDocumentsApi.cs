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
namespace HealthGateway.ClinicalDocument.Api
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.ClinicalDocument.Models.PHSA;
    using HealthGateway.Common.Models.PHSA;
    using Refit;

    /// <summary>
    /// Interface to interact with PHSA Clinical Documents API.
    /// </summary>
    public interface IClinicalDocumentsApi
    {
        /// <summary>
        /// Retrieves clinical document records by patient identifier.
        /// </summary>
        /// <param name="pid">The patient id to get clinical document records.</param>
        /// <returns>The clinical document records for the patient identifier.</returns>
        [Get("/patient/{pid}/health-data?Categories=4")]
        Task<PhsaHealthDataResponse> GetClinicalDocumentRecordsAsync(string pid);

        /// <summary>
        /// Retrieves clinical document file by patient identifier and file id.
        /// </summary>
        /// <param name="pid">The patient id to get a clinical document file.</param>
        /// <param name="id">The file id to get a clinical document file.</param>
        /// <returns>The clinical document file for the patient identifier and file id.</returns>
        [Get("/patient/{pid}/file/{id}")]
        Task<EncodedMedia> GetClinicalDocumentFileAsync(Guid pid, string id);
    }
}
