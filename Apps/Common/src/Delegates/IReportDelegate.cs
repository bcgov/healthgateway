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
namespace HealthGateway.Common.Delegates
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// The report delegate.
    /// </summary>
    public interface IReportDelegate
    {
        /// <summary>
        /// Gets the vaccine status pdf.
        /// </summary>
        /// <param name="vaccineStatus">The vaccine status information.</param>
        /// <param name="address">The optional patient address information.</param>
        /// <returns>Returns the vaccine status pdf document.</returns>
        RequestResult<ReportModel> GetVaccineStatusPDF(VaccineStatus vaccineStatus, Address? address);

        /// <summary>
        /// Gets the vaccine status and record card pdf.
        /// </summary>
        /// <param name="vaccineStatus">The vaccine status information.</param>
        /// <param name="address">The optional patient address information.</param>
        /// <param name="base64RecordCard">The base64 of the record card PDF.</param>
        /// <returns>Returns the report model containing the pdf document.</returns>
        RequestResult<ReportModel> GetVaccineStatusAndRecordPDF(VaccineStatus vaccineStatus, Address? address, string base64RecordCard);

        /// <summary>
        /// Merges two PDFs.
        /// </summary>
        /// <param name="base64Document1">The first document, encoded in base64.</param>
        /// <param name="base64Document2">The second document, encoded in base64.</param>
        /// <param name="fileName">The file name to assign to the merged document.</param>
        /// <returns>Returns the report model containing the merged pdf document.</returns>
        RequestResult<ReportModel> MergePDFs(string base64Document1, string base64Document2, string fileName);
    }
}
