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

namespace HealthGateway.Admin.Client.Services
{
    using AutoMapper;
    using HealthGateway.Admin.Client.Models;
    using HealthGateway.Admin.Common.Models;

    /// <inheritdoc/>
    public class AdminClientMappingService(IMapper mapper) : IAdminClientMappingService
    {
        /// <inheritdoc/>
        public ExtendedDelegateInfo MapToExtendedDelegateInfo(DelegateInfo source)
        {
            return mapper.Map<DelegateInfo, ExtendedDelegateInfo>(source);
        }

        /// <inheritdoc/>
        public ExtendedUserFeedbackView MapToExtendedUserFeedbackView(UserFeedbackView source)
        {
            return mapper.Map<UserFeedbackView, ExtendedUserFeedbackView>(source);
        }
    }
}
