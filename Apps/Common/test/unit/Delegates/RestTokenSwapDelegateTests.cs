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
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for RestTokenSwapDelegate.
    /// </summary>
    public class RestTokenSwapDelegateTests
    {
        private const string AccessToken = "access_token";
        private const string HttpExceptionMessage = "Error with Token Swap API";

        /// <summary>
        /// Swap token - Happy Path.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task SwapToken()
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new()
            {
                AccessToken = AccessToken,
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "file.read",
            };

            ITokenSwapDelegate tokenSwapDelegate = GetTokenSwapDelegate(expectedTokenSwapResponse, false);

            // Act
            RequestResult<TokenSwapResponse> actualResult = await tokenSwapDelegate.SwapTokenAsync(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedTokenSwapResponse.AccessToken, actualResult.ResourcePayload?.AccessToken);
        }

        /// <summary>
        /// Swap token - HttpRequestException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SwapTokenThrowsException()
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new()
            {
                AccessToken = AccessToken,
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "file.read",
            };

            ITokenSwapDelegate tokenSwapDelegate = GetTokenSwapDelegate(expectedTokenSwapResponse, true);

            // Act
            RequestResult<TokenSwapResponse> actualResult = await tokenSwapDelegate.SwapTokenAsync(It.IsAny<string>());

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(HttpExceptionMessage, actualResult.ResultError?.ResultMessage);
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
                .AddInMemoryCollection(configuration)
                .Build();
        }

        private static ITokenSwapDelegate GetTokenSwapDelegate(TokenSwapResponse response, bool throwException)
        {
            Mock<ITokenSwapApi> mockTokenSwapApi = new();
            if (!throwException)
            {
                mockTokenSwapApi.Setup(s => s.SwapTokenAsync(It.IsAny<FormUrlEncodedContent>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(response);
            }
            else
            {
                mockTokenSwapApi.Setup(s => s.SwapTokenAsync(It.IsAny<FormUrlEncodedContent>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            return new RestTokenSwapDelegate(
                new Mock<ILogger<RestTokenSwapDelegate>>().Object,
                mockTokenSwapApi.Object,
                GetIConfigurationRoot());
        }
    }
}
