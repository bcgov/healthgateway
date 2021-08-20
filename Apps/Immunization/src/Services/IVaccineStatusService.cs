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
namespace HealthGateway.Immunization.Services
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// The Vaccine Status data service.
    /// </summary>
    public interface IVaccineStatusService
    {
        /// <summary>
        /// Gets the vaccine status for the given patient info.
        /// </summary>
        /// <param name="phn">The patient personal health number.</param>
        /// <param name="dateOfBirth">The patient date of birth in yyyyMMdd format.</param>
        /// <param name="token">The captcha token.</param>
        /// <returns>Returns the vaccine status.</returns>
        Task<RequestResult<VaccineStatus>> GetVaccineStatus(string phn, string dateOfBirth, string token);

        /// <summary>
        /// Gets the vaccine status pdf for the given patient info.
        /// </summary>
        /// <param name="phn">The patient personal health number.</param>
        /// <param name="dateOfBirth">The date of birth in yyyyMMdd format.</param>
        /// <param name="token">The captcha token.</param>
        /// <returns>Returns the vaccine status pdf document.</returns>
        Task<RequestResult<ReportModel>> GetVaccineStatusPDF(string phn, string dateOfBirth, string token);
    }
}
