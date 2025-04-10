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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class DataAccessService(
        IBlockedAccessDelegate blockedAccessDelegate,
        IDelegationDelegate delegationDelegate,
        IMessagingVerificationDelegate messageVerificationDelegate,
        IUserProfileDelegate userProfileDelegate,
        ILogger<DataAccessService> logger) : IDataAccessService
    {
        /// <inheritdoc/>
        public async Task<BlockedDatasets> GetBlockedDatasetsAsync(string hdid, CancellationToken ct = default)
        {
            return new()
            {
                Hdid = hdid,
                DataSources = await blockedAccessDelegate.GetDataSourcesAsync(hdid, ct),
            };
        }

        /// <inheritdoc/>
        public async Task<ContactInfo> GetContactInfoAsync(string hdid, CancellationToken ct = default)
        {
            UserProfile userProfile = await userProfileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException(ErrorMessages.UserProfileNotFound);

            string? emailAddress = !string.IsNullOrEmpty(userProfile.Email)
                ? userProfile.Email
                : (await messageVerificationDelegate.GetLastForUserAsync(userProfile.HdId, MessagingVerificationType.Email, ct))?.Email?.To;

            string? smsNumber = !string.IsNullOrEmpty(userProfile.SmsNumber)
                ? userProfile.SmsNumber
                : (await messageVerificationDelegate.GetLastForUserAsync(userProfile.HdId, MessagingVerificationType.Sms, ct))?.SmsNumber;

            return new()
            {
                Hdid = hdid,
                Email = emailAddress,
                Phone = smsNumber,
            };
        }

        /// <inheritdoc/>
        public async Task<UserProtection> GetUserProtectionAsync(string hdid, string delegateHdid, CancellationToken ct = default)
        {
            UserProtection userProtection = new()
            {
                DelegateHdid = delegateHdid,
                ProtectedSubject = new(hdid),
            };

            Dependent? dependent = await delegationDelegate.GetDependentAsync(hdid, true, ct);
            if (dependent is { Protected: true } && dependent.AllowedDelegations.All(d => d.DelegateHdId == delegateHdid))
            {
                userProtection.ProtectedSubject = new(hdid, true);
            }

            bool isProtected = userProtection.ProtectedSubject.Protected;
            logger.LogDebug("Subject: {SubjectHdid} isProtected: {IsProtected} by Delegate: {DelegateHdid}", userProtection.ProtectedSubject.SubjectHdid, isProtected, delegateHdid);

            return userProtection;
        }
    }
}
