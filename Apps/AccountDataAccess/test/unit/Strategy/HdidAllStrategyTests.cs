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
    using System.ServiceModel;
    using AccountDataAccessTest.Utils;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
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
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldGetPatientByHdid(bool useCache)
        {
            // Arrange
            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientModel cachedPatient = patient;

            PatientIdentity? patientIdentity = null;

            HdidAllStrategy hdidAllStrategy = useCache ? GetHdidAllStrategy(patient, patientIdentity, cachedPatient) : GetHdidAllStrategy(patient);

            PatientRequest request = new(Hdid, useCache);

            // Act
            PatientModel? result = await hdidAllStrategy.GetPatientAsync(request);

            // Verify
            Assert.Equal(Hdid, result?.Hdid);
            Assert.Equal(Phn, result?.Phn);
        }

        /// <summary>
        /// Get patient identity by hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientIdentityByHdid()
        {
            // Arrange
            PatientModel expectedPatient = new()
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

            PatientModel? patient = null;
            PatientModel? cachedPatient = null;

            PatientIdentity patientIdentity = new()
            {
                Phn = Phn,
                HdId = PhsaHdid,
                Gender = Gender,
                HasDeathIndicator = false,
            };

            PatientRequest request = new(PhsaHdid, false);

            HdidAllStrategy hdidAllStrategy = GetHdidAllStrategy(patient, patientIdentity, cachedPatient);

            // Act
            PatientModel? actual = await hdidAllStrategy.GetPatientAsync(request);

            // Verify
            expectedPatient.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Get patient identity by hdid throws not found api exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientIdentityByHdidThrowsNotFoundApiException()
        {
            // Arrange
            PatientModel? patient = null;
            PatientModel? cachedPatient = null;
            PatientIdentity? patientIdentity = null;

            PatientRequest request = new(PhsaHdidNotFound, false);

            HdidAllStrategy hdidAllStrategy = GetHdidAllStrategy(patient, patientIdentity, cachedPatient);

            // Act
            PatientModel? actual = await hdidAllStrategy.GetPatientAsync(request);

            // Verify
            Assert.Null(actual);
        }

        private static HdidAllStrategy GetHdidAllStrategy(
            PatientModel patient,
            PatientIdentity? patientIdentity = null,
            PatientModel? cachedPatient = null)
        {
            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{Hdid}")).Returns(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, Hdid, false)).ReturnsAsync(patient);
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, PhsaHdid, false))
                .Throws(new CommunicationException("Unit test PHSA get patient identity."));

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(PhsaHdid))!.ReturnsAsync(patientIdentity);

            HdidAllStrategy hdidAllStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);
            return hdidAllStrategy;
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
    }
}
