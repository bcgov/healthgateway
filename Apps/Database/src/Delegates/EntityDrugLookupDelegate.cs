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
    using System.Linq;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Implementation of IDrugLookupDelegate that uses a DB connection for data management
    /// </summary>
    public class EntityDrugLookupDelegate : IDrugLookupDelegate
    {
        private readonly DrugDBContext dbContext;

        /// <summary>
        /// Constructor that requires a database context factory.
        /// </summary>
        /// <param name="contextFactory">The context factory to be used when accessing the databaase context.</param>
        public EntityDrugLookupDelegate(DrugDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public List<DrugProduct> FindDrugProductsByDIN(List<string> drugIdentifiers)
        {
            return this.dbContext.DrugProduct.Where(dp => drugIdentifiers.Contains(dp.DrugIdentificationNumber)).ToList();
        }
    }
}