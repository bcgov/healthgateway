// //-------------------------------------------------------------------------
// // Copyright © 2019 Province of British Columbia
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //-------------------------------------------------------------------------
namespace HealthGateway.DrugMaintainer
{
    using System.Collections.Generic;
    using HealthGateway.DIN.Models;
    using CsvHelper;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    public class FederalDrugProductParser : IDrugProductParser
    {
        private readonly ILogger logger;

        public FederalDrugProductParser(ILogger<FederalDrugProductParser> logger)
        {
            this.logger = logger;
        }

        public List<DrugProduct> ParseDrugFile(string filePath)
        {
            this.logger.LogInformation("Parsing Drug file");
            using (var reader = new StreamReader(Path.Combine(filePath, "drug.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    csv.Configuration.RegisterClassMap<DrugProductMapper>();
                    List<DrugProduct> records = csv.GetRecords<DrugProduct>().ToList();
                    return records;
                }
            }
        }

        public List<ActiveIngredient> ParseActiveIngredientFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Ingredients file");
            using (var reader = new StreamReader(Path.Combine(filePath, "ingred.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    ActiveIngredientMapper mapper = new ActiveIngredientMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<ActiveIngredient> records = csv.GetRecords<ActiveIngredient>().ToList();
                    return records;
                }
            }
        }

        public List<Company> ParseCompanyFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Company file");
            using (var reader = new StreamReader(Path.Combine(filePath, "comp.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    CompanyMapper mapper = new CompanyMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<Company> records = csv.GetRecords<Company>().ToList();
                    return records;
                }
            }
        }

        public List<Status> ParseStatusFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Status file");
            using (var reader = new StreamReader(Path.Combine(filePath, "status.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    StatusMapper mapper = new StatusMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<Status> records = csv.GetRecords<Status>().ToList();
                    return records;
                }
            }
        }

        public List<Form> ParseFormFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Form file");
            using (var reader = new StreamReader(Path.Combine(filePath, "form.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    FormMapper mapper = new FormMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<Form> records = csv.GetRecords<Form>().ToList();
                    return records;
                }
            }
        }

        public List<Packaging> ParsePackagingFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Package file");
            using (var reader = new StreamReader(Path.Combine(filePath, "package.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    PackagingMapper mapper = new PackagingMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<Packaging> records = csv.GetRecords<Packaging>().ToList();
                    return records;
                }
            }
        }

        public List<PharmaceuticalStd> ParsePharmaceuticalStdFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Pharmaceutical file");
            using (var reader = new StreamReader(Path.Combine(filePath, "pharm.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    PharmaceuticalStdMapper mapper = new PharmaceuticalStdMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<PharmaceuticalStd> records = csv.GetRecords<PharmaceuticalStd>().ToList();
                    return records;
                }
            }
        }

        public List<Route> ParseRouteFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Route file");
            using (var reader = new StreamReader(Path.Combine(filePath, "route.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    RouteMapper mapper = new RouteMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<Route> records = csv.GetRecords<Route>().ToList();
                    return records;
                }
            }
        }

        public List<Schedule> ParseScheduleFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Schedule file");
            using (var reader = new StreamReader(Path.Combine(filePath, "schedule.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    ScheduleMapper mapper = new ScheduleMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<Schedule> records = csv.GetRecords<Schedule>().ToList();
                    return records;
                }
            }
        }

        public List<TherapeuticClass> ParseTherapeuticFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Therapeutical file");
            using (var reader = new StreamReader(Path.Combine(filePath, "ther.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    TherapeuticMapper mapper = new TherapeuticMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<TherapeuticClass> records = csv.GetRecords<TherapeuticClass>().ToList();
                    return records;
                }
            }
        }

        public List<VeterinarySpecies> ParseVeterinarySpeciesFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Veterinary file");
            using (var reader = new StreamReader(Path.Combine(filePath, "vet.txt")))
            {
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    VeterinarySpeciesMapper mapper = new VeterinarySpeciesMapper(drugProducts);
                    csv.Configuration.RegisterClassMap(mapper);
                    List<VeterinarySpecies> records = csv.GetRecords<VeterinarySpecies>().ToList();
                    return records;
                }
            }
        }
    }
}