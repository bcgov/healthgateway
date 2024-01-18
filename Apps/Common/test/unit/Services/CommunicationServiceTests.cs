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
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
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
                    Assert.Equal(communication.Id, cacheResult!.ResourcePayload!.Id);
                    break;
            }
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
