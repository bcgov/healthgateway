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
namespace HealthGateway.EncounterTests
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using HealthGateway.Common.Models;
    using System;
    using HealthGateway.Encounter.Controllers;
    using HealthGateway.Encounter.Services;
    using Microsoft.AspNetCore.Http;
    using HealthGateway.Encounter.Models;
    using Microsoft.AspNetCore.Mvc;

    public class EncounterController_Test
    {
        private Mock<IEncounterService> encounterService;
        private EncounterController encounterController;


        public EncounterController_Test()
        {
            encounterService = new Mock<IEncounterService>();
            var loggerService = new Mock<ILogger<EncounterController>>();
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            encounterService.Setup(x => x.GetEncounters(It.IsAny<string>())).ReturnsAsync(GetEncounters());

            encounterController = new EncounterController(loggerService.Object, encounterService.Object, httpContextAccessorMock.Object);
        }

        [Fact]
        public void TestGetClaims()
        {
            var result = encounterController.GetEncounters("123");
            var jsonResult = Assert.IsType<JsonResult>(result.Result);
        }

        private RequestResult<IEnumerable<EncounterModel>> GetEncounters()
        {
            RequestResult<IEnumerable<EncounterModel>> result = new RequestResult<IEnumerable<EncounterModel>>();
            var encounters = new List<EncounterModel>();
            encounters.Add(new EncounterModel()
            {
                Id = "1",
                EncounterDate = new DateTime(2020 - 05 - 27),
                SpecialtyDescription = "LABORATORY MEDICINE",
                PractitionerName = "PRACTITIONER NAME",
                Clinic = new Clinic()
                {
                    Name = "LOCATION NAME",
                    AddrLine1 = "address line 1",
                    AddrLine2 = "address line 2",
                    AddrLine3 = "address line 3",
                    AddrLine4 = "address line 4",
                    City = "Victoria",
                    PostalCode = "V9V9V9",
                    Province = "BC",
                }
            });
            encounters.Add(new EncounterModel()
            {
                Id = "2",
                EncounterDate = new DateTime(2020 - 06 - 27),
                SpecialtyDescription = "LABORATORY MEDICINE",
                PractitionerName = "PRACTITIONER NAME",
                Clinic = new Clinic()
                {
                    Name = "LOCATION NAME",
                    AddrLine1 = "address line 1",
                    AddrLine2 = "address line 2",
                    AddrLine3 = "address line 3",
                    AddrLine4 = "address line 4",
                    City = "Victoria",
                    PostalCode = "V9V9V9",
                    Province = "BC",
                }
            });
            result.ResourcePayload = encounters;
            return result;
        }
    }
}
