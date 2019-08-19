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
    using Xunit;
    using Moq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    public class ConfigurationServiceTest
    {
        private IConfiguration config;
        private Mock<ILogger<ConfigurationService>> mockLog;
        private ConfigurationService service;

        public ConfigurationServiceTest()
        {
            this.config = new ConfigurationBuilder()
                .AddJsonFile("UnitTest.json").Build();
            // Mock dependency injection of controller
            this.mockLog = new Mock<ILogger<ConfigurationService>>();
            // Creates the controller passing mocked dependencies
            this.service = new ConfigurationService(mockLog.Object, config);
        }

        [Fact]
        public void TestGetConfig()
        {
            Models.ExternalConfiguration expectedResult = new Models.ExternalConfiguration()
            {
                OpenIdConnect = new Models.OpenIdConnectConfiguration()
                {
                    Authority = "Authority",
                    ClientId = "ClientId",
                    ResponseType = "ResponseType",
                    Scope = "Scope",
                    LogonCallbackURI = "LogonCallbackURI",
                    LogoutCallbackURI = "LogoutCallbackURI",
                },
                IdentityProviders = new Models.IdentityProviderConfiguration[]
                {
                    new Models.IdentityProviderConfiguration()
                    {
                        Id = "Id",
                        Name = "Name",
                        Icon = "Icon",
                        Hint = "Hint",
                        Disabled = true,
                    }
                },
                WebClient = new Models.WebClientConfiguration()
                {
                    LogLevel = "LogLevel",
                    Timeouts = new Models.TimeOutsConfiguration()
                    {
                        Idle = "Idle",
                        LogoutRedirect = "LogoutRedirect",
                    },
                    ExternalURLs = new Models.UriConfiguration[]
                   {
                       new Models.UriConfiguration()
                       {
                           Name = "Name",
                           URI = "URI",
                       }
                   }
                },
                ServiceEndpoints = new Models.UriConfiguration[]
                {
                    new Models.UriConfiguration()
                    {
                        Name = "Name",
                        URI = "URI",
                    }
                }
            };
            Models.ExternalConfiguration actualResult = service.GetConfiguration();
            Assert.True(expectedResult.IsDeepEqual(actualResult)); 
        }
    }
}
