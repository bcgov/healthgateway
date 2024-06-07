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
    using Moq;
    using Xunit;

    /// <summary>
    /// LegalAgreementService unit tests.
    /// </summary>
    public class LegalAgreementServiceTests
    {
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);

        /// <summary>
        /// GetActiveTermsOfServiceAsync call.
        /// </summary>
        /// <param name="legalAgreementFound">The value indicating whether legal agreement was found or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetActiveTermsOfServiceAsync(bool legalAgreementFound)
        {
            // Arrange
            GetActiveTermsOfServiceMock mock = SetupGetActiveTermsOfServiceMock(legalAgreementFound);

            // Act
            RequestResult<TermsOfServiceModel> actual = await mock.LegalAgreementService.GetActiveTermsOfServiceAsync();

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
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
            GetActiveLegalAgreementIdMock mock = SetupGetActiveLegalAgreementIdMock(legalAgreementFound);

            // Act
            Guid? actual = await mock.LegalAgreementService.GetActiveLegalAgreementId(LegalAgreementType.TermsOfService);

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
        }

        private static ActiveTermsOfServiceMock SetupActiveTermsOfServiceMock(bool legalAgreementFound)
        {
            LegalAgreement? legalAgreement = legalAgreementFound
                ? new()
                {
                    Id = Guid.NewGuid(),
                    LegalText = "Test",
                    LegalAgreementCode = LegalAgreementType.TermsOfService,
                    EffectiveDate = DateTime.UtcNow,
                }
                : null;

            Mock<ILegalAgreementDelegate> legalAgreementDelegateMock = new();
            legalAgreementDelegateMock.Setup(
                    s => s.GetActiveByAgreementTypeAsync(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(legalAgreement);

            ILegalAgreementService legalAgreementService = new LegalAgreementService(legalAgreementDelegateMock.Object, MappingService);

            return new(legalAgreementService, legalAgreement);
        }

        private static GetActiveTermsOfServiceMock SetupGetActiveTermsOfServiceMock(bool legalAgreementFound)
        {
            ActiveTermsOfServiceMock mock = SetupActiveTermsOfServiceMock(legalAgreementFound);

            RequestResult<TermsOfServiceModel> expected = new()
            {
                ResultStatus = legalAgreementFound ? ResultType.Success : ResultType.Error,
                ResourcePayload = legalAgreementFound ? MappingService.MapToTermsOfServiceModel(mock.LegalAgreement) : null,
                ResultError = legalAgreementFound
                    ? null
                    : new()
                    {
                        ResultMessage = ErrorMessages.LegalAgreementNotFound,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    },
            };

            return new(mock.LegalAgreementService, expected);
        }

        private static GetActiveLegalAgreementIdMock SetupGetActiveLegalAgreementIdMock(bool legalAgreementFound)
        {
            ActiveTermsOfServiceMock mock = SetupActiveTermsOfServiceMock(legalAgreementFound);
            return new(mock.LegalAgreementService, mock.LegalAgreement?.Id);
        }

        private sealed record ActiveTermsOfServiceMock(ILegalAgreementService LegalAgreementService, LegalAgreement? LegalAgreement);

        private sealed record GetActiveTermsOfServiceMock(ILegalAgreementService LegalAgreementService, RequestResult<TermsOfServiceModel> Expected);

        private sealed record GetActiveLegalAgreementIdMock(ILegalAgreementService LegalAgreementService, Guid? Expected);
    }
}
