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
namespace HealthGateway.ClinicalDocument.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.ClinicalDocument.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// The clinical document data service.
    /// </summary>
    public interface IClinicalDocumentService
    {
        /// <summary>
        /// Gets the collection of clinical document records associated with the given HDID.
        /// </summary>
        /// <param name="hdid">The subject's HDID.</param>
        /// <returns>The collection of clinical document records.</returns>
        Task<RequestResult<IEnumerable<ClinicalDocumentRecord>>> GetRecordsAsync(string hdid);

        /// <summary>
        /// Gets a specific clinical document file associated with the given HDID and file ID.
        /// </summary>
        /// <param name="hdid">The subject's HDID.</param>
        /// <param name="fileId">The ID of the file to fetch.</param>
        /// <returns>The specified clinical document file.</returns>
        Task<RequestResult<EncodedMedia>> GetFileAsync(string hdid, string fileId);
    }
}
