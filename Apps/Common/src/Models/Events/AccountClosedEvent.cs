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
namespace HealthGateway.Common.Models.Events;

using System;
using HealthGateway.Common.Messaging;

/// <summary>
/// Represents an event when an account is closed.
/// </summary>
/// <param name="Hdid">The user's profile id and HDID.</param>
/// <param name="ClosedDate">The date the account was actually closed.</param>
public record AccountClosedEvent(string Hdid, DateTime ClosedDate) : MessageBase;
