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
namespace HealthGateway.GatewayApi.Models
{
    /// <summary>
    /// Model representing contact info.
    /// </summary>
    public class UserProtection
    {
        /// <summary>
        /// Gets or sets the delegate user's HDID (e.g., the requesting delegate).
        /// </summary>
        public string DelegateHdid { get; set; } = null!;

        /// <summary>
        /// Gets or sets the protection status information for the user.
        /// </summary>
        public ProtectedSubject ProtectedSubject { get; set; } = null!;
    }

    /// <summary>
    /// Represents the protection status of a subject user.
    /// </summary>
    /// <param name="SubjectHdid">The HDID of the subject user being queried.</param>
    /// <param name="Protected">Indicates whether the subject is protected (<c>true</c>) or not (<c>false</c>).</param>
    public record ProtectedSubject(string SubjectHdid, bool Protected = false);
}
