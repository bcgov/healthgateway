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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;

    /// <inheritdoc/>
    /// <param name="profileDelegate">The injected user profile delegate.</param>
    /// <param name="notificationSettingDelegate">The injected user profile notification setting delegate.</param>
    /// <param name="mappingService">The injected gateway api mapping service.</param>
    /// <param name="outboxStore">The outbox store to use.</param>
    public class UserProfileNotificationSettingService(
        IUserProfileDelegate profileDelegate,
        IUserProfileNotificationSettingDelegate notificationSettingDelegate,
        IGatewayApiMappingService mappingService,
        IOutboxStore outboxStore)
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

            UserProfileNotificationSetting setting =
                existing.SingleOrDefault(x => x.NotificationType == model.Type) ?? new UserProfileNotificationSetting
                {
                    Hdid = hdid,
                    NotificationType = model.Type,
                };

            // Update email preference only when an email value is provided.
            if (model.EmailEnabled.HasValue)
            {
                setting.EmailEnabled = model.EmailEnabled.Value;
            }

            // Update SMS preference only when an SMS value is provided.
            if (model.SmsEnabled.HasValue)
            {
                setting.SmsEnabled = model.SmsEnabled.Value;
            }

            IReadOnlyCollection<NotificationTargets> emailNotificationTargets =
                model.EmailEnabled.HasValue
                    ? GetTargets(model.Type, model.EmailEnabled.Value, !string.IsNullOrEmpty(userProfile.Email))
                    : [];

            IReadOnlyCollection<NotificationTargets> smsNotificationTargets =
                model.SmsEnabled.HasValue
                    ? GetTargets(model.Type, model.SmsEnabled.Value, !string.IsNullOrEmpty(userProfile.SmsNumber))
                    : [];

            MessageEnvelope[] events =
                [new(new NotificationChannelPreferencesChangedEvent(hdid, userProfile.SmsNumber, smsNotificationTargets, userProfile.Email, emailNotificationTargets), hdid)];

            await notificationSettingDelegate.UpdateAsync(setting, false, ct);
            await outboxStore.StoreAsync(events, ct: ct);
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(
            string hdid,
            IReadOnlyCollection<UserProfileNotificationSettingModel> models,
            bool commit = true,
            CancellationToken ct = default)
        {
            UserProfile userProfile = await profileDelegate.GetUserProfileAsync(hdid, ct: ct)
                                      ?? throw new NotFoundException($"User profile not found for hdid {hdid}");

            IReadOnlyList<UserProfileNotificationSetting> existing =
                await notificationSettingDelegate.GetAsync(hdid, ct);

            List<NotificationTargets> emailNotificationTargets = [];
            List<NotificationTargets> smsNotificationTargets = [];

            foreach (UserProfileNotificationSettingModel model in models)
            {
                UserProfileNotificationSetting setting =
                    existing.SingleOrDefault(x => x.NotificationType == model.Type) ?? new UserProfileNotificationSetting
                    {
                        Hdid = hdid,
                        NotificationType = model.Type,
                    };

                // Use provided email value when present.
                if (model.EmailEnabled.HasValue)
                {
                    setting.EmailEnabled = model.EmailEnabled.Value;

                    emailNotificationTargets.AddRange(
                        GetTargets(model.Type, model.EmailEnabled.Value, !string.IsNullOrEmpty(userProfile.Email)));
                }

                // Otherwise use existing email value when available.
                if (!model.EmailEnabled.HasValue && setting.EmailEnabled.HasValue)
                {
                    emailNotificationTargets.AddRange(
                        GetTargets(model.Type, setting.EmailEnabled.Value, !string.IsNullOrEmpty(userProfile.Email)));
                }

                // Use provided SMS value when present.
                if (model.SmsEnabled.HasValue)
                {
                    setting.SmsEnabled = model.SmsEnabled.Value;

                    smsNotificationTargets.AddRange(
                        GetTargets(model.Type, model.SmsEnabled.Value, !string.IsNullOrEmpty(userProfile.SmsNumber)));
                }

                // Otherwise use existing SMS value when available.
                if (!model.SmsEnabled.HasValue && setting.SmsEnabled.HasValue)
                {
                    smsNotificationTargets.AddRange(
                        GetTargets(model.Type, setting.SmsEnabled.Value, !string.IsNullOrEmpty(userProfile.SmsNumber)));
                }

                await notificationSettingDelegate.UpdateAsync(setting, false, ct);
            }

            MessageEnvelope[] events =
            [
                new(
                    new NotificationChannelPreferencesChangedEvent(
                        hdid,
                        userProfile.SmsNumber,
                        smsNotificationTargets,
                        userProfile.Email,
                        emailNotificationTargets),
                    hdid),
            ];

            // When commit is false, the caller is responsible for saving changes
            // and committing any active transaction.
            await outboxStore.StoreAsync(events, commit, ct);
        }

        /// <summary>
        /// Resolves the notification targets for a given profile notification type and channel state.
        /// </summary>
        /// <param name="type">The profile notification type.</param>
        /// <param name="enabled">Indicates whether the notification channel is enabled.</param>
        /// <param name="hasChannelValue">
        /// Indicates whether the user has a valid value for the channel (e.g., email address or SMS number).
        /// </param>
        /// <returns>
        /// A collection containing the resolved <see cref="NotificationTargets"/> when enabled and valid;
        /// otherwise, an empty collection.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the provided notification type is not supported.
        /// </exception>
        private static IReadOnlyCollection<NotificationTargets> GetTargets(
            ProfileNotificationType type,
            bool enabled,
            bool hasChannelValue)
        {
            if (!enabled || !hasChannelValue)
            {
                return [];
            }

            NotificationTargets target = type switch
            {
                ProfileNotificationType.BcCancerScreening => NotificationTargets.BcCancer,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported notification type"),
            };

            return [target];
        }
    }
}
