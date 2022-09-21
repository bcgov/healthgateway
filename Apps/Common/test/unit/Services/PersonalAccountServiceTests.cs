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
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// PersonalAccountService's Unit Tests.
    /// </summary>
    public class PersonalAccountServiceTests
    {
        private const string PersonalAccountsErrorMessage = $"Error with HTTP Request for Personal Accounts";

        /// <summary>
        /// Get Personal Account.
        /// </summary>
        [Fact]
        public void ShouldGetPatientAccount()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PersonalAccount expectedPersonalAccount = GetPatientAccount(id);

            IPersonalAccountsService personalAccountsServiceService = GetPersonalAccountsService(expectedPersonalAccount, null, false, false);

            // Act
            RequestResult<PersonalAccount?> actualResult = personalAccountsServiceService.GetPatientAccount(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(id, actualResult.ResourcePayload?.Id);
        }

        /// <summary>
        /// Get Personal Account - from cache.
        /// </summary>
        [Fact]
        public void ShouldGetPatientAccountUseCache()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PersonalAccount expectedPersonalAccount = GetPatientAccount(id);

            IPersonalAccountsService personalAccountsServiceService = GetPersonalAccountsService(expectedPersonalAccount, null, true, false);

            // Act
            RequestResult<PersonalAccount?> actualResult = personalAccountsServiceService.GetPatientAccount(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(id, actualResult.ResourcePayload?.Id);
        }

        /// <summary>
        /// Get Personal Account returns ApiException.
        /// </summary>
        [Fact]
        public void ShouldGetPatientAccountReturnsApiException()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PersonalAccount expectedPersonalAccount = GetPatientAccount(id);

            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            RefitSettings refitSettings = new RefitSettings();
            ApiException apiException = ApiException.Create(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpMethod>(), responseMessage, refitSettings, null).Result;
            responseMessage.Dispose();

            IPersonalAccountsService personalAccountsServiceService = GetPersonalAccountsService(expectedPersonalAccount, apiException, false, false);

            // Act
            RequestResult<PersonalAccount?> actualResult = personalAccountsServiceService.GetPatientAccount(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload);
        }

        /// <summary>
        /// Get Personal Account throws HttpRequestException.
        /// </summary>
        [Fact]
        public void ShouldGetPatientAccountThrowsException()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PersonalAccount expectedPersonalAccount = GetPatientAccount(id);

            IPersonalAccountsService personalAccountsServiceService = GetPersonalAccountsService(expectedPersonalAccount, null, false, true);

            // Act
            RequestResult<PersonalAccount?> actualResult = personalAccountsServiceService.GetPatientAccount(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(PersonalAccountsErrorMessage, actualResult.ResultError?.ResultMessage);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string> configuration = new()
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

        private static PersonalAccount GetPatientAccount(Guid id)
        {
            return new PersonalAccount()
            {
                Id = id,
                CreationTimeStampUtc = DateTime.Today,
                ModifyTimeStampUtc = DateTime.Today,
            };
        }

        private static IPersonalAccountsService GetPersonalAccountsService(PersonalAccount content, ApiException? error, bool useCache, bool throwException)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(error == null ? HttpStatusCode.OK : HttpStatusCode.NotFound);

            IApiResponse<PersonalAccount> apiResponse = new ApiResponse<PersonalAccount>(
                responseMessage,
                content,
                It.IsAny<RefitSettings>(),
                error);

            Mock<ICacheProvider> cacheProviderMock = new();
            cacheProviderMock.Setup(p => p.GetItem<PersonalAccount>(It.IsAny<string>())).Returns(useCache ? content : null);

            Mock<IPersonalAccountsApi> personalAccountsApiMock = new();
            if (!throwException)
            {
                personalAccountsApiMock.Setup(p => p.AccountLookupByHdid(It.IsAny<string>())).Returns(Task.FromResult(apiResponse));
            }
            else
            {
                personalAccountsApiMock.Setup(p => p.AccountLookupByHdid(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            apiResponse.Dispose();
            responseMessage.Dispose();

            return new PersonalAccountsService(
                new Mock<ILogger<PersonalAccountsService>>().Object,
                GetIConfigurationRoot(),
                cacheProviderMock.Object,
                personalAccountsApiMock.Object);
        }
    }
}
