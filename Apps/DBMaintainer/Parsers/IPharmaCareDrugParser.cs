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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Interface to parse the BC Pharmacare drug file.
    /// </summary>
    public interface IPharmaCareDrugParser
    {
        /// <summary>
        /// Parses the drug file.
        /// </summary>
        /// <param name="filename">The file to parse.</param>
        /// <param name="fileDownload">The related download file to associate with.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>a list of PharmaCare drugs.</returns>
        Task<IList<PharmaCareDrug>> ParsePharmaCareDrugFileAsync(string filename, FileDownload fileDownload, CancellationToken ct = default);
    }
}
