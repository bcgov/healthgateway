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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// VaccineStatusService's Unit Tests.
    /// </summary>
    public class VaccineStatusServiceTests
    {
        private readonly string phn = "9735353315";
        private readonly DateTime dob = new(1967, 06, 02);
        private readonly DateTime dov = new(2021, 07, 04);
        private readonly string accessToken = "XXDDXX";
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private readonly string userId = "1001";
        private readonly IConfiguration configuration = GetIConfigurationRoot();

        /// <summary>
        /// GetPublicVaccineStatus and GetAuthenticatedVaccineStatus - Happy Path.
        /// </summary>
        /// <param name="statusIndicator"> status indicator from delegate.</param>
        /// <param name="state">final state.</param>
        /// <param name="isPublicEndpoint">check to determine if the test is for public or authenticated page.</param>
        [Theory]
        [InlineData("Exempt", VaccineState.Exempt, true)]
        [InlineData("PartialDosesReceived", VaccineState.PartialDosesReceived, true)]
        [InlineData("AllDosesReceived", VaccineState.AllDosesReceived, true)]
        [InlineData("Exempt", VaccineState.Exempt, false)]
        [InlineData("PartialDosesReceived", VaccineState.PartialDosesReceived, false)]
        [InlineData("AllDosesReceived", VaccineState.AllDosesReceived, false)]
        public void ShouldGetVaccineStatus(string statusIndicator, VaccineState state, bool isPublicEndpoint)
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
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

            RequestResult<VaccineStatus> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus()
                {
                    Loaded = true,
                    RetryIn = 0,
                    PersonalHealthNumber = isPublicEndpoint ? this.phn : null,
                    FirstName = "Bob",
                    LastName = "Test",
                    Birthdate = this.dob,
                    State = state,
                    FederalVaccineProof = new(),
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new();
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, isPublicEndpoint)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                this.GetHttpContextAccessor().Object);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                RequestResult<VaccineStatus> actualResultPublic = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, dobString, dovString).ConfigureAwait(true)).Result;

                expectedResult.ShouldDeepEqual(actualResultPublic);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = Task.Run(async () => await service.GetAuthenticatedVaccineStatus(this.hdid).ConfigureAwait(true)).Result;

                expectedResult.ShouldDeepEqual(actualResultAuthenticated);
            }
        }

        /// <summary>
        /// GetPublicVaccineStatus and GetAuthenticatedVaccineStatus - get the error result when the status indicator is DataMismatch.
        /// </summary>
        /// <param name="isPublicEndpoint">check to determine if the test is for public or authenticated page.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldGetErrorDataMismatchVaccineStatus(bool isPublicEndpoint)
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.ActionRequired,
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

            RequestResult<VaccineStatus> expectedResult = new()
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

            Mock<IVaccineStatusDelegate> mockDelegate = new();
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, isPublicEndpoint)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                this.GetHttpContextAccessor().Object);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineStatus> actualResultPublic = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, dobString, dovString).ConfigureAwait(true)).Result;

                Assert.Equal(ResultType.ActionRequired, actualResultPublic.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultPublic.ResultError?.ActionCode);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = Task.Run(async () => await service.GetAuthenticatedVaccineStatus(this.hdid).ConfigureAwait(true)).Result;

                Assert.Equal(ResultType.ActionRequired, actualResultAuthenticated.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultAuthenticated.ResultError?.ActionCode);
            }
        }

        /// <summary>
        /// GetPublicVaccineStatus and GetAuthenticatedVaccineStatus - get the error result when the refresh in progress is enable.
        /// </summary>
        /// <param name="isPublicEndpoint">check to determine if the test is for public or authenticated page.</param>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldGetErrorRefreshInProgressVaccineStatus(bool isPublicEndpoint)
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = true, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult()
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob,
                        StatusIndicator = "PartialDosesReceived",
                    },
                },
                ResultError = new()
                {
                    ActionCode = ActionType.Refresh,
                },
            };
            JWTModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus()
                {
                    Loaded = true,
                    RetryIn = 10000,
                    PersonalHealthNumber = this.phn,
                    FirstName = "Bob",
                    LastName = "Test",
                    Birthdate = this.dob,
                    State = VaccineState.PartialDosesReceived,
                },
                ResultError = new()
                {
                    ActionCode = ActionType.Refresh,
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new();
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, isPublicEndpoint)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                this.GetHttpContextAccessor().Object);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineStatus> actualResultPublic = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, dobString, dovString).ConfigureAwait(true)).Result;

                Assert.Equal(ResultType.ActionRequired, actualResultPublic.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultPublic.ResultError?.ActionCode);
                Assert.Equal(expectedResult.ResourcePayload.RetryIn, actualResultPublic.ResourcePayload?.RetryIn);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = Task.Run(async () => await service.GetAuthenticatedVaccineStatus(this.hdid).ConfigureAwait(true)).Result;

                Assert.Equal(ResultType.ActionRequired, actualResultAuthenticated.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultAuthenticated.ResultError?.ActionCode);
                Assert.Equal(expectedResult.ResourcePayload.RetryIn, actualResultAuthenticated.ResourcePayload?.RetryIn);
            }
        }

        /// <summary>
        /// GetPublicVaccineStatus and GetAuthenticatedVaccineStatus - get the error result when the status indicator is NotFound.
        /// </summary>
        /// <param name="isPublicEndpoint">check to determine if the test is for public (true) or authenticated (false) page.</param>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldGetErrorNotFoundVaccineStatus(bool isPublicEndpoint)
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResourcePayload = new PHSAResult<VaccineStatusResult>()
                {
                    LoadState = new PHSALoadState() { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult()
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob,
                        StatusIndicator = "NotFound",
                    },
                },
                ResultError = new()
                {
                    ActionCode = ActionType.Invalid,
                },
            };
            JWTModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new()
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
                    State = VaccineState.NotFound,
                },
                ResultError = new()
                {
                    ActionCode = ActionType.Invalid,
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new();
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, isPublicEndpoint)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                this.GetHttpContextAccessor().Object);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineStatus> actualResultPublic = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, dobString, dovString).ConfigureAwait(true)).Result;

                Assert.Equal(ResultType.ActionRequired, actualResultPublic.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultPublic.ResultError?.ActionCode);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = Task.Run(async () => await service.GetAuthenticatedVaccineStatus(this.hdid).ConfigureAwait(true)).Result;

                Assert.Equal(ResultType.ActionRequired, actualResultAuthenticated.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultAuthenticated.ResultError?.ActionCode);
            }
        }

        /// <summary>
        /// GetPublicVaccineProof and GetAuthenticatedVaccineProof - get the vaccine proof for public and authenticated site (happy path).
        /// </summary>
        /// <param name="isPublicEndpoint">check to determine if the test is for public (true) or authenticated (false) page.</param>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ShouldGetVaccineProof(bool isPublicEndpoint)
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
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

            RequestResult<VaccineStatus> expectedResult = new()
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

            Mock<IVaccineStatusDelegate> mockDelegate = new();
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken, isPublicEndpoint)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                this.GetHttpContextAccessor().Object);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<Models.VaccineProofDocument> actualResultPublic = Task.Run(async () => await service.GetPublicVaccineProof(this.phn, dobString, dovString).ConfigureAwait(true)).Result;

                Assert.Equal(expectedResult.ResourcePayload.FederalVaccineProof.Data, actualResultPublic.ResourcePayload?.Document.Data);
                Assert.NotNull(actualResultPublic.ResourcePayload?.Document.Data);
                Assert.Equal(ResultType.Success, actualResultPublic.ResultStatus);
            }
            else
            {
                RequestResult<Models.VaccineProofDocument> actualResultAuthenticated = Task.Run(async () => await service.GetAuthenticatedVaccineProof(this.hdid).ConfigureAwait(true)).Result;

                Assert.Equal(expectedResult.ResourcePayload.FederalVaccineProof.Data, actualResultAuthenticated.ResourcePayload?.Document.Data);
                Assert.NotNull(actualResultAuthenticated.ResourcePayload?.Document.Data);
                Assert.Equal(ResultType.Success, actualResultAuthenticated.ResultStatus);
            }
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
                this.GetHttpContextAccessor().Object);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<VaccineStatus> actualResult = Task.Run(async () => await service.GetPublicVaccineStatus("123", dobString, dovString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
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
                this.GetHttpContextAccessor().Object);

            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<VaccineStatus> actualResult = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, "yyyyMMddx", dovString).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
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
                this.GetHttpContextAccessor().Object);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<VaccineStatus> actualResult = Task.Run(async () => await service.GetPublicVaccineStatus(this.phn, dobString, "yyyyMMddx").ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }

        private Mock<IHttpContextAccessor> GetHttpContextAccessor()
        {
            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();
            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", this.accessToken },
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
