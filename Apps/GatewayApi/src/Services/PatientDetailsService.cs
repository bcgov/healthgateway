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
namespace HealthGateway.GatewayApi.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.GatewayApi.Models;

    /// <inheritdoc/>
    public class PatientDetailsService : IPatientDetailsService
    {
        private readonly IGatewayApiMappingService mappingService;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDetailsService"/> class.
        /// </summary>
        /// <param name="mappingService">The injected mapping service.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        public PatientDetailsService(IGatewayApiMappingService mappingService, IPatientRepository patientRepository)
        {
            this.mappingService = mappingService;
            this.patientRepository = patientRepository;
        }

        /// <inheritdoc/>
        public async Task<PatientDetails> GetPatientAsync(
            string identifier,
            PatientIdentifierType identifierType = PatientIdentifierType.Hdid,
            bool disableIdValidation = false,
            CancellationToken ct = default)
        {
            PatientDetailsQuery query = identifierType == PatientIdentifierType.Hdid
                ? new PatientDetailsQuery(Hdid: identifier, Source: PatientDetailSource.Empi, UseCache: true)
                : new PatientDetailsQuery(identifier, Source: PatientDetailSource.Empi, UseCache: true);

            PatientModel patientModel = (await this.patientRepository.QueryAsync(query, ct)).Item;

            if (patientModel.LegalName == null && patientModel.CommonName == null)
            {
                throw new InvalidDataException(ErrorMessages.InvalidServicesCard);
            }

            if (string.IsNullOrEmpty(patientModel.Hdid) && string.IsNullOrEmpty(patientModel.Phn) && !disableIdValidation)
            {
                throw new InvalidDataException(ErrorMessages.InvalidServicesCard);
            }

            return this.mappingService.MapToPatientDetails(patientModel);
        }
    }
}
