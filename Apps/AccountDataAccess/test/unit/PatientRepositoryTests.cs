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
    using System.Net;
    using System.ServiceModel;
    using AccountDataAccessTest.Utils;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Patient Repository Unit Tests.
    /// </summary>
    public class PatientRepositoryTests
    {
        private const string PatientCacheDomain = "PatientV2";
        private const string Hdid = "abc123";
        private const string PhsaHdid = "phsa123";
        private const string PhsaHdidNotFound = "phsa123NotFound";
        private const string Phn = "9735353315";
        private const string Gender = "Male";

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// GetDemographics by PHN - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetDemographicsByPhn()
        {
            // Arrange
            PatientDetailsQuery patientDetailsQuery = new(Phn, Source: PatientDetailSource.Empi);

            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery);

            // Act
            PatientQueryResult result = await patientRepository.Query(patientDetailsQuery, CancellationToken.None).ConfigureAwait(true);

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
            PatientDetailsQuery patientDetailsQuery = new(Hdid: Hdid, Source: PatientDetailSource.All);

            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery);

            // Act
            PatientQueryResult result = await patientRepository.Query(patientDetailsQuery, CancellationToken.None).ConfigureAwait(true);

            // Verify
            Assert.Equal(Phn, result.Items.SingleOrDefault()?.Phn);
        }

        /// <summary>
        /// GetDemographics by Hdid - using cache.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetDemographicsByHdidUsingCache()
        {
            // Arrange
            PatientDetailsQuery patientDetailsQuery = new(Hdid: Hdid, Source: PatientDetailSource.All);

            PatientModel? patient = null;

            PatientModel cachedPatient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientIdentity? patientResult = null;

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery, patientResult, cachedPatient);

            // Act
            PatientQueryResult result = await patientRepository.Query(patientDetailsQuery, CancellationToken.None).ConfigureAwait(true);

            // Verify
            Assert.Equal(Phn, result.Items.SingleOrDefault()?.Phn);
        }

        /// <summary>
        /// Get patient identity by hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientIdentityByHdid()
        {
            // Arrange
            PatientModel expectedPatient = new()
            {
                Phn = Phn,
                Hdid = PhsaHdid,
                Gender = Gender,
                ResponseCode = string.Empty,
                IsDeceased = false,
                CommonName = new Name
                    { GivenName = string.Empty, Surname = string.Empty },
                LegalName = new Name
                    { GivenName = string.Empty, Surname = string.Empty },
            };

            PatientDetailsQuery patientDetailsQuery = new(Hdid: PhsaHdid, Source: PatientDetailSource.All);

            PatientModel? patient = null;
            PatientModel? cachedPatient = null;

            PatientIdentity patientIdentity = new()
            {
                Phn = Phn,
                HdId = PhsaHdid,
                Gender = Gender,
                HasDeathIndicator = false,
            };

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery, patientIdentity, cachedPatient);

            // Act
            PatientQueryResult actual = await patientRepository.Query(patientDetailsQuery, CancellationToken.None).ConfigureAwait(true);

            // Verify
            expectedPatient.ShouldDeepEqual(actual.Items.SingleOrDefault());
        }

        /// <summary>
        /// Get patient identity by hdid throws not found api exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientIdentityThrowsNotFoundApiException()
        {
            // Arrange
            PatientDetailsQuery patientDetailsQuery = new(Hdid: PhsaHdidNotFound, Source: PatientDetailSource.All);

            PatientModel? patient = null;
            PatientModel? cachedPatient = null;
            PatientIdentity? patientIdentity = null;

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery, patientIdentity, cachedPatient);

            // Act
            PatientQueryResult actual = await patientRepository.Query(patientDetailsQuery, CancellationToken.None).ConfigureAwait(true);

            // Verify
            Assert.Null(actual.Items.SingleOrDefault());
        }

        /// <summary>
        /// Client registry get demographics throws problem details exception given an invalid phn.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsProblemDetailsExceptionGivenClientRegistryPhnNotValid()
        {
            // Arrange
            const string invalidPhn = "abc123";
            PatientDetailsQuery patientQuery = new(invalidPhn, Source: PatientDetailSource.All);

            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientRepository patientRepository = GetPatientRepository(patient, patientQuery);

            // Act
            async Task Actual()
            {
                await patientRepository.Query(patientQuery, CancellationToken.None).ConfigureAwait(true);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual).ConfigureAwait(true);
            Assert.Equal(ErrorMessages.PhnInvalid, exception.ProblemDetails!.Detail);
        }

        private static IConfigurationRoot GetConfiguration()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();
        }

        private static PatientRepository GetPatientRepository(
            PatientModel patient,
            PatientDetailsQuery patientDetailsQuery,
            PatientIdentity? patientIdentity = null,
            PatientModel? cachedPatient = null)
        {
            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{patientDetailsQuery.Hdid}")).Returns(cachedPatient);

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(PhsaHdid))!.ReturnsAsync(patientIdentity);
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(PhsaHdidNotFound)).Throws(MockRefitExceptionHelper.CreateApiException(HttpStatusCode.NotFound, HttpMethod.Get));

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, patientDetailsQuery.Hdid, false)).ReturnsAsync(patient);
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Phn, patientDetailsQuery.Phn, false)).ReturnsAsync(patient);
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, PhsaHdid, false))
                .Throws(new CommunicationException("Unit test PHSA get patient identity."));

            PatientRepository patientRepository = new(
                clientRegistriesDelegate.Object,
                cacheProvider.Object,
                GetConfiguration(),
                new Mock<ILogger<PatientRepository>>().Object,
                patientIdentityApi.Object,
                new Mock<DbBlockedAccessDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IAgentAuditDelegate>().Object,
                Mapper);
            return patientRepository;
        }
    }
}
