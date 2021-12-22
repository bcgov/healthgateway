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
namespace HealthGateway.Common.Delegates
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    public interface IClientRegistriesDelegate
    {
        /// <summary>
        /// Gets the patient record.
        /// </summary>
        /// <param name="hdid">The hdid to retrieve the patient demographics in.</param>
        /// <param name="disableIdValidation">Disables the validation on HDID/PHN when true.</param>
        /// <returns>The patient information.</returns>
        Task<RequestResult<PatientModel>> GetDemographicsByHDIDAsync(string hdid, bool disableIdValidation = false);

        /// <summary>
        /// Gets the patient record.
        /// </summary>
        /// <param name="phn">The phn to retrive the patient demographics information.</param>
        /// <param name="disableIdValidation">Disables the validation on HDID/PHN when true.</param>
        /// <returns>The patient information.</returns>
        Task<RequestResult<PatientModel>> GetDemographicsByPHNAsync(string phn, bool disableIdValidation = false);
    }
}
