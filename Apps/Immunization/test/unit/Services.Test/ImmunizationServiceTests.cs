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
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.MapUtils;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using HealthGateway.ImmunizationTests.Utils;
    using Moq;
    using Xunit;

    /// <summary>
    /// ImmunizationService's Unit Tests.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1506:Avoid excessive class coupling", Justification = "Unit Test")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Ignore broken tests")]
    public class ImmunizationServiceTests
    {
        private readonly string recomendationSetId = "set-recomendation-id";
        private readonly string diseaseEligibleDateString = "2021-02-02";
        private readonly string diseaseName = "Human papillomavirus infection";
        private readonly string vaccineName = "Human Papillomavirus-HPV9 Vaccine";
        private readonly string antigenName = "HPV-9";
        private readonly IMapper autoMapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetImmunizations()
        {
            Mock<IImmunizationDelegate> mockDelegate = new();
            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<ImmunizationResponse>()
                {
                    LoadState = new PhsaLoadState() { RefreshInProgress = false, },
                    Result = new ImmunizationResponse(
                        new List<ImmunizationViewResponse>
                        {
                            new ImmunizationViewResponse()
                            {
                                Id = Guid.NewGuid(),
                                Name = "MockImmunization",
                                OccurrenceDateTime = DateTime.Now,
                                SourceSystemId = "MockSourceID",
                            },
                        },
                        new List<ImmunizationRecommendationResponse>()),
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };
            RequestResult<ImmunizationResult> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new ImmunizationResult(
                    this.autoMapper.Map<LoadStateModel>(delegateResult.ResourcePayload.LoadState),
                    this.autoMapper.Map<IList<ImmunizationEvent>>(delegateResult.ResourcePayload.Result.ImmunizationViews),
                    new List<ImmunizationRecommendation>()),
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>())).ReturnsAsync(delegateResult);
            IImmunizationService service = new ImmunizationService(mockDelegate.Object, this.autoMapper);

            Task<RequestResult<ImmunizationResult>> actualResult = service.GetImmunizations(It.IsAny<string>());

            expectedResult.ShouldDeepEqual(actualResult.Result);
        }

        /// <summary>
        /// GetImmunization - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetImmunization()
        {
            Mock<IImmunizationDelegate> mockDelegate = new();
            RequestResult<PhsaResult<ImmunizationViewResponse>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<ImmunizationViewResponse>()
                {
                    LoadState = new PhsaLoadState() { RefreshInProgress = false, },
                    Result = new ImmunizationViewResponse()
                    {
                        Id = Guid.NewGuid(),
                        Name = "MockImmunization",
                        OccurrenceDateTime = DateTime.Now,
                        SourceSystemId = "MockSourceID",
                    },
                },
                PageIndex = 0,
                PageSize = 5,
                TotalResultCount = 1,
            };

            RequestResult<ImmunizationEvent> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = this.autoMapper.Map<ImmunizationEvent>(delegateResult.ResourcePayload.Result),
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunization(It.IsAny<string>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(mockDelegate.Object, this.autoMapper);

            Task<RequestResult<ImmunizationEvent>> actualResult = service.GetImmunization("immz_id");

            expectedResult.ShouldDeepEqual(actualResult.Result);
        }

        /// <summary>
        /// GetImmunizations - Happy Path (With Recommendations).
        /// </summary>
        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public void ShouldGetRecommendation()
        {
            ImmunizationRecommendationResponse immzRecommendationResponse = this.GetImmzRecommendationResponse();
            Mock<IImmunizationDelegate> mockDelegate = new();
            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = GetPHSAResult(immzRecommendationResponse);
            RequestResult<ImmunizationResult> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new ImmunizationResult(
                    this.autoMapper.Map<LoadStateModel>(delegateResult.ResourcePayload?.LoadState),
                    new List<ImmunizationEvent>(),
                    ImmunizationRecommendationMapUtils.FromPHSAModelList(delegateResult?.ResourcePayload?.Result?.Recommendations, this.autoMapper)),
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(mockDelegate.Object, this.autoMapper);

            Task<RequestResult<ImmunizationResult>> actualResult = service.GetImmunizations(It.IsAny<string>());

            expectedResult.ShouldDeepEqual(actualResult.Result);
            Assert.Equal(1, expectedResult.ResourcePayload.Recommendations.Count);
            ImmunizationRecommendation recomendationResult = expectedResult.ResourcePayload.Recommendations[0];
            Assert.Equal(this.recomendationSetId, recomendationResult.RecommendationSetId);
            Assert.Equal(this.vaccineName, recomendationResult.Immunization.Name);
            Assert.Collection(
                recomendationResult.Immunization.ImmunizationAgents,
                item => Assert.Equal(this.antigenName, item.Name));
            Assert.Equal(this.antigenName, recomendationResult.Immunization.ImmunizationAgents.First().Name);
            Assert.Collection(
                recomendationResult?.TargetDiseases,
                item => Assert.Equal(immzRecommendationResponse.Recommendations.First().TargetDisease?.TargetDiseaseCodes?.FirstOrDefault()?.Code, item.Code));
            Assert.Equal(this.diseaseName, recomendationResult?.TargetDiseases.First().Name);
            Assert.Equal(DateTime.Parse(this.diseaseEligibleDateString, CultureInfo.CurrentCulture), recomendationResult?.DiseaseEligibleDate);
            Assert.Null(recomendationResult?.DiseaseDueDate);
            Assert.Null(recomendationResult?.AgentDueDate);
            Assert.Null(recomendationResult?.AgentEligibleDate);
        }

        /// <summary>
        /// GetImmunizations - Request Error.
        /// </summary>
        [Fact]
        public void ValidateImmunizationError()
        {
            Mock<IImmunizationDelegate> mockDelegate = new();
            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError()
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

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(mockDelegate.Object, this.autoMapper);

            Task<RequestResult<ImmunizationResult>> actualResult = service.GetImmunizations(It.IsAny<string>());

            expectedResult.ShouldDeepEqual(actualResult.Result);
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
            mockVaccineDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));
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

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;

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
            mockVaccineDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));

            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                new Mock<IVaccineProofService>().Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;

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
            mockVaccineDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));

            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                new Mock<IVaccineProofService>().Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;

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
            mockVaccineDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));
            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                new Mock<IVaccineProofService>().Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;

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
            mockVaccineDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));
            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
                GetConfiguration(),
                new Mock<ILogger<ImmunizationService>>().Object,
                new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
                new Mock<IVaccineProofService>().Object,
                mockVaccineDelegate.Object,
                mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;

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
            mockVaccineDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));
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

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;

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
            mockVaccineDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), It.IsAny<string>(), false)).Returns(Task.FromResult(vaccineStatusResult));

            var mockHttpContextAccessor = CreateValidHttpContext("token", "userid", "hdid");

            IImmunizationService service = new ImmunizationService(
              GetConfiguration(myConfiguration),
              new Mock<ILogger<ImmunizationService>>().Object,
              new Mock<Immunization.Delegates.IImmunizationDelegate>().Object,
              new Mock<IVaccineProofService>().Object,
              mockVaccineDelegate.Object,
              mockHttpContextAccessor.Object);

            RequestResult<VaccineProofDocument> actualResult = Task.Run(async () => await service.GetVaccineProof(hdid, VaccineProofTemplate.Provincial).ConfigureAwait(true)).Result;

            Assert.True(actualResult.ResultStatus == ResultType.ActionRequired && actualResult.ResultError != null);
        }
        */

        private static RequestResult<PhsaResult<ImmunizationResponse>> GetPHSAResult(ImmunizationRecommendationResponse immzRecommendationResponse)
        {
            return new RequestResult<PhsaResult<ImmunizationResponse>>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<ImmunizationResponse>()
                {
                    LoadState = new() { RefreshInProgress = false, },
                    Result = new(
                        new List<ImmunizationViewResponse>(),
                        new List<ImmunizationRecommendationResponse>
                        {
                            immzRecommendationResponse,
                        }),
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

        private ImmunizationRecommendationResponse GetImmzRecommendationResponse()
        {
            ImmunizationRecommendationResponse immzRecommendationResponse = new()
            {
                ForecastCreationDate = DateTime.Now,
                RecommendationId = this.recomendationSetId,
                RecommendationSourceSystem = "MockSourceSystem",
                RecommendationSourceSystemId = "MockSourceID",
            };

            RecommendationResponse recommendationResponse = new();
            recommendationResponse.ForecastStatus.ForecastStatusText = "Eligible";
            recommendationResponse.TargetDisease.TargetDiseaseCodes.Add(new SystemCode()
            {
                Code = "240532009",
                CommonType = "DiseaseCode",
                Display = this.diseaseName,
                System = "https://ehealthbc.ca/NamingSystem/ca-bc-panorama-immunization-disease-code",
            });
            recommendationResponse.VaccineCode.VaccineCodeText = this.vaccineName;
            recommendationResponse.VaccineCode.VaccineCodes.Add(new SystemCode()
            {
                Code = "BCYSCT_AN032",
                CommonType = "AntiGenCode",
                Display = this.antigenName,
                System = "https://ehealthbc.ca/NamingSystem/ca-bc-panorama-immunization-antigen-code",
            });

            recommendationResponse.DateCriterions.Add(new DateCriterion()
            {
                DateCriterionCode = new DateCriterionCode()
                {
                    Text = "Forecast by Disease Eligible Date",
                },
                Value = this.diseaseEligibleDateString,
            });

            immzRecommendationResponse.Recommendations.Add(recommendationResponse);
            return immzRecommendationResponse;
        }
    }
}
