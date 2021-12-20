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
namespace HealthGateway.Immunization.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// The Vaccine Status data service.
    /// </summary>
    public interface IVaccineStatusService
    {
        /// <summary>
        /// Gets the COVID-19 vaccine status for the given patient info.
        /// </summary>
        /// <param name="phn">The patient's Personal Health Number.</param>
        /// <param name="dateOfBirth">The patient's date of birth in yyyy-MM-dd format.</param>
        /// <param name="dateOfVaccine">The date of one of the patient's vaccine doses in yyyy-MM-dd format.</param>
        /// <returns>Returns the vaccine status.</returns>
        Task<RequestResult<VaccineStatus>> GetPublicVaccineStatus(string phn, string dateOfBirth, string dateOfVaccine);

        /// <summary>
        /// Gets the COVID-19 vaccine status for the given HDID.
        /// </summary>
        /// <param name="hdid">The identifier to fetch the vaccine status for.</param>
        /// <returns>Returns the vaccine status.</returns>
        Task<RequestResult<VaccineStatus>> GetAuthenticatedVaccineStatus(string hdid);

        /// <summary>
        /// Gets the COVID-19 Vaccine Proof PDF for the given patient info.
        /// </summary>
        /// <param name="phn">The patient's Personal Health Number.</param>
        /// <param name="dateOfBirth">The patient's date of birth in yyyy-MM-dd format.</param>
        /// <param name="dateOfVaccine">The date of one of the patient's vaccine doses in yyyy-MM-dd format.</param>
        /// <returns>The base64-encoded PDF and QR Code in a RequestResult.</returns>
        Task<RequestResult<VaccineProofDocument>> GetPublicVaccineProof(string phn, string dateOfBirth, string dateOfVaccine);

        /// <summary>
        /// Gets the COVID-19 Vaccine Proof PDF for the given HDID.
        /// </summary>
        /// <param name="hdid">The identifier to fetch the PDF for.</param>
        /// <returns>The base64-encoded PDF and QR Code in a RequestResult.</returns>
        Task<RequestResult<VaccineProofDocument>> GetAuthenticatedVaccineProof(string hdid);
    }
}
