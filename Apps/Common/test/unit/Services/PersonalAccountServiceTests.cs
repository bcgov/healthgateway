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
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
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
        /// Get Patient Account Result.
        /// </summary>
        /// <param name="useCache">The value indicates whether cache should be used or not.</param>
        /// <param name="cacheToLive">The number of minutes cache lives.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, null)]
        [InlineData(false, "90")]
        [InlineData(false, "0")]
        public async Task ShouldGetPatientAccountResult(bool useCache, string? cacheToLive)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            PersonalAccount expectedPersonalAccount = GetPatientAccount(id);

            Mock<ICacheProvider> cacheProviderMock = new();
            IPersonalAccountsService personalAccountsService = GetPersonalAccountsService(expectedPersonalAccount, useCache, false, cacheProviderMock, cacheToLive);

            // Act
            RequestResult<PersonalAccount> actualResult = await personalAccountsService.GetPersonalAccountResultAsync(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(id, actualResult.ResourcePayload?.Id);

            cacheProviderMock
                .Verify(
                    s => s.AddItemAsync(It.IsAny<string>(), It.IsAny<PersonalAccount>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
                    cacheToLive is "90" ? Times.Once : Times.Never);
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

            IPersonalAccountsService personalAccountsService = GetPersonalAccountsService(expectedPersonalAccount, false, true);

            // Act
            RequestResult<PersonalAccount> actualResult = await personalAccountsService.GetPersonalAccountResultAsync(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(PersonalAccountsErrorMessage, actualResult.ResultError?.ResultMessage);
        }

        private static IConfigurationRoot GetIConfigurationRoot(string cacheToLive = "90")
        {
            Dictionary<string, string?> configuration = new()
            {
                { "PhsaV2:PersonalAccountsCacheTtl", cacheToLive },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configuration)
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

        private static IPersonalAccountsService GetPersonalAccountsService(
            PersonalAccount content,
            bool useCache,
            bool throwException,
            Mock<ICacheProvider>? cacheProviderMock = null,
            string? cacheToLive = null)
        {
            cacheProviderMock ??= new();
            cacheProviderMock.Setup(p => p.GetItemAsync<PersonalAccount>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(useCache ? content : null);

            Mock<IPersonalAccountsApi> personalAccountsApiMock = new();
            if (!throwException)
            {
                personalAccountsApiMock.Setup(p => p.AccountLookupByHdidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(content);
            }
            else
            {
                personalAccountsApiMock.Setup(p => p.AccountLookupByHdidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            return new PersonalAccountsService(
                new Mock<ILogger<PersonalAccountsService>>().Object,
                GetIConfigurationRoot(cacheToLive),
                cacheProviderMock.Object,
                personalAccountsApiMock.Object);
        }
    }
}
