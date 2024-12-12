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
            PatientRequest patientRequest = new(Hdid, useCache);

            PatientModel expected = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientModel? cachedPatient = useCache ? expected : null;

            (HdidEmpiStrategy hdidEmpiStrategy, Mock<ICacheProvider> cacheProvider) = SetupCachePatientMock(useCache, expected, cachedPatient);

            // Act
            PatientModel actual = await hdidEmpiStrategy.GetPatientAsync(patientRequest);

            // Assert
            actual.ShouldDeepEqual(expected);

            // Verify
            cacheProvider.Verify(
                v => v.AddItemAsync(
                    It.Is<string>(x => x == $"{PatientCacheDomain}:HDID:{patientRequest.Identifier}"),
                    It.IsAny<PatientModel>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken>()),
                useCache ? Times.AtLeastOnce : Times.Never);

            cacheProvider.Verify(
                v => v.GetItemAsync<PatientModel>(
                    It.Is<string>(x => x == $"{PatientCacheDomain}:HDID:{patientRequest.Identifier}"),
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
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static CachePatientMock SetupCachePatientMock(bool useCache, PatientModel patient, PatientModel? cachedPatient)
        {
            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(
                    s => s.GetItemAsync<PatientModel>(
                        It.Is<string>(x => x == $"{PatientCacheDomain}:HDID:{Hdid}"),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(
                    s => s.GetDemographicsAsync(
                        It.Is<OidType>(x => x == OidType.Hdid),
                        It.Is<string>(x => x == Hdid),
                        It.Is<bool>(x => !x),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            HdidEmpiStrategy hdidEmpiStrategy = new(
                GetConfiguration(useCache),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<HdidEmpiStrategy>>().Object);

            return new(hdidEmpiStrategy, cacheProvider);
        }

        private sealed record CachePatientMock(
            HdidEmpiStrategy Strategy,
            Mock<ICacheProvider> CacheProvider);
    }
}
