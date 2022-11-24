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
namespace HealthGateway.ImmunizationTests.Delegates.Test
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Api;
    using HealthGateway.Immunization.Delegates;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// VaccineStatusDelegate's Unit Tests.
    /// </summary>
    public class VaccineStatusDelegateTests
    {
        private readonly string accessToken = "XXDDXX";
        private readonly DateTime dob = new(1990, 01, 05);
        private readonly string hdId = "43465786";
        private readonly string phn = "9735353315";

        /// <summary>
        /// GetVaccineStatusPublic - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusPublic()
        {
            PhsaResult<VaccineStatusResult> expectedPayload = new()
            {
                Result = new()
                {
                    FirstName = "Bob",
                    StatusIndicator = "Exempt",
                },
            };

            Mock<IImmunizationApi> mockImmunizationApi = new(MockBehavior.Strict);
            Mock<IImmunizationPublicApi> mockImmunizationPublicApi = new();
            mockImmunizationPublicApi.Setup(a => a.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken)).ReturnsAsync(expectedPayload);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                mockImmunizationPublicApi.Object);

            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = this.phn,
                DateOfBirth = this.dob,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatusPublic(query, this.accessToken).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            expectedPayload.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetVaccineStatusPublic - HTTP error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusPublicHttpError()
        {
            RequestResultError expectedError = GetRequestResultError();

            Mock<IImmunizationApi> mockImmunizationApi = new(MockBehavior.Strict);
            Mock<IImmunizationPublicApi> mockImmunizationPublicApi = new();
            mockImmunizationPublicApi.Setup(a => a.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken))
                .ThrowsAsync(new HttpRequestException(null, null, HttpStatusCode.BadRequest));

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                mockImmunizationPublicApi.Object);

            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = this.phn,
                DateOfBirth = this.dob,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatusPublic(query, this.accessToken).ConfigureAwait(true);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            expectedError.ShouldDeepEqual(actualResult.ResultError);
        }

        /// <summary>
        /// GetVaccineStatusPublic - no result.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusPublicNoResult()
        {
            PhsaResult<VaccineStatusResult> expectedPayload = new();
            RequestResultError expectedError = GetRequestResultError();

            Mock<IImmunizationApi> mockImmunizationApi = new(MockBehavior.Strict);
            Mock<IImmunizationPublicApi> mockImmunizationPublicApi = new();
            mockImmunizationPublicApi.Setup(a => a.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken)).ReturnsAsync(expectedPayload);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                mockImmunizationPublicApi.Object);

            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = this.phn,
                DateOfBirth = this.dob,
            };

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatusPublic(query, this.accessToken).ConfigureAwait(true);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            expectedError.ShouldDeepEqual(actualResult.ResultError);
        }

        /// <summary>
        /// GetVaccineStatus - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatus()
        {
            PhsaResult<VaccineStatusResult> expectedPayload = new()
            {
                Result = new()
                {
                    FirstName = "Bob",
                    StatusIndicator = "Exempt",
                },
            };

            Mock<IImmunizationApi> mockImmunizationApi = new();
            mockImmunizationApi.Setup(a => a.GetVaccineStatusAsync(this.hdId, It.IsAny<bool>(), this.accessToken)).ReturnsAsync(expectedPayload);
            Mock<IImmunizationPublicApi> mockImmunizationPublicApi = new(MockBehavior.Strict);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                mockImmunizationPublicApi.Object);

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatus(this.hdId, true, this.accessToken).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            expectedPayload.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetVaccineStatus - no result.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusNoResult()
        {
            PhsaResult<VaccineStatusResult> expectedPayload = new();
            RequestResultError expectedError = GetRequestResultError();

            Mock<IImmunizationApi> mockImmunizationApi = new();
            mockImmunizationApi.Setup(a => a.GetVaccineStatusAsync(this.hdId, It.IsAny<bool>(), this.accessToken)).ReturnsAsync(expectedPayload);
            Mock<IImmunizationPublicApi> mockImmunizationPublicApi = new(MockBehavior.Strict);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                mockImmunizationPublicApi.Object);

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatus(this.hdId, true, this.accessToken).ConfigureAwait(true);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            expectedError.ShouldDeepEqual(actualResult.ResultError);
        }

        /// <summary>
        /// GetVaccineStatus - HTTP error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusHttpError()
        {
            RequestResultError expectedError = GetRequestResultError();

            Mock<IImmunizationApi> mockImmunizationApi = new();
            mockImmunizationApi.Setup(a => a.GetVaccineStatusAsync(this.hdId, It.IsAny<bool>(), this.accessToken))
                .ThrowsAsync(new HttpRequestException(null, null, HttpStatusCode.BadRequest));
            Mock<IImmunizationPublicApi> mockImmunizationPublicApi = new(MockBehavior.Strict);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                mockImmunizationPublicApi.Object);

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatus(this.hdId, true, this.accessToken).ConfigureAwait(true);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            expectedError.ShouldDeepEqual(actualResult.ResultError);
        }

        private static RequestResultError GetRequestResultError()
        {
            return new()
            {
                ResultMessage = "Error getting vaccine status",
                ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
            };
        }
    }
}
