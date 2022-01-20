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
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// Interface to retrieve Medication Requests.
    /// </summary>
    public interface IMedicationRequestDelegate
    {
        /// <summary>
        /// Returns a set of MedicationRequests for the given phn.
        /// </summary>
        /// <param name="phn">The PHN of the user querying.</param>
        /// <returns>The MedicationRequest result.</returns>
        Task<RequestResult<IList<MedicationRequest>>> GetMedicationRequestsAsync(string phn);
    }
}
