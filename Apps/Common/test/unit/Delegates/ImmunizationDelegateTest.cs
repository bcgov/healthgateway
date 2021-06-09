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
namespace HealthGateway.CommonTests.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// ImmunizationDelegate's Unit Tests.
    /// </summary>
    public class ImmunizationDelegateTest
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationDelegateTest"/> class.
        /// </summary>
        public ImmunizationDelegateTest()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetImmunization - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateGetImmunization200()
        {
            string token = "some-bearer-token";
            string userId = "UserId123";
            string hdid = "TheTestHdid";
            string immunizationId = "fbdec97f-a603-4305-59ed-08d906b6603e";

            var expectedRequestResult = new RequestResult<ImmunizationEvent>()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                ResourcePayload = new ImmunizationEvent()
                {
                    Id = "fbdec97f-a603-4305-59ed-08d906b6603e",
                    DateOfImmunization = DateTime.Now,
                    Status = "Completed",
                    ProviderOrClinic = "North Island Hospital, Campbell River \u0026 District",
                    TargetedDisease = "COVID19",
                    Immunization = new ImmunizationDefinition()
                    {
                        Name = "COVID - 19 mRNA",
                        ImmunizationAgents = new List<ImmunizationAgent>()
                        {
                            new ImmunizationAgent()
                            {
                                Code = "1119349007",
                                Name = "COVID - 19 mRNA",
                                LotNumber = "EL0203",
                                ProductName = "Pfizer mRNA BNT162b2",
                            },
                        },
                    },
                },
            };

            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedRequestResult)),
            };

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IImmunizationDelegate immunizationDelegate = new RestImmunizationDelegate(
                loggerFactory.CreateLogger<RestImmunizationDelegate>(),
                GetHttpClientServiceMock(httpResponseMessage).Object,
                this.configuration,
                CreateValidHttpContext(token, userId, hdid).Object);

            RequestResult<ImmunizationEvent> actualResult = Task.Run(async () => await immunizationDelegate.GetImmunization(hdid, immunizationId).ConfigureAwait(true)).Result;
            Assert.Equal(expectedRequestResult.ResourcePayload.Id, actualResult.ResourcePayload?.Id);
            Assert.Equal(expectedRequestResult.ResourcePayload.Immunization.Name, actualResult.ResourcePayload?.Immunization.Name);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "ServiceEndpoints:Immunization", "https://some-test-url/Immunization" },
            };

            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static Mock<IHttpClientService> GetHttpClientServiceMock(HttpResponseMessage httpResponseMessage)
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

            return mockHttpClientService;
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
    }
}
