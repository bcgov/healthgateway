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
namespace HealthGateway.GatewayApi.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using CommunicationType = HealthGateway.GatewayApi.Constants.CommunicationType;

    /// <inheritdoc/>
    /// <param name="communicationService">
    /// The communication service to interact with the communication delegate to access the
    /// DB.
    /// </param>
    /// <param name="mappingService">The injected mapping service.</param>
    public class GatewayCommunicationService(ICommunicationService communicationService, IGatewayApiMappingService mappingService) : IGatewayCommunicationService
    {
        /// <inheritdoc/>
        public async Task<RequestResult<CommunicationModel?>> GetActiveCommunicationAsync(CommunicationType communicationType, CancellationToken ct = default)
        {
            RequestResult<Communication?> communication = await communicationService.GetActiveCommunicationAsync(mappingService.MapToCommunicationType(communicationType), ct);
            return mappingService.MapToRequestResult(communication);
        }
    }
}
