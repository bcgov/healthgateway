// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace AccountDataAccessTest.Strategy
{
    using System.Net;
    using AccountDataAccessTest.Utils;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Hdid Phsa Strategy Unit Tests.
    /// </summary>
    public class HdidPhsaStrategyTests
    {
        private const string PatientCacheDomain = "PatientV2";
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";
        private const string Gender = "Male";
        private const string PhsaHdidNotFound = "phsa123NotFound";

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// GetPatientAsync by hdid - happy path.
        /// </summary>
        /// <param name="useCache">The value indicates whether cache should be used or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldGetPatient(bool useCache)
        {
            // Arrange
            PatientRequest patientRequest = new(Hdid, useCache);

            PatientModel expected = new()
            {
                Phn = Phn,
                Hdid = Hdid,
                Gender = Gender,
                ResponseCode = string.Empty,
                IsDeceased = false,
                CommonName = new Name
                {
                    GivenName = string.Empty,
                    Surname = string.Empty,
                },
                LegalName = new Name
                {
                    GivenName = string.Empty,
                    Surname = string.Empty,
                },
            };

            (HdidPhsaStrategy strategy, Mock<ICacheProvider> cacheProvider) = SetupPatientMockForGetPatient(useCache, expected);

            // Act
            PatientModel actual = await strategy.GetPatientAsync(patientRequest);

            // Assert
            actual.ShouldDeepEqual(expected);

            // Verify
            cacheProvider.Verify(
                v => v.GetItemAsync<PatientModel>(
                    It.Is<string>(x => x == $"{PatientCacheDomain}:HDID:{patientRequest.Identifier}"),
                    It.IsAny<CancellationToken>()),
                useCache ? Times.AtLeastOnce : Times.Never);
        }

        /// <summary>
        /// Get patient identity by hdid handles PHSA ApiException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetPatientIdentityHandlesPhsaApiException()
        {
            // Arrange
            PatientRequest patientRequest = new(PhsaHdidNotFound, false);
            Type expected = typeof(NotFoundException);
            HdidPhsaStrategy strategy = SetupHdidPhsaStrategyForGetPatientIdentityHandlesPhsaApiException();

            // Act and Assert
            await Assert.ThrowsAsync(
                expected,
                async () => { await strategy.GetPatientAsync(patientRequest); });
        }

        private static IConfigurationRoot GetConfiguration()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "PatientService:CacheTTL", "90" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static PatientMock SetupPatientMockForGetPatient(bool useCache, PatientModel patient)
        {
            Mock<ICacheProvider> cacheProvider = new();
            Mock<IPatientIdentityApi> patientIdentityApi = new();

            if (useCache)
            {
                cacheProvider.Setup(
                        p => p.GetItemAsync<PatientModel>(
                            It.Is<string>(x => x == $"{PatientCacheDomain}:HDID:{Hdid}"),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(patient);
            }
            else
            {
                PatientIdentity patientIdentity = new()
                {
                    Phn = patient.Phn,
                    HdId = patient.Hdid,
                    Gender = patient.Gender,
                    HasDeathIndicator = patient.IsDeceased!.Value,
                };

                patientIdentityApi.Setup(
                        p => p.GetPatientIdentityAsync(
                            It.Is<string>(x => x == Hdid),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(patientIdentity);
            }

            HdidPhsaStrategy hdidPhsaStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidPhsaStrategy>>().Object,
                Mapper);

            return new(hdidPhsaStrategy, cacheProvider);
        }

        private static HdidPhsaStrategy SetupHdidPhsaStrategyForGetPatientIdentityHandlesPhsaApiException()
        {
            PatientModel? cachedPatient = null;
            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(
                    s => s.GetItemAsync<PatientModel>(
                        It.Is<string>(x => x == $"{PatientCacheDomain}:HDID:{Hdid}"),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedPatient);

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(
                    s => s.GetPatientIdentityAsync(
                        It.Is<string>(x => x == PhsaHdidNotFound),
                        It.IsAny<CancellationToken>()))
                .Throws(RefitExceptionUtil.CreateApiException(HttpStatusCode.NotFound).Result);

            return new(
                GetConfiguration(),
                cacheProvider.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidPhsaStrategy>>().Object,
                Mapper);
        }

        private sealed record PatientMock(
            HdidPhsaStrategy Strategy,
            Mock<ICacheProvider> CacheProvider);
    }
}
