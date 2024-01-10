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
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Hdid Empi Strategy Unit Tests.
    /// </summary>
    public class HdidEmpiStrategyTests
    {
        private const string PatientCacheDomain = "PatientV2";
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";

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

            HdidEmpiStrategy hdidEmpiStrategy = useCache ? GetHdidEmpiStrategy(patient, cachedPatient) : GetHdidEmpiStrategy(patient);

            PatientRequest request = new(Hdid, useCache);

            // Act
            PatientModel? result = await hdidEmpiStrategy.GetPatientAsync(request);

            // Verify
            Assert.Equal(Hdid, result?.Hdid);
            Assert.Equal(Phn, result?.Phn);
        }

        private static HdidEmpiStrategy GetHdidEmpiStrategy(
            PatientModel patient,
            PatientModel? cachedPatient = null)
        {
            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{Hdid}")).Returns(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, Hdid, false, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

            HdidEmpiStrategy hdidEmpiStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<HdidEmpiStrategy>>().Object);
            return hdidEmpiStrategy;
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
