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
    using HealthGateway.Common.Models;
    using Moq;
    using System.Collections.Generic;
    using Xunit;


    public class MedicationController_Test
    {
        [Fact]
        public void ShouldGetSingleMedication()
        {
            // Setup
            Mock<IMedicationService> serviceMock = new Mock<IMedicationService>();
            serviceMock.Setup(s => s.GetMedications(It.IsAny<List<string>>())).Returns(new List<Medication>());

            string drugIdentifier = "000001";
            MedicationController controller = new MedicationController(serviceMock.Object);

            // Act
            RequestResult<List<Medication>> actual = controller.GetMedication(drugIdentifier);

            // Verify
            serviceMock.Verify(s => s.GetMedications(new List<string> { drugIdentifier }), Times.Once());
            Assert.True(actual.ResourcePayload.Count == 0);
        }

        [Fact]
        public void ShouldGetMultipleMedications()
        {
            // Setup
            Mock<IMedicationService> serviceMock = new Mock<IMedicationService>();
            serviceMock.Setup(s => s.GetMedications(It.IsAny<List<string>>())).Returns(new List<Medication>());

            List<string> drugIdentifiers = new List<string>() { "000001", "000003", "000003" };
            MedicationController controller = new MedicationController(serviceMock.Object);

            // Act
            RequestResult<List<Medication>> actual = controller.GetMedications(drugIdentifiers);

            // Verify
            serviceMock.Verify(s => s.GetMedications(drugIdentifiers), Times.Once());
            Assert.True(actual.ResourcePayload.Count == 0);
        }
    }
}
