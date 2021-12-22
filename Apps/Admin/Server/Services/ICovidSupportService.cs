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
namespace HealthGateway.Admin.Server.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Models.CovidSupport;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Service that provides COVID-19 Support functionality.
    /// </summary>
    public interface ICovidSupportService
    {
        /// <summary>
        /// Gets the patient and immunization information.
        /// </summary>
        /// <param name="phn">The personal health number that matches the person to retrieve.</param>
        /// <param name="refresh">Whether the call should force cached data to be refreshed.</param>
        /// <returns>The covid ionformation wrapped in a RequestResult.</returns>
        Task<RequestResult<CovidInformation>> GetCovidInformation(string phn, bool refresh);

        /// <summary>
        /// Mails a document that represents a patient's vaccine card.
        /// </summary>
        /// <param name="request">The request information to retrieve patient information.</param>
        /// <returns>A RequestResult with True if the request was sucessfull.</returns>
        Task<PrimitiveRequestResult<bool>> MailVaccineCardAsync(MailDocumentRequest request);

        /// <summary>
        /// Gets a document that represents a patient's vaccine card and vaccine history.
        /// </summary>
        /// <param name="phn">The personal health number that matches the person to retrieve.</param>
        /// <returns>The encoded document.</returns>
        Task<RequestResult<ReportModel>> RetrieveVaccineRecordAsync(string phn);
    }
}
