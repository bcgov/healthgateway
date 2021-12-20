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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;

    /// <summary>
    /// The CDogs report service.
    /// </summary>
    public interface ICDogsDelegate
    {
        /// <summary>
        /// Generates a report based on the request model provided.
        /// </summary>
        /// <param name="request">The cdogs request model.</param>
        /// <returns>The report data.</returns>
        Task<RequestResult<ReportModel>> GenerateReportAsync(CDogsRequestModel request);
    }
}
