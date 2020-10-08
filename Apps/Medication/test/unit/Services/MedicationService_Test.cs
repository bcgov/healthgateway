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
namespace HealthGateway.Medication.Services.Test
{
    using System.Collections.Generic;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using Microsoft.Extensions.Logging;
    using System;
    using DeepEqual.Syntax;
    using System.Linq;

    public class MedicationService_Test
    {
        private readonly IConfiguration configuration;
        public MedicationService_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
        }

        [Fact]
        public void ShouldGetMedications()
        {
            DateTime loadDate = DateTime.Parse("2020/09/29");
            string DIN = "00000000";
            List<DrugProduct> fedData = new List<DrugProduct>()
            {
                new DrugProduct()
                {
                    DrugIdentificationNumber = DIN,
                    UpdatedDateTime = loadDate,
                }
            };

            List<PharmaCareDrug> provData = new List<PharmaCareDrug>()
            {
                new PharmaCareDrug()
                {
                    DINPIN = DIN,
                    UpdatedDateTime = loadDate,
                },
            };

            Dictionary<string, Models.MedicationResult> expected = new Dictionary<string, Models.MedicationResult>()
            {
                {
                    DIN,
                    new Models.MedicationResult()
                    {
                        DIN = DIN,
                        FederalData = new FederalDrugSource()
                        {
                            UpdateDateTime = loadDate,
                            DrugProduct = fedData.First(),
                        },
                        ProvincialData = new ProvincialDrugSource()
                        {
                            UpdateDateTime = loadDate,
                            PharmaCareDrug = provData.First(),
                        }
                    }
                }
            };

            Mock<IDrugLookupDelegate> mockDelegate = new Mock<IDrugLookupDelegate>();
            mockDelegate.Setup(s => s.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(fedData);
            mockDelegate.Setup(s => s.GetPharmaCareDrugsByDIN(It.IsAny<List<string>>())).Returns(provData);

            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<RestMedicationService> logger = loggerFactory.CreateLogger<RestMedicationService>();

            IMedicationService service = new RestMedicationService(logger, mockDelegate.Object);
            var actual = service.GetMedications(new List<string>());
            Assert.True(actual.IsDeepEqual(expected));
        }

        [Fact]
        public void ShouldGetMedicationsNoFederal()
        {
            DateTime loadDate = DateTime.Parse("2020/09/29");
            string DIN = "00000000";
            List<DrugProduct> fedData = new List<DrugProduct>()
            {
            };

            List<PharmaCareDrug> provData = new List<PharmaCareDrug>()
            {
                new PharmaCareDrug()
                {
                    DINPIN = DIN,
                    UpdatedDateTime = loadDate,
                },
            };

            Dictionary<string, Models.MedicationResult> expected = new Dictionary<string, Models.MedicationResult>()
            {
                {
                    DIN,
                    new Models.MedicationResult()
                    {
                        DIN = DIN,

                        ProvincialData = new ProvincialDrugSource()
                        {
                            UpdateDateTime = loadDate,
                            PharmaCareDrug = provData.First(),
                        }
                    }
                }
            };

            Mock<IDrugLookupDelegate> mockDelegate = new Mock<IDrugLookupDelegate>();
            mockDelegate.Setup(s => s.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(fedData);
            mockDelegate.Setup(s => s.GetPharmaCareDrugsByDIN(It.IsAny<List<string>>())).Returns(provData);

            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<RestMedicationService> logger = loggerFactory.CreateLogger<RestMedicationService>();

            IMedicationService service = new RestMedicationService(logger, mockDelegate.Object);
            var actual = service.GetMedications(new List<string>());
            Assert.True(actual.IsDeepEqual(expected));
        }
    }
}
