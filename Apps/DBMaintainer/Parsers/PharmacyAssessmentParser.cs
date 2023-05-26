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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using CsvHelper;
    using CsvHelper.Configuration;
    using HealthGateway.Database.Models;
    using HealthGateway.DBMaintainer.Mappers;
    using HealthGateway.DBMaintainer.Mappers.Converters;
    using HealthGateway.DBMaintainer.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Concrete implementation of the <see cref="IPharmacyAssessmentParser"/>.
    /// </summary>
    public class PharmacyAssessmentParser : IPharmacyAssessmentParser
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PharmacyAssessmentParser"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public PharmacyAssessmentParser(ILogger<PharmacyAssessmentParser> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public IEnumerable<PharmacyAssessment> ParsePharmacyAssessmentFile(string filename, FileDownload fileDownload)
        {
            this.logger.LogInformation("Parsing Pharmacy Assessment file");
            using StreamReader reader = new(filename);
            CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture);
            using CsvReader csv = new(reader, csvConfig);
            csv.Context.TypeConverterCache.AddConverter<bool>(new BooleanConverter());
            PharmacyAssessmentMapper mapper = new(fileDownload);
            csv.Context.RegisterClassMap(mapper);
            IEnumerable<PharmacyAssessment> records = csv.GetRecords<PharmacyAssessment>().ToList();
            return records;
        }
    }
}
