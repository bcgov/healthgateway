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
namespace HealthGateway.DBMaintainer.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;
    using HealthGateway.Database.Models;
    using HealthGateway.DBMaintainer.Mappers;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Concrete implementation of the <see cref="IPharmaCareDrugParser"/>.
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
        public async Task<IList<PharmaCareDrug>> ParsePharmaCareDrugFileAsync(string filename, FileDownload fileDownload, CancellationToken ct = default)
        {
            this.logger.LogInformation("Parsing PharmaCare Drug file");
            using StreamReader reader = new(filename);
            CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture);
            using CsvReader csv = new(reader, csvConfig);
            TypeConverterOptions options = new()
            {
                Formats = ["yyyyMMdd"],
            };
            csv.Context.TypeConverterOptionsCache.AddOptions<DateOnly>(options);
            PharmaCareDrugMapper mapper = new(fileDownload);
            csv.Context.RegisterClassMap(mapper);
            List<PharmaCareDrug> records = [];
            await foreach (PharmaCareDrug record in csv.GetRecordsAsync<PharmaCareDrug>(ct))
            {
                records.Add(record);
            }

            return records;
        }
    }
}
