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
namespace HealthGateway.Admin.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the CommunicationService class.
    /// </summary>
    public class CommunicationServiceTests
    {
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IAdminServerMappingService MappingService = new AdminServerMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        /// <summary>
        /// Add communication.
        /// </summary>
        /// <param name="dbErrorExists">Value indicating whether a db error exists or not.</param>
        /// <param name="daysToAddToEffectiveDate">The number of days to add to effective date.</param>
        /// <param name="daysToAddToExpiryDate">The number of days to add to expiry date.</param>
        /// <param name="expectedResultType">The expected result type.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false, 0, 1, ResultType.Success)]
        [InlineData(false, -1, 0, ResultType.Success)]
        [InlineData(false, 0, 0, ResultType.Error)]
        [InlineData(false, 0, -1, ResultType.Error)]
        [InlineData(false, -1, -5, ResultType.Error)]
        [InlineData(true, 0, 1, ResultType.Error)]
        public async Task ShouldAddAsync(bool dbErrorExists, int daysToAddToEffectiveDate, int daysToAddToExpiryDate, ResultType expectedResultType)
        {
            DateTime now = DateTime.UtcNow;

            // Arrange
            Communication communication = new()
            {
                CommunicationStatusCode = CommunicationStatus.New,
                CommunicationTypeCode = CommunicationType.InApp,
                Text = "Test",
                EffectiveDateTime = now.AddDays(daysToAddToEffectiveDate),
                ExpiryDateTime = now.AddDays(daysToAddToExpiryDate),
            };

            Database.Models.Communication dbCommunication = new()
            {
                CommunicationStatusCode = communication.CommunicationStatusCode,
                CommunicationTypeCode = communication.CommunicationTypeCode,
                Text = communication.Text,
                EffectiveDateTime = communication.EffectiveDateTime,
                ExpiryDateTime = communication.ExpiryDateTime,
            };

            DbResult<Database.Models.Communication> dbResult = new()
            {
                Status = dbErrorExists ? DbStatusCode.Error : DbStatusCode.Created,
                Payload = dbCommunication,
            };

            RequestResult<Communication> expected = new()
            {
                ResultStatus = expectedResultType,
                ResourcePayload = expectedResultType == ResultType.Success || dbErrorExists ? communication : null,
                ResultError = expectedResultType == ResultType.Error
                    ? new RequestResultError
                    {
                        ResultMessage = !dbErrorExists ? "Effective Date should be before Expiry Date." : string.Empty,
                        ErrorCode = dbErrorExists ? ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) : ErrorTranslator.InternalError(ErrorType.InvalidState),
                    }
                    : null,
            };

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.AddAsync(It.IsAny<Database.Models.Communication>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(dbResult);
            ICommunicationService service = GetCommunicationService(communicationDelegateMock: communicationDelegateMock);

            // Act
            RequestResult<Communication> actual = await service.AddAsync(communication);

            // Assert
            expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Delete communication.
        /// </summary>
        /// <param name="communicationStatus">The communication status for the Communication to be deleted in the service.</param>
        /// <param name="dbStatus">The db status code for the communication to be deleted in the delegate.</param>
        /// <param name="expectedResultType">The expected result type.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(CommunicationStatus.Processed, DbStatusCode.Deleted, ResultType.Error)]
        [InlineData(CommunicationStatus.New, DbStatusCode.Error, ResultType.Error)]
        [InlineData(CommunicationStatus.New, DbStatusCode.Deleted, ResultType.Success)]
        [InlineData(CommunicationStatus.Error, DbStatusCode.Deleted, ResultType.Success)]
        [InlineData(CommunicationStatus.Draft, DbStatusCode.Deleted, ResultType.Success)]
        [InlineData(CommunicationStatus.Pending, DbStatusCode.Deleted, ResultType.Success)]
        [InlineData(CommunicationStatus.Processing, DbStatusCode.Deleted, ResultType.Success)]
        public async Task ShouldDeleteAsync(CommunicationStatus communicationStatus, DbStatusCode dbStatus, ResultType expectedResultType)
        {
            // Arrange
            Communication communication = new()
            {
                Id = Guid.NewGuid(),
                CommunicationStatusCode = communicationStatus,
                CommunicationTypeCode = CommunicationType.InApp,
                Text = "Test",
            };

            DbResult<Database.Models.Communication> dbResult = new()
            {
                Status = dbStatus,
                Payload = new()
                {
                    Id = communication.Id,
                    CommunicationStatusCode = communicationStatus,
                    CommunicationTypeCode = communication.CommunicationTypeCode,
                    Text = communication.Text,
                },
            };

            RequestResult<Communication> expected = new()
            {
                ResultStatus = expectedResultType,
                ResourcePayload = communicationStatus != CommunicationStatus.Processed ? communication : null,
                ResultError = expectedResultType == ResultType.Error
                    ? new RequestResultError
                    {
                        ResultMessage = communicationStatus == CommunicationStatus.Processed ? "Processed communication can't be deleted.." : string.Empty,
                        ErrorCode = communicationStatus == CommunicationStatus.Processed
                            ? ErrorTranslator.InternalError(ErrorType.InvalidState)
                            : ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    }
                    : null,
            };

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.DeleteAsync(It.IsAny<Database.Models.Communication>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(dbResult);
            ICommunicationService service = GetCommunicationService(communicationDelegateMock: communicationDelegateMock);

            // Act
            RequestResult<Communication> actual = await service.DeleteAsync(communication);

            // Assert
            if (expectedResultType == ResultType.Success)
            {
                expected.ShouldDeepEqual(actual);
            }
            else
            {
                Assert.Equal(expectedResultType, actual.ResultStatus);
            }
        }

        /// <summary>
        /// Get all communications.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAllAsync()
        {
            // Arrange
            Database.Models.Communication communication = new()
            {
                Id = Guid.NewGuid(),
                CommunicationStatusCode = CommunicationStatus.New,
                CommunicationTypeCode = CommunicationType.InApp,
                Text = "Test",
            };

            RequestResult<IEnumerable<Communication>> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload =
                [
                    new()
                    {
                        Id = communication.Id,
                        CommunicationStatusCode = communication.CommunicationStatusCode,
                        CommunicationTypeCode = communication.CommunicationTypeCode,
                        Text = communication.Text,
                    },
                ],
            };

            IList<Database.Models.Communication> communications = [communication];
            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(communications);
            ICommunicationService service = GetCommunicationService(communicationDelegateMock: communicationDelegateMock);

            // Act
            RequestResult<IEnumerable<Communication>> actual = await service.GetAllAsync();

            // Assert
            expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Update communication.
        /// </summary>
        /// <param name="dbErrorExists">Value indicating whether a db error exists or not.</param>
        /// <param name="daysToAddToEffectiveDate">The number of days to add to effective date.</param>
        /// <param name="daysToAddToExpiryDate">The number of days to add to expiry date.</param>
        /// <param name="expectedResultType">The expected result type.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false, 0, 1, ResultType.Success)]
        [InlineData(false, -1, 0, ResultType.Success)]
        [InlineData(false, 0, 0, ResultType.Error)]
        [InlineData(false, 0, -1, ResultType.Error)]
        [InlineData(false, -1, -5, ResultType.Error)]
        [InlineData(true, 0, 1, ResultType.Error)]
        public async Task ShouldUpdateAsync(bool dbErrorExists, int daysToAddToEffectiveDate, int daysToAddToExpiryDate, ResultType expectedResultType)
        {
            DateTime now = DateTime.UtcNow;

            // Arrange
            Communication communication = new()
            {
                Id = Guid.NewGuid(),
                CommunicationStatusCode = CommunicationStatus.New,
                CommunicationTypeCode = CommunicationType.InApp,
                Text = "Test",
                EffectiveDateTime = now.AddDays(daysToAddToEffectiveDate),
                ExpiryDateTime = now.AddDays(daysToAddToExpiryDate),
            };

            Database.Models.Communication dbCommunication = new()
            {
                Id = communication.Id,
                CommunicationStatusCode = communication.CommunicationStatusCode,
                CommunicationTypeCode = communication.CommunicationTypeCode,
                Text = communication.Text,
                EffectiveDateTime = communication.EffectiveDateTime,
                ExpiryDateTime = communication.ExpiryDateTime,
            };

            DbResult<Database.Models.Communication> dbResult = new()
            {
                Status = dbErrorExists ? DbStatusCode.Error : DbStatusCode.Updated,
                Payload = dbCommunication,
            };

            RequestResult<Communication> expected = new()
            {
                ResultStatus = expectedResultType,
                ResourcePayload = expectedResultType == ResultType.Success || dbErrorExists ? communication : null,
                ResultError = expectedResultType == ResultType.Error
                    ? new RequestResultError
                    {
                        ResultMessage = !dbErrorExists ? "Effective Date should be before Expiry Date." : string.Empty,
                        ErrorCode = dbErrorExists ? ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) : ErrorTranslator.InternalError(ErrorType.InvalidState),
                    }
                    : null,
            };

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.UpdateAsync(It.IsAny<Database.Models.Communication>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(dbResult);
            ICommunicationService service = GetCommunicationService(communicationDelegateMock: communicationDelegateMock);

            // Act
            RequestResult<Communication> actual = await service.UpdateAsync(communication);

            // Assert
            expected.ShouldDeepEqual(actual);
        }

        private static ICommunicationService GetCommunicationService(Mock<ICommunicationDelegate> communicationDelegateMock)
        {
            return new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                MappingService);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
                { "EnabledRoles", "[ \"AdminUser\", \"AdminReviewer\", \"SupportUser\" ]" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }
    }
}
