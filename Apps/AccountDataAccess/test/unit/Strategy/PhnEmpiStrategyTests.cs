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
        public async Task ShouldGetPatientByPhn(bool useCache)
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
                    It.Is<string>(x => x == $"{PatientCacheDomain}:PHN:{mock.PatientRequest.Identifier}"),
                    It.IsAny<CancellationToken>()),
                useCache ? Times.AtLeastOnce : Times.Never);
        }

        /// <summary>
        /// Get patient by phn  throws not found due to invalid phn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPatientThrowsException()
        {
            // Arrange
            GetPatientThrowsExceptionMock mock = SetupGetPatientThrowsExceptionMock();

            // Act and Verify
            Exception exception = await Assert.ThrowsAsync(
                mock.Expected,
                async () => { await mock.Strategy.GetPatientAsync(mock.PatientRequest); });

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
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static PhnEmpiStrategy GetPhnEmpiStrategy(IMock<ICacheProvider> cacheProvider, IMock<IClientRegistriesDelegate> clientRegistriesDelegate)
        {
            return new PhnEmpiStrategy(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<PhnEmpiStrategy>>().Object);
        }

        private static GetPatientMock SetupGetPatientMock(bool useCache)
        {
            PatientRequest patientRequest = new(Phn, useCache);
            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };
            PatientModel? cachedPatient = useCache ? patient : null;

            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(
                    s => s.GetItemAsync<PatientModel>(
                        It.Is<string>(x => x == $"{PatientCacheDomain}:PHN:{Phn}"),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(
                    s => s.GetDemographicsAsync(
                        It.Is<OidType>(x => x == OidType.Phn),
                        It.Is<string>(x => x == Phn),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            PhnEmpiStrategy phnEmpiStrategy = GetPhnEmpiStrategy(cacheProvider, clientRegistriesDelegate);
            return new(phnEmpiStrategy, cacheProvider, patient, patientRequest);
        }

        private static GetPatientThrowsExceptionMock SetupGetPatientThrowsExceptionMock()
        {
            PatientRequest patientRequest = new(InvalidPhn, true);
            PatientModel? cachedPatient = null;
            PatientModel patient = new();

            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(
                    s => s.GetItemAsync<PatientModel>(
                        It.Is<string>(x => x == $"{PatientCacheDomain}:PHN:{Phn}"),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(
                    s => s.GetDemographicsAsync(
                        It.Is<OidType>(x => x == OidType.Phn),
                        It.Is<string>(x => x == Phn),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            PhnEmpiStrategy phnEmpiStrategy = GetPhnEmpiStrategy(cacheProvider, clientRegistriesDelegate);
            return new(phnEmpiStrategy, typeof(ValidationException), patientRequest);
        }

        private sealed record GetPatientMock(
            PhnEmpiStrategy Strategy,
            Mock<ICacheProvider> CacheProvider,
            PatientModel Expected,
            PatientRequest PatientRequest);

        private sealed record GetPatientThrowsExceptionMock(
            PhnEmpiStrategy Strategy,
            Type Expected,
            PatientRequest PatientRequest);
    }
}
