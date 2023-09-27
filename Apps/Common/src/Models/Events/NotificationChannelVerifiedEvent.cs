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

/// <summary>
/// Represents an event when a notification channel is verified.
/// </summary>
/// <param name="Hdid">The user's profile Id and HDID.</param>
/// <param name="Type">The channel in which the user has verified their notification settings.</param>
/// <param name="Value">The email or phone number for the channel selected.</param>
public record NotificationChannelVerifiedEvent(string Hdid, NotificationChannel Type, string Value);
