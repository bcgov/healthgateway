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
    using System.ServiceModel;
    using AccountDataAccessTest.Utils;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Hdid All (both EMPI and PHSA) Strategy Unit Tests.
    /// </summary>
    public class HdidAllStrategyTests
    {
        private const string PatientCacheDomain = "PatientV2";
        private const string Hdid = "abc123";
        private const string PhsaHdid = "phsa123";
        private const string PhsaHdidNotFound = "phsa123NotFound";
        private const string Phn = "9735353315";
        private const string Gender = "Male";

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// GetPatientAsync by hdid - happy path.
        /// </summary>
        /// <param name="useCache">The value indicates whether cache should be used or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldGetPatientAsync(bool useCache)
        {
            // Arrange
            GetPatientMock mock = SetupGetPatientMock(useCache);

            // Act
            PatientModel actual = await mock.Strategy.GetPatientAsync(mock.PatientRequest);

            // Verify
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Get patient identity by hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientAsyncFromPhsa()
        {
            // Arrange
            GetPatientIdentityMock mock = SetupGetPatientIdentityMock();

            // Act
            PatientModel actual = await mock.Strategy.GetPatientAsync(mock.PatientRequest);

            // Verify
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Get patient identity by hdid handles client registry and phsa api exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientAsyncHandlesPatientIdentityException()
        {
            // Arrange
            GetPatientHandlesExceptionMock mock = SetupGetPatientHandlesExceptionMock();

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

        private static HdidAllStrategy GetHdidAllStrategy(IMock<ICacheProvider> cacheProvider, IMock<IClientRegistriesDelegate> clientRegistriesDelegate, IMock<IPatientIdentityApi> patientIdentityApi)
        {
            return new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);
        }

        private static GetPatientMock SetupGetPatientMock(bool useCache)
        {
            PatientRequest patientRequest = new(Hdid, useCache);

            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientModel? cachedPatient = useCache ? patient : null;

            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{Hdid}")).Returns(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, Hdid, false, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

            HdidAllStrategy hdidAllStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<IPatientIdentityApi>().Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);

            return new(hdidAllStrategy, patient, patientRequest);
        }

        private static GetPatientIdentityMock SetupGetPatientIdentityMock()
        {
            PatientRequest patientRequest = new(PhsaHdid, false);

            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = PhsaHdid,
                Gender = Gender,
                ResponseCode = string.Empty,
                IsDeceased = false,
                CommonName = new Name
                    { GivenName = string.Empty, Surname = string.Empty },
                LegalName = new Name
                    { GivenName = string.Empty, Surname = string.Empty },
            };

            PatientModel? cachedPatient = null;

            PatientIdentity patientIdentity = new()
            {
                Phn = patient.Phn,
                HdId = patient.Hdid,
                Gender = patient.Gender,
                HasDeathIndicator = false,
            };

            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{Hdid}")).Returns(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, PhsaHdid, false, It.IsAny<CancellationToken>()))
                .Throws(new CommunicationException("Unit test PHSA get patient identity."));

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(PhsaHdid, It.IsAny<CancellationToken>()))!.ReturnsAsync(patientIdentity);

            HdidAllStrategy hdidAllStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);

            return new(hdidAllStrategy, patient, patientRequest);
        }

        private static GetPatientHandlesExceptionMock SetupGetPatientHandlesExceptionMock()
        {
            PatientRequest patientRequest = new(PhsaHdidNotFound, false);

            PatientModel? cachedPatient = null;

            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{Hdid}")).Returns(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, PhsaHdidNotFound, false, It.IsAny<CancellationToken>()))
                .Throws(new CommunicationException("Unit test PHSA get patient identity. NotFound"));

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(PhsaHdidNotFound, It.IsAny<CancellationToken>()))!.Throws(
                RefitExceptionUtil.CreateApiException(HttpStatusCode.NotFound).Result);

            HdidAllStrategy hdidAllStrategy = GetHdidAllStrategy(cacheProvider, clientRegistriesDelegate, patientIdentityApi);

            return new(hdidAllStrategy, typeof(NotFoundException), patientRequest);
        }

        private sealed record GetPatientMock(HdidAllStrategy Strategy, PatientModel Expected, PatientRequest PatientRequest);

        private sealed record GetPatientIdentityMock(HdidAllStrategy Strategy, PatientModel Expected, PatientRequest PatientRequest);

        private sealed record GetPatientHandlesExceptionMock(HdidAllStrategy Strategy, Type ExpectedExceptionType, PatientRequest PatientRequest);
    }
}
