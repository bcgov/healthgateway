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
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Cache Provider used in Strategy Unit Tests.
    /// </summary>
    public class CacheProviderTests
    {
        private const string PatientCacheDomain = "PatientV2";
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";

        /// <summary>
        /// Cache patient when calling get patient.
        /// </summary>
        /// <param name="useCache">The value indicates whether cache should be used or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldCachePatientAsync(bool useCache)
        {
            // Arrange
            CachePatientMock mock = SetupCachePatientMock(useCache);

            // Act
            PatientModel? actual = await mock.Strategy.GetPatientAsync(mock.PatientRequest);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);

            // Verify
            mock.CacheProvider.Verify(
                v => v.AddItemAsync(
                    It.IsAny<string>(),
                    It.IsAny<PatientModel>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken>()),
                useCache ? Times.AtLeastOnce : Times.Never);
        }

        private static IConfigurationRoot GetConfiguration(bool useCache)
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "PatientService:CacheTTL", useCache ? "90" : "0" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static CachePatientMock SetupCachePatientMock(bool useCache)
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

            HdidEmpiStrategy hdidEmpiStrategy = new(
                GetConfiguration(useCache),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<HdidEmpiStrategy>>().Object);

            return new(hdidEmpiStrategy, cacheProvider, patient, patientRequest);
        }

        private sealed record CachePatientMock(HdidEmpiStrategy Strategy, Mock<ICacheProvider> CacheProvider, PatientModel Expected, PatientRequest PatientRequest);
    }
}
