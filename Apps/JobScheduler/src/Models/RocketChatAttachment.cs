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
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a RocketChat web hook attachment object.
    /// </summary>
    public class RocketChatAttachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RocketChatAttachment"/> class.
        /// </summary>
        public RocketChatAttachment()
        {
        }

        /// <summary>
        /// Gets or sets the Title of the attachment.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the attachment URL link.
        /// </summary>
        [JsonPropertyName("title_link")]
        public Uri? TitleLink { get; set; }

        /// <summary>
        /// Gets or sets the text associated to the attachment.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the image URL for the attachment.
        /// </summary>
        [JsonPropertyName("image_url")]
        public Uri? ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        [JsonPropertyName("color")]
        public string? Color { get; set; }
    }
}
