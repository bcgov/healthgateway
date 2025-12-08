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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handle Patient data.
    /// </summary>
    internal class PatientRepository : IPatientRepository
    {
        private const string BlockedAccessKey = "BlockedAccess";
        private const string CacheTtlKey = "CacheTtl";
        private readonly int blockedAccessCacheTtl;
        private readonly bool blockedDataSourcesChangeFeedEnabled;

        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger<PatientRepository> logger;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IBlockedAccessDelegate blockedAccessDelegate;
        private readonly ICacheProvider cacheProvider;
        private readonly PatientQueryFactory patientQueryFactory;
        private readonly IMessageSender messageSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientRepository"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="blockedAccessDelegate">The injected blocked access delegate.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        /// <param name="cacheProvider">The injected cache provider.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="patientQueryFactory">The injected patient query factory.</param>
        /// <param name="messageSender">The change feed message sender.</param>
        public PatientRepository(
            ILogger<PatientRepository> logger,
            IBlockedAccessDelegate blockedAccessDelegate,
            IAuthenticationDelegate authenticationDelegate,
            ICacheProvider cacheProvider,
            IConfiguration configuration,
            PatientQueryFactory patientQueryFactory,
            IMessageSender messageSender)
        {
            this.logger = logger;
            this.blockedAccessDelegate = blockedAccessDelegate;
            this.authenticationDelegate = authenticationDelegate;
            this.cacheProvider = cacheProvider;
            this.patientQueryFactory = patientQueryFactory;
            this.messageSender = messageSender;
            this.blockedAccessCacheTtl = configuration.GetValue($"{BlockedAccessKey}:{CacheTtlKey}", 30);
            ChangeFeedOptions? changeFeedConfiguration = configuration.GetSection(ChangeFeedOptions.ChangeFeed).Get<ChangeFeedOptions>();
            this.blockedDataSourcesChangeFeedEnabled = changeFeedConfiguration?.BlockedDataSources.Enabled ?? false;
        }

        private static ActivitySource ActivitySource { get; } = new(typeof(PatientRepository).FullName);

        /// <inheritdoc/>
        public async Task<PatientQueryResult> QueryAsync(PatientQuery query, CancellationToken ct = default)
        {
            return query switch
            {
                PatientDetailsQuery q => await this.HandlePatientDetailsQueryAsync(q, ct),

                _ => throw new NotImplementedException($"{query.GetType().FullName}"),
            };
        }

        /// <inheritdoc/>
        public async Task<bool> CanAccessDataSourceAsync(string hdid, DataSource dataSource, CancellationToken ct = default)
        {
            this.logger.LogDebug("Checking if {DataSource} can be accessed by {Hdid}", dataSource, hdid);
            IEnumerable<DataSource> blockedDataSources = await this.GetDataSourcesAsync(hdid, ct);
            return !blockedDataSources.Contains(dataSource);
        }

        /// <inheritdoc/>
        public async Task BlockAccessAsync(BlockAccessCommand command, CancellationToken ct = default)
        {
            string authenticatedUserId = this.authenticationDelegate.FetchAuthenticatedUserId() ?? UserId.DefaultUser;

            AgentAudit agentAudit = new()
            {
                Hdid = command.Hdid,
                Reason = command.Reason,
                OperationCode = AuditOperation.ChangeDataSourceAccess,
                GroupCode = AuditGroup.BlockedAccess,
                AgentUsername = this.authenticationDelegate.FetchAuthenticatedPreferredUsername() ?? authenticatedUserId,
                TransactionDateTime = DateTime.UtcNow,
                CreatedBy = authenticatedUserId,
                UpdatedBy = authenticatedUserId,
            };

            BlockedAccess blockedAccess = await this.blockedAccessDelegate.GetBlockedAccessAsync(command.Hdid, ct) ?? new()
            {
                Hdid = command.Hdid,
                CreatedBy = authenticatedUserId,
            };

            blockedAccess.UpdatedBy = authenticatedUserId;

            // Suppress IDE0305 ("Collection initialization can be simplified").
            // Although C# collection expressions (e.g., [.. enumerable]) are valid,
            // using ToList() here is more explicit and consistent with the rest of the codebase.
            // It also makes the deduplication step (Distinct + ToList) clearer to readers.
#pragma warning disable IDE0305
            IList<DataSource> sources = command.DataSources
                .Distinct()
                .ToList();

            // commit to the database if change feed is disabled; if change feed enabled, commit will happen when message sender is called
            // this is temporary and will be changed when we introduce a proper unit of work to manage transactions.
            if (sources.Count != 0)
            {
                blockedAccess.DataSources = sources;
                blockedAccess.CreatedDateTime = DateTime.UtcNow;
                blockedAccess.UpdatedDateTime = DateTime.UtcNow;
                await this.blockedAccessDelegate.UpdateBlockedAccessAsync(blockedAccess, agentAudit, !this.blockedDataSourcesChangeFeedEnabled, ct);
            }
            else
            {
                await this.blockedAccessDelegate.DeleteBlockedAccessAsync(blockedAccess, agentAudit, !this.blockedDataSourcesChangeFeedEnabled, ct);
            }

            if (this.blockedDataSourcesChangeFeedEnabled)
            {
                this.logger.LogDebug("Sending change feed notification for blocked data sources");
                IEnumerable<string> dataSourceValues = blockedAccess.DataSources.Select(ds => EnumUtility.ToEnumString(ds));
                await this.messageSender.SendAsync([new MessageEnvelope(new DataSourcesBlockedEvent(command.Hdid, dataSourceValues), command.Hdid)], ct);
            }
        }

        /// <inheritdoc/>
        public async Task<BlockedAccess?> GetBlockedAccessRecordsAsync(string hdid, CancellationToken ct = default)
        {
            return await this.blockedAccessDelegate.GetBlockedAccessAsync(hdid, ct);
        }

        /// <inheritdoc/>
        [SuppressMessage("Performance", "CA1863:Use 'CompositeFormat'", Justification = "Team decision")]
        public async Task<IEnumerable<DataSource>> GetDataSourcesAsync(string hdid, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();
            activity?.AddBaggage("CacheIdentifier", hdid);
            this.logger.LogDebug("Accessing blocked access cache");

            IEnumerable<DataSource>? dataSources = await this.cacheProvider.GetOrSetAsync(
                string.Format(CultureInfo.InvariantCulture, ICacheProvider.BlockedAccessCachePrefixKey, hdid),
                () => this.blockedAccessDelegate.GetDataSourcesAsync(hdid, ct),
                TimeSpan.FromMinutes(this.blockedAccessCacheTtl),
                ct);

            return dataSources ?? [];
        }

        private static string GetStrategy(string? hdid, PatientDetailSource source)
        {
            const string prefix = "HealthGateway.AccountDataAccess.Patient.Strategy.";
            string type = hdid != null ? "Hdid" : "Phn";
            const string suffix = "Strategy";
            return $"{prefix}{type}{source}{suffix}";
        }

        private async Task<PatientQueryResult> HandlePatientDetailsQueryAsync(PatientDetailsQuery query, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                throw new InvalidOperationException("Cancellation was requested");
            }

            using Activity? activity = ActivitySource.StartActivity();

            if (!string.IsNullOrEmpty(query.Hdid))
            {
                activity?.AddBaggage("PatientHdid", query.Hdid);
            }

            if (!string.IsNullOrEmpty(query.Phn))
            {
                activity?.AddBaggage("PatientPhn", query.Phn);
            }

            activity?.AddBaggage("UseCache", query.UseCache.ToString());
            activity?.AddBaggage("Source", EnumUtility.ToEnumString(query.Source));

            this.logger.LogDebug("Retrieving patient details");

            return (query.Hdid ?? query.Phn) == null
                ? throw new InvalidOperationException("Must specify either Hdid or Phn to query patient details")
                : await this.GetPatientAsync(query, ct: ct);
        }

        private async Task<PatientQueryResult> GetPatientAsync(PatientDetailsQuery query, bool disabledValidation = false, CancellationToken ct = default)
        {
            PatientRequest patientRequest = new(
                query.Hdid ?? query.Phn,
                query.UseCache,
                disabledValidation);

            string strategy = GetStrategy(query.Hdid, query.Source);

            PatientQueryStrategy patientQueryStrategy = this.patientQueryFactory.GetPatientQueryStrategy(strategy);
            PatientQueryContext context = new(patientQueryStrategy);
            PatientModel patient = await context.GetPatientAsync(patientRequest, ct);
            return new PatientQueryResult(patient);
        }
    }
}
