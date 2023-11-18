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
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentValidation.Results;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models.Cacheable;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class DelegateService : IDelegateService
    {
        private readonly IMapper autoMapper;
        private readonly ILogger<DelegateService> logger;
        private readonly IDelegateInvitationDelegate delegateInvitationDelegate;
        private readonly IPatientDetailsService patientDetailsService;
        private readonly IHashDelegate hashDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateService"/> class.
        /// </summary>
        /// <param name="autoMapper">The injected auto mapper provider.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="delegateInvitationDelegate">The injected delegate invitation delegate.</param>
        /// <param name="hashDelegate">The injected hash delegate.</param>
        /// <param name="patientDetailsService">The injected patient details service.</param>
        public DelegateService(
            IMapper autoMapper,
            ILogger<DelegateService> logger,
            IDelegateInvitationDelegate delegateInvitationDelegate,
            IHashDelegate hashDelegate,
            IPatientDetailsService patientDetailsService)
        {
            this.autoMapper = autoMapper;
            this.logger = logger;
            this.delegateInvitationDelegate = delegateInvitationDelegate;
            this.hashDelegate = hashDelegate;
            this.patientDetailsService = patientDetailsService;
        }

        /// <inheritdoc/>
        public async Task<string> CreateDelegateInvitationAsync(string hdid, CreateDelegateInvitationRequest request, CancellationToken ct = default)
        {
            ValidationResult validationResult = await new CreateDelegateInvitationRequestValidator().ValidateAsync(request, ct);

            if (!validationResult.IsValid)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.InvalidDelegateInvitationRequest, HttpStatusCode.BadRequest, nameof(DelegateService)));
            }

            PatientDetails patient = await this.patientDetailsService.GetPatientAsync(hdid, PatientIdentifierType.Hdid, false, ct);
            string resourceOwnerIdentifier = $"{patient.PreferredName.GivenName} {patient.PreferredName.Surname[0]}";
            this.logger.LogDebug("Resource owner identifier for delegate invitation: {ResourceOwnerIdentifier}", resourceOwnerIdentifier);

            DelegateInvitation delegateInvitation = this.autoMapper.Map<DelegateInvitation>(request);
            string sharingCode = VerificationCodeUtility.Generate();
            IHash hash = this.hashDelegate.Hash(sharingCode);
            this.logger.LogDebug("Sharing code: {SharingCode} \n Hash: {Hash} \n Salt: {Salt} ", sharingCode, hash?.Hash!, hash?.Salt!);

            delegateInvitation.SharingCodeHash = hash.Hash!;
            delegateInvitation.SharingCodeSalt = hash.Salt!;
            delegateInvitation.SharingCodeIterations = hash.Iterations;
            delegateInvitation.SharingCodeHashFunction = hash.PseudoRandomFunction.ToString();
            delegateInvitation.ResourceOwnerHdid = hdid;
            delegateInvitation.ResourceOwnerIdentifier = resourceOwnerIdentifier;

            await this.delegateInvitationDelegate.UpdateDelegateInvitationAsync(delegateInvitation);
            return sharingCode;
        }
    }
}
