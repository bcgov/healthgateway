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
    using FluentValidation;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Phn Empi Strategy Unit Tests.
    /// </summary>
    public class PhnEmpiStrategyTests
    {
        private const string PatientCacheDomain = "PatientV2";
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";
        private const string InvalidPhn = "99999555000";

        /// <summary>
        /// GetPatientAsync by phn - happy path.
        /// </summary>
        /// <param name="useCache">The value indicates whether cache should be used or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldGetPatient(bool useCache)
        {
            // Arrange
            PatientRequest patientRequest = new(Phn, useCache);
            PatientModel expected = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            (PhnEmpiStrategy strategy, Mock<ICacheProvider> cacheProvider) = SetupGetPatientMock(useCache, expected);

            // Act
            PatientModel actual = await strategy.GetPatientAsync(patientRequest);

            // Assert
            actual.ShouldDeepEqual(expected);

            // Verify
            cacheProvider.Verify(
                v => v.GetItemAsync<PatientModel>(
                    It.Is<string>(x => x == $"{PatientCacheDomain}:PHN:{patientRequest.Identifier}"),
                    It.IsAny<CancellationToken>()),
                useCache ? Times.AtLeastOnce : Times.Never);
        }

        /// <summary>
        /// Get patient by phn throws not found due to invalid phn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientThrowsException()
        {
            // Arrange
            PatientRequest patientRequest = new(InvalidPhn, false); // Invalid PHN will cause an exception to be thrown.
            Type expected = typeof(ValidationException);

            PhnEmpiStrategy strategy = new(
                GetConfiguration(),
                new Mock<ICacheProvider>().Object,
                new Mock<IClientRegistriesDelegate>().Object,
                new Mock<ILogger<PhnEmpiStrategy>>().Object);

            // Act and Verify
            Exception exception = await Assert.ThrowsAsync(
                expected,
                async () => { await strategy.GetPatientAsync(patientRequest); });

            // Assert
            Assert.Contains(ErrorMessages.PhnInvalid, exception.Message, StringComparison.OrdinalIgnoreCase);
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

        private static PatientMock SetupGetPatientMock(bool useCache, PatientModel patient)
        {
            Mock<ICacheProvider> cacheProvider = new();
            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();

            if (useCache)
            {
                cacheProvider.Setup(
                        s => s.GetItemAsync<PatientModel>(
                            It.Is<string>(x => x == $"{PatientCacheDomain}:PHN:{Phn}"),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(patient);
            }
            else
            {
                clientRegistriesDelegate.Setup(
                        s => s.GetDemographicsAsync(
                            It.Is<OidType>(x => x == OidType.Phn),
                            It.Is<string>(x => x == Phn),
                            It.Is<bool>(x => !x),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(patient);
            }

            PhnEmpiStrategy phnEmpiStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<PhnEmpiStrategy>>().Object);

            return new(phnEmpiStrategy, cacheProvider);
        }

        private sealed record PatientMock(
            PhnEmpiStrategy Strategy,
            Mock<ICacheProvider> CacheProvider);
    }
}
