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
    using HealthGateway.Common.Authentication.Models;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public interface IPharmacyService
    {
        /// <summary>
        /// Gets the pharmacy record.
        /// </summary>
        /// <param name="jwtModel">The JWTModel that contains the authentication context.</param>
        /// <param name="pharmacyId">The pharmacy identifier.</param>
        /// <param name="userId">The user id of the request.</param>
        /// <param name="ipAddress">The ip address of the request.</param>
        /// <returns>The Prescriptions model.</returns>
        Task<HNMessage<Pharmacy>> GetPharmacyAsync(JWTModel jwtModel, string pharmacyId, string userId, string ipAddress);
    }
}