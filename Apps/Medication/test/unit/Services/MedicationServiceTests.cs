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
namespace HealthGateway.MedicationTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using DeepEqual.Syntax;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// MedicationService's Unit Tests.
    /// </summary>
    public class MedicationServiceTests
    {
        /// <summary>
        /// GetMedications - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetMedications()
        {
            DateTime loadDate = DateTime.Parse("2020/09/29", CultureInfo.CurrentCulture);
            string din = "00000000";
            List<DrugProduct> fedData = new()
            {
                new DrugProduct
                {
                    DrugIdentificationNumber = din,
                    UpdatedDateTime = loadDate,
                },
            };

            List<PharmaCareDrug> provData = new()
            {
                new PharmaCareDrug
                {
                    DinPin = din,
                    UpdatedDateTime = loadDate,
                },
            };

            Dictionary<string, MedicationInformation> expected = new()
            {
                {
                    din,
                    new MedicationInformation
                    {
                        Din = din,
                        FederalData = new FederalDrugSource
                        {
                            UpdateDateTime = loadDate,
                            DrugProduct = fedData.First(),
                        },
                        ProvincialData = new ProvincialDrugSource
                        {
                            UpdateDateTime = loadDate,
                            PharmaCareDrug = provData.First(),
                        },
                    }
                },
            };

            Mock<IDrugLookupDelegate> mockDelegate = new();
            mockDelegate.Setup(s => s.GetDrugProductsByDin(It.IsAny<List<string>>())).Returns(fedData);
            mockDelegate.Setup(s => s.GetPharmaCareDrugsByDin(It.IsAny<List<string>>())).Returns(provData);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<RestMedicationService> logger = loggerFactory.CreateLogger<RestMedicationService>();

            IMedicationService service = new RestMedicationService(logger, mockDelegate.Object);
            IDictionary<string, MedicationInformation> actual = service.GetMedications(new List<string>());
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetMedications - Happy Path (No Federal).
        /// </summary>
        [Fact]
        public void ShouldGetMedicationsNoFederal()
        {
            DateTime loadDate = DateTime.Parse("2020/09/29", CultureInfo.CurrentCulture);
            string din = "00000000";
            List<DrugProduct> fedData = new();

            List<PharmaCareDrug> provData = new()
            {
                new PharmaCareDrug
                {
                    DinPin = din,
                    UpdatedDateTime = loadDate,
                },
            };

            Dictionary<string, MedicationInformation> expected = new()
            {
                {
                    din,
                    new MedicationInformation
                    {
                        Din = din,

                        ProvincialData = new ProvincialDrugSource
                        {
                            UpdateDateTime = loadDate,
                            PharmaCareDrug = provData.First(),
                        },
                    }
                },
            };

            Mock<IDrugLookupDelegate> mockDelegate = new();
            mockDelegate.Setup(s => s.GetDrugProductsByDin(It.IsAny<List<string>>())).Returns(fedData);
            mockDelegate.Setup(s => s.GetPharmaCareDrugsByDin(It.IsAny<List<string>>())).Returns(provData);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<RestMedicationService> logger = loggerFactory.CreateLogger<RestMedicationService>();

            IMedicationService service = new RestMedicationService(logger, mockDelegate.Object);
            IDictionary<string, MedicationInformation> actual = service.GetMedications(new List<string>());
            actual.ShouldDeepEqual(expected);
        }
    }
}
