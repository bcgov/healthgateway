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
namespace HealthGateway.Immunization.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// VaccineStatusService's Unit Tests.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Ignore broken tests")]
    public class VaccineStatusServiceTests
    {
        private readonly string phn = "9735353315";
        private readonly DateTime dob = new DateTime(1967, 06, 02);
        private readonly DateTime dov = new DateTime(2021, 07, 04);
        private readonly string accessToken = "XXDDXX";
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private readonly string userId = "1001";
        private readonly IConfiguration configuration = GetIConfigurationRoot();

        /// <summary>
        /// GetPublicVaccineStatus - Happy Path.
        /// </summary>
        /// <param name="statusIndicator"> status indicator from delegate.</param>
        /// <param name="state">final state.</param>
        [Theory]
        [InlineData("Exempt", VaccineState.Exempt)]
        [InlineData("PartialDosesReceived", VaccineState.PartialDosesReceived)]
        [InlineData("AllDosesReceived", VaccineState.AllDosesReceived)]
        public void ShouldGetPublicVaccineStatus(string statusIndicator, VaccineState state)
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult()
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob,
                        StatusIndicator = statusIndicator,
                    },
                },
            };
            JWTModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new RequestResult<VaccineStatus>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus()
                {
                    Loaded = true,
                    RetryIn = 0,
                    PersonalHealthNumber = this.phn,
                    FirstName = "Bob",
                    LastName = "Test",
                    Birthdate = this.dob,
                    State = state,
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new Mock<IVaccineStatusDelegate>();
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, true)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new Mock<IAuthenticationDelegate>();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                GetMemoryCache(),
                this.GetHttpContextAccessor().Object);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, dobString, dovString).ConfigureAwait(true)).Result;
            Assert.True(expectedResult.IsDeepEqual(actualResult));
        }

        /// <summary>
        /// GetAuthenticatedVaccineStatus - Happy Path.
        /// </summary>
        /// <param name="statusIndicator"> status indicator from delegate.</param>
        /// <param name="state">final state.</param>
        [Theory]
        [InlineData("AllDosesReceived", VaccineState.AllDosesReceived)]
        public void ShouldGetAuthenticatedVaccineStatus(string statusIndicator, VaccineState state)
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult()
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob,
                        StatusIndicator = statusIndicator,
                        FederalVaccineProof = new(),
                    },
                },
            };
            JWTModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new RequestResult<VaccineStatus>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus()
                {
                    Loaded = true,
                    RetryIn = 0,                   
                    FirstName = "Bob",
                    LastName = "Test",
                    Birthdate = this.dob,
                    State = state,
                    FederalVaccineProof = new(),
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new Mock<IVaccineStatusDelegate>();
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, false)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new Mock<IAuthenticationDelegate>();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                GetMemoryCache(),
                this.GetHttpContextAccessor().Object);

            var actualResult = Task.Run(async () => await service.GetAuthenticatedVaccineStatus(this.hdid).ConfigureAwait(true)).Result;
            Assert.True(expectedResult.IsDeepEqual(actualResult));
        }

        /// <summary>
        /// GetVaccineStatus - when the status when there is data mismatch.
        /// </summary>
        [Fact]
        public void ShouldGetErrorDismatchVaccineStatus()
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = Common.Constants.ResultType.ActionRequired,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult()
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob,
                        StatusIndicator = "DataMismatch",
                    },
                },
            };
            JWTModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new RequestResult<VaccineStatus>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus()
                {
                    Loaded = true,
                    RetryIn = 0,
                    PersonalHealthNumber = this.phn,
                    FirstName = "Bob",
                    LastName = "Test",
                    Birthdate = this.dob,
                    State = VaccineState.DataMismatch,
                },
                ResultError = new()
                {
                    ActionCode = ActionType.DataMismatch,
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new Mock<IVaccineStatusDelegate>();
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, true)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new Mock<IAuthenticationDelegate>();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                GetMemoryCache(),
                this.GetHttpContextAccessor().Object);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, dobString, dovString).ConfigureAwait(true)).Result;
            Assert.Equal(Common.Constants.ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.Equal(expectedResult.ResultError.ActionCode, actual: actualResult.ResultError?.ActionCode);
        }

        /// <summary>
        /// GetPublicVaccineProof - Happy path.
        /// </summary>
        [Fact]
        public void ShouldPublicGetVaccineProof()
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult()
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob,
                        StatusIndicator = "PartialDosesReceived",
                        FederalVaccineProof = new()
                        {
                            Data = "this is pdf",
                            Encoding = "base64",
                            Type = "application/pdf",
                        },
                    },
                },
            };
            JWTModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new RequestResult<VaccineStatus>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus()
                {
                    Loaded = true,
                    RetryIn = 0,
                    PersonalHealthNumber = this.phn,
                    FirstName = "Bob",
                    LastName = "Test",
                    Birthdate = this.dob,
                    State = VaccineState.PartialDosesReceived,
                    FederalVaccineProof = new()
                    {
                        Data = "this is pdf",
                        Encoding = "base64",
                        Type = "application/pdf",
                    },
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new Mock<IVaccineStatusDelegate>();
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, true)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new Mock<IAuthenticationDelegate>();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                GetMemoryCache(),
                this.GetHttpContextAccessor().Object);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetPublicVaccineProof(this.phn, dobString, dovString).ConfigureAwait(true)).Result;
            Assert.Equal(expectedResult.ResourcePayload.FederalVaccineProof.Data, actual: actualResult.ResourcePayload?.Document.Data);
            Assert.NotNull(actualResult.ResourcePayload?.Document.Data);
            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetAuthenticatedVaccineProof - Happy path.
        /// </summary>
        [Fact]
        public void ShouldGetAuthenticatedVaccineProof()
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult()
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob,
                        StatusIndicator = "PartialDosesReceived",
                        FederalVaccineProof = new()
                        {
                            Data = "this is pdf",
                            Encoding = "base64",
                            Type = "application/pdf",
                        },
                    },
                },
            };
            JWTModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new RequestResult<VaccineStatus>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus()
                {
                    Loaded = true,
                    RetryIn = 0,
                    PersonalHealthNumber = this.phn,
                    FirstName = "Bob",
                    LastName = "Test",
                    Birthdate = this.dob,
                    State = VaccineState.PartialDosesReceived,
                    FederalVaccineProof = new()
                    {
                        Data = "this is pdf",
                        Encoding = "base64",
                        Type = "application/pdf",
                    },
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new Mock<IVaccineStatusDelegate>();

            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, false)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new Mock<IAuthenticationDelegate>();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                GetMemoryCache(),
                this.GetHttpContextAccessor().Object);

            var actualResult = Task.Run(async () => await service.GetAuthenticatedVaccineProof(this.hdid).ConfigureAwait(true)).Result;
            Assert.Equal(expectedResult.ResourcePayload.FederalVaccineProof.Data, actual: actualResult.ResourcePayload?.Document.Data);
            Assert.NotNull(actualResult.ResourcePayload?.Document.Data);
            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetVaccineStatus - Invalid PHN.
        /// </summary>
        [Fact]
        public void ShouldErrorOnPHN()
        {
            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                GetMemoryCache(),
                this.GetHttpContextAccessor().Object);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetPublicVaccineStatus("123", dobString, dovString).ConfigureAwait(true)).Result;
            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetVaccineStatus - Invalid DOB.
        /// </summary>
        [Fact]
        public void ShouldErrorOnDOB()
        {
            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                GetMemoryCache(),
                this.GetHttpContextAccessor().Object);

            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, "yyyyMMddx", dovString).ConfigureAwait(true)).Result;
            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetVaccineStatus - Invalid DOV.
        /// </summary>
        [Fact]
        public void ShouldErrorOnDOV()
        {
            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                GetMemoryCache(),
                this.GetHttpContextAccessor().Object);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, dobString, "yyyyMMddx").ConfigureAwait(true)).Result;
            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
        }

        private static IMemoryCache? GetMemoryCache()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddMemoryCache();
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetService<IMemoryCache>();
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }

        private Mock<IHttpContextAccessor> GetHttpContextAccessor()
        {
            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", this.accessToken);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = this.accessToken },
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            return httpContextAccessorMock;
        }

        private ClaimsPrincipal GetClaimsPrincipal()
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, this.userId),
                new Claim("hdid", this.hdid),
            };
            ClaimsIdentity identity = new(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }
    }
}
