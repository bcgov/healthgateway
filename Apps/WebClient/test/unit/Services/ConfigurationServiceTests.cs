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
    using System.Threading.Tasks;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestGetConfig()
        {
            ExternalConfiguration expectedResult = GenerateExternalConfiguration();
            ExternalConfiguration actualResult = await this.service.GetConfigurationAsync();

            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetMobileConfiguration - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestGetMobileConfiguration()
        {
            MobileConfiguration expectedResult = new(
                true,
                new Uri("https://hg-prod.api.gov.bc.ca/"),
                new MobileAuthenticationSettings(
                    new Uri("https://loginproxy.gov.bc.ca/auth/realms/health-gateway-gold"),
                    "bcsc-mobile",
                    "hg-mobile",
                    "hg-mobile-android",
                    "hg-mobile-ios",
                    "myhealthbc://*"),
                2)
            {
                Datasets =
                [
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
                ],
                DependentDatasets =
                [
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
                ],
                Services =
                [
                    "organDonorRegistration",
                    "healthConnectRegistry",
                ],
            };

            MobileConfiguration actualResult = await this.service.GetMobileConfigurationAsync();

            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// GetConfigurationAsync - Handles exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetConfigurationAsyncHandlesException()
        {
            // Arrange
            const string invalidPath = "invalid path";

            ExternalConfiguration expected = GenerateExternalConfiguration(
                invalidPath,
                false);

            IConfiguration originalConfiguration = new ConfigurationBuilder()
                .AddJsonFile("Assets/appsettings.json")
                .Build();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddConfiguration(originalConfiguration)
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        { "WebClient:FeatureToggleFilePath", invalidPath },
                    })
                .Build();

            ConfigurationService configurationService = new(
                new Mock<ILogger<ConfigurationService>>().Object,
                configuration);

            // Act
            ExternalConfiguration actual = await configurationService.GetConfigurationAsync();

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static ExternalConfiguration GenerateExternalConfiguration(
            string featureToggleConfigurationPath = "Assets/featuretoggleconfig.json",
            bool showFeatureToggleConfiguration = true)
        {
            return new()
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
                IdentityProviders =
                [
                    new IdentityProviderConfiguration
                    {
                        Id = "Id",
                        Name = "Name",
                        Icon = "Icon",
                        Hint = "Hint",
                        Disabled = true,
                    },
                ],
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
                    FeatureToggleFilePath = featureToggleConfigurationPath,
                    FeatureToggleConfiguration = showFeatureToggleConfiguration
                        ? new FeatureToggleConfiguration(
                            new HomepageSettings(true, true, true, new OtherRecordSourcesSettings(true, [new("accessMyHealth", false)])),
                            new NotificationCentreSettings(true),
                            new TimelineSettings(true),
                            GetFeatureDatasetSettings(),
                            new Covid19Settings(true, new PublicCovid19Settings(true, true), new ProofOfVaccinationSettings(false)),
                            new DependentsSettings(true, true, [new("note", false)]),
                            new ServicesSettings(true, [new("organDonorRegistration", true), new("healthConnectRegistry", true)]))
                        : null,
                },
                ServiceEndpoints = new Dictionary<string, Uri>
                {
                    {
                        "Service", new Uri("https://localhost/service")
                    },
                },
            };
        }

        private static DatasetSettings[] GetFeatureDatasetSettings()
        {
            return
            [
                new("bcCancerScreening", true), new("clinicalDocument", true), new("covid19TestResult", true), new("diagnosticImaging", true), new("healthVisit", true),
                new("hospitalVisit", true), new("immunization", true), new("labResult", true), new("medication", true), new("note", true), new("specialAuthorityRequest", true),
            ];
        }
    }
}
