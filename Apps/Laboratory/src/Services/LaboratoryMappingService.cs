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
    using System;
    using AutoMapper;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    public class LaboratoryMappingService(IMapper mapper, IConfiguration configuration) : ILaboratoryMappingService
    {
        private TimeZoneInfo LocalTimeZone => DateFormatter.GetLocalTimeZone(configuration);

        /// <inheritdoc/>
        public Covid19Order MapToCovid19Order(PhsaCovid19Order source)
        {
            return mapper.Map<PhsaCovid19Order, Covid19Order>(source);
        }

        /// <inheritdoc/>
        public Covid19Test MapToCovid19Test(PhsaCovid19Test source)
        {
            return mapper.Map<PhsaCovid19Test, Covid19Test>(source);
        }

        /// <inheritdoc/>
        public LaboratoryOrder MapToLaboratoryOrder(PhsaLaboratoryOrder source)
        {
            LaboratoryOrder? dest = mapper.Map<PhsaLaboratoryOrder, LaboratoryOrder>(source);

            if (dest.CollectionDateTime != null)
            {
                dest.CollectionDateTime = DateFormatter.SpecifyTimeZone(dest.CollectionDateTime.Value, this.LocalTimeZone);
            }

            dest.TimelineDateTime = DateFormatter.SpecifyTimeZone(dest.TimelineDateTime, this.LocalTimeZone);

            return dest;
        }

        /// <inheritdoc/>
        public LaboratoryTest MapToLaboratoryTest(PhsaLaboratoryTest source)
        {
            return mapper.Map<PhsaLaboratoryTest, LaboratoryTest>(source);
        }
    }
}
