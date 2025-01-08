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
namespace HealthGateway.Admin.Server.MapProfiles
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AutoMapper;
    using HealthGateway.Admin.Common.Constants;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between DB and UI Models.
    /// </summary>
    [SuppressMessage("Style", "IDE0072:Populate switch", Justification = "Team decision")]
    public class BetaFeatureProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BetaFeatureProfile"/> class.
        /// </summary>
        public BetaFeatureProfile()
        {
            this.CreateMap<Database.Constants.BetaFeature, BetaFeature>()
                .ConvertUsing(
                    (source, _, _) => source switch
                    {
                        Database.Constants.BetaFeature.Salesforce => BetaFeature.Salesforce,
                        _ => throw new NotImplementedException($"Mapping for {source} is not implemented"),
                    });

            this.CreateMap<BetaFeature, Database.Constants.BetaFeature>()
                .ConvertUsing(
                    (source, _, _) => source switch
                    {
                        BetaFeature.Salesforce => Database.Constants.BetaFeature.Salesforce,
                        _ => throw new NotImplementedException($"Reverse mapping for {source} is not implemented"),
                    });
        }
    }
}
