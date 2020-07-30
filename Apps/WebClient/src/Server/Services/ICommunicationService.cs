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
namespace HealthGateway.WebClient.Services
{
    using System.Collections.Generic;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Service to interact with the Communication Delegate.
    /// </summary>
    public interface ICommunicationService
    {
        /// <summary>
        /// Gets the active communication banner from the backend.
        /// </summary>
        /// <returns>The active communication wrapped in a RequestResult.</returns>
        public RequestResult<Communication> GetActiveBanner();
    }
}
