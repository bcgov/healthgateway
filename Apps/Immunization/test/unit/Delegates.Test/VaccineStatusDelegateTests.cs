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
namespace HealthGateway.ImmunizationTests.Delegates.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
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
    /// VaccineStatusDelegate's Unit Tests.
    /// </summary>
    public class VaccineStatusDelegateTests
    {
        private readonly string accessToken = "XXDDXX";
        private readonly IConfiguration configuration;
        private readonly DateTime dob = new(1990, 01, 05);
        private readonly string hdId = "43465786";
        private readonly string phn = "9735353315";

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusDelegateTests"/> class.
        /// </summary>
        public VaccineStatusDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetVaccineStatus - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatus()
        {
            VaccineStatusResult expectedVaccineStatus = new()
            {
                FirstName = "Bob",
                StatusIndicator = "Exempt",
            };

            PhsaResult<VaccineStatusResult> phsaResponse = new()
            {
                Result = expectedVaccineStatus,
            };

            string json = JsonSerializer.Serialize(phsaResponse);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = this.phn,
                DateOfBirth = this.dob,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult = await vaccineStatusDelegate.GetVaccineStatus(query, this.accessToken, true).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetVaccineStatus - not public end point.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusNotPublicEndPoint()
        {
            VaccineStatusResult expectedVaccineStatus = new()
            {
                FirstName = "Bob",
                StatusIndicator = "Exempt",
            };

            PhsaResult<VaccineStatusResult> phsaResponse = new()
            {
                Result = expectedVaccineStatus,
            };

            string json = JsonSerializer.Serialize(phsaResponse);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = this.phn,
                DateOfBirth = this.dob,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult = await vaccineStatusDelegate.GetVaccineStatus(query, this.accessToken, false).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetVaccineStatus - not public end point with HdId.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusNotPublicEndPointWithHdId()
        {
            VaccineStatusResult expectedVaccineStatus = new()
            {
                FirstName = "Bob",
                StatusIndicator = "Exempt",
            };

            PhsaResult<VaccineStatusResult> phsaResponse = new()
            {
                Result = expectedVaccineStatus,
            };

            string json = JsonSerializer.Serialize(phsaResponse);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = this.phn,
                DateOfBirth = this.dob,
                HdId = this.hdId,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult = await vaccineStatusDelegate.GetVaccineStatus(query, this.accessToken, false).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetVaccineStatus - Not Found.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusNoContent()
        {
            VaccineStatusResult expectedVaccineStatus = new()
            {
                StatusIndicator = "NotFound",
            };

            PhsaResult<VaccineStatusResult> phsaResponse = new()
            {
                Result = expectedVaccineStatus,
            };

            string json = JsonSerializer.Serialize(phsaResponse);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = this.phn,
                DateOfBirth = this.dob,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult = await vaccineStatusDelegate.GetVaccineStatus(query, this.accessToken, true).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(VaccineState.NotFound.ToString(), actualResult.ResourcePayload!.Result!.StatusIndicator);
        }

        /// <summary>
        /// GetVaccineStatus - Http exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusCatchException403()
        {
            RequestResult<PhsaResult<VaccineStatusResult>> expected = new()
            {
                ResultError = new RequestResultError
                {
                    ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {HttpStatusCode.Forbidden}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                },
            };

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.Forbidden,
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = this.phn,
                DateOfBirth = this.dob,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult = await vaccineStatusDelegate.GetVaccineStatus(query, this.accessToken, true).ConfigureAwait(true);

            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(expected.ResultError.ErrorCode, actualResult.ResultError?.ErrorCode);
            Assert.Equal(expected.ResultError.ResultMessage, actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetVaccineStatus - default Http exception handling.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusCatchExceptionOther()
        {
            RequestResult<PhsaResult<VaccineStatusResult>> expected = new()
            {
                ResultError = new RequestResultError
                {
                    ResultMessage = $"Unable to connect to Immunizations/VaccineStatus Endpoint, HTTP Error {HttpStatusCode.BadRequest}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                },
            };

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.BadRequest,
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = this.phn,
                DateOfBirth = this.dob,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult = await vaccineStatusDelegate.GetVaccineStatus(query, this.accessToken, true).ConfigureAwait(true);

            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(expected.ResultError.ErrorCode, actualResult.ResultError?.ErrorCode);
            Assert.Equal(expected.ResultError.ResultMessage, actualResult.ResultError?.ResultMessage);
        }

        private static IHttpClientService GetHttpClientService(HttpResponseMessage httpResponseMessage)
        {
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage)
                .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            return mockHttpClientService.Object;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "PHSA:BaseUrl", "https://some-test-url/" },
            };
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static ClaimsPrincipal GetClaimsPrincipal()
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, "username"),
            };
            ClaimsIdentity identity = new(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private Mock<IHttpContextAccessor> GetHttpContextAccessor()
        {
            ClaimsPrincipal claimsPrincipal = GetClaimsPrincipal();
            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", this.accessToken },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            Mock<ConnectionInfo> connectionMock = new();
            connectionMock.Setup(c => c.RemoteIpAddress).Returns(new IPAddress(1000));
            httpContextMock.Setup(s => s.Connection).Returns(connectionMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            AuthenticateResult authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(
                new[]
                {
                    new AuthenticationToken { Name = "access_token", Value = this.accessToken },
                });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            return httpContextAccessorMock;
        }
    }
}
