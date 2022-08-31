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
    using DeepEqual.Syntax;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.CommonTests.Utils;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// CommunicationService's Unit Tests.
    /// </summary>
    public class CommunicationServiceTests
    {
        private const string Update = "UPDATE";
        private const string Insert = "INSERT";
        private const string Delete = "DELETE";

        /// <summary>
        /// Validates the Communication is pulled from the MemoryCache.
        /// </summary>
        /// <param name="scenario">The scenario the test is executing.</param>
        [Theory]
        [InlineData(Scenario.Active)]
        [InlineData(Scenario.Future)]
        public void ShouldGetActiveCommunicationFromCache(Scenario scenario)
        {
            MemoryCacheEntryOptions options = new()
            {
                AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(1)),
            };

            Communication communication = new()
            {
                Id = Guid.NewGuid(),
                ExpiryDateTime = DateTime.UtcNow.AddDays(10),
            };

            switch (scenario)
            {
                case Scenario.Future:
                    communication.EffectiveDateTime = DateTime.UtcNow.AddDays(3);
                    break;
                default:
                    communication.EffectiveDateTime = DateTime.UtcNow;
                    break;
            }

            RequestResult<Communication?> commResult = GetCommResult(communication, ResultType.Success);
            IMemoryCache memoryCache = CreateCache(commResult, CommunicationService.BannerCacheKey, options);
            ICacheProvider cacheProvider = new MemoryCacheProvider(memoryCache);

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                cacheProvider);

            RequestResult<Communication?> actualResult = service.GetActiveCommunication(CommunicationType.Banner);

            switch (scenario)
            {
                case Scenario.Future:
                    Assert.NotSame(commResult, actualResult);
                    Assert.True(actualResult.ResultStatus == ResultType.Success);
                    break;
                default:
                    Assert.Same(commResult, actualResult);
                    break;
            }

            memoryCache.Dispose();
        }

        /// <summary>
        /// Get the communication data directly from the DB.
        /// </summary>
        /// <param name="dbStatusCode">The status returned from the DB Delegate.</param>
        [Theory]
        [InlineData(DBStatusCode.Read)]
        [InlineData(DBStatusCode.NotFound)]
        [InlineData(DBStatusCode.Error)]
        public void ShouldGetActiveCommunicationFromDb(DBStatusCode dbStatusCode)
        {
            Communication? communication = null;
            if (dbStatusCode == DBStatusCode.Read)
            {
                communication = new()
                {
                    Id = Guid.NewGuid(),
                    EffectiveDateTime = DateTime.Now,
                    ExpiryDateTime = DateTime.Now.AddDays(1),
                };
            }

            DBResult<Communication?> dbResult = new()
            {
                Status = dbStatusCode,
                Payload = communication,
            };

            IMemoryCache memoryCache = CreateCache();
            ICacheProvider cacheProvider = new MemoryCacheProvider(memoryCache);

            Mock<ICommunicationDelegate> communicationDelegateMock = new();
            communicationDelegateMock.Setup(s => s.GetNext(It.IsAny<CommunicationType>())).Returns(dbResult);

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                cacheProvider);

            RequestResult<Communication?> actualResult = service.GetActiveCommunication(CommunicationType.Banner);

            if (dbStatusCode == DBStatusCode.Read || dbStatusCode == DBStatusCode.NotFound)
            {
                Assert.True(actualResult != null);
                Assert.True(actualResult.ResultStatus == ResultType.Success);
                if (dbStatusCode == DBStatusCode.Read)
                {
                    Assert.True(dbResult.Payload.IsDeepEqual(actualResult.ResourcePayload));
                    Assert.True(actualResult.TotalResultCount == 1);
                }
                else
                {
                    Assert.True(actualResult.TotalResultCount == 0);
                }
            }
            else
            {
                Assert.True(actualResult.ResultStatus == ResultType.Error);
                Assert.True(actualResult.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
            }

            memoryCache.Dispose();
        }

        /// <summary>
        /// Validates Insert and Update operations when cache is empty.
        /// </summary>
        /// <param name="action">The action to perform for the test.</param>
        /// <param name="scenario">The scenario to to initialize data.</param>
        /// <param name="cached">Put the initial communication into the cache.</param>
        /// <param name="commType">The optional communication type to use.</param>
        /// <param name="cacheMiss">If true, the cached item will be an empty communication with a unique guid.</param>
        [Theory]
        [InlineData(Insert, Scenario.Active, false)]
        [InlineData(Insert, Scenario.Active, false, CommunicationType.InApp)]
        [InlineData(Insert, Scenario.Expired, false)]
        [InlineData(Update, Scenario.Active, false)]
        [InlineData(Update, Scenario.Expired, false)]
        [InlineData(Insert, Scenario.Active, true)]
        [InlineData(Insert, Scenario.Expired, true)]
        [InlineData(Update, Scenario.Active, true)]
        [InlineData(Update, Scenario.Expired, true)]
        [InlineData(Delete, Scenario.Deleted, true)]
        [InlineData(Insert, Scenario.Active, true, CommunicationType.Banner, true)]
        [InlineData(Insert, Scenario.Future, false)]
        public void ShouldProcessChangeEvent(string action, Scenario scenario, bool cached, CommunicationType commType = CommunicationType.Banner, bool cacheMiss = false)
        {
            Communication communication = new()
            {
                Id = Guid.NewGuid(),
                CommunicationTypeCode = commType,
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

            IMemoryCache memoryCache;
            if (cached)
            {
                RequestResult<Communication?> cacheEntry = new()
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = !cacheMiss
                        ? communication
                        : new()
                        {
                            Id = Guid.NewGuid(),
                        },
                    TotalResultCount = 1,
                };

                memoryCache = CreateCache(cacheEntry, CommunicationService.BannerCacheKey);
            }
            else
            {
                memoryCache = CreateCache();
            }

            ICacheProvider cacheProvider = new MemoryCacheProvider(memoryCache);
            Mock<ICommunicationDelegate> communicationDelegateMock = new();

            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                communicationDelegateMock.Object,
                cacheProvider);

            service.ProcessChange(changeEvent);
            string cacheKey = commType == CommunicationType.Banner ? CommunicationService.BannerCacheKey : CommunicationService.InAppCacheKey;
            RequestResult<Communication>? cacheResult = cacheProvider.GetItem<RequestResult<Communication>>(cacheKey);
            switch (scenario)
            {
                case Scenario.Active:
                    if (!cached || (cached && !cacheMiss))
                    {
                        Assert.Same(communication, cacheResult?.ResourcePayload);
                    }
                    else
                    {
                        Assert.True(communication.Id != cacheResult?.ResourcePayload?.Id);
                    }

                    break;
                case Scenario.Expired:
                    Assert.True(cacheResult?.ResultStatus == ResultType.Success);
                    Assert.True(cacheResult.TotalResultCount == 0);
                    break;
                case Scenario.Deleted:
                    Assert.True(cacheResult is null);
                    break;
                case Scenario.Future:
                    Assert.Same(communication, cacheResult?.ResourcePayload);
                    break;
            }

            memoryCache.Dispose();
        }

        /// <summary>
        /// Validates that GetActiveBanner throws an exception when the wrong Communication types are passed in.
        /// </summary>
        [Fact]
        public void ShouldThrowException()
        {
            ICommunicationService service = new CommunicationService(
                new Mock<ILogger<CommunicationService>>().Object,
                new Mock<ICommunicationDelegate>().Object,
                new Mock<ICacheProvider>().Object);
            Assert.Throws<ArgumentOutOfRangeException>(() => service.GetActiveCommunication(CommunicationType.Email));
        }

        private static IMemoryCache CreateCache(RequestResult<Communication?>? cacheEntry = null, string? cacheKey = null, MemoryCacheEntryOptions? options = null)
        {
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            if (cacheEntry != null && cacheKey != null)
            {
                memoryCache.Set(cacheKey, cacheEntry, options ?? new MemoryCacheEntryOptions());
            }

            return memoryCache;
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
    }
}
