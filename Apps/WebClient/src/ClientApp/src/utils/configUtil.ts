// Port of utility/configUtil.ts (the parts Home uses), reading the config store directly.
import { EntryType, ServiceName } from "@/constants/entryType";
import {
    ProfileNotificationPreference,
    ProfileNotificationType,
} from "@/constants/profileNotifications";
import { selectFeatureToggles, useConfigStore } from "@/stores/config.store";

const toggles = () => selectFeatureToggles(useConfigStore.getState());

export const ConfigUtil = {
    isDatasetEnabled: (dataset: EntryType) =>
        toggles()?.datasets.find(
            (d) => d.name.toLowerCase() === dataset.toLowerCase()
        )?.enabled === true,
    /** A dataset is on for dependents unless `dependents.datasets` switches it off. */
    isDependentDatasetEnabled: (dataset: EntryType) => {
        const disabledByOverride =
            toggles()?.dependents?.datasets?.find(
                (d) => d.name.toLowerCase() === dataset.toLowerCase()
            )?.enabled === false;
        return ConfigUtil.isDatasetEnabled(dataset) && !disabledByOverride;
    },
    isServiceEnabled: (service: ServiceName) => {
        const c = toggles();
        return Boolean(
            c?.services?.enabled &&
            c.services.services?.some(
                (s) =>
                    s.name.toLowerCase() === service.toLowerCase() && s.enabled
            )
        );
    },
    isOtherRecordSourcesFeatureEnabled: () =>
        toggles()?.homepage?.otherRecordSources?.enabled ?? false,
    /** CardNames.AccessMyHealth = "accessMyHealth" */
    isOtherRecordSourcesCardEnabled: (cardName: string) => {
        const f = toggles()?.homepage?.otherRecordSources;
        return Boolean(
            f?.enabled &&
            f.sources?.some(
                (c) =>
                    c.name.toLowerCase() === cardName.toLowerCase() && c.enabled
            )
        );
    },
    isImmunizationRecordLinkEnabled: () =>
        toggles()?.homepage?.showImmunizationRecordLink ?? false,
    showRecommendationsLink: () =>
        toggles()?.homepage?.showRecommendationsLink ?? false,
    isProfileNotificationsFeatureEnabled: () =>
        toggles()?.profile?.notifications?.enabled ?? false,
    /** Matches the configured type name case-insensitively, like the web client. */
    isProfileNotificationTypeEnabled: (type: ProfileNotificationType) => {
        const feature = toggles()?.profile?.notifications;
        if (!feature?.enabled || !feature.type) return false;
        return feature.type.some(
            (t) => t.name.toLowerCase() === type.toLowerCase() && t.enabled
        );
    },
    isProfileNotificationPreferenceEnabled: (
        type: ProfileNotificationType,
        preference: ProfileNotificationPreference
    ) => {
        const feature = toggles()?.profile?.notifications;
        if (!feature?.enabled || !feature.type) return false;
        const setting = feature.type.find(
            (t) => t.name.toLowerCase() === type.toLowerCase() && t.enabled
        );
        return Boolean(setting?.preferences?.[preference]);
    },
};
