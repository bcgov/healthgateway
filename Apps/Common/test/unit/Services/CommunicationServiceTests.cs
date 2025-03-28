//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.CommonTests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.CommonTests.Utils;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Npgsql;
    using Xunit;

    /// <summary>
    /// CommunicationService's Unit Tests.
    /// </summary>
    public class CommunicationServiceTests
    {
        private const string Update = "UPDATE";
        private const string Insert = "INSERT";
        private const string Delete = "DELETE";
        private readonly DistributedCacheProvider cacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationServiceTests"/> class.
        /// </summary>
        public CommunicationServiceTests()
        {
            MemoryDistributedCache cache = new(Options.Create(new MemoryDistributedCacheOptions()));
            this.cacheProvider = new DistributedCacheProvider(cache);
        }

        /// <summary>
        /// Validates the Communication is pulled from the MemoryCache.
        /// </summary>
        /// <param name="scenario">The scenario the test is executing.</param>
        /// <param name="communicationType">The type of Communication to retrieve.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(Scenario.Active, CommunicationType.Banner)]
        [InlineData(Scenario.Future, CommunicationType.Banner)]
        [InlineData(Scenario.Active, CommunicationType.InApp)]
        [InlineData(Scenario.Future, CommunicationType.InApp)]
        [InlineData(Scenario.Active, CommunicationType.Mobile)]
        [InlineData(Scenario.Future, CommunicationType.Mobile)]
        public async Task ShouldGetActiveCommunicationFromCache(Scenario scenario, CommunicationType communicationType)
        {
            Communication communication = new()
            {
                Id = Guid.NewGuid(),
                CommunicationTypeCode = communicationType,
                ExpiryDateTime = DateTime.UtcNow.AddDays(10),
                EffectiveDateTime = scenario switch
                {
                    Scenario.Future => DateTime.UtcNow.AddDays(3),
                    Scenario.Active => DateTime.UtcNow,
                    Scenario.Expired => DateTime.UtcNow,
                    Scenario.Deleted => DateTime.UtcNow,
                    Scenario.DifferentCache => DateTime.UtcNow,
                    _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unhandled scenario value"),
                },
            };

            RequestResult<Communication?> commResult = GetCommResult(communication, ResultType.Success);
            string cacheKey = CommunicationService.GetCacheKey(communicationType);
            this.CreateCache(commResult, cacheKey, TimeSpan.FromDays(1));

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                this.cacheProvider);

            RequestResult<Communication?> actualResult = await service.GetActiveCommunicationAsync(communicationType);

            switch (scenario)
            {
                case Scenario.Future:
                    Assert.NotSame(commResult, actualResult);
                    Assert.Equal(ResultType.Success, actualResult.ResultStatus);
                    break;

                case Scenario.Active:
                case Scenario.Expired:
                case Scenario.Deleted:
                case Scenario.DifferentCache:
                    Assert.Equal(commResult.ResourcePayload!.Id, actualResult.ResourcePayload!.Id);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario), $"Unexpected scenario: {scenario}");
            }
        }

        /// <summary>
        /// Get the communication data directly from the DB.
        /// </summary>
        /// <param name="communicationExists">Indicates whether there is a communication record or not in the database.</param>
        /// <param name="communicationType">The type of Communication to retrieve.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, CommunicationType.Banner)]
        [InlineData(false, CommunicationType.Banner)]
        [InlineData(true, CommunicationType.InApp)]
        [InlineData(false, CommunicationType.InApp)]
        [InlineData(true, CommunicationType.Mobile)]
        [InlineData(false, CommunicationType.Mobile)]
        public async Task ShouldGetActiveCommunicationFromDb(bool communicationExists, CommunicationType communicationType)
        {
            Communication? communication = null;
            if (communicationExists)
            {
                communication = new()
                {
                    Id = Guid.NewGuid(),
                    CommunicationTypeCode = communicationType,
                    EffectiveDateTime = DateTime.Now,
                    ExpiryDateTime = DateTime.Now.AddDays(1),
                };
            }

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.GetNextAsync(It.IsAny<CommunicationType>(), It.IsAny<CancellationToken>())).ReturnsAsync(communication);

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                this.cacheProvider);

            RequestResult<Communication?> actualResult = await service.GetActiveCommunicationAsync(communicationType);

            Assert.NotNull(actualResult);
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            if (communicationExists)
            {
                actualResult.ResourcePayload.ShouldDeepEqual(communication);
                Assert.Equal(1, actualResult.TotalResultCount);
            }
            else
            {
                Assert.Equal(0, actualResult.TotalResultCount);
            }
        }

        /// <summary>
        /// GetActiveCommunicationAsync - DB Exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetActiveCommunicationDbExceptionShouldReturnError()
        {
            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.GetNextAsync(It.IsAny<CommunicationType>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NpgsqlException());

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                this.cacheProvider);

            RequestResult<Communication?> actualResult = await service.GetActiveCommunicationAsync(CommunicationType.Banner);

            Assert.NotNull(actualResult);
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        // ReSharper disable once CognitiveComplexity

        /// <summary>
        /// Validates Insert and Update operations when cache is empty.
        /// </summary>
        /// <param name="action">The action to perform for the test.</param>
        /// <param name="scenario">The scenario to initialize data.</param>
        /// <param name="cached">Put the initial communication into the cache.</param>
        /// <param name="communicationType">The optional communication type to use.</param>
        /// <param name="cacheMiss">If true, the cached item will be an empty communication with a unique guid.</param>
        [Theory]
        [InlineData(Insert, Scenario.Active, false)]
        [InlineData(Insert, Scenario.Active, false, CommunicationType.InApp)]
        [InlineData(Insert, Scenario.Expired, false)]
        [InlineData(Insert, Scenario.Expired, false, CommunicationType.Mobile)]
        [InlineData(Update, Scenario.Active, false)]
        [InlineData(Update, Scenario.Expired, false)]
        [InlineData(Insert, Scenario.Active, true)]
        [InlineData(Insert, Scenario.Expired, true)]
        [InlineData(Update, Scenario.Active, true)]
        [InlineData(Update, Scenario.Expired, true)]
        [InlineData(Delete, Scenario.Deleted, true)]
        [InlineData(Insert, Scenario.Active, true, CommunicationType.Banner, true)]
        [InlineData(Insert, Scenario.Future, false)]
        [InlineData(Insert, Scenario.DifferentCache, true)]
        public void ShouldProcessChangeEvent(string action, Scenario scenario, bool cached, CommunicationType communicationType = CommunicationType.Banner, bool cacheMiss = false)
        {
            Communication communication = new()
            {
                Id = Guid.NewGuid(),
                CommunicationTypeCode = communicationType,
            };
            switch (scenario)
            {
                case Scenario.Expired:
                    communication.EffectiveDateTime = DateTime.Now.AddDays(-10);
                    communication.ExpiryDateTime = DateTime.Now.AddDays(-5);
                    break;

                case Scenario.Future:
                    communication.EffectiveDateTime = DateTime.Now.AddDays(1);
                    communication.ExpiryDateTime = DateTime.Now.AddDays(5);
                    break;

                case Scenario.Active:
                case Scenario.Deleted:
                case Scenario.DifferentCache:
                default:
                    communication.EffectiveDateTime = DateTime.Now;
                    communication.ExpiryDateTime = DateTime.Now.AddDays(1);
                    break;
            }

            BannerChangeEvent changeEvent = new()
            {
                Action = action,
                Data = communication,
            };

            string cacheKey = CommunicationService.GetCacheKey(communicationType);
            if (cached)
            {
                Communication? differentCache = null;

                if (scenario == Scenario.DifferentCache)
                {
                    // Need to change so that UTC now will be before current effective date time and current effective date time is also before cached effective date time
                    differentCache = new()
                    {
                        Id = Guid.NewGuid(),
                        CommunicationTypeCode = communicationType,
                        EffectiveDateTime = DateTime.Now.AddDays(1),
                        ExpiryDateTime = DateTime.Now.AddDays(5),
                    };
                }

                RequestResult<Communication?> cacheEntry = new()
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = !cacheMiss
                        ? scenario == Scenario.DifferentCache ? differentCache : communication
                        : new()
                        {
                            Id = Guid.NewGuid(),
                        },
                    TotalResultCount = 1,
                };

                this.CreateCache(cacheEntry, cacheKey);
            }

            Mock<ICommunicationDelegate> communicationDelegateMock = new();

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                this.cacheProvider);

            service.ProcessChange(changeEvent);
            RequestResult<Communication>? cacheResult = this.cacheProvider.GetItem<RequestResult<Communication>>(cacheKey);
            switch (scenario)
            {
                case Scenario.Active:
                    if (!cached || !cacheMiss)
                    {
                        Assert.Equal(communication.Id, cacheResult!.ResourcePayload!.Id);
                    }
                    else
                    {
                        Assert.NotEqual(communication.Id, cacheResult!.ResourcePayload!.Id);
                    }

                    break;

                case Scenario.Expired:
                    Assert.Equal(ResultType.Success, cacheResult!.ResultStatus);
                    Assert.Equal(0, cacheResult.TotalResultCount);
                    break;

                case Scenario.Deleted:
                    Assert.Null(cacheResult);
                    break;

                case Scenario.Future:
                    Assert.Equal(ResultType.Success, cacheResult!.ResultStatus);
                    Assert.Equal(0, cacheResult.TotalResultCount);
                    break;

                case Scenario.DifferentCache:
                    Assert.Equal(ResultType.Success, cacheResult!.ResultStatus);
                    Assert.Equal(1, cacheResult.TotalResultCount);
                    break;

                default:
                    throw new InvalidOperationException($"Unhandled scenario: {scenario}");
            }
        }

        /// <summary>
        /// Validates that GetActiveBanner throws an exception when the wrong Communication types are passed in.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldThrowException()
        {
            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                new Mock<ICommunicationDelegate>().Object,
                new Mock<ICacheProvider>().Object);
            await Assert.ThrowsAsync<NotImplementedException>(() => service.GetActiveCommunicationAsync(CommunicationType.Email));
        }

        /// <summary>
        /// ClearCache - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateClearCache()
        {
            // Arrange
            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                new Mock<ICommunicationDelegate>().Object,
                this.cacheProvider);

            Communication communication = new();
            this.cacheProvider.AddItem(CommunicationService.BannerCacheKey, communication);
            this.cacheProvider.AddItem(CommunicationService.InAppCacheKey, communication);
            this.cacheProvider.AddItem(CommunicationService.MobileCacheKey, communication);

            // Act
            service.ClearCache();

            // Assert
            Assert.Null(this.cacheProvider.GetItem<Communication>(CommunicationService.BannerCacheKey));
            Assert.Null(this.cacheProvider.GetItem<Communication>(CommunicationService.InAppCacheKey));
            Assert.Null(this.cacheProvider.GetItem<Communication>(CommunicationService.MobileCacheKey));
        }

        private static RequestResult<Communication?> GetCommResult(Communication? communication, ResultType resultStatus)
        {
            RequestResult<Communication?> commResult = new()
            {
                ResultStatus = resultStatus,
                TotalResultCount = communication is null ? 0 : 1,
                ResourcePayload = communication,
            };

            return commResult;
        }

        private void CreateCache(RequestResult<Communication?>? cacheEntry = null, string? cacheKey = null, TimeSpan? expiry = null)
        {
            if (cacheEntry != null && cacheKey != null)
            {
                this.cacheProvider.AddItem(cacheKey, cacheEntry, expiry);
            }
        }
    }
}
