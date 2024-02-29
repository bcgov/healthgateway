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
namespace HealthGateway.ImmunizationTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using HealthGateway.ImmunizationTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// ImmunizationService's Unit Tests.
    /// </summary>
    [SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Ignore broken tests")]
    public class ImmunizationServiceTests
    {
        private const string AntigenName = "HPV-9";
        private const string DiseaseEligibleDateString = "2021-02-02";
        private const string DiseaseName = "Human papillomavirus infection";
        private const string RecommendationSetId = "set-recomendation-id";
        private const string TargetDiseaseCode = "240532009";
        private const string VaccineName = "Human Papillomavirus-HPV9 Vaccine";
        private const string RecommendedVaccinations = $"{AntigenName} ({VaccineName})";

        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IImmunizationMappingService MappingService = new ImmunizationMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        /// <param name="canAccessDataSource">The value indicates whether the immunization data source can be accessed or not.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetImmunizations(bool canAccessDataSource)
        {
            Mock<IImmunizationDelegate> mockDelegate = new();
            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<ImmunizationResponse>
                {
                    LoadState = new PhsaLoadState
                        { RefreshInProgress = false },
                    Result = new ImmunizationResponse(
                        [
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Name = "MockImmunization",
                                OccurrenceDateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                                SourceSystemId = "MockSourceID",
                            },
                        ],
                        []),
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };
            RequestResult<ImmunizationResult> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new ImmunizationResult(
                    MappingService.MapToLoadStateModel(delegateResult.ResourcePayload.LoadState),
                    delegateResult.ResourcePayload.Result.ImmunizationViews.Select(MappingService.MapToImmunizationEvent).ToList(),
                    []),
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunizationsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(delegateResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            IImmunizationService service = new ImmunizationService(mockDelegate.Object, patientRepository.Object, MappingService);

            RequestResult<ImmunizationResult> actualResult = await service.GetImmunizationsAsync(It.IsAny<string>());

            if (canAccessDataSource)
            {
                expectedResult.ShouldDeepEqual(actualResult);
            }
            else
            {
                Assert.Equal(0, actualResult.ResourcePayload?.Immunizations.Count);
                Assert.Equal(0, actualResult.ResourcePayload?.Recommendations.Count);
            }
        }

        /// <summary>
        /// GetImmunization.
        /// </summary>
        /// <param name="expectedResultType"> result type from service.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(ResultType.Success)]
        [InlineData(ResultType.Error)]
        public async Task ShouldGetImmunization(ResultType expectedResultType)
        {
            Mock<IImmunizationDelegate> mockDelegate = new();
            RequestResult<PhsaResult<ImmunizationViewResponse>> delegateResult = new()
            {
                ResultStatus = expectedResultType,
                ResourcePayload = expectedResultType == ResultType.Success
                    ? new PhsaResult<ImmunizationViewResponse>
                    {
                        LoadState = new PhsaLoadState
                            { RefreshInProgress = false },
                        Result = new ImmunizationViewResponse
                        {
                            Id = Guid.NewGuid(),
                            Name = "MockImmunization",
                            OccurrenceDateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                            SourceSystemId = "MockSourceID",
                        },
                    }
                    : null,
                PageIndex = expectedResultType == ResultType.Success ? 0 : null,
                PageSize = expectedResultType == ResultType.Success ? 5 : null,
                TotalResultCount = expectedResultType == ResultType.Success ? 1 : null,
            };

            RequestResult<ImmunizationEvent> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = delegateResult.ResultStatus == ResultType.Success ? MappingService.MapToImmunizationEvent(delegateResult.ResourcePayload!.Result) : null,
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunizationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(delegateResult);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IImmunizationService service = new ImmunizationService(mockDelegate.Object, patientRepository.Object, MappingService);

            RequestResult<ImmunizationEvent> actualResult = await service.GetImmunizationAsync("immz_id");

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GetImmunizations - Happy Path (With Recommendations).
        /// </summary>
        /// <param name="targetDiseaseExists">
        /// bool indicating whether target disease should exist or not in the recommendation
        /// response.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetRecommendation(bool targetDiseaseExists)
        {
            // Arrange
            ImmunizationRecommendationResponse immzRecommendationResponse = GetImmzRecommendationResponse(targetDiseaseExists);
            Mock<IImmunizationDelegate> immunizationDelegate = new();
            RequestResult<PhsaResult<ImmunizationResponse>> immunizationResponse = GetPhsaResult(immzRecommendationResponse);
            RequestResult<ImmunizationResult> expectedResult = new()
            {
                ResultStatus = immunizationResponse.ResultStatus,
                ResourcePayload = new ImmunizationResult(
                    MappingService.MapToLoadStateModel(immunizationResponse.ResourcePayload?.LoadState),
                    [],
                    MappingService.MapToImmunizationRecommendations(immunizationResponse.ResourcePayload?.Result?.Recommendations)),
                PageIndex = immunizationResponse.PageIndex,
                PageSize = immunizationResponse.PageSize,
                TotalResultCount = immunizationResponse.TotalResultCount,
            };

            immunizationDelegate.Setup(s => s.GetImmunizationsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(immunizationResponse);

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IImmunizationService service = new ImmunizationService(immunizationDelegate.Object, patientRepository.Object, MappingService);

            // Act
            RequestResult<ImmunizationResult> actualResult = await service.GetImmunizationsAsync(It.IsAny<string>());

            // Assert
            expectedResult.ShouldDeepEqual(actualResult);
            ImmunizationRecommendation actualRecommendation = actualResult.ResourcePayload?.Recommendations[0]!;
            Assert.Equal(RecommendationSetId, actualRecommendation.RecommendationSetId);
            Assert.Equal(VaccineName, actualRecommendation.Immunization.Name);
            Assert.Equal(AntigenName, actualRecommendation.Immunization.ImmunizationAgents.First().Name);

            if (targetDiseaseExists)
            {
                Assert.Equal(TargetDiseaseCode, actualRecommendation.TargetDiseases[0].Code);
                Assert.Equal(DiseaseName, actualRecommendation.TargetDiseases[0].Name);
                Assert.Empty(actualRecommendation.RecommendedVaccinations);
            }
            else
            {
                Assert.Equal(RecommendedVaccinations, actualRecommendation.RecommendedVaccinations);
                Assert.Empty(actualRecommendation.TargetDiseases);
            }

            Assert.Equal(DateOnly.Parse(DiseaseEligibleDateString, CultureInfo.CurrentCulture), actualRecommendation.DiseaseEligibleDate);
            Assert.Null(actualRecommendation.DiseaseDueDate);
            Assert.Null(actualRecommendation.AgentDueDate);
            Assert.Null(actualRecommendation.AgentEligibleDate);
        }

        /// <summary>
        /// GetImmunizations - Request Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateImmunizationError()
        {
            Mock<IImmunizationDelegate> mockDelegate = new();
            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = "Mock Error",
                    ErrorCode = "MOCK_BAD_ERROR",
                },
            };
            RequestResult<IEnumerable<ImmunizationEvent>> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResultError = delegateResult.ResultError,
            };

            mockDelegate.Setup(s => s.GetImmunizationsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(delegateResult));

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IImmunizationService service = new ImmunizationService(mockDelegate.Object, patientRepository.Object, MappingService);

            RequestResult<ImmunizationResult> actualResult = await service.GetImmunizationsAsync(It.IsAny<string>());

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /*
        /// <summary>
        /// Get a Vaccine Record Proof - Happy Path.
        /// </summary>
        [Fact]
        public void GetVaccineRecordOk()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "BCMailPlus:Endpoint", "https://${HOST}/${ENV}/auth=${TOKEN}/JSON/" },
                { "BCMailPlus:Host", "Host" },
                { "BCMailPlus:JobEnvironment", "JobEnvironment" },
                { "BCMailPlus:JobClass", "JobClass" },
                { "BCMailPlus:SchemaVersion", "SchemaVersion" },
                { "BCMailPlus:BackOffMilliseconds", "10" },
                { "BCMailPlus:MaxRetries", "2" },
            };

            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<VaccineStatusResult>()
                {
                    LoadState = new PhsaLoadState() { RefreshInProgress = false, },
                    Result = new()
                    {
                        Birthdate = null,
                        DoseCount = 2,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        QRCode = new EncodedMedia()
                        {
                            Data = "QR Code",
                            Encoding = string.Empty,
                            Type = string.Empty,
                        },
                        StatusIndicator = VaccineState.AllDosesReceived.ToString(),
                    },
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };

            string hdid = "mock hdid";

            var mockVaccineDelegate = new Mock<IVaccineStatusDelegate>();
            mockVaccineDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));
            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            RequestResult<ReportModel> assetResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Data = "BASE 64 Data",
                    FileName = "filename",
                },
            };
            var mockVaccineProofService = new Mock<IVaccineProofService>();
            mockVaccineProofService.Setup(s => s.GetVaccineProof(It.IsAny<string>(), It.IsAny<VaccineProofRequest>(), It.IsAny<VaccineProofTemplate>())).Returns(Task.FromResult(assetResult));

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(myConfiguration),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                mockVaccineProofService.Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial)).Result;

            Assert.True(actualResult.ResultStatus == ResultType.Success);
            Assert.True(actualResult.ResourcePayload != null && actualResult.ResourcePayload.Document.Data == assetResult.ResourcePayload.Data);
        }

        /// <summary>
        /// A refresh in progress is occurring while we're getting the Vaccine Status.
        /// </summary>
        [Fact]
        public void GetVaccineRecordRIP()
        {
            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<VaccineStatusResult>()
                {
                    LoadState = new PhsaLoadState() { RefreshInProgress = true, },
                    Result = new()
                    {
                        Birthdate = null,
                        DoseCount = 0,
                        FirstName = string.Empty,
                        LastName = string.Empty,
                        QRCode = new EncodedMedia()
                        {
                            Data = string.Empty,
                            Encoding = string.Empty,
                            Type = string.Empty,
                        },
                        StatusIndicator = VaccineState.NotFound.ToString(),
                    },
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };

            string hdid = "mock hdid";

            var mockVaccineDelegate = new Mock<IVaccineStatusDelegate>();
            mockVaccineDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));

            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                new Mock<IVaccineProofService>().Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial)).Result;

            Assert.True(actualResult.ResultStatus == ResultType.ActionRequired && actualResult.ResultError != null);
        }

        /// <summary>
        /// We get an error while pulling the Vaccine Status.
        /// </summary>
        [Fact]
        public void GetVaccineRecordPHSAError()
        {
            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "Unable to generate vaccine proof pdf", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) },
            };

            string hdid = "mock hdid";

            var mockVaccineDelegate = new Mock<IVaccineStatusDelegate>();
            mockVaccineDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));

            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                new Mock<IVaccineProofService>().Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial)).Result;

            vaccineStatusResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// The Vaccine status is in a state where we will not generate a proof.
        /// </summary>
        [Fact]
        public void GetVaccineRecordInvalidState()
        {
            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<VaccineStatusResult>()
                {
                    LoadState = new PhsaLoadState() { RefreshInProgress = false, },
                    Result = new()
                    {
                        StatusIndicator = VaccineState.DataMismatch.ToString(),
                    },
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };

            string hdid = "mock hdid";

            var mockVaccineDelegate = new Mock<IVaccineStatusDelegate>();
            mockVaccineDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));
            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                new Mock<IVaccineProofService>().Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial)).Result;

            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// An unexpected state was returned for the vaccine status.
        /// </summary>
        [Fact]
        public void GetVaccineRecordUnknownState()
        {
            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<VaccineStatusResult>()
                {
                    LoadState = new PhsaLoadState() { RefreshInProgress = false, },
                    Result = new()
                    {
                        StatusIndicator = "999",
                    },
                },
            };

            string hdid = "mock hdid";

            var mockVaccineDelegate = new Mock<IVaccineStatusDelegate>();
            mockVaccineDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));
            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                new Mock<IVaccineProofService>().Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial)).Result;

            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// The request to the Vaccine Proof Service errored out.
        /// </summary>
        [Fact]
        public void GetVaccineRecordServiceProofFailed()
        {
            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<VaccineStatusResult>()
                {
                    LoadState = new PhsaLoadState() { RefreshInProgress = false, },
                    Result = new()
                    {
                        Birthdate = null,
                        DoseCount = 2,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        QRCode = new EncodedMedia()
                        {
                            Data = "QR Code",
                            Encoding = string.Empty,
                            Type = string.Empty,
                        },
                        StatusIndicator = VaccineState.AllDosesReceived.ToString(),
                    },
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };

            string hdid = "mock hdid";

            var mockVaccineDelegate = new Mock<IVaccineStatusDelegate>();
            mockVaccineDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));
            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            RequestResult<ReportModel> assetResult = new()
            {
                ResultStatus = ResultType.Error,
            };
            var mockVaccineProofService = new Mock<IVaccineProofService>();
            mockVaccineProofService.Setup(s => s.GetVaccineProof(It.IsAny<string>(), It.IsAny<VaccineProofRequest>(), It.IsAny<VaccineProofTemplate>())).Returns(Task.FromResult(assetResult));

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                mockVaccineProofService.Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial)).Result;

            Assert.True(actualResult.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// The vaccine status is NotFound where there is no records found.
        /// </summary>
        [Fact]
        public void GetVaccineRecordNoRecordFound()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "BCMailPlus:Endpoint", "https://${HOST}/${ENV}/auth=${TOKEN}/JSON/" },
                { "BCMailPlus:Host", "Host" },
                { "BCMailPlus:JobEnvironment", "JobEnvironment" },
                { "BCMailPlus:JobClass", "JobClass" },
                { "BCMailPlus:SchemaVersion", "SchemaVersion" },
                { "BCMailPlus:BackOffMilliseconds", "10" },
                { "BCMailPlus:MaxRetries", "2" },
            };

            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<VaccineStatusResult>()
                {
                    LoadState = new PhsaLoadState() { RefreshInProgress = false, },
                    Result = new()
                    {
                        Birthdate = null,
                        DoseCount = 2,
                        FirstName = "FirstName",
                        LastName = "LastName",
                        QRCode = new EncodedMedia()
                        {
                            Data = "QR Code",
                            Encoding = string.Empty,
                            Type = string.Empty,
                        },
                        StatusIndicator = VaccineState.NotFound.ToString(),
                    },
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };

            string hdid = "mock hdid";

            var mockVaccineDelegate = new Mock<IVaccineStatusDelegate>();
            mockVaccineDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));

            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
              GetConfiguration(myConfiguration),
              new Mock<ILogger<ImmunizationService>>().Object,
              new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
              new Mock<IVaccineProofService>().Object,
              mockVaccineDelegate.Object,
              mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial)).Result;

            Assert.True(actualResult.ResultStatus == ResultType.ActionRequired && actualResult.ResultError != null);
        }
        */

        private static RequestResult<PhsaResult<ImmunizationResponse>> GetPhsaResult(ImmunizationRecommendationResponse immzRecommendationResponse)
        {
            return new RequestResult<PhsaResult<ImmunizationResponse>>
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<ImmunizationResponse>
                {
                    LoadState = new() { RefreshInProgress = false },
                    Result = new(
                        [],
                        [immzRecommendationResponse]),
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };
        }

        /*
        private static IConfiguration GetConfiguration(Dictionary<string, string>? keyValuePairs = null)
        {
            keyValuePairs ??= new();
            IConfiguration configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(keyValuePairs)
                        .Build();
            return configuration;
        }

        private static Mock<IHttpContextAccessor> CreateValidHttpContext(string token, string userId, string hdid)
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            headerDictionary.Add("referer", "http://localhost/");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("hdid", hdid),
                new Claim("auth_time", "123"),
                new Claim("access_token", token),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = token },
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            return httpContextAccessorMock;
        }
        */

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> configuration = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configuration)
                .Build();
        }

        private static ImmunizationRecommendationResponse GetImmzRecommendationResponse(bool targetDiseaseExists)
        {
            ImmunizationRecommendationResponse immzRecommendationResponse = new()
            {
                ForecastCreationDate = DateOnly.FromDateTime(DateTime.Now),
                RecommendationId = RecommendationSetId,
                RecommendationSourceSystem = "MockSourceSystem",
                RecommendationSourceSystemId = "MockSourceID",
            };

            immzRecommendationResponse.Recommendations.Add(GetRecommendationResponse(targetDiseaseExists));
            immzRecommendationResponse.Recommendations.Add(GetRecommendationResponse(true));
            return immzRecommendationResponse;
        }

        private static RecommendationResponse GetRecommendationResponse(bool targetDiseaseExists)
        {
            RecommendationResponse recommendationResponse = new()
            {
                ForecastStatus =
                {
                    ForecastStatusText = "Eligible",
                },
                TargetDisease = targetDiseaseExists
                    ? new()
                    {
                        TargetDiseaseCodes =
                        {
                            new SystemCode
                            {
                                Code = TargetDiseaseCode,
                                CommonType = "DiseaseCode",
                                Display = DiseaseName,
                                System = "https://ehealthbc.ca/NamingSystem/ca-bc-panorama-immunization-disease-code",
                            },
                        },
                    }
                    : null,
                VaccineCode =
                {
                    VaccineCodeText = VaccineName,
                },
            };
            recommendationResponse.VaccineCode.VaccineCodes.Add(
                new SystemCode
                {
                    Code = "BCYSCT_AN032",
                    CommonType = "AntiGenCode",
                    Display = AntigenName,
                    System = "https://ehealthbc.ca/NamingSystem/ca-bc-panorama-immunization-antigen-code",
                });

            recommendationResponse.DateCriterions.Add(
                new DateCriterion
                {
                    DateCriterionCode = new DateCriterionCode
                    {
                        Text = "Forecast by Disease Eligible Date",
                    },
                    Value = DiseaseEligibleDateString,
                });

            return recommendationResponse;
        }
    }
}
