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
    using System;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;

    /// <summary>
    /// A mechanism to generate or mail out Vaccine Proofs.
    /// </summary>
    public interface IVaccineProofDelegate
    {
        /// <summary>
        /// Initiates the creation of a Vaccine Proof and requests the output be mailed.
        /// </summary>
        /// <param name="vaccineProofTemplate">The template to be used for the Vaccine Proof.</param>
        /// <param name="request">The vaccination data to be sent to populate the Vaccine Proof.</param>
        /// <param name="address">The address where the Vaccine Proof should be mailed.</param>
        /// <returns>A response object that includes the status and identifier of the Vaccine Proof.</returns>
        public Task<RequestResult<VaccineProofResponse>> MailAsync(VaccineProofTemplate vaccineProofTemplate, VaccineProofRequest request, Address address);

        /// <summary>
        /// Initiates the creation of a Vaccine Proof for later retrieval.
        /// </summary>
        /// <param name="vaccineProofTemplate">The template to be used for the Vaccine Proof.</param>
        /// <param name="request">The vaccination data to be used for generating the Vaccine Proof.</param>
        /// <returns>A response object that includes the status and identifier of the Vaccine Proof.</returns>
        public Task<RequestResult<VaccineProofResponse>> GenerateAsync(VaccineProofTemplate vaccineProofTemplate, VaccineProofRequest request);

        /// <summary>
        /// Fetches the generated Vaccine Proof directly.
        /// </summary>
        /// <param name="assetUri">The uri to fetch the asset.</param>
        /// <returns>A report model containing the generated Vaccine Proof document.</returns>
        public Task<RequestResult<ReportModel>> GetAssetAsync(Uri assetUri);
    }
}
