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

namespace HHealthGateway.Admin.Server.MapUtils
{
    using AutoMapper;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Static Helper classes for conversion of model objects.
    /// </summary>
    public static class UserFeedbackMapUtils
    {
        /// <summary>
        /// Creates a UI model from a DB model.
        /// </summary>
        /// <param name="userFeedback">The DB model to convert.</param>
        /// <param name="email">The email address associated with the person who created the feedback.</param>
        /// <returns>The created UI model.</returns>
        /// <param name="mapper">The AutoMapper IMapper.</param>
        /// <returns>The UserProfileModel.</returns>
        public static UserFeedbackView ToUiModel(UserFeedback userFeedback, string email, IMapper mapper)
        {
            UserFeedbackView userFeedbackView = mapper.Map<UserFeedback, UserFeedbackView>(
                userFeedback,
                opts =>
                    opts.AfterMap(
                        (src, dest) =>
                            dest.Email = email));
            return userFeedbackView;
        }
    }
}
