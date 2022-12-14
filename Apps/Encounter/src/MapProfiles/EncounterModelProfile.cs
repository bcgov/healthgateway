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
namespace HealthGateway.Encounter.MapProfiles
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using AutoMapper;
    using HealthGateway.Encounter.Models;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between PHSA and UI Models.
    /// </summary>
    public class EncounterModelProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterModelProfile"/> class.
        /// </summary>
        [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "Team decision")]
        [SuppressMessage("Security", "SCS0006:Weak hashing function", Justification = "Team decision")]
        public EncounterModelProfile()
        {
            this.CreateMap<Claim, EncounterModel>()
                .ForMember(dest => dest.EncounterDate, opt => opt.MapFrom(src => src.ServiceDate))
                .ForMember(dest => dest.SpecialtyDescription, opt => opt.MapFrom(src => src.SpecialtyDesc))
                .ForMember(
                    dest => dest.Clinic,
                    opt => opt.MapFrom(
                        src => new Clinic
                            { Name = src.LocationName }))
                .AfterMap(
                    (src, dest) =>
                    {
                        StringBuilder sourceId = new();
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.ServiceDate:yyyyMMdd}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.SpecialtyDesc}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.PractitionerName}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.LocationName}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.LocationAddress.Province}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.LocationAddress.City}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.LocationAddress.PostalCode}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.LocationAddress.AddrLine1}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.LocationAddress.AddrLine2}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.LocationAddress.AddrLine3}");
                        sourceId.Append(CultureInfo.InvariantCulture, $"{src.LocationAddress.AddrLine4}");

                        byte[] hashBytes = MD5.HashData(Encoding.Default.GetBytes(sourceId.ToString()));
                        dest.Id = new Guid(hashBytes).ToString();
                    });
        }
    }
}
