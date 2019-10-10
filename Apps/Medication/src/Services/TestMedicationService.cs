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
        public async Task<List<MedicationStatement>> GetMedicationsAsync(string phn, string userId, string ipAddress)
        {
            List<MedicationStatement> returnList = new List<MedicationStatement>();

            for (int i = 0; i < 10; i++)
            {
                returnList.Add(
                    new MedicationStatement()
                    {
                        DispensedDate = DateTime.Today.AddDays(i % 5),
                        PractitionerSurname = "Doctor Gateway",
                        Pharmacy = new Pharmacy()
                        {
                            Name = "Pharmacorp" + i % 3,
                            AddressLine1 = "Good street 1234",
                            AddressLine2 = "Unit5",
                            City = "Victoria",
                            Province = "BC",
                            PhoneNumber = "250-555-1234"
                        },
                        Medication = new Medication()
                        {
                            BrandName = "Brandicon A" + i,
                            DIN = "ABC" + i,
                            GenericName = "A generic name " + i,
                            Quantity = 30,
                            Dosage = 4.35f,
                        }
                    }
                );
            }

            return returnList;
        }
    }
}