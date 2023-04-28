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
namespace AccountDataAccessTest
{
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// ClientRegistriesDelegate's Unit Tests.
    /// </summary>
    public class PatientRepositoryTests
    {
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";

        /// <summary>
        /// GetDemographics by PHN - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetDemographicsByPhn()
        {
            // Arrange
            PatientRepository patientRepository = GetPatientRepository(Phn, Phn);
            PatientQuery patientQuery = new PatientDetailsQuery(Phn: Phn);

            // Act
            PatientQueryResult result = await patientRepository.Query(patientQuery, CancellationToken.None).ConfigureAwait(true);

            // Verify
            Assert.Equal(Phn, result.Items.SingleOrDefault()?.Phn);
        }

        /// <summary>
        /// GetDemographics by Hdid - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetDemographicsByHdid()
        {
            // Arrange
            PatientRepository patientRepository = GetPatientRepository(Phn, Hdid);
            PatientQuery patientQuery = new PatientDetailsQuery(Hdid: Hdid);

            // Act
            PatientQueryResult result = await patientRepository.Query(patientQuery, CancellationToken.None).ConfigureAwait(true);

            // Verify
            Assert.Equal(Phn, result.Items.SingleOrDefault()?.Phn);
        }

        /// <summary>
        /// Client registry get demographics throws api patient exception given client registry records not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsProblemDetailsExceptionGivenClientRegistryPhnNotValid()
        {
            const string invalidPhn = "abc123";

            // Arrange
            PatientRepository patientRepository = GetPatientRepository(Phn, invalidPhn);
            PatientQuery patientQuery = new PatientDetailsQuery(Phn: invalidPhn);

            // Act
            async Task Actual()
            {
                await patientRepository.Query(patientQuery, CancellationToken.None).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.PhnInvalid, exception.ProblemDetails!.Detail);
        }

        private static PatientRepository GetPatientRepository(string expectedPhn, string expectedIdentifier)
        {
            PatientModel patient = new()
            {
                CommonName = new Name
                {
                    GivenName = "John",
                    Surname = "Doe",
                },
                Phn = expectedPhn,
                Hdid = Hdid,
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsAsync(OidType.Hdid, expectedIdentifier, false)).ReturnsAsync(patient);
            patientDelegateMock.Setup(p => p.GetDemographicsAsync(OidType.Phn, expectedIdentifier, false)).ReturnsAsync(patient);

            PatientRepository patientRepository = new(
                patientDelegateMock.Object,
                new Mock<ICacheProvider>().Object,
                configuration,
                new Mock<ILogger<PatientRepository>>().Object,
                new Mock<IPersonalAccountsApi>().Object,
                new Mock<IPatientIdentityApi>().Object);
            return patientRepository;
        }
    }
}
