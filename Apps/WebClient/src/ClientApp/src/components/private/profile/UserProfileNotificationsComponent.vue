<script setup lang="ts">
import { computed, reactive, ref, watch } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import InfoTooltipComponent from "@/components/common/InfoTooltipComponent.vue";
import SectionHeaderComponent from "@/components/common/SectionHeaderComponent.vue";
import {
    getNotificationPreferenceTypes,
    getUserProfileNotificationSettings,
    getUserProfileNotificationType,
    ProfileNotificationPreference,
    ProfileNotificationType,
} from "@/constants/profileNotifications";
import { NotificationPreference } from "@/models/notificationPreference";
import { UserProfileNotificationSettingModel } from "@/models/userProfile";
import { UserProfileNotificationSettings } from "@/models/userProfileNotificationSettings";
import { useUserStore } from "@/stores/user";
import ConfigUtil from "@/utility/configUtil";

type NotificationChannel = "email" | "sms";

const userStore = useUserStore();

const saveError = ref<string | null>(null);

const email = computed(() => userStore.user.email);
const emailVerified = computed(() => userStore.user.verifiedEmail);
const sms = computed(() => userStore.user.sms);
const smsVerified = computed(() => userStore.user.verifiedSms);

const isEmailChannelDisabled = computed(
    () => !email.value || !emailVerified.value
);

const isSmsChannelDisabled = computed(() => !sms.value || !smsVerified.value);

const uiNotificationSettings = computed<UserProfileNotificationSettings[]>(() =>
    getUserProfileNotificationSettings(userStore.user.notificationSettings)
);

const notificationPreferencesByType = computed(
    () =>
        new Map(
            uiNotificationSettings.value.map((setting) => [
                setting.type,
                setting.preferences ?? [],
            ])
        )
);

function isNotificationTypeEnabled(type: ProfileNotificationType): boolean {
    return ConfigUtil.isProfileNotificationTypeEnabled(type);
}

const channelState = reactive<
    Record<string, { emailEnabled: boolean; smsEnabled: boolean }>
>({});

const enabledNotificationPreferences = computed<NotificationPreference[]>(() =>
    getNotificationPreferenceTypes()
        .filter((definition) => isNotificationTypeEnabled(definition.type))
        .map((definition) => {
            if (!channelState[definition.id]) {
                const enabledPreferences =
                    notificationPreferencesByType.value.get(definition.type) ??
                    [];
                channelState[definition.id] = {
                    emailEnabled: enabledPreferences.includes(
                        ProfileNotificationPreference.Email
                    ),
                    smsEnabled: enabledPreferences.includes(
                        ProfileNotificationPreference.Sms
                    ),
                };
            }

            return {
                ...definition,
                enabled: true,
            };
        })
);

const hasEnabledNotificationPreferences = computed(
    () => enabledNotificationPreferences.value.length > 0
);

const showNotificationSection = computed(() =>
    ConfigUtil.isProfileNotificationsFeatureEnabled()
);

const showEmailColumn = computed(() =>
    enabledNotificationPreferences.value.some((pref) =>
        ConfigUtil.isProfileNotificationPreferenceEnabled(
            pref.type,
            ProfileNotificationPreference.Email
        )
    )
);

const showSmsColumn = computed(() =>
    enabledNotificationPreferences.value.some((pref) =>
        ConfigUtil.isProfileNotificationPreferenceEnabled(
            pref.type,
            ProfileNotificationPreference.Sms
        )
    )
);

const hasAnyChannelEnabled = computed(
    () => showEmailColumn.value || showSmsColumn.value
);

function buildNotificationPreferences(
    rowId: string
): ProfileNotificationPreference[] {
    const state = channelState[rowId];
    const preferences: ProfileNotificationPreference[] = [];

    if (state?.emailEnabled) {
        preferences.push(ProfileNotificationPreference.Email);
    }

    if (state?.smsEnabled) {
        preferences.push(ProfileNotificationPreference.Sms);
    }

    return preferences;
}

async function handleChannelToggle(
    item: NotificationPreference,
    channel: NotificationChannel,
    newValue: boolean | null
): Promise<void> {
    const normalizedValue: boolean = newValue ?? false;

    saveError.value = null;

    const state = channelState[item.id];
    if (!state) {
        return;
    }

    // Because v-model may already have applied newValue by the time this runs,
    // compute the "previous" value using newValue.
    const previousEmailEnabled =
        channel === "email" ? !normalizedValue : state.emailEnabled;
    const previousSmsEnabled =
        channel === "sms" ? !normalizedValue : state.smsEnabled;

    // Ensure state reflects the intended new value
    if (channel === "email") {
        state.emailEnabled = normalizedValue;
    } else {
        state.smsEnabled = normalizedValue;
    }

    const preferences: ProfileNotificationPreference[] =
        buildNotificationPreferences(item.id);

    try {
        await saveNotificationPreferences(item.type, preferences);
    } catch {
        state.emailEnabled = previousEmailEnabled;
        state.smsEnabled = previousSmsEnabled;

        saveError.value =
            "An unexpected error has occurred. Please try again later.";
    }
}

