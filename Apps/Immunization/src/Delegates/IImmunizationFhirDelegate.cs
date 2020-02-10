﻿//-------------------------------------------------------------------------
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
namespace HealthGateway.Immunization.Delegates
{
    using System.Threading.Tasks;
    using HealthGateway.Immunization.Models;
    using Hl7.Fhir.Model;

    /// <summary>
    /// Interface that defines a delegate to retrieve patient information.
    /// </summary>
    public interface IImmunizationFhirDelegate
    {
        /// <summary>
        /// Gets the immunization summary for the provided phn.
        /// </summary>
        /// <param name="phn">The patient hdid.</param>
        /// <returns>The immunization fhir bundle.</returns>
        Task<Bundle> GetImmunizationBundle(string phn);
    }
}
