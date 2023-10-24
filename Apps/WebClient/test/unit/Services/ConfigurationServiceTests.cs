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
namespace HealthGateway.WebClientTests.Services
{
    using System;
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Server.Models;
    using HealthGateway.WebClient.Server.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// ConfigurationService's Unit Tests.
    /// </summary>
    public class ConfigurationServiceTests
    {
        private readonly ConfigurationService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationServiceTests"/> class.
        /// </summary>
        public ConfigurationServiceTests()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("Assets/appsettings.json")
                .Build();

            // Mock dependency injection of controller
            Mock<ILogger<ConfigurationService>> mockLog = new();

            // Creates the controller passing mocked dependencies
            this.service = new ConfigurationService(mockLog.Object, config);
        }

        /// <summary>
        /// GetConfiguration - Happy Path.
        /// </summary>
        [Fact]
        public void TestGetConfig()
        {
            ExternalConfiguration expectedResult = new()
            {
                OpenIdConnect = new OpenIdConnectConfiguration
                {
                    Authority = "Authority",
                    ClientId = "ClientId",
                    ResponseType = "ResponseType",
                    Scope = "Scope",
                    Callbacks = new Dictionary<string, Uri>
                    {
                        { "Logon", new Uri("https://localhost/logon") },
                        { "Logout", new Uri("https://localhost/logout") },
                    },
                },
                IdentityProviders = new[]
                {
                    new IdentityProviderConfiguration
                    {
                        Id = "Id",
                        Name = "Name",
                        Icon = "Icon",
                        Hint = "Hint",
                        Disabled = true,
                    },
                },
                WebClient = new WebClientConfiguration
                {
                    LogLevel = "LogLevel",
                    Timeouts = new TimeOutsConfiguration
                    {
                        Idle = 10000,
                        LogoutRedirect = "LogoutRedirect",
                    },
                    ExternalUrLs = new Dictionary<string, Uri>
                    {
                        {
                            "External", new Uri("https://localhost/external")
                        },
                    },
                    FeatureToggleFilePath = "Assets/featuretoggleconfig.json",
                    FeatureToggleConfiguration = new FeatureToggleConfiguration(
                        new HomepageSettings(true, true),
                        new WaitingQueueSettings(true),
                        new NotificationCentreSettings(true),
                        new TimelineSettings(true),
                        new DatasetSettings[]
                        {
                            new("bcCancerScreening", true),
                            new("clinicalDocument", true),
                            new("covid19TestResult", true),
                            new("diagnosticImaging", true),
                            new("healthVisit", true),
                            new("hospitalVisit", true),
                            new("immunization", true),
                            new("labResult", true),
                            new("medication", true),
                            new("note", true),
                            new("specialAuthorityRequest", true),
                        },
                        new Covid19Settings(true, new PublicCovid19Settings(true), new ProofOfVaccinationSettings(false)),
                        new DependentsSettings(true, true, new DatasetSettings[] { new("note", false) }),
                        new ServicesSettings(true, new ServiceSetting[] { new("organDonorRegistration", true), new("healthConnectRegistry", true) })),
                },
                ServiceEndpoints = new Dictionary<string, Uri>
                {
                    {
                        "Service", new Uri("https://localhost/service")
                    },
                },
            };
            ExternalConfiguration actualResult = this.service.GetConfiguration();

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GetMobileConfiguration - Happy Path.
        /// </summary>
        [Fact]
        public void TestGetMobileConfiguration()
        {
            MobileConfiguration expectedResult = new(
                true,
                new Uri("https://hg-prod.api.gov.bc.ca/"),
                new MobileAuthenticationSettings(
                    new Uri("https://loginproxy.gov.bc.ca/auth/realms/health-gateway-gold"),
                    "bcsc-mobile",
                    "hg-mobile",
                    "myhealthbc://*"),
                2)
            {
                Datasets = new[]
                {
                    "bcCancerScreening",
                    "clinicalDocument",
                    "covid19TestResult",
                    "diagnosticImaging",
                    "healthVisit",
                    "hospitalVisit",
                    "immunization",
                    "labResult",
                    "medication",
                    "note",
                    "specialAuthorityRequest",
                },
                DependentDatasets = new[]
                {
                    "bcCancerScreening",
                    "clinicalDocument",
                    "covid19TestResult",
                    "diagnosticImaging",
                    "healthVisit",
                    "hospitalVisit",
                    "immunization",
                    "labResult",
                    "medication",
                    "specialAuthorityRequest",
                },
                Services = new[]
                {
                    "organDonorRegistration",
                    "healthConnectRegistry",
                },
            };

            MobileConfiguration actualResult = this.service.GetMobileConfiguration();

            expectedResult.ShouldDeepEqual(actualResult);
        }
    }
}
