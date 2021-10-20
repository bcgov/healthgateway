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
namespace HealthGateway.Common.Models
{
    using HealthGateway.Common.Constants;

    /// <summary>
    /// The response from a Vaccine Proof request.
    /// </summary>
    public class VaccineProofResponse
    {
        /// <summary>
        /// Gets or sets the unique Id of the Vaccine Proof Request.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Gets or sets the status of the Vaccine Proof Request.
        /// </summary>
        public VaccineProofRequestStatus Status { get; set; } = VaccineProofRequestStatus.Unknown;
    }
}
