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
        private const string GenericVaccineProof =
            "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OV0gL0ZvbnRGaWxlMiAyOSAwIFI+Pg0KZW5kb2JqDQoxMCAwIG9iag0KPDwvVHlwZS9FeHRHU3RhdGUvQk0vTm9ybWFsL2NhIDE+Pg0KZW5kb2JqDQoxMSAwIG9iag0KPDwvVHlwZS9FeHRHU3RhdGUvQk0vTm9ybWFsL0NBIDE+Pg0KZW5kb2JqDQoxMiAwIG9iag0KPDwvVHlwZS9Gb250L1N1YnR5cGUvVHJ1ZVR5cGUvTmFtZS9GMi9CYXNlRm9udC9CQ0RGRUUrQXB0b3MvRW5jb2RpbmcvV2luQW5zaUVuY29kaW5nL0ZvbnREZXNjcmlwdG9yIDEzIDAgUi9GaXJzdENoYXIgMzIvTGFzdENoYXIgMzIvV2lkdGhzIDMxIDAgUj4+DQplbmRvYmoNCjEzIDAgb2JqDQo8PC9UeXBlL0ZvbnREZXNjcmlwdG9yL0ZvbnROYW1lL0JDREZFRStBcHRvcy9GbGFncyAzMi9JdGFsaWNBbmdsZSAwL0FzY2VudCA5MzkvRGVzY2VudCAtMjgyL0NhcEhlaWdodCA5MzkvQXZnV2lkdGggNTYxL01heFdpZHRoIDE2ODIvRm9udFdlaWdodCA0MDAvWEhlaWdodCAyNTAvU3RlbVYgNTYvRm9udEJCb3hbIC01MDAgLTI4MiAxMTgyIDkzOV0gL0ZvbnRGaWxlMiAyOSAwIFI+Pg0KZW5kb2JqDQoxNCAwIG9iag0KPDwvU3VidHlwZS9MaW5rL1JlY3RbIDM0My41OCA2NzUuMDIgNDY3Ljc3IDY5Ny41MV0gL0JTPDwvVyAwPj4vRiA0L0E8PC9UeXBlL0FjdGlvbi9TL1VSSS9VUkkoaHR0cHM6Ly93d3cyLmdvdi5iYy5jYS9nb3YvY29udGVudC9oZWFsdGgvbWFuYWdpbmcteW91ci1oZWFsdGgvaW1tdW5pemF0aW9ucy9jb3ZpZC0xOS1pbW11bml6YXRpb24jcmVjb3JkKSA+Pi9TdHJ1Y3RQYXJlbnQgMT4+DQplbmRvYmoNCjE1IDAgb2JqDQo8PC9BdXRob3IoUGVubmVyLCBKYXJlZCBbUEhTQV0pIC9DcmVhdG9yKP7/AE0AaQBjAHIAbwBzAG8AZgB0AK4AIABXAG8AcgBkACAAZgBvAHIAIABNAGkAYwByAG8AcwBvAGYAdAAgADMA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OVsgMzQwXSBdIA0KZW5kb2JqDQozMSAwIG9iag0KWyAyMDNdIA0KZW5kb2JqDQozMiAwIG9iag0KPDwvVHlwZS9NZXRhZGF0YS9TdWJ0eXBlL1hNTC9MZW5ndGggMzA5Nj4+DQpzdHJlYW0NCjw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+PHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iMy4xLTcwMSI+CjxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+CjxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiICB4bWxuczpwZGY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vcGRmLzEuMy8iPgo8cGRmOlByb2R1Y2VyPk1pY3Jvc29mdMKuIFdvcmQgZm9yIE1pY3Jvc29mdCAzNjU8L3BkZjpQcm9kdWNlcj48L3JkZjpEZXNjcmlwdGlvbj4KPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyI+CjxkYzpjcmVhdG9yPjxyZGY6U2VxPjxyZGY6bGk+UGVubmVyLCBKYXJlZCBbUEhTQV08L3JkZjpsaT48L3JkZjpTZXE+PC9kYzpjcmVhdG9yPjwvcmRmOkRlc2NyaXB0aW9uPgo8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiAgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIj4KPHhtcDpDcmVhdG9yVG9vbD5NaWNyb3NvZnTCriBXb3JkIGZvciBNaWNyb3NvZnQgMzY1PC94bXA6Q3JlYXRvclRvb2w+PHhtcDpDcmVhdGVEYXRlPjIwMjYtMDEtMDZUMTQ6NDM6NDEtMDg6MDA8L3htcDpDcmVhdGVEYXRlPjx4bXA6TW9kaWZ5RGF0ZT4yMDI2LTAxLTA2VDE0OjQzOjQxLTA4OjAwPC94bXA6TW9kaWZ5RGF0ZT48L3JkZjpEZXNjcmlwdGlvbj4KPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIj4KPHhtcE1NOkRvY3VtZW50SUQ+dXVpZDo5QTlDMTUzMS1FMDc4LTQ4MjItQjIxQy0zQkEyNkE1MTIzMDI8L3htcE1NOkRvY3VtZW50SUQ+PHhtcE1NOkluc3RhbmNlSUQ+dXVpZDo5QTlDMTUzMS1FMDc4LTQ4MjItQjIxQy0zQkEyNkE1MTIzMDI8L3htcE1NOkluc3RhbmNlSUQ+PC9yZGY6RGVzY3JpcHRpb24+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAo8L3JkZjpSREY+PC94OnhtcG1ldGE+PD94cGFja2V0IGVuZD0idyI/Pg0KZW5kc3RyZWFtDQplbmRvYmoNCjMzIDAgb2JqDQo8PC9EaXNwbGF5RG9jVGl0bGUgdHJ1ZT4+DQplbmRvYmoNCjM0IDAgb2JqDQo8PC9UeXBlL1hSZWYvU2l6ZSAzNC9XWyAxIDQgMl0gL1Jvb3QgMSAwIFIvSW5mbyAxNSAwIFIvSURbPDMxMTU5QzlBNzhFMDIyNDhCMjFDM0JBMjZBNTEyMzAyPjwzMTE1OUM5QTc4RTAyMjQ4QjIxQzNCQTI2QTUxMjMwMj5dIC9GaWx0ZXIvRmxhdGVEZWNvZGUvTGVuZ3RoIDEyMD4+DQpzdHJlYW0NCnicNc7NDUBAEAXgsf6JiBMlcHCQiAMN6MBNVTpQhkYkuqAAxnvMYb/M7szsiGjct6VnJvKykgMYA5wNuAOZyAk8XvolmckCAr6FFYgKHaijc/GITwISEpd8lZH2xd2fOcQmFjFaktT4IW1B34BhJxcYdbMH1DYNWw0KZW5kc3RyZWFtDQplbmRvYmoNCnhyZWYNCjAgMzUNCjAwMDAwMDAwMTYgNjU1MzUgZg0KMDAwMDAwMDAxNyAwMDAwMCBuDQowMDAwMDAwMTYzIDAwMDAwIG4NCjAwMDAwMDAyMTkgMDAwMDAgbg0KMDAwMDAwMDUxNCAwMDAwMCBuDQowMDAwMDAxMjExIDAwMDAwIG4NCjAwMDAwMDEzMzkgMDAwMDAgbg0KMDAwMDAwMTM2NyAwMDAwMCBuDQowMDAwMDAxNTIyIDAwMDAwIG4NCjAwMDAwMDE1OTUgMDAwMDAgbg0KMDAwMDAwMTgzMiAwMDAwMCBuDQowMDAwMDAxODg2IDAwMDAwIG4NCjAwMDAwMDE5NDAgMDAwMDAgbg0KMDAwMDAwMjEwNyAwMDAwMCBuDQowMDAwMDAyMzQ1IDAwMDAwIG4NCjAwMDAwMDI1ODQgMDAwMDAgbg0KMDAwMDAwMDAxNyA2NTUzNSBmDQowMDAwMDAwMDE4IDY1NTM1IGYNCjAwMDAwMDAwMTkgNjU1MzUgZg0KMDAwMDAwMDAyMCA2NTUzNSBmDQowMDAwMDAwMDIxIDY1NTM1IGYNCjAwMDAwMDAwMjIgNjU1MzUgZg0KMDAwMDAwMDAyMyA2NTUzNSBmDQowMDAwMDAwMDI0IDY1NTM1IGYNCjAwMDAwMDAwMjUgNjU1MzUgZg0KMDAwMDAwMDAyNiA2NTUzNSBmDQowMDAwMDAwMDI3IDY1NTM1IGYNCjAwMDAwMDAwMDAgNjU1MzUgZg0KMDAwMDAwMzM3MyAwMDAwMCBuDQowMDAwMDAzODkyIDAwMDAwIG4NCjAwMDAwMTQ4OTcgMDAwMDAgbg0KMDAwMDAxNTMyMCAwMDAwMCBuDQowMDAwMDE1MzQ3IDAwMDAwIG4NCjAwMDAwMTg1MjYgMDAwMDAgbg0KMDAwMDAxODU3MSAwMDAwMCBuDQp0cmFpbGVyDQo8PC9TaXplIDM1L1Jvb3QgMSAwIFIvSW5mbyAxNSAwIFIvSURbPDMxMTU5QzlBNzhFMDIyNDhCMjFDM0JBMjZBNTEyMzAyPjwzMTE1OUM5QTc4RTAyMjQ4QjIxQzNCQTI2QTUxMjMwMj5dID4+DQpzdGFydHhyZWYNCjE4ODkyDQolJUVPRg0KeHJlZg0KMCAwDQp0cmFpbGVyDQo8PC9TaXplIDM1L1Jvb3QgMSAwIFIvSW5mbyAxNSAwIFIvSURbPDMxMTU5QzlBNzhFMDIyNDhCMjFDM0JBMjZBNTEyMzAyPjwzMTE1OUM5QTc4RTAyMjQ4QjIxQzNCQTI2QTUxMjMwMj5dIC9QcmV2IDE4ODkyL1hSZWZTdG0gMTg1NzE+Pg0Kc3RhcnR4cmVmDQoxOTc0OQ0KJSVFT0Y=";

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
                                Data = GenericVaccineProof,
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
                            Data = GenericVaccineProof,
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
