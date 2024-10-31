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
namespace HealthGateway.ImmunizationTests.Controllers.Test
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Immunization.Controllers;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// PublicVaccineStatusController's Unit Tests.
    /// </summary>
    public class PublicVaccineStatusControllerTests
    {
        private readonly string phn = "1234567890";
        private readonly string dob = "1990-01-05";
        private readonly string dov = "2021-06-05";

        /// <summary>
        /// GetVaccineStatus - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetVaccineStatus()
        {
            RequestResult<VaccineStatus> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 2,
                ResourcePayload = new()
                {
                    PersonalHealthNumber = this.phn,
                    FirstName = "Bob",
                    LastName = "Test",
                    Birthdate = DateOnly.ParseExact(this.dob, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    VaccineDate = DateOnly.ParseExact(this.dov, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                },
            };

            Mock<IVaccineStatusService> svcMock = new();
            svcMock.Setup(s => s.GetPublicVaccineStatusAsync(this.phn, this.dob, this.dov, It.IsAny<CancellationToken>())).ReturnsAsync(expectedRequestResult);

            PublicVaccineStatusController controller = new(svcMock.Object);

            // Act
            RequestResult<VaccineStatus> actual = await controller.GetVaccineStatus(this.phn, this.dob, this.dov, default);

            // Verify
            Assert.Equal(ResultType.Success, actual.ResultStatus);
            actual.ShouldDeepEqual(expectedRequestResult);
        }
    }
}
