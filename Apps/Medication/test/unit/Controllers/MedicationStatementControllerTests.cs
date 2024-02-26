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

            // Medication Statement
            Assert.Equal(expectedResult.ResourcePayload[0].PrescriptionIdentifier, actual.ResourcePayload![0].PrescriptionIdentifier);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensedDate, actual.ResourcePayload![0].DispensedDate);
            Assert.Equal(expectedResult.ResourcePayload[0].Directions, actual.ResourcePayload![0].Directions);
            Assert.Equal(expectedResult.ResourcePayload[0].PractitionerSurname, actual.ResourcePayload![0].PractitionerSurname);

            // Dispensing Pharmacy
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.AddressLine1, actual.ResourcePayload![0].DispensingPharmacy.AddressLine1);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.AddressLine2, actual.ResourcePayload![0].DispensingPharmacy.AddressLine2);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.City, actual.ResourcePayload![0].DispensingPharmacy.City);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.CountryCode, actual.ResourcePayload![0].DispensingPharmacy.CountryCode);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.FaxNumber, actual.ResourcePayload![0].DispensingPharmacy.FaxNumber);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.Name, actual.ResourcePayload![0].DispensingPharmacy.Name);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.PharmacyId, actual.ResourcePayload![0].DispensingPharmacy.PharmacyId);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.PhoneNumber, actual.ResourcePayload![0].DispensingPharmacy.PhoneNumber);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.PostalCode, actual.ResourcePayload![0].DispensingPharmacy.PostalCode);
            Assert.Equal(expectedResult.ResourcePayload[0].DispensingPharmacy.Province, actual.ResourcePayload![0].DispensingPharmacy.Province);

            // Medication Summary
            Assert.False(actual.ResourcePayload![0].MedicationSummary.IsPin);
            Assert.True(actual.ResourcePayload![0].MedicationSummary.PrescriptionProvided);
            Assert.True(actual.ResourcePayload![0].MedicationSummary.RedirectedToHealthCareProvider);

            Assert.Equal(pharmacyAssessmentTitle, actual.ResourcePayload![0].MedicationSummary.PharmacyAssessmentTitle);
            Assert.Equal(expectedPharmacistAssessment, actual.ResourcePayload![0].MedicationSummary.IsPharmacistAssessment);
            Assert.Equal(expectedTitle, actual.ResourcePayload![0].MedicationSummary.Title);
            Assert.Equal(expectedSubtitle, actual.ResourcePayload![0].MedicationSummary.Subtitle);

            Assert.Equal(expectedResult.ResourcePayload[0].MedicationSummary.Form, actual.ResourcePayload![0].MedicationSummary.Form);
            Assert.Equal(expectedResult.ResourcePayload[0].MedicationSummary.Manufacturer, actual.ResourcePayload![0].MedicationSummary.Manufacturer);
            Assert.Equal(expectedResult.ResourcePayload![0].MedicationSummary.Quantity, actual.ResourcePayload![0].MedicationSummary.Quantity);
            Assert.Equal(expectedResult.ResourcePayload![0].MedicationSummary.Strength, actual.ResourcePayload![0].MedicationSummary.Strength);
            Assert.Equal(expectedResult.ResourcePayload![0].MedicationSummary.StrengthUnit, actual.ResourcePayload![0].MedicationSummary.StrengthUnit);
        }
    }
}
