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
namespace HealthGateway.Admin.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models.Support;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Service that provides Covid Support functionality.
    /// </summary>
    public interface ICovidSupportService
    {
        /// <summary>
        /// Gets the patient and immunization information.
        /// </summary>
        /// <param name="phn">The personal health number that matches the person to retrieve.</param>
        /// <returns>The covid ionformation wrapped in a RequestResult.</returns>
        Task<RequestResult<CovidInformation>> GetCovidInformation(string phn);

        /// <summary>
        /// Gets all the emails in the system up to the pageSize.
        /// </summary>
        /// <param name="request">The request information to retrieve patient information.</param>
        /// <returns>A RequestResult with True if the request was sucessfull.</returns>
        Task<PrimitiveRequestResult<bool>> MailDocumentAsync(MailDocumentRequest request);

        /// <summary>
        /// Gets all the emails in the system up to the pageSize.
        /// </summary>
        /// <param name="phn">The personal health number that matches the person to retrieve.</param>
        /// <param name="address">The optional patient address information.</param>
        /// <returns>The encoded document.</returns>
        Task<RequestResult<ReportModel>> RetrieveDocumentAsync(string phn, Address? address);
    }
}
