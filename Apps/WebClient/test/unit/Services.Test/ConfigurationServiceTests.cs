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
namespace HealthGateway.WebClient.Test.Services
{
    using System;
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// ConfigurationService's Unit Tests.
    /// </summary>
    public class ConfigurationServiceTests
    {
        private readonly IConfiguration config;
        private readonly Mock<ILogger<ConfigurationService>> mockLog;
        private readonly ConfigurationService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationServiceTests"/> class.
        /// </summary>
        public ConfigurationServiceTests()
        {
            this.config = new ConfigurationBuilder()
                .AddJsonFile("UnitTest.json").Build();

            // Mock dependency injection of controller
            this.mockLog = new Mock<ILogger<ConfigurationService>>();

            // Creates the controller passing mocked dependencies
            this.service = new ConfigurationService(this.mockLog.Object, this.config);
        }

        /// <summary>
        /// GetConfiguration - Happy Path.
        /// </summary>
        [Fact]
        public void TestGetConfig()
        {
            ExternalConfiguration expectedResult = new()
            {
                OpenIdConnect = new OpenIdConnectConfiguration()
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
                IdentityProviders = new IdentityProviderConfiguration[]
                {
                    new IdentityProviderConfiguration()
                    {
                        Id = "Id",
                        Name = "Name",
                        Icon = "Icon",
                        Hint = "Hint",
                        Disabled = true,
                    },
                },
                WebClient = new WebClientConfiguration()
                {
                    LogLevel = "LogLevel",
                    Timeouts = new TimeOutsConfiguration()
                    {
                        Idle = 10000,
                        LogoutRedirect = "LogoutRedirect",
                    },
                    ExternalURLs = new Dictionary<string, Uri>
                    {
                       {
                           "External", new Uri("https://localhost/external")
                       },
                    },
                },
                ServiceEndpoints = new Dictionary<string, Uri>()
                {
                    {
                        "Service", new Uri("https://localhost/service")
                    },
                },
            };
            ExternalConfiguration actualResult = this.service.GetConfiguration();
            Assert.True(expectedResult.IsDeepEqual(actualResult));
        }
    }
}
