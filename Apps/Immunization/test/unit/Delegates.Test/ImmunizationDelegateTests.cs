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
namespace HealthGateway.Immunization.Test.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Common.Services;
    using HealthGateway.Immunization.Delegates;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// RestImmunizationDelegate's Unit Tests.
    /// </summary>
    public class ImmunizationDelegateTests
    {
        private readonly IConfiguration configuration;
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private readonly string token = "Fake Access Token";
        private readonly string userId = "1001";

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationDelegateTests"/> class.
        /// </summary>
        public ImmunizationDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        [Fact]
        public void GetImmunization()
        {
            ImmunizationViewResponse expectedViewResponse = new ImmunizationViewResponse()
            {
                Id = Guid.NewGuid(),
                SourceSystemId = "mockSourceSystemId",
                Name = "mockName",
                OccurrenceDateTime = DateTime.ParseExact("2020/09/10 17:16:10.809", "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            };

            PHSAResult<ImmunizationViewResponse> phsaResponse = new PHSAResult<ImmunizationViewResponse>()
            {
                Result = expectedViewResponse,
            };

            string json = JsonSerializer.Serialize(phsaResponse);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(
                loggerFactory.CreateLogger<RestImmunizationDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            var actualResult = immsDelegate.GetImmunizations(0).Result;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        [Fact]
        public void GetImmunizations()
        {
            ImmunizationViewResponse expectedViewResponse = new ImmunizationViewResponse()
            {
                Id = Guid.NewGuid(),
                SourceSystemId = "mockSourceSystemId",
                Name = "mockName",
                OccurrenceDateTime = DateTime.ParseExact("2020/09/10 17:16:10.809", "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            };

            PHSAResult<ImmunizationResponse> phsaResponse = new PHSAResult<ImmunizationResponse>()
            {
                Result = new ImmunizationResponse(
                    new List<ImmunizationViewResponse>() { expectedViewResponse },
                    new List<ImmunizationRecommendationResponse>()),
            };

            string json = JsonSerializer.Serialize(phsaResponse);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(
                loggerFactory.CreateLogger<RestImmunizationDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            var actualResult = immsDelegate.GetImmunizations(0).Result;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Collection(actualResult.ResourcePayload?.Result?.ImmunizationViews, item => Assert.Equal(expectedViewResponse.Id, item.Id));
        }

        /// <summary>
        /// GetImmunizations - Bad Data.
        /// </summary>
        [Fact]
        public void BadData()
        {
            string json = "{}";

            RequestResult<IEnumerable<ImmunizationResponse>> expectedResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                PageIndex = 0,
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = $"Error with JSON data",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                },
            };

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(
                loggerFactory.CreateLogger<RestImmunizationDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);

            var actualResult = immsDelegate.GetImmunizations(0).Result;

            Assert.True(expectedResult.IsDeepEqual(actualResult));
        }

        /// <summary>
        /// GetImmunizations - No Content.
        /// </summary>
        [Fact]
        public void NoContent()
        {
            int pageIndex = 0;
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(string.Empty),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(
                loggerFactory.CreateLogger<RestImmunizationDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            var actualResult = immsDelegate.GetImmunizations(pageIndex).Result;

            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Success && actualResult?.ResourcePayload?.Result == null);
            Assert.Null(actualResult?.ResourcePayload?.Result);
        }

        /// <summary>
        /// GetImmunizations - Forbidden.
        /// </summary>
        [Fact]
        public void Forbidden()
        {
            int pageIndex = 0;
            RequestResult<IEnumerable<ImmunizationResponse>> expectedResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {HttpStatusCode.Forbidden}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                },
                PageIndex = pageIndex,
            };

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(string.Empty),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(
                loggerFactory.CreateLogger<RestImmunizationDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            var actualResult = immsDelegate.GetImmunizations(pageIndex).Result;

            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// GetImmunizations - RequestTimeout.
        /// </summary>
        [Fact]
        public void RequestTimeout()
        {
            int pageIndex = 0;
            RequestResult<IEnumerable<ImmunizationResponse>> expectedResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = $"Unable to connect to Immunizations Endpoint, HTTP Error {HttpStatusCode.RequestTimeout}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                },
                PageIndex = pageIndex,
            };

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.RequestTimeout,
                Content = new StringContent(string.Empty),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(
                loggerFactory.CreateLogger<RestImmunizationDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            var actualResult = immsDelegate.GetImmunizations(pageIndex).Result;

            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// GetImmunizations - HttpRequestException.
        /// </summary>
        [Fact]
        public void Exception()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .Throws<HttpRequestException>()
               .Verifiable();

            int pageIndex = 0;
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.RequestTimeout,
                Content = new StringContent(string.Empty),
            };
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(
                loggerFactory.CreateLogger<RestImmunizationDelegate>(),
                mockHttpClientService.Object,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            var actualResult = immsDelegate.GetImmunizations(pageIndex).Result;

            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error);
            Assert.True(actualResult?.ResultError?.ErrorCode.EndsWith("-CE-PHSA", StringComparison.InvariantCulture));
        }

        private static IHttpClientService GetHttpClientService(HttpResponseMessage httpResponseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage)
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            return mockHttpClientService.Object;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }

        private Mock<IHttpContextAccessor> GetHttpContextAccessor()
        {
            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", this.token);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = this.token },
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            return httpContextAccessorMock;
        }

        private ClaimsPrincipal GetClaimsPrincipal()
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, this.userId),
                new Claim("hdid", this.hdid),
            };
            ClaimsIdentity identity = new(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }
    }
}
