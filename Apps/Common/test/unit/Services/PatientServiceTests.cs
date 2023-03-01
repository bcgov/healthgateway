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
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
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
        /// GetPatientPHN - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetPatientPhn()
        {
            RequestResult<string> actual = GetPatientPhn(new Dictionary<string, string?>(), false);

            Assert.Equal(Phn, actual.ResourcePayload);
        }

        /// <summary>
        /// GetPatient - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetPatient()
        {
            GetPatient(PatientIdentifierType.Hdid, new Dictionary<string, string?>());
        }

        /// <summary>
        /// GetPatient - Happy Path (Cached).
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCache()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };
            GetPatient(PatientIdentifierType.Hdid, configDictionary, true);
        }

        /// <summary>
        /// GetPatient - Happy Path (Using PHN).
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCacheWithPhn()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };
            GetPatient(PatientIdentifierType.Phn, configDictionary);
        }

        /// <summary>
        /// GetPatient - DB Error.
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCacheWithDbError()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };
            GetPatient(PatientIdentifierType.Hdid, configDictionary);
        }

        /// <summary>
        /// GetPatient - Valid ID.
        /// </summary>
        [Fact]
        public void ShouldSearchByValidIdentifier()
        {
            RequestResult<PatientModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    CommonName = new Name
                    {
                        GivenName = "John",
                        Surname = "Doe",
                    },
                    PersonalHealthNumber = Phn,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsByPhnAsync(It.IsAny<string>(), false)).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object);

            // Act
            RequestResult<PatientModel> actual = Task.Run(async () => await service.GetPatient(Phn, PatientIdentifierType.Phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(Phn, actual.ResourcePayload?.PersonalHealthNumber);
        }

        /// <summary>
        /// GetPatient - Valid ID.
        /// </summary>
        [Fact]
        public void ShouldFailModCheck()
        {
            RequestResult<PatientModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    CommonName = new Name
                    {
                        GivenName = "John",
                        Surname = "Doe",
                    },
                    PersonalHealthNumber = "abc123",
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsByPhnAsync(It.IsAny<string>(), false)).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object);

            // Act
            RequestResult<PatientModel> actual = Task.Run(async () => await service.GetPatient("abc123", PatientIdentifierType.Phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.ActionRequired, actual.ResultStatus);
        }

        /// <summary>
        /// GetPatient - Invalid Id.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
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
                    CommonName = new Name
                    {
                        GivenName = "John",
                        Surname = "Doe",
                    },
                    PersonalHealthNumber = phn,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsByHdidAsync(It.IsAny<string>(), false)).ReturnsAsync(requestResult);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object);

            await Assert.ThrowsAsync<NotImplementedException>(() => service.GetPatient("abc123", (PatientIdentifierType)23)).ConfigureAwait(true);
        }

        private static RequestResult<string> GetPatientPhn(Dictionary<string, string?> configDictionary, bool returnNullPatientResult)
        {
            RequestResult<PatientModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    CommonName = new Name
                    {
                        GivenName = "John",
                        Surname = "Doe",
                    },
                    HdId = Hdid,
                    PersonalHealthNumber = Phn,
                },
            };
            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();

            patientDelegateMock.Setup(p => p.GetDemographicsByHdidAsync(It.IsAny<string>(), false)).ReturnsAsync(requestResult);

            Mock<ICacheProvider> cacheProviderMock = new();
            cacheProviderMock.Setup(p => p.GetItem<PatientModel>(It.IsAny<string>())).Returns(returnNullPatientResult ? null : requestResult.ResourcePayload);

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                config,
                patientDelegateMock.Object,
                cacheProviderMock.Object);

            // Act
            RequestResult<string> actual = Task.Run(async () => await service.GetPatientPhn(Hdid).ConfigureAwait(true)).Result;
            return actual;
        }

        private static void GetPatient(PatientIdentifierType identifierType, Dictionary<string, string?> configDictionary, bool returnValidCache = false)
        {
            RequestResult<PatientModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                PageSize = 1,
                ResourcePayload = new PatientModel
                {
                    CommonName = new Name
                    {
                        GivenName = "John",
                        Surname = "Doe",
                    },
                    HdId = Hdid,
                    PersonalHealthNumber = Phn,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();

            patientDelegateMock.Setup(p => p.GetDemographicsByHdidAsync(It.IsAny<string>(), false)).ReturnsAsync(requestResult);
            patientDelegateMock.Setup(p => p.GetDemographicsByPhnAsync(It.IsAny<string>(), false)).ReturnsAsync(requestResult);

            Mock<ICacheProvider> cacheProviderMock = new();
            if (returnValidCache)
            {
                cacheProviderMock.Setup(p => p.GetItem<PatientModel>(It.IsAny<string>())).Returns(requestResult.ResourcePayload);
            }

            IPatientService service = new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                config,
                patientDelegateMock.Object,
                cacheProviderMock.Object);

            // Act
            RequestResult<PatientModel> actual = Task.Run(async () => await service.GetPatient(identifierType == PatientIdentifierType.Hdid ? Hdid : Phn, identifierType).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            Assert.Equal(Hdid, actual.ResourcePayload?.HdId);
        }
    }
}
