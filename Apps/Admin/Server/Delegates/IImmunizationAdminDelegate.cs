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
namespace HealthGateway.Admin.Server.Delegates
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models.CovidSupport;

    /// <summary>
    /// Provides access to Administrative Immunization data.
    /// </summary>
    public interface IImmunizationAdminDelegate
    {
        /// <summary>
        /// Gets the vaccine details for the provided patient, retrying multiple times if there is a refresh in progress.
        /// The patient must have the PHN and DOB provided.
        /// </summary>
        /// <param name="phn">The phn to query for vaccine details.</param>
        /// <param name="accessToken">The connection access token.</param>
        /// <param name="refresh">Whether the call should force cached data to be refreshed.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The wrapped vaccine details.</returns>
        Task<VaccineDetails> GetVaccineDetailsWithRetriesAsync(string phn, string accessToken, bool refresh = false, CancellationToken ct = default);
    }
}
