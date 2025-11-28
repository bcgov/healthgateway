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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using IMemoryCache = Microsoft.Extensions.Caching.Memory.IMemoryCache;

    /// <summary>
    /// LegalAgreementService unit tests.
    /// </summary>
    public class LegalAgreementServiceTests
    {
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);

        /// <summary>
        /// GetActiveTermsOfServiceAsync.
        /// </summary>
        /// <param name="legalAgreementFound">The value indicating whether legal agreement was found or not.</param>
        /// <param name="cacheExists">The value indicating whether to check cache or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(null, true)]
        public async Task ShouldGetActiveTermsOfService(bool? legalAgreementFound, bool cacheExists)
        {
            // Arrange
            LegalAgreement? legalAgreement = legalAgreementFound is true || cacheExists
                ? new()
                {
                    Id = Guid.NewGuid(),
                    LegalText = "Test",
                    LegalAgreementCode = LegalAgreementType.TermsOfService,
                    EffectiveDate = DateTime.UtcNow,
                }
                : null;

            RequestResult<TermsOfServiceModel> expected = GetTermsOfServiceModelResult(legalAgreement);

            ILegalAgreementService service = SetupLegalAgreementService(legalAgreement, cacheExists);

            // Act
            RequestResult<TermsOfServiceModel> actual = await service.GetActiveTermsOfServiceAsync();

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetActiveLegalAgreementId.
        /// </summary>
        /// <param name="legalAgreementFound">The value indicating whether legal agreement was found or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetActiveLegalAgreementId(bool legalAgreementFound)
        {
            // Arrange
            Guid? expected = legalAgreementFound ? Guid.NewGuid() : null;

            LegalAgreement? legalAgreement = legalAgreementFound
                ? new()
                {
                    Id = expected!.Value,
                    LegalText = "Test",
                    LegalAgreementCode = LegalAgreementType.TermsOfService,
                    EffectiveDate = DateTime.UtcNow,
                }
                : null;

            ILegalAgreementService service = SetupLegalAgreementService(legalAgreement);

            // Act
            Guid? actual = await service.GetActiveLegalAgreementId(LegalAgreementType.TermsOfService);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static ILegalAgreementService SetupLegalAgreementService(LegalAgreement? legalAgreement, bool cacheExists = false)
        {
            Mock<ILegalAgreementDelegate> legalAgreementDelegateMock = new();
            legalAgreementDelegateMock.Setup(s => s.GetActiveByAgreementTypeAsync(
                    It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalAgreement);


            Mock<IMemoryCache> memoryCacheMock = new();
            object? cachedTos = cacheExists && legalAgreement != null ? GetTermsOfServiceModelResult(legalAgreement) : null;

            memoryCacheMock.Setup(m => m.TryGetValue(
                    It.IsAny<object>(),
                    out cachedTos))
                .Returns(cacheExists);

            if (!cacheExists)
            {
                Mock<ICacheEntry> cacheEntryMock = new();
                cacheEntryMock.SetupAllProperties();
                cacheEntryMock.Setup(e => e.Dispose());

                memoryCacheMock
                    .Setup(m => m.CreateEntry(It.IsAny<object>()))
                    .Returns(cacheEntryMock.Object);
            }

            return new LegalAgreementService(
                new Mock<ILogger<LegalAgreementService>>().Object,
                GetIConfiguration(),
                legalAgreementDelegateMock.Object,
                MappingService,
                memoryCacheMock.Object);
        }

        private static IConfigurationRoot GetIConfiguration(int cacheTtl = 3600)
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "LegalAgreementService:CacheTTL", cacheTtl.ToString(CultureInfo.InvariantCulture) },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection([.. myConfiguration])
                .Build();
        }

        private static RequestResult<TermsOfServiceModel> GetTermsOfServiceModelResult(LegalAgreement? legalAgreement = null)
        {
            return new()
            {
                ResultStatus = legalAgreement != null ? ResultType.Success : ResultType.Error,
                ResourcePayload = legalAgreement != null ? MappingService.MapToTermsOfServiceModel(legalAgreement) : null,
                ResultError = legalAgreement == null
                    ? new()
                    {
                        ResultMessage = ErrorMessages.LegalAgreementNotFound,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    }
                    : null,
            };
        }
    }
}
