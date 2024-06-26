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
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.PHSA;
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
        private readonly DateTime dob = DateTime.Parse("1990-01-05", CultureInfo.InvariantCulture);
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
            mockImmunizationPublicApi.Setup(a => a.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPayload);

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
                await vaccineStatusDelegate.GetVaccineStatusPublicAsync(query, this.accessToken, string.Empty);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            actualResult.ResourcePayload.ShouldDeepEqual(expectedPayload);
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
            mockImmunizationPublicApi.Setup(a => a.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
                await vaccineStatusDelegate.GetVaccineStatusPublicAsync(query, this.accessToken, string.Empty);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            actualResult.ResultError.ShouldDeepEqual(expectedError);
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
            mockImmunizationPublicApi.Setup(a => a.GetVaccineStatusAsync(It.IsAny<VaccineStatusQuery>(), this.accessToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPayload);

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
                await vaccineStatusDelegate.GetVaccineStatusPublicAsync(query, this.accessToken, string.Empty);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            actualResult.ResultError.ShouldDeepEqual(expectedError);
        }

        /// <summary>
        /// GetVaccineStatusAsync - happy path.
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
            mockImmunizationApi.Setup(a => a.GetVaccineStatusAsync(this.hdId, It.IsAny<bool>(), this.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(expectedPayload);
            Mock<IImmunizationPublicApi> mockImmunizationPublicApi = new(MockBehavior.Strict);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                mockImmunizationPublicApi.Object);

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatusAsync(this.hdId, true, this.accessToken);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            actualResult.ResourcePayload.ShouldDeepEqual(expectedPayload);
        }

        /// <summary>
        /// GetVaccineStatusAsync - no result.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusNoResult()
        {
            PhsaResult<VaccineStatusResult> expectedPayload = new();
            RequestResultError expectedError = GetRequestResultError();

            Mock<IImmunizationApi> mockImmunizationApi = new();
            mockImmunizationApi.Setup(a => a.GetVaccineStatusAsync(this.hdId, It.IsAny<bool>(), this.accessToken, It.IsAny<CancellationToken>())).ReturnsAsync(expectedPayload);
            Mock<IImmunizationPublicApi> mockImmunizationPublicApi = new(MockBehavior.Strict);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                mockImmunizationPublicApi.Object);

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatusAsync(this.hdId, true, this.accessToken);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            actualResult.ResultError.ShouldDeepEqual(expectedError);
        }

        /// <summary>
        /// GetVaccineStatusAsync - HTTP error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusHttpError()
        {
            RequestResultError expectedError = GetRequestResultError();

            Mock<IImmunizationApi> mockImmunizationApi = new();
            mockImmunizationApi.Setup(a => a.GetVaccineStatusAsync(this.hdId, It.IsAny<bool>(), this.accessToken, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException(null, null, HttpStatusCode.BadRequest));
            Mock<IImmunizationPublicApi> mockImmunizationPublicApi = new(MockBehavior.Strict);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                mockImmunizationPublicApi.Object);

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatusAsync(this.hdId, true, this.accessToken);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            actualResult.ResultError.ShouldDeepEqual(expectedError);
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
