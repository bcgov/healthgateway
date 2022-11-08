//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Database.Delegates
{
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Database.Context;
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Entity framework based implementation of the sequence delegate.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DbSequenceDelegate : ISequenceDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbSequenceDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database context.</param>
        public DbSequenceDelegate(
            ILogger<DbSequenceDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets the next sequence number for the given sequence name.
        /// </summary>
        /// <param name="sequenceName">The sequence name.</param>
        /// <returns>The next sequence value.</returns>
        public long GetNextValueForSequence(string sequenceName)
        {
            this.logger.LogTrace("Getting next value for sequence from database... {SequenceName}", sequenceName);
            NpgsqlParameter result = new("@result", NpgsqlDbType.Integer)
            {
                Direction = ParameterDirection.Output,
            };
            this.dbContext.ExecuteSqlCommand($"SELECT nextval('{sequenceName}')", result);
            this.logger.LogDebug("Finished getting next value for sequence from database. {Result}", result.Value);

            if (result.Value == null)
            {
                throw new NpgsqlException("Sequence could not be retrieved from database (result is null).");
            }

            return (long)result.Value;
        }
    }
}
