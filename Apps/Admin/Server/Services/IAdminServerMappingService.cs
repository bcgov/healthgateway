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
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Admin.Server.Models.Immunization;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Models;
    using Communication = HealthGateway.Database.Models.Communication;

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
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        Common.Models.Communication MapToCommonCommunication(Communication source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        Communication MapToDatabaseCommunication(Common.Models.Communication source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        DelegateInfo MapToDelegateInfo(HealthGateway.Common.Models.PatientModel source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        DependentInfo MapToDependentInfo(HealthGateway.Common.Models.PatientModel source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        PatientSupportDependentInfo MapToPatientSupportDependentInfo(PatientModel source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="userProfile">The user profile associated with the object.</param>
        /// <returns>The destination object.</returns>
        PatientSupportResult MapToPatientSupportResult(PatientModel? source, UserProfile? userProfile);

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
