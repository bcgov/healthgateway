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
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using HealthGateway.AccountDataAccess.Audit;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Api;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="mappingService">The injected mapping service.</param>
    /// <param name="commonMappingService">The injected common mapping service.</param>
    /// <param name="messagingVerificationDelegate">The Messaging verification delegate to interact with the DB.</param>
    /// <param name="patientRepository">The injected patient repository.</param>
    /// <param name="resourceDelegateDelegate">The resource delegate used to lookup delegates and owners.</param>
    /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
    /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
    /// <param name="immunizationAdminDelegate">The injected immunization admin delegate.</param>
    /// <param name="immunizationAdminApi">The injected immunization admin api.</param>
    /// <param name="auditRepository">The injected audit repository.</param>
    /// <param name="cacheProvider">The injected cache provider.</param>
    /// <param name="logger">The injected logger provider.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
    public class SupportService(
        IAdminServerMappingService mappingService,
        ICommonMappingService commonMappingService,
        IMessagingVerificationDelegate messagingVerificationDelegate,
        IPatientRepository patientRepository,
        IResourceDelegateDelegate resourceDelegateDelegate,
        IUserProfileDelegate userProfileDelegate,
        IAuthenticationDelegate authenticationDelegate,
        IImmunizationAdminDelegate immunizationAdminDelegate,
        IImmunizationAdminApi immunizationAdminApi,
        IAuditRepository auditRepository,
        ICacheProvider cacheProvider,
        ILogger<SupportService> logger) : ISupportService
    {
        /// <inheritdoc/>
        public async Task<PatientSupportDetails> GetPatientSupportDetailsAsync(
            PatientSupportDetailsQuery query,
            CancellationToken ct = default)
        {
            PatientModel patient;

            if (query.QueryType == ClientRegistryType.Hdid)
            {
                patient = await this.GetPatientAsync(new(Hdid: query.QueryParameter, Source: PatientDetailSource.All, UseCache: false), ct);
            }
            else
            {
                patient = await this.GetPatientAsync(new(query.QueryParameter, Source: PatientDetailSource.Empi, UseCache: false), ct);
            }

            Task<VaccineDetails>? getVaccineDetails =
                query.IncludeCovidDetails ? this.GetVaccineDetailsAsync(patient, query.RefreshVaccineDetails, ct) : null;

            Task<CovidAssessmentDetailsResponse>? getCovidAssessmentDetails =
                query.IncludeCovidDetails ? immunizationAdminApi.GetCovidAssessmentDetailsAsync(new() { Phn = patient.Phn }, await this.GetAccessTokenAsync(ct), ct) : null;

            IEnumerable<MessagingVerificationModel>? messagingVerifications =
                query.IncludeMessagingVerifications ? await this.GetMessagingVerificationsAsync(patient.Hdid, ct) : null;

            IEnumerable<DataSource>? blockedDataSources =
                query.IncludeBlockedDataSources ? await this.GetBlockedDataSourcesAsync(patient.Hdid, ct) : null;

            IEnumerable<AgentAction>? agentActions =
                query.IncludeAgentActions ? await this.GetAgentActionsAsync(patient.Hdid, ct) : null;

            IEnumerable<PatientSupportDependentInfo>? dependents =
                query.IncludeDependents ? await this.GetAllDependentInfoAsync(patient.Hdid, ct) : null;

            return new()
            {
                MessagingVerifications = messagingVerifications,
                BlockedDataSources = blockedDataSources,
                AgentActions = agentActions,
                Dependents = dependents,
                VaccineDetails = getVaccineDetails == null ? null : await getVaccineDetails,
                CovidAssessmentDetails = getCovidAssessmentDetails == null ? null : await getCovidAssessmentDetails,
            };
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PatientSupportResult>> GetPatientsAsync(PatientQueryType queryType, string queryString, CancellationToken ct = default)
        {
            if (queryType is PatientQueryType.Hdid or PatientQueryType.Phn)
            {
                PatientIdentifierType identifierType = queryType == PatientQueryType.Phn ? PatientIdentifierType.Phn : PatientIdentifierType.Hdid;
                PatientSupportResult? patientSupportDetails = await this.GetPatientSupportResultAsync(identifierType, queryString, ct);
                return patientSupportDetails == null ? [] : [patientSupportDetails];
            }

            IEnumerable<UserProfile> profiles = queryType switch
            {
                PatientQueryType.Dependent =>
                    await this.GetDelegateProfilesAsync(queryString, ct),
                PatientQueryType.Email =>
                    await userProfileDelegate.GetUserProfilesAsync(UserQueryType.Email, queryString, ct),
                PatientQueryType.Sms =>
                    await userProfileDelegate.GetUserProfilesAsync(UserQueryType.Sms, queryString, ct),
                _ =>
                    throw new ValidationException($"Unknown {nameof(queryType)}"),
            };

            IEnumerable<Task<PatientSupportResult>> tasks = profiles.Select(profile => this.GetPatientSupportResultAsync(profile, ct));
            return await Task.WhenAll(tasks);
        }

        /// <inheritdoc/>
        public async Task BlockAccessAsync(string hdid, IEnumerable<DataSource> dataSources, string reason, CancellationToken ct = default)
        {
            BlockAccessCommand blockAccessCommand = new(hdid, dataSources, reason);
            await patientRepository.BlockAccessAsync(blockAccessCommand, ct);
        }

        private async Task<PatientModel> GetPatientAsync(PatientDetailsQuery query, CancellationToken ct = default)
        {
            return (await patientRepository.QueryAsync(query, ct)).Item;
        }

        private async Task<PatientModel?> TryGetPatientAsync(PatientIdentifierType identifierType, string queryString, CancellationToken ct)
        {
            PatientDetailsQuery query = identifierType == PatientIdentifierType.Hdid
                ? new PatientDetailsQuery(Hdid: queryString, Source: PatientDetailSource.All, UseCache: false)
                : new PatientDetailsQuery(queryString, Source: PatientDetailSource.Empi, UseCache: false);

            try
            {
                return await this.GetPatientAsync(query, ct);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        private async Task<IEnumerable<MessagingVerificationModel>> GetMessagingVerificationsAsync(string hdid, CancellationToken ct)
        {
            IEnumerable<MessagingVerification> verifications = await messagingVerificationDelegate.GetUserMessageVerificationsAsync(hdid, ct);
            return verifications.Select(commonMappingService.MapToMessagingVerificationModel);
        }

        private async Task<IEnumerable<DataSource>> GetBlockedDataSourcesAsync(string hdid, CancellationToken ct)
        {
            // Invalidate blocked data source cache and then get newest value(s) from database.
            string blockedAccessCacheKey = string.Format(CultureInfo.InvariantCulture, ICacheProvider.BlockedAccessCachePrefixKey, hdid);
            string message = $"Removing item for key: {blockedAccessCacheKey} from cache";
            logger.LogDebug("{Message}", message);
            await cacheProvider.RemoveItemAsync(blockedAccessCacheKey, ct);

            return await patientRepository.GetDataSourcesAsync(hdid, ct);
        }

        private async Task<IEnumerable<AgentAction>> GetAgentActionsAsync(string hdid, CancellationToken ct)
        {
            IEnumerable<AgentAudit> audits = await auditRepository.HandleAsync(new(hdid), ct);
            return audits.Select(mappingService.MapToAgentAction);
        }

        private async Task<IEnumerable<PatientSupportDependentInfo>> GetAllDependentInfoAsync(string delegateHdid, CancellationToken ct)
        {
            ResourceDelegateQuery query = new() { ByDelegateHdid = delegateHdid, IncludeDependent = true };
            ResourceDelegateQueryResult result = await resourceDelegateDelegate.SearchAsync(query, ct);
            IEnumerable<Task<PatientSupportDependentInfo?>> tasks = result.Items.Select(r => this.GetDependentInfoAsync(r, ct));
            return (await Task.WhenAll(tasks)).OfType<PatientSupportDependentInfo>();
        }

        private async Task<PatientSupportDependentInfo?> GetDependentInfoAsync(ResourceDelegateQueryResultItem item, CancellationToken ct)
        {
            PatientModel? patient = await this.TryGetPatientAsync(PatientIdentifierType.Hdid, item.ResourceDelegate.ResourceOwnerHdid, ct);
            if (patient == null)
            {
                return null;
            }

            PatientSupportDependentInfo dependentInfo = mappingService.MapToPatientSupportDependentInfo(patient);
            dependentInfo.ExpiryDate = item.ResourceDelegate.ExpiryDate;
            dependentInfo.Protected = item.Dependent?.Protected == true;
            return dependentInfo;
        }

        private async Task<VaccineDetails> GetVaccineDetailsAsync(PatientModel patient, bool refresh, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(patient.Phn) || patient.Birthdate == DateTime.MinValue)
            {
                logger.LogError("Patient PHN {PersonalHealthNumber} or DOB {Birthdate}) are invalid", patient.Phn, patient.Birthdate);
                throw new InvalidDataException(ErrorMessages.PhnOrDateOfBirthInvalid);
            }

            return await immunizationAdminDelegate.GetVaccineDetailsWithRetriesAsync(patient.Phn, await this.GetAccessTokenAsync(ct), refresh, ct);
        }

        private async Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            string? accessToken = await authenticationDelegate.FetchAuthenticatedUserTokenAsync(ct);
            return accessToken ?? throw new UnauthorizedAccessException(ErrorMessages.CannotFindAccessToken);
        }

        private async Task<PatientSupportResult?> GetPatientSupportResultAsync(PatientIdentifierType identifierType, string identifier, CancellationToken ct)
        {
            PatientModel? patient = await this.TryGetPatientAsync(identifierType, identifier, ct);

            string? hdid = identifierType == PatientIdentifierType.Hdid
                ? identifier
                : patient?.Hdid;
            UserProfile? profile = hdid == null
                ? null
                : await userProfileDelegate.GetUserProfileAsync(hdid, ct: ct);

            if (patient == null && profile == null)
            {
                return null;
            }

            return mappingService.MapToPatientSupportResult(patient, profile);
        }

        private async Task<PatientSupportResult> GetPatientSupportResultAsync(UserProfile profile, CancellationToken ct)
        {
            PatientModel? patient = await this.TryGetPatientAsync(PatientIdentifierType.Hdid, profile.HdId, ct);
            return mappingService.MapToPatientSupportResult(patient, profile);
        }

        private async Task<IEnumerable<UserProfile>> GetDelegateProfilesAsync(string dependentPhn, CancellationToken ct)
        {
            PatientModel? dependent = await this.TryGetPatientAsync(PatientIdentifierType.Phn, dependentPhn, ct);
            if (dependent == null)
            {
                return [];
            }

            ResourceDelegateQuery query = new()
            {
                ByOwnerHdid = dependent.Hdid,
                IncludeProfile = true,
                TakeAmount = 25,
            };
            ResourceDelegateQueryResult result = await resourceDelegateDelegate.SearchAsync(query, ct);
            return result.Items.Select(c => c.ResourceDelegate.UserProfile);
        }
    }
}
