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
    using AccountDataAccessTest.Utils;
    using AutoMapper;
    using DeepEqual.Syntax;
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
        private const string PatientCacheDomain = "PatientV2";
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";
        private const string Gender = "Male";

        // PHSA Preferred Names
        private const string PhsaPreferredFirstName = "Ted";
        private const string PhsaPreferredSecondName = "Fido";
        private const string PhsaPreferredThirdName = "Shaw";
        private const string PhsaPreferredLastName = "Rogers";

        // PHSA Legal Names
        private const string PhsaLegalFirstName = "TSN";
        private const string PhsaLegalSecondName = "CTV";
        private const string PhsaLegalThirdName = "City";
        private const string PhsaLegalLastName = "Bell";

        // PHSA Home Address
        private const string PhsaHomeAddressStreetOne = "200 Main Street";
        private const string PhsaHomeAddressCity = "Victoria";
        private const string PhsaHomeAddressProvState = "BC";
        private const string PhsaHomeAddressPostal = "V8V 2L9";
        private const string PhsaHomeAddressCountry = "Canada";

        // PHSA Mail Address
        private const string PhsaMailAddressStreetOne = "200 Sutlej Street";
        private const string PhsaMailAddressStreetTwo = "Suite 303";
        private const string PhsaMailAddressStreetThree = "Buzz 303";
        private const string PhsaMailAddressCity = "Victoria";
        private const string PhsaMailAddressProvState = "BC";
        private const string PhsaMailAddressPostal = "V8V 2L9";
        private const string PhsaMailAddressCountry = "Canada";

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// GetDemographics by PHN - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetDemographicsByPhn()
        {
            // Arrange
            PatientDetailsQuery patientDetailsQuery = new(Phn, Source: PatientDetailSource.AllCache);

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
            PatientDetailsQuery patientDetailsQuery = new(Hdid: Hdid, Source: PatientDetailSource.AllCache);

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
            PatientDetailsQuery patientDetailsQuery = new(Hdid: Hdid, Source: PatientDetailSource.AllCache);

            PatientModel? patient = null;

            PatientModel cachedPatient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientIdentityResult? patientResult = null;

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery, patientResult, cachedPatient);

            // Act
            PatientQueryResult result = await patientRepository.Query(patientDetailsQuery, CancellationToken.None).ConfigureAwait(true);

            // Verify
            Assert.Equal(Phn, result.Items.SingleOrDefault()?.Phn);
        }

        /// <summary>
        /// GetPatientIdentity by Hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetPatientIdentityByHdid()
        {
            const string delimiter = " ";

            string expectedPreferredGivenName = PhsaPreferredFirstName + delimiter + PhsaPreferredSecondName + delimiter + PhsaPreferredThirdName;
            string expectedPreferredSurname = PhsaPreferredLastName;

            string expectedCommonGivenName = expectedPreferredGivenName;
            string expectedCommonSurname = expectedPreferredSurname;

            string expectedLegalGivenName = PhsaLegalFirstName + delimiter + PhsaLegalSecondName + delimiter + PhsaLegalThirdName;
            string expectedLegalSurname = PhsaLegalLastName;

            PatientModel expectedPatient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
                Gender = Gender,
                ResponseCode = string.Empty,
                IsDeceased = false,
                CommonName = new Name
                    { GivenName = expectedCommonGivenName, Surname = expectedCommonSurname },
                LegalName = new Name
                    { GivenName = expectedLegalGivenName, Surname = expectedLegalSurname },
                PhysicalAddress = new Address
                {
                    StreetLines = new List<string>
                    {
                        PhsaHomeAddressStreetOne,
                    },
                    City = PhsaHomeAddressCity,
                    State = PhsaHomeAddressProvState,
                    PostalCode = PhsaHomeAddressPostal,
                    Country = PhsaHomeAddressCountry,
                },
                PostalAddress = new Address
                {
                    StreetLines = new List<string>
                    {
                        PhsaMailAddressStreetOne,
                        PhsaMailAddressStreetTwo,
                        PhsaMailAddressStreetThree,
                    },
                    City = PhsaMailAddressCity,
                    State = PhsaMailAddressProvState,
                    PostalCode = PhsaMailAddressPostal,
                    Country = PhsaMailAddressCountry,
                },
            };

            // Arrange
            PatientDetailsQuery patientDetailsQuery = new(Hdid: Hdid, Source: PatientDetailSource.AllCache);

            PatientModel? patient = null;
            PatientModel? cachedPatient = null;

            PatientIdentity patientIdentity = new()
            {
                Phn = Phn,
                HdId = Hdid,
                Gender = Gender,
                PreferredFirstName = PhsaPreferredFirstName,
                PreferredSecondName = PhsaPreferredSecondName,
                PreferredThirdName = PhsaPreferredThirdName,
                PreferredLastName = PhsaPreferredLastName,
                LegalFirstName = PhsaLegalFirstName,
                LegalSecondName = PhsaLegalSecondName,
                LegalThirdName = PhsaLegalThirdName,
                LegalLastName = PhsaLegalLastName,
                HomeAddressStreetOne = PhsaHomeAddressStreetOne,
                HomeAddressCity = PhsaHomeAddressCity,
                HomeAddressProvState = PhsaHomeAddressProvState,
                HomeAddressPostal = PhsaHomeAddressPostal,
                HomeAddressCountry = PhsaHomeAddressCountry,
                MailAddressStreetOne = PhsaMailAddressStreetOne,
                MailAddressStreetTwo = PhsaMailAddressStreetTwo,
                MailAddressStreetThree = PhsaMailAddressStreetThree,
                MailAddressCity = PhsaMailAddressCity,
                MailAddressProvState = PhsaMailAddressProvState,
                MailAddressPostal = PhsaMailAddressPostal,
                MailAddressCountry = PhsaMailAddressCountry,
            };

            PatientIdentityResult patientIdentityResult = new(new PatientIdentityMetadata(), patientIdentity);

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery, patientIdentityResult, cachedPatient);

            // Act
            PatientQueryResult actual = await patientRepository.Query(patientDetailsQuery, CancellationToken.None).ConfigureAwait(true);

            // Verify
            expectedPatient.ShouldDeepEqual(actual.Items.SingleOrDefault());
        }

        /// <summary>
        /// Client registry get demographics throws api patient exception given client registry records not found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetDemographicsThrowsProblemDetailsExceptionGivenClientRegistryPhnNotValid()
        {
            // Arrange
            const string invalidPhn = "abc123";
            PatientDetailsQuery patientQuery = new(invalidPhn, Source: PatientDetailSource.AllCache);

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

        private static PatientRepository GetPatientRepository(
            PatientModel patient,
            PatientDetailsQuery patientDetailsQuery,
            PatientIdentityResult? patientIdentityResult = null,
            PatientModel? cachedPatient = null)
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "PatientService:CacheTTL", "90" },
            };

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();

            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{patientDetailsQuery.Hdid}")).Returns(cachedPatient);

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(p => p.PatientLookupByHdidAsync(patientDetailsQuery.Hdid)).ReturnsAsync(patientIdentityResult);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, patientDetailsQuery.Hdid, false)).ReturnsAsync(patient);
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Phn, patientDetailsQuery.Phn, false)).ReturnsAsync(patient);

            PatientRepository patientRepository = new(
                clientRegistriesDelegate.Object,
                cacheProvider.Object,
                configuration,
                new Mock<ILogger<PatientRepository>>().Object,
                patientIdentityApi.Object,
                Mapper);
            return patientRepository;
        }
    }
}
