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
namespace HealthGateway.CommonTests.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.Extensions.Configuration;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class HttpClientService_Test
    {
        [Fact]
        public void ShouldGetHttpClientswithTimeout()
        {
            int timeout = 23;
            Mock<IHttpClientFactory> mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
            Dictionary<string, string> configDictionary = new Dictionary<string, string>
            {
                {"HttpClient:Timeout", $"00:00:{timeout}"},
            };

            IConfiguration config = new ConfigurationBuilder()
                                        .AddInMemoryCollection(configDictionary)
                                        .Build();
            HttpClientService service = new HttpClientService(mockHttpClientFactory.Object, config);
            using HttpClient client = service.CreateDefaultHttpClient();

            Assert.True(client is HttpClient && client.Timeout.TotalSeconds == timeout);

            using HttpClient untrustedClient = service.CreateUntrustedHttpClient();
            Assert.True(untrustedClient is HttpClient && client.Timeout.TotalSeconds == timeout);
        }
    }
}
