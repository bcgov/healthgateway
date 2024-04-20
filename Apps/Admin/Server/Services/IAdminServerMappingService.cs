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
    using System.Collections.Generic;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Admin.Server.Models.Immunization;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using BetaFeatureAccess = HealthGateway.Database.Models.BetaFeatureAccess;
    using Communication = HealthGateway.Admin.Common.Models.Communication;

    /// <summary>
    /// Service to map between models at different layers.
    /// </summary>
    public interface IAdminServerMappingService
    {
        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        AdminTag MapToAdminTag(AdminTagView source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        AdminTagView MapToAdminTagView(AdminTag source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        AdminUserProfileView MapToAdminUserProfileView(AdminUserProfile source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        AdminUserProfileView MapToAdminUserProfileView(UserRepresentation source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        AgentAction MapToAgentAction(AgentAudit source);

        /// <summary>Maps model.</summary>
        /// <param name="hdid">The hdid to associate the beta feature with.</param>
        /// <param name="betaFeature">The beta feature available to the hdid.</param>
        /// <returns>The destination object.</returns>
        BetaFeatureAccess MapToBetaFeatureAccess(string hdid, BetaFeature betaFeature);

        /// <summary>Maps model.</summary>
        /// <param name="email">The email to associate the beta feature with.</param>
        /// <param name="betaFeatures">The collection of beta feature available to the email.</param>
        /// <returns>The destination object.</returns>
        UserBetaAccess MapToUserBetaAccess(string email, IEnumerable<Database.Constants.BetaFeature> betaFeatures);

        /// <summary>Maps enum.</summary>
        /// <param name="source">The beta feature to convert.</param>
        /// <returns>The destination object.</returns>
        Database.Constants.BetaFeature MapToBetaFeature(BetaFeature source);

        /// <summary>Maps enum.</summary>
        /// <param name="source">The beta feature to convert.</param>
        /// <returns>The destination object.</returns>
        BetaFeature MapToBetaFeature(Database.Constants.BetaFeature source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        Communication MapToCommonCommunication(Database.Models.Communication source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        Database.Models.Communication MapToDatabaseCommunication(Communication source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        DelegateInfo MapToDelegateInfo(PatientModel source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        DependentInfo MapToDependentInfo(PatientModel source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        PatientSupportDependentInfo MapToPatientSupportDependentInfo(AccountDataAccess.Patient.PatientModel source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="userProfile">The user profile associated with the object.</param>
        /// <returns>The destination object.</returns>
        PatientSupportResult MapToPatientSupportResult(AccountDataAccess.Patient.PatientModel? source, UserProfile? userProfile);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        UserFeedback MapToUserFeedback(UserFeedbackView source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="email">The email address associated with the person who submitted the feedback.</param>
        /// <returns>The destination object.</returns>
        UserFeedbackView MapToUserFeedbackView(UserFeedback source, string email);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        VaccineDose MapToVaccineDose(VaccineDoseResponse source);
    }
}
