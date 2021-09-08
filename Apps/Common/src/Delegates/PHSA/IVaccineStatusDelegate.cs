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
namespace HealthGateway.Common.Delegates.PHSA
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Interface that defines a delegate to retrieve vaccine status information.
    /// </summary>
    public interface IVaccineStatusDelegate
    {
        /// <summary>
        /// Returns the vaccine status for the given patient.
        /// </summary>
        /// <param name="query">The vaccine status query.</param>
        /// <param name="accessToken">The connection access token.</param>
        /// <param name="isPublicEndpoint">Indicates whether it should use the public endpoint.</param>
        /// <returns>The vaccine status result for the given patient.</returns>
        Task<RequestResult<PHSAResult<VaccineStatusResult>>> GetVaccineStatus(VaccineStatusQuery query, string accessToken, bool isPublicEndpoint);

        /// <summary>
        /// Returns the vaccine status for the given patient, retrying multiple times if there is a refresh in progress.
        /// </summary>
        /// <param name="query">The vaccine status query.</param>
        /// <param name="accessToken">The connection access token.</param>
        /// <param name="isPublicEndpoint">Indicates whether it should use the public endpoint.</param>
        /// <returns>The vaccine status result for the given patient.</returns>
        Task<RequestResult<PHSAResult<VaccineStatusResult>>> GetVaccineStatusWithRetries(VaccineStatusQuery query, string accessToken, bool isPublicEndpoint);

        /// <summary>
        /// Returns the record card for the given patient.
        /// </summary>
        /// <param name="query">The record card query.</param>
        /// <param name="accessToken">The connection access token.</param>
        /// <returns>The record card result for the given patient.</returns>
        Task<RequestResult<PHSAResult<RecordCard>>> GetRecordCard(RecordCardQuery query, string accessToken);

        /// <summary>
        /// Returns the record card for the given patient, retrying multiple times if there is a refresh in progress.
        /// </summary>
        /// <param name="query">The record card query.</param>
        /// <param name="accessToken">The connection access token.</param>
        /// <returns>The record card result for the given patient.</returns>
        Task<RequestResult<PHSAResult<RecordCard>>> GetRecordCardWithRetries(RecordCardQuery query, string accessToken);
    }
}
