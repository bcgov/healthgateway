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
namespace HealthGateway.Database.Delegates
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Delegate for CRUD operations on Application Settings.
    /// </summary>
    public interface IApplicationSettingsDelegate
    {
        /// <summary>
        /// Gets a single application setting.
        /// </summary>
        /// <param name="application">The name of the application.</param>
        /// <param name="component">The name of the component.</param>
        /// <param name="key">The name of the key.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The fetched application setting.</returns>
        Task<ApplicationSetting?> GetApplicationSettingAsync(string application, string component, string key, CancellationToken ct = default);

        /// <summary>
        /// Prepares a new application setting to be inserted.
        /// </summary>
        /// <param name="appSetting">The application setting to insert.</param>
        void AddApplicationSetting(ApplicationSetting appSetting);
    }
}
