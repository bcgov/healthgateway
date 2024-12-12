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
namespace HealthGateway.AccountDataAccess.Audit
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Handle audit data.
    /// </summary>
    /// <param name="agentAuditDelegate">The injected agent audit delegate.</param>
    public class AuditRepository(IAgentAuditDelegate agentAuditDelegate) : IAuditRepository
    {
        private static ActivitySource ActivitySource { get; } = new(typeof(AuditRepository).FullName);

        /// <inheritdoc/>
        public async Task<IEnumerable<AgentAudit>> HandleAsync(AgentAuditQuery query, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();
            activity?.AddBaggage("AuditHdid", query.Hdid);
            if (query.Group != null)
            {
                activity?.AddBaggage("AuditGroup", query.Group.ToString());
            }

            return await agentAuditDelegate.GetAgentAuditsAsync(query.Hdid, query.Group, ct);
        }
    }
}
