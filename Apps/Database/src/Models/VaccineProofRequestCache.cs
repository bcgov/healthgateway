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
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// A model object storing VaccineProof Requests.
    /// </summary>
    public class VaccineProofRequestCache : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique id for this entry.
        /// </summary>
        [Column("VaccineProofRequestCacheId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier for the person of this entry.
        /// </summary>
        [Required]
        [MaxLength(54)]
        public string? PersonIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the base6 encoded md5 hash of the SHC Image.
        /// </summary>
        [Required]
        public string? ShcImageHash { get; set; }

        /// <summary>
        /// Gets or sets the VaccineProofTemplate used for the entry.
        /// </summary>
        [Required]
        public VaccineProofTemplate ProofTemplate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this entry expires.
        /// </summary>
        [Required]
        public DateTime? ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the associated response id from the Vaccine Proof Request.
        /// </summary>
        [Required]
        public string? VaccineProofResponseId { get; set; }

        /// <summary>
        /// Gets or sets the asset endpoint as a string from the Vaccine Proof Request.
        /// </summary>
        [Required]
        public string? AssetEndpoint { get; set; }
    }
}
