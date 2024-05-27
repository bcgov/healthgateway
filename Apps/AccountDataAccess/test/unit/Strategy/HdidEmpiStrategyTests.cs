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
            actual.ShouldDeepEqual(mock.Expected);

            // Verify
            mock.CacheProvider.Verify(
                v => v.GetItemAsync<PatientModel>(
                    It.Is<string>(x => x == $"{PatientCacheDomain}:HDID:{mock.PatientRequest.Identifier}"),
                    It.IsAny<CancellationToken>()),
                useCache ? Times.AtLeastOnce : Times.Never);
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
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            HdidEmpiStrategy hdidEmpiStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<HdidEmpiStrategy>>().Object);

            return new(hdidEmpiStrategy, cacheProvider, patient, patientRequest);
        }

        private sealed record GetPatientMock(
            HdidEmpiStrategy Strategy,
            Mock<ICacheProvider> CacheProvider,
            PatientModel Expected,
            PatientRequest PatientRequest);
    }
}
