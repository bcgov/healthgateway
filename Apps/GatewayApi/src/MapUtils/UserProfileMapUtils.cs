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

namespace HealthGateway.GatewayApi.MapUtils
{
    using System;
    using AutoMapper;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Static Helper classes for conversion of model objects.
    /// </summary>
    public static class UserProfileMapUtils
    {
        /// <summary>
        /// Helper Method to convert from UserProfile to UserProfileModel.
        /// </summary>
        /// <param name="userProfile">The user Profile to convert.</param>
        /// <param name="latestTermsOfServiceId">The latest terms of service id used to check if terms have updated.</param>
        /// <param name="mapper">The AutoMapper IMapper.</param>
        /// <returns>The UserProfileModel.</returns>
        public static UserProfileModel CreateFromDbModel(UserProfile userProfile, Guid? latestTermsOfServiceId, IMapper mapper)
        {
            UserProfileModel userProfileModel = mapper.Map<UserProfile, UserProfileModel>(
                userProfile,
                opts =>
                    opts.AfterMap(
                        (src, dest) =>
                            dest.HasTermsOfServiceUpdated = src.TermsOfServiceId != latestTermsOfServiceId));
            return userProfileModel;
        }
    }
}
