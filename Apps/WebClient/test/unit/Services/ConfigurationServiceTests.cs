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
    using System.Globalization;
    using DeepEqual.Syntax;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
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
        private static readonly string TourChangeDateTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        private readonly IList<ApplicationSetting> tourSettings = new List<ApplicationSetting>
        {
            new()
            {
                Key = TourSettingsMapper.LatestChangeDateTime,
                Value = TourChangeDateTime,
            },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationServiceTests"/> class.
        /// </summary>
        public ConfigurationServiceTests()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("UnitTest.json")
                .Build();

            // Mock dependency injection of controller
            Mock<ILogger<ConfigurationService>> mockLog = new();
            Mock<ICacheProvider> mockCacheProvider = new();
            Mock<IApplicationSettingsDelegate> mockAppSettingsDelegate = new();
            mockAppSettingsDelegate.Setup(del => del.GetApplicationSettings(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(this.tourSettings);

            // Creates the controller passing mocked dependencies
            this.service = new ConfigurationService(mockLog.Object, config, mockAppSettingsDelegate.Object, mockCacheProvider.Object);
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
                    TourConfiguration = new TourConfiguration
                    {
                        LatestChangeDateTime = DateTime.Parse(TourChangeDateTime, CultureInfo.InvariantCulture),
                    },
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
    }
}
