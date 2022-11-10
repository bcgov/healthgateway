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
    using System.Linq;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
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
        [Fact]
        public void GetPhsaToken()
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new()
            {
                AccessToken = AccessToken,
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "file.read",
            };

            IAccessTokenService accessTokenService = GetAccessTokenService(expectedTokenSwapResponse, true, false);

            // Act
            RequestResult<TokenSwapResponse> actualResult = accessTokenService.GetPhsaAccessToken().Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedTokenSwapResponse.AccessToken, actualResult.ResourcePayload?.AccessToken);
        }

        /// <summary>
        /// Get PHSA access token - use cache.
        /// </summary>
        [Fact]
        public void GetPhsaTokenFromCache()
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
            RequestResult<TokenSwapResponse> actualResult = accessTokenService.GetPhsaAccessToken().Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(expectedTokenSwapResponse.AccessToken, actualResult.ResourcePayload?.AccessToken);
        }

        /// <summary>
        /// Get PHSA access token - could not get access token from authentication delegate.
        /// </summary>
        [Fact]
        public void GetPhsaTokenReturnsError()
        {
            // Arrange
            TokenSwapResponse expectedTokenSwapResponse = new()
            {
                AccessToken = AccessToken,
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "file.read",
            };

            IAccessTokenService accessTokenService = GetAccessTokenService(expectedTokenSwapResponse, false, true);

            // Act
            RequestResult<TokenSwapResponse> actualResult = accessTokenService.GetPhsaAccessToken().Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(AccessTokenNotFoundMessage, actualResult.ResultError!.ResultMessage);
            Assert.Null(actualResult.ResourcePayload);
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
                .AddInMemoryCollection(configuration.ToList())
                .Build();
        }

        private static IAccessTokenService GetAccessTokenService(TokenSwapResponse response, bool isAccessTokenFound, bool useCache)
        {
            RequestResult<TokenSwapResponse> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = response,
            };

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate.Setup(a => a.FetchAuthenticatedUserToken()).Returns(isAccessTokenFound ? AccessToken : null);
            mockAuthenticationDelegate.Setup(a => a.FetchAuthenticatedUserId()).Returns(UserId);

            Mock<ITokenSwapDelegate> mockTokenSwapDelegate = new();
            mockTokenSwapDelegate.Setup(s => s.SwapToken(It.IsAny<string>())).ReturnsAsync(requestResult);

            Mock<ICacheProvider> cacheProviderMock = new();
            cacheProviderMock.Setup(p => p.GetItem<TokenSwapResponse>(It.IsAny<string>())).Returns(useCache ? response : null);

            return new AccessTokenService(
                new Mock<ILogger<AccessTokenService>>().Object,
                mockTokenSwapDelegate.Object,
                cacheProviderMock.Object,
                mockAuthenticationDelegate.Object,
                GetIConfigurationRoot());
        }
    }
}
