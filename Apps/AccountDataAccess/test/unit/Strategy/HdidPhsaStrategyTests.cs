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
    using AccountDataAccessTest.Utils;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.CacheProviders;
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

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// GetPatientAsync by hdid - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientByHdid()
        {
            // Arrange
            PatientModel expectedPatient = new()
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

            HdidPhsaStrategy hdidPhsaStrategy = GetHdidPhsaStrategy(patientIdentity);

            PatientRequest request = new(Hdid, false);

            // Act
            PatientModel? actual = await hdidPhsaStrategy.GetPatientAsync(request).ConfigureAwait(true);

            // Verify
            expectedPatient.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// GetPatientAsync by hdid - using cache.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientByHdidUsingCache()
        {
            // Arrange
            PatientModel cachedPatient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientIdentity patientIdentity = new()
            {
                Phn = Phn,
                HdId = Hdid,
                Gender = Gender,
                HasDeathIndicator = false,
            };

            HdidPhsaStrategy hdidPhsaStrategy = GetHdidPhsaStrategy(patientIdentity, cachedPatient);

            PatientRequest request = new(Hdid, true);

            // Act
            PatientModel? result = await hdidPhsaStrategy.GetPatientAsync(request).ConfigureAwait(true);

            // Verify
            Assert.Equal(patientIdentity.HdId, result?.Hdid);
            Assert.Equal(patientIdentity.Phn, result?.Phn);
        }

        private static HdidPhsaStrategy GetHdidPhsaStrategy(
            PatientIdentity patientIdentity,
            PatientModel? cachedPatient = null)
        {
            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{Hdid}")).Returns(cachedPatient);

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(Hdid))!.ReturnsAsync(patientIdentity);

            Mock<ILogger<HdidPhsaStrategy>> logger = new();

            HdidPhsaStrategy hdidPhsaStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidPhsaStrategy>>().Object,
                Mapper);
            return hdidPhsaStrategy;
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
