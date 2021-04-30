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
    using System;
    using System.Collections.Generic;
    using HealthGateway.Common.Models;
    using HealthGateway.Encounter.Controllers;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// EncounterController's Unit Tests.
    /// </summary>
    public class EncounterControllerTests
    {
        private readonly Mock<IEncounterService> encounterService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterControllerTests"/> class.
        /// </summary>
        public EncounterControllerTests()
        {
            this.encounterService = new Mock<IEncounterService>();
            this.encounterService.Setup(x => x.GetEncounters(It.IsAny<string>())).ReturnsAsync(GetEncounters());
        }

        private static RequestResult<IEnumerable<EncounterModel>> GetEncounters()
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
                },
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
                },
            });
            result.ResourcePayload = encounters;
            return result;
        }
    }
}
