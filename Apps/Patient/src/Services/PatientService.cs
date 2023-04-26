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
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Patient.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly ILogger<PatientService> logger;
        private readonly IMapper mapper;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientService"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="mapper">The mapper.</param>
        public PatientService(ILogger<PatientService> logger, IPatientRepository patientRepository, IMapper mapper)
        {
            this.logger = logger;
            this.patientRepository = patientRepository;
            this.mapper = mapper;
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

            this.logger.LogDebug("Starting GetPatient for identifier type: {IdentifierType}", identifierType);

            PatientDetailsQuery query = identifierType == PatientIdentifierType.Hdid
                ? new PatientDetailsQuery(Hdid: identifier)
                : new PatientDetailsQuery(Phn: identifier);

            PatientModel? patientDetails = (await this.patientRepository.Query(query, ct).ConfigureAwait(true)).Items.SingleOrDefault();

            if (patientDetails == null)
            {
                // BCHCIM.GD.2.0018 Not found
                this.logger.LogWarning("Client Registry did not find any records. Returned message code: {ResponseCode}", "Not found");
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryRecordsNotFound, HttpStatusCode.NotFound, nameof(PatientService)));
            }

            if (patientDetails.IsDeceased == true)
            {
                this.logger.LogWarning("Client Registry returned a person with the deceased indicator set to true. No PHN was populated. {ActionType}", ActionType.Deceased.Value);
                throw new ProblemDetailsException(
                    ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryReturnedDeceasedPerson, HttpStatusCode.NotFound, nameof(PatientService)));
            }

            if (patientDetails.CommonName == null)
            {
                this.logger.LogWarning("Client Registry returned a person without a Documented Name.");
                if (patientDetails.LegalName == null)
                {
                    this.logger.LogWarning("Client Registry is unable to determine patient name due to missing legal name. Action Type: {ActionType}", ActionType.InvalidName.Value);
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.InvalidServicesCard, HttpStatusCode.NotFound, nameof(PatientService)));
                }
            }

            if (string.IsNullOrEmpty(patientDetails.Hdid) && string.IsNullOrEmpty(patientDetails.Phn) && !disableIdValidation)
            {
                this.logger.LogWarning("Client Registry was unable to retrieve identifiers. Action Type: {ActionType}", ActionType.NoHdId.Value);
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.InvalidServicesCard, HttpStatusCode.NotFound, nameof(PatientService)));
            }

            activity?.Stop();

            return this.mapper.Map<PatientDetails>(patientDetails);
        }
    }
}
