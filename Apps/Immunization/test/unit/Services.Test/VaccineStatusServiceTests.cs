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
    using System.Linq;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Parser;
    using HealthGateway.Immunization.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// VaccineStatusService's Unit Tests.
    /// </summary>
    public class VaccineStatusServiceTests
    {
        private readonly string phn = "9735353315";
        private readonly DateTime dob = new DateTime(1990, 01, 05);
        private readonly string accessToken = "XXDDXX";

        private readonly IConfiguration configuration = GetIConfigurationRoot();

        /// <summary>
        /// GetVaccineStatus - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetVaccineStatus()
        {
            RequestResult<PHSAResult<VaccineStatusResult>> delegateResult = new RequestResult<PHSAResult<VaccineStatusResult>>()
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
                        StatusIndicator = "Exempt",
                    },
                },
            };
            JWTModel jwtModel = new JWTModel()
            {
                AccessToken = this.accessToken,
            };
            RequestResult<VaccineStatus> expectedResult = new RequestResult<VaccineStatus>()
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
                    State = Constants.VaccineState.Exempt,
                },
            };

            Mock<IVaccineStatusDelegate> mockDelegate = new Mock<IVaccineStatusDelegate>();
            mockDelegate.Setup(s => s.GetVaccineStatus(this.phn, this.dob, this.accessToken)).Returns(Task.FromResult(delegateResult));

            Mock<IAuthenticationDelegate> mockAuthDelegate = new Mock<IAuthenticationDelegate>();
            mockAuthDelegate.Setup(s => s.AuthenticateAsUser(It.IsAny<Uri>(), It.IsAny<ClientCredentialsTokenRequest>())).Returns(jwtModel);

            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                mockAuthDelegate.Object,
                mockDelegate.Object);

            var actualResult = await service.GetVaccineStatus(this.phn, this.dob.ToString("yyyyMMdd", CultureInfo.CurrentCulture)).ConfigureAwait(true);
            Assert.True(expectedResult.IsDeepEqual(actualResult));
        }

        /// <summary>
        /// GetVaccineStatus - Invalid PHN.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldErrorOnPHN()
        {
            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object);

            var actualResult = await service.GetVaccineStatus("123", this.dob.ToString("yyyyMMdd", CultureInfo.CurrentCulture)).ConfigureAwait(true);
            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetVaccineStatus - Invalid DOB.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldErrorOnDOB()
        {
            IVaccineStatusService service = new VaccineStatusService(
                this.configuration,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<IVaccineStatusDelegate>().Object);

            var actualResult = await service.GetVaccineStatus(this.phn, "yyyyMMddx").ConfigureAwait(true);
            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
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
    }
}
