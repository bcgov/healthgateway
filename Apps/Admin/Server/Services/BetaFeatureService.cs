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
namespace HealthGateway.Admin.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="userProfileDelegate">The user profile delegate to interact with the DB.</param>
    /// <param name="betaFeatureAccessDelegate">The beta feature access delegate to interact with the DB.</param>
    /// <param name="mappingService">The injected mapping service.</param>
    /// <param name="logger">The injected logger.</param>
    public class BetaFeatureService(
        IUserProfileDelegate userProfileDelegate,
        IBetaFeatureAccessDelegate betaFeatureAccessDelegate,
        IAdminServerMappingService mappingService,
        ILogger<BetaFeatureService> logger)
        : IBetaFeatureService
    {
        /// <inheritdoc/>
        public async Task SetUserAccessAsync(UserBetaAccess access, CancellationToken ct = default)
        {
            string email = access.Email;
            ISet<BetaFeature> betaFeatures = access.BetaFeatures;

            logger.LogDebug("Email: {Email} - Beta Features: {Features}", email, betaFeatures);
            IList<UserProfile> userProfiles = await userProfileDelegate.GetUserProfilesAsync(email, true, ct);

            if (userProfiles.Count == 0)
            {
                throw new NotFoundException(ErrorMessages.UserProfileNotFound);
            }

            IEnumerable<string> hdids = userProfiles.Select(x => x.HdId);
            IEnumerable<BetaFeatureAccess> existingAccess = await betaFeatureAccessDelegate.GetAsync(hdids, ct);

            IEnumerable<BetaFeatureAccess> accessToRemove = existingAccess
                .Where(x => !betaFeatures.Contains(mappingService.MapToBetaFeature(x.BetaFeatureCode)))
                .ToList();

            IEnumerable<BetaFeatureAccess> accessToAdd = betaFeatures
                .Where(f => existingAccess.All(a => a.BetaFeatureCode != mappingService.MapToBetaFeature(f)))
                .SelectMany(f => userProfiles.Select(p => mappingService.MapToBetaFeatureAccess(p.HdId, f)))
                .ToList();

            await betaFeatureAccessDelegate.DeleteRangeAsync(accessToRemove, false, ct);
            await betaFeatureAccessDelegate.AddRangeAsync(accessToAdd, true, ct);
        }

        /// <inheritdoc/>
        public async Task<UserBetaAccess> GetUserAccessAsync(string email, CancellationToken ct = default)
        {
            IList<UserProfile> userProfiles = await userProfileDelegate.GetUserProfilesAsync(email, true, ct);

            if (userProfiles.Count == 0)
            {
                throw new NotFoundException(ErrorMessages.UserProfileNotFound);
            }

            IEnumerable<BetaFeatureCode> betaFeatureCodes = userProfiles
                .SelectMany(x => x.BetaFeatureCodes ?? Enumerable.Empty<BetaFeatureCode>())
                .Distinct();

            return new() { Email = userProfiles[0].Email!, BetaFeatures = new HashSet<BetaFeature>(betaFeatureCodes.Select(x => mappingService.MapToBetaFeature(x.Code))) };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserBetaAccess>> GetAllUserAccessAsync(CancellationToken ct = default)
        {
            IList<BetaFeatureAccess> betaFeatures = await betaFeatureAccessDelegate.GetAllAsync(true, ct);

            return betaFeatures
                .GroupBy(x => x.UserProfile.Email)
                .Select(g => mappingService.MapToUserBetaAccess(g.Key!, g.Select(x => x.BetaFeatureCode)))
                .ToList();
        }
    }
}
