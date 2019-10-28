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
namespace HealthGateway.CommonTests.Auditing
{
    using System;
    using System.Security.Principal;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Auditing;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class AuditLogger_Test
    {
        [Fact]
        public void ShouldPopulateWithHttpContext()
        {
            DefaultHttpContext ctx = new DefaultHttpContext();
            ctx.Connection.RemoteIpAddress = new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 });
            AuditEvent expected = new AuditEvent()
            {
                ApplicationType = AuditApplicationType.Configuration,
                ClientIP = "127.0.0.1",
                Trace = ctx.TraceIdentifier,
                TransacationName = "",
                TransactionResultType = AuditTransactionResultType.Success,
                TransactionVersion = ""
            };

            Mock<ILogger<IAuditLogger>> logger = new Mock<ILogger<IAuditLogger>>();
            Mock<IAuditDbContext> dbContext = new Mock<IAuditDbContext>();
            Mock<IConfiguration> config = new Mock<IConfiguration>();
            AuditLogger auditLogger = new AuditLogger(logger.Object, dbContext.Object, config.Object);

            AuditEvent actual = new AuditEvent();
            auditLogger.PopulateWithHttpContext(ctx, actual);

            Assert.True(expected.IsDeepEqual(actual));
        }
    }
}
