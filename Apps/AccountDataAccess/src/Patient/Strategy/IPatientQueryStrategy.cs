// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.AccountDataAccess.Patient.Strategy
{
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents the patient detail source to determine what to query.
    /// </summary>
    public enum PatientStrategy
    {
        /// <summary>
        /// Specifies that the EMPI data source is to be queried against for Hdid.
        /// </summary>
        HdidEmpi,

        /// <summary>
        /// Specifies that both cache and the EMPI (if necessary) data source are to be queried against for Hdid.
        /// </summary>
        HdidEmpiCache,

        /// <summary>
        /// Specifies that the PHSA data source is to be queried against for Hdid.
        /// </summary>
        HdidPhsa,

        /// <summary>
        /// Specifies that both the cache and PHSA (if necessary) data source are to be queried against for Hdid.
        /// </summary>
        HdidPhsaCache,

        /// <summary>
        /// Specifies that both EMPI and PHSA (if necessary) data sources are to be queried against for Hdid.
        /// </summary>
        HdidAll,

        /// <summary>
        /// Specifies that the cache, EMPI (if necessary) and PHSA (if necessary) data sources are to be queried against for Hdid.
        /// </summary>
        HdidAllCache,

        /// <summary>
        /// Specifies that the EMPI data source is to be queried against for Phn.
        /// </summary>
        PhnEmpi,

        /// <summary>
        /// Specifies that both cache and the EMPI (if necessary) data source are to be queried against for Phn.
        /// </summary>
        PhnEmpiCache,
    }

    /// <summary>
    /// The Strategy interface declares operations common to all supported
    /// versions of the get patient async algorithm.
    /// The Context uses this interface to call the algorithm defined by Concrete
    /// Strategies.
    /// </summary>
    internal interface IPatientQueryStrategy
    {
        /// <summary>
        /// Returns patient from the database.
        /// </summary>
        /// <param name="request">The patient request parameters to use.</param>
        /// <returns>The patient model.</returns>
        Task<PatientModel?> GetPatientAsync(PatientRequest request);
    }

    /// <summary>
    /// The patient request.
    /// </summary>
    internal record PatientRequest(
        string Identifier,
        IClientRegistriesDelegate ClientRegistriesDelegate,
        IPatientIdentityApi PatientIdentityApi,
        ILogger<PatientRepository> Logger,
        IMapper Mapper,
        PatientModel? CachedPatient = null,
        bool DisabledValidation = false);
}
