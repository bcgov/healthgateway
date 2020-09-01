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
            encounterService.Setup(x => x.GetClaims(It.IsAny<string>())).ReturnsAsync(GetClaims());

            encounterController = new EncounterController(loggerService.Object, encounterService.Object, httpContextAccessorMock.Object);
        }

        [Fact]
        public void TestGetClaims()
        {
            var result = encounterController.GetClaims("123");
            var jsonResult = Assert.IsType<JsonResult>(result.Result);
        }

        private RequestResult<IEnumerable<Claim>> GetClaims()
        {
            RequestResult<IEnumerable<Claim>> result = new RequestResult<IEnumerable<Claim>>();
            var claims = new List<Claim>();
            claims.Add(new Claim()
            {
                ClaimId = 1,
                ServiceDate = new DateTime(2020-05-27),
                FeeDesc = "TACROLIMUS",
                DiagnosticCode = new DiagnosticCode()
                {
                    DiagCode1 = "01L",
                    DiagCode2 = "02L",
                    DiagCode3 = "03L",
                },
                SpecialtyDesc = "LABORATORY MEDICINE",
                PractitionerNumber = "PRACTITIONER NAME",
                LocationName = "PAYEE NAME",
                LocationAddress = new LocationAddress()
                {
                    AddrLine1 = "address line 1",
                    AddrLine2 = "address line 2",
                    AddrLine3 = "address line 3",
                    AddrLine4 = "address line 4",
                    City = "Victoria",
                    PostalCode = "V9V9V9",
                    Province = "BC",
                }
            });
            claims.Add(new Claim()
            {
                ClaimId = 2,
                ServiceDate = new DateTime(2020 - 06 - 27),
                FeeDesc = "TACROLIMUS2",
                DiagnosticCode = new DiagnosticCode()
                {
                    DiagCode1 = "01L",
                    DiagCode2 = "02L",
                    DiagCode3 = "03L",
                },
                SpecialtyDesc = "LABORATORY MEDICINE",
                PractitionerNumber = "PRACTITIONER NAME",
                LocationName = "PAYEE NAME",
                LocationAddress = new LocationAddress()
                {
                    AddrLine1 = "address line 1",
                    AddrLine2 = "address line 2",
                    AddrLine3 = "address line 3",
                    AddrLine4 = "address line 4",
                    City = "Victoria",
                    PostalCode = "V9V9V9",
                    Province = "BC",
                }
            });
            result.ResourcePayload = claims;
            return result;
        }
    }
}
