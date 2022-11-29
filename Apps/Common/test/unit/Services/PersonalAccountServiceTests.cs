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
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// PersonalAccountService's Unit Tests.
    /// </summary>
    public class PersonalAccountServiceTests
    {
        private const string PersonalAccountsErrorMessage = "Error with request for Personal Accounts";

        /// <summary>
        /// Get Personal Account.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientAccount()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PersonalAccount expectedPersonalAccount = GetPatientAccount(id);

            IPersonalAccountsService personalAccountsServiceService = GetPersonalAccountsService(expectedPersonalAccount, false, false);

            // Act
            RequestResult<PersonalAccount> actualResult = await personalAccountsServiceService.GetPatientAccountResultAsync(It.IsAny<string>()).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(id, actualResult.ResourcePayload?.Id);
        }

        /// <summary>
        /// Get Personal Account - from cache.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientAccountUseCache()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PersonalAccount expectedPersonalAccount = GetPatientAccount(id);

            IPersonalAccountsService personalAccountsServiceService = GetPersonalAccountsService(expectedPersonalAccount, true, false);

            // Act
            RequestResult<PersonalAccount> actualResult = await personalAccountsServiceService.GetPatientAccountResultAsync(It.IsAny<string>()).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(id, actualResult.ResourcePayload?.Id);
        }

        /// <summary>
        /// Get Personal Account throws HttpRequestException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientAccountThrowsException()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PersonalAccount expectedPersonalAccount = GetPatientAccount(id);

            IPersonalAccountsService personalAccountsServiceService = GetPersonalAccountsService(expectedPersonalAccount, false, true);

            // Act
            RequestResult<PersonalAccount> actualResult = await personalAccountsServiceService.GetPatientAccountResultAsync(It.IsAny<string>()).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(PersonalAccountsErrorMessage, actualResult.ResultError?.ResultMessage);
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

        private static PersonalAccount GetPatientAccount(Guid id)
        {
            return new PersonalAccount
            {
                Id = id,
                CreationTimeStampUtc = DateTime.Today,
                ModifyTimeStampUtc = DateTime.Today,
                PatientIdentity = new(),
            };
        }

        private static IPersonalAccountsService GetPersonalAccountsService(PersonalAccount content, bool useCache, bool throwException)
        {
            Mock<ICacheProvider> cacheProviderMock = new();
            cacheProviderMock.Setup(p => p.GetItem<PersonalAccount>(It.IsAny<string>())).Returns(useCache ? content : null);

            Mock<IPersonalAccountsApi> personalAccountsApiMock = new();
            if (!throwException)
            {
                personalAccountsApiMock.Setup(p => p.AccountLookupByHdidAsync(It.IsAny<string>())).ReturnsAsync(content);
            }
            else
            {
                personalAccountsApiMock.Setup(p => p.AccountLookupByHdidAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            return new PersonalAccountsService(
                new Mock<ILogger<PersonalAccountsService>>().Object,
                GetIConfigurationRoot(),
                cacheProviderMock.Object,
                personalAccountsApiMock.Object);
        }
    }
}
