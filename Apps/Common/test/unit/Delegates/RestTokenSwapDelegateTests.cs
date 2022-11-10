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
namespace HealthGateway.CommonTests.Delegates
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// Unit Tests for RestTokenSwapDelegate.
    /// </summary>
    public class RestTokenSwapDelegateTests
    {
        private const string AccessToken = "access_token";
        private const string HttpExceptionMessage = "Error with HTTP Request";
        private const string InternalServerErrorMessage = "Unable to connect to Token Endpoint, HTTP Error InternalServerError";

        /// <summary>
        /// Swap token - Happy Path.
        /// </summary>
        [Fact]
        public void SwapToken()
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new()
            {
                AccessToken = AccessToken,
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "file.read",
            };

            ITokenSwapDelegate tokenSwapDelegate = GetTokenSwapDelegate(expectedTokenSwapResponse, HttpStatusCode.OK, false);

            // Act
            RequestResult<TokenSwapResponse> actualResult = tokenSwapDelegate.SwapToken(It.IsAny<string>()).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedTokenSwapResponse.AccessToken, actualResult.ResourcePayload?.AccessToken);
        }

        /// <summary>
        /// Swap token - HttpRequestException.
        /// </summary>
        [Fact]
        public void SwapTokenThrowsException()
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new()
            {
                AccessToken = AccessToken,
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "file.read",
            };

            ITokenSwapDelegate tokenSwapDelegate = GetTokenSwapDelegate(expectedTokenSwapResponse, HttpStatusCode.OK, true);

            // Act
            RequestResult<TokenSwapResponse> actualResult = tokenSwapDelegate.SwapToken(It.IsAny<string>()).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(HttpExceptionMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// Tests various http status codes on Token Swap Response.
        /// </summary>
        /// <param name="httpStatusCode">The http status code to return from the mock.</param>
        /// <param name="resultStatus">The result code to return from the mock.</param>
        /// <param name="resultMessage">The result message from the mock.</param>
        [Theory]
        [InlineData(HttpStatusCode.OK, ResultType.Success, null)]
        [InlineData(HttpStatusCode.InternalServerError, ResultType.Error, InternalServerErrorMessage)]
        public void GetTokenSwapResponse(HttpStatusCode httpStatusCode, ResultType resultStatus, string resultMessage)
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new();
            ITokenSwapDelegate tokenSwapDelegate = GetTokenSwapDelegate(expectedTokenSwapResponse, httpStatusCode, false);

            // Act
            RequestResult<TokenSwapResponse> actualResult = tokenSwapDelegate.SwapToken(It.IsAny<string>()).Result;

            // Assert
            Assert.True(actualResult.ResultStatus == resultStatus);
            Assert.Equal(actualResult?.ResultError?.ResultMessage, resultMessage);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> configuration = new()
            {
                { "TokenSwap:BaseUrl", "http://localhost" },
                { "TokenSwap:ClientId", "healthgateway" },
                { "TokenSwap:ClientSecret", "client-secret" },
                { "TokenSwap:GrantType", "healthdata.read" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configuration.ToList<KeyValuePair<string, string?>>())
                .Build();
        }

        private static ITokenSwapDelegate GetTokenSwapDelegate(TokenSwapResponse response, HttpStatusCode statusCode, bool throwException)
        {
            Mock<IApiResponse<TokenSwapResponse>> mockApiResponse = new();
            mockApiResponse.Setup(s => s.Content).Returns(response);
            mockApiResponse.Setup(s => s.StatusCode).Returns(statusCode);

            Mock<ITokenSwapApi> mockTokenSwapApi = new();
            if (!throwException)
            {
                mockTokenSwapApi.Setup(s => s.SwapToken(It.IsAny<FormUrlEncodedContent>()))
                    .ReturnsAsync(mockApiResponse.Object);
            }
            else
            {
                mockTokenSwapApi.Setup(s => s.SwapToken(It.IsAny<FormUrlEncodedContent>()))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            return new RestTokenSwapDelegate(
                new Mock<ILogger<RestTokenSwapDelegate>>().Object,
                mockTokenSwapApi.Object,
                GetIConfigurationRoot());
        }
    }
}
