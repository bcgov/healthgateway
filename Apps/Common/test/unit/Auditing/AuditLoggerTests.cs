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
    using DeepEqual.Syntax;
    using HealthGateway.Common.Auditing;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// AuditLogger's Unit Tests.
    /// </summary>
    public class AuditLoggerTests
    {
        /// <summary>
        /// PopulateWithHttpContext - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldPopulateWithHttpContext()
        {
            DefaultHttpContext ctx = new();
            ctx.Connection.RemoteIpAddress = new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 });
            AuditEvent expected = new()
            {
                ApplicationType = ApplicationType.Configuration,
                ClientIP = "127.0.0.1",
                Trace = ctx.TraceIdentifier,
                TransactionName = @"\",
                TransactionResultCode = AuditTransactionResult.Success,
                TransactionVersion = string.Empty,
            };

            Mock<ILogger<IAuditLogger>> logger = new();
            Mock<IWriteAuditEventDelegate> dbContext = new();
            AuditLogger auditLogger = new(logger.Object, dbContext.Object);

            AuditEvent actual = new();
            auditLogger.PopulateWithHttpContext(ctx, actual);

            expected.ShouldDeepEqual(actual);
        }
    }
}
