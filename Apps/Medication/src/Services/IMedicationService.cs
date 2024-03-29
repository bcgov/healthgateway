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
namespace HealthGateway.Medication.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// The medication service.
    /// </summary>
    public interface IMedicationService
    {
        /// <summary>
        /// Gets medication information matching the requested drug identifiers.
        /// </summary>
        /// <param name="drugIdentifiers">The list of drug identifiers to retrieve.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A dictionary mapping drug identifiers to medication information.</returns>
        Task<IDictionary<string, MedicationInformation>> GetMedicationsAsync(IList<string> drugIdentifiers, CancellationToken ct = default);
    }
}
