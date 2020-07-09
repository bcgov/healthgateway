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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBUserPreferenceDelegate : IUserPreferenceDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBUserPreferenceDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBUserPreferenceDelegate(
            ILogger<DBProfileDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<UserPreference> InsertUserPreference(UserPreference preference)
        {
            // Todo: Tiago to implement code here
            return new DBResult<UserPreference>()
            {
                Payload = preference,
                Status = DBStatusCode.Created,
            };
        }

        /// <inheritdoc />
        public DBResult<UserPreference> GetUserPreference(string hdid)
        {
            // Todo: Tiago to implement code here
            return new DBResult<UserPreference>()
            {
                Payload = new UserPreference()
                {
                    HdId = hdid,
                    DismissedMyNotePopover = false,
                    Id = Guid.NewGuid(),
                },
                Status = DBStatusCode.Read,
            };
        }
    }
}