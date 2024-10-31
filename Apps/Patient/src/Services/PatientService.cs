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
namespace HealthGateway.Patient.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Patient.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The patient service.
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly ILogger<PatientService> logger;
        private readonly IPatientMappingService mappingService;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientService"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        public PatientService(ILogger<PatientService> logger, IPatientRepository patientRepository, IPatientMappingService mappingService)
        {
            this.logger = logger;
            this.patientRepository = patientRepository;
            this.mappingService = mappingService;
        }

        /// <inheritdoc/>
        public async Task<PatientDetails> GetPatientAsync(
            string identifier,
            PatientIdentifierType identifierType = PatientIdentifierType.Hdid,
            bool disableIdValidation = false,
            CancellationToken ct = default)
        {
            PatientDetailsQuery query = identifierType == PatientIdentifierType.Hdid
                ? new PatientDetailsQuery(Hdid: identifier, Source: PatientDetailSource.All, UseCache: true)
                : new PatientDetailsQuery(identifier, Source: PatientDetailSource.All, UseCache: true);

            PatientModel patientDetails = (await this.patientRepository.QueryAsync(query, ct)).Item;

            if (patientDetails.IsDeceased == true)
            {
                this.logger.LogWarning("Patient is deceased");
                throw new InvalidDataException(ErrorMessages.ClientRegistryReturnedDeceasedPerson);
            }

            if (patientDetails.CommonName == null && patientDetails.LegalName == null)
            {
                throw new InvalidDataException(ErrorMessages.InvalidServicesCard);
            }

            if (string.IsNullOrEmpty(patientDetails.Hdid) && string.IsNullOrEmpty(patientDetails.Phn) && !disableIdValidation)
            {
                throw new InvalidDataException(ErrorMessages.InvalidServicesCard);
            }

            return this.mappingService.MapToPatientDetails(patientDetails);
        }
    }
}
