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
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class PatientDetailsService : IPatientDetailsService
    {
        private readonly IMapper autoMapper;
        private readonly ILogger logger;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDetailsService"/> class.
        /// </summary>
        /// <param name="autoMapper">The inject automapper provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="patientRepository">The injected patient repository provider.</param>
        public PatientDetailsService(
            IMapper autoMapper,
            ILogger<PatientDetailsService> logger,
            IPatientRepository patientRepository)
        {
            this.autoMapper = autoMapper;
            this.logger = logger;
            this.patientRepository = patientRepository;
        }

        private static ActivitySource Source { get; } = new(nameof(PatientDetailsService));

        /// <inheritdoc/>
        public async Task<PatientDetails> GetPatientAsync(
            string identifier,
            PatientIdentifierType identifierType = PatientIdentifierType.Hdid,
            bool disableIdValidation = false,
            CancellationToken ct = default)
        {
            using Activity? activity = Source.StartActivity();

            PatientDetailsQuery query = identifierType == PatientIdentifierType.Hdid
                ? new PatientDetailsQuery(Hdid: identifier, Source: PatientDetailSource.Empi, UseCache: true)
                : new PatientDetailsQuery(identifier, Source: PatientDetailSource.Empi, UseCache: true);

            this.logger.LogDebug("Starting GetPatient for identifier type: {IdentifierType} and patient data source: {Source}", identifierType, query.Source);

            PatientModel? patientDetails = (await this.patientRepository.Query(query, ct).ConfigureAwait(true)).Items.SingleOrDefault();

            if (patientDetails == null)
            {
                // BCHCIM.GD.2.0018 Not found
                this.logger.LogWarning("Client Registry did not find any records. Returned message code: {ResponseCode}", "Not found");
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryRecordsNotFound, HttpStatusCode.NotFound, nameof(PatientDetailsService)));
            }

            if (patientDetails.LegalName == null && patientDetails.CommonName == null)
            {
                this.logger.LogWarning("Client Registry is unable to determine patient name due to missing legal name. Action Type: {ActionType}", ActionType.InvalidName.Value);
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.InvalidServicesCard, HttpStatusCode.NotFound, nameof(PatientDetailsService)));
            }

            if (string.IsNullOrEmpty(patientDetails.Hdid) && string.IsNullOrEmpty(patientDetails.Phn) && !disableIdValidation)
            {
                this.logger.LogWarning("Client Registry was unable to retrieve identifiers. Action Type: {ActionType}", ActionType.NoHdId.Value);
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.InvalidServicesCard, HttpStatusCode.NotFound, nameof(PatientDetailsService)));
            }

            activity?.Stop();

            return this.autoMapper.Map<PatientDetails>(patientDetails);
        }
    }
}
