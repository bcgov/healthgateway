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
    using System.Diagnostics.CodeAnalysis;
    using AutoMapper;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;

    /// <summary>
    /// An AutoMapper profile that defines mappings between PHSA and Health Gateway Models.
    /// </summary>
    public class LaboratoryTestProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryTestProfile"/> class.
        /// </summary>
        [SuppressMessage("Minor Code Smell", "S3440:Variables should not be checked against the values they\'re about to be assigned", Justification = "Team decision")]
        public LaboratoryTestProfile()
        {
            this.CreateMap<PhsaLaboratoryTest, LaboratoryTest>()
                .AfterMap(
                    (src, dest) =>
                    {
                        dest.TestStatus = src.PlisTestStatus switch
                        {
                            "Active" => "Pending",
                            _ => src.PlisTestStatus,
                        };
                        dest.Result = src.PlisTestStatus switch
                        {
                            "Completed" or "Corrected" when src.OutOfRange => "Out of Range",
                            "Completed" or "Corrected" when !src.OutOfRange => "In Range",
                            "Cancelled" => "Cancelled",
                            _ => "Pending",
                        };
                    });
        }
    }
}
