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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// PatientService's Unit Tests.
    /// </summary>
    public class PatientServiceTests
    {
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";

        /// <summary>
        /// GetPatient - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatient()
        {
            await GetPatientAsync(PatientIdentifierType.Hdid, []);
        }

        /// <summary>
        /// GetPatient - Happy Path (Cached).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientFromCache()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };
            await GetPatientAsync(PatientIdentifierType.Hdid, configDictionary, true);
        }

        /// <summary>
        /// GetPatient - Happy Path (Using PHN).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientFromCacheWithPhn()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };
            await GetPatientAsync(PatientIdentifierType.Phn, configDictionary);
        }

        /// <summary>
        /// GetPatient - DB Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientFromCacheWithDbError()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };
            await GetPatientAsync(PatientIdentifierType.Hdid, configDictionary);
        }

        /// <summary>
        /// GetPatient - Valid ID.
        /// </summary>
        /// <param name="identifierType">The identifier type used to query with.</param>
        /// <param name="identifier">The identifier value used to query with.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [InlineData(PatientIdentifierType.Phn, Phn)]
        [InlineData(PatientIdentifierType.Hdid, Hdid)]
        [Theory]
        public async Task ShouldSearchByValidIdentifier(PatientIdentifierType identifierType, string identifier)
        {
            RequestResult<PatientModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PersonalHealthNumber = Phn,
                    HdId = Hdid,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            if (identifierType == PatientIdentifierType.Hdid)
            {
                patientDelegateMock.Setup(p => p.GetDemographicsByHdidAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);
            }
            else
            {
                patientDelegateMock.Setup(p => p.GetDemographicsByPhnAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);
            }

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object);

            // Act
            RequestResult<PatientModel> actual = await service.GetPatientAsync(identifier, identifierType);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(Phn, actual.ResourcePayload?.PersonalHealthNumber);
            Assert.Equal(Hdid, actual.ResourcePayload?.HdId);
        }

        /// <summary>
        /// GetPatient - Valid ID.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldFailModCheck()
        {
            RequestResult<PatientModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PersonalHealthNumber = "abc123",
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsByPhnAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object);

            // Act
            RequestResult<PatientModel> actual = await service.GetPatientAsync("abc123", PatientIdentifierType.Phn);

            // Verify
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
        }

        /// <summary>
        /// GetPatient - Invalid Id.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldThrowIfInvalidIdentifierType()
        {
            string phn = "abc123";

            RequestResult<PatientModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PersonalHealthNumber = phn,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsByHdidAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object);

            await Assert.ThrowsAsync<NotImplementedException>(() => service.GetPatientAsync("abc123", (PatientIdentifierType)23));
        }

        /// <summary>
        /// GetPatient - Returns action required when identifier is null.
        /// </summary>
        /// <param name="identifierType">The identifier type used to query with.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [InlineData(PatientIdentifierType.Phn)]
        [InlineData(PatientIdentifierType.Hdid)]
        [Theory]
        public async Task GetPatientReturnsActionRequired(PatientIdentifierType identifierType)
        {
            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object);

            RequestResult<PatientModel> actual = await service.GetPatientAsync(string.Empty, identifierType);
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
        }

        /// <summary>
        /// GetPatient - throws not implemented exception for invalid patient identifier type.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task GetPatientFromCacheThrowsNotImplementedException()
        {
            const PatientIdentifierType identifierType = (PatientIdentifierType)999; // This will cause not implemented exception

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object);

            // Act and Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => service.GetPatientAsync(Hdid, identifierType));
        }

        private static async Task GetPatientAsync(PatientIdentifierType identifierType, Dictionary<string, string?> configDictionary, bool returnValidCache = false)
        {
            RequestResult<PatientModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    FirstName = "John",
                    LastName = "Doe",
                    HdId = Hdid,
                    PersonalHealthNumber = Phn,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary)
                .Build();

            patientDelegateMock.Setup(p => p.GetDemographicsByHdidAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);
            patientDelegateMock.Setup(p => p.GetDemographicsByPhnAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);

            Mock<ICacheProvider> cacheProviderMock = new();
            if (returnValidCache)
            {
                cacheProviderMock.Setup(p => p.GetItemAsync<PatientModel>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(requestResult.ResourcePayload);
            }

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                config,
                patientDelegateMock.Object,
                cacheProviderMock.Object);

            // Act
            RequestResult<PatientModel> actual = await service.GetPatientAsync(identifierType == PatientIdentifierType.Hdid ? Hdid : Phn, identifierType);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(Hdid, actual.ResourcePayload?.HdId);
        }
    }
}
