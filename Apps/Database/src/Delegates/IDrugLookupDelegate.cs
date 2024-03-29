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
namespace HealthGateway.Database.Delegates
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Delegate that retrieves drug information.
    /// </summary>
    public interface IDrugLookupDelegate
    {
        /// <summary>
        /// Retrieves DrugProducts that match drug identifier numbers (DINs).
        /// </summary>
        /// <param name="drugIdentifiers">List of drug identifiers.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of DrugProducts.</returns>
        Task<IList<DrugProduct>> GetDrugProductsByDinAsync(IList<string> drugIdentifiers, CancellationToken ct = default);

        /// <summary>
        /// Retrieves PharmaCareDrugs that match drug identifier numbers (DINs) or provincial identifier numbers (PINs).
        /// </summary>
        /// <param name="drugIdentifiers">List of drug identifiers.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of PharmaCareDrugs.</returns>
        Task<IList<PharmaCareDrug>> GetPharmaCareDrugsByDinAsync(IList<string> drugIdentifiers, CancellationToken ct = default);
    }
}
