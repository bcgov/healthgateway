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
namespace HealthGateway.GatewayApi.MapProfiles
{
    using System;
    using AutoMapper;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using CommunicationStatus = HealthGateway.GatewayApi.Constants.CommunicationStatus;
    using CommunicationType = HealthGateway.GatewayApi.Constants.CommunicationType;

    /// <summary>
    /// An AutoMapper profile class which defines mapping between DB Model Communication and API Model CommunicationModel.
    /// </summary>
    public class CommunicationProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationProfile"/> class.
        /// </summary>
        public CommunicationProfile()
        {
            this.CreateMap<Communication, CommunicationModel>();
            this.CreateMap<RequestResult<Communication?>, RequestResult<CommunicationModel?>>();

            this.CreateMap<Common.Data.Constants.CommunicationStatus, CommunicationStatus>()
                .ConvertUsing(
                    (source, _, _) => source switch
                    {
                        Common.Data.Constants.CommunicationStatus.New => CommunicationStatus.New,
                        Common.Data.Constants.CommunicationStatus.Error => CommunicationStatus.Error,
                        Common.Data.Constants.CommunicationStatus.Processed => CommunicationStatus.Processed,
                        Common.Data.Constants.CommunicationStatus.Pending => CommunicationStatus.Pending,
                        Common.Data.Constants.CommunicationStatus.Processing => CommunicationStatus.Processing,
                        Common.Data.Constants.CommunicationStatus.Draft => CommunicationStatus.Draft,
                        _ => throw new NotImplementedException($"Mapping for {source} is not implemented"),
                    });

            this.CreateMap<Common.Data.Constants.CommunicationType, CommunicationType>()
                .ConvertUsing(
                    (source, _, _) => source switch
                    {
                        Common.Data.Constants.CommunicationType.Banner => CommunicationType.Banner,
                        Common.Data.Constants.CommunicationType.Email => CommunicationType.Email,
                        Common.Data.Constants.CommunicationType.InApp => CommunicationType.InApp,
                        Common.Data.Constants.CommunicationType.Mobile => CommunicationType.Mobile,
                        _ => throw new NotImplementedException($"Mapping for {source} is not implemented"),
                    });

            this.CreateMap<CommunicationType, Common.Data.Constants.CommunicationType>()
                .ConvertUsing(
                    (source, _, _) => source switch
                    {
                        CommunicationType.Banner => Common.Data.Constants.CommunicationType.Banner,
                        CommunicationType.Email => Common.Data.Constants.CommunicationType.Email,
                        CommunicationType.InApp => Common.Data.Constants.CommunicationType.InApp,
                        CommunicationType.Mobile => Common.Data.Constants.CommunicationType.Mobile,
                        _ => throw new NotImplementedException($"Reverse mapping for {source} is not implemented"),
                    });
        }
    }
}
