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
namespace HealthGateway.ImmunizationTests.Services.Test
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models.PHSA;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using HealthGateway.ImmunizationTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// VaccineStatusService's Unit Tests.
    /// </summary>
    public class VaccineStatusServiceTests
    {
        private readonly IMapper autoMapper = MapperUtil.InitializeAutoMapper();
        private readonly string phn = "9735353315";
        private readonly DateTime dob = DateTime.Parse("1967-06-02", CultureInfo.InvariantCulture);
        private readonly DateTime dov = DateTime.Parse("2021-07-04", CultureInfo.InvariantCulture);
        private readonly string accessToken = "XXDDXX";
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private readonly IConfiguration configuration = GetIConfigurationRoot();

        /// <summary>
        /// GetPublicVaccineStatus and GetAuthenticatedVaccineStatus - Happy Path.
        /// </summary>
        /// <param name="statusIndicator"> status indicator from delegate.</param>
        /// <param name="state">final state.</param>
        /// <param name="isPublicEndpoint">check to determine if the test is for public or authenticated page.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData("Exempt", VaccineState.Exempt, true)]
        [InlineData("PartialDosesReceived", VaccineState.PartialDosesReceived, true)]
        [InlineData("AllDosesReceived", VaccineState.AllDosesReceived, true)]
        [InlineData("Exempt", VaccineState.Exempt, false)]
        [InlineData("PartialDosesReceived", VaccineState.PartialDosesReceived, false)]
        [InlineData("AllDosesReceived", VaccineState.AllDosesReceived, false)]
        public async Task ShouldGetVaccineStatus(string statusIndicator, VaccineState state, bool isPublicEndpoint)
        {
            RequestResult<PhsaResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<VaccineStatusResult>
                {
                    LoadState = new PhsaLoadState
                        { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob,
                        StatusIndicator = statusIndicator,
                        FederalVaccineProof = new(),
                    },
                },
            };
            JwtModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus
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
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken)).Returns(Task.FromResult(delegateResult));
            mockDelegate.Setup(s => s.GetVaccineStatusPublic(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystem(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(this.accessToken);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                this.autoMapper);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                RequestResult<VaccineStatus> actualResultPublic = await service.GetPublicVaccineStatus(this.phn, dobString, dovString);

                expectedResult.ShouldDeepEqual(actualResultPublic);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = await service.GetAuthenticatedVaccineStatus(this.hdid);

                expectedResult.ShouldDeepEqual(actualResultAuthenticated);
            }
        }

        /// <summary>
        /// GetPublicVaccineStatus and GetAuthenticatedVaccineStatus - get the error result when the status indicator is
        /// DataMismatch.
        /// </summary>
        /// <param name="isPublicEndpoint">check to determine if the test is for public or authenticated page.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetErrorDataMismatchVaccineStatus(bool isPublicEndpoint)
        {
            RequestResult<PhsaResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResourcePayload = new PhsaResult<VaccineStatusResult>
                {
                    LoadState = new PhsaLoadState
                        { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob,
                        StatusIndicator = "DataMismatch",
                    },
                },
            };
            JwtModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus
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
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken)).Returns(Task.FromResult(delegateResult));
            mockDelegate.Setup(s => s.GetVaccineStatusPublic(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystem(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(this.accessToken);
            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                this.autoMapper);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineStatus> actualResultPublic = await service.GetPublicVaccineStatus(this.phn, dobString, dovString);

                Assert.Equal(ResultType.ActionRequired, actualResultPublic.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultPublic.ResultError?.ActionCode);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = await service.GetAuthenticatedVaccineStatus(this.hdid);

                Assert.Equal(ResultType.ActionRequired, actualResultAuthenticated.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultAuthenticated.ResultError?.ActionCode);
            }
        }

        /// <summary>
        /// GetPublicVaccineStatus and GetAuthenticatedVaccineStatus - get the error result when the refresh in progress is enable.
        /// </summary>
        /// <param name="isPublicEndpoint">check to determine if the test is for public or authenticated page.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldGetErrorRefreshInProgressVaccineStatus(bool isPublicEndpoint)
        {
            RequestResult<PhsaResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResourcePayload = new PhsaResult<VaccineStatusResult>
                {
                    LoadState = new PhsaLoadState
                        { RefreshInProgress = true, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult
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
            JwtModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus
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
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken)).Returns(Task.FromResult(delegateResult));
            mockDelegate.Setup(s => s.GetVaccineStatusPublic(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystem(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(this.accessToken);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                this.autoMapper);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineStatus> actualResultPublic = await service.GetPublicVaccineStatus(this.phn, dobString, dovString);

                Assert.Equal(ResultType.ActionRequired, actualResultPublic.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultPublic.ResultError?.ActionCode);
                Assert.Equal(expectedResult.ResourcePayload.RetryIn, actualResultPublic.ResourcePayload?.RetryIn);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = await service.GetAuthenticatedVaccineStatus(this.hdid);
                Assert.Equal(ResultType.ActionRequired, actualResultAuthenticated.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultAuthenticated.ResultError?.ActionCode);
                Assert.Equal(expectedResult.ResourcePayload.RetryIn, actualResultAuthenticated.ResourcePayload?.RetryIn);
            }
        }

        /// <summary>
        /// GetPublicVaccineStatus and GetAuthenticatedVaccineStatus - get the error result when the status indicator is NotFound.
        /// </summary>
        /// <param name="isPublicEndpoint">check to determine if the test is for public (true) or authenticated (false) page.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldGetErrorNotFoundVaccineStatus(bool isPublicEndpoint)
        {
            RequestResult<PhsaResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResourcePayload = new PhsaResult<VaccineStatusResult>
                {
                    LoadState = new PhsaLoadState
                        { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult
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
            JwtModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus
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
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken)).Returns(Task.FromResult(delegateResult));
            mockDelegate.Setup(s => s.GetVaccineStatusPublic(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystem(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(this.accessToken);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                this.autoMapper);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineStatus> actualResultPublic = await service.GetPublicVaccineStatus(this.phn, dobString, dovString);

                Assert.Equal(ResultType.ActionRequired, actualResultPublic.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultPublic.ResultError?.ActionCode);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = await service.GetAuthenticatedVaccineStatus(this.hdid);

                Assert.Equal(ResultType.ActionRequired, actualResultAuthenticated.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultAuthenticated.ResultError?.ActionCode);
            }
        }

        /// <summary>
        /// GetPublicVaccineProof and GetAuthenticatedVaccineProof - get the vaccine proof for public and authenticated site (happy
        /// path).
        /// </summary>
        /// <param name="isPublicEndpoint">check to determine if the test is for public (true) or authenticated (false) page.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldGetVaccineProof(bool isPublicEndpoint)
        {
            RequestResult<PhsaResult<VaccineStatusResult>> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<VaccineStatusResult>
                {
                    LoadState = new PhsaLoadState
                        { RefreshInProgress = false, BackOffMilliseconds = 500 },
                    Result = new VaccineStatusResult
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
            JwtModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResourcePayload = new VaccineStatus
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
            mockDelegate.Setup(s => s.GetVaccineStatus(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken)).Returns(Task.FromResult(delegateResult));
            mockDelegate.Setup(s => s.GetVaccineStatusPublic(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystem(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>(), It.IsAny<bool>())).Returns(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserToken()).Returns(this.accessToken);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                this.autoMapper);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineProofDocument> actualResultPublic = await service.GetPublicVaccineProof(this.phn, dobString, dovString);

                Assert.Equal(expectedResult.ResourcePayload.FederalVaccineProof.Data, actualResultPublic.ResourcePayload?.Document.Data);
                Assert.NotNull(actualResultPublic.ResourcePayload?.Document.Data);
                Assert.Equal(ResultType.Success, actualResultPublic.ResultStatus);
            }
            else
            {
                RequestResult<VaccineProofDocument> actualResultAuthenticated = await service.GetAuthenticatedVaccineProof(this.hdid);

                Assert.Equal(expectedResult.ResourcePayload.FederalVaccineProof.Data, actualResultAuthenticated.ResourcePayload?.Document.Data);
                Assert.NotNull(actualResultAuthenticated.ResourcePayload?.Document.Data);
                Assert.Equal(ResultType.Success, actualResultAuthenticated.ResultStatus);
            }
        }

        /// <summary>
        /// GetVaccineStatusAsync - Invalid PHN.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldErrorOnPHN()
        {
            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                null,
                this.autoMapper);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<VaccineStatus> actualResult = await service.GetPublicVaccineStatus("123", dobString, dovString);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetVaccineStatusAsync - Invalid DOB.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldErrorOnDOB()
        {
            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                null,
                this.autoMapper);

            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<VaccineStatus> actualResult = await service.GetPublicVaccineStatus(this.phn, "yyyyMMddx", dovString);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetVaccineStatusAsync - Invalid DOV.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldErrorOnDOV()
        {
            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                null,
                this.autoMapper);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<VaccineStatus> actualResult = await service.GetPublicVaccineStatus(this.phn, dobString, "yyyyMMddx");

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .Build();
        }
    }
}
