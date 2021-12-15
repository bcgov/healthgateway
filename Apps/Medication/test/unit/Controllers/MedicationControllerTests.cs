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
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Controllers;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// MedicationController's Unit Tests.
    /// </summary>
    public class MedicationControllerTests
    {
        /// <summary>
        /// GetMedications - Not Found.
        /// </summary>
        [Fact]
        public void ShouldNotGetSingleMedication()
        {
            // Setup
            Mock<IMedicationService> serviceMock = new();
            serviceMock.Setup(s => s.GetMedications(It.IsAny<List<string>>())).Returns(new Dictionary<string, MedicationInformation>());

            string drugIdentifier = "000001";
            string paddedDin = drugIdentifier.PadLeft(8, '0');
            MedicationController controller = new(serviceMock.Object);

            // Act
            RequestResult<MedicationInformation> actual = controller.GetMedication(drugIdentifier);

            // Verify
            serviceMock.Verify(s => s.GetMedications(new List<string> { paddedDin }), Times.Once());
            Assert.True(actual.TotalResultCount == 0);
        }

        /// <summary>
        /// GetMedications - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetSingleMedication()
        {
            // Setup
            string drugIdentifier = "00000001";
            Dictionary<string, MedicationInformation> expectedResult = new()
            {
                {
                    drugIdentifier,
                    new MedicationInformation()
                    {
                        DIN = drugIdentifier,
                        FederalData = new FederalDrugSource()
                        {
                            DrugProduct = new DrugProduct()
                            {
                                DrugCode = drugIdentifier,
                            },
                        },
                    }
                },
            };

            Mock<IMedicationService> serviceMock = new();
            serviceMock.Setup(s => s.GetMedications(It.IsAny<List<string>>())).Returns(expectedResult);

            string paddedDin = drugIdentifier.PadLeft(8, '0');
            MedicationController controller = new(serviceMock.Object);

            // Act
            RequestResult<MedicationInformation> actual = controller.GetMedication(drugIdentifier);

            // Verify
            serviceMock.Verify(s => s.GetMedications(new List<string> { paddedDin }), Times.Once());
            Assert.True(actual.TotalResultCount == 1);
        }

        /// <summary>
        /// GetMedications - Happy Path (Multiple).
        /// </summary>
        [Fact]
        public void ShouldGetMultipleMedications()
        {
            // Setup
            Mock<IMedicationService> serviceMock = new();
            serviceMock.Setup(s => s.GetMedications(It.IsAny<List<string>>())).Returns(new Dictionary<string, MedicationInformation>());

            List<string> drugIdentifiers = new() { "000001", "000003", "000003" };
            List<string> paddedDinList = drugIdentifiers.Select(x => x.PadLeft(8, '0')).ToList();
            MedicationController controller = new(serviceMock.Object);

            // Act
            RequestResult<IDictionary<string, MedicationInformation>> actual = controller.GetMedications(drugIdentifiers);

            // Verify
            serviceMock.Verify(s => s.GetMedications(paddedDinList), Times.Once());
            Assert.True(actual?.ResourcePayload?.Count == 0);
        }
    }
}
