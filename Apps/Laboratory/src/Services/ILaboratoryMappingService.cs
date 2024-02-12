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
namespace HealthGateway.Laboratory.Services
{
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;

    /// <summary>
    /// Service to map between models at different layers.
    /// </summary>
    public interface ILaboratoryMappingService
    {
        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        Covid19Order MapToCovid19Order(PhsaCovid19Order source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        Covid19Test MapToCovid19Test(PhsaCovid19Test source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        LaboratoryOrder MapToLaboratoryOrder(PhsaLaboratoryOrder source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        LaboratoryTest MapToLaboratoryTest(PhsaLaboratoryTest source);
    }
}
