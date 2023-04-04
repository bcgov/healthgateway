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
namespace HealthGateway.Patient.Mappings
{
    using AutoMapper;
    using HealthGateway.PatientDataAccess;
    using OrganDonorRegistrationStatus = HealthGateway.Patient.Models.OrganDonorRegistrationStatus;

    /// <summary>
    /// The AutoMapper profile for the Organ Donor Registration module.
    /// </summary>
    public class OrganDonorRegistrationProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganDonorRegistrationProfile"/> class.
        /// </summary>
        public OrganDonorRegistrationProfile()
        {
            this.CreateMap<OrganDonorRegistration, Services.OrganDonorRegistration>()
                .ForMember(
                    d => d.Status,
                    opts => opts.MapFrom(s => MapOrganDonorStatus(s.Status)));
        }

        private static OrganDonorRegistrationStatus MapOrganDonorStatus(PatientDataAccess.OrganDonorRegistrationStatus status)
        {
            return status switch
            {
                PatientDataAccess.OrganDonorRegistrationStatus.Registered => OrganDonorRegistrationStatus.Registered,
                PatientDataAccess.OrganDonorRegistrationStatus.NotRegistered => OrganDonorRegistrationStatus.NotRegistered,
                PatientDataAccess.OrganDonorRegistrationStatus.Error => OrganDonorRegistrationStatus.Error,
                PatientDataAccess.OrganDonorRegistrationStatus.Pending => OrganDonorRegistrationStatus.Pending,
                _ => OrganDonorRegistrationStatus.Unknown,
            };
        }
    }
}
