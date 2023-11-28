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
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentValidation.Results;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models.Cacheable;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class DelegationService : IDelegationService
    {
        private readonly IConfiguration configuration;
        private readonly IMapper autoMapper;
        private readonly ILogger<DelegationService> logger;
        private readonly IDelegationDelegate delegationDelegate;
        private readonly IPatientDetailsService patientDetailsService;
        private readonly IHashDelegate hashDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="autoMapper">The injected auto mapper provider.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="delegationDelegate">The injected delegation delegate.</param>
        /// <param name="hashDelegate">The injected hash delegate.</param>
        /// <param name="patientDetailsService">The injected patient details service.</param>
        public DelegationService(
            IConfiguration configuration,
            IMapper autoMapper,
            ILogger<DelegationService> logger,
            IDelegationDelegate delegationDelegate,
            IHashDelegate hashDelegate,
            IPatientDetailsService patientDetailsService)
        {
            this.configuration = configuration;
            this.autoMapper = autoMapper;
            this.logger = logger;
            this.delegationDelegate = delegationDelegate;
            this.hashDelegate = hashDelegate;
            this.patientDetailsService = patientDetailsService;
        }

        /// <inheritdoc/>
        public async Task<string> CreateDelegationAsync(string hdid, CreateDelegationRequest request, CancellationToken ct = default)
        {
            TimeZoneInfo localTimezone = DateFormatter.GetLocalTimeZone(this.configuration);
            DateOnly referenceDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimezone));

            ValidationResult validationResult = await new CreateDelegationRequestValidator(referenceDate).ValidateAsync(request, ct);

            if (!validationResult.IsValid)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.InvalidDelegateInvitationRequest, HttpStatusCode.BadRequest, nameof(DelegationService)));
            }

            PatientDetails patient = await this.patientDetailsService.GetPatientAsync(hdid, PatientIdentifierType.Hdid, false, ct);
            string resourceOwnerIdentifier = $"{patient.PreferredName.GivenName} {patient.PreferredName.Surname[0]}";
            this.logger.LogDebug("Resource owner identifier for delegate invitation: {ResourceOwnerIdentifier}", resourceOwnerIdentifier);

            Delegation delegation = this.autoMapper.Map<Delegation>(request);
            string sharingCode = VerificationCodeUtility.Generate();
            IHash hash = this.hashDelegate.Hash(sharingCode);
            this.logger.LogDebug("Sharing code: {SharingCode} \n Hash: {Hash} \n Salt: {Salt} ", sharingCode, hash.Hash!, hash.Salt!);

            delegation.SharingCodeHash = hash.Hash!;
            delegation.SharingCodeSalt = hash.Salt!;
            delegation.SharingCodeIterations = hash.Iterations;
            delegation.SharingCodeHashFunction = hash.PseudoRandomFunction;
            delegation.ResourceOwnerHdid = hdid;
            delegation.ResourceOwnerIdentifier = resourceOwnerIdentifier;

            await this.delegationDelegate.UpdateDelegationAsync(delegation);
            return sharingCode;
        }
    }
}
