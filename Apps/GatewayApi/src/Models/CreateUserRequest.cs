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
namespace HealthGateway.GatewayApi.Models
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Object that defines the request for creating a User.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Gets the create user profile.
        /// </summary>
        public CreateUserProfile Profile { get; init; } = new(string.Empty, Guid.Empty);
    }

    /// <summary>
    /// The user profile to create.
    /// </summary>
    /// <param name="HdId">The hdid associated with the user profile to create.</param>
    /// <param name="TermsOfServiceId">The terms of service id associated with the user profile to create.</param>
    /// <param name="SmsNumber">The sms number associated with the user profile to create.</param>
    /// <param name="Email">The email associated with the user profile to create.</param>
    public record CreateUserProfile(string HdId, [property: JsonRequired] Guid TermsOfServiceId, string? SmsNumber = null, string? Email = null);
}
