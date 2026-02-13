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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Models.Phsa;
    using CommunicationType = HealthGateway.GatewayApi.Constants.CommunicationType;

    /// <summary>
    /// Service to map between models at different layers.
    /// </summary>
    public interface IGatewayApiMappingService
    {
        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="encryptionKey">The key to use when encrypting the text.</param>
        /// <returns>The destination object.</returns>
        Comment MapToComment(UserComment source, string encryptionKey);

        /// <summary>Maps enum.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        Common.Data.Constants.CommunicationType MapToCommunicationType(CommunicationType source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="patientModel">The associated patient model.</param>
        /// <param name="totalDelegateCount">The total number of delegates with access to the dependent.</param>
        /// <returns>The destination object.</returns>
        DependentModel MapToDependentModel(ResourceDelegate source, PatientModel patientModel, int totalDelegateCount);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="encryptionKey">The key to use when encrypting the text.</param>
        /// <returns>The destination object.</returns>
        Note MapToNote(UserNote source, string encryptionKey);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        PatientDetails MapToPatientDetails(AccountDataAccess.Patient.PatientModel source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        Rating MapToRating(SubmitRating source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        RatingModel MapToRatingModel(Rating source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        RequestResult<CommunicationModel> MapToRequestResult(RequestResult<Communication?> source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        TermsOfServiceModel MapToTermsOfServiceModel(LegalAgreement source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="decryptionKey">The key to use when decrypting the text.</param>
        /// <returns>The destination object.</returns>
        UserComment MapToUserComment(Comment source, string decryptionKey);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="hdid">The HDID of the user providing the feedback.</param>
        /// <returns>The destination object.</returns>
        UserFeedback MapToUserFeedback(Feedback source, string hdid);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="decryptionKey">The key to use when decrypting the text.</param>
        /// <returns>The destination object.</returns>
        UserNote MapToUserNote(Note source, string decryptionKey);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        UserPreference MapToUserPreference(UserPreferenceModel source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        UserPreferenceModel MapToUserPreferenceModel(UserPreference source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <param name="latestTermsOfServiceId">The ID of the latest terms of service, used to check if the terms have updated.</param>
        /// <returns>The destination object.</returns>
        UserProfileModel MapToUserProfileModel(UserProfile source, Guid? latestTermsOfServiceId);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        WebAlert MapToWebAlert(PhsaWebAlert source);

        /// <summary>Maps model.</summary>
        /// <param name="source">The source object to transform.</param>
        /// <returns>The destination object.</returns>
        IList<UserProfileNotificationSettingModel> MapToUserProfileNotificationSettingModels(
            IReadOnlyList<UserProfileNotificationSetting> source);
    }
}
