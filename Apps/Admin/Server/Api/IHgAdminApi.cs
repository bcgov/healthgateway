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
namespace HealthGateway.Admin.Server.Api
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Refit;

    /// <summary>
    /// Interface to interact with PHSA.CCD.API.HealthGatewayAdmin Patient API.
    /// </summary>
    public interface IHgAdminApi
    {
        /// <summary>
        /// Checks whether a personal health number (PHN) is registered with the system.
        /// </summary>
        /// <param name="request">The request containing the personal health number (PHN) to check for registration status.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="PersonalAccountsResponse"/>
        /// indicating whether the PHN is registered.
        /// </returns>
        [Post("/personal-accounts/status")]
        Task<PersonalAccountsResponse> PersonalAccountsStatusAsync([Body] PersonalAccountStatusRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Represents a request to check whether a personal health number (PHN) is registered in the system.
    /// </summary>
    public record PersonalAccountStatusRequest
    {
        /// <summary>
        /// Gets the personal health number (PHN) to check for registration status.
        /// </summary>
        /// <remarks>
        /// This value is sent in the request body to identify which patient's registration status should be queried.
        /// </remarks>
        [JsonPropertyName("phn")]
        public string Phn { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents the response indicating whether a personal health number (PHN) is registered in the system.
    /// </summary>
    // Refit instantiates this via deserialization
    [ExcludeFromCodeCoverage]
    public record PersonalAccountsResponse
    {
        /// <summary>
        /// Gets a value indicating whether personal account is registered.
        /// </summary>
        /// <returns>
        /// A value of <c>true</c> means the personal account is registered; <c>false</c> otherwise.
        /// </returns>
        [JsonPropertyName("registered")]
        public bool Registered { get; init; }
    }
}
