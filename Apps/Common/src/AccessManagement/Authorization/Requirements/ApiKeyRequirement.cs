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
namespace HealthGateway.Common.AccessManagement.Authorization.Requirements
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// ApiKeyRequirement looks up the Configuration API Key.
    /// </summary>
    public class ApiKeyRequirement : IAuthorizationRequirement
    {
        private const string WebHookApiSectionKey = "AcaPy";
        private const string WebHookApiKey = "webhookApiKey";

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyRequirement"/> class.
        /// </summary>
        /// <param name="configuration">The configuration provider.</param>
        public ApiKeyRequirement(IConfiguration configuration)
        {
            this.ApiKey = configuration.GetValue<string>($"{WebHookApiSectionKey}:{WebHookApiKey}");
        }

        /// <summary>
        /// Gets a value indicating whether the ownership of the resource should be confirmed.
        /// </summary>
        public string ApiKey { get; }
    }
}
