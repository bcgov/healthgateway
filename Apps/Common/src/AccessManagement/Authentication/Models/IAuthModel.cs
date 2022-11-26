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
namespace HealthGateway.Common.AccessManagement.Authentication.Models
{
    /// <summary>
    /// The authorization model.
    /// </summary>
    public interface IAuthModel
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        string? AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the token expiration in seconds.
        /// </summary>
        int? ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the refresh token expiration in minutes.
        /// </summary>
        int? RefreshExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        string? RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the token type.
        /// </summary>
        string? TokenType { get; set; }

        /// <summary>
        /// Gets or sets the not-before-policy.
        /// </summary>
        int? NotBeforePolicy { get; set; }

        /// <summary>
        /// Gets or sets the session state.
        /// </summary>
        string? SessionState { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        string? Scope { get; set; }
    }
}
