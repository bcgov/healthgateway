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

using System.Text.Json.Serialization;
using HealthGateway.Common.Utils;

/// <summary>
/// Base record for messages
/// It uses PolymorphicJsonConverter to ensure the type is always serialized in the payload.
/// </summary>
/// <param name="SessionId">Optional session identifier to support FIFO behaviour for a particular subject.</param>
[JsonConverter(typeof(PolymorphicJsonConverter<MessageBase>))]
public abstract record MessageBase(string? SessionId = null);
