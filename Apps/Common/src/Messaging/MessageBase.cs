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

namespace HealthGateway.Common.Messaging;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using HealthGateway.Common.Utils;

/// <summary>
/// Base record for messages
/// It uses PolymorphicJsonConverter to ensure the type is always serialized in the payload.
/// </summary>
[ExcludeFromCodeCoverage]
[JsonConverter(typeof(PolymorphicJsonConverter<MessageBase>))]
public abstract record MessageBase;

/// <summary>
/// Message envelope that contains a message and metadata.
/// </summary>
/// <param name="SessionId">The session id to manage FIFO.</param>
/// <param name="Content">The message.</param>
[ExcludeFromCodeCoverage]
public record MessageEnvelope(MessageBase Content, string SessionId)
{
    /// <summary>
    /// Gets or sets the message creation timestamp.
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the message type name.
    /// </summary>
    public string MessageType => this.Content.GetType().Name;

    /// <summary>
    /// Gets the creation time timestamp as a string.
    /// </summary>
    public string CreatedOnTimestamp => this.CreatedOn.ToString("o");
}
