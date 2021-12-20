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
namespace HealthGateway.Immunization.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Parser;
    using HealthGateway.Immunization.Services;
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

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetImmunizations()
        {
            var mockDelegate = new Mock<Immunization.Delegates.IImmunizationDelegate>();
            RequestResult<PHSAResult<ImmunizationResponse>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PHSAResult<ImmunizationResponse>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, },
                    Result = new ImmunizationResponse(
                        new List<ImmunizationViewResponse>()
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
                    LoadStateModel.FromPHSAModel(delegateResult.ResourcePayload.LoadState),
                    EventParser.FromPHSAModelList(delegateResult.ResourcePayload.Result.ImmunizationViews),
                    new List<ImmunizationRecommendation>()),
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<int>())).ReturnsAsync(delegateResult);
            IImmunizationService service = new ImmunizationService(mockDelegate.Object);

            var actualResult = service.GetImmunizations(0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
        }

        /// <summary>
        /// GetImmunization - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetImmunization()
        {
            var mockDelegate = new Mock<Immunization.Delegates.IImmunizationDelegate>();
            RequestResult<PHSAResult<ImmunizationViewResponse>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PHSAResult<ImmunizationViewResponse>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, },
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
                ResourcePayload = EventParser.FromPHSAModel(delegateResult.ResourcePayload.Result),
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunization(It.IsAny<string>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(mockDelegate.Object);
            var actualResult = service.GetImmunization("immz_id");
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
        }

        /// <summary>
        /// GetImmunizations - Happy Path (With Recommendations).
        /// </summary>
        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public void ShouldGetRecomendation()
        {
            var immzRecommendationResponse = this.GetImmzRecommendationResponse();
            var mockDelegate = new Mock<Immunization.Delegates.IImmunizationDelegate>();
            RequestResult<PHSAResult<ImmunizationResponse>> delegateResult = GetPHSAResult(immzRecommendationResponse);
            RequestResult<ImmunizationResult> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new ImmunizationResult(
                    LoadStateModel.FromPHSAModel(delegateResult?.ResourcePayload?.LoadState),
                    new List<ImmunizationEvent>(),
                    ImmunizationRecommendation.FromPHSAModelList(delegateResult?.ResourcePayload?.Result?.Recommendations)),
                PageIndex = delegateResult.PageIndex,
                PageSize = delegateResult.PageSize,
                TotalResultCount = delegateResult.TotalResultCount,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(mockDelegate.Object);

            var actualResult = service.GetImmunizations(0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
            Assert.Equal(1, expectedResult.ResourcePayload.Recommendations.Count);
            var recomendationResult = expectedResult.ResourcePayload.Recommendations[0];
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
            Assert.Equal(DateTime.Parse(this.diseaseEligibleDateString, CultureInfo.CurrentCulture), recomendationResult?.DisseaseEligibleDate);
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
            var mockDelegate = new Mock<Immunization.Delegates.IImmunizationDelegate>();
            RequestResult<PHSAResult<ImmunizationResponse>> delegateResult = new()
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

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(mockDelegate.Object);

            var actualResult = service.GetImmunizations(0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
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

            RequestResult<PHSAResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, },
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
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Success);
            Assert.True(actualResult.ResourcePayload != null && actualResult.ResourcePayload.Document.Data == assetResult.ResourcePayload.Data);
        }

        /// <summary>
        /// A refresh in progress is occurring while we're getting the Vaccine Status.
        /// </summary>
        [Fact]
        public void GetVaccineRecordRIP()
        {
            RequestResult<PHSAResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = true, },
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
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.ActionRequired && actualResult.ResultError != null);
        }

        /// <summary>
        /// We get an error while pulling the Vaccine Status.
        /// </summary>
        [Fact]
        public void GetVaccineRecordPHSAError()
        {
            RequestResult<PHSAResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Error,
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
            Assert.True(vaccineStatusResult.IsDeepEqual(actualResult));
        }

        /// <summary>
        /// The Vaccine status is in a state where we will not generate a proof.
        /// </summary>
        [Fact]
        public void GetVaccineRecordInvalidState()
        {
            RequestResult<PHSAResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, },
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
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error);
        }

        /// <summary>
        /// An unexpected state was returned for the vaccine status.
        /// </summary>
        [Fact]
        public void GetVaccineRecordUnknownState()
        {
            RequestResult<PHSAResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, },
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
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error);
        }

        /// <summary>
        /// The request to the Vaccine Proof Service errored out.
        /// </summary>
        [Fact]
        public void GetVaccineRecordServiceProofFailed()
        {
            RequestResult<PHSAResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, },
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

            RequestResult<PHSAResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, },
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

        private static RequestResult<PHSAResult<ImmunizationResponse>> GetPHSAResult(ImmunizationRecommendationResponse immzRecommendationResponse)
        {
            return new RequestResult<PHSAResult<ImmunizationResponse>>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PHSAResult<ImmunizationResponse>()
                {
                    LoadState = new() { RefreshInProgress = false, },
                    Result = new(
                        new List<ImmunizationViewResponse>(),
                        new List<ImmunizationRecommendationResponse>()
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

            List<Claim> claims = new List<Claim>()
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
