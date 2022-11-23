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
namespace HealthGateway.CommonTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.CommonTests.Utils;
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

        /// <summary>
        /// CreateBroadcastAsync.
        /// </summary>
        [Fact]
        public void ShouldCreateBroadcast()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, false);

            // Act
            RequestResult<Broadcast> actualResult = service.CreateBroadcastAsync(new Broadcast()).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.TotalResultCount == 1);
            Assert.Equal(CategoryName, actualResult.ResourcePayload.CategoryName);
        }

        /// <summary>
        /// CreateBroadcastAsync - api throws exception.
        /// </summary>
        [Fact]
        public void CreateBroadcastShouldThrowsException()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, true);

            // Act
            RequestResult<Broadcast> actualResult = service.CreateBroadcastAsync(new Broadcast()).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetBroadcastsAsync - api returns one row.
        /// </summary>
        [Fact]
        public void ShouldGetBroadcasts()
        {
            // Arrange
            Guid expectedId = Guid.NewGuid();
            IBroadcastService service = GetBroadcastService(expectedId, false);

            // Act
            RequestResult<IEnumerable<Broadcast>> actualResult = service.GetBroadcastsAsync().Result;

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
        [Fact]
        public void ShouldGetBroadcastsNoRowsReturned()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, false);

            // Act
            RequestResult<IEnumerable<Broadcast>> actualResult = service.GetBroadcastsAsync().Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(!actualResult.ResourcePayload.Any());
            Assert.True(actualResult.TotalResultCount == 0);
        }

        /// <summary>
        /// GetBroadcastsAsync - api throws exception.
        /// </summary>
        [Fact]
        public void GetBroadcastsShouldThrowsException()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, true);

            // Act
            RequestResult<IEnumerable<Broadcast>> actualResult = service.GetBroadcastsAsync().Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// UpdateBroadcastAsync.
        /// </summary>
        [Fact]
        public void ShouldUpdateBroadcast()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
            };
            IBroadcastService service = GetBroadcastService(expectedId, false);

            // Act
            RequestResult<Broadcast> actualResult = service.UpdateBroadcastAsync(broadcast).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.TotalResultCount == 1);
            Assert.Equal(expectedId, actualResult.ResourcePayload.Id);
        }

        /// <summary>
        /// CreateBroadcastAsync - api throws exception.
        /// </summary>
        [Fact]
        public void UpdateBroadcastShouldThrowsException()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
            };
            IBroadcastService service = GetBroadcastService(null, true);

            // Act
            RequestResult<Broadcast> actualResult = service.UpdateBroadcastAsync(broadcast).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// DeleteBroadcastAsync.
        /// </summary>
        [Fact]
        public void ShouldDeleteBroadcast()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
            };
            IBroadcastService service = GetBroadcastService(expectedId, false);

            // Act
            RequestResult<Broadcast> actualResult = service.DeleteBroadcastAsync(broadcast).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.TotalResultCount == 1);
            Assert.Equal(expectedId, actualResult.ResourcePayload.Id);
        }

        /// <summary>
        /// DeleteBroadcastAsync - api throws exception.
        /// </summary>
        [Fact]
        public void DeleteBroadcastShouldThrowException()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
            };
            IBroadcastService service = GetBroadcastService(null, true);

            // Act
            RequestResult<Broadcast> actualResult = service.DeleteBroadcastAsync(broadcast).Result;

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

            List<BroadcastResponse> apiGetResponse = new();
            if (id != null)
            {
                apiGetResponse.Add(GetApiResponse(id));
            }

            Mock<ISystemBroadcastApi> mockSystemBroadcastApi = new();

            if (!throwException)
            {
                mockSystemBroadcastApi.Setup(s => s.GetBroadcastsAsync()).ReturnsAsync(apiGetResponse);
                mockSystemBroadcastApi.Setup(s => s.UpdateBroadcastAsync(It.IsAny<string>(), It.IsAny<BroadcastRequest>())).ReturnsAsync(response);
                mockSystemBroadcastApi.Setup(s => s.CreateBroadcastAsync(It.IsAny<BroadcastRequest>())).ReturnsAsync(response);
                mockSystemBroadcastApi.Setup(s => s.DeleteBroadcastAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            }
            else
            {
                mockSystemBroadcastApi.Setup(s => s.GetBroadcastsAsync()).ThrowsAsync(new HttpRequestException(string.Empty));
                mockSystemBroadcastApi.Setup(s => s.UpdateBroadcastAsync(It.IsAny<string>(), It.IsAny<BroadcastRequest>())).ThrowsAsync(new HttpRequestException(string.Empty));
                mockSystemBroadcastApi.Setup(s => s.CreateBroadcastAsync(It.IsAny<BroadcastRequest>())).ThrowsAsync(new HttpRequestException(string.Empty));
                mockSystemBroadcastApi.Setup(s => s.DeleteBroadcastAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException(string.Empty));
            }

            return new BroadcastService(
                new Mock<ILogger<BroadcastService>>().Object,
                mockSystemBroadcastApi.Object,
                MapperUtil.InitializeAutoMapper());
        }
    }
}
