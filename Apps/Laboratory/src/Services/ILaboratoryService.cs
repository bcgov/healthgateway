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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Laboratory.Models;

    /// <summary>
    /// The laboratory data service.
    /// </summary>
    public interface ILaboratoryService
    {
        /// <summary>
        /// Returns a List of lab orders for the authenticated user.
        /// It has a collection of one or more Lab Results depending on the tests ordered.
        /// </summary>
        /// <param name="bearerToken">The security token representing the authenticated user.</param>
        /// <param name="hdid">The requested hdid.</param>
        /// <param name="pageIndex">The page index to return.</param>
        /// <returns>The list of Lab Reports available for the user identified by the bearerToken.</returns>
        Task<RequestResult<IEnumerable<LaboratoryModel>>> GetLaboratoryOrders(string bearerToken, string hdid, int pageIndex = 0);

        /// <summary>
        /// Gets the Lab report for the supplied id belonging to the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the lab report to get.</param>
        /// <param name="hdid">The requested HDID which owns the reportId.</param>
        /// <param name="bearerToken">The security token representing the authenticated user.</param>
        /// <returns>A base64 encoded PDF.</returns>
        Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string hdid, string bearerToken);

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
