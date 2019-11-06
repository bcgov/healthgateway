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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// Interface that defines a client to connect to hnclient.
    /// </summary>
    public interface IHNClientDelegate
    {
        /// <summary>
        /// Retrieves a list of MedicationStatements that for the given phn.
        /// </summary>
        /// <param name="phn">The patient phn.</param>
        /// <param name="protectiveWord">The clients protective word.</param>
        /// <param name="userId">The user id of the request.</param>
        /// <param name="ipAddress">The ip address of the request.</param>
        /// <returns>A List of MedicationStatement models.</returns>
        Task<HNMessage<List<MedicationStatement>>> GetMedicationStatementsAsync(string phn, string protectiveWord, string userId, string ipAddress);

        /// <summary>
        /// Retrieves a pharmacy record that match the given pharmacy id.
        /// </summary>
        /// <param name="pharmacyId">The pharmacy identifier.</param>
        /// <param name="userId">The user id of the request.</param>
        /// <param name="ipAddress">The ip address of the request.</param>
        /// <returns>The pharmacy that matches the given id.</returns>
        Task<HNMessage<Pharmacy>> GetPharmacyAsync(string pharmacyId, string userId, string ipAddress);
    }
}