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
    using HealthGateway.Common.Data.ErrorHandling;
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
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldGetPatientByPhn(bool useCache)
        {
            // Arrange
            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientModel cachedPatient = patient;

            PhnEmpiStrategy phnEmpiStrategy = useCache ? GetPhnEmpiStrategy(patient, cachedPatient) : GetPhnEmpiStrategy(patient);

            PatientRequest request = new(Phn, useCache);

            // Act
            PatientModel? result = await phnEmpiStrategy.GetPatientAsync(request).ConfigureAwait(true);

            // Verify
            Assert.Equal(Hdid, result?.Hdid);
            Assert.Equal(Phn, result?.Phn);
        }

        /// <summary>
        /// Get patient by phn  throws not found due to invalid phn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientThrowsBadRequestException()
        {
            // Arrange
            PatientModel patient = new();

            PhnEmpiStrategy phnEmpiStrategy = GetPhnEmpiStrategy(patient);

            PatientRequest request = new(InvalidPhn, true);

            // Act
            async Task Actual()
            {
                await phnEmpiStrategy.GetPatientAsync(request).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.PhnInvalid, exception.ProblemDetails!.Detail);
        }

        private static PhnEmpiStrategy GetPhnEmpiStrategy(
            PatientModel patient,
            PatientModel? cachedPatient = null)
        {
            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:PHN:{Phn}")).Returns(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Phn, Phn, false)).ReturnsAsync(patient);

            Mock<ILogger<PhnEmpiStrategy>> logger = new();

            PhnEmpiStrategy phnEmpiStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<PhnEmpiStrategy>>().Object);
            return phnEmpiStrategy;
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
