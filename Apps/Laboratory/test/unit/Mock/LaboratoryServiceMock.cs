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
namespace HealthGateway.LaboratoryTests.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// Class to mock ILaboratoryService.
    /// </summary>
    public class LaboratoryServiceMock : Mock<ILaboratoryService>
    {
        private readonly LaboratoryService laboratoryService;
        private readonly IConfiguration configuration = GetIConfigurationRoot();

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        public LaboratoryServiceMock()
        {
            this.laboratoryService = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new Mock<ILaboratoryDelegateFactory>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                GetMemoryCache(),
                new Mock<IHttpContextAccessor>().Object);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">return object of the delegate service.</param>
        /// <param name="token">token needed for authentication.</param>
        /// <param name="mockHttpContextAccessor">IHttpContextAccessor Mock.</param>
        /// <param name="claimsPrincipal">exposes a collection of identities.</param>
        public LaboratoryServiceMock(RequestResult<IEnumerable<PhsaCovid19Order>> delegateResult, Mock<IHttpContextAccessor> mockHttpContextAccessor, string token, ClaimsPrincipal claimsPrincipal)
        {
            SetupAuthenticationMock(mockHttpContextAccessor, token, claimsPrincipal);

            this.laboratoryService = new LaboratoryService(
                 this.configuration,
                 new Mock<ILogger<LaboratoryService>>().Object,
                 new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult)).Object,
                 null!,
                 null!,
                 mockHttpContextAccessor.Object);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">return object of the delegate service.</param>
        /// <param name="token">token needed for authentication.</param>
        /// <param name="mockHttpContextAccessor">IHttpContextAccessor Mock.</param>
        /// <param name="claimsPrincipal">exposes a collection of identities.</param>
        public LaboratoryServiceMock(RequestResult<LaboratoryReport> delegateResult, Mock<IHttpContextAccessor> mockHttpContextAccessor, string token, ClaimsPrincipal claimsPrincipal)
        {
            SetupAuthenticationMock(mockHttpContextAccessor, token, claimsPrincipal);

            this.laboratoryService = new LaboratoryService(
                 this.configuration,
                 new Mock<ILogger<LaboratoryService>>().Object,
                 new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult)).Object,
                 null!,
                 null!,
                 mockHttpContextAccessor.Object);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">list of COVID-19 Test Results.</param>
        /// <param name="mockHttpContextAccessor">IHttpContextAccessor Mock.</param>
        /// <param name="token">token needed for authentication.</param>
        /// <param name="claimsPrincipal">exposes a collection of identities.</param>
        public LaboratoryServiceMock(RequestResult<PHSAResult<IEnumerable<CovidTestResult>>> delegateResult, Mock<IHttpContextAccessor> mockHttpContextAccessor, string token, ClaimsPrincipal claimsPrincipal)
        {
            SetupAuthenticationMock(mockHttpContextAccessor, token, claimsPrincipal);

            JWTModel jwtModel = new()
            {
                AccessToken = token,
            };

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), false)).Returns(jwtModel);

            this.laboratoryService = new LaboratoryService(
                 this.configuration,
                 new Mock<ILogger<LaboratoryService>>().Object,
                 new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult, token)).Object,
                 mockAuthDelegate.Object,
                 GetMemoryCache(),
                 mockHttpContextAccessor.Object);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryServiceMock"/> class.
        /// </summary>
        /// <param name="delegateResult">return object of the delegate service.</param>
        /// <param name="token">token needed for authentication.</param>
        /// <param name="mockHttpContextAccessor">IHttpContextAccessor Mock.</param>
        /// <param name="claimsPrincipal">exposes a collection of identities.</param>
        public LaboratoryServiceMock(RequestResult<PhsaLaboratorySummary> delegateResult, Mock<IHttpContextAccessor> mockHttpContextAccessor, string token, ClaimsPrincipal claimsPrincipal)
        {
            SetupAuthenticationMock(mockHttpContextAccessor, token, claimsPrincipal);

            this.laboratoryService = new LaboratoryService(
                this.configuration,
                new Mock<ILogger<LaboratoryService>>().Object,
                new LaboratoryDelegateFactoryMock(new LaboratoryDelegateMock(delegateResult)).Object,
                null!,
                null!,
                mockHttpContextAccessor.Object);
        }

        /// <summary>
        /// Get the Laboratory Service Mock Instance.
        /// </summary>
        /// <returns>LaboratoryService.</returns>
        public LaboratoryService LaboratoryServiceMockInstance()
        {
            return this.laboratoryService;
        }

        private static IMemoryCache? GetMemoryCache()
        {
            ServiceCollection services = new();
            services.AddMemoryCache();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetService<IMemoryCache>();
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string>? myConfiguration = new()
            {
                { "Laboratory:BackOffMilliseconds", "0" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static void SetupAuthenticationMock(Mock<IHttpContextAccessor> mockHttpContextAccessor, string token, ClaimsPrincipal claimsPrincipal)
        {
            Mock<IAuthenticationService> authenticationMock = new();
            mockHttpContextAccessor
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            AuthenticateResult authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = token, },
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(mockHttpContextAccessor.Object!.HttpContext!, It.IsAny<string>()))
                .ReturnsAsync(authResult);
        }
    }
}
