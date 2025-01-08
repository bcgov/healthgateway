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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// AccessTokenService's Unit Tests.
    /// </summary>
    public class AccessTokenServiceTests
    {
        private const string AccessToken = "access_token";
        private const string UserId = "userid";
        private const string TokenSwapCacheDomain = "TokenSwap";
        private const string CacheKey = $"{TokenSwapCacheDomain}:{UserId}";
        private const string AccessTokenNotFoundMessage = $"Internal Error: Unable to get authenticated user token from context for: {CacheKey}";

        /// <summary>
        /// Get PHSA access token.
        /// </summary>
        /// <param name="cacheEnabled">Boolean value indicating whether the token should be cached.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetPhsaToken(bool cacheEnabled)
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new()
            {
                AccessToken = AccessToken,
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "file.read",
            };

            IAccessTokenService accessTokenService = GetAccessTokenService(expectedTokenSwapResponse, true, false, cacheEnabled);

            // Act
            RequestResult<TokenSwapResponse> actualResult = await accessTokenService.GetPhsaAccessTokenAsync();

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedTokenSwapResponse.AccessToken, actualResult.ResourcePayload?.AccessToken);
        }

        /// <summary>
        /// Get PHSA access token - use cache.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPhsaTokenFromCache()
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new()
            {
                AccessToken = AccessToken,
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "file.read",
            };

            IAccessTokenService accessTokenService = GetAccessTokenService(expectedTokenSwapResponse, true, true);

            // Act
            RequestResult<TokenSwapResponse> actualResult = await accessTokenService.GetPhsaAccessTokenAsync();

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedTokenSwapResponse.AccessToken, actualResult.ResourcePayload?.AccessToken);
        }

        /// <summary>
        /// Get PHSA access token - could not get access token from authentication delegate.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPhsaTokenReturnsError()
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new()
            {
                AccessToken = AccessToken,
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "file.read",
            };

            IAccessTokenService accessTokenService = GetAccessTokenService(expectedTokenSwapResponse, false, false);

            // Act
            RequestResult<TokenSwapResponse> actualResult = await accessTokenService.GetPhsaAccessTokenAsync();

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(AccessTokenNotFoundMessage, actualResult.ResultError!.ResultMessage);
            Assert.Null(actualResult.ResourcePayload);
        }

        private static IConfigurationRoot GetIConfigurationRoot(bool cacheEnabled)
        {
            Dictionary<string, string?> configuration = new()
            {
                { "TokenSwap:BaseUrl", "http://localhost" },
                { "TokenSwap:ClientId", "healthgateway" },
                { "TokenSwap:ClientSecret", "client-secret" },
                { "TokenSwap:GrantType", "healthdata.read" },
            };

            if (cacheEnabled)
            {
                configuration["PhsaV2:TokenCacheEnabled"] = "true";
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configuration)
                .Build();
        }

        private static IAccessTokenService GetAccessTokenService(TokenSwapResponse response, bool isAccessTokenFound, bool existsInCache, bool cacheEnabled = true)
        {
            RequestResult<TokenSwapResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = response,
            };

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate.Setup(a => a.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(isAccessTokenFound ? AccessToken : null);
            mockAuthenticationDelegate.Setup(a => a.FetchAuthenticatedUserId()).Returns(UserId);

            Mock<ITokenSwapDelegate> mockTokenSwapDelegate = new();
            mockTokenSwapDelegate.Setup(s => s.SwapTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);

            Mock<ICacheProvider> cacheProviderMock = new();
            cacheProviderMock.Setup(p => p.GetItemAsync<TokenSwapResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(existsInCache ? response : null);

            return new AccessTokenService(
                new Mock<ILogger<AccessTokenService>>().Object,
                mockTokenSwapDelegate.Object,
                cacheProviderMock.Object,
                mockAuthenticationDelegate.Object,
                GetIConfigurationRoot(cacheEnabled));
        }
    }
}
