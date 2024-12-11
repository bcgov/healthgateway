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
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Admin.Server.Models.Immunization;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using BetaFeatureAccess = HealthGateway.Database.Models.BetaFeatureAccess;
    using Communication = HealthGateway.Admin.Common.Models.Communication;

    /// <inheritdoc/>
    public class AdminServerMappingService(IMapper mapper, IConfiguration configuration) : IAdminServerMappingService
    {
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

            // change from UTC to local time using local time offset to ensure daylight savings time is applied correctly.
            // note that this model is not returned in any API calls; it is used to populate a CSV with values in local time
            TimeSpan localTimeOffset = DateFormatter.GetLocalTimeOffset(configuration, DateTime.UtcNow);
            dest.LastLoginDateTime = source.LastLoginDateTime.AddMinutes(localTimeOffset.TotalMinutes);

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
        public BetaFeatureAccess MapToBetaFeatureAccess(string hdid, BetaFeature betaFeature)
        {
            return new()
            {
                Hdid = hdid,
                BetaFeatureCode = this.MapToBetaFeature(betaFeature),
            };
        }

        /// <inheritdoc/>
        public UserBetaAccess MapToUserBetaAccess(string email, IEnumerable<Database.Constants.BetaFeature> betaFeatures)
        {
            return new()
            {
                Email = email,
                BetaFeatures = new HashSet<BetaFeature>(betaFeatures.Select(this.MapToBetaFeature)),
            };
        }

        /// <inheritdoc/>
        public Database.Constants.BetaFeature MapToBetaFeature(BetaFeature source)
        {
            return mapper.Map<BetaFeature, Database.Constants.BetaFeature>(source);
        }

        /// <inheritdoc/>
        public BetaFeature MapToBetaFeature(Database.Constants.BetaFeature source)
        {
            return mapper.Map<Database.Constants.BetaFeature, BetaFeature>(source);
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
        public DelegateInfo MapToDelegateInfo(PatientModel source)
        {
            return mapper.Map<PatientModel, DelegateInfo>(source);
        }

        /// <inheritdoc/>
        public DependentInfo MapToDependentInfo(PatientModel source)
        {
            return mapper.Map<PatientModel, DependentInfo>(source);
        }

        /// <inheritdoc/>
        public PatientSupportDependentInfo MapToPatientSupportDependentInfo(AccountDataAccess.Patient.PatientModel source)
        {
            return mapper.Map<AccountDataAccess.Patient.PatientModel, PatientSupportDependentInfo>(source);
        }

        /// <inheritdoc/>
        public PatientSupportResult MapToPatientSupportResult(AccountDataAccess.Patient.PatientModel? source, UserProfile? userProfile)
        {
            PatientSupportResult dest = mapper.Map<AccountDataAccess.Patient.PatientModel?, PatientSupportResult>(source) ?? new PatientSupportResult();

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
