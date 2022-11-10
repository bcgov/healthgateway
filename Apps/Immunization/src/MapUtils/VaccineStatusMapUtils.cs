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

namespace HealthGateway.Immunization.MapUtils
{
    using AutoMapper;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// Static helper class for conversion of model objects.
    /// </summary>
    public static class VaccineStatusMapUtils
    {
        /// <summary>
        /// Creates a UI model from a PHSA model.
        /// </summary>
        /// <param name="mapper">The AutoMapper IMapper.</param>
        /// <param name="phsaModel">The PHSA model to convert.</param>
        /// <param name="personalHealthNumber">The patient's personal health number.</param>
        /// <returns>The created UI model.</returns>
        public static VaccineStatus ToUiModel(IMapper mapper, VaccineStatusResult phsaModel, string? personalHealthNumber)
        {
            VaccineStatus uiModel = mapper.Map<VaccineStatusResult, VaccineStatus>(
                phsaModel,
                opts => opts.AfterMap((_, dest) => dest.PersonalHealthNumber = personalHealthNumber));
            return uiModel;
        }
    }
}
