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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using FluentValidation.Results;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models.Cacheable;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class DelegateService : IDelegateService
    {
        private const string DelegationInviteKey = "DelegationInvite";
        private const string ExpiryHoursKey = "ExpiryHours";
        private const string DataProtectionPurpose = "CreateDelegationInvite";

        private readonly IConfiguration configuration;
        private readonly IMapper autoMapper;
        private readonly ILogger<DelegateService> logger;
        private readonly IDataProtector dataProtector;
        private readonly IDelegationDelegate delegationDelegate;
        private readonly IHashDelegate hashDelegate;
        private readonly IEmailQueueService emailQueueService;
        private readonly IHttpRequestService httpRequestService;
        private readonly IPatientDetailsService patientDetailsService;
        private readonly int delegationInvitationExpiryHours;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="autoMapper">The injected auto mapper provider.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="dataProtectionProvider">The injected data protection provider.</param>
        /// <param name="delegationDelegate">The injected delegation delegate.</param>
        /// <param name="hashDelegate">The injected hash delegate.</param>
        /// <param name="emailQueueService">The injected email queue service.</param>
        /// <param name="httpRequestService">The injected http request service.</param>
        /// <param name="patientDetailsService">The injected patient details service.</param>
        public DelegateService(
            IConfiguration configuration,
            IMapper autoMapper,
            ILogger<DelegateService> logger,
            IDataProtectionProvider dataProtectionProvider,
            IDelegationDelegate delegationDelegate,
            IHashDelegate hashDelegate,
            IEmailQueueService emailQueueService,
            IHttpRequestService httpRequestService,
            IPatientDetailsService patientDetailsService)
        {
            this.configuration = configuration;
            this.autoMapper = autoMapper;
            this.logger = logger;
            this.dataProtector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
            this.delegationDelegate = delegationDelegate;
            this.hashDelegate = hashDelegate;
            this.emailQueueService = emailQueueService;
            this.httpRequestService = httpRequestService;
            this.patientDetailsService = patientDetailsService;
            this.delegationInvitationExpiryHours = configuration.GetValue($"{DelegationInviteKey}:{ExpiryHoursKey}", 48);
        }

        /// <inheritdoc/>
        public async Task<string> CreateDelegationAsync(string hdid, CreateDelegationRequest request, CancellationToken ct = default)
        {
            TimeZoneInfo localTimezone = DateFormatter.GetLocalTimeZone(this.configuration);
            DateOnly referenceDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimezone));

            ValidationResult validationResult = await new CreateDelegationRequestValidator(referenceDate).ValidateAsync(request, ct);

            if (!validationResult.IsValid)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.InvalidDelegateInvitationRequest, HttpStatusCode.BadRequest, nameof(DelegateService)));
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

            string inviteKey = this.dataProtector.Protect(delegation.Id.ToString());
            this.logger.LogDebug("Invite Key: {InviteKey}", inviteKey);
            this.SendCreateDelegationInviteEmail(request.Email, resourceOwnerIdentifier, inviteKey, sharingCode);

            return sharingCode;
        }

        private void SendCreateDelegationInviteEmail(string toEmail, string resourceOwnerIdentifier, string inviteKey, string sharingCode)
        {
            string host = this.httpRequestService.GetRefererHost();
            this.logger.LogDebug("Referer Authority Uri: {Host}", host);

            Dictionary<string, string> keyValues = new()
            {
                [EmailTemplateVariable.InviteKey] = inviteKey,
                [EmailTemplateVariable.ActivationHost] = host,
                [EmailTemplateVariable.ResourceOwnerIdentifier] = resourceOwnerIdentifier,
                [EmailTemplateVariable.SharingCode] = sharingCode,
                [EmailTemplateVariable.ExpiryHours] = this.delegationInvitationExpiryHours.ToString(CultureInfo.InvariantCulture),
            };

            Email email = this.emailQueueService.ProcessTemplate(toEmail, EmailTemplateName.CreateDelegationInvite, keyValues);
            this.logger.LogDebug("Sending Email Template: {Template} To: {To} Priority: {Priority}", email.Template, email.To, email.Priority);

            this.emailQueueService.QueueNewEmail(email);
        }
    }
}
