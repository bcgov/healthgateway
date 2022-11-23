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
namespace HealthGateway.AdminWebClientTests.Delegates.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Api;
    using HealthGateway.Admin.Delegates;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// VaccineStatusDelegate's Unit Tests.
    /// </summary>
    public class VaccineStatusDelegateTests
    {
        private readonly string accessToken = "XXDDXX";
        private readonly IConfiguration configuration;
        private readonly DateTime dob = new(1990, 01, 05);
        private readonly string phn = "9735353315";

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusDelegateTests"/> class.
        /// </summary>
        public VaccineStatusDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetVaccineStatusWithRetries - happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusWithRetries()
        {
            PhsaResult<VaccineStatusResult> expectedPayload = new()
            {
                Result = new()
                {
                    FirstName = "Bob",
                    StatusIndicator = "Exempt",
                },
            };

            Mock<IImmunizationAdminApi> mockImmunizationApi = new();
            mockImmunizationApi.Setup(a => a.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken)).ReturnsAsync(expectedPayload);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                this.configuration);

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatusWithRetries(this.phn, this.dob, this.accessToken).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            expectedPayload.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetVaccineStatusWithRetries - no result.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusWithRetriesNoResult()
        {
            PhsaResult<VaccineStatusResult> expectedPayload = new();
            RequestResultError expectedError = GetRequestResultError();

            Mock<IImmunizationAdminApi> mockImmunizationApi = new();
            mockImmunizationApi.Setup(a => a.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken)).ReturnsAsync(expectedPayload);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                this.configuration);

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatusWithRetries(this.phn, this.dob, this.accessToken).ConfigureAwait(true);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            expectedError.ShouldDeepEqual(actualResult.ResultError);
        }

        /// <summary>
        /// GetVaccineStatusWithRetries - HTTP error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetVaccineStatusHttpError()
        {
            RequestResultError expectedError = GetRequestResultError();

            Mock<IImmunizationAdminApi> mockImmunizationApi = new();
            mockImmunizationApi.Setup(a => a.GetVaccineStatus(It.IsAny<VaccineStatusQuery>(), this.accessToken))
                .ThrowsAsync(new HttpRequestException(null, null, HttpStatusCode.BadRequest));

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            IVaccineStatusDelegate vaccineStatusDelegate = new RestVaccineStatusDelegate(
                loggerFactory.CreateLogger<RestVaccineStatusDelegate>(),
                mockImmunizationApi.Object,
                this.configuration);

            RequestResult<PhsaResult<VaccineStatusResult>> actualResult =
                await vaccineStatusDelegate.GetVaccineStatusWithRetries(this.phn, this.dob, this.accessToken).ConfigureAwait(true);

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

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "PHSA:BaseUrl", "https://some-test-url/" },
            };
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }
    }
}
