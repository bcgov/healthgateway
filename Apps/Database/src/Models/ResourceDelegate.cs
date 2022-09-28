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
namespace HealthGateway.Database.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// The resource delegate model.
    /// </summary>
    public class ResourceDelegate : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the owner of the hdid.
        /// </summary>
        [MaxLength(52)]
        public string ResourceOwnerHdid { get; set; } = null!;

        /// <summary>
        /// Gets or sets the hdid which has delegated access to the owner Id.
        /// </summary>
        [MaxLength(52)]
        public string ProfileHdid { get; set; } = null!;

        /// <summary>
        /// Gets or sets reason code for the resource delegate.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public ResourceDelegateReason ReasonCode { get; set; }

        /// <summary>
        /// Gets or sets the ReasonObject type.
        /// This value is used by code to reconstruct the JSON POCO.
        /// </summary>
        public string? ReasonObjectType { get; set; }

        /// <summary>
        /// Gets or sets the resource delegation Reason object.
        /// </summary>
        public JsonDocument? ReasonObject { get; set; }
    }
}
