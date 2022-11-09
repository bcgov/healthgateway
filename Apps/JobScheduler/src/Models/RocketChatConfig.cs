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
namespace HealthGateway.JobScheduler.Models
{
    using System;

    /// <summary>
    /// Represents the RocketChat Configuration.
    /// </summary>
    public class RocketChatConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RocketChatConfig"/> class.
        /// </summary>
        public RocketChatConfig()
        {
        }

        /// <summary>
        /// Gets or sets the Rocket Chat Message to be posted.
        /// </summary>
        public RocketChatMessage? Message { get; set; }

        /// <summary>
        /// Gets or sets the webhook URL to post the message to.
        /// </summary>
        public Uri? WebHookURL { get; set; }
    }
}
