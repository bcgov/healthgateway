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
namespace HealthGateway.AdminWebClientTests.Services.Test;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using HealthGateway.Database.Constants;
using HealthGateway.Database.Delegates;
using HealthGateway.Database.Models;
using HealthGateway.Database.Wrapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AuthenticationService = HealthGateway.Admin.Services.AuthenticationService;
using IAuthenticationService = HealthGateway.Admin.Services.IAuthenticationService;

/// <summary>
/// AuthenticationService's Unit Tests.
/// </summary>
public class AuthenticationServiceTests
{
    private readonly string accessToken = "ACCESS-TOKEN";
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationServiceTests"/> class.
    /// </summary>
    public AuthenticationServiceTests()
    {
        this.configuration = GetIConfigurationRoot();
    }

    /// <summary>
    /// Add last login date time when admin user profile record does not exist in DB.
    /// </summary>
    [Fact]
    public void ShouldAddLastLoginDateTimeWhenAdminUserProfileDoesNotExist()
    {
        // Arrange
        DbResult<AdminUserProfile> getResult = new()
        {
            Status = DbStatusCode.NotFound,
        };

        Mock<IAdminUserProfileDelegate> profileDelegateMock = new();
        profileDelegateMock.Setup(s => s.GetAdminUserProfile(It.IsAny<string>())).Returns(getResult);

        DbResult<AdminUserProfile> insertResult = new()
        {
            Status = DbStatusCode.Created,
            Payload = new AdminUserProfile
            {
                Username = "username",
            },
        };
        profileDelegateMock.Setup(s => s.Add(It.IsAny<AdminUserProfile>())).Returns(insertResult);

        IAuthenticationService service = new AuthenticationService(
            new Mock<ILogger<AuthenticationService>>().Object,
            this.GetHttpContextAccessor().Object,
            this.configuration,
            profileDelegateMock.Object);

        // Act
        service.SetLastLoginDateTime();

        // Assert
        profileDelegateMock.Verify(m => m.Add(It.IsAny<AdminUserProfile>()), Times.Once);
        profileDelegateMock.Verify(m => m.Update(It.IsAny<AdminUserProfile>(), true), Times.Never);
    }

    /// <summary>
    /// Update last login date time when admin user profile record exists in DB.
    /// </summary>
    [Fact]
    public void ShouldUpdateLastLoginDateTimeWhenAdminUserProfileExists()
    {
        // Arrange
        AdminUserProfile profile = new()
        {
            Username = "username",
        };

        DbResult<AdminUserProfile> getResult = new()
        {
            Status = DbStatusCode.Read,
            Payload = profile,
        };

        Mock<IAdminUserProfileDelegate> profileDelegateMock = new();
        profileDelegateMock.Setup(s => s.GetAdminUserProfile(It.IsAny<string>())).Returns(getResult);

        DbResult<AdminUserProfile> updateResult = new()
        {
            Status = DbStatusCode.Updated,
            Payload = profile,
        };
        profileDelegateMock.Setup(s => s.Update(It.IsAny<AdminUserProfile>(), true)).Returns(updateResult);

        IAuthenticationService service = new AuthenticationService(
            new Mock<ILogger<AuthenticationService>>().Object,
            this.GetHttpContextAccessor().Object,
            this.configuration,
            profileDelegateMock.Object);

        // Act
        service.SetLastLoginDateTime();

        // Assert
        profileDelegateMock.Verify(m => m.Add(It.IsAny<AdminUserProfile>()), Times.Never);
        profileDelegateMock.Verify(m => m.Update(It.IsAny<AdminUserProfile>(), true), Times.Once);
    }

    private static IConfigurationRoot GetIConfigurationRoot()
    {
        Dictionary<string, string?> myConfiguration = new()
        {
            { "EnabledRoles", "[ \"AdminUser\", \"AdminReviewer\", \"SupportUser\" ]" },
        };

        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddInMemoryCollection(myConfiguration.ToList<KeyValuePair<string, string?>>())
            .Build();
    }

    private static ClaimsPrincipal GetClaimsPrincipal()
    {
        List<Claim> claims = new()
        {
            new Claim("auth_time", "1640202141"),
            new Claim("preferred_username", "username"),
            new Claim(ClaimTypes.Name, "username"),
            new Claim(ClaimTypes.Email, "user@idr"),
        };
        ClaimsIdentity identity = new(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private Mock<IHttpContextAccessor> GetHttpContextAccessor()
    {
        ClaimsPrincipal claimsPrincipal = GetClaimsPrincipal();

        IHeaderDictionary headerDictionary = new HeaderDictionary
        {
            { "Authorization", this.accessToken },
        };
        Mock<HttpRequest> httpRequestMock = new();
        httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
        Mock<HttpContext> httpContextMock = new();
        httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
        httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
        Mock<ConnectionInfo> connectionMock = new();
        connectionMock.Setup(c => c.RemoteIpAddress).Returns(new IPAddress(1000));
        httpContextMock.Setup(s => s.Connection).Returns(connectionMock.Object);

        Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

        Mock<Microsoft.AspNetCore.Authentication.IAuthenticationService> authenticationMock = new();
        httpContextAccessorMock
            .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(Microsoft.AspNetCore.Authentication.IAuthenticationService)))
            .Returns(authenticationMock.Object);
        AuthenticateResult authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
        authResult.Properties.StoreTokens(
            new[]
            {
                new AuthenticationToken { Name = "access_token", Value = this.accessToken },
            });
        authenticationMock
            .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
            .ReturnsAsync(authResult);

        return httpContextAccessorMock;
    }
}
