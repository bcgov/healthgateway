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
namespace HealthGateway.DrugMaintainer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Concrete implemention of the <see cref="IDrugProductParser"/>.
    /// </summary>
    public class FederalDrugProductParser : IDrugProductParser
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FederalDrugProductParser"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public FederalDrugProductParser(ILogger<FederalDrugProductParser> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public IList<DrugProduct> ParseDrugFile(string sourceFolder, FileDownload fileDownload)
        {
            this.logger.LogInformation("Parsing Drug file");
            using var reader = new StreamReader(GetFileMatching(sourceFolder, "drug*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            DrugProductMapper mapper = new DrugProductMapper(fileDownload);
            csv.Context.RegisterClassMap(mapper);
            List<DrugProduct> records = csv.GetRecords<DrugProduct>().ToList();

            return records;
        }

        /// <inheritdoc/>
        public IList<ActiveIngredient> ParseActiveIngredientFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Ingredients file");
            using var reader = new StreamReader(GetFileMatching(filePath, "ingred*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            ActiveIngredientMapper mapper = new ActiveIngredientMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<ActiveIngredient> records = csv.GetRecords<ActiveIngredient>().ToList();
            return records;
        }

        /// <inheritdoc/>
        public IList<Company> ParseCompanyFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Company file");
            using var reader = new StreamReader(GetFileMatching(filePath, "comp*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            CompanyMapper mapper = new CompanyMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<Company> records = csv.GetRecords<Company>().ToList();
            return records;
        }

        /// <inheritdoc/>
        public IList<Status> ParseStatusFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Status file");
            using var reader = new StreamReader(GetFileMatching(filePath, "status*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            StatusMapper mapper = new StatusMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<Status> records = csv.GetRecords<Status>().ToList();
            return records;
        }

        /// <inheritdoc/>
        public IList<Form> ParseFormFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Form file");
            using var reader = new StreamReader(GetFileMatching(filePath, "form*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            FormMapper mapper = new FormMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<Form> records = csv.GetRecords<Form>().ToList();
            return records;
        }

        /// <inheritdoc/>
        public IList<Packaging> ParsePackagingFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Package file");
            using var reader = new StreamReader(GetFileMatching(filePath, "package*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            PackagingMapper mapper = new PackagingMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<Packaging> records = csv.GetRecords<Packaging>().ToList();
            return records;
        }

        /// <inheritdoc/>
        public IList<PharmaceuticalStd> ParsePharmaceuticalStdFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Pharmaceutical file");
            using var reader = new StreamReader(GetFileMatching(filePath, "pharm*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            PharmaceuticalStdMapper mapper = new PharmaceuticalStdMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<PharmaceuticalStd> records = csv.GetRecords<PharmaceuticalStd>().ToList();
            return records;
        }

        /// <inheritdoc/>
        public IList<Route> ParseRouteFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Route file");
            using var reader = new StreamReader(GetFileMatching(filePath, "route*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            RouteMapper mapper = new RouteMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<Route> records = csv.GetRecords<Route>().ToList();
            return records;
        }

        /// <inheritdoc/>
        public IList<Schedule> ParseScheduleFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Schedule file");
            using var reader = new StreamReader(GetFileMatching(filePath, "schedule*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            ScheduleMapper mapper = new ScheduleMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<Schedule> records = csv.GetRecords<Schedule>().ToList();
            return records;
        }

        /// <inheritdoc/>
        public IList<TherapeuticClass> ParseTherapeuticFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Therapeutical file");
            using var reader = new StreamReader(GetFileMatching(filePath, "ther*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            TherapeuticMapper mapper = new TherapeuticMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<TherapeuticClass> records = csv.GetRecords<TherapeuticClass>().ToList();
            return records;
        }

        /// <inheritdoc/>
        public IList<VeterinarySpecies> ParseVeterinarySpeciesFile(string filePath, IEnumerable<DrugProduct> drugProducts)
        {
            this.logger.LogInformation("Parsing Veterinary file");
            using var reader = new StreamReader(GetFileMatching(filePath, "vet*.txt"));
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using var csv = new CsvReader(reader, csvConfig);
            VeterinarySpeciesMapper mapper = new VeterinarySpeciesMapper(drugProducts);
            csv.Context.RegisterClassMap(mapper);
            List<VeterinarySpecies> records = csv.GetRecords<VeterinarySpecies>().ToList();
            return records;
        }

        /// <summary>
        /// Searchs teh SourceFolder and returns a single file matching the pattern.
        /// </summary>
        /// <param name="sourceFolder">The source folder to search.</param>
        /// <param name="fileMatch">The file pattern to match.</param>
        /// <returns>The filename of the file matching.</returns>
        private static string GetFileMatching(string sourceFolder, string fileMatch)
        {
            string[] files = Directory.GetFiles(sourceFolder, fileMatch);
            if (files.Length > 1 || files.Length == 0)
            {
                throw new FormatException($"The zip file contained {files.Length} CSV files, very confused.");
            }

            return files[0];
        }
    }
}
