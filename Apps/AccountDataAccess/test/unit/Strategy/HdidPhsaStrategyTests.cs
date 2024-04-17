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
        public async Task ShouldGetPatientByHdid(bool useCache)
        {
            // Arrange
            GetPatientMock mock = SetupGetPatientMock(useCache);

            // Act
            PatientModel actual = await mock.Strategy.GetPatientAsync(mock.PatientRequest);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Get patient identity by hdid handles PHSA ApiException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetPatientIdentityHandlesPhsaApiException()
        {
            // Arrange
            GetPatientHandlesPhsaApiExceptionMock mock = SetupGetPatientHandlesPhsaApiExceptionMock();

            // Act and Assert
            await Assert.ThrowsAsync(
                mock.ExpectedExceptionType,
                async () => { await mock.Strategy.GetPatientAsync(mock.PatientRequest); });
        }

        private static IConfigurationRoot GetConfiguration()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "PatientService:CacheTTL", "90" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static HdidPhsaStrategy GetHdidPhsaStrategy(IMock<ICacheProvider> cacheProvider, IMock<IPatientIdentityApi> patientIdentityApi)
        {
            return new(
                GetConfiguration(),
                cacheProvider.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidPhsaStrategy>>().Object,
                Mapper);
        }

        private static GetPatientMock SetupGetPatientMock(bool useCache)
        {
            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
                Gender = Gender,
                ResponseCode = string.Empty,
                IsDeceased = false,
                CommonName = new Name
                    { GivenName = string.Empty, Surname = string.Empty },
                LegalName = new Name
                    { GivenName = string.Empty, Surname = string.Empty },
            };

            PatientIdentity patientIdentity = new()
            {
                Phn = Phn,
                HdId = Hdid,
                Gender = Gender,
                HasDeathIndicator = false,
            };

            PatientModel? cachedPatient = useCache ? patient : null;
            PatientRequest patientRequest = new(Hdid, useCache);

            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{Hdid}")).Returns(cachedPatient);

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(Hdid, It.IsAny<CancellationToken>()))!.ReturnsAsync(patientIdentity);

            HdidPhsaStrategy hdidPhsaStrategy = GetHdidPhsaStrategy(cacheProvider, patientIdentityApi);

            return new(hdidPhsaStrategy, patient, patientRequest);
        }

        private static GetPatientHandlesPhsaApiExceptionMock SetupGetPatientHandlesPhsaApiExceptionMock()
        {
            PatientModel? cachedPatient = null;
            PatientRequest patientRequest = new(PhsaHdidNotFound, false);

            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{Hdid}")).Returns(cachedPatient);

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(PhsaHdidNotFound, It.IsAny<CancellationToken>()))!.Throws(
                RefitExceptionUtil.CreateApiException(HttpStatusCode.NotFound).Result);

            HdidPhsaStrategy hdidPhsaStrategy = GetHdidPhsaStrategy(cacheProvider, patientIdentityApi);

            return new(hdidPhsaStrategy, typeof(NotFoundException), patientRequest);
        }

        private sealed record GetPatientMock(HdidPhsaStrategy Strategy, PatientModel Expected, PatientRequest PatientRequest);

        private sealed record GetPatientHandlesPhsaNullResultMock(HdidPhsaStrategy Strategy, Type ExpectedExceptionType, PatientRequest PatientRequest);

        private sealed record GetPatientHandlesPhsaApiExceptionMock(HdidPhsaStrategy Strategy, Type ExpectedExceptionType, PatientRequest PatientRequest);
    }
}
