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
namespace HealthGateway.WebClient.Server.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Settings for authentication.
    /// </summary>
    /// <param name="Endpoint">Gets the URI for the Authentication server.</param>
    /// <param name="IdentityProviderId">Gets the ID of the Identity Provider to be used.</param>
    /// <param name="ClientId">Gets the client ID.</param>
    /// <param name="AndroidClientId">Gets the android client ID.</param>
    /// <param name="IosClientId">Gets the ios client ID.</param>
    /// <param name="RedirectUri">Gets the redirect URI.</param>
    public record MobileAuthenticationSettings(
        Uri? Endpoint = null,
        string? IdentityProviderId = null,
        string? ClientId = null,
        string? AndroidClientId = null,
        string? IosClientId = null,
        [SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Special URI Values")]
        [SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "Special URI Values")]
        string? RedirectUri = null);
}
