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
        public async Task ShouldGetPatient(bool useCache)
        {
            // Arrange
            PatientRequest patientRequest = new(Hdid, useCache);

            PatientModel expected = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            (HdidAllStrategy strategy, Mock<ICacheProvider> cacheProvider) = SetupGetPatientMock(useCache, expected);

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
        /// Get patient identity by hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientFromPhsa()
        {
            // Arrange
            PatientRequest patientRequest = new(PhsaHdid, false);

            PatientModel expected = new()
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

            PatientIdentity patientIdentity = new()
            {
                Phn = expected.Phn,
                HdId = expected.Hdid,
                Gender = expected.Gender,
                HasDeathIndicator = false,
            };

            HdidAllStrategy strategy = SetupHdidAllStrategyForGetPatientFromPhsa(patientIdentity);

            // Act
            PatientModel actual = await strategy.GetPatientAsync(patientRequest);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// Get patient identity by hdid handles client registry and phsa api exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPatientHandlesPatientIdentityException()
        {
            // Arrange
            PatientRequest patientRequest = new(PhsaHdidNotFound, false);
            Type expected = typeof(NotFoundException);
            HdidAllStrategy strategy = SetupHdidAllStrategyForGetPatientHandlesPatientIdentityException();

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
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static PatientMock SetupGetPatientMock(bool useCache, PatientModel patient)
        {
            Mock<ICacheProvider> cacheProvider = new();
            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();

            if (useCache)
            {
                cacheProvider.Setup(
                        s => s.GetItemAsync<PatientModel>(
                            It.Is<string>(x => x == $"{PatientCacheDomain}:HDID:{Hdid}"),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(patient);
            }
            else
            {
                clientRegistriesDelegate.Setup(
                        s => s.GetDemographicsAsync(
                            It.Is<OidType>(x => x == OidType.Hdid),
                            It.Is<string>(x => x == Hdid),
                            It.Is<bool>(x => x == false),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(patient);
            }

            HdidAllStrategy hdidAllStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<IPatientIdentityApi>().Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);

            return new(hdidAllStrategy, cacheProvider);
        }

        private static HdidAllStrategy SetupHdidAllStrategyForGetPatientFromPhsa(PatientIdentity patientIdentity)
        {
            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(
                    s => s.GetDemographicsAsync(
                        It.Is<OidType>(x => x == OidType.Hdid),
                        It.Is<string>(x => x == PhsaHdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .Throws(new CommunicationException("Unit test PHSA get patient identity."));

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(
                    s => s.GetPatientIdentityAsync(
                        It.Is<string>(x => x == PhsaHdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientIdentity);

            return new(
                GetConfiguration(),
                new Mock<ICacheProvider>().Object,
                clientRegistriesDelegate.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);
        }

        private static HdidAllStrategy SetupHdidAllStrategyForGetPatientHandlesPatientIdentityException()
        {
            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(
                    s => s.GetDemographicsAsync(
                        It.Is<OidType>(x => x == OidType.Hdid),
                        It.Is<string>(x => x == PhsaHdidNotFound),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .Throws(new CommunicationException("Unit test PHSA get patient identity. NotFound"));

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(
                    s => s.GetPatientIdentityAsync(
                        It.Is<string>(x => x == PhsaHdidNotFound),
                        It.IsAny<CancellationToken>()))
                .Throws(
                    RefitExceptionUtil.CreateApiException(HttpStatusCode.NotFound).Result);

            return new(
                GetConfiguration(),
                new Mock<ICacheProvider>().Object,
                clientRegistriesDelegate.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);
        }

        private sealed record PatientMock(
            HdidAllStrategy Strategy,
            Mock<ICacheProvider> CacheProvider);
    }
}
