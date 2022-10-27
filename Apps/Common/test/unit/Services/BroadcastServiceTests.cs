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
    using System.Net;
    using System.Net.Http;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.CommonTests.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// BroadcastService's Unit Tests.
    /// </summary>
    public class BroadcastServiceTests
    {
        private const string CategoryName = "Test Category Name";
        private const string UnexpectedErrorMessage = "An unexpected error occurred while processing external call";
        private const string ThrownExceptionMessage = "Error with HTTP Request";

        /// <summary>
        /// CreateBroadcast.
        /// </summary>
        [Fact]
        public void ShouldCreateBroadcast()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, HttpStatusCode.OK, false);

            // Act
            RequestResult<Broadcast> actualResult = service.CreateBroadcastAsync(new Broadcast()).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.TotalResultCount == 1);
            Assert.Equal(CategoryName, actualResult.ResourcePayload.CategoryName);
        }

        /// <summary>
        /// CreateBroadcast - api returns error.
        /// </summary>
        [Fact]
        public void CreateBroadcastShouldReturnsError()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, HttpStatusCode.InternalServerError, false);

            // Act
            RequestResult<Broadcast> actualResult = service.CreateBroadcastAsync(new Broadcast()).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(UnexpectedErrorMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// CreateBroadcast - api throws exception.
        /// </summary>
        [Fact]
        public void CreateBroadcastShouldThrowsException()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, null, true);

            // Act
            RequestResult<Broadcast> actualResult = service.CreateBroadcastAsync(new Broadcast()).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetBroadcasts - api returns one row.
        /// </summary>
        [Fact]
        public void ShouldGetBroadcasts()
        {
            // Arrange
            Guid expectedId = Guid.NewGuid();
            IBroadcastService service = GetBroadcastService(expectedId, HttpStatusCode.OK, false);

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
        /// GetBroadcasts - returns no rows.
        /// </summary>
        [Fact]
        public void ShouldGetBroadcastsNoRowsReturned()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, HttpStatusCode.OK, false);

            // Act
            RequestResult<IEnumerable<Broadcast>> actualResult = service.GetBroadcastsAsync().Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(!actualResult.ResourcePayload.Any());
            Assert.True(actualResult.TotalResultCount == 0);
        }

        /// <summary>
        /// GetBroadcasts - api returns error.
        /// </summary>
        [Fact]
        public void GetBroadcastsShouldReturnsError()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, HttpStatusCode.InternalServerError, false);

            // Act
            RequestResult<IEnumerable<Broadcast>> actualResult = service.GetBroadcastsAsync().Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(UnexpectedErrorMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetBroadcasts - api throws exception.
        /// </summary>
        [Fact]
        public void GetBroadcastsShouldThrowsException()
        {
            // Arrange
            IBroadcastService service = GetBroadcastService(null, null, true);

            // Act
            RequestResult<IEnumerable<Broadcast>> actualResult = service.GetBroadcastsAsync().Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// UpdateBroadcast.
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
            IBroadcastService service = GetBroadcastService(expectedId, HttpStatusCode.OK, false);

            // Act
            RequestResult<Broadcast> actualResult = service.UpdateBroadcastAsync(broadcast).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.TotalResultCount == 1);
            Assert.Equal(expectedId, actualResult.ResourcePayload.Id);
        }

        /// <summary>
        /// UpdateBroadcast -api returns error.
        /// </summary>
        [Fact]
        public void UpdateBroadcastShouldReturnsError()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
            };
            IBroadcastService service = GetBroadcastService(null, HttpStatusCode.InternalServerError, false);

            // Act
            RequestResult<Broadcast> actualResult = service.UpdateBroadcastAsync(broadcast).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(UnexpectedErrorMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// CreateBroadcast -api throws exception.
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
            IBroadcastService service = GetBroadcastService(null, null, true);

            // Act
            RequestResult<Broadcast> actualResult = service.UpdateBroadcastAsync(broadcast).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(ThrownExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// DeleteBroadcast.
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
            IBroadcastService service = GetBroadcastService(expectedId, HttpStatusCode.OK, false);

            // Act
            RequestResult<Broadcast> actualResult = service.DeleteBroadcastAsync(broadcast).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResultError);
        }

        /// <summary>
        /// DeleteBroadcast -api returns error.
        /// </summary>
        [Fact]
        public void DeleteBroadcastShouldReturnError()
        {
            Guid expectedId = Guid.NewGuid();

            // Arrange
            Broadcast broadcast = new()
            {
                Id = expectedId,
            };
            IBroadcastService service = GetBroadcastService(null, HttpStatusCode.InternalServerError, false);

            // Act
            RequestResult<Broadcast> actualResult = service.DeleteBroadcastAsync(broadcast).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(UnexpectedErrorMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// DeleteBroadcast -api throws exception.
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
            IBroadcastService service = GetBroadcastService(null, null, true);

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

        private static IBroadcastService GetBroadcastService(Guid? id, HttpStatusCode? statusCode, bool throwException)
        {
            Mock<IApiResponse> mockApiDeleteResponse = new();
            mockApiDeleteResponse.Setup(r => r.StatusCode).Returns(statusCode ?? HttpStatusCode.OK);

            Mock<IApiResponse<BroadcastResponse>> mockApiUpdateResponse = new();
            mockApiUpdateResponse.Setup(r => r.Content).Returns(GetApiResponse(id));
            mockApiUpdateResponse.Setup(r => r.StatusCode).Returns(statusCode ?? HttpStatusCode.OK);

            Mock<IApiResponse<BroadcastResponse>> mockApiCreateResponse = new();
            mockApiCreateResponse.Setup(r => r.Content).Returns(GetApiResponse(null));
            mockApiCreateResponse.Setup(r => r.StatusCode).Returns(statusCode ?? HttpStatusCode.OK);

            Mock<IApiResponse<IEnumerable<BroadcastResponse>>> mockApiGetResponse = new();
            List<BroadcastResponse> responses = new();
            if (id != null)
            {
                responses.Add(GetApiResponse(id));
            }

            mockApiGetResponse.Setup(r => r.Content).Returns(responses);
            mockApiGetResponse.Setup(r => r.StatusCode).Returns(statusCode ?? HttpStatusCode.OK);

            Mock<ISystemBroadcastApi> mockSystemBroadcastApi = new();

            if (!throwException)
            {
                mockSystemBroadcastApi.Setup(s => s.GetBroadcasts()).ReturnsAsync(mockApiGetResponse.Object);
                mockSystemBroadcastApi.Setup(s => s.UpdateBroadcast(It.IsAny<string>(), It.IsAny<BroadcastRequest>())).ReturnsAsync(mockApiUpdateResponse.Object);
                mockSystemBroadcastApi.Setup(s => s.CreateBroadcast(It.IsAny<BroadcastRequest>())).ReturnsAsync(mockApiCreateResponse.Object);
                mockSystemBroadcastApi.Setup(s => s.DeleteBroadcast(It.IsAny<string>())).ReturnsAsync(mockApiDeleteResponse.Object);
            }
            else
            {
                mockSystemBroadcastApi.Setup(s => s.GetBroadcasts()).ThrowsAsync(new HttpRequestException(string.Empty));
                mockSystemBroadcastApi.Setup(s => s.UpdateBroadcast(It.IsAny<string>(), It.IsAny<BroadcastRequest>())).ThrowsAsync(new HttpRequestException(string.Empty));
                mockSystemBroadcastApi.Setup(s => s.CreateBroadcast(It.IsAny<BroadcastRequest>())).ThrowsAsync(new HttpRequestException(string.Empty));
                mockSystemBroadcastApi.Setup(s => s.DeleteBroadcast(It.IsAny<string>())).ThrowsAsync(new HttpRequestException(string.Empty));
            }

            return new BroadcastService(
                new Mock<ILogger<BroadcastService>>().Object,
                mockSystemBroadcastApi.Object,
                MapperUtil.InitializeAutoMapper());
        }
    }
}
