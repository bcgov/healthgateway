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
    using AccountDataAccessTest.Utils;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
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
        public async Task ShouldQueryAsync(PatientDetailSource source, bool useHdid, bool useCache)
        {
            // Arrange
            QueryMock mock = SetupQueryMock(source, useHdid, useCache);

            // Act
            PatientQueryResult actual = await mock.PatientRepository.QueryAsync(mock.PatientDetailsQuery, CancellationToken.None);

            // Assert
            mock.Expected.ShouldDeepEqual(actual.Items.SingleOrDefault());
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
        public async Task QueryAsyncThrowsInvalidOperationException(string? hdid, string? phn, bool cancellationRequested)
        {
            // Arrange
            using CancellationTokenSource cancellationTokenSource = new();

            if (cancellationRequested)
            {
                await cancellationTokenSource.CancelAsync();
            }

            CancellationToken ct = cancellationTokenSource.Token;
            QueryThrowsInvalidOperationExceptionMock mock = SetupQueryThrowsInvalidOperationExceptionMock(hdid, phn, ct);

            // Act and Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mock.PatientRepository.QueryAsync(mock.PatientDetailsQuery, ct));
            Assert.Equal(mock.Expected, exception.Message);
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
        public async Task ShouldBlockAccessAsync(bool changeFeedEnabled, bool datasourceExist)
        {
            // Arrange
            BlockAccessMock mock = SetupBlockAccessMock(changeFeedEnabled, datasourceExist);

            // Act
            await mock.PatientRepository.BlockAccessAsync(mock.Command);

            // Verify
            mock.BlockedAccessDelegate.Verify(
                d => d.UpdateBlockedAccessAsync(
                    It.Is<BlockedAccess>(ba => AssertBlockedAccess(mock.BlockedAccess, ba)),
                    It.Is<AgentAudit>(aa => AssertAgentAudit(mock.Audit, aa)),
                    mock.Commit,
                    It.IsAny<CancellationToken>()),
                datasourceExist ? Times.Once : Times.Never);

            mock.BlockedAccessDelegate.Verify(
                d => d.DeleteBlockedAccessAsync(
                    It.Is<BlockedAccess>(ba => AssertBlockedAccess(mock.BlockedAccess, ba)),
                    It.Is<AgentAudit>(aa => AssertAgentAudit(mock.Audit, aa)),
                    mock.Commit,
                    It.IsAny<CancellationToken>()),
                !datasourceExist ? Times.Once : Times.Never);

            mock.MessageSender.Verify(
                s => s.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        me => AssertDataSourcesBlockedEvent(
                            mock.BlockedAccess,
                            me.Select(envelope => envelope.Content as DataSourcesBlockedEvent).First())),
                    It.IsAny<CancellationToken>()),
                changeFeedEnabled ? Times.Once : Times.Never);
        }

        /// <summary>
        /// Can access data source.
        /// </summary>
        /// <param name="dataSource">The data source to check for access.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DataSource.Note)]
        [InlineData(DataSource.Medication)]
        public async Task CanAccessDataSourceAsync(DataSource dataSource)
        {
            // Arrange
            CanAccessDatasourceMock mock = SetupCanAccessDatasourceMock(dataSource);

            // Act
            bool actual = await mock.PatientRepository.CanAccessDataSourceAsync(Hdid, mock.DataSource);

            // Assert
            Assert.Equal(mock.Expected, actual);
        }

        /// <summary>
        /// Get blocked access by hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetBlockedAccessAsync()
        {
            // Arrange
            GetBlockedAccessMock mock = SetupGetBlockedAccessMock();

            // Act
            BlockedAccess? actual = await mock.PatientRepository.GetBlockedAccessRecordsAsync(mock.Hdid);

            // Verify
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Get data sources by hdid.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDataSourcesAsync()
        {
            // Arrange
            GetDataSourcesMock mock = SetupGetDataSourcesMock();

            // Act
            IEnumerable<DataSource> actual = await mock.PatientRepository.GetDataSourcesAsync(mock.Hdid);

            // Verify
            mock.Expected.ShouldDeepEqual(actual);
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
                { "BlockedAccess:CacheTtl", "5" },
                { "ChangeFeed:BlockedDataSources:Enabled", blockedDataSourcesEnabled.ToString() },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static IEnumerable<string> GetDataSourceValues(HashSet<DataSource> dataSources)
        {
            return dataSources.Select(ds => EnumUtility.ToEnumString(ds));
        }

        private static BlockAccessMock SetupBlockAccessMock(bool changeFeedEnabled, bool datasourceExist)
        {
            const string reason = "Unit Test Block Access";
            bool commit = !changeFeedEnabled;
            HashSet<DataSource> dataSources = datasourceExist ? [DataSource.Immunization, DataSource.Medication] : [];

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

            Mock<IBlockedAccessDelegate> blockedAccessDelegate = new();
            blockedAccessDelegate.Setup(p => p.GetBlockedAccessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(blockedAccess);
            blockedAccessDelegate.Setup(p => p.GetDataSourcesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(blockedAccess.DataSources);

            BlockAccessCommand command = new(Hdid, dataSources, reason);
            Mock<IMessageSender> messageSender = new();

            if (changeFeedEnabled)
            {
                IEnumerable<MessageEnvelope> events = new MessageEnvelope[]
                {
                    new(new DataSourcesBlockedEvent(blockedAccess.Hdid, GetDataSourceValues(blockedAccess.DataSources)), blockedAccess.Hdid),
                };
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

            return new(patientRepository, blockedAccessDelegate, messageSender, blockedAccess, audit, commit, command);
        }

        private static CanAccessDatasourceMock SetupCanAccessDatasourceMock(DataSource dataSource)
        {
            HashSet<DataSource> dataSources = [DataSource.Immunization, DataSource.Medication];
            Mock<ICacheProvider> cacheProviderMock = new();
            cacheProviderMock.Setup(
                    p =>
                        p.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<DataSource>>>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataSources);

            PatientRepository patientRepository = new(
                new Mock<ILogger<PatientRepository>>().Object,
                new Mock<IBlockedAccessDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                cacheProviderMock.Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(new Mock<IServiceProvider>().Object),
                new Mock<IMessageSender>().Object);

            return new(patientRepository, !dataSources.Contains(dataSource), dataSource);
        }

        private static GetBlockedAccessMock SetupGetBlockedAccessMock()
        {
            BlockedAccess blockedAccess = new()
            {
                Hdid = Hdid,
                DataSources = [DataSource.Immunization, DataSource.Medication],
            };

            Mock<IBlockedAccessDelegate> blockedAccessDelegate = new();
            blockedAccessDelegate.Setup(p => p.GetBlockedAccessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(blockedAccess);

            PatientRepository patientRepository = new(
                new Mock<ILogger<PatientRepository>>().Object,
                blockedAccessDelegate.Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<ICacheProvider>().Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(new Mock<IServiceProvider>().Object),
                new Mock<IMessageSender>().Object);

            return new(patientRepository, blockedAccess, Hdid);
        }

        private static GetDataSourcesMock SetupGetDataSourcesMock()
        {
            HashSet<DataSource> dataSources = [DataSource.Immunization, DataSource.Medication];
            Mock<ICacheProvider> cacheProviderMock = new();
            cacheProviderMock.Setup(
                    p =>
                        p.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<IEnumerable<DataSource>>>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataSources);

            PatientRepository patientRepository = new(
                new Mock<ILogger<PatientRepository>>().Object,
                new Mock<IBlockedAccessDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                cacheProviderMock.Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(new Mock<IServiceProvider>().Object),
                new Mock<IMessageSender>().Object);

            return new(patientRepository, dataSources, Hdid);
        }

        private static QueryMock SetupQueryMock(PatientDetailSource source, bool useHdid, bool useCache)
        {
            PatientDetailsQuery patientDetailsQuery = new(Hdid: useHdid ? Hdid : null, Phn: !useHdid ? Phn : null, Source: source, UseCache: useCache);

            PatientRequest patientRequest = new(useHdid ? Hdid : Phn, useCache);

            PatientModel patient = new()
            {
                Phn = Phn,
                Hdid = Hdid,
            };

            ServiceCollection serviceCollection = [];

            Mock<HdidEmpiStrategy> hdidEmpiStrategyMock = new(
                GetConfiguration(),
                new Mock<ICacheProvider>().Object,
                new Mock<IClientRegistriesDelegate>().Object,
                new Mock<ILogger<HdidEmpiStrategy>>().Object);
            hdidEmpiStrategyMock.Setup(s => s.GetPatientAsync(It.Is<PatientRequest>(x => x == patientRequest), It.IsAny<CancellationToken>())).ReturnsAsync(patient);

            Mock<PhnEmpiStrategy> phnEmpiStrategyMock = new(
                GetConfiguration(),
                new Mock<ICacheProvider>().Object,
                new Mock<IClientRegistriesDelegate>().Object,
                new Mock<ILogger<PhnEmpiStrategy>>().Object);
            phnEmpiStrategyMock.Setup(s => s.GetPatientAsync(It.Is<PatientRequest>(x => x == patientRequest), It.IsAny<CancellationToken>())).ReturnsAsync(patient);

            Mock<HdidAllStrategy> hdidAllStrategyMock = new(
                GetConfiguration(),
                new Mock<ICacheProvider>().Object,
                new Mock<IClientRegistriesDelegate>().Object,
                new Mock<IPatientIdentityApi>().Object,
                new Mock<ILogger<HdidAllStrategy>>().Object,
                Mapper);
            hdidAllStrategyMock.Setup(s => s.GetPatientAsync(It.Is<PatientRequest>(x => x == patientRequest), It.IsAny<CancellationToken>())).ReturnsAsync(patient);

            Mock<HdidPhsaStrategy> hdidPhsaStrategyMock = new(
                GetConfiguration(),
                new Mock<ICacheProvider>().Object,
                new Mock<IPatientIdentityApi>().Object,
                new Mock<ILogger<HdidPhsaStrategy>>().Object,
                Mapper);
            hdidPhsaStrategyMock.Setup(s => s.GetPatientAsync(It.Is<PatientRequest>(x => x == patientRequest), It.IsAny<CancellationToken>())).ReturnsAsync(patient);

            serviceCollection.AddScoped<HdidEmpiStrategy>(_ => hdidEmpiStrategyMock.Object);
            serviceCollection.AddScoped<PhnEmpiStrategy>(_ => phnEmpiStrategyMock.Object);
            serviceCollection.AddScoped<HdidAllStrategy>(_ => hdidAllStrategyMock.Object);
            serviceCollection.AddScoped<HdidPhsaStrategy>(_ => hdidPhsaStrategyMock.Object);
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            PatientRepository patientRepository = new(
                new Mock<ILogger<PatientRepository>>().Object,
                new Mock<IBlockedAccessDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<ICacheProvider>().Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(serviceProvider),
                new Mock<IMessageSender>().Object);

            return new(patientRepository, patient, patientDetailsQuery);
        }

        private static QueryThrowsInvalidOperationExceptionMock SetupQueryThrowsInvalidOperationExceptionMock(string? hdid, string? phn, CancellationToken ct)
        {
            PatientDetailsQuery patientDetailsQuery = new(Hdid: hdid, Phn: phn, Source: PatientDetailSource.Empi, UseCache: true);
            string? expected = string.Empty;

            if (string.IsNullOrEmpty(hdid))
            {
                expected = "Must specify either Hdid or Phn to query patient details";
            }

            if (string.IsNullOrEmpty(phn))
            {
                expected = "Must specify either Hdid or Phn to query patient details";
            }

            if (ct.IsCancellationRequested)
            {
                expected = "cancellation was requested";
            }

            PatientRepository patientRepository = new(
                new Mock<ILogger<PatientRepository>>().Object,
                new Mock<IBlockedAccessDelegate>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<ICacheProvider>().Object,
                GetIConfigurationRoot(),
                new PatientQueryFactory(new Mock<IServiceProvider>().Object),
                new Mock<IMessageSender>().Object);

            return new(patientRepository, expected, patientDetailsQuery);
        }

        private sealed record BlockAccessMock(
            PatientRepository PatientRepository,
            Mock<IBlockedAccessDelegate> BlockedAccessDelegate,
            Mock<IMessageSender> MessageSender,
            BlockedAccess BlockedAccess,
            AgentAudit Audit,
            bool Commit,
            BlockAccessCommand Command);

        private sealed record CanAccessDatasourceMock(PatientRepository PatientRepository, bool Expected, DataSource DataSource);

        private sealed record GetBlockedAccessMock(PatientRepository PatientRepository, BlockedAccess Expected, string Hdid);

        private sealed record GetDataSourcesMock(PatientRepository PatientRepository, IEnumerable<DataSource> Expected, string Hdid);

        private sealed record QueryMock(
            PatientRepository PatientRepository,
            PatientModel Expected,
            PatientDetailsQuery PatientDetailsQuery);

        private sealed record QueryThrowsInvalidOperationExceptionMock(
            PatientRepository PatientRepository,
            string Expected,
            PatientDetailsQuery PatientDetailsQuery);
    }
}
