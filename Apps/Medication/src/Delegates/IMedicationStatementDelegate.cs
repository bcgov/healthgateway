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
namespace HealthGateway.Medication.Delegates
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Medication.Models.ODR;

    /// <summary>
    /// Interface to retrieve medication statements.
    /// </summary>
    public interface IMedicationStatementDelegate
    {
        /// <summary>
        /// Gets medication statements.
        /// </summary>
        /// <param name="query">The medication statement query to execute against the ODR.</param>
        /// <param name="protectiveWord">The protective word to validate.</param>
        /// <param name="hdid">The HDID of the user querying.</param>
        /// <param name="ipAddress">The IP of the user querying.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The medication history response wrapped in a RequestResult.</returns>
        Task<RequestResult<MedicationHistoryResponse>> GetMedicationStatementsAsync(OdrHistoryQuery query, string? protectiveWord, string hdid, string ipAddress, CancellationToken ct = default);
    }
}
