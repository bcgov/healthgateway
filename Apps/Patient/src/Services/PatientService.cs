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
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Patient.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Patient data service.
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

        private static ActivitySource Source { get; } = new(nameof(PatientService));

        /// <inheritdoc/>
        public async Task<PatientDetails> GetPatientAsync(
            string identifier,
            PatientIdentifierType identifierType = PatientIdentifierType.Hdid,
            bool disableIdValidation = false,
            CancellationToken ct = default)
        {
            using Activity? activity = Source.StartActivity();

            PatientDetailsQuery query = identifierType == PatientIdentifierType.Hdid
                ? new PatientDetailsQuery(Hdid: identifier, Source: PatientDetailSource.All, UseCache: true)
                : new PatientDetailsQuery(identifier, Source: PatientDetailSource.All, UseCache: true);

            this.logger.LogDebug("Starting GetPatient for identifier type: {IdentifierType} and patient data source: {Source}", identifierType, query.Source);

            PatientModel patientDetails = (await this.patientRepository.QueryAsync(query, ct)).Item;

            if (patientDetails.IsDeceased == true)
            {
                this.logger.LogWarning("Client Registry returned a person with the deceased indicator set to true. No PHN was populated. {ActionType}", ActionType.Deceased.Value);
                throw new InvalidDataException(ErrorMessages.ClientRegistryReturnedDeceasedPerson);
            }

            if (patientDetails.CommonName == null)
            {
                this.logger.LogWarning("Client Registry returned a person without a Documented Name");
                if (patientDetails.LegalName == null)
                {
                    this.logger.LogWarning("Client Registry is unable to determine patient name due to missing legal name. Action Type: {ActionType}", ActionType.InvalidName.Value);
                    throw new InvalidDataException(ErrorMessages.InvalidServicesCard);
                }
            }

            if (string.IsNullOrEmpty(patientDetails.Hdid) && string.IsNullOrEmpty(patientDetails.Phn) && !disableIdValidation)
            {
                this.logger.LogWarning("Client Registry was unable to retrieve identifiers. Action Type: {ActionType}", ActionType.NoHdId.Value);
                throw new InvalidDataException(ErrorMessages.InvalidServicesCard);
            }

            activity?.Stop();

            return this.mappingService.MapToPatientDetails(patientDetails);
        }
    }
}
