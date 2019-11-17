﻿// -------------------------------------------------------------------------
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
namespace HealthGateway.WebClient.Services
{
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <inheritdoc />
    public class UserProfileService : IUserProfileService
    {
        private readonly IProfileDelegate profileDelegate;
        private readonly IEmailQueueService emailQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="profileDelegate">The profile delegate to interact with the DB.</param>
        public UserProfileService(IProfileDelegate profileDelegate, IEmailQueueService emailQueueService)
        {
            this.profileDelegate = profileDelegate;
            this.emailQueueService = emailQueueService;
        }

        /// <inheritdoc />
        public DBResult<UserProfile> GetUserProfile(string hdid)
        {
            return this.profileDelegate.GetUserProfile(hdid);
        }

        /// <inheritdoc />
        public DBResult<UserProfile> CreateUserProfile(UserProfile userProfile)
        {
            Email email = new Email();
            email.From = "stephen@lawsfamily.ca";
            email.To = "stephen.s.laws@gov.bc.ca";
            email.Subject = "Testing Hangfire Email Job";
            email.Body = "Testing";
            emailQueueService.QueueEmail(email);
            return null;
           //return this.profileDelegate.CreateUserProfile(userProfile);
        }
    }
}
