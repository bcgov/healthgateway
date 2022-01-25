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
namespace HealthGateway.Laboratory.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Laboratory.Models;

    /// <summary>
    /// The laboratory data service.
    /// </summary>
    public interface ILaboratoryService
    {
        /// <summary>
        /// Returns a List of COVID-19 orders for the authenticated user.
        /// It has a collection of one or more COVID-19 results depending on the tests ordered.
        /// </summary>
        /// <param name="hdid">The requested hdid.</param>
        /// <param name="pageIndex">The page index to return.</param>
        /// <returns>The list of COVID-19 orders available for the authenticated user.</returns>
        Task<RequestResult<IEnumerable<Covid19Order>>> GetCovid19Orders(string hdid, int pageIndex = 0);

        /// <summary>
        /// Gets the Lab report for the supplied id belonging to the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the lab report to get.</param>
        /// <param name="hdid">The requested HDID which owns the reportId.</param>
        /// <param name="isCovid19">Indicates whether the COVID-19 report should be returned..</param>
        /// <returns>A base64 encoded PDF.</returns>
        Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string hdid, bool isCovid19);

        /// <summary>
        /// Returns a laboratory summary which contains a list of lab orders for the authenticated user.
        /// It has a collection of one or more Lab Results depending on the tests ordered.
        /// </summary>
        /// <param name="hdid">The requested hdid.</param>
        /// <returns>Returns laboratory summary of lab orders.</returns>
        Task<RequestResult<LaboratorySummary>> GetLaboratorySummary(string hdid);

        /// <summary>
        /// Post the rapid test for the given patient info.
        /// </summary>
        /// <param name="hdid">The requested HDID which owns the rapid test request.</param>
        /// <param name="rapidTestRequest">The rapid test request model.</param>
        /// <returns>Returns the Rapid Test response.</returns>
        Task<RequestResult<AuthenticatedRapidTestResponse>> CreateRapidTestAsync(string hdid, AuthenticatedRapidTestRequest rapidTestRequest);

        /// <summary>
        /// Gets a COVID-19 test response for the given patient info.
        /// </summary>
        /// <param name="phn">The patient's Personal Health Number.</param>
        /// <param name="dateOfBirthString">The patient's date of birth in yyyy-MM-dd format.</param>
        /// <param name="collectionDateString">The date the test was collected in yyyy-MM-dd format.</param>
        /// <returns>Returns the COVID-19 test response.</returns>
        Task<RequestResult<PublicCovidTestResponse>> GetPublicCovidTestsAsync(string phn, string dateOfBirthString, string collectionDateString);
    }
}
