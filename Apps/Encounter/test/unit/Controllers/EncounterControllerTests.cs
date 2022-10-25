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
namespace HealthGateway.EncounterTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Encounter.Controllers;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.PHSA;
    using HealthGateway.Encounter.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for EncounterController.
    /// </summary>
    public class EncounterControllerTests
    {
        private const string Hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";

        /// <summary>
        /// GetEncounters - Happy Path.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetEncounters()
        {
            RequestResult<IEnumerable<EncounterModel>> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 2,
                ResourcePayload = new List<EncounterModel>
                {
                    new()
                    {
                        Id = "1",
                        EncounterDate = new DateTime(2020, 05, 27),
                        SpecialtyDescription = "LABORATORY MEDICINE",
                        PractitionerName = "PRACTITIONER NAME",
                        Clinic = new Clinic
                        {
                            Name = "LOCATION NAME",
                        },
                    },
                    new()
                    {
                        Id = "2",
                        EncounterDate = new DateTime(2020, 06, 27),
                        SpecialtyDescription = "LABORATORY MEDICINE",
                        PractitionerName = "PRACTITIONER NAME",
                        Clinic = new Clinic
                        {
                            Name = "LOCATION NAME",
                        },
                    },
                },
            };

            Mock<IEncounterService> svcMock = new();
            svcMock.Setup(s => s.GetEncounters(Hdid)).ReturnsAsync(expectedRequestResult);

            EncounterController controller = new(new Mock<ILogger<EncounterController>>().Object, svcMock.Object);

            // Act
            RequestResult<IEnumerable<EncounterModel>> actual = await controller.GetEncounters(Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Success);

            Assert.Equal(2, actual?.ResourcePayload?.Count());
            Assert.Equal(2, actual?.TotalResultCount);
        }

        /// <summary>
        /// GetHospitalVisits - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetHospitalVisits()
        {
            RequestResult<HospitalVisitResult> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
                ResourcePayload = new HospitalVisitResult
                {
                    HospitalVisits = new List<HospitalVisitModel>
                    {
                        new()
                        {
                            EncounterId = "123",
                            AdmitDateTime = DateTime.Now,
                            EndDateTime = DateTime.Now.AddDays(10),
                        },
                    },
                },
            };

            Mock<IEncounterService> svcMock = new();
            svcMock.Setup(s => s.GetHospitalVisits(Hdid)).ReturnsAsync(expectedRequestResult);

            EncounterController controller = new(new Mock<ILogger<EncounterController>>().Object, svcMock.Object);

            // Act
            RequestResult<HospitalVisitResult> actual = await controller.GetHospitalVisits(Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Success);

            Assert.Equal(1, actual?.ResourcePayload?.HospitalVisits.Count());
            Assert.Equal(1, actual?.TotalResultCount);
        }
    }
}
