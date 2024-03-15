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
namespace HealthGateway.Admin.Tests.Utils
{
    using AutoMapper;
    using HealthGateway.Admin.Server.MapProfiles;
    using HealthGateway.Common.MapProfiles;

    /// <summary>
    /// Static utility class to provide a fully initialized AutoMapper.
    /// NOTE: Any newly added profiles will have to be registered.
    /// </summary>
    public static class MapperUtil
    {
        /// <summary>
        /// Creates an AutoMapper.
        /// </summary>
        /// <returns>A configured AutoMapper.</returns>
        public static IMapper InitializeAutoMapper()
        {
            MapperConfiguration config = new(
                cfg =>
                {
                    cfg.AddProfile(new AddressProfile());
                    cfg.AddProfile(new AgentActionProfile());
                    cfg.AddProfile(new AdminUserProfileViewProfile());
                    cfg.AddProfile(new BroadcastProfile());
                    cfg.AddProfile(new DelegateInfoProfile());
                    cfg.AddProfile(new DependentInfoProfile());
                    cfg.AddProfile(new MessagingVerificationProfile());
                    cfg.AddProfile(new PatientSupportDependentInfoProfile());
                    cfg.AddProfile(new PatientSupportDetailsProfile());
                    cfg.AddProfile(new VaccineDoseProfile());
                    cfg.AddProfile(new CommunicationProfile());
                    cfg.AddProfile(new BetaFeatureAccessProfile());
                });

            return config.CreateMapper();
        }
    }
}
