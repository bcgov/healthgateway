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
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Audit;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.MapUtils;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class SupportService : ISupportService
    {
        private readonly IMapper autoMapper;
        private readonly IConfiguration configuration;
        private readonly IMessagingVerificationDelegate messagingVerificationDelegate;
        private readonly IPatientRepository patientRepository;
        private readonly IDelegationDelegate delegationDelegate;
        private readonly IResourceDelegateDelegate resourceDelegateDelegate;
        private readonly IUserProfileDelegate userProfileDelegate;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IImmunizationAdminDelegate immunizationAdminDelegate;
        private readonly IImmunizationAdminApi immunizationAdminApi;
        private readonly IAuditRepository auditRepository;
        private readonly ICacheProvider cacheProvider;
        private readonly ILogger<SupportService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportService"/> class.
        /// </summary>
        /// <param name="autoMapper">The injected automapper provider.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="messagingVerificationDelegate">The Messaging verification delegate to interact with the DB.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="delegationDelegate">The injected delegation delegate.</param>
        /// <param name="resourceDelegateDelegate">The resource delegate used to lookup delegates and owners.</param>
        /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="immunizationAdminDelegate">The injected immunization admin delegate.</param>
        /// <param name="immunizationAdminApi">The injected immunization admin api.</param>
        /// <param name="auditRepository">The injected audit repository.</param>
        /// <param name="cacheProvider">The injected cache provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        public SupportService(
            IMapper autoMapper,
            IConfiguration configuration,
            IMessagingVerificationDelegate messagingVerificationDelegate,
            IPatientRepository patientRepository,
            IDelegationDelegate delegationDelegate,
            IResourceDelegateDelegate resourceDelegateDelegate,
            IUserProfileDelegate userProfileDelegate,
            IAuthenticationDelegate authenticationDelegate,
            IImmunizationAdminDelegate immunizationAdminDelegate,
            IImmunizationAdminApi immunizationAdminApi,
            IAuditRepository auditRepository,
            ICacheProvider cacheProvider,
            ILogger<SupportService> logger)
        {
            this.autoMapper = autoMapper;
            this.configuration = configuration;
            this.messagingVerificationDelegate = messagingVerificationDelegate;
            this.patientRepository = patientRepository;
            this.delegationDelegate = delegationDelegate;
            this.resourceDelegateDelegate = resourceDelegateDelegate;
            this.userProfileDelegate = userProfileDelegate;
            this.authenticationDelegate = authenticationDelegate;
            this.immunizationAdminDelegate = immunizationAdminDelegate;
            this.immunizationAdminApi = immunizationAdminApi;
            this.auditRepository = auditRepository;
            this.cacheProvider = cacheProvider;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<PatientSupportDetails> GetPatientSupportDetailsAsync(
            ClientRegistryType queryType,
            string queryString,
            bool includeMessagingVerifications,
            bool includeBlockedDataSources,
            bool includeAgentActions,
            bool includeDependents,
            bool includeCovidDetails,
            bool refreshVaccineDetails,
            CancellationToken ct = default)
        {
            IEnumerable<MessagingVerificationModel>? messagingVerifications = null;
            IEnumerable<DataSource>? blockedDataSources = null;
            IEnumerable<AgentAction>? agentActions = null;
            IEnumerable<PatientSupportDependentInfo>? dependents = null;
            VaccineDetails? vaccineDetails = null;
            CovidAssessmentDetailsResponse? covidAssessmentDetails = null;
            PatientModel patient;

            if (queryType == ClientRegistryType.Hdid)
            {
                PatientDetailsQuery query = new(Hdid: queryString, Source: PatientDetailSource.All, UseCache: false);
                patient = await this.GetPatientAsync(query, ct).ConfigureAwait(true);
            }
            else
            {
                PatientDetailsQuery query = new(queryString, Source: PatientDetailSource.Empi, UseCache: false);
                patient = await this.GetPatientAsync(query, ct).ConfigureAwait(true);
            }

            if (includeMessagingVerifications)
            {
                messagingVerifications = await this.GetMessagingVerificationsAsync(patient.Hdid, ct);
            }

            if (includeBlockedDataSources)
            {
                blockedDataSources = await this.GetBlockedDataSourcesAsync(patient.Hdid, ct);
            }

            if (includeAgentActions)
            {
                agentActions = await this.GetAgentActionsAsync(patient.Hdid, ct);
            }

            if (includeDependents)
            {
                dependents = await this.GetDependentsAsync(patient.Hdid, ct);
            }

            if (includeCovidDetails)
            {
                vaccineDetails = await this.GetVaccineDetails(patient, refreshVaccineDetails).ConfigureAwait(true);
                covidAssessmentDetails = await this.immunizationAdminApi.GetCovidAssessmentDetails(new() { Phn = patient.Phn }, this.GetAccessToken()).ConfigureAwait(true);
            }

            PatientSupportDetails details = new()
            {
                MessagingVerifications = messagingVerifications,
                AgentActions = agentActions,
                BlockedDataSources = blockedDataSources,
                Dependents = dependents,
                VaccineDetails = vaccineDetails,
                CovidAssessmentDetails = covidAssessmentDetails,
            };

            return details;
        }

        /// <inheritdoc/>
        public async Task<IList<PatientSupportResult>> GetPatientsAsync(PatientQueryType queryType, string queryString, CancellationToken ct = default)
        {
            if (queryType is PatientQueryType.Hdid or PatientQueryType.Phn)
            {
                PatientIdentifierType identifierType = queryType == PatientQueryType.Phn ? PatientIdentifierType.Phn : PatientIdentifierType.Hdid;
                PatientSupportResult? patientSupportDetails = await this.GetPatientSupportResultAsync(identifierType, queryString, ct).ConfigureAwait(true);
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

            IEnumerable<Task<PatientSupportResult>> tasks = profiles.Select(profile => this.GetPatientSupportResultAsync(profile, ct));
            return await Task.WhenAll(tasks).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task BlockAccessAsync(string hdid, IEnumerable<DataSource> dataSources, string reason, CancellationToken ct = default)
        {
            BlockAccessCommand blockAccessCommand = new(hdid, dataSources, reason);
            await this.patientRepository.BlockAccess(blockAccessCommand, ct).ConfigureAwait(true);
        }

        private async Task<IList<PatientSupportDependentInfo>> GetDependentsAsync(string delegateHdid, CancellationToken ct)
        {
            List<PatientSupportDependentInfo> dependents = new();

            IList<ResourceDelegate> resourceDelegates = await this.SearchDependents(delegateHdid);
            foreach (ResourceDelegate resourceDelegate in resourceDelegates)
            {
                PatientModel? dependentPatient = await this.GetPatientAsync(PatientIdentifierType.Hdid, resourceDelegate.ResourceOwnerHdid, ct);
                if (dependentPatient != null)
                {
                    Dependent? dependent = await this.delegationDelegate.GetDependentAsync(resourceDelegate.ResourceOwnerHdid, false, ct);
                    PatientSupportDependentInfo dependentInfo = this.autoMapper.Map<PatientModel, PatientSupportDependentInfo>(dependentPatient);
                    dependentInfo.ExpiryDate = resourceDelegate.ExpiryDate;
                    dependentInfo.Protected = dependent?.Protected == true;
                    dependents.Add(dependentInfo);
                }
            }

            return dependents;
        }

        private async Task<IEnumerable<MessagingVerificationModel>> GetMessagingVerificationsAsync(string hdid, CancellationToken ct)
        {
            IEnumerable<MessagingVerification> verifications = await this.messagingVerificationDelegate.GetUserMessageVerificationsAsync(hdid, ct);

            TimeZoneInfo localTimezone = DateFormatter.GetLocalTimeZone(this.configuration);
            return verifications.Select(m => MessagingVerificationMapUtils.ToUiModel(m, this.autoMapper, localTimezone));
        }

        private async Task<IEnumerable<DataSource>> GetBlockedDataSourcesAsync(string hdid, CancellationToken ct)
        {
            // Invalidate blocked data source cache and then get newest value(s) from database.
            string blockedAccessCacheKey = string.Format(CultureInfo.InvariantCulture, ICacheProvider.BlockedAccessCachePrefixKey, hdid);
            string message = $"Removing item for key: {blockedAccessCacheKey} from cache";
            this.logger.LogDebug("{Message}", message);
            await this.cacheProvider.RemoveItemAsync(blockedAccessCacheKey);

            return await this.patientRepository.GetDataSources(hdid, ct);
        }

        private async Task<IEnumerable<AgentAction>> GetAgentActionsAsync(string hdid, CancellationToken ct)
        {
            AgentAuditQuery agentAuditQuery = new(hdid);
            IEnumerable<AgentAudit> audits = await this.auditRepository.Handle(agentAuditQuery, ct).ConfigureAwait(true);

            return audits.Select(audit => this.autoMapper.Map<AgentAudit, AgentAction>(audit));
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

        private async Task<PatientSupportResult?> GetPatientSupportResultAsync(PatientIdentifierType identifierType, string identifier, CancellationToken ct)
        {
            PatientModel? patient = await this.GetPatientAsync(identifierType, identifier, ct).ConfigureAwait(true);

            string? hdid = identifierType == PatientIdentifierType.Hdid
                ? identifier
                : patient?.Hdid;
            UserProfile? profile = hdid == null
                ? null
                : await this.userProfileDelegate.GetUserProfileAsync(hdid).ConfigureAwait(true);

            if (patient == null && profile == null)
            {
                return null;
            }

            return this.MapToPatientSupportResult(patient, profile);
        }

        private async Task<PatientSupportResult> GetPatientSupportResultAsync(UserProfile profile, CancellationToken ct)
        {
            PatientModel? patient = await this.GetPatientAsync(PatientIdentifierType.Hdid, profile.HdId, ct).ConfigureAwait(true);

            return this.MapToPatientSupportResult(patient, profile);
        }

        private string GetAccessToken()
        {
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (accessToken == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.CannotFindAccessToken, HttpStatusCode.Unauthorized, nameof(SupportService)));
            }

            return accessToken;
        }

        private async Task<PatientModel?> GetPatientAsync(PatientIdentifierType identifierType, string queryString, CancellationToken ct)
        {
            PatientDetailsQuery query = identifierType == PatientIdentifierType.Hdid
                ? new PatientDetailsQuery(Hdid: queryString, Source: PatientDetailSource.All, UseCache: false)
                : new PatientDetailsQuery(queryString, Source: PatientDetailSource.Empi, UseCache: false);

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

        private async Task<PatientModel> GetPatientAsync(PatientDetailsQuery query, CancellationToken ct = default)
        {
            PatientModel? patient = (await this.patientRepository.Query(query, ct).ConfigureAwait(true)).Items.SingleOrDefault();
            if (patient == null)
            {
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryRecordsNotFound, HttpStatusCode.NotFound, nameof(SupportService)));
            }

            return patient;
        }

        private async Task<IList<ResourceDelegate>> SearchDependents(string delegateHdid)
        {
            ResourceDelegateQuery query = new() { ByDelegateHdid = delegateHdid };
            ResourceDelegateQueryResult result = await this.resourceDelegateDelegate.SearchAsync(query);
            return result.Items;
        }

        private async Task<VaccineDetails> GetVaccineDetails(PatientModel patient, bool refresh)
        {
            if (!string.IsNullOrEmpty(patient.Phn) && patient.Birthdate != DateTime.MinValue)
            {
                return await this.immunizationAdminDelegate.GetVaccineDetailsWithRetries(patient.Phn, this.GetAccessToken(), refresh).ConfigureAwait(true);
            }

            this.logger.LogError("Patient PHN {PersonalHealthNumber} or DOB {Birthdate}) are invalid", patient.Phn, patient.Birthdate);
            throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.PhnOrDateAndBirthInvalid, HttpStatusCode.BadRequest, nameof(SupportService)));
        }

        private PatientSupportResult MapToPatientSupportResult(PatientModel? patient, UserProfile? userProfile)
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
