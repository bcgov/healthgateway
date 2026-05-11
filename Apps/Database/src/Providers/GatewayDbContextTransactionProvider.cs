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
namespace HealthGateway.Database.Providers
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using Microsoft.EntityFrameworkCore.Storage;

    /// <inheritdoc/>
    /// <param name="dbContext">The <see cref="GatewayDbContext"/> used to manage transactions and persist changes.</param>
    public class GatewayDbContextTransactionProvider(GatewayDbContext dbContext) : IGatewayDbContextTransactionProvider
    {
        /// <inheritdoc/>
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            return dbContext.Database.BeginTransactionAsync(ct);
        }

        /// <inheritdoc/>
        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return dbContext.SaveChangesAsync(ct);
        }
    }
}
