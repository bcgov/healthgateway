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
namespace HealthGateway.Admin.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    /// <summary>
    /// Tests for the ConfigurationService class.
    /// </summary>
    public class ConfigurationServiceTests
    {
        /// <summary>
        /// Get configuration.
        /// </summary>
        [Fact]
        public void ShouldGetAllAsync()
        {
            // Arrange
            IConfigurationService service = GetConfigurationService();

            // Act
            ExternalConfiguration actual = service.GetConfiguration();

            // Assert
            Assert.NotNull(actual);
        }

        private static IConfigurationService GetConfigurationService()
        {
            return new ConfigurationService(GetIConfigurationRoot());
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "ClientIp", "127.0.0.0" },
                { "OpenIdConnect:Authority", "https://openid.com/auth/realms/hg" },
                { "OpenIdConnect:ClientId", "hg" },
                { "OpenIdConnect:Audience", "hg" },
                { "Features:Showcase", "true" },
                { "Features:UserInfo", "true" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }
    }
}
