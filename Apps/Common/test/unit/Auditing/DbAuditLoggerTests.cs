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
    using System.IO;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
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
    /// DbAuditLogger's Unit Tests.
    /// </summary>
    public class DbAuditLoggerTests
    {
        private const string Hdid = "EXAMPLE-HDID";
        private const string Idir = "EXAMPLE-IDIR";

        /// <summary>
        /// PopulateWithHttpContext - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldPopulateWithHttpContext()
        {
            DefaultHttpContext ctx = new()
            {
                Connection = { RemoteIpAddress = new IPAddress(new byte[] { 127, 0, 0, 1 }) },
                User = new(new ClaimsIdentity([new Claim("hdid", Hdid), new Claim("preferred_username", Idir)])),
            };
            AuditEvent expected = new()
            {
                ApplicationSubject = Hdid,
                ApplicationType = ApplicationType.Configuration,
                ClientIp = "127.0.0.1",
                CreatedBy = Hdid,
                Trace = ctx.TraceIdentifier,
                TransactionName = @"\",
                TransactionResultCode = AuditTransactionResult.Success,
                TransactionVersion = string.Empty,
            };

            Mock<ILogger<DbAuditLogger>> logger = new();
            Mock<IWriteAuditEventDelegate> dbContext = new();
            DbAuditLogger dbAuditLogger = new(logger.Object, dbContext.Object);

            AuditEvent actual = new();
            dbAuditLogger.PopulateWithHttpContext(ctx, actual);

            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// PopulateWithHttpContext - HTTP Status 401.
        /// </summary>
        [Fact]
        public void ShouldPopulateWithHttpContextHandleStatus401()
        {
            DefaultHttpContext ctx = new()
            {
                Connection = { RemoteIpAddress = new IPAddress(new byte[] { 127, 0, 0, 1 }) },
                Response = { StatusCode = 401 },
            };
            AuditEvent expected = new()
            {
                ApplicationType = ApplicationType.Configuration,
                ClientIp = "127.0.0.1",
                Trace = ctx.TraceIdentifier,
                TransactionName = @"\",
                TransactionResultCode = AuditTransactionResult.Unauthorized,
                TransactionVersion = string.Empty,
            };

            Mock<ILogger<DbAuditLogger>> logger = new();
            Mock<IWriteAuditEventDelegate> dbContext = new();
            DbAuditLogger dbAuditLogger = new(logger.Object, dbContext.Object);

            AuditEvent actual = new();
            dbAuditLogger.PopulateWithHttpContext(ctx, actual);

            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// PopulateWithHttpContext - HTTP Status 400-level (other than 401).
        /// </summary>
        [Fact]
        public void ShouldPopulateWithHttpContextHandleOtherStatus400()
        {
            DefaultHttpContext ctx = new()
            {
                Connection = { RemoteIpAddress = new IPAddress(new byte[] { 127, 0, 0, 1 }) },
                Response = { StatusCode = 405 },
            };
            AuditEvent expected = new()
            {
                ApplicationType = ApplicationType.Configuration,
                ClientIp = "127.0.0.1",
                Trace = ctx.TraceIdentifier,
                TransactionName = @"\",
                TransactionResultCode = AuditTransactionResult.Failure,
                TransactionVersion = string.Empty,
            };

            Mock<ILogger<DbAuditLogger>> logger = new();
            Mock<IWriteAuditEventDelegate> dbContext = new();
            DbAuditLogger dbAuditLogger = new(logger.Object, dbContext.Object);

            AuditEvent actual = new();
            dbAuditLogger.PopulateWithHttpContext(ctx, actual);

            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// PopulateWithHttpContext - HTTP Status 500-level.
        /// </summary>
        [Fact]
        public void ShouldPopulateWithHttpContextHandle500()
        {
            DefaultHttpContext ctx = new()
            {
                Connection = { RemoteIpAddress = new IPAddress(new byte[] { 127, 0, 0, 1 }) },
                Response = { StatusCode = 500 },
            };
            AuditEvent expected = new()
            {
                ApplicationType = ApplicationType.Configuration,
                ClientIp = "127.0.0.1",
                Trace = ctx.TraceIdentifier,
                TransactionName = @"\",
                TransactionResultCode = AuditTransactionResult.SystemError,
                TransactionVersion = string.Empty,
            };

            Mock<ILogger<DbAuditLogger>> logger = new();
            Mock<IWriteAuditEventDelegate> dbContext = new();
            DbAuditLogger dbAuditLogger = new(logger.Object, dbContext.Object);

            AuditEvent actual = new();
            dbAuditLogger.PopulateWithHttpContext(ctx, actual);

            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// WriteAuditEvent - Happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldWriteAuditEvent()
        {
            DefaultHttpContext ctx = new()
            {
                Connection = { RemoteIpAddress = new IPAddress(new byte[] { 127, 0, 0, 1 }) },
            };
            AuditEvent expected = new()
            {
                ApplicationType = ApplicationType.Configuration,
                ClientIp = "127.0.0.1",
                Trace = ctx.TraceIdentifier,
                TransactionName = @"\",
                TransactionResultCode = AuditTransactionResult.Success,
                TransactionVersion = string.Empty,
            };

            Mock<ILogger<DbAuditLogger>> logger = new();
            Mock<IWriteAuditEventDelegate> dbContext = new();
            DbAuditLogger dbAuditLogger = new(logger.Object, dbContext.Object);

            await dbAuditLogger.WriteAuditEventAsync(expected);

            Assert.Equal(AuditTransactionResult.Success, expected.TransactionResultCode);
            logger.Verify(
                m => m.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals("Saved AuditEvent", o.ToString(), StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        /// <summary>
        /// WriteAuditEvent - handle exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldWriteAuditEventHandleException()
        {
            DefaultHttpContext ctx = new()
            {
                Connection = { RemoteIpAddress = new IPAddress(new byte[] { 127, 0, 0, 1 }) },
            };
            AuditEvent expected = new()
            {
                ApplicationType = ApplicationType.Configuration,
                ClientIp = "127.0.0.1",
                Trace = ctx.TraceIdentifier,
                TransactionName = @"\",
                TransactionResultCode = AuditTransactionResult.Success,
                TransactionVersion = string.Empty,
            };

            Mock<ILogger<DbAuditLogger>> logger = new();
            Mock<IWriteAuditEventDelegate> dbContext = new();
            dbContext.Setup(e => e.WriteAuditEventAsync(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new IOException());
            DbAuditLogger dbAuditLogger = new(logger.Object, dbContext.Object);

            await dbAuditLogger.WriteAuditEventAsync(expected);

            logger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => string.Equals("In WriteAuditEvent", o.ToString(), StringComparison.OrdinalIgnoreCase)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
