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
namespace HealthGateway.MedicationService.Models
{
    /// <summary>
    /// The json web token model.
    /// </summary>
    public class JWTModel : IAuthModel
    {
        /// <inheritdoc/>
        public int ExpiresInMinutes { get; set; }

        /// <inheritdoc/>
        public int RefreshExpiresInMinutes { get; set; }

        /// <inheritdoc/>
        public string RefreshToken { get; set; }

        /// <inheritdoc/>
        public string AccessToken { get; set; }

        /// <inheritdoc/>
        public string TokenType { get; set; }

        /// <inheritdoc/>
        public string SessionState { get; set; }
    }
}