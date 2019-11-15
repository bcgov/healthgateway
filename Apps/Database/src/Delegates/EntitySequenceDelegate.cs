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
    using HealthGateway.Database.Context;
    using Npgsql;
    using NpgsqlTypes;

    /// <summary>
    /// Entity framework based implementation of the sequence delegate.
    /// </summary>
    public class EntitySequenceDelegate : ISequenceDelegate
    {
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySequenceDelegate"/> class.
        /// </summary>
        /// <param name="dbContext">The context to be used when accessing the database context.</param>
        public EntitySequenceDelegate(GatewayDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets the next sequence number for the given sequence name.
        /// </summary>
        /// <param name="sequenceName">The sequence name.</param>
        /// <returns>The next sequence value.</returns>
        public long NextValueForSequence(string sequenceName)
        {
            NpgsqlParameter result = new NpgsqlParameter("@result", NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.Output,
            };
            this.dbContext.ExecuteSqlCommand($"SELECT nextval('{sequenceName}')", result);

            // code below is to be used when updating to EF 3
            // ctx.Database.ExecuteSqlRaw($"SELECT nextval('{seq}')", result);
            return (long)result.Value;
        }
    }
}
