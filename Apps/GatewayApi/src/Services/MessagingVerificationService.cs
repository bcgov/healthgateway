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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;

    /// <inheritdoc/>
    /// <param name="messageVerificationDelegate">The injected message verification delegate.</param>
    /// <param name="userEmailService">The injected user email service.</param>
    /// <param name="userSmsService">The injected user SMS service.</param>
    public class MessagingVerificationService(
        IMessagingVerificationDelegate messageVerificationDelegate,
        IUserEmailServiceV2 userEmailService,
        IUserSmsServiceV2 userSmsService) : IMessagingVerificationService
    {
        /// <inheritdoc/>
        public async Task<MessagingVerification> AddEmailVerificationAsync(string hdid, string email, bool isEmailVerified, bool shouldCommit = true, CancellationToken ct = default)
        {
            MessagingVerification emailVerification = await userEmailService.GenerateMessagingVerificationAsync(hdid, email, Guid.NewGuid(), isEmailVerified, ct);
            await messageVerificationDelegate.InsertAsync(emailVerification, shouldCommit, ct);

            return emailVerification;
        }

        /// <inheritdoc/>
        public async Task<MessagingVerification> AddSmsVerificationAsync(string hdid, string smsNumber, bool shouldCommit = true, CancellationToken ct = default)
        {
            MessagingVerification smsVerification = userSmsService.GenerateMessagingVerification(hdid, smsNumber);
            await messageVerificationDelegate.InsertAsync(smsVerification, shouldCommit, ct);

            return smsVerification;
        }
    }
}
