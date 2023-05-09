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

namespace HealthGateway.Database.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Outbox status.
/// </summary>
public enum OutboxItemStatus
{
    /// <summary>
    /// Item is pending forwarding.
    /// </summary>
    Pending,

    /// <summary>
    /// Item is waiting retry forwarding.
    /// </summary>
    Retry,
}

/// <summary>
/// Stores an message in the outbox table.
/// </summary>
public class OutboxItem
{
    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the create timestamp.
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the queued serialized message.
    /// </summary>
    [Required]
    [Column(TypeName = "jsonb")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// Gets or sets the message metadata.
    /// </summary>
    [Required]
    [Column(TypeName = "jsonb")]
    public OutboxItemMetadata Metadata { get; set; } = null!;

    /// <summary>
    /// Gets or sets the status of the item.
    /// </summary>
    [Required]
    public OutboxItemStatus Status { get; set; } = OutboxItemStatus.Pending;
}

/// <summary>
/// Metadata about a message in the outbox table.
/// </summary>
public record OutboxItemMetadata
{
    /// <summary>
    /// Gets or sets the message type.
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets the message creation timestamp.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets an optional session id for the message.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Gets or sets the assembly qualified name of the message for deserialization purposes.
    /// </summary>
    public string AssemblyQualifiedName { get; set; } = null!;
}
