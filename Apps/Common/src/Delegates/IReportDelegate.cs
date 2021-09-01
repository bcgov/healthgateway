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
    }
}
