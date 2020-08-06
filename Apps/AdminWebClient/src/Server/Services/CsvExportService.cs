// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using ServiceStack.Text;

    /// <inheritdoc />
    public class CsvExportService : ICsvExportService
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvExportService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        public CsvExportService(ILogger<CsvExportService> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public Stream GetComments(DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Stream GetNotes(DateTime? startDate, DateTime? endDate)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Stream GetUserProfiles(DateTime? startDate, DateTime? endDate)
        {
            using MemoryStream retStream = new MemoryStream();
            using StreamWriter writer = new StreamWriter(retStream);
            writer.Write("a,b,c");
            writer.Flush();
            return retStream;
        }
    }
}
