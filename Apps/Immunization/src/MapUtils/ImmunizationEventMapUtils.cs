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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Static helper class for conversion of model objects.
    /// </summary>
    public static class ImmunizationEventMapUtils
    {
        /// <summary>
        /// Creates an ImmunizationEvent model from a PHSA model.
        /// </summary>
        /// <param name="model">The PHSA model to convert.</param>
        /// <param name="mapper">The automapper to use.</param>
        /// <param name="timezone">The timezone to use.</param>
        /// <returns>The newly created model.</returns>
        public static ImmunizationEvent ToUiModel(ImmunizationViewResponse model, IMapper mapper, TimeZoneInfo timezone)
        {
            return mapper.Map<ImmunizationViewResponse, ImmunizationEvent>(
                model,
                opts => opts.AfterMap((_, dest) => dest.DateOfImmunization = DateFormatter.SpecifyTimeZone(dest.DateOfImmunization, timezone)));
        }

        /// <summary>
        /// Creates ImmunizationEvent models from PHSA models.
        /// </summary>
        /// <param name="models">The PHSA models to convert.</param>
        /// <param name="mapper">The automapper to use.</param>
        /// <param name="timezone">The timezone to use.</param>
        /// <returns>The newly created models.</returns>
        public static IList<ImmunizationEvent> ToUiModels(IEnumerable<ImmunizationViewResponse> models, IMapper mapper, TimeZoneInfo timezone)
        {
            return models.Select(m => ToUiModel(m, mapper, timezone)).ToList();
        }
    }
}
