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
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a RocketChat Web Hook object.
    /// </summary>
    public class RocketChatMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RocketChatMessage"/> class.
        /// </summary>
        public RocketChatMessage()
        {
        }

        /// <summary>
        /// Gets or sets the Emoji Icon for the message.
        /// </summary>
        [JsonPropertyName("icon_emoji")]
        public string? IconEmoji { get; set; }

        /// <summary>
        /// Gets or sets the Message text.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the list of attachments.
        /// </summary>
        [JsonPropertyName("attachments")]
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<RocketChatAttachment>? Attachments { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
