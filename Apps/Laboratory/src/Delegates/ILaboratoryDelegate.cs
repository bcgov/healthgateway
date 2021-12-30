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
namespace HealthGateway.Laboratory.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Models;

    /// <summary>
    /// Interface that defines a delegate to retrieve laboratory information.
    /// </summary>
    public interface ILaboratoryDelegate
    {
        /// <summary>
        /// Returns a List of Lab Orders for the authenticated user.
        /// It has a collection of one or more Lab Results depending on the tests ordered.
        /// </summary>
        /// <param name="bearerToken">The security token representing the authenticated user.</param>
        /// <param name="hdid">The requested hdid.</param>
        /// <param name="pageIndex">The page index to return.</param>
        /// <returns>The list of Lab Reports available for the user identified by the bearerToken.</returns>
        Task<RequestResult<IEnumerable<LaboratoryOrder>>> GetLaboratoryOrders(string bearerToken, string hdid, int pageIndex = 0);

        /// <summary>
        /// Gets the Lab report in binary format for the supplied id belonging to the authenticated user.
        /// </summary>
        /// <param name="id">The ID of the lab report to get.</param>
        /// <param name="hdid">The requested HDID which owns the reportId.</param>
        /// <param name="bearerToken">The security token representing the authenticated user.</param>
        /// <returns>A base64 encoded PDF.</returns>
        Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string hdid, string bearerToken);

        /// <summary>
        /// Returns the public COVID-19 test results for the given patient.
        /// </summary>
        /// <param name="accessToken">The connection access token.</param>
        /// <param name="phn">The patient's Personal Health Number.</param>
        /// <param name="dateOfBirth">The patient's date of birth.</param>
        /// <param name="collectionDate">The date the test was collected.</param>
        /// <returns>The COVID-19 test results result for the given patient.</returns>
        Task<RequestResult<PHSAResult<IEnumerable<CovidTestResult>>>> GetPublicTestResults(string accessToken, string phn, DateOnly dateOfBirth, DateOnly collectionDate);

        /// <summary>
        /// Post the rapid test for the give patient info.
        /// </summary>
        /// <param name="hdid">The requested HDID which owns the reportId.</param>
        /// <param name="bearerToken">The security token representing the authenticated user.</param>
        /// <param name="rapidTestRequest">The rapid test request model.</param>
        /// <returns>Returns the PHSA response for the Rapid Test.</returns>
        Task<RequestResult<IEnumerable<RapidTestResult>>> CreateRapidTestAsync(string hdid, string bearerToken, AuthenticaeRapidTestRequest rapidTestRequest);
    }
}
