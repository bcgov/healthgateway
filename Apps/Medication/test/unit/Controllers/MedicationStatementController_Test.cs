//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Medication.Test
{
    using HealthGateway.Medication.Controllers;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Http;
    using HealthGateway.Common.Models;
    using Moq;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Xunit;


    public class MedicationStatementController_Test
    {
        [Fact]
        public async Task ShouldGetMedicationStatemets()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            Mock<IMedicationStatementService> svcMock = new Mock<IMedicationStatementService>();
            svcMock.Setup(s => s.GetMedicationStatements(hdid)).ReturnsAsync(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()));
            MedicationStatementController controller = new MedicationStatementController(svcMock.Object);

            // Act
            RequestResult<List<MedicationStatement>> actual = await controller.GetMedicationStatements(hdid);

            // Verify
            Assert.True(actual.ResourcePayload.Count == 0);
        }

        [Fact]
        public async Task ShouldMapError()
        {
            // Setup
            string errorMessage = "The error message";
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";

            Mock<IMedicationStatementService> svcMock = new Mock<IMedicationStatementService>();
            svcMock.Setup(s => s.GetMedicationStatements(hdid)).ReturnsAsync(new HNMessage<List<MedicationStatement>>(true, errorMessage));

            MedicationStatementController controller = new MedicationStatementController(svcMock.Object);

            // Act
            RequestResult<List<MedicationStatement>> actual = await controller.GetMedicationStatements(hdid);

            // Verify
            Assert.Null(actual.ResourcePayload);
            Assert.Equal(errorMessage, actual.ErrorMessage);
        }
    }
}
