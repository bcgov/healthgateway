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
namespace HealthGateway.Encounter.Services
{
    using System;
    using AutoMapper;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.PHSA;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    public class EncounterMappingService(IMapper mapper, IConfiguration configuration) : IEncounterMappingService
    {
        private TimeZoneInfo LocalTimeZone => DateFormatter.GetLocalTimeZone(configuration);

        /// <inheritdoc/>
        public EncounterModel MapToEncounterModel(Claim source)
        {
            return mapper.Map<Claim, EncounterModel>(source);
        }

        /// <inheritdoc/>
        public HospitalVisitModel MapToHospitalVisitModel(HospitalVisit source)
        {
            HospitalVisitModel? dest = mapper.Map<HospitalVisit, HospitalVisitModel>(source);

            dest.AdmitDateTime = dest.AdmitDateTime == null ? null : DateFormatter.SpecifyTimeZone(dest.AdmitDateTime.Value, this.LocalTimeZone);
            dest.EndDateTime = dest.EndDateTime == null ? null : DateFormatter.SpecifyTimeZone(dest.EndDateTime.Value, this.LocalTimeZone);

            return dest;
        }
    }
}
