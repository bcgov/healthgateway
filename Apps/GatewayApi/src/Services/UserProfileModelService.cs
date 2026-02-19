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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;

    /// <inheritdoc/>
    /// <param name="applicationSettingsService">The injected application settings service.</param>
    /// <param name="legalAgreementService">The injected legal agreement service.</param>
    /// <param name="mappingService">The injected mapping service.</param>
    /// <param name="messageVerificationDelegate">The injected message verification delegate.</param>
    /// <param name="patientRepository">The injected patient repository.</param>
    /// <param name="userPreferenceService">The injected user preference service.</param>
    /// <param name="userProfileDelegate">The injected user profile database delegate.</param>
    /// <param name="notificationSettingService">The injected user profile notification setting service.</param>
#pragma warning disable S107 // The number of DI parameters should be ignored
    public class UserProfileModelService(
        IApplicationSettingsService applicationSettingsService,
        ILegalAgreementServiceV2 legalAgreementService,
        IGatewayApiMappingService mappingService,
        IMessagingVerificationDelegate messageVerificationDelegate,
        IPatientRepository patientRepository,
        IUserPreferenceServiceV2 userPreferenceService,
        IUserProfileDelegate userProfileDelegate,
        IUserProfileNotificationSettingService notificationSettingService) : IUserProfileModelService
    {
        /// <inheritdoc/>
        public async Task<UserProfileModel> BuildUserProfileModelAsync(UserProfile userProfile, int userProfileHistoryRecordLimit, CancellationToken ct = default)
        {
            IList<UserProfileHistory> historyCollection = await userProfileDelegate.GetUserProfileHistoryListAsync(userProfile.HdId, userProfileHistoryRecordLimit, ct);

            string? emailAddress = !string.IsNullOrEmpty(userProfile.Email)
                ? userProfile.Email
                : (await messageVerificationDelegate.GetLastForUserAsync(userProfile.HdId, MessagingVerificationType.Email, ct))?.Email?.To;

            string? smsNumber = !string.IsNullOrEmpty(userProfile.SmsNumber)
                ? userProfile.SmsNumber
                : (await messageVerificationDelegate.GetLastForUserAsync(userProfile.HdId, MessagingVerificationType.Sms, ct))?.SmsNumber;

            Guid? termsOfServiceId = await legalAgreementService.GetActiveLegalAgreementId(LegalAgreementType.TermsOfService, ct);
            UserProfileModel userProfileModel = mappingService.MapToUserProfileModel(userProfile, termsOfServiceId);
            userProfileModel.Email = emailAddress;
            userProfileModel.SmsNumber = smsNumber;

            DateTime? latestTourChangeDateTime = await applicationSettingsService.GetLatestTourChangeDateTimeAsync(ct);
            userProfileModel.HasTourUpdated = historyCollection.Count != 0 &&
                                              latestTourChangeDateTime != null &&
                                              historyCollection.Max(x => x.LastLoginDateTime) < latestTourChangeDateTime;

            userProfileModel.BlockedDataSources = await patientRepository.GetDataSourcesAsync(userProfile.HdId, ct);
            userProfileModel.Preferences = await userPreferenceService.GetUserPreferencesAsync(userProfileModel.HdId, ct);
            userProfileModel.LastLoginDateTimes = [userProfile.LastLoginDateTime, .. historyCollection.Select(h => h.LastLoginDateTime)];

            userProfileModel.NotificationSettings = await notificationSettingService.GetAsync(userProfile.HdId, ct);

            return userProfileModel;
        }
    }
}
