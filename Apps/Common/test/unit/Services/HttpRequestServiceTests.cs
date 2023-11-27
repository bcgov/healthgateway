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
namespace HealthGateway.CommonTests.Services
{
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// HttpRequestService's Unit Tests.
    /// </summary>
    public class HttpRequestServiceTests
    {
        private const string Host = "http://localhost:5002";
        private static readonly IHeaderDictionary HeaderDictionary = new HeaderDictionary
        {
            { "referer", Host },
        };

        /// <summary>
        /// Get Referer Host.
        /// </summary>
        [Fact]
        public void ShouldGetRefererHost()
        {
            // Setup
            string expected = Host;
            HttpRequestService httpRequestService = GetHttpHttpRequestService();

            // Act
            string actual = httpRequestService.GetRefererHost();

            // Verify
            Assert.Equal(expected, actual);
        }

        private static HttpRequestService GetHttpHttpRequestService()
        {
            Mock<HttpRequest> httpRequest = new();
            httpRequest.Setup(s => s.Headers).Returns(HeaderDictionary);

            Mock<HttpContext> httpContext = new();
            httpContext.Setup(s => s.Request).Returns(httpRequest.Object);

            Mock<IHttpContextAccessor> httpContextAccessor = new();
            httpContextAccessor.Setup(s => s.HttpContext).Returns(httpContext.Object);

            return new HttpRequestService(
                new Mock<ILogger<HttpRequestService>>().Object,
                httpContextAccessor.Object);
        }
    }
}
