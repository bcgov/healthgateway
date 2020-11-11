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
namespace HealthGateway.Database.Delegates
{
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBUserDelegateStatementDelegate : IUserDelegateStatementDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBUserDelegateStatementDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBUserDelegateStatementDelegate(
            ILogger<DBUserDelegateStatementDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<UserDelegateStatement> Insert(UserDelegateStatement userDelegateStatement, bool commit = true)
        {
            this.logger.LogTrace($"Inserting user delegate statement to DB... {JsonSerializer.Serialize(userDelegateStatement)}");
            DBResult<UserDelegateStatement> result = new DBResult<UserDelegateStatement>()
            {
                Payload = userDelegateStatement,
                Status = DBStatusCode.Deferred,
            };

            this.dbContext.Add<UserDelegateStatement>(userDelegateStatement);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError($"Error inserting user delegate statement to DB with exception ({e.ToString()})");
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogTrace($"Finished inserting user statement delegate to DB... {JsonSerializer.Serialize(result)}");
            return result;
        }
    }
}
