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
namespace HealthGateway.Common.Api
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models.CDogs;
    using Refit;

    /// <summary>
    /// Refit interface to interact with the Common Document Generation Service (CDOGS).
    /// </summary>
    public interface ICDogsApi
    {
        /// <summary>
        /// Generates a document.
        /// </summary>
        /// <param name="request">Model containing an inline template and set of substitution variables.</param>
        /// <returns>HttpResponseMessage containing the raw binary-encoded PDF that was generated.</returns>
        [Post("/api/v2/template/render")]
        Task<HttpResponseMessage> GenerateDocumentAsync([Body] CDogsRequestModel request);
    }
}
