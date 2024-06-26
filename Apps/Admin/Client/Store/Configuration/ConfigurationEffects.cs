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
namespace HealthGateway.Admin.Client.Store.Configuration
{
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class ConfigurationEffects(ILogger<ConfigurationEffects> logger, IConfigurationApi configApi)
    {
        [EffectMethod(typeof(ConfigurationActions.LoadAction))]
        public async Task HandleLoadAction(IDispatcher dispatcher)
        {
            logger.LogInformation("Loading external configuration");

            try
            {
                ExternalConfiguration response = await configApi.GetConfigurationAsync();
                logger.LogInformation("External configuration loaded successfully!");
                dispatcher.Dispatch(new ConfigurationActions.LoadSuccessAction { Data = response });
            }
            catch (ApiException ex)
            {
                RequestError error = StoreUtility.FormatRequestError(ex);
                logger.LogError(ex, "Error loading external configuration, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new ConfigurationActions.LoadFailureAction { Error = error });
            }
        }
    }
}
