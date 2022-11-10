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
    using System.Linq;
    using System.Net.Http;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// HttpClientService's Unit Tests.
    /// </summary>
    public class HttpClientServiceTests
    {
        /// <summary>
        /// CreateDefaultHttpClient - With Timeout.
        /// </summary>
        [Fact]
        public void ShouldGetHttpClientsWithTimeout()
        {
            int timeout = 23;
            Mock<IHttpClientFactory> mockHttpClientFactory = new();
            using HttpClient httpClient = new();
            mockHttpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>())).Returns(httpClient);
            Dictionary<string, string?> configDictionary = new()
            {
                { "HttpClient:Timeout", $"00:00:{timeout}" },
            };

            IConfiguration config = new ConfigurationBuilder()
                                        .AddInMemoryCollection(configDictionary.ToList<KeyValuePair<string, string?>>())
                                        .Build();
            HttpClientService service = new(mockHttpClientFactory.Object, config);

            using HttpClient client = service.CreateDefaultHttpClient();

            Assert.IsType<HttpClient>(client);
            Assert.Equal(client.Timeout.TotalSeconds, timeout);
        }
    }
}
