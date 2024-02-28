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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Medication.Controllers;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// MedicationStatementController's Unit Tests.
    /// </summary>
    public class MedicationStatementControllerTests
    {
        /// <summary>
        /// GetMedications - Happy Path.
        /// </summary>
        /// <param name="pharmacyAssessmentTitle">Pharmacy assessment title..</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("Pharmacist Assessment")]
        [InlineData("")]
        public async Task GetMedications(string pharmacyAssessmentTitle)
        {
            // Setup
            const string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const string brandName = "KADIAN 10MG CAPSULE";
            const string genericName = "Generic Name";
            bool expectedPharmacistAssessment = !string.IsNullOrEmpty(pharmacyAssessmentTitle);
            string expectedTitle = expectedPharmacistAssessment ? "Pharmacist Assessment" : brandName;
            string expectedSubtitle = expectedPharmacistAssessment ? expectedTitle : genericName;

            RequestResult<IList<MedicationStatement>> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new List<MedicationStatement>
                {
                    new()
                    {
                        PrescriptionIdentifier = "identifier",
                        DispensedDate = DateOnly.Parse("09/28/2020", CultureInfo.CurrentCulture),
                        Directions = "Directions",
                        DispensingPharmacy = new Pharmacy
                        {
                            AddressLine1 = "Line 1",
                            AddressLine2 = "Line 2",
                            CountryCode = "CA",
                            City = "City",
                            PostalCode = "A1A 1A1",
                            Province = "PR",
                            FaxNumber = "1111111111",
                            Name = "Name",
                            PharmacyId = "ID",
                            PhoneNumber = "2222222222",
                        },
                        MedicationSummary = new()
                        {
                            Din = "02242163",
                            BrandName = brandName,
                            Form = "Form",
                            GenericName = genericName,
                            IsPin = false,
                            Manufacturer = "Nomos",
                            Quantity = 1,
                            Strength = "Strong",
                            StrengthUnit = "ml",
                            PharmacyAssessmentTitle = pharmacyAssessmentTitle,
                            PrescriptionProvided = true,
                            RedirectedToHealthCareProvider = true,
                        },
                        PractitionerSurname = "Surname",
                    },
                },
            };

            Mock<IMedicationStatementService> svcMock = new();
            svcMock
                .Setup(s => s.GetMedicationStatementsAsync(hdid, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
            MedicationStatementController controller = new(svcMock.Object);

            // Act
            RequestResult<IList<MedicationStatement>> actual = await controller.GetMedicationStatements(hdid);

            // Verify
            Assert.NotNull(actual);
            expectedResult.ShouldDeepEqual(actual);

            // Medication Summary
            Assert.Equal(pharmacyAssessmentTitle, actual.ResourcePayload![0].MedicationSummary.PharmacyAssessmentTitle);
            Assert.Equal(expectedPharmacistAssessment, actual.ResourcePayload![0].MedicationSummary.IsPharmacistAssessment);
            Assert.Equal(expectedTitle, actual.ResourcePayload![0].MedicationSummary.Title);
            Assert.Equal(expectedSubtitle, actual.ResourcePayload![0].MedicationSummary.Subtitle);
        }
    }
}
