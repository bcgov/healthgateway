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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// UMA 2.0 PermissionResponse.
    /// </summary>
    public class PermissionResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionResponse"/> class.
        /// </summary>
        /// <param name="ticket">A permission ticket.</param>
        public PermissionResponse(string ticket)
        {
            this.Ticket = ticket;
        }

        /// <summary>
        /// Gets or sets the permission ticket.
        /// </summary>
        [JsonPropertyName("ticket")]
        public string Ticket { get; set; } = string.Empty;
    }
}
