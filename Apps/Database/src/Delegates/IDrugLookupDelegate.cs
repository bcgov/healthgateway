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
    using HealthGateway.Database.Models;

    /// <summary>
    /// Delegate that retrieves DrugProducts based on the drug identifier.
    /// </summary>
    public interface IDrugLookupDelegate
    {
        /// <summary>
        /// Retrieves DrugProduct(s) that match a the drug identifier numbers (DINs).
        /// </summary>
        /// <param name="drugIdentifiers">List of drug identifiers.</param>
        /// <returns>A request results with the outcome of the lookup.</returns>
        IList<DrugProduct> GetDrugProductsByDin(IList<string> drugIdentifiers);

        /// <summary>
        /// Retrieves PharmaCareDrug(s) that match a the drug identifier numbers (DINs) or provincial identifier numbers (PINs).
        /// </summary>
        /// <param name="drugIdentifiers">List of drug identifiers.</param>
        /// <returns>A request results with the outcome of the lookup.</returns>
        IList<PharmaCareDrug> GetPharmaCareDrugsByDin(IList<string> drugIdentifiers);

        /// <summary>
        /// Retrieves the brand names that match the drug identifier numbers (DINs) or provincial identifier numbers (PINs).
        /// </summary>
        /// <param name="drugIdentifiers">List of drug identifiers.</param>
        /// <returns>A dictionary with the drug identifier as key and the result brand name as value.</returns>
        Dictionary<string, string> GetDrugsBrandNameByDin(IList<string> drugIdentifiers);
    }
}
