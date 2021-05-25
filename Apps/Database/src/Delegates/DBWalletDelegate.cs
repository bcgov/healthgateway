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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class DBWalletDelegate : IWalletDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBWalletDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBWalletDelegate(
            ILogger<DBFeedbackDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<WalletConnection> GetConnection(Guid id)
        {
            DBResult<WalletConnection> result = new ()
            {
                Status = DBStatusCode.NotFound,
            };
            WalletConnection? connection = this.dbContext.WalletConnection
                                    .Where(p => p.Id == id)
                                    .FirstOrDefault();
            if (connection != null)
            {
                result.Status = DBStatusCode.Read;
                result.Payload = connection;
            }

            return result;
        }

        /// <inheritdoc />
        public DBResult<WalletConnection> GetConnection(string userProfileId)
        {
            DBResult<WalletConnection> result = new ()
            {
                Status = DBStatusCode.NotFound,
            };
            WalletConnection? connection = this.dbContext.WalletConnection
                                    .Where(p => p.UserProfileId == userProfileId &&
                                                p.Status != WalletConnectionStatus.Disconnected)
                                    .FirstOrDefault();
            if (connection != null)
            {
                result.Status = DBStatusCode.Read;
                result.Payload = connection;
            }

            return result;
        }

        /// <inheritdoc />
        public DBResult<WalletCredential> GetCredential(string exchangeId)
        {
            DBResult<WalletCredential> result = new ()
            {
                Status = DBStatusCode.NotFound,
            };
            WalletCredential? credential = this.dbContext.WalletCredential
                                    .Where(p => p.ExchangeId == exchangeId)
                                    .FirstOrDefault();
            if (credential != null)
            {
                result.Status = DBStatusCode.Read;
                result.Payload = credential;
            }

            return result;
        }

        /// <inheritdoc />
        public DBResult<WalletConnection> InsertConnection(WalletConnection connection, bool commit = true)
        {
            this.logger.LogTrace($"Inserting rating to DB... {JsonSerializer.Serialize(connection)}");
            DBResult<WalletConnection> result = new DBResult<WalletConnection>();
            this.dbContext.Add<WalletConnection>(connection);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished inserting Wallet Connection to DB... {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<WalletConnection> UpdateConnection(WalletConnection connection, bool commit = true)
        {
            this.logger.LogTrace($"Updating Wallet Connection in DB...");
            DBResult<WalletConnection> result = new DBResult<WalletConnection>()
            {
                Payload = connection,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.WalletConnection.Update(connection);
            this.dbContext.Entry(connection).Property(p => p.UserProfileId).IsModified = false;
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished updating Wallet Connection in DB");
            return result;
        }

        /// <inheritdoc />
        public DBResult<WalletCredential> InsertCredential(WalletCredential credential, bool commit = true)
        {
            this.logger.LogTrace($"Inserting rating to DB... {JsonSerializer.Serialize(credential)}");
            DBResult<WalletCredential> result = new DBResult<WalletCredential>();
            this.dbContext.Add<WalletCredential>(credential);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished inserting Wallet Credential to DB... {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<WalletCredential> UpdateCredential(WalletCredential credential, bool commit = true)
        {
            this.logger.LogTrace($"Updating Wallet Credential in DB...");
            DBResult<WalletCredential> result = new DBResult<WalletCredential>()
            {
                Payload = credential,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.WalletCredential.Update(credential);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished updating Wallet Connection in DB");
            return result;
        }
    }
}
