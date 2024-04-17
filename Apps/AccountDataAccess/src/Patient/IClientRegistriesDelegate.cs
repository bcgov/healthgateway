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
namespace HealthGateway.AccountDataAccess.Patient
{
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    internal interface IClientRegistriesDelegate
    {
        /// <summary>
        /// Gets the patient record.
        /// </summary>
        /// <param name="type">The oid type value.</param>
        /// <param name="identifier">The associated oid type's identifier to retrieve the patient demographics information.</param>
        /// <param name="disableIdValidation">Disables the validation on HDID/PHN when true.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <exception cref="NotFoundException">No patient could be found matching the provided criteria.</exception>
        /// <exception cref="ValidationException">The provided PHN identifier is invalid.</exception>
        /// <returns>The patient model wrapped in an api result object.</returns>
        Task<PatientModel> GetDemographicsAsync(OidType type, string identifier, bool disableIdValidation = false, CancellationToken ct = default);
    }
}
