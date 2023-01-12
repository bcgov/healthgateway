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
namespace HealthGateway.PatientTests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Delegates;
    using HealthGateway.Patient.Services;
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
        [Fact]
        public void ShouldGetPatient()
        {
            // Arrange
            IPatientService service = GetPatientService(Phn, Hdid);

            // Act
            ApiResult<PatientModel> actual = Task.Run(async () => await service.GetPatient(Hdid).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Hdid, actual.ResourcePayload?.HdId);
        }

        /// <summary>
        /// GetPatient - Happy Path (Cached).
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCache()
        {
            // Arrange
            IPatientService service = GetPatientService(Phn, Hdid, true);

            // Act
            ApiResult<PatientModel> actual = Task.Run(async () => await service.GetPatient(Hdid).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Hdid, actual.ResourcePayload?.HdId);
        }

        /// <summary>
        /// GetPatient - Happy Path (Using PHN).
        /// </summary>
        [Fact]
        public void ShouldGetPatientFromCacheWithPhn()
        {
            // Arrange
            IPatientService service = GetPatientService(Phn, Phn, true);

            // Act
            ApiResult<PatientModel> actual = Task.Run(async () => await service.GetPatient(Phn, PatientIdentifierType.Phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Hdid, actual.ResourcePayload?.HdId);
        }

        /// <summary>
        /// GetPatient - Valid Phn.
        /// </summary>
        [Fact]
        public void ShouldGetPatientByValidPhn()
        {
            // Arrange
            IPatientService service = GetPatientService(Phn, Phn);

            // Act
            ApiResult<PatientModel> actual = Task.Run(async () => await service.GetPatient(Phn, PatientIdentifierType.Phn).ConfigureAwait(true)).Result;

            // Verify
            Assert.Equal(Phn, actual.ResourcePayload?.PersonalHealthNumber);
        }

        /// <summary>
        /// GetPatient throws api patient exception given invalid phn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldThrowApiPatientExceptionGivenInvalidPhn()
        {
            string expectedPhn = "abc123";

            // Arrange
            IPatientService service = GetPatientService(expectedPhn, expectedPhn);

            // Act
            async Task Actual()
            {
                await service.GetPatient("abc123", PatientIdentifierType.Phn).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.PhnInvalid, exception.ProblemDetails!.Detail);
        }

        private static IPatientService GetPatientService(string expectedPhn, string expectedIdentifier, bool returnValidCache = false)
        {
            ApiResult<PatientModel> requestResult = new()
            {
                ResourcePayload = new PatientModel
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PersonalHealthNumber = expectedPhn,
                    HdId = Hdid,
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsAsync(OidType.Hdid, expectedIdentifier, false)).ReturnsAsync(requestResult);
            patientDelegateMock.Setup(p => p.GetDemographicsAsync(OidType.Phn, expectedIdentifier, false)).ReturnsAsync(requestResult);

            Mock<ICacheProvider> cacheProviderMock = new();
            if (returnValidCache)
            {
                cacheProviderMock.Setup(p => p.GetItem<PatientModel>(It.IsAny<string>())).Returns(requestResult.ResourcePayload);
            }

            return new PatientService(
                new Mock<ILogger<PatientService>>().Object,
                configuration,
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object);
        }
    }
}
