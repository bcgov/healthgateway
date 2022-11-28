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
namespace HealthGateway.Immunization.Delegates
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Interface that defines a delegate to retrieve vaccine status information.
    /// </summary>
    public interface IVaccineStatusDelegate
    {
        /// <summary>
        /// Retrieves the vaccine status for a given patient as an authenticated user.
        /// </summary>
        /// <param name="hdid">The HDID identifying the subject of the request.</param>
        /// <param name="includeFederalPvc">
        /// A value indicating if the federal proof of vaccination should be included in the
        /// response.
        /// </param>
        /// <param name="accessToken">The bearer token to authorize the call.</param>
        /// <returns>The vaccine status result for the given patient.</returns>
        Task<RequestResult<PhsaResult<VaccineStatusResult>>> GetVaccineStatus(string hdid, bool includeFederalPvc, string accessToken);

        /// <summary>
        /// Retrieves the vaccine status for the patient identified by the query.
        /// </summary>
        /// <param name="query">The vaccine status query.</param>
        /// <param name="accessToken">The bearer token to authorize the call.</param>
        /// <param name="clientIp">The IP of the client calling the service.</param>
        /// <returns>The vaccine status result for the given patient.</returns>
        Task<RequestResult<PhsaResult<VaccineStatusResult>>> GetVaccineStatusPublic(VaccineStatusQuery query, string accessToken, string clientIp);
    }
}
