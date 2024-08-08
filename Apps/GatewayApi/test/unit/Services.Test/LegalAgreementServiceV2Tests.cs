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
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Moq;
    using Xunit;

    /// <summary>
    /// LegalAgreementServiceV2 unit tests.
    /// </summary>
    public class LegalAgreementServiceV2Tests
    {
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(
            MapperUtil.InitializeAutoMapper(),
            new Mock<ICryptoDelegate>().Object,
            new Mock<IAuthenticationDelegate>().Object);
        private static readonly Guid LegalAgreementId = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// GetActiveTermsOfServiceAsync call.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetActiveTermsOfServiceAsync()
        {
            // Arrange
            LegalAgreement? legalAgreement = GenerateLegalAgreement(id: LegalAgreementId);
            TermsOfServiceModel expected = MappingService.MapToTermsOfServiceModel(legalAgreement);
            ActiveTermsOfServiceMock mock = SetupActiveTermsOfServiceMock(legalAgreementId: LegalAgreementId);

            // Act
            TermsOfServiceModel actual = await mock.LegalAgreementService.GetActiveTermsOfServiceAsync();

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetActiveTermsOfServiceAsync throws DatabaseException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetActiveTermsOfServiceAsyncThrowsDatabaseException()
        {
            // Arrange
            ActiveTermsOfServiceMock mock = SetupActiveTermsOfServiceMock(false);
            Type expected = typeof(DatabaseException);

            // Act and Assert
            await Assert.ThrowsAsync(
                expected,
                async () => { await mock.LegalAgreementService.GetActiveTermsOfServiceAsync(); });
        }

        /// <summary>
        /// GetActiveTermsOfServiceAsync call.
        /// </summary>
        /// <param name="legalAgreementFound">The value indicating whether legal agreement was found or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetActiveLegalAgreementIdAsync(bool legalAgreementFound)
        {
            // Arrange
            LegalAgreement? legalAgreement = GenerateLegalAgreement(legalAgreementFound, LegalAgreementId);
            Guid? expected = legalAgreement?.Id;
            ActiveTermsOfServiceMock mock = SetupActiveTermsOfServiceMock(legalAgreementFound, LegalAgreementId);

            // Act
            Guid? actual = await mock.LegalAgreementService.GetActiveLegalAgreementId(LegalAgreementType.TermsOfService);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static LegalAgreement? GenerateLegalAgreement(bool legalAgreementFound = true, Guid? id = null)
        {
            return legalAgreementFound
                ? new()
                {
                    Id = id ?? Guid.NewGuid(),
                    LegalText = "Test",
                    LegalAgreementCode = LegalAgreementType.TermsOfService,
                    EffectiveDate = DateTime.UtcNow.Date,
                }
                : null;
        }

        private static ActiveTermsOfServiceMock SetupActiveTermsOfServiceMock(bool legalAgreementFound = true, Guid? legalAgreementId = null)
        {
            LegalAgreement? legalAgreement = GenerateLegalAgreement(legalAgreementFound, legalAgreementId);

            Mock<ILegalAgreementDelegate> legalAgreementDelegateMock = new();
            legalAgreementDelegateMock.Setup(
                    s => s.GetActiveByAgreementTypeAsync(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalAgreement);

            ILegalAgreementServiceV2 legalAgreementService = new LegalAgreementServiceV2(
                legalAgreementDelegateMock.Object,
                MappingService);

            return new(legalAgreementService);
        }

        private sealed record ActiveTermsOfServiceMock(
            ILegalAgreementServiceV2 LegalAgreementService);
    }
}
