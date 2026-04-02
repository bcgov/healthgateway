<script setup lang="ts">
import { computed, reactive, ref, watch, watchEffect } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import InteractiveInfoTooltipComponent from "@/components/common/InteractiveInfoTooltipComponent.vue";
import SectionHeaderComponent from "@/components/common/SectionHeaderComponent.vue";
import {
    getNotificationPreferenceTypes,
    getUserProfileNotificationSettings,
    getUserProfileNotificationType,
    ProfileNotificationPreference,
    ProfileNotificationType,
} from "@/constants/profileNotifications";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { NotificationPreference } from "@/models/notificationPreference";
import { UserProfileNotificationSettingModel } from "@/models/userProfile";
import { UserProfileNotificationSettings } from "@/models/userProfileNotificationSettings";
import { ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";
import ConfigUtil from "@/utility/configUtil";

type NotificationChannel = "email" | "sms";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const errorStore = useErrorStore();
const userStore = useUserStore();

const saveError = ref<string | null>(null);

const savingState = reactive<Record<string, boolean>>({});

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

const channelState = reactive<
    Record<string, { emailEnabled: boolean; smsEnabled: boolean }>
>({});

const enabledNotificationPreferences = computed<NotificationPreference[]>(() =>
    getNotificationPreferenceTypes()
        .filter((definition) => isNotificationTypeEnabled(definition.type))
        .map((definition) => ({
            ...definition,
            enabled: true,
        }))
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

const requiresContactVerification = computed(() => {
    const emailNeedsAction =
        showEmailColumn.value && (!email.value || !emailVerified.value);

    const smsNeedsAction =
        showSmsColumn.value && (!sms.value || !smsVerified.value);

    return emailNeedsAction || smsNeedsAction;
});

const verificationMessage = computed(() => {
    if (
        requiresContactVerification.value &&
        showEmailColumn.value &&
        showSmsColumn.value
    ) {
        return "You must verify your email address and cell number to receive notifications.";
    }

    if (
        requiresContactVerification.value &&
        showEmailColumn.value &&
        !showSmsColumn.value
    ) {
        return "You must verify your email address to receive notifications.";
    }

    if (
        requiresContactVerification.value &&
        !showEmailColumn.value &&
        showSmsColumn.value
    ) {
        return "You must verify your cell number to receive notifications.";
    }

    return null;
});

function isNotificationTypeEnabled(type: ProfileNotificationType): boolean {
    return ConfigUtil.isProfileNotificationTypeEnabled(type);
}

function isEmailPreferenceEnabledForType(
    type: ProfileNotificationType
): boolean {
    return ConfigUtil.isProfileNotificationPreferenceEnabled(
        type,
        ProfileNotificationPreference.Email
    );
}

function isSmsPreferenceEnabledForType(type: ProfileNotificationType): boolean {
    return ConfigUtil.isProfileNotificationPreferenceEnabled(
        type,
        ProfileNotificationPreference.Sms
    );
}

function isEmailToggleDisabledForType(type: ProfileNotificationType): boolean {
    return (
        isEmailChannelDisabled.value || !isEmailPreferenceEnabledForType(type)
    );
}

function isSmsToggleDisabledForType(type: ProfileNotificationType): boolean {
    return isSmsChannelDisabled.value || !isSmsPreferenceEnabledForType(type);
}

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

function disableInvalidChannels(
    rowId: string,
    emailDisabled: boolean,
    smsDisabled: boolean
): boolean {
    const state = channelState[rowId];
    if (!state) {
        return false;
    }

    let changed = false;

    if (emailDisabled && state.emailEnabled) {
        state.emailEnabled = false;
        changed = true;
    }

    if (smsDisabled && state.smsEnabled) {
        state.smsEnabled = false;
        changed = true;
    }

    return changed;
}

function handleSaveNotificationPreferencesError(err: unknown): void {
    const resultError = err as ResultError | undefined;
    logger.error(resultError?.message ?? String(err));

    if (resultError?.statusCode === 429) {
        saveError.value = null;
        errorStore.setTooManyRequestsError("page");
        return;
    }

    saveError.value =
        "An unexpected error has occurred. Please try again later.";
}

async function saveNotificationPreferences(
    type: ProfileNotificationType,
    preferences: ProfileNotificationPreference[]
): Promise<void> {
    const notificationSetting: UserProfileNotificationSettingModel = {
        type: getUserProfileNotificationType(type), // "bcCancerScreening" to "BcCancerScreening"
        emailEnabled: isEmailPreferenceEnabledForType(type)
            ? preferences.includes(ProfileNotificationPreference.Email)
            : null,
        smsEnabled: isSmsPreferenceEnabledForType(type)
            ? preferences.includes(ProfileNotificationPreference.Sms)
            : null,
    };

    // Let errors bubble up so the caller can roll back UI state.
    await userStore.updateNotificationSettings(notificationSetting);
}

async function handleChannelToggle(
    item: NotificationPreference,
    channel: NotificationChannel,
    newValue: boolean | null
): Promise<void> {
    const newChannelEnabled: boolean = newValue ?? false;

    // Prevent rapid repeated toggles while a save is in-flight for this row.
    if (savingState[item.id]) {
        return;
    }

    saveError.value = null;

    const state = channelState[item.id];
    if (!state) {
        return;
    }

    // Compute previous values safely using the event payload
    const previousEmailEnabled =
        channel === "email" ? !newChannelEnabled : state.emailEnabled;
    const previousSmsEnabled =
        channel === "sms" ? !newChannelEnabled : state.smsEnabled;

    // Ensure state reflects the intended new value
    if (channel === "email") {
        state.emailEnabled = newChannelEnabled;
    } else {
        state.smsEnabled = newChannelEnabled;
    }

    const preferences: ProfileNotificationPreference[] =
        buildNotificationPreferences(item.id);

    savingState[item.id] = true;

    try {
        await saveNotificationPreferences(item.type, preferences);
    } catch (err: unknown) {
        // Roll back UI state
        state.emailEnabled = previousEmailEnabled;
        state.smsEnabled = previousSmsEnabled;

        const resultError = err as ResultError | undefined;
        logger.error(resultError?.message ?? String(err));

        if (resultError?.statusCode === 429) {
            saveError.value = null;
            errorStore.setTooManyRequestsError("page");
            return;
        }

        saveError.value =
            "An unexpected error has occurred. Please try again later.";
    } finally {
        savingState[item.id] = false;
    }
}

async function syncDisabledChannelsForRow(
    row: NotificationPreference,
    emailDisabled: boolean,
    smsDisabled: boolean
): Promise<boolean> {
    if (savingState[row.id]) {
        return true;
    }

    const changed = disableInvalidChannels(row.id, emailDisabled, smsDisabled);
    if (!changed) {
        return true;
    }

    try {
        await saveNotificationPreferences(
            row.type,
            buildNotificationPreferences(row.id)
        );
        return true;
    } catch (err: unknown) {
        handleSaveNotificationPreferencesError(err);

        const resultError = err as ResultError | undefined;
        return resultError?.statusCode !== 429;
    }
}

// When a contact channel (email or SMS) becomes invalid (deleted or unverified),
// automatically turn OFF any enabled notification preferences for that channel
// and persist the updated settings to the backend.
//
// This enforces the business rule that notifications cannot remain enabled
// if the corresponding contact method is missing or unverified.
watch(
    [isEmailChannelDisabled, isSmsChannelDisabled],
    async ([emailDisabled, smsDisabled]) => {
        for (const row of enabledNotificationPreferences.value) {
            const shouldContinue = await syncDisabledChannelsForRow(
                row,
                emailDisabled,
                smsDisabled
            );

            if (!shouldContinue) {
                return;
            }
        }
    }
);

// Ensure channelState is initialized for each enabled notification type.
// This keeps the UI switch state in sync with the user's saved preferences.
// Runs immediately and whenever the enabled notification rows or
// underlying preference data change.
watchEffect(() => {
    const rows = enabledNotificationPreferences.value;
    const prefsMap = notificationPreferencesByType.value;

    for (const row of rows) {
        // Initialize channel state once
        if (!channelState[row.id]) {
            const enabledPrefs = prefsMap.get(row.type) ?? [];
            channelState[row.id] = {
                emailEnabled: enabledPrefs.includes(
                    ProfileNotificationPreference.Email
                ),
                smsEnabled: enabledPrefs.includes(
                    ProfileNotificationPreference.Sms
                ),
            };
        }

        // Initialize saving state once
        if (savingState[row.id] === undefined) {
            savingState[row.id] = false;
        }

        // Force off if feature toggle disables that preference for this type
        if (!isEmailPreferenceEnabledForType(row.type)) {
            channelState[row.id].emailEnabled = false;
        }

        if (!isSmsPreferenceEnabledForType(row.type)) {
            channelState[row.id].smsEnabled = false;
        }

        // Force off if contact channel is missing or unverified
        if (isEmailChannelDisabled.value) {
            channelState[row.id].emailEnabled = false;
        }

        if (isSmsChannelDisabled.value) {
            channelState[row.id].smsEnabled = false;
        }
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
            v-if="verificationMessage"
            class="mt-2"
            data-testid="profile-notification-preferences-verification-message"
        >
            {{ verificationMessage }}
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
        <v-container fluid class="pa-0">
            <!-- Key change: constrain width so Email/SMS don't drift on 4K/5K -->
            <v-sheet max-width="720" class="pa-0">
                <template v-if="hasEnabledNotificationPreferences">
                    <v-row
                        no-gutters
                        class="font-weight-bold mb-2"
                        data-testid="profile-notification-preferences-header"
                    >
                        <v-col
                            data-testid="profile-notification-preferences-header-type"
                            cols="8"
                            class="py-0 pe-2"
                        >
                            Notification Type
                        </v-col>
                        <v-col
                            v-if="showEmailColumn"
                            data-testid="profile-notification-preferences-header-email"
                            cols="2"
                            class="py-0 text-center"
                        >
                            Email
                        </v-col>
                        <v-col
                            v-if="showSmsColumn"
                            data-testid="profile-notification-preferences-header-sms"
                            cols="2"
                            class="py-0 text-center"
                        >
                            Text (SMS)
                        </v-col>
                    </v-row>
                    <v-row
                        v-for="item in enabledNotificationPreferences"
                        :key="item.id"
                        no-gutters
                        align="center"
                        :data-testid="`profile-notification-preferences-row-${item.id}`"
                    >
                        <v-col cols="8" class="py-0 pe-2">
                            <div class="d-flex align-center">
                                <span
                                    :data-testid="`profile-notification-preferences-${item.id}-label-value`"
                                >
                                    {{ item.label }}
                                </span>
                                <InteractiveInfoTooltipComponent
                                    :icon-testid="`info-tooltip-${item.id}-icon`"
                                    :tooltip-testid="`info-tooltip-${item.id}`"
                                >
                                    <p class="mb-0">
                                        {{ item.tooltip.text }}
                                        <a
                                            :href="item.tooltip.href"
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            class="text-white text-decoration-underline"
                                            :data-testid="`info-tooltip-${item.id}-link`"
                                        >
                                            {{ item.tooltip.linkText }}
                                        </a>
                                        {{ item.tooltip.suffix }}
                                    </p>
                                </InteractiveInfoTooltipComponent>
                            </div>
                        </v-col>
                        <v-col
                            v-if="showEmailColumn"
                            cols="2"
                            class="py-0 d-flex justify-center"
                        >
                            <v-switch
                                v-model="channelState[item.id].emailEnabled"
                                color="primary"
                                class="hg-switch"
                                density="compact"
                                hide-details
                                inset
                                :data-testid="`profile-notification-preferences-${item.id}-email-value`"
                                :disabled="
                                    isEmailToggleDisabledForType(item.type) ||
                                    savingState[item.id]
                                "
                                @update:model-value="
                                    handleChannelToggle(item, 'email', $event)
                                "
                            />
                        </v-col>
                        <v-col
                            v-if="showSmsColumn"
                            cols="2"
                            class="py-0 d-flex justify-center"
                        >
                            <v-switch
                                v-model="channelState[item.id].smsEnabled"
                                color="primary"
                                class="hg-switch"
                                density="compact"
                                hide-details
                                inset
                                :data-testid="`profile-notification-preferences-${item.id}-sms-value`"
                                :disabled="
                                    isSmsToggleDisabledForType(item.type) ||
                                    savingState[item.id]
                                "
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
            </v-sheet>
        </v-container>
        <v-divider class="my-4" />
    </div>
</template>

<style lang="scss" scoped>
:deep(.hg-switch .v-switch__track.bg-primary) {
    opacity: 1 !important;
}
</style>
