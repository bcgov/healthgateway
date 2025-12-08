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
    // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
    using System.Globalization;
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
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
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
        private const string Hdid = "abc123";
        private const string Phn = "9735353315";
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// GetDemographics by Hdid - happy path.
        /// </summary>
        /// <param name="source">The patient detail source to determine where to query.</param>
        /// <param name="useHdid">The value indicates whether hdid should be used/ If false, then phn is used.</param>
        /// <param name="useCache">The value indicates whether cache should be used or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(PatientDetailSource.Empi, true, true)]
        [InlineData(PatientDetailSource.Empi, true, false)]
        [InlineData(PatientDetailSource.Phsa, true, true)]
        [InlineData(PatientDetailSource.Phsa, true, false)]
        [InlineData(PatientDetailSource.All, true, true)]
        [InlineData(PatientDetailSource.All, true, false)]
        [InlineData(PatientDetailSource.Empi, false, true)]
        [InlineData(PatientDetailSource.Empi, false, false)]
        public async Task ShouldQuery(PatientDetailSource source, bool useHdid, bool useCache)
        {
            // Arrange
            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            PatientIdentity patientIdentity = new()
            {
                Phn = Phn,
                HdId = Hdid,
            };

            PatientModel expected = source == PatientDetailSource.Phsa && !useCache
                ? Mapper.Map<PatientIdentity, PatientModel>(patientIdentity)
                : patient;

            PatientDetailsQuery patientDetailsQuery = new(
                Hdid: useHdid ? Hdid : null,
                Phn: !useHdid ? Phn : null,
                Source: source,
                UseCache: useCache);

            IPatientRepository patientRepository = SetupPatientRepositoryForQuery(patient, patientIdentity);

            // Act
            PatientQueryResult actual = await patientRepository.QueryAsync(patientDetailsQuery, CancellationToken.None);

            // Assert
            actual.Item.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// QueryAsync throws InvalidOperationException.
        /// </summary>
        /// <param name="hdid">The hdid to query on.</param>
        /// <param name="phn">The phn to query on.</param>
        /// <param name="cancellationRequested">The value indicating whether cancellation has been requested.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(null, null, false)]
        [InlineData(Hdid, Phn, true)]
        public async Task QueryThrowsInvalidOperationException(string? hdid, string? phn, bool cancellationRequested)
        {
            // Arrange
            using CancellationTokenSource cancellationTokenSource = new();

            if (cancellationRequested)
            {
                await cancellationTokenSource.CancelAsync();
            }

            CancellationToken ct = cancellationTokenSource.Token;

            PatientDetailsQuery patientDetailsQuery = new(
                Hdid: hdid,
                Phn: phn,
                Source: PatientDetailSource.Empi,
                UseCache: true);

            string expected = ct.IsCancellationRequested ? "Cancellation was requested" : "Must specify either Hdid or Phn to query patient details";

            IPatientRepository patientRepository = SetupPatientRepositoryForQueryThrowsInvalidOperationException();

            // Act and Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => patientRepository.QueryAsync(patientDetailsQuery, ct));
            Assert.Equal(expected, exception.Message);
        }

        /// <summary>
        /// Block access.
        /// </summary>
        /// <param name="changeFeedEnabled">
        /// The value indicates whether blocked data sources change feed should be enabled
        /// or not.
        /// </param>
        /// <param name="datasourceExist">
        /// The value indicates whether datasource(s) exist or not.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task ShouldBlockAccess(bool changeFeedEnabled, bool datasourceExist)
        {
            // Arrange
            const string reason = "Unit Test Block Access";
            bool commit = !changeFeedEnabled;
            IList<DataSource> dataSources = datasourceExist ? [DataSource.Immunization, DataSource.Medication] : [];

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
            (IPatientRepository repository, Mock<IBlockedAccessDelegate> blockedAccessDelegateMock, Mock<IMessageSender> messageSenderMock) = SetupBlockAccessMock(blockedAccess, changeFeedEnabled);

            // Act
            await repository.BlockAccessAsync(command);

            // Verify
            blockedAccessDelegateMock.Verify(
                d => d.UpdateBlockedAccessAsync(
                    It.Is<BlockedAccess>(ba => AssertBlockedAccess(blockedAccess, ba)),
                    It.Is<AgentAudit>(aa => AssertAgentAudit(audit, aa)),
                    commit,
                    It.IsAny<CancellationToken>()),
                datasourceExist ? Times.Once : Times.Never);

            blockedAccessDelegateMock.Verify(
                d => d.DeleteBlockedAccessAsync(
                    It.Is<BlockedAccess>(ba => AssertBlockedAccess(blockedAccess, ba)),
                    It.Is<AgentAudit>(aa => AssertAgentAudit(audit, aa)),
                    commit,
                    It.IsAny<CancellationToken>()),
                !datasourceExist ? Times.Once : Times.Never);

            messageSenderMock.Verify(
                s => s.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(me => AssertDataSourcesBlockedEvent(
                        blockedAccess,
                        me.Select(envelope => envelope.Content as DataSourcesBlockedEvent).First())),
                    It.IsAny<CancellationToken>()),
                changeFeedEnabled ? Times.Once : Times.Never);
        }

        /// <summary>
        /// Can access data source.
        /// </summary>
        /// <param name="dataSource">The data source to check for access.</param>
        /// <param name="useCache">The value indicating whether cache should be used or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DataSource.Note, false)]
        [InlineData(DataSource.Medication, true)]
        public async Task CanAccessDataSource(DataSource dataSource, bool useCache)
        {
            // Arrange
            HashSet<DataSource> dataSources = [DataSource.Immunization, DataSource.Medication];
            bool expected = !dataSources.Contains(dataSource);
            IPatientRepository patientRepository = SetupPatientRepositoryForCanAccessDataSource(dataSources, useCache);

            // Act
            bool actual = await patientRepository.CanAccessDataSourceAsync(Hdid, dataSource);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Get blocked access by hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetBlockedAccess()
        {
            // Arrange
            BlockedAccess expected = new()
            {
                Hdid = Hdid,
                DataSources = [DataSource.Immunization, DataSource.Medication],
            };

            IPatientRepository patientRepository = SetupPatientRepositoryForGetBlockedAccess(expected);

            // Act
            BlockedAccess? actual = await patientRepository.GetBlockedAccessRecordsAsync(Hdid);

            // Verify
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// Get data sources by hdid.
        /// </summary>
        /// <param name="useCache">The value indicating whether cache should be used or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetDataSources(bool useCache)
        {
            // Arrange
            HashSet<DataSource> expected = [DataSource.Immunization, DataSource.Medication];
            IPatientRepository patientRepository = SetupPatientRepositoryForGetDataSources(expected, useCache);

            // Act
            IEnumerable<DataSource> actual = await patientRepository.GetDataSourcesAsync(Hdid);

            // Verify
            actual.ShouldDeepEqual(expected);
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

        private static bool AssertDataSourcesBlockedEvent(BlockedAccess expected, DataSourcesBlockedEvent actual)
        {
            Assert.Equal(expected.Hdid, actual.Hdid);
            Assert.Equal(GetDataSourceValues(expected.DataSources), actual.DataSources);
            return true;
        }

        private static IConfigurationRoot GetIConfigurationRoot(bool blockedDataSourcesEnabled = false)
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "PatientService:CacheTTL", "90" },
                { "BlockedAccess:CacheTtl", "5" },
                { "ChangeFeed:BlockedDataSources:Enabled", blockedDataSourcesEnabled.ToString() },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static IEnumerable<string> GetDataSourceValues(IList<DataSource> dataSources)
        {
            return dataSources.Select(ds => EnumUtility.ToEnumString(ds));
        }

        private static BlockAccessMock SetupBlockAccessMock(BlockedAccess blockedAccess, bool changeFeedEnabled)
        {
            Mock<IBlockedAccessDelegate> blockedAccessDelegate = new();
            blockedAccessDelegate.Setup(s => s.GetBlockedAccessAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(blockedAccess);
            blockedAccessDelegate.Setup(s => s.GetDataSourcesAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(blockedAccess.DataSources);

            Mock<IMessageSender> messageSender = new();

            if (changeFeedEnabled)
            {
                IEnumerable<MessageEnvelope> events =
                [
                    new(
                        new DataSourcesBlockedEvent(blockedAccess.Hdid, GetDataSourceValues(blockedAccess.DataSources)),
                        blockedAccess.Hdid),
                ];
                messageSender.Setup(ms => ms.SendAsync(events, It.IsAny<CancellationToken>()));
            }

            PatientRepository patientRepository = new(
                new Mock<ILogger<PatientRepository>>().Object,
                blockedAccessDelegate.Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<ICacheProvider>().Object,
                GetIConfigurationRoot(changeFeedEnabled),
                new PatientQueryFactory(new Mock<IServiceProvider>().Object),
                messageSender.Object);

            return new(patientRepository, blockedAccessDelegate, messageSender);
        }

        private static IPatientRepository SetupPatientRepositoryForCanAccessDataSource(HashSet<DataSource> dataSources, bool useCache)
        {
            Mock<ICacheProvider> cacheProviderMock = new();
            Mock<IBlockedAccessDelegate> blockedAccessDelegateMock = new();

            if (useCache)
            {
                cacheProviderMock.Setup(s => s.GetOrSetAsync(
                        It.IsAny<string>(),
                        It.IsAny<Func<Task<IEnumerable<DataSource>>>>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dataSources);
            }
            else
            {
                blockedAccessDelegateMock.Setup(s => s.GetDataSourcesAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dataSources);

                cacheProviderMock.Setup(s => s.GetOrSetAsync(
                        It.IsAny<string>(),
                        It.IsAny<Func<Task<IEnumerable<DataSource>>>>(),
                        It.IsAny<TimeSpan?>(), // TimeSpan? for consistency
                        It.IsAny<CancellationToken>()))
                    .Returns((string _, Func<Task<IEnumerable<DataSource>?>> valueFactory, TimeSpan? _, CancellationToken _) =>
                    {
                        Task<IEnumerable<DataSource>?> task = valueFactory.Invoke();
                        return task;
                    });
            }

            return new PatientRepository(
                new Mock<ILogger<PatientRepository>>().Object,
                blockedAccessDelegateMock.Object,
                new Mock<IAuthenticationDelegate>().Object,
                cacheProviderMock.Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(new Mock<IServiceProvider>().Object),
                new Mock<IMessageSender>().Object);
        }

        private static IPatientRepository SetupPatientRepositoryForGetBlockedAccess(BlockedAccess blockedAccess)
        {
            Mock<IBlockedAccessDelegate> blockedAccessDelegate = new();
            blockedAccessDelegate.Setup(s => s.GetBlockedAccessAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(blockedAccess);

            return new PatientRepository(
                new Mock<ILogger<PatientRepository>>().Object,
                blockedAccessDelegate.Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<ICacheProvider>().Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(new Mock<IServiceProvider>().Object),
                new Mock<IMessageSender>().Object);
        }

        private static IPatientRepository SetupPatientRepositoryForGetDataSources(HashSet<DataSource> dataSources, bool useCache)
        {
            string cacheKey = string.Format(CultureInfo.InvariantCulture, ICacheProvider.BlockedAccessCachePrefixKey, Hdid);
            Mock<ICacheProvider> cacheProviderMock = new();
            Mock<IBlockedAccessDelegate> blockedAccessDelegateMock = new();

            if (useCache)
            {
                cacheProviderMock.Setup(s => s.GetOrSetAsync(
                        It.Is<string>(x => x.Contains(cacheKey)),
                        It.IsAny<Func<Task<IEnumerable<DataSource>>>>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dataSources);
            }
            else
            {
                blockedAccessDelegateMock.Setup(s => s.GetDataSourcesAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(dataSources);

                cacheProviderMock.Setup(s => s.GetOrSetAsync(
                        It.Is<string>(x => x.Contains(cacheKey)),
                        It.IsAny<Func<Task<IEnumerable<DataSource>>>>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<CancellationToken>()))
                    .Returns((string _, Func<Task<IEnumerable<DataSource>?>> valueFactory, TimeSpan? _, CancellationToken _) =>
                    {
                        Task<IEnumerable<DataSource>?> task = valueFactory.Invoke();
                        return task;
                    });
            }

            return new PatientRepository(
                new Mock<ILogger<PatientRepository>>().Object,
                blockedAccessDelegateMock.Object,
                new Mock<IAuthenticationDelegate>().Object,
                cacheProviderMock.Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(new Mock<IServiceProvider>().Object),
                new Mock<IMessageSender>().Object);
        }

        private static IPatientRepository SetupPatientRepositoryForQuery(PatientModel patient, PatientIdentity patientIdentity)
        {
            ServiceCollection serviceCollection = [];

            Mock<ICacheProvider> cacheProviderMock = new();
            cacheProviderMock.Setup(s => s.GetItemAsync<PatientModel>(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegate = new();
            clientRegistriesDelegate.Setup(s => s.GetDemographicsAsync(
                    It.IsAny<OidType>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            HdidEmpiStrategy hdidEmpiStrategy = new(
                GetIConfigurationRoot(),
                cacheProviderMock.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<HdidEmpiStrategy>>().Object);

            PhnEmpiStrategy phnEmpiStrategy = new(
                GetIConfigurationRoot(),
                cacheProviderMock.Object,
                clientRegistriesDelegate.Object,
                new Mock<ILogger<PhnEmpiStrategy>>().Object);

            Mock<IPatientIdentityApi> patientIdentityApiMock = new();
            patientIdentityApiMock.Setup(s => s.GetPatientIdentityAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientIdentity);

            HdidAllStrategy hdidAllStrategy = new(
                GetIConfigurationRoot(),
                cacheProviderMock.Object,
                clientRegistriesDelegate.Object,
                patientIdentityApiMock.Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);

            HdidPhsaStrategy hdidPhsaStrategy = new(
                GetIConfigurationRoot(),
                cacheProviderMock.Object,
                patientIdentityApiMock.Object,
                new Mock<ILogger<HdidPhsaStrategy>>().Object,
                Mapper);

            serviceCollection.AddScoped<HdidEmpiStrategy>(_ => hdidEmpiStrategy);
            serviceCollection.AddScoped<PhnEmpiStrategy>(_ => phnEmpiStrategy);
            serviceCollection.AddScoped<HdidAllStrategy>(_ => hdidAllStrategy);
            serviceCollection.AddScoped<HdidPhsaStrategy>(_ => hdidPhsaStrategy);
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            return new PatientRepository(
                new Mock<ILogger<PatientRepository>>().Object,
                new Mock<IBlockedAccessDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<ICacheProvider>().Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(serviceProvider),
                new Mock<IMessageSender>().Object);
        }

        private static IPatientRepository SetupPatientRepositoryForQueryThrowsInvalidOperationException()
        {
            return new PatientRepository(
                new Mock<ILogger<PatientRepository>>().Object,
                new Mock<IBlockedAccessDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<ICacheProvider>().Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(new Mock<IServiceProvider>().Object),
                new Mock<IMessageSender>().Object);
        }

        private sealed record BlockAccessMock(
            PatientRepository PatientRepository,
            Mock<IBlockedAccessDelegate> BlockedAccessDelegate,
            Mock<IMessageSender> MessageSender);
    }
}
