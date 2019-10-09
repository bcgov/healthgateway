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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// The TEST Medication data service.
    /// IMPORTANT TODO: Service only to be used for development.
    /// </summary>
    public class TestMedicationService : IMedicationService
    {
        /// <inheritdoc/>
        public async Task<HNMessage<List<MedicationStatement>>> GetMedicationsAsync(string phn, string userId, string ipAddress)
        {
            List<MedicationStatement> returnList = new List<MedicationStatement>();

            for(int i = 0; i < 10; i++)
            {
                returnList.Add(
                    new MedicationStatement()
                    {
                        BrandName = "A brand name " + i,
                        DIN = "ABC" + i,
                        GenericName = "A generic name " + i,
                        DispensedDate = DateTime.Today.AddDays(i % 5) ,
                    }
                );
            }

            return new HNMessage<List<MedicationStatement>>(returnList);
        }
    }
}