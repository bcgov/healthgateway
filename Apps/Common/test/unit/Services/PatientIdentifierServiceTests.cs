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
    /// PatientIdentifierService Unit Tests.
    /// </summary>
    public class PatientIdentifierServiceTests
    {
        private const string PatientIdentifierErrorMessage = "Error with request for patient identifier";

        /// <summary>
        /// GetPatientIdentifierResultAsync should return result.
        /// </summary>
        /// <param name="useCache">The value indicates whether cache should be used or not.</param>
        /// <param name="cacheToLive">The number of minutes cache lives.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, null)]
        [InlineData(false, "90")]
        [InlineData(false, "0")]
        public async Task GetPatientIdentifierResultAsyncShouldReturnResult(bool useCache, string? cacheToLive)
        {
            // Arrange
            Guid pid = Guid.NewGuid();

            Mock<ICacheProvider> cacheProviderMock = new();
            IPatientIdentifierService patientIdentifierService = GetPatientIdentifierService(pid, useCache, false, cacheProviderMock, cacheToLive);

            // Act
            RequestResult<Guid> actualResult = await patientIdentifierService.GetPatientIdentifierResultAsync(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(pid, actualResult.ResourcePayload);

            cacheProviderMock
                .Verify(
                    s => s.AddItemAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
                    cacheToLive is "90" ? Times.Once : Times.Never);
        }

        /// <summary>
        /// GetPatientIdentifierResultAsync throws HttpRequestException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientIdentifierResultAsyncShouldThrowException()
        {
            // Arrange
            Guid pid = Guid.NewGuid();

            IPatientIdentifierService patientIdentifierService = GetPatientIdentifierService(pid, false, true);

            // Act
            RequestResult<Guid> actualResult = await patientIdentifierService.GetPatientIdentifierResultAsync(It.IsAny<string>());

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(PatientIdentifierErrorMessage, actualResult.ResultError?.ResultMessage);
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

        private static IPatientIdentifierService GetPatientIdentifierService(
            Guid pid,
            bool useCache,
            bool throwException,
            Mock<ICacheProvider>? cacheProviderMock = null,
            string? cacheToLive = null)
        {
            cacheProviderMock ??= new();
            cacheProviderMock.Setup(p => p.GetItemAsync<Guid?>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(useCache ? pid : null);

            Mock<IPatientIdentifierApi> patientIdentifierApiMock = new();
            if (!throwException)
            {
                patientIdentifierApiMock.Setup(p => p.GetPatientIdentifierByHdidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PatientIdentifierResponse { Pid = pid });
            }
            else
            {
                patientIdentifierApiMock.Setup(p => p.GetPatientIdentifierByHdidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new HttpRequestException("Unit Test HTTP Request Exception"));
            }

            return new PatientIdentifierService(
                new Mock<ILogger<PatientIdentifierService>>().Object,
                GetIConfigurationRoot(cacheToLive),
                cacheProviderMock.Object,
                patientIdentifierApiMock.Object);
        }
    }
}
