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
namespace HealthGateway.AccountDataAccess.Patient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handle Patient data.
    /// </summary>
    internal class PatientRepository : IPatientRepository
    {
        /// <summary>
        /// The generic cache domain to store patients against.
        /// </summary>
        private const string PatientCacheDomain = "PatientV2";

        private readonly ICacheProvider cacheProvider;
        private readonly int cacheTtl;

        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger<PatientRepository> logger;

        private readonly IClientRegistriesDelegate clientRegistriesDelegate;
        private readonly IAgentAuditDelegate agentAuditDelegate;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IBlockedAccessDelegate blockedAccessDelegate;
        private readonly IPatientIdentityApi patientIdentityApi;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientRepository"/> class.
        /// </summary>
        /// <param name="clientRegistriesDelegate">The injected client registries delegate.</param>
        /// <param name="cacheProvider">The provider responsible for caching.</param>
        /// <param name="logger">The service Logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="patientIdentityApi">The patient identity api to use.</param>
        /// <param name="blockedAccessDelegate">The injected blocked access delegate.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        /// <param name="agentAuditDelegate">The injected agent audit delegate.</param>
        /// <param name="mapper">The injected mapper.</param>
        public PatientRepository(
            IClientRegistriesDelegate clientRegistriesDelegate,
            ICacheProvider cacheProvider,
            IConfiguration configuration,
            ILogger<PatientRepository> logger,
            IPatientIdentityApi patientIdentityApi,
            IBlockedAccessDelegate blockedAccessDelegate,
            IAuthenticationDelegate authenticationDelegate,
            IAgentAuditDelegate agentAuditDelegate,
            IMapper mapper)
        {
            this.clientRegistriesDelegate = clientRegistriesDelegate;
            this.cacheProvider = cacheProvider;
            this.cacheTtl = configuration.GetSection("PatientService").GetValue("CacheTTL", 0);
            this.logger = logger;
            this.patientIdentityApi = patientIdentityApi;
            this.blockedAccessDelegate = blockedAccessDelegate;
            this.authenticationDelegate = authenticationDelegate;
            this.agentAuditDelegate = agentAuditDelegate;
            this.mapper = mapper;
        }

        private static ActivitySource Source { get; } = new(nameof(PatientRepository));

        /// <inheritdoc/>
        public async Task<PatientQueryResult> Query(PatientQuery query, CancellationToken ct)
        {
            return query switch
            {
                PatientDetailsQuery q => await this.Handle(q, ct).ConfigureAwait(true),

                _ => throw new NotImplementedException($"{query.GetType().FullName}"),
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AgentAudit>> AgentAuditQuery(string hdid, AuditGroup group, CancellationToken ct)
        {
            AgentAuditQuery agentAuditQuery = new()
            {
                Hdid = hdid,
                GroupCode = group,
            };

            return await this.agentAuditDelegate.GetAgentAuditsAsync(agentAuditQuery).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<AgentAudit> BlockAccessCommand(string hdid, IEnumerable<DataSource> dataSources, string reason, CancellationToken ct)
        {
            string authenticatedUserId = this.authenticationDelegate.FetchAuthenticatedUserId() ?? UserId.DefaultUser;

            AgentAudit agentAudit = new()
            {
                Hdid = hdid,
                Reason = reason,
                OperationCode = AuditOperation.ChangeDataSourceAccess,
                GroupCode = AuditGroup.BlockedAccess,
                AgentUsername = this.authenticationDelegate.FetchAuthenticatedPreferredUsername() ?? authenticatedUserId,
                TransactionDateTime = DateTime.UtcNow,
                CreatedBy = authenticatedUserId,
                UpdatedBy = authenticatedUserId,
            };

            BlockedAccess? blockedAccess = await this.blockedAccessDelegate.GetBlockedAccessAsync(hdid).ConfigureAwait(true) ?? new()
            {
                Hdid = hdid,
                CreatedBy = authenticatedUserId,
            };

            blockedAccess.UpdatedBy = authenticatedUserId;

            Dictionary<string, string> sources = dataSources.ToDictionary(x => x.ToString(), _ => "true");

            if (sources.Any())
            {
                blockedAccess.DataSources = sources;
                blockedAccess.CreatedDateTime = DateTime.UtcNow;
                blockedAccess.UpdatedDateTime = DateTime.UtcNow;
                await this.blockedAccessDelegate.UpdateBlockedAccessAsync(blockedAccess, agentAudit).ConfigureAwait(true);
            }
            else
            {
                await this.blockedAccessDelegate.DeleteBlockedAccessAsync(blockedAccess, agentAudit);
            }

            return agentAudit;
        }

        /// <inheritdoc/>
        public async Task<BlockedAccess?> BlockedAccessQuery(string hdid, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                throw new InvalidOperationException("cancellation was requested");
            }

            return await this.blockedAccessDelegate.GetBlockedAccessAsync(hdid).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, string>> DataSourceQuery(string hdid, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                throw new InvalidOperationException("cancellation was requested");
            }

            return await this.blockedAccessDelegate.GetDataSourcesAsync(hdid).ConfigureAwait(true);
        }

        private async Task<PatientQueryResult> Handle(PatientDetailsQuery query, CancellationToken ct)
        {
            this.logger.LogDebug("Patient details query source: {Source} - cache: {Cache}", query.Source, query.UseCache);

            if (ct.IsCancellationRequested)
            {
                throw new InvalidOperationException("cancellation was requested");
            }

            if (query.Hdid == null && query.Phn == null)
            {
                throw new InvalidOperationException("Must specify either Hdid or Phn to query patient details");
            }

            return await this.GetPatient(query).ConfigureAwait(true);
        }

        private async Task<PatientQueryResult> GetPatient(PatientDetailsQuery query, bool disabledValidation = false)
        {
            PatientModel? cachedPatient = query.Hdid != null ? this.GetFromCache(query.Hdid, PatientIdentifierType.Hdid) : this.GetFromCache(query.Phn, PatientIdentifierType.Phn);
            PatientRequest patientRequest = new(query.Hdid ?? query.Phn, this.clientRegistriesDelegate, this.patientIdentityApi, this.logger, this.mapper, cachedPatient, disabledValidation);

            // Create enum string to derive actual enum to be used to extract the appropriate strategy for querying patient
            string prefix = query.Hdid != null ? "Hdid" : "Phn";
            string suffix = query.UseCache ? "Cache" : string.Empty;
            string strategy = $"{prefix}{query.Source}{suffix}";
            this.logger.LogDebug("Strategy: {Strategy}", strategy);

            if (Enum.TryParse(strategy, out PatientStrategy strategyKey))
            {
                this.logger.LogDebug("Strategy key: {Strategy}", strategyKey);

                // Get the appropriate strategy from factory to query patient
                IPatientQueryStrategy patientQueryStrategy = PatientQueryFactory.GetStrategy(strategyKey);
                PatientQueryContext context = new(patientQueryStrategy);
                PatientModel? patient = await context.GetPatientAsync(patientRequest).ConfigureAwait(true);

                // Only cache if validation is enabled (as some clients could get invalid data) and when successful.
                if (!disabledValidation && patient != null)
                {
                    this.CachePatient(patient);
                }

                return new PatientQueryResult(new[] { patient! });
            }

            throw new InvalidOperationException($"Unable to retrieve patient query strategy due to invalid key: {strategy}");
        }

        /// <summary>
        /// Attempts to get the Patient model from the Generic Cache.
        /// </summary>
        /// <param name="identifier">The resource identifier used to determine the key to use.</param>
        /// <param name="identifierType">The type of patient identifier we are searching for.</param>
        /// <returns>The found Patient model or null.</returns>
        private PatientModel? GetFromCache(string identifier, PatientIdentifierType identifierType)
        {
            using Activity? activity = Source.StartActivity();
            PatientModel? retPatient = null;
            if (this.cacheTtl > 0)
            {
                switch (identifierType)
                {
                    case PatientIdentifierType.Hdid:
                        this.logger.LogDebug("Querying Patient Cache by HDID");
                        retPatient = this.cacheProvider.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{identifier}");
                        break;

                    case PatientIdentifierType.Phn:
                        this.logger.LogDebug("Querying Patient Cache by PHN");
                        retPatient = this.cacheProvider.GetItem<PatientModel>($"{PatientCacheDomain}:PHN:{identifier}");
                        break;
                }

                string message = $"Patient with identifier {identifier} was {(retPatient == null ? "not" : string.Empty)} found in cache";
                this.logger.LogDebug("{Message}", message);
            }

            activity?.Stop();
            return retPatient;
        }

        /// <summary>
        /// Caches the Patient model if enabled.
        /// </summary>
        /// <param name="patientModel">The patient to cache.</param>
        private void CachePatient(PatientModel patientModel)
        {
            using Activity? activity = Source.StartActivity();
            string hdid = patientModel.Hdid;
            if (this.cacheTtl > 0)
            {
                this.logger.LogDebug("Attempting to cache patient: {Hdid}", hdid);
                TimeSpan expiry = TimeSpan.FromMinutes(this.cacheTtl);
                if (!string.IsNullOrEmpty(patientModel.Hdid))
                {
                    this.cacheProvider.AddItem($"{PatientCacheDomain}:HDID:{patientModel.Hdid}", patientModel, expiry);
                }

                if (!string.IsNullOrEmpty(patientModel.Phn))
                {
                    this.cacheProvider.AddItem($"{PatientCacheDomain}:PHN:{patientModel.Phn}", patientModel, expiry);
                }
            }
            else
            {
                this.logger.LogDebug("Patient caching is disabled will not cache patient: {Hdid}", hdid);
            }

            activity?.Stop();
        }
    }
}
