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
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// ClientRegistriesDelegate's Unit Tests.
    /// </summary>
    public class PatientRepositoryTests
    {
        /// <summary>
        /// Client registry get demographics throws api patient exception given client registry records not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsProblemDetailsExceptionGivenClientRegistryPhnNotValid()
        {
            // Setup
            ApiResult<PatientModel> requestResult = new()
            {
                ResourcePayload = new PatientModel
                {
                    CommonName = new Name
                    {
                        GivenName = "John",
                        Surname = "Doe",
                    },
                    Phn = "abc123",
                    Hdid = "abc123",
                },
            };

            Mock<IClientRegistriesDelegate> patientDelegateMock = new();
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();
            patientDelegateMock.Setup(p => p.GetDemographicsAsync(It.IsAny<OidType>(), It.IsAny<string>(), false)).ReturnsAsync(requestResult);

            PatientRepository patientRepository = new(patientDelegateMock.Object, new Mock<ICacheProvider>().Object, configuration, new Mock<ILogger<PatientRepository>>().Object);
            PatientQuery patientQuery = new PatientDetailsQuery(Phn: "abc123");

            // Act
            async Task Actual()
            {
                await patientRepository.Query(patientQuery, new CancellationToken(false)).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.PhnInvalid, exception.ProblemDetails!.Detail);
        }
    }
}
