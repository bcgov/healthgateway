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

namespace HealthGateway.Encounter.MapUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.PHSA;

    /// <summary>
    /// Static helper class for conversion of model objects.
    /// </summary>
    public static class HospitalVisitMapUtils
    {
        /// <summary>
        /// Creates a UI model from a PHSA model.
        /// </summary>
        /// <param name="model">The model to convert.</param>
        /// <param name="mapper">The AutoMapper IMapper.</param>
        /// <param name="timezone">The timezone to use.</param>
        /// <returns>The created UI model.</returns>
        public static HospitalVisitModel ToUiModel(HospitalVisit model, IMapper mapper, TimeZoneInfo timezone)
        {
            return mapper.Map<HospitalVisit, HospitalVisitModel>(
                model,
                opts =>
                    opts.AfterMap(
                        (_, dest) =>
                        {
                                dest.AdmitDateTime = dest.AdmitDateTime == null ? null : DateFormatter.SpecifyTimeZone(dest.AdmitDateTime.Value, timezone);
                                dest.EndDateTime = dest.EndDateTime == null ? null : DateFormatter.SpecifyTimeZone(dest.EndDateTime.Value, timezone);
                        }));
        }

        /// <summary>
        /// Creates a UI model from a PHSA model.
        /// </summary>
        /// <param name="models">The model to convert.</param>
        /// <param name="mapper">The AutoMapper IMapper.</param>
        /// <param name="timezone">The timezone to use.</param>
        /// <returns>The created UI model.</returns>
        public static IEnumerable<HospitalVisitModel> ToUiModels(IEnumerable<HospitalVisit> models, IMapper mapper, TimeZoneInfo timezone)
        {
            return models.Select(m => ToUiModel(m, mapper, timezone)).ToList();
        }
    }
}
