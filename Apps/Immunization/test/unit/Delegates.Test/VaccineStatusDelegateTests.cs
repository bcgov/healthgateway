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
    using HealthGateway.Immunization.Constants;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
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
        private readonly IConfiguration configuration;
        private readonly string phn = "9735353315";
        private readonly DateTime dob = new DateTime(1990, 01, 05);
        private readonly string accessToken = "XXDDXX";

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
            VaccineStatusResult expectedVaccineStatus = new VaccineStatusResult()
            {
                FirstName = "Bob",
                StatusIndicator = "Exempt",
            };

            PHSAResult<VaccineStatusResult> phsaResponse = new PHSAResult<VaccineStatusResult>()
            {
                Result = expectedVaccineStatus,
            };

            string json = JsonSerializer.Serialize(phsaResponse, null);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
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
            var actualResult = await vaccineStatusDelegate.GetVaccineStatus(this.phn, this.dob, this.accessToken).ConfigureAwait(true);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetVaccineStatus - No Content.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusNoContent()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NoContent,
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                httpClientService,
                this.configuration,
                this.GetHttpContextAccessor().Object);
            var actualResult = await vaccineStatusDelegate.GetVaccineStatus(this.phn, this.dob, this.accessToken).ConfigureAwait(true);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Equal(VaccineState.NotFound.ToString(), actualResult.ResourcePayload!.Result!.StatusIndicator);
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

        private static ClaimsPrincipal GetClaimsPrincipal()
        {
            List<Claim> claims = new ()
            {
                new Claim(ClaimTypes.Name, "username"),
            };
            ClaimsIdentity identity = new (claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private Mock<IHttpContextAccessor> GetHttpContextAccessor()
        {
            ClaimsPrincipal claimsPrincipal = GetClaimsPrincipal();
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", this.accessToken);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            Mock<ConnectionInfo> connectionMock = new Mock<ConnectionInfo>();
            connectionMock.Setup(c => c.RemoteIpAddress).Returns(new IPAddress(1000));
            httpContextMock.Setup(s => s.Connection).Returns(connectionMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
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
