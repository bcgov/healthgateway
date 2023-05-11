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
namespace HealthGateway.Admin.Server.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Audit;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.MapUtils;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    public class SupportService : ISupportService
    {
        private readonly IMapper autoMapper;
        private readonly IConfiguration configuration;
        private readonly IMessagingVerificationDelegate messagingVerificationDelegate;
        private readonly IPatientRepository patientRepository;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IAuditRepository auditRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportService"/> class.
        /// </summary>
        /// <param name="autoMapper">The injected automapper provider.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="messagingVerificationDelegate">The Messaging verification delegate to interact with the DB.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="resourceDelegateDelegate">The resource delegate used to lookup delegates and owners.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="auditRepository">The injected audit repository.</param>
        public SupportService(
            IMapper autoMapper,
            IConfiguration configuration,
            IMessagingVerificationDelegate messagingVerificationDelegate,
            IPatientRepository patientRepository,
            IResourceDelegateDelegate resourceDelegateDelegate,
            IUserProfileDelegate userProfileDelegate,
            IAuditRepository auditRepository)
        {
            this.autoMapper = autoMapper;
            this.configuration = configuration;
            this.messagingVerificationDelegate = messagingVerificationDelegate;
            this.patientRepository = patientRepository;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.auditRepository = auditRepository;
        }

        /// <inheritdoc/>
        public async Task<PatientSupportDetails> GetMessageVerificationsAsync(string hdid, CancellationToken ct = default)
        {
            IList<MessagingVerification> messagingVerifications = await this.messagingVerificationDelegate.GetUserMessageVerificationsAsync(hdid).ConfigureAwait(true);
            AgentAuditQuery agentAuditQuery = new(hdid, AuditGroup.BlockedAccess);
            IEnumerable<AgentAudit> agentAudits = await this.auditRepository.Handle(agentAuditQuery, ct).ConfigureAwait(true);
            Dictionary<string, string> dataSources = await this.patientRepository.DataSourceQuery(hdid, ct).ConfigureAwait(true);
            TimeZoneInfo localTimezone = DateFormatter.GetLocalTimeZone(this.configuration);

            PatientSupportDetails details = new()
            {
                MessagingVerifications = messagingVerifications.Select(m => MessagingVerificationMapUtils.ToUiModel(m, this.autoMapper, localTimezone)),
                AgentActions = agentAudits.Select(audit => this.autoMapper.Map<AgentAction>(audit)),
                DataSources = dataSources,
            };

            return details;
        }

        /// <inheritdoc/>
        public async Task<IList<PatientSupportResult>> GetPatientsAsync(PatientQueryType queryType, string queryString, CancellationToken ct = default)
        {
            if (queryType is PatientQueryType.Hdid or PatientQueryType.Phn)
            {
                PatientIdentifierType identifierType = queryType == PatientQueryType.Phn ? PatientIdentifierType.Phn : PatientIdentifierType.Hdid;
                PatientSupportResult? patientSupportDetails = await this.GetPatientSupportDetailsAsync(identifierType, queryString, ct).ConfigureAwait(true);
                return patientSupportDetails == null ? Array.Empty<PatientSupportResult>() : new List<PatientSupportResult> { patientSupportDetails };
            }

            IEnumerable<UserProfile> profiles = queryType switch
            {
                PatientQueryType.Dependent =>
                    await this.GetDelegateProfilesAsync(queryString, ct).ConfigureAwait(true),
                PatientQueryType.Email =>
                    await this.userProfileDelegate.GetUserProfilesAsync(UserQueryType.Email, queryString).ConfigureAwait(true),
                PatientQueryType.Sms =>
                    await this.userProfileDelegate.GetUserProfilesAsync(UserQueryType.Sms, queryString).ConfigureAwait(true),
                _ =>
                    throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails($"Unknown {nameof(queryType)}", HttpStatusCode.BadRequest, nameof(SupportService))),
            };

            IEnumerable<Task<PatientSupportResult>> tasks = profiles.Select(profile => this.GetPatientSupportDetailsAsync(profile, ct));
            return await Task.WhenAll(tasks).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task BlockAccessAsync(string hdid, IEnumerable<DataSource> dataSources, string reason, CancellationToken ct = default)
        {
            BlockAccessCommand blockAccessCommand = new(hdid, dataSources, reason);
            await this.patientRepository.BlockAccess(blockAccessCommand, ct).ConfigureAwait(true);
        }

        private async Task<IEnumerable<UserProfile>> GetDelegateProfilesAsync(string dependentPhn, CancellationToken ct)
        {
            PatientModel? dependent = await this.GetPatientAsync(PatientIdentifierType.Phn, dependentPhn, ct).ConfigureAwait(true);
            if (dependent == null)
            {
                return Enumerable.Empty<UserProfile>();
            }

            ResourceDelegateQuery query = new()
            {
                ByOwnerHdid = dependent.Hdid,
                IncludeProfile = true,
                TakeAmount = 25,
            };
            ResourceDelegateQueryResult result = await this.resourceDelegateDelegate.SearchAsync(query).ConfigureAwait(true);
            return result.Items.Select(rd => rd.UserProfile);
        }

        private async Task<PatientSupportResult?> GetPatientSupportDetailsAsync(PatientIdentifierType identifierType, string identifier, CancellationToken ct)
        {
            PatientModel? patient = await this.GetPatientAsync(identifierType, identifier, ct).ConfigureAwait(true);
            UserProfile? profile = null;

            string? hdid = identifierType == PatientIdentifierType.Hdid ? identifier : patient?.Hdid;
            if (hdid != null)
            {
                profile = await this.userProfileDelegate.GetUserProfileAsync(hdid).ConfigureAwait(true);
            }

            if (patient == null && profile == null)
            {
                return null;
            }

            return this.MapToPatientSupportDetails(patient, profile);
        }

        private async Task<PatientSupportResult> GetPatientSupportDetailsAsync(UserProfile profile, CancellationToken ct)
        {
            PatientModel? patient = await this.GetPatientAsync(PatientIdentifierType.Hdid, profile.HdId, ct).ConfigureAwait(true);

            return this.MapToPatientSupportDetails(patient, profile);
        }

        private async Task<PatientModel?> GetPatientAsync(PatientIdentifierType identifierType, string queryString, CancellationToken ct)
        {
            PatientDetailsQuery query = identifierType == PatientIdentifierType.Hdid
                ? new PatientDetailsQuery(Hdid: queryString, Source: PatientDetailSource.All, UseCache: true)
                : new PatientDetailsQuery(queryString, Source: PatientDetailSource.Empi, UseCache: true);

            try
            {
                PatientQueryResult result = await this.patientRepository.Query(query, ct).ConfigureAwait(true);
                return result.Items.SingleOrDefault();
            }
            catch (ProblemDetailsException e) when (e.ProblemDetails?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private PatientSupportResult MapToPatientSupportDetails(PatientModel? patient, UserProfile? userProfile)
        {
            PatientSupportResult patientSupportResult = this.autoMapper.Map<PatientModel?, PatientSupportResult>(patient) ?? new PatientSupportResult();

            patientSupportResult.Status = PatientStatus.Default;
            if (patient == null)
            {
                patientSupportResult.Status = PatientStatus.NotFound;
            }
            else if (patient.IsDeceased == true)
            {
                patientSupportResult.Status = PatientStatus.Deceased;
            }
            else if (userProfile == null)
            {
                patientSupportResult.Status = PatientStatus.NotUser;
            }

            patientSupportResult.ProfileCreatedDateTime = userProfile?.CreatedDateTime;
            patientSupportResult.ProfileLastLoginDateTime = userProfile?.LastLoginDateTime;

            if (patient == null && userProfile != null)
            {
                patientSupportResult.Hdid = userProfile.HdId;
            }

            return patientSupportResult;
        }
    }
}
