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
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Concrete implemention of the <see cref="IPharmaCareDrugParser"/>.
    /// </summary>
    public class PharmaCareDrugParser : IPharmaCareDrugParser
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PharmaCareDrugParser"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public PharmaCareDrugParser(ILogger<PharmaCareDrugParser> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public IList<PharmaCareDrug> ParsePharmaCareDrugFile(string filename, FileDownload filedownload)
        {
            this.logger.LogInformation("Parsing PharmaCare Drug file");
            using var reader = new StreamReader(filename);
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
            using var csv = new CsvReader(reader, csvConfig);
            csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = new[] { "yyyyMMdd" };
            PharmaCareDrugMapper mapper = new PharmaCareDrugMapper(filedownload);
            csv.Context.RegisterClassMap(mapper);
            List<PharmaCareDrug> records = csv.GetRecords<PharmaCareDrug>().ToList();
            foreach (PharmaCareDrug drug in records)
            {
                drug.EffectiveDate = drug.EffectiveDate.ToUniversalTime();
                drug.EndDate = drug.EndDate.ToUniversalTime();
                drug.FormularyListDate = drug.FormularyListDate.ToUniversalTime();
            }

            return records;
        }
    }
}
