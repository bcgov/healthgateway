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
namespace HealthGateway.Laboratory.MapProfiles
{
    using AutoMapper;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;

    /// <summary>
    /// An AutoMapper profile that defines mappings between PHSA and Health Gateway Models.
    /// </summary>
    public class Covid19OrderProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Covid19OrderProfile"/> class.
        /// </summary>
        public Covid19OrderProfile()
        {
            this.CreateMap<PhsaCovid19Order, Covid19Order>()
                .ForMember(dest => dest.Phn, opt => opt.MapFrom(src => MaskPhn(src.Phn)));
        }

        private static string MaskPhn(string phn)
        {
            string retVal = "****";
            if (phn.Length > 3)
            {
                retVal = $"{phn.Remove(phn.Length - 5, 4)}****";
            }

            return retVal;
        }
    }
}
