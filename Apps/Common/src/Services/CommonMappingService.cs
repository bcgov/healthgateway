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
namespace HealthGateway.Common.Services
{
    using AutoMapper;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Database.Models;

    /// <inheritdoc/>
    public class CommonMappingService(IMapper mapper) : ICommonMappingService
    {
        /// <inheritdoc/>
        public Broadcast MapToBroadcast(BroadcastResponse source)
        {
            return mapper.Map<BroadcastResponse, Broadcast>(source);
        }

        /// <inheritdoc/>
        public BroadcastRequest MapToBroadcastRequest(Broadcast source)
        {
            return mapper.Map<Broadcast, BroadcastRequest>(source);
        }

        /// <inheritdoc/>
        public MessagingVerificationModel MapToMessagingVerificationModel(MessagingVerification source)
        {
            return mapper.Map<MessagingVerification, MessagingVerificationModel>(source);
        }
    }
}
