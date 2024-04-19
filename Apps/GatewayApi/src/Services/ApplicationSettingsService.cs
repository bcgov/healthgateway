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
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;

    /// <inheritdoc/>
    /// <param name="applicationSettingsDelegate">The injected Application Settings delegate.</param>
    /// <param name="cacheProvider">The injected cache provider.</param>
    public class ApplicationSettingsService(IApplicationSettingsDelegate applicationSettingsDelegate, ICacheProvider cacheProvider) : IApplicationSettingsService
    {
        /// <inheritdoc/>
        public async Task<DateTime?> GetLatestTourChangeDateTimeAsync(CancellationToken ct = default)
        {
            return await cacheProvider.GetOrSetAsync(
                $"{TourApplicationSettings.Application}:{TourApplicationSettings.Component}:{TourApplicationSettings.LatestChangeDateTime}",
                async () =>
                {
                    ApplicationSetting? applicationSetting = await applicationSettingsDelegate.GetApplicationSettingAsync(
                        TourApplicationSettings.Application,
                        TourApplicationSettings.Component,
                        TourApplicationSettings.LatestChangeDateTime,
                        ct);

                    return applicationSetting?.Value != null
                        ? DateTime.Parse(applicationSetting.Value, CultureInfo.InvariantCulture).ToUniversalTime()
                        : (DateTime?)null;
                },
                TimeSpan.FromMinutes(30),
                ct);
        }
    }
}
