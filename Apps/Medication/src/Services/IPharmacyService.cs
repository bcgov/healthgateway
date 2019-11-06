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
    using System.Threading.Tasks;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// Interface that defined a PharmacyService.
    /// </summary>
    public interface IPharmacyService
    {
        /// <summary>
        /// Gets the pharmacy record.
        /// </summary>
        /// <param name="pharmacyId">The pharmacy identifier.</param>
        /// <returns>The Prescriptions model.</returns>
        Task<HNMessage<Pharmacy>> GetPharmacyAsync(string pharmacyId);
    }
}