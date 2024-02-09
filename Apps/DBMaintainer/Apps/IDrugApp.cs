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
namespace HealthGateway.DBMaintainer.Apps
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Common interface for all drug apps.
    /// </summary>
    public interface IDrugApp
    {
        /// <summary>
        /// Processes the downloaded files.
        /// </summary>
        /// <param name="configSectionName">The name of the configuration to use for configuration.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous Job.
        /// </returns>
        Task ProcessAsync(string configSectionName, CancellationToken ct = default);
    }
}
