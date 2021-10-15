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
namespace HealthGateway.Common.Delegates
{
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;

    /// <summary>
    /// A mechanism to generate a Vaccine Proof for Print or Mail.
    /// </summary>
    public interface IVaccineProofDelegate
    {
        /// <summary>
        /// Initiates the creation of the Vaccine Proof for the purpose of Printing.
        /// </summary>
        /// <param name="printTemplate">The template to be used for printing.</param>
        /// <param name="request">The VaccineProof data to be used.</param>
        /// <returns>.</returns>
        public RequestResult<VaccineProofResponse> Print(VaccineProofTemplate printTemplate, VaccineProofRequest request);

        /// <summary>
        /// Initiates the creation of the Vaccine Proof and requests the output be mailed.
        /// </summary>
        /// <param name="mailTemplate">The template to be used for printing.</param>
        /// <param name="request">The VaccineProof data to be used.</param>
        /// <param name="address">The address to send the vaccine proof out to.</param>
        /// <returns>.</returns>
        public RequestResult<VaccineProofResponse> Mail(VaccineProofTemplate mailTemplate, VaccineProofRequest request, Address address);

        /// <summary>
        /// Determines if the Vaccine Proof has been generated and can be fetched.
        /// </summary>
        /// <param name="id">The id from the VaccineProofResponse object.</param>
        /// <returns>.</returns>
        public RequestResult<VaccineProofResponse> Status(string id);

        /// <summary>
        /// Fetches the generated Vaccine Proof.
        /// </summary>
        /// <param name="id">The id from the VaccineProofResponse object.</param>
        /// <returns>.</returns>
        public RequestResult<object> GetAsset(string id);
    }
}
