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
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.PHSA;
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
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IImmunizationMappingService MappingService = new ImmunizationMappingService(MapperUtil.InitializeAutoMapper(), Configuration);

        private readonly string phn = "9735353315";
        private readonly DateOnly dob = DateOnly.Parse("1967-06-02", CultureInfo.InvariantCulture);
        private readonly DateOnly dov = DateOnly.Parse("2021-07-04", CultureInfo.InvariantCulture);
        private readonly string accessToken = "XXDDXX";
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";

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
                    {
                        RefreshInProgress = false,
                        BackOffMilliseconds = 500,
                    },
                    Result = new VaccineStatusResult
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob.ToDateTime(TimeOnly.MinValue),
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
            mockDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(delegateResult);
            mockDelegate.Setup(s => s.GetVaccineStatusPublicAsync(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(delegateResult);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.accessToken);

            IVaccineStatusService service = new VaccineStatusService(
                Configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                MappingService);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                RequestResult<VaccineStatus> actualResultPublic = await service.GetPublicVaccineStatusAsync(this.phn, dobString, dovString);

                actualResultPublic.ShouldDeepEqual(expectedResult);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = await service.GetAuthenticatedVaccineStatusAsync(this.hdid);

                actualResultAuthenticated.ShouldDeepEqual(expectedResult);
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
                    {
                        RefreshInProgress = false,
                        BackOffMilliseconds = 500,
                    },
                    Result = new VaccineStatusResult
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob.ToDateTime(TimeOnly.MinValue),
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
            mockDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(delegateResult);
            mockDelegate.Setup(s => s.GetVaccineStatusPublicAsync(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(delegateResult);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.accessToken);
            IVaccineStatusService service = new VaccineStatusService(
                Configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                MappingService);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineStatus> actualResultPublic = await service.GetPublicVaccineStatusAsync(this.phn, dobString, dovString);

                Assert.Equal(ResultType.ActionRequired, actualResultPublic.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultPublic.ResultError?.ActionCode);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = await service.GetAuthenticatedVaccineStatusAsync(this.hdid);

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
                    {
                        RefreshInProgress = true,
                        BackOffMilliseconds = 500,
                    },
                    Result = new VaccineStatusResult
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob.ToDateTime(TimeOnly.MinValue),
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
            mockDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(delegateResult);
            mockDelegate.Setup(s => s.GetVaccineStatusPublicAsync(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(delegateResult);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.accessToken);

            IVaccineStatusService service = new VaccineStatusService(
                Configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                MappingService);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineStatus> actualResultPublic = await service.GetPublicVaccineStatusAsync(this.phn, dobString, dovString);

                Assert.Equal(ResultType.ActionRequired, actualResultPublic.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultPublic.ResultError?.ActionCode);
                Assert.Equal(expectedResult.ResourcePayload.RetryIn, actualResultPublic.ResourcePayload?.RetryIn);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = await service.GetAuthenticatedVaccineStatusAsync(this.hdid);
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
                    {
                        RefreshInProgress = false,
                        BackOffMilliseconds = 500,
                    },
                    Result = new VaccineStatusResult
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob.ToDateTime(TimeOnly.MinValue),
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
            mockDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(delegateResult);
            mockDelegate.Setup(s => s.GetVaccineStatusPublicAsync(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(delegateResult);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.accessToken);

            IVaccineStatusService service = new VaccineStatusService(
                Configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                MappingService);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineStatus> actualResultPublic = await service.GetPublicVaccineStatusAsync(this.phn, dobString, dovString);

                Assert.Equal(ResultType.ActionRequired, actualResultPublic.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultPublic.ResultError?.ActionCode);
            }
            else
            {
                RequestResult<VaccineStatus> actualResultAuthenticated = await service.GetAuthenticatedVaccineStatusAsync(this.hdid);

                Assert.Equal(ResultType.ActionRequired, actualResultAuthenticated.ResultStatus);
                Assert.Equal(expectedResult.ResultError.ActionCode, actualResultAuthenticated.ResultError?.ActionCode);
            }
        }

        /// <summary>
        /// GetPublicVaccineProof and GetAuthenticatedVaccineProof - get the vaccine proof for public and authenticated site (happy
        /// path).
        /// </summary>
        /// <param name="isPublicEndpoint">check to determine if the test is for public (true) or authenticated (false) page.</param>
        /// <param name="federalVaccineProofExists">
        /// bool indicating whether federal vaccine proof from GetVaccineStatusAsync or GetVaccineStatusPublicAsync exists or not.
        /// </param>
        /// <param name="vaccineStatusResultType">
        /// The vaccine status request result type from GetVaccineStatusAsync or
        /// GetVaccineStatusPublicAsync.
        /// </param>
        /// <param name="vaccineStatusIndicator">
        /// The vaccine status indicator from GetVaccineStatusAsync or
        /// GetVaccineStatusPublicAsync.
        /// </param>
        /// <param name="expectedResultType">The expected request result type for get vaccine proof.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(true, false, ResultType.Success, "AllDosesReceived", ResultType.Error)]
        [InlineData(false, false, ResultType.Success, "AllDosesReceived", ResultType.Error)]
        [InlineData(true, true, ResultType.Success, "AllDosesReceived", ResultType.Success)]
        [InlineData(false, true, ResultType.Success, "AllDosesReceived", ResultType.Success)]
        [InlineData(true, true, ResultType.Success, "PartialDosesReceived", ResultType.Success)]
        [InlineData(false, true, ResultType.Success, "PartialDosesReceived", ResultType.Success)]
        [InlineData(true, true, ResultType.Success, "Threshold", ResultType.ActionRequired)]
        [InlineData(false, true, ResultType.Success, "Threshold", ResultType.ActionRequired)]
        [InlineData(true, true, ResultType.Success, "Blocked", ResultType.ActionRequired)]
        [InlineData(false, true, ResultType.Success, "Blocked", ResultType.ActionRequired)]
        [InlineData(true, true, ResultType.Success, "DataMismatch", ResultType.ActionRequired)]
        [InlineData(false, true, ResultType.Success, "DataMismatch", ResultType.ActionRequired)]
        public async Task ShouldGetVaccineProof(bool isPublicEndpoint, bool federalVaccineProofExists, ResultType vaccineStatusResultType, string vaccineStatusIndicator, ResultType expectedResultType)
        {
            RequestResult<PhsaResult<VaccineStatusResult>> vaccineStatusResult = new()
            {
                ResultStatus = vaccineStatusResultType,
                ResourcePayload = new PhsaResult<VaccineStatusResult>
                {
                    LoadState = new PhsaLoadState
                    {
                        RefreshInProgress = false,
                        BackOffMilliseconds = 500,
                    },
                    Result = new VaccineStatusResult
                    {
                        FirstName = "Bob",
                        LastName = "Test",
                        Birthdate = this.dob.ToDateTime(TimeOnly.MinValue),
                        StatusIndicator = vaccineStatusIndicator,
                        FederalVaccineProof = federalVaccineProofExists
                            ? new()
                            {
                                Data = "this is pdf",
                                Encoding = "base64",
                                Type = "application/pdf",
                            }
                            : null,
                    },
                },
            };

            JwtModel jwtModel = new()
            {
                AccessToken = this.accessToken,
            };

            RequestResult<VaccineStatus> expectedResult = new()
            {
                ResultStatus = vaccineStatusResult.ResultStatus,
                ResourcePayload = new VaccineStatus
                {
                    Loaded = true,
                    RetryIn = 0,
                    PersonalHealthNumber = this.phn,
                    FirstName = "Bob",
                    LastName = "Test",
                    Birthdate = this.dob,
                    State = VaccineState.PartialDosesReceived,
                    FederalVaccineProof = federalVaccineProofExists
                        ? new()
                        {
                            Data = "this is pdf",
                            Encoding = "base64",
                            Type = "application/pdf",
                        }
                        : null,
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new();
            mockDelegate.Setup(s => s.GetVaccineStatusAsync(It.IsAny<string>(), It.IsAny<bool>(), this.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(vaccineStatusResult);
            mockDelegate.Setup(s => s.GetVaccineStatusPublicAsync(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(vaccineStatusResult);

            Mock<IAuthenticationDelegate> mockAuthDelegate = new();
            mockAuthDelegate.Setup(s => s.AuthenticateAsSystemAsync(It.IsAny<ClientCredentialsRequest>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(jwtModel);
            mockAuthDelegate.Setup(s => s.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.accessToken);

            IVaccineStatusService service = new VaccineStatusService(
                Configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                mockAuthDelegate.Object,
                mockDelegate.Object,
                null,
                MappingService);

            if (isPublicEndpoint)
            {
                string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

                RequestResult<VaccineProofDocument> actualResultPublic = await service.GetPublicVaccineProofAsync(this.phn, dobString, dovString);
                Assert.Equal(expectedResult.ResourcePayload.FederalVaccineProof?.Data, actualResultPublic.ResourcePayload?.Document.Data);
                Assert.Equal(expectedResultType, actualResultPublic.ResultStatus);
            }
            else
            {
                RequestResult<VaccineProofDocument> actualResultAuthenticated = await service.GetAuthenticatedVaccineProofAsync(this.hdid);
                Assert.Equal(expectedResult.ResourcePayload.FederalVaccineProof?.Data, actualResultAuthenticated.ResourcePayload?.Document.Data);
                Assert.Equal(expectedResultType, actualResultAuthenticated.ResultStatus);
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
                Configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                null,
                MappingService);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<VaccineStatus> actualResult = await service.GetPublicVaccineStatusAsync("123", dobString, dovString);

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
                Configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                null,
                MappingService);

            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<VaccineStatus> actualResult = await service.GetPublicVaccineStatusAsync(this.phn, "yyyyMMddx", dovString);

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
                Configuration,
                new Mock<ILogger<VaccineStatusService>>().Object,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object,
                null,
                MappingService);

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);

            RequestResult<VaccineStatus> actualResult = await service.GetPublicVaccineStatusAsync(this.phn, dobString, "yyyyMMddx");

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
