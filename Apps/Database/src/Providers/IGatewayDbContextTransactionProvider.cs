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

    /// <summary>
    /// Provides an abstraction over <see cref="GatewayDbContext"/> transaction and persistence operations.
    /// </summary>
    /// <remarks>
    /// This interface is used to decouple services from direct dependency on Entity Framework Core,
    /// enabling easier unit testing by allowing transaction and save operations to be mocked.
    /// </remarks>
    public interface IGatewayDbContextTransactionProvider
    {
        /// <summary>
        /// Begins a new database transaction asynchronously.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation, containing the created <see cref="IDbContextTransaction"/>.</returns>
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

        /// <summary>
        /// Persists all changes made in the current context to the database asynchronously.
        /// </summary>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing the number of state entries written to the
        /// database.
        /// </returns>
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
