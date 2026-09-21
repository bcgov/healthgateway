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
namespace HealthGateway.ImmunizationTests.Controllers.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Handlers;
    using HealthGateway.Common.AspNetConfiguration.Modules;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Immunization.Controllers;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Xunit;

    /// <summary>
    /// ImmunizationController's Unit Tests.
    /// </summary>
    public class ImmunizationControllerTests
    {
        private const string Hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetImmunizations()
        {
            ImmunizationEvent event1 =
                new()
                {
                    DateOfImmunization = DateTime.Today,
                    ProviderOrClinic = "Mocked Clinic",
                    Immunization = new ImmunizationDefinition
                    {
                        Name = "Mocked Name",
                        ImmunizationAgents =
                        [
                            new()
                            {
                                Name = "mocked agent",
                                Code = "mocked code",
                                LotNumber = "mocked lot number",
                                ProductName = "mocked product",
                            },
                        ],
                    },
                };

            // Blank agent
            ImmunizationEvent event2 = new()
            {
                DateOfImmunization = DateTime.Today,
                Immunization = new ImmunizationDefinition
                {
                    Name = "Mocked Name",
                    ImmunizationAgents = [],
                },
            };

            RequestResult<ImmunizationResult> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 2,
                ResourcePayload = new()
                {
                    LoadState = new LoadStateModel { RefreshInProgress = false },
                    Immunizations = [event1, event2],
                },
            };

            Mock<IImmunizationService> svcMock = new();
            svcMock.Setup(s => s.GetImmunizationsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedRequestResult);

            ImmunizationController controller = new(new Mock<ILogger<ImmunizationController>>().Object, svcMock.Object);

            // Act
            RequestResult<ImmunizationResult> actual = await controller.GetImmunizations(Hdid, CancellationToken.None);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            int count = actual.ResourcePayload?.Immunizations.Count() ?? 0;
            Assert.Equal(2, count);
        }

        [Theory]
        [InlineData(typeof(ImmunizationController), 11, true)]
        [InlineData(typeof(ImmunizationController), 13, false)]
        [InlineData(typeof(ImmunizationControllerV2), 11, true)]
        [InlineData(typeof(ImmunizationControllerV2), 13, false)]
        public async Task ShouldEnforceDependentAgeUsingApplicationSettings(Type controllerType, int age, bool expectedAccess)
        {
            // Embed the actual app settings, with no local overrides or test-supplied age limit.
            await using Stream settings = typeof(ImmunizationControllerTests).Assembly
                .GetManifestResourceStream("Immunization.AppSettings.json")!;
            IConfiguration configuration = new ConfigurationBuilder().AddJsonStream(settings).Build();
            Assert.Equal(12, configuration.GetValue<int?>("Authorization:MaxDependentAge"));

            const string guardianHdid = "guardian";
            const string dependentHdid = "dependent";
            ClaimsPrincipal guardian = new(new ClaimsIdentity([new Claim(GatewayClaims.Hdid, guardianHdid)], "Test"));
            DefaultHttpContext httpContext = new()
            {
                User = guardian,
                Request =
                {
                    QueryString = new QueryString($"?hdid={dependentHdid}"),
                },
            };

            Mock<IResourceDelegateDelegate> delegation = new();
            delegation.Setup(d => d.ExistsAsync(dependentHdid, guardianHdid, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            Mock<IPatientService> patient = new();
            patient.Setup(p => p.GetPatientAsync(dependentHdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RequestResult<PatientModel>(new PatientModel { Birthdate = DateTime.Today.AddYears(-age) }, ResultType.Success));

            ServiceCollection services = new();
            services.AddLogging();
            services.AddSingleton(configuration);
            services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = httpContext });
            services.AddSingleton(delegation.Object);
            services.AddSingleton(patient.Object);
            Auth.ConfigureAuthorizationServices(services, NullLogger.Instance, configuration);
            services.AddScoped<IAuthorizationHandler, UserDelegatedAccessHandler>();
            await using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();

            // Resolve the real policy declared by each version's action instead of duplicating its requirements.
            MethodInfo action = controllerType.GetMethod(nameof(ImmunizationController.GetImmunizations))!;
            AuthorizeAttribute attribute = Assert.Single(action.GetCustomAttributes<AuthorizeAttribute>());
            Assert.False(string.IsNullOrEmpty(attribute.Policy));
            IAuthorizationService authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
            AuthorizationResult result = await authorization.AuthorizeAsync(guardian, httpContext, attribute.Policy!);

            Assert.Equal(expectedAccess, result.Succeeded);
            patient.Verify(p => p.GetPatientAsync(dependentHdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
