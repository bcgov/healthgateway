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
    using System.Text.Json.Serialization;

    /// <summary>
    /// Various timeout values used by the VUE WebClient application.
    /// </summary>
    public class TimeOutsConfiguration
    {
        /// <summary>
        /// Gets or sets the idle time in seconds that the Webclient will use
        /// before it automatically logs the user out.
        /// </summary>
        public int Idle { get; set; }

        /// <summary>
        /// Gets or sets the amount of time in seconds after which the user will be
        /// redirected from the logout page back to the home.
        /// </summary>
        public string LogoutRedirect { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the amount of time in minutes after which the user
        /// can retry the verification code.
        /// </summary>
        [JsonPropertyName("resendSMS")]
        public int ResendSms { get; set; }
    }
}
