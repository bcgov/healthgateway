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
namespace HealthGateway.Medication.Controllers.Test
{
    using System.Linq;
    using HealthGateway.Medication.Controllers;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using HealthGateway.Common.Models;
    using Moq;
    using System.Collections.Generic;
    using Xunit;
    using HealthGateway.Database.Models;

    public class MedicationController_Test
    {
        [Fact]
        public void ShouldNotGetSingleMedication()
        {
            // Setup
            Mock<IMedicationService> serviceMock = new Mock<IMedicationService>();
            serviceMock.Setup(s => s.GetMedications(It.IsAny<List<string>>())).Returns(new Dictionary<string, MedicationResult>());

            string drugIdentifier = "000001";
            string paddedDin = drugIdentifier.PadLeft(8, '0');
            MedicationController controller = new MedicationController(serviceMock.Object);

            // Act
            RequestResult<MedicationResult> actual = controller.GetMedication(drugIdentifier);

            // Verify
            serviceMock.Verify(s => s.GetMedications(new List<string> { paddedDin }), Times.Once());
            Assert.True(actual.TotalResultCount == 0);
        }

        [Fact]
        public void ShouldGetSingleMedication()
        {
            // Setup
            string drugIdentifier = "00000001";
            Dictionary<string, MedicationResult> expectedResult = new Dictionary<string, MedicationResult>()
            {
                {
                    drugIdentifier,
                    new MedicationResult()
                    {
                        DIN = drugIdentifier,
                        FederalData = new FederalDrugSource()
                        {
                            DrugProduct = new DrugProduct()
                            {
                                DrugCode = drugIdentifier,
                            }
                        }
                    }
                },
            };

            Mock<IMedicationService> serviceMock = new Mock<IMedicationService>();
            serviceMock.Setup(s => s.GetMedications(It.IsAny<List<string>>())).Returns(expectedResult);


            string paddedDin = drugIdentifier.PadLeft(8, '0');
            MedicationController controller = new MedicationController(serviceMock.Object);

            // Act
            RequestResult<MedicationResult> actual = controller.GetMedication(drugIdentifier);

            // Verify
            serviceMock.Verify(s => s.GetMedications(new List<string> { paddedDin }), Times.Once());
            Assert.True(actual.TotalResultCount == 1);
        }

        [Fact]
        public void ShouldGetMultipleMedications()
        {
            // Setup
            Mock<IMedicationService> serviceMock = new Mock<IMedicationService>();
            serviceMock.Setup(s => s.GetMedications(It.IsAny<List<string>>())).Returns(new Dictionary<string, MedicationResult>());

            List<string> drugIdentifiers = new List<string>() { "000001", "000003", "000003" };
            List<string> paddedDinList = drugIdentifiers.Select(x => x.PadLeft(8, '0')).ToList();
            MedicationController controller = new MedicationController(serviceMock.Object);

            // Act
            RequestResult<Dictionary<string, MedicationResult>> actual = controller.GetMedications(drugIdentifiers);

            // Verify
            serviceMock.Verify(s => s.GetMedications(paddedDinList), Times.Once());
            Assert.True(actual.ResourcePayload.Count == 0);
        }
    }
}
