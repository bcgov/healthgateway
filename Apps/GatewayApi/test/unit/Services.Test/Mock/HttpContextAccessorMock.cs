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
namespace HealthGateway.GatewayApiTests.Services.Test.Mock
{
    using Microsoft.AspNetCore.Http;
    using Moq;

    /// <summary>
    /// HttpContextAccessorMock.
    /// </summary>
    public class HttpContextAccessorMock : Mock<IHttpContextAccessor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextAccessorMock"/> class.
        /// </summary>
        /// <param name="headerDictionary">header dictionary.</param>
        public HttpContextAccessorMock(IHeaderDictionary headerDictionary)
        {
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            this.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
        }
    }
}
