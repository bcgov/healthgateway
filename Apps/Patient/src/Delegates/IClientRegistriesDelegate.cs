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
namespace HealthGateway.Patient.Delegates
{
    using System.Threading.Tasks;
    using ServiceReference;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    public interface IClientRegistriesDelegate
    {
        /// <summary>
        /// Gets the patient record.
        /// </summary>
        /// <param name="request">The get demographics request.</param>
        /// <returns>The patient get demographics response.</returns>
        Task<HCIM_IN_GetDemographicsResponse1> GetDemographicsAsync(HCIM_IN_GetDemographicsRequest request);
    }
}