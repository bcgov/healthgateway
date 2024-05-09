//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.GatewayApi.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="userPreferenceDelegate">The preference delegate to interact with the DB.</param>
    /// <param name="mappingService">The injected automapper provider.</param>
    /// <param name="logger">The injected logger provider.</param>
    public class UserPreferenceServiceV2(IUserPreferenceDelegate userPreferenceDelegate, IGatewayApiMappingService mappingService, ILogger<UserPreferenceServiceV2> logger) : IUserPreferenceServiceV2
    {
        /// <inheritdoc/>
        public async Task<RequestResult<UserPreferenceModel>> UpdateUserPreferenceAsync(UserPreferenceModel userPreferenceModel, CancellationToken ct = default)
        {
            logger.LogTrace("Updating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);
            UserPreference userPreference = mappingService.MapToUserPreference(userPreferenceModel);
            DbResult<UserPreference> dbResult = await userPreferenceDelegate.UpdateUserPreferenceAsync(userPreference, ct: ct);

            return dbResult.Status != DbStatusCode.Updated
                ? RequestResultFactory.ServiceError<UserPreferenceModel>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message)
                : RequestResultFactory.Success(mappingService.MapToUserPreferenceModel(dbResult.Payload));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserPreferenceModel>> CreateUserPreferenceAsync(UserPreferenceModel userPreferenceModel, CancellationToken ct = default)
        {
            logger.LogTrace("Creating user preference... {Preference} for {Hdid}", userPreferenceModel.Preference, userPreferenceModel.HdId);
            UserPreference userPreference = mappingService.MapToUserPreference(userPreferenceModel);
            DbResult<UserPreference> dbResult = await userPreferenceDelegate.CreateUserPreferenceAsync(userPreference, ct: ct);

            return dbResult.Status != DbStatusCode.Created
                ? RequestResultFactory.ServiceError<UserPreferenceModel>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message)
                : RequestResultFactory.Success(mappingService.MapToUserPreferenceModel(dbResult.Payload));
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, UserPreferenceModel>> GetUserPreferencesAsync(string hdid, CancellationToken ct = default)
        {
            logger.LogTrace("Getting user preference... {Hdid}", hdid);
            IEnumerable<UserPreference> userPreferences = await userPreferenceDelegate.GetUserPreferencesAsync(hdid, ct);
            return userPreferences.Select(mappingService.MapToUserPreferenceModel).ToDictionary(x => x.Preference, x => x);
        }
    }
}
