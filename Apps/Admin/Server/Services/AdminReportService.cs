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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;

    /// <inheritdoc/>
    public class AdminReportService : IAdminReportService
    {
        private readonly IDelegationDelegate delegationDelegate;
        private readonly IBlockedAccessDelegate blockedAccessDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminReportService"/> class.
        /// </summary>
        /// <param name="delegationDelegate">The ResourceDelegate delegate to communicate with DB.</param>
        /// <param name="blockedAccessDelegate">The BlockedAccess delegate to communicate with DB.</param>
        public AdminReportService(IDelegationDelegate delegationDelegate, IBlockedAccessDelegate blockedAccessDelegate)
        {
            this.delegationDelegate = delegationDelegate;
            this.blockedAccessDelegate = blockedAccessDelegate;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> GetProtectedDependentsReportAsync(CancellationToken ct)
        {
            return this.delegationDelegate.GetProtectedDependentHdidsAsync(ct);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<BlockedAccess>> GetBlockedAccessReportAsync(CancellationToken ct)
        {
            return this.blockedAccessDelegate.GetAllAsync(ct);
        }
    }
}
