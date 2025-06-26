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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
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

        /// <summary>
        /// Retrieves the refresh status for a specified personal health number (PHN) from a given system source.
        /// </summary>
        /// <param name="request">
        /// The request containing the personal health number (PHN) and system source for which to check
        /// refresh status.
        /// </param>
        /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A task representing the asynchronous operation. Upon completion, the task result contains a
        /// <see cref="HealthDataResponse"/> with the raw refresh status and source system details for the given PHN.
        /// </returns>
        [Post("/patient/health-data/status")]
        Task<HealthDataResponse> HealthDataStatusAsync([Body] HealthDataStatusRequest request, CancellationToken ct = default);

        /// <summary>
        /// Queues a request to refresh cached health data for a specified personal health number (PHN) and system source.
        /// </summary>
        /// <param name="request">
        /// The request containing the personal health number (PHN) and the source system identifier to refresh data from.
        /// </param>
        /// <param name="ct">
        /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        [Post("/patient/health-data/queue-refresh")]
        Task HealthDataQueueRefreshAsync([Body] HealthDataStatusRequest request, CancellationToken ct = default);
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
        public required string Phn { get; init; }
    }

    /// <summary>
    /// Represents the response containing refresh status information for a specific
    /// personal health number (PHN) and associated system source.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record HealthDataResponse
    {
        /// <summary>
        /// Gets the raw refresh status message as provided by the source system.
        /// </summary>
        /// <returns>
        /// A string representing the source system's refresh status. The content is not interpreted by this service.
        /// </returns>
        [JsonPropertyName("status")]
        public string? Status { get; init; }

        /// <summary>
        /// Gets the last refresh date as provided by the source system.
        /// </summary>
        /// <returns>
        /// A nullable <see cref="DateOnly"/> representing the last known refresh date from the source system.
        /// </returns>
        [JsonPropertyName("lastRefreshDate")]
        public DateOnly? LastRefreshDate { get; init; }

        /// <summary>
        /// Gets the system source from which the refresh status originated.
        /// </summary>
        /// <returns>
        /// A <see cref="SystemSource"/> value such as <c>DiagnosticImaging</c> or <c>Laboratory</c>.
        /// </returns>
        [JsonPropertyName("system")]
        public SystemSource System { get; init; }
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
