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
namespace HealthGateway.Admin.Server.Services
{
    using System;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Admin.Server.Models.Immunization;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Communication = HealthGateway.Admin.Common.Models.Communication;

    /// <inheritdoc/>
    public class AdminServerMappingService(IMapper mapper, IConfiguration configuration) : IAdminServerMappingService
    {
        private TimeZoneInfo LocalTimeZone => DateFormatter.GetLocalTimeZone(configuration);

        /// <inheritdoc/>
        public AdminTag MapToAdminTag(AdminTagView source)
        {
            return mapper.Map<AdminTagView, AdminTag>(source);
        }

        /// <inheritdoc/>
        public AdminTagView MapToAdminTagView(AdminTag source)
        {
            return mapper.Map<AdminTag, AdminTagView>(source);
        }

        /// <inheritdoc/>
        public AdminUserProfileView MapToAdminUserProfileView(AdminUserProfile source)
        {
            AdminUserProfileView dest = mapper.Map<AdminUserProfile, AdminUserProfileView>(source);

            if (dest.LastLoginDateTime != null)
            {
                dest.LastLoginDateTime = TimeZoneInfo.ConvertTimeFromUtc(dest.LastLoginDateTime.Value, this.LocalTimeZone);
            }

            return dest;
        }

        /// <inheritdoc/>
        public AdminUserProfileView MapToAdminUserProfileView(UserRepresentation source)
        {
            return mapper.Map<UserRepresentation, AdminUserProfileView>(source);
        }

        /// <inheritdoc/>
        public AgentAction MapToAgentAction(AgentAudit source)
        {
            return mapper.Map<AgentAudit, AgentAction>(source);
        }

        /// <inheritdoc/>
        public Communication MapToCommonCommunication(Database.Models.Communication source)
        {
            return mapper.Map<Database.Models.Communication, Communication>(source);
        }

        /// <inheritdoc/>
        public Database.Models.Communication MapToDatabaseCommunication(Communication source)
        {
            return mapper.Map<Communication, Database.Models.Communication>(source);
        }

        /// <inheritdoc/>
        public DelegateInfo MapToDelegateInfo(HealthGateway.Common.Models.PatientModel source)
        {
            return mapper.Map<HealthGateway.Common.Models.PatientModel, DelegateInfo>(source);
        }

        /// <inheritdoc/>
        public DependentInfo MapToDependentInfo(HealthGateway.Common.Models.PatientModel source)
        {
            return mapper.Map<HealthGateway.Common.Models.PatientModel, DependentInfo>(source);
        }

        /// <inheritdoc/>
        public PatientSupportDependentInfo MapToPatientSupportDependentInfo(PatientModel source)
        {
            return mapper.Map<PatientModel, PatientSupportDependentInfo>(source);
        }

        /// <inheritdoc/>
        public PatientSupportResult MapToPatientSupportResult(PatientModel? source, UserProfile? userProfile)
        {
            PatientSupportResult dest = mapper.Map<PatientModel?, PatientSupportResult>(source) ?? new PatientSupportResult();

            dest.Status = PatientStatus.Default;
            if (source == null)
            {
                dest.Status = PatientStatus.NotFound;
            }
            else if (source.IsDeceased == true)
            {
                dest.Status = PatientStatus.Deceased;
            }
            else if (userProfile == null)
            {
                dest.Status = PatientStatus.NotUser;
            }

            dest.ProfileCreatedDateTime = userProfile?.CreatedDateTime;
            dest.ProfileLastLoginDateTime = userProfile?.LastLoginDateTime;

            if (source == null && userProfile != null)
            {
                dest.Hdid = userProfile.HdId;
            }

            return dest;
        }

        /// <inheritdoc/>
        public UserFeedbackView MapToUserFeedbackView(UserFeedback source, string email)
        {
            UserFeedbackView dest = mapper.Map<UserFeedback, UserFeedbackView>(source);

            dest.Email = email;

            return dest;
        }

        /// <inheritdoc/>
        public UserFeedback MapToUserFeedback(UserFeedbackView source)
        {
            return mapper.Map<UserFeedbackView, UserFeedback>(source);
        }

        /// <inheritdoc/>
        public VaccineDose MapToVaccineDose(VaccineDoseResponse source)
        {
            return mapper.Map<VaccineDoseResponse, VaccineDose>(source);
        }
    }
}
