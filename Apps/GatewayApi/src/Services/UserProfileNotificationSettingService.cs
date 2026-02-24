//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.GatewayApi.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;

    /// <inheritdoc/>
    /// <param name="profileDelegate">The injected user profile delelgate.</param>
    /// <param name="notificationSettingDelegate">The injected user profile notification setting delegate.</param>
    /// <param name="mappingService">The injected gateway api mapping service.</param>
    /// <param name="messageSender">The injected message sender.</param>
    public class UserProfileNotificationSettingService(
        IUserProfileDelegate profileDelegate,
        IUserProfileNotificationSettingDelegate notificationSettingDelegate,
        IGatewayApiMappingService mappingService,
        IMessageSender messageSender)
        : IUserProfileNotificationSettingService
    {
        /// <inheritdoc/>
        public async Task<IList<UserProfileNotificationSettingModel>> GetAsync(
            string hdid,
            CancellationToken ct = default)
        {
            IReadOnlyList<UserProfileNotificationSetting> notificationSettings =
                await notificationSettingDelegate.GetAsync(hdid, ct);
            return mappingService.MapToUserProfileNotificationSettingModels(notificationSettings);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(
            string hdid,
            UserProfileNotificationSettingModel model,
            CancellationToken ct = default)
        {
            UserProfile userProfile = await profileDelegate.GetUserProfileAsync(hdid, ct: ct) ?? throw new NotFoundException($"User profile not found for hdid {hdid}");

            IReadOnlyList<UserProfileNotificationSetting> existing =
                await notificationSettingDelegate.GetAsync(hdid, ct);

            UserProfileNotificationSetting? setting =
                existing.SingleOrDefault(x => x.NotificationTypeCode == model.Type);

            if (setting is null)
            {
                setting = new UserProfileNotificationSetting
                {
                    Hdid = hdid,
                    NotificationTypeCode = model.Type,
                    EmailEnabled = model.EmailEnabled,
                    SmsEnabled = model.SmsEnabled,
                };
            }
            else
            {
                setting.EmailEnabled = model.EmailEnabled;
                setting.SmsEnabled = model.SmsEnabled;
            }

            IReadOnlyCollection<NotificationTargets> emailNotificationTargets =
                model.EmailEnabled
                    ? new[] { NotificationTargets.BcCancer }
                    : [];

            IReadOnlyCollection<NotificationTargets> smsNotificationTargets =
                model.SmsEnabled
                    ? new[] { NotificationTargets.BcCancer }
                    : [];

            MessageEnvelope[] events =
                [new(new NotificationChannelPreferencesChangedEvent(hdid, userProfile.SmsNumber, smsNotificationTargets, userProfile.Email, emailNotificationTargets), hdid)];

            await notificationSettingDelegate.UpdateAsync(setting, false, ct);

            await messageSender.SendAsync(events, ct);
        }
    }
}
