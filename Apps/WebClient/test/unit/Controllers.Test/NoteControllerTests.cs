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
namespace HealthGateway.WebClient.Test.Controllers
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Controllers;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// NoteController's Unit Tests.
    /// </summary>
    public class NoteControllerTests
    {
        private const string Hdid = "mockedHdId";
        private const string Token = "Access Token Mock";
        private const string UserId = "User ID Mock";

        /// <summary>
        /// CreateNote - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldCreateNote()
        {
            RequestResult<UserNote> expectedResult = new RequestResult<UserNote>()
            {
                ResourcePayload = new UserNote()
                {
                    HdId = Hdid,
                    CreatedBy = Hdid,
                    UpdatedBy = Hdid,
                },
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(Token, UserId, Hdid);
            Mock<INoteService> noteServiceMock = new Mock<INoteService>();
            noteServiceMock.Setup(s => s.CreateNote(It.IsAny<UserNote>())).Returns(expectedResult);

            NoteController controller = new NoteController(noteServiceMock.Object, httpContextAccessorMock.Object);
            var actualResult = controller.CreateNote(Hdid, expectedResult.ResourcePayload);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// UpdateNote - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldUpdateNote()
        {
            RequestResult<UserNote> expectedResult = new RequestResult<UserNote>()
            {
                ResourcePayload = new UserNote()
                {
                    HdId = Hdid,
                    UpdatedBy = Hdid,
                },
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(Token, UserId, Hdid);
            Mock<INoteService> noteServiceMock = new Mock<INoteService>();
            noteServiceMock.Setup(s => s.UpdateNote(It.IsAny<UserNote>())).Returns(expectedResult);

            NoteController controller = new NoteController(noteServiceMock.Object, httpContextAccessorMock.Object);
            var actualResult = controller.UpdateNote(Hdid, expectedResult.ResourcePayload);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// DeleteNote - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldDeleteNote()
        {
            RequestResult<UserNote> expectedResult = new RequestResult<UserNote>()
            {
                ResourcePayload = new UserNote()
                {
                    HdId = Hdid,
                    UpdatedBy = Hdid,
                },
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(Token, UserId, Hdid);
            Mock<INoteService> noteServiceMock = new Mock<INoteService>();
            noteServiceMock.Setup(s => s.DeleteNote(It.IsAny<UserNote>())).Returns(expectedResult);

            NoteController controller = new NoteController(noteServiceMock.Object, httpContextAccessorMock.Object);
            var actualResult = controller.DeleteNote(Hdid, expectedResult.ResourcePayload);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// GetAll Notes - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldGetAll()
        {
            List<UserNote> mockedNotes = new List<UserNote>();
            for (int i = 0; i < 10; i++)
            {
                mockedNotes.Add(new UserNote()
                {
                    Text = "note " + i,
                    HdId = Hdid,
                });
            }

            RequestResult<IEnumerable<UserNote>> expectedResult = new RequestResult<IEnumerable<UserNote>>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = mockedNotes,
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(Token, UserId, Hdid);
            Mock<INoteService> noteServiceMock = new Mock<INoteService>();
            noteServiceMock.Setup(s => s.GetNotes(It.IsAny<string>(), 0, 500)).Returns(expectedResult);

            NoteController service = new NoteController(noteServiceMock.Object, httpContextAccessorMock.Object);
            var actualResult = service.GetAll(Hdid);
            RequestResult<IEnumerable<UserNote>> actualRequestResult = (RequestResult<IEnumerable<UserNote>>)((JsonResult)actualResult).Value;
            Assert.Equal(Common.Constants.ResultType.Success, actualRequestResult.ResultStatus);
        }

        private static Mock<IHttpContextAccessor> CreateValidHttpContext(string token, string userId, string hdid)
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            headerDictionary.Add("referer", "http://localhost/");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("hdid", hdid),
                new Claim("auth_time", "123"),
                new Claim("access_token", token),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties?.StoreTokens(new[]
            {
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = token,
                },
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            httpContextAccessorMock
                .Setup(x => x.HttpContext.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            return httpContextAccessorMock;
        }
    }
}
