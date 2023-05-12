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
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.AccessManagement.Authentication;
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
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger<PatientRepository> logger;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IBlockedAccessDelegate blockedAccessDelegate;
        private readonly PatientQueryFactory patientQueryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientRepository"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="blockedAccessDelegate">The injected blocked access delegate.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate.</param>
        /// <param name="patientQueryFactory">The injected patient query factory.</param>
        public PatientRepository(
            IConfiguration configuration,
            ILogger<PatientRepository> logger,
            IBlockedAccessDelegate blockedAccessDelegate,
            IAuthenticationDelegate authenticationDelegate,
            PatientQueryFactory patientQueryFactory)
        {
            this.logger = logger;
            this.blockedAccessDelegate = blockedAccessDelegate;
            this.authenticationDelegate = authenticationDelegate;
            this.patientQueryFactory = patientQueryFactory;
        }

        private static ActivitySource Source { get; } = new(nameof(PatientRepository));

        /// <inheritdoc/>
        public async Task<PatientQueryResult> Query(PatientQuery query, CancellationToken ct = default)
        {
            return query switch
            {
                PatientDetailsQuery q => await this.Handle(q, ct).ConfigureAwait(true),

                _ => throw new NotImplementedException($"{query.GetType().FullName}"),
            };
        }

        /// <inheritdoc/>
        public async Task BlockAccess(BlockAccessCommand command, CancellationToken ct = default)
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

            BlockedAccess? blockedAccess = await this.blockedAccessDelegate.GetBlockedAccessAsync(command.Hdid).ConfigureAwait(true) ?? new()
            {
                Hdid = command.Hdid,
                CreatedBy = authenticatedUserId,
            };

            blockedAccess.UpdatedBy = authenticatedUserId;

            Dictionary<string, string> sources = command.DataSources.ToDictionary(x => x.ToString(), _ => "true");

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
        }

        /// <inheritdoc/>
        public async Task<BlockedAccess?> BlockedAccessQuery(string hdid, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                throw new InvalidOperationException("cancellation was requested");
            }

            return await this.blockedAccessDelegate.GetBlockedAccessAsync(hdid).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, string>> DataSourceQuery(string hdid, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                throw new InvalidOperationException("cancellation was requested");
            }

            return await this.blockedAccessDelegate.GetDataSourcesAsync(hdid).ConfigureAwait(true);
        }

        private static string GetStrategyKey(string? hdid, PatientDetailSource source)
        {
            string prefix = "HealthGateway.AccountDataAccess.Patient.Strategy.";
            string type = hdid != null ? "Hdid" : "Phn";
            string suffix = "Strategy";
            return $"{prefix}{type}{source}{suffix}";
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
            PatientRequest patientRequest = new(
                query.Hdid ?? query.Phn,
                query.UseCache,
                disabledValidation);

            string strategyKey = GetStrategyKey(query.Hdid, query.Source);
            this.logger.LogDebug("Strategy: {Strategy}", strategyKey);

            // Get the appropriate strategy from factory to query patient
            PatientQueryStrategy? patientQueryStrategy = this.patientQueryFactory.GetPatientQueryStrategy(strategyKey);

            if (patientQueryStrategy != null)
            {
                PatientQueryContext context = new(patientQueryStrategy);
                PatientModel? patient = await context.GetPatientAsync(patientRequest).ConfigureAwait(true);
                return new PatientQueryResult(new[] { patient! });
            }

            throw new InvalidOperationException($"Unable to retrieve patient query strategy due to invalid key: {strategyKey}");
        }
    }
}
