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
    using AutoMapper;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between DB and UI Models.
    /// </summary>
    public class BetaFeatureAccessProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BetaFeatureAccessProfile"/> class.
        /// </summary>
        public BetaFeatureAccessProfile()
        {
            this.CreateMap<BetaFeature, Common.Constants.BetaFeature>()
                .ConvertUsing(
                    (source, _, _) =>
                    {
                        return source switch
                        {
                            BetaFeature.Salesforce => Common.Constants.BetaFeature.Salesforce,
                            _ => throw new NotImplementedException($"Mapping for {source} is not implemented"),
                        };
                    });

            this.CreateMap<Common.Constants.BetaFeature, BetaFeature>()
                .ConvertUsing(
                    (source, _, _) =>
                    {
                        return source switch
                        {
                            Common.Constants.BetaFeature.Salesforce => BetaFeature.Salesforce,
                            _ => throw new NotImplementedException($"Reverse mapping for {source} is not implemented"),
                        };
                    });
        }
    }
}
