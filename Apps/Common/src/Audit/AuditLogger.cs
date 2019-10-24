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
namespace HealthGateway.Common.Audit
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using HealthGateway.Common.Database.Models;

    /// <summary>
    /// The Authorization service
    /// </summary>
    public class AuditLogger : IAuditLogger
    {
        private ILogger<IAuditLogger> logger;
        private IConfiguration configuration;

        private AuditDbContext dbContext;

        public AuditLogger(ILogger<IAuditLogger> logger, AuditDbContext dbContext, IConfiguration config)
        {
            this.logger = logger;
            this.configuration = config;
            this.dbContext = dbContext;
        }
        public void WriteAuditEvent(AuditEvent auditEvent)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this.logger.LogDebug(@"Begin WriteAuditEvent(auditEvent)");
#pragma warning restore CA1303 // Do not pass literals as localized parameters

            this.dbContext.SaveChanges();
            logger.LogInformation(@"Saved AuditEvent");
        }
}
}