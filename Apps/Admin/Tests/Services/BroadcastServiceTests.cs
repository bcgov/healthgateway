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
namespace HealthGateway.Admin.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Admin.Tests.Utils;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// BroadcastService's Unit Tests.
    /// </summary>
    public class BroadcastServiceTests
    {
        private const string CategoryName = "Test Category Name";
        private const string ThrownExceptionMessage = "Error with HTTP Request";

        private static readonly ICommonMappingService CommonMappingService = new CommonMappingService(MapperUtil.InitializeAutoMapper());
        private static readonly DateTime Today = new(2022, 12, 21, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// CreateBroadcastAsync.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldCreateBroadcast()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, false);
            Broadcast broadcast = new()
            {
                ScheduledDateUtc = Today,
                ExpirationDateUtc = Today.AddDays(1),
            };

            // Act
            RequestResult<Broadcast> actualResult = await service.CreateBroadcastAsync(broadcast);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.TotalResultCount == 1);
            Assert.Equal(CategoryName, actualResult.ResourcePayload.CategoryName);
        }

        /// <summary>
        /// CreateBroadcastAsync fails effective expiry date validation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task CreateBroadcastFailDateValidation()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, false);
            Broadcast broadcast = new()
            {
                ScheduledDateUtc = Today,
                ExpirationDateUtc = Today,
            };

            // Act
            RequestResult<Broadcast> actualResult = await service.CreateBroadcastAsync(broadcast);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// CreateBroadcastAsync - api throws exception.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task CreateBroadcastShouldThrowsException()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, true);
            Broadcast broadcast = new()
            {
                ScheduledDateUtc = Today,
                ExpirationDateUtc = Today.AddDays(1),
            };

            // Act
            RequestResult<Broadcast> actualResult = await service.CreateBroadcastAsync(broadcast);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetBroadcastsAsync - api returns one row.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetBroadcasts()
        {
            // Arrange
            Guid expectedId = Guid.NewGuid();
            IBroadcastService service = GetBroadcastService(expectedId, false);

            // Act
            RequestResult<IEnumerable<Broadcast>> actualResult = await service.GetBroadcastsAsync();

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.ResourcePayload.Count() == 1);
            Assert.True(actualResult.TotalResultCount == 1);
            Assert.Equal(expectedId, actualResult.ResourcePayload.First().Id);
        }

        /// <summary>
        /// GetBroadcastsAsync - returns no rows.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldGetBroadcastsNoRowsReturned()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, false);

            // Act
            RequestResult<IEnumerable<Broadcast>> actualResult = await service.GetBroadcastsAsync();

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(!actualResult.ResourcePayload.Any());
            Assert.True(actualResult.TotalResultCount == 0);
        }

        /// <summary>
        /// GetBroadcastsAsync - api throws exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetBroadcastsShouldThrowsException()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, true);

            // Act
            RequestResult<IEnumerable<Broadcast>> actualResult = await service.GetBroadcastsAsync();

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// UpdateBroadcastAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateBroadcast()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
                ScheduledDateUtc = Today,
                ExpirationDateUtc = Today.AddDays(1),
            };
            IBroadcastService service = GetBroadcastService(expectedId, false);

            // Act
            RequestResult<Broadcast> actualResult = await service.UpdateBroadcastAsync(broadcast);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.TotalResultCount == 1);
            Assert.Equal(expectedId, actualResult.ResourcePayload.Id);
        }

        /// <summary>
        /// UpdateBroadcastAsync fails effective expiry date validation.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateBroadcastFailsDateValidation()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
                ScheduledDateUtc = Today,
                ExpirationDateUtc = Today,
            };
            IBroadcastService service = GetBroadcastService(expectedId, false);

            // Act
            RequestResult<Broadcast> actualResult = await service.UpdateBroadcastAsync(broadcast);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// CreateBroadcastAsync - api throws exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateBroadcastShouldThrowsException()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
                ScheduledDateUtc = Today,
                ExpirationDateUtc = Today.AddDays(1),
            };
            IBroadcastService service = GetBroadcastService(null, true);

            // Act
            RequestResult<Broadcast> actualResult = await service.UpdateBroadcastAsync(broadcast);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// DeleteBroadcastAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteBroadcast()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
            };
            IBroadcastService service = GetBroadcastService(expectedId, false);

            // Act
            RequestResult<Broadcast> actualResult = await service.DeleteBroadcastAsync(broadcast);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.TotalResultCount == 1);
            Assert.Equal(expectedId, actualResult.ResourcePayload.Id);
        }

        /// <summary>
        /// DeleteBroadcastAsync - api throws exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteBroadcastShouldThrowException()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
            };
            IBroadcastService service = GetBroadcastService(null, true);

            // Act
            RequestResult<Broadcast> actualResult = await service.DeleteBroadcastAsync(broadcast);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        private static BroadcastResponse GetApiResponse(Guid? id)
        {
            BroadcastResponse response = new()
            {
                Id = id ?? Guid.NewGuid(),
                CategoryName = CategoryName,
                ModifiedDateUtc = DateTime.UtcNow,
                CreationDateUtc = DateTime.UtcNow,
            };
            return response;
        }

        private static IBroadcastService GetBroadcastService(Guid? id, bool throwException)
        {
            BroadcastResponse response = GetApiResponse(id);

            List<BroadcastResponse> apiGetResponse = [];
            if (id != null)
            {
                apiGetResponse.Add(GetApiResponse(id));
            }

            Mock<ISystemBroadcastApi> mockSystemBroadcastApi = new();

            if (!throwException)
            {
                mockSystemBroadcastApi.Setup(s => s.GetBroadcastsAsync(default)).ReturnsAsync(apiGetResponse);
                mockSystemBroadcastApi.Setup(s => s.UpdateBroadcastAsync(It.IsAny<string>(), It.IsAny<BroadcastRequest>(), default)).ReturnsAsync(response);
                mockSystemBroadcastApi.Setup(s => s.CreateBroadcastAsync(It.IsAny<BroadcastRequest>(), default)).ReturnsAsync(response);
                mockSystemBroadcastApi.Setup(s => s.DeleteBroadcastAsync(It.IsAny<string>(), default)).Returns(Task.CompletedTask);
            }
            else
            {
                mockSystemBroadcastApi.Setup(s => s.GetBroadcastsAsync(default)).ThrowsAsync(new HttpRequestException(string.Empty));
                mockSystemBroadcastApi.Setup(s => s.UpdateBroadcastAsync(It.IsAny<string>(), It.IsAny<BroadcastRequest>(), default)).ThrowsAsync(new HttpRequestException(string.Empty));
                mockSystemBroadcastApi.Setup(s => s.CreateBroadcastAsync(It.IsAny<BroadcastRequest>(), default)).ThrowsAsync(new HttpRequestException(string.Empty));
                mockSystemBroadcastApi.Setup(s => s.DeleteBroadcastAsync(It.IsAny<string>(), default)).ThrowsAsync(new HttpRequestException(string.Empty));
            }

            return new BroadcastService(
                new Mock<ILogger<BroadcastService>>().Object,
                mockSystemBroadcastApi.Object,
                CommonMappingService);
        }
    }
}
