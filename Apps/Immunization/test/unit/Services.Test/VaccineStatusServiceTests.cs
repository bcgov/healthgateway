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
    using System.Globalization;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.Extensions.Caching.Memory;
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
        private readonly DateTime dob = new DateTime(1990, 01, 05);
        private readonly DateTime dov = new DateTime(2021, 06, 05);
        private readonly string accessToken = "XXDDXX";

        private readonly IConfiguration configuration = GetIConfigurationRoot();

        /// <summary>
        /// GetVaccineStatus - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetVaccineStatus()
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
                    State = VaccineState.Exempt,
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
                GetMemoryCache());

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetVaccineStatus(this.phn, dobString, dovString).ConfigureAwait(true)).Result;
            Assert.True(expectedResult.IsDeepEqual(actualResult));
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
                GetMemoryCache());

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetVaccineStatus("123", dobString, dovString).ConfigureAwait(true)).Result;
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
                GetMemoryCache());

            string dovString = this.dov.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetVaccineStatus(this.phn, "yyyyMMddx", dovString).ConfigureAwait(true)).Result;
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
                GetMemoryCache());

            string dobString = this.dob.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
            var actualResult = Task.Run(async () => await service.GetVaccineStatus(this.phn, dobString, "yyyyMMddx").ConfigureAwait(true)).Result;
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
    }
}
