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
namespace HealthGateway.Common.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// Gets the patient PHN.
        /// </summary>
        /// <param name="hdid">The patient HDID.</param>
        /// <returns>The patient PHN.</returns>
        Task<RequestResult<string>> GetPatientPhn(string hdid);

        /// <summary>
        /// Gets the patient HDID by PHN.
        /// </summary>
        /// <param name="phn">The patient PHN.</param>
        /// <returns>The patient HDID.</returns>
        Task<string> GetPatientHdid(string phn);

        /// <summary>
        /// Gets the patient record.
        /// </summary>
        /// <param name="identifier">The patient identifier.</param>
        /// <param name="identifierType">The type of identifier being passed in.</param>
        /// <param name="disableIdValidation">Disables the validation on HDID/PHN when true.</param>
        /// <returns>The patient model.</returns>
        Task<RequestResult<PatientModel>> GetPatient(string identifier, PatientIdentifierType identifierType = PatientIdentifierType.Hdid, bool disableIdValidation = false);
    }
}
