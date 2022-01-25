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
namespace HealthGateway.LaboratoryTests.Mock
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Moq;

    /// <summary>
    /// Class to mock IHttpContextAccessor.
    /// </summary>
    public class HttpContextAccessorMock : Mock<IHttpContextAccessor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextAccessorMock"/> class.
        /// </summary>
        /// <param name="token">token needed for authentication.</param>
        /// <param name="claimsPrincipal">exposes a collection of identities.</param>
        public HttpContextAccessorMock(string token, ClaimsPrincipal claimsPrincipal)
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", token },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);

            AuthenticateResult authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = token, },
            });

            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object!.HttpContext!, It.IsAny<string>()))
                .ReturnsAsync(authResult);
        }
    }
}
