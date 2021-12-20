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
namespace HealthGateway.Encounter.Delegates
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Encounter.Models.ODR;

    /// <summary>
    /// Interface to retrieve MSP Visit History.
    /// </summary>
    public interface IMSPVisitDelegate
    {
        /// <summary>
        /// Returns a set of MSP Visits.
        /// </summary>
        /// <param name="query">The Encounter statement query execute against the ODR.</param>
        /// <param name="hdid">The HDID of the user querying.</param>
        /// <param name="ipAddress">The IP of the user querying.</param>
        /// <returns>The Encounter Model response wrapped in an HNMessage.</returns>
        Task<RequestResult<MSPVisitHistoryResponse>> GetMSPVisitHistoryAsync(ODRHistoryQuery query, string hdid, string ipAddress);
    }
}
