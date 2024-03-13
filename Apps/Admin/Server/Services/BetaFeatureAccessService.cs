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
    public class BetaFeatureAccessService(
        IUserProfileDelegate userProfileDelegate,
        IBetaFeatureAccessDelegate betaFeatureAccessDelegate,
        IAdminServerMappingService mappingService,
        ILogger<BetaFeatureAccessService> logger)
        : IBetaFeatureAccessService
    {
        /// <inheritdoc/>
        public async Task SetUserAccessAsync(string email, IList<BetaFeature> betaFeatures, CancellationToken ct = default)
        {
            logger.LogDebug("Email: {Email} - Beta Features: {Features}", email, betaFeatures);
            IList<UserProfile> userProfiles = await userProfileDelegate.GetUserProfileAsync(email, true, ct);

            if (userProfiles.Count == 0)
            {
                throw new NotFoundException(ErrorMessages.UserProfileNotFound);
            }

            IEnumerable<string> hdids = userProfiles.Select(x => x.HdId);
            IEnumerable<BetaFeatureAccess> existingBetaFeatures = await betaFeatureAccessDelegate.GetAsync(hdids, ct);

            IEnumerable<BetaFeatureAccess> betaFeaturesToDelete = existingBetaFeatures
                .Where(x => !betaFeatures.Contains(mappingService.MapToBetaFeature(x.BetaFeatureCode)));

            IEnumerable<BetaFeatureAccess> betaFeaturesToAdd = betaFeatures
                .Where(betaFeature => existingBetaFeatures.All(betaFeatureAccess => betaFeatureAccess.BetaFeatureCode != mappingService.MapToBetaFeature(betaFeature)))
                .SelectMany(betaFeature => userProfiles.Select(profile => mappingService.MapToBetaFeatureAccess(profile.HdId, betaFeature)));

            await betaFeatureAccessDelegate.DeleteRangeAsync(betaFeaturesToDelete, false, ct);
            await betaFeatureAccessDelegate.AddRangeAsync(betaFeaturesToAdd, ct: ct);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BetaFeature>> GetUserAccessAsync(string email, CancellationToken ct = default)
        {
            IList<UserProfile> userProfiles = await userProfileDelegate.GetUserProfileAsync(email, true, ct);

            if (userProfiles.Count == 0)
            {
                throw new NotFoundException(ErrorMessages.UserProfileNotFound);
            }

            ICollection<BetaFeatureCode> betaFeatureCodes = userProfiles.First().BetaFeatureCodes;
            return betaFeatureCodes.Select(x => mappingService.MapToBetaFeature(x.Code));
        }
    }
}
