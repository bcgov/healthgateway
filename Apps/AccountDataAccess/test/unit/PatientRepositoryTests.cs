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
    using System.Globalization;
    using System.Net;
    using System.ServiceModel;
    using AccountDataAccessTest.Utils;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
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
            PatientDetailsQuery patientDetailsQuery = new(Phn, Source: PatientDetailSource.Empi, UseCache: false);

            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery);

            // Act
            PatientQueryResult result = await patientRepository.Query(patientDetailsQuery, CancellationToken.None);

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
            PatientDetailsQuery patientDetailsQuery = new(Hdid: Hdid, Source: PatientDetailSource.Empi, UseCache: false);

            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery);

            // Act
            PatientQueryResult result = await patientRepository.Query(patientDetailsQuery, CancellationToken.None);

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
            PatientDetailsQuery patientDetailsQuery = new(Hdid: Hdid, Source: PatientDetailSource.Empi, UseCache: true);

            PatientModel? patient = null;

            PatientModel cachedPatient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientIdentity? patientResult = null;

            PatientRepository patientRepository = GetPatientRepository(patient, patientDetailsQuery, patientResult, cachedPatient);

            // Act
            PatientQueryResult result = await patientRepository.Query(patientDetailsQuery, CancellationToken.None);

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
            PatientQueryResult actual = await patientRepository.Query(patientDetailsQuery, CancellationToken.None);

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
            PatientQueryResult actual = await patientRepository.Query(patientDetailsQuery, CancellationToken.None);

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
            PatientDetailsQuery patientQuery = new(invalidPhn, Source: PatientDetailSource.Empi);

            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientRepository patientRepository = GetPatientRepository(patient, patientQuery);

            // Act
            async Task Actual()
            {
                await patientRepository.Query(patientQuery, CancellationToken.None);
            }

            // Verify
            ProblemDetailsException exception = await Assert.ThrowsAsync<ProblemDetailsException>(Actual);
            Assert.Equal(ErrorMessages.PhnInvalid, exception.ProblemDetails!.Detail);
        }

        /// <summary>
        /// Block access.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldBlockAccess()
        {
            // Arrange
            string reason = "Unit Test Block Access";

            Mock<IBlockedAccessDelegate> blockedAccessDelegate = new();
            HashSet<DataSource> dataSources = new()
            {
                DataSource.Immunization,
                DataSource.Medication,
            };

            BlockedAccess blockedAccess = new()
            {
                Hdid = Hdid,
                DataSources = dataSources,
            };

            AgentAudit audit = new()
            {
                Hdid = Hdid,
                Reason = reason,
                OperationCode = AuditOperation.ChangeDataSourceAccess,
                GroupCode = AuditGroup.BlockedAccess,
            };

            BlockAccessCommand command = new(Hdid, dataSources, reason);
            PatientRepository patientRepository = GetPatientRepository(blockedAccess, blockedAccessDelegate);

            // Act
            await patientRepository.BlockAccess(command);

            // Verify
            blockedAccessDelegate.Verify(
                v => v.UpdateBlockedAccessAsync(
                    It.Is<BlockedAccess>(ba => AssertBlockedAccess(blockedAccess, ba)),
                    It.Is<AgentAudit>(aa => AssertAgentAudit(audit, aa))));
        }

        /// <summary>
        /// Can access data source.
        /// </summary>
        /// <param name="dataSource">The data source to check for access.</param>
        /// <param name="canAccessDataSource">The value indicates whether the data source can be accessed or not.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Theory]
        [InlineData(DataSource.Note, true)]
        [InlineData(DataSource.Medication, false)]
        public async Task CanAccessDataSource(DataSource dataSource, bool canAccessDataSource)
        {
            // Arrange
            string hdid = Hdid;

            HashSet<DataSource> dataSources = new()
            {
                DataSource.Immunization,
                DataSource.Medication,
            };

            BlockedAccess blockedAccess = new()
            {
                Hdid = hdid,
                DataSources = dataSources,
            };

            PatientRepository patientRepository = GetPatientRepository(blockedAccess);

            // Act
            bool actual = await patientRepository.CanAccessDataSourceAsync(hdid, dataSource);

            // Verify
            Assert.Equal(canAccessDataSource, actual);
        }

        /// <summary>
        /// Get blocked access by hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetBlockedAccessByHdid()
        {
            // Arrange
            string hdid = Hdid;

            BlockedAccess blockedAccess = new()
            {
                Hdid = hdid,
                DataSources = new HashSet<DataSource>
                {
                    DataSource.Immunization, DataSource.Medication,
                },
            };

            PatientRepository patientRepository = GetPatientRepository(blockedAccess);

            // Act
            BlockedAccess? actual = await patientRepository.GetBlockedAccessRecords(hdid);

            // Verify
            blockedAccess.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Get data sources by hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ShouldGetDataSourcesByHdid()
        {
            // Arrange
            string hdid = Hdid;

            HashSet<DataSource> dataSources = new()
            {
                DataSource.Immunization,
                DataSource.Medication,
            };

            BlockedAccess blockedAccess = new()
            {
                Hdid = hdid,
                DataSources = dataSources,
            };

            PatientRepository patientRepository = GetPatientRepository(blockedAccess);

            // Act
            IEnumerable<DataSource> actual = await patientRepository.GetDataSources(hdid);

            // Verify
            dataSources.ShouldDeepEqual(actual);
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

        private static bool AssertAgentAudit(AgentAudit expected, AgentAudit actual)
        {
            Assert.Equal(expected.Hdid, actual.Hdid);
            Assert.Equal(expected.Reason, actual.Reason);
            Assert.Equal(expected.OperationCode, actual.OperationCode);
            Assert.Equal(expected.GroupCode, actual.GroupCode);
            return true;
        }

        private static bool AssertBlockedAccess(BlockedAccess expected, BlockedAccess actual)
        {
            Assert.Equal(expected.Hdid, actual.Hdid);
            Assert.Equal(expected.DataSources, actual.DataSources);
            return true;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "BlockedAccess:CacheTtl", "5" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static PatientRepository GetPatientRepository(
            BlockedAccess blockedAccess,
            Mock<IBlockedAccessDelegate>? blockedAccessDelegate = null)
        {
            blockedAccessDelegate ??= blockedAccessDelegate ?? new();
            blockedAccessDelegate.Setup(p => p.GetBlockedAccessAsync(It.IsAny<string>())).ReturnsAsync(blockedAccess);
            blockedAccessDelegate.Setup(p => p.GetDataSourcesAsync(It.IsAny<string>())).ReturnsAsync(blockedAccess.DataSources);

            string blockedAccessCacheKey = string.Format(CultureInfo.InvariantCulture, ICacheProvider.BlockedAccessCachePrefixKey, blockedAccess.Hdid);
            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(
                    p => p.GetOrSetAsync(
                        blockedAccessCacheKey,
                        It.IsAny<Func<Task<IEnumerable<DataSource>>>>(),
                        It.IsAny<TimeSpan>()))
                .ReturnsAsync(blockedAccess.DataSources);

            PatientRepository patientRepository = new(
                new Mock<ILogger<PatientRepository>>().Object,
                blockedAccessDelegate.Object,
                new Mock<IAuthenticationDelegate>().Object,
                cacheProvider.Object,
                GetIConfigurationRoot(),
                GetPatientQueryFactory(
                    new PatientModel(),
                    new PatientDetailsQuery(Hdid: Hdid, Source: PatientDetailSource.All, UseCache: false)));
            return patientRepository;
        }

        private static PatientRepository GetPatientRepository(
            PatientModel patient,
            PatientDetailsQuery patientDetailsQuery,
            PatientIdentity? patientIdentity = null,
            PatientModel? cachedPatient = null)
        {
            PatientRepository patientRepository = new(
                new Mock<ILogger<PatientRepository>>().Object,
                new Mock<IBlockedAccessDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<ICacheProvider>().Object,
                GetIConfigurationRoot(),
                GetPatientQueryFactory(patient, patientDetailsQuery, patientIdentity, cachedPatient));
            return patientRepository;
        }

        private static PatientQueryFactory GetPatientQueryFactory(
            PatientModel patient,
            PatientDetailsQuery patientDetailsQuery,
            PatientIdentity? patientIdentity = null,
            PatientModel? cachedPatient = null)
        {
            ServiceCollection serviceCollection = new();

            Mock<ICacheProvider> cacheProvider = new();
            cacheProvider.Setup(p => p.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{patientDetailsQuery.Hdid}")).Returns(cachedPatient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, patientDetailsQuery.Hdid, false)).ReturnsAsync(patient);
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Phn, patientDetailsQuery.Phn, false)).ReturnsAsync(patient);
            clientRegistriesDelegate.Setup(p => p.GetDemographicsAsync(OidType.Hdid, PhsaHdid, false))
                .Throws(new CommunicationException("Unit test PHSA get patient identity."));

            HdidEmpiStrategy hdidEmpiStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<HdidEmpiStrategy>>().Object);
            serviceCollection.AddScoped<HdidEmpiStrategy>(_ => hdidEmpiStrategy);

            PhnEmpiStrategy phnEmpiStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<PhnEmpiStrategy>>().Object);
            serviceCollection.AddScoped<PhnEmpiStrategy>(_ => phnEmpiStrategy);

            Mock<IPatientIdentityApi> patientIdentityApi = new();
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(PhsaHdid))!.ReturnsAsync(patientIdentity);
            patientIdentityApi.Setup(p => p.GetPatientIdentityAsync(PhsaHdidNotFound)).Throws(MockRefitExceptionHelper.CreateApiException(HttpStatusCode.NotFound, HttpMethod.Get));

            HdidAllStrategy hdidAllStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                clientRegistriesDelegate.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);
            serviceCollection.AddScoped<HdidAllStrategy>(_ => hdidAllStrategy);

            HdidPhsaStrategy hdidPhsaStrategy = new(
                GetConfiguration(),
                cacheProvider.Object,
                patientIdentityApi.Object,
                new Mock<ILogger<HdidPhsaStrategy>>().Object,
                Mapper);
            serviceCollection.AddScoped<HdidPhsaStrategy>(_ => hdidPhsaStrategy);

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            return new(serviceProvider);
        }
    }
}
