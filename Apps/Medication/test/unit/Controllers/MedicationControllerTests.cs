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
namespace HealthGateway.MedicationTests.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldNotGetSingleMedication()
        {
            // Setup
            Mock<IMedicationService> serviceMock = new();
            serviceMock.Setup(s => s.GetMedicationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<string, MedicationInformation>());

            const string drugIdentifier = "000001";
            string paddedDin = drugIdentifier.PadLeft(8, '0');
            MedicationController controller = new(serviceMock.Object);

            // Act
            RequestResult<MedicationInformation> actual = await controller.GetMedication(drugIdentifier, default);

            // Verify
            serviceMock.Verify(s => s.GetMedicationsAsync(new List<string> { paddedDin }, default), Times.Once());
            Assert.Equal(0, actual.TotalResultCount);
        }

        /// <summary>
        /// GetMedications - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetSingleMedication()
        {
            // Setup
            const string drugIdentifier = "00000001";
            Dictionary<string, MedicationInformation> expectedResult = new()
            {
                {
                    drugIdentifier,
                    new MedicationInformation
                    {
                        Din = drugIdentifier,
                        FederalData = new FederalDrugSource
                        {
                            DrugProduct = new DrugProduct
                            {
                                DrugCode = drugIdentifier,
                            },
                        },
                    }
                },
            };

            Mock<IMedicationService> serviceMock = new();
            serviceMock.Setup(s => s.GetMedicationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            string paddedDin = drugIdentifier.PadLeft(8, '0');
            MedicationController controller = new(serviceMock.Object);

            // Act
            RequestResult<MedicationInformation> actual = await controller.GetMedication(drugIdentifier, default);

            // Verify
            serviceMock.Verify(s => s.GetMedicationsAsync(new List<string> { paddedDin }, default), Times.Once());
            Assert.Equal(1, actual.TotalResultCount);
        }

        /// <summary>
        /// GetMedications - Happy Path (Multiple).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetMultipleMedications()
        {
            // Setup
            Mock<IMedicationService> serviceMock = new();
            serviceMock.Setup(s => s.GetMedicationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Dictionary<string, MedicationInformation>());

            List<string> drugIdentifiers = new() { "000001", "000003", "000003" };
            List<string> paddedDinList = drugIdentifiers.Select(x => x.PadLeft(8, '0')).ToList();
            MedicationController controller = new(serviceMock.Object);

            // Act
            RequestResult<IDictionary<string, MedicationInformation>> actual = await controller.GetMedications(drugIdentifiers, default);

            // Verify
            serviceMock.Verify(s => s.GetMedicationsAsync(paddedDinList, default), Times.Once());
            Assert.Equal(0, actual.ResourcePayload?.Count);
        }
    }
}