async function saveNotificationPreferences(
    type: ProfileNotificationType,
    preferences: ProfileNotificationPreference[]
): Promise<void> {
    const notificationSetting: UserProfileNotificationSettingModel = {
        type: getUserProfileNotificationType(type), // "bcCancerScreening" to "BcCancerScreening"
        emailEnabled: preferences.includes(ProfileNotificationPreference.Email),
        smsEnabled: preferences.includes(ProfileNotificationPreference.Sms),
    };

    await userStore.updateNotificationSettings(notificationSetting);
}

watch(isEmailChannelDisabled, (disabled) => {
    if (disabled) {
        Object.values(channelState).forEach(
            (state) => (state.emailEnabled = false)
        );
    }
});

watch(isSmsChannelDisabled, (disabled) => {
    if (disabled) {
        Object.values(channelState).forEach(
            (state) => (state.smsEnabled = false)
        );
    }
});
</script>
<template>
    <div v-if="showNotificationSection" class="mb-4">
        <SectionHeaderComponent
            title="Notifications"
            data-testid="profile-notification-preferences-label"
        />
        <p
            class="mt-2"
            data-testid="profile-notification-preferences-verify-note"
        >
            You must verify your email address and cell number to receive
            notifications.
        </p>
        <HgAlertComponent
            v-if="saveError"
            data-testid="profile-notification-preferences-save-error"
            class="d-print-none pa-0"
            variant="text"
            type="error"
        >
            {{ saveError }}
        </HgAlertComponent>
        <v-divider class="my-4" />
        <v-container class="pa-0">
            <template v-if="hasEnabledNotificationPreferences">
                <v-row
                    no-gutters
                    class="font-weight-bold mb-2"
                    data-testid="profile-notification-preferences-header"
                >
                    <v-col
                        :cols="hasAnyChannelEnabled ? 8 : 12"
                        :class="['py-0', hasAnyChannelEnabled ? 'pe-2' : '']"
                        >Notification Type</v-col
                    >
                    <v-col
                        v-if="showEmailColumn"
                        :cols="showSmsColumn ? 2 : 4"
                        class="py-0 text-center"
                        >Email</v-col
                    >
                    <v-col
                        v-if="showSmsColumn"
                        :cols="showEmailColumn ? 2 : 4"
                        class="py-0 text-center"
                        >Text (SMS)</v-col
                    >
                </v-row>

                <v-row
                    v-for="item in enabledNotificationPreferences"
                    :key="item.id"
                    no-gutters
                    align="center"
                    :data-testid="`profile-notification-preferences-row-${item.id}`"
                >
                    <v-col
                        :cols="hasAnyChannelEnabled ? 8 : 12"
                        :class="['py-0', hasAnyChannelEnabled ? 'pe-2' : '']"
                    >
                        <div class="d-flex align-center">
                            <span>{{ item.label }}</span>
                            <InfoTooltipComponent
                                :tooltip-testid="`info-tooltip-${item.id}`"
                                class="ml-2"
                                size="x-small"
                                content-class="bg-primary text-white"
                            >
                                <p>
                                    {{ item.tooltip }}
                                </p>
                            </InfoTooltipComponent>
                        </div>
                    </v-col>
                    <v-col
                        v-if="showEmailColumn"
                        :cols="showSmsColumn ? 2 : 4"
                        class="py-0 d-flex justify-center"
                    >
                        <v-switch
                            v-model="channelState[item.id].emailEnabled"
                            color="primary"
                            class="hg-switch"
                            density="compact"
                            hide-details
                            inset
                            :disabled="isEmailChannelDisabled"
                            @update:model-value="
                                handleChannelToggle(item, 'email', $event)
                            "
                        />
                    </v-col>
                    <v-col
                        v-if="showSmsColumn"
                        :cols="showEmailColumn ? 2 : 4"
                        class="py-0 d-flex justify-center"
                    >
                        <v-switch
                            v-model="channelState[item.id].smsEnabled"
                            color="primary"
                            class="hg-switch"
                            density="compact"
                            hide-details
                            inset
                            :disabled="isSmsChannelDisabled"
                            @update:model-value="
                                handleChannelToggle(item, 'sms', $event)
                            "
                        />
                    </v-col>
                </v-row>
            </template>

            <p
                v-else
                class="mb-0"
                data-testid="profile-notification-preferences-empty"
            >
                No notification options are currently available.
            </p>
        </v-container>

        <v-divider class="my-4" />
    </div>
</template>

<style lang="scss" scoped>
:deep(.hg-switch .v-switch__track.bg-primary) {
    opacity: 1 !important;
}
</style>
