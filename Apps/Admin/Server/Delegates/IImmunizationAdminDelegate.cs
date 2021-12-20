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
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Models.CovidSupport;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Provides access to Administrative Immunization data.
    /// </summary>
    public interface IImmunizationAdminDelegate
    {
        /// <summary>
        /// Gets the vaccine details for the provided patient, retrying multiple times if there is a refresh in progress.
        /// The patient must have the PHN and DOB provided.
        /// </summary>
        /// <param name="patient">The patient to query for vaccine details.</param>
        /// <param name="refresh">Whether the call should force cached data to be refreshed.</param>
        /// <returns>The wrapped vaccine details.</returns>
        Task<RequestResult<VaccineDetails>> GetVaccineDetailsWithRetries(PatientModel patient, bool refresh);
    }
}
