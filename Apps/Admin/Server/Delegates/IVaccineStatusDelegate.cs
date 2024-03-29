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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Provides access to vaccine status information.
    /// </summary>
    public interface IVaccineStatusDelegate
    {
        /// <summary>
        /// Returns the vaccine status for the given patient, retrying multiple times if there is a refresh in progress.
        /// </summary>
        /// <param name="phn">The PHN identifying the patient.</param>
        /// <param name="dateOfBirth">The date of birth of the patient.</param>
        /// <param name="accessToken">The connection access token.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The vaccine status result for the given patient.</returns>
        Task<PhsaResult<VaccineStatusResult>> GetVaccineStatusWithRetriesAsync(string phn, DateTime dateOfBirth, string accessToken, CancellationToken ct = default);
    }
}
