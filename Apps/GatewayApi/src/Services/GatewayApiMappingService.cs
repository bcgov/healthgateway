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
    using AutoMapper;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Models.Phsa;
    using CommunicationType = HealthGateway.GatewayApi.Constants.CommunicationType;

    /// <inheritdoc/>
    public class GatewayApiMappingService(IMapper mapper, ICryptoDelegate cryptoDelegate) : IGatewayApiMappingService
    {
        /// <inheritdoc/>
        public Comment MapToComment(UserComment source, string encryptionKey)
        {
            Comment dest = mapper.Map<UserComment, Comment>(source);

            dest.Text = !string.IsNullOrEmpty(source.Text) ? cryptoDelegate.Encrypt(encryptionKey, source.Text) : string.Empty;

            return dest;
        }

        /// <inheritdoc/>
        public Common.Data.Constants.CommunicationType MapToCommunicationType(CommunicationType source)
        {
            return mapper.Map<CommunicationType, Common.Data.Constants.CommunicationType>(source);
        }

        /// <inheritdoc/>
        public DependentModel MapToDependentModel(ResourceDelegate source, PatientModel patientModel, int totalDelegateCount)
        {
            DependentModel dest = mapper.Map<ResourceDelegate, DependentModel>(source);

            dest.DependentInformation = mapper.Map<PatientModel, DependentInformation>(patientModel);
            dest.TotalDelegateCount = totalDelegateCount;

            return dest;
        }

        /// <inheritdoc/>
        public Note MapToNote(UserNote source, string encryptionKey)
        {
            Note dest = mapper.Map<UserNote, Note>(source);

            dest.Title = !string.IsNullOrEmpty(source.Title) ? cryptoDelegate.Encrypt(encryptionKey, source.Title) : string.Empty;
            dest.Text = !string.IsNullOrEmpty(source.Text) ? cryptoDelegate.Encrypt(encryptionKey, source.Text) : string.Empty;

            return dest;
        }

        /// <inheritdoc/>
        public PatientDetails MapToPatientDetails(AccountDataAccess.Patient.PatientModel source)
        {
            return mapper.Map<AccountDataAccess.Patient.PatientModel, PatientDetails>(source);
        }

        /// <inheritdoc/>
        public Rating MapToRating(SubmitRating source)
        {
            return mapper.Map<SubmitRating, Rating>(source);
        }

        /// <inheritdoc/>
        public RatingModel MapToRatingModel(Rating source)
        {
            return mapper.Map<Rating, RatingModel>(source);
        }

        /// <inheritdoc/>
        public RequestResult<CommunicationModel> MapToRequestResult(RequestResult<Communication?> source)
        {
            return mapper.Map<RequestResult<Communication?>, RequestResult<CommunicationModel>>(source);
        }

        /// <inheritdoc/>
        public TermsOfServiceModel MapToTermsOfServiceModel(LegalAgreement source)
        {
            return mapper.Map<LegalAgreement, TermsOfServiceModel>(source);
        }

        /// <inheritdoc/>
        public UserComment MapToUserComment(Comment source, string decryptionKey)
        {
            UserComment dest = mapper.Map<Comment, UserComment>(source);

            dest.Text = dest.Text = !string.IsNullOrEmpty(source.Text) ? cryptoDelegate.Decrypt(decryptionKey, source.Text) : string.Empty;

            return dest;
        }

        /// <inheritdoc/>
        public UserFeedback MapToUserFeedback(Feedback source, string hdid)
        {
            UserFeedback dest = mapper.Map<Feedback, UserFeedback>(source);

            dest.UserProfileId = hdid;
            dest.CreatedBy = hdid;
            dest.UpdatedBy = hdid;

            return dest;
        }

        /// <inheritdoc/>
        public UserNote MapToUserNote(Note source, string decryptionKey)
        {
            UserNote dest = mapper.Map<Note, UserNote>(source);

            dest.Title = !string.IsNullOrEmpty(source.Title) ? cryptoDelegate.Decrypt(decryptionKey, source.Title) : string.Empty;
            dest.Text = !string.IsNullOrEmpty(source.Text) ? cryptoDelegate.Decrypt(decryptionKey, source.Text) : string.Empty;

            return dest;
        }

        /// <inheritdoc/>
        public UserPreference MapToUserPreference(UserPreferenceModel source)
        {
            return mapper.Map<UserPreferenceModel, UserPreference>(source);
        }

        /// <inheritdoc/>
        public UserPreferenceModel MapToUserPreferenceModel(UserPreference source)
        {
            return mapper.Map<UserPreference, UserPreferenceModel>(source);
        }

        /// <inheritdoc/>
        public UserProfileModel MapToUserProfileModel(UserProfile source, Guid? latestTermsOfServiceId)
        {
            UserProfileModel? dest = mapper.Map<UserProfile, UserProfileModel>(source);

            dest.HasTermsOfServiceUpdated = source.TermsOfServiceId != latestTermsOfServiceId;

            return dest;
        }

        /// <inheritdoc/>
        public WebAlert MapToWebAlert(PhsaWebAlert source)
        {
            return mapper.Map<PhsaWebAlert, WebAlert>(source);
        }

        /// <inheritdoc/>
        public IList<UserProfileNotificationSettingModel> MapToUserProfileNotificationSettingModels(
            IReadOnlyList<UserProfileNotificationSetting> source)
        {
            return mapper.Map<IReadOnlyList<UserProfileNotificationSetting>, IList<UserProfileNotificationSettingModel>>(source);
        }
    }
}
