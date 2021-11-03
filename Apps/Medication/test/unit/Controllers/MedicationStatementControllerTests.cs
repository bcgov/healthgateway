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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    using DeepEqual.Syntax;

    using HealthGateway.Common.Models;
    using HealthGateway.Medication.Controllers;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;

    using Microsoft.AspNetCore.Mvc;

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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetMedications()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            RequestResult<IList<MedicationStatementHistory>> expectedResult = new RequestResult<IList<MedicationStatementHistory>>()
            {
                ResultStatus = HealthGateway.Common.Constants.ResultType.Success,
                ResourcePayload = new List<MedicationStatementHistory>()
                    {
                        new MedicationStatementHistory()
                        {
                            PrescriptionIdentifier = "identifier",
                            PrescriptionStatus = 'M',
                            DispensedDate = DateTime.Parse("09/28/2020", CultureInfo.CurrentCulture),
                            DateEntered = DateTime.Parse("09/28/2020", CultureInfo.CurrentCulture),
                            Directions = "Directions",
                            DispensingPharmacy = new Pharmacy()
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
                            MedicationSummary = new MedicationSummary()
                            {
                                DIN = "02242163",
                                BrandName = "KADIAN 10MG CAPSULE",
                                DrugDiscontinuedDate = DateTime.Parse("09/28/2020", CultureInfo.CurrentCulture),
                                Form = "Form",
                                GenericName = "Generic Name",
                                IsPin = false,
                                Manufacturer = "Nomos",
                                MaxDailyDosage = 100,
                                Quantity = 1,
                                Strength = "Strong",
                                StrengthUnit = "ml",
                            },
                            PharmacyId = "Id",
                            PractitionerSurname = "Surname",
                        },
                    },
            };

            Mock<IMedicationStatementService> svcMock = new Mock<IMedicationStatementService>();
            svcMock
                .Setup(s => s.GetMedicationStatementsHistory(hdid, null))
                .ReturnsAsync(expectedResult);
            MedicationStatementController controller = new MedicationStatementController(svcMock.Object);

            // Act
            IActionResult actual = await controller.GetMedicationStatements(hdid).ConfigureAwait(true);

            // Verify
            Assert.IsType<JsonResult>(actual);
            JsonResult? jsonResult = actual as JsonResult;
            RequestResult<IList<MedicationStatementHistory>>? requestResult = jsonResult?.Value as RequestResult<IList<MedicationStatementHistory>>;
            Assert.True(requestResult != null && requestResult.IsDeepEqual(expectedResult));
        }
    }
}
