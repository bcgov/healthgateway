
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
namespace HealthGateway.Medication.Delegates
{
    using System.Linq;
    using System.Collections.Generic;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Database;
    using HealthGateway.Common.Database;
    using HealthGateway.Common.Database.Models;

    /// <summary>
    /// Implementation of IDrugLookupDelegate that uses a DB connection for data management
    /// </summary>
    public class EntityDrugLookupDelegate : IDrugLookupDelegate
    {
        private IDBContextFactory contextFactory;

        /// <summary>
        /// Constructor that requires a database context factory
        /// </summary>
        public EntityDrugLookupDelegate(IDBContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        /// <inheritdoc/>
        public List<Medication> FindMedicationsByDIN(List<string> drugIdentifiers)
        {
            using (MedicationDBContext ctx = (MedicationDBContext)this.contextFactory.CreateContext())
            {
                List<DrugProduct> drugProducts = ctx.DrugProduct.Where(dp => drugIdentifiers.Contains(dp.DrugIdentificationNumber)).ToList();
                return SimpleModelMapper.ToMedicationList(drugProducts);
            }
        }
    }
}