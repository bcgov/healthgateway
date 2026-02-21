import { CardNames } from "@/constants/cardNames";
import { EntryType } from "@/constants/entryType";
import {
    ProfileNotificationPreference,
    ProfileNotificationType,
} from "@/constants/profileNotifications";
import { ServiceName } from "@/constants/serviceName";
import {
    FeatureToggleConfiguration,
    WebClientConfiguration,
} from "@/models/configData";
import { useConfigStore } from "@/stores/config";

export default abstract class ConfigUtil {
    public static getWebClientConfiguration(): WebClientConfiguration {
        const configStore = useConfigStore();
        return configStore.webConfig;
    }

    public static getFeatureConfiguration(): FeatureToggleConfiguration {
        return ConfigUtil.getWebClientConfiguration()
            .featureToggleConfiguration;
    }

    public static isDatasetEnabled(datasetName: EntryType): boolean {
        const config = ConfigUtil.getFeatureConfiguration();

        return (
            config.datasets.find(
                (ds) => ds.name.toLowerCase() === datasetName.toLowerCase()
            )?.enabled === true
        );
    }

    public static isDependentDatasetEnabled(datasetName: EntryType): boolean {
        const config = ConfigUtil.getFeatureConfiguration();

        const disabledByOverride =
            config.dependents.datasets.find(
                (ds) => ds.name.toLowerCase() === datasetName.toLowerCase()
            )?.enabled === false;

        return ConfigUtil.isDatasetEnabled(datasetName) && !disabledByOverride;
    }

    public static isServiceEnabled(serviceName: ServiceName): boolean {
        const config = ConfigUtil.getFeatureConfiguration();
        if (config.services?.enabled && config.services.services) {
            return config.services.services.some(
                (service) =>
                    service.name.toLowerCase() === serviceName.toLowerCase() &&
                    service.enabled
            );
        }
        return false;
    }

    public static isServicesFeatureEnabled(): boolean {
        const config = ConfigUtil.getFeatureConfiguration();
        return config.services?.enabled;
    }

    public static isOtherRecordSourcesFeatureEnabled(): boolean {
        const config = ConfigUtil.getFeatureConfiguration();
        return config.homepage?.otherRecordSources?.enabled ?? false;
    }

    public static isOtherRecordSourcesCardEnabled(
        cardName: CardNames
    ): boolean {
        const config = ConfigUtil.getFeatureConfiguration();
        const feature = config.homepage?.otherRecordSources;

        if (feature?.enabled && feature.sources) {
            return feature.sources.some(
                (card) =>
                    card.name.toLowerCase() === cardName.toLowerCase() &&
                    card.enabled
            );
        }

        return false;
    }

    public static isImmunizationRecordLinkEnabled(): boolean {
        const config = ConfigUtil.getFeatureConfiguration();
        return config.homepage?.showImmunizationRecordLink ?? false;
    }

    public static isProfileNotificationsFeatureEnabled(): boolean {
        const config = ConfigUtil.getFeatureConfiguration();
        return config.profile?.notifications?.enabled ?? false;
    }

    public static isProfileNotificationTypeEnabled(
        notificationType: ProfileNotificationType
    ): boolean {
        const config = ConfigUtil.getFeatureConfiguration();
        const feature = config.profile?.notifications;

        if (feature?.enabled && feature.type) {
            return feature.type.some(
                (typeSetting) =>
                    typeSetting.name.toLowerCase() ===
                        notificationType.toLowerCase() && typeSetting.enabled
            );
        }

        return false;
    }

    public static isProfileNotificationPreferenceEnabled(
        notificationType: ProfileNotificationType,
        preference: ProfileNotificationPreference
    ): boolean {
        const config = ConfigUtil.getFeatureConfiguration();
        const notifications = config.profile?.notifications;

        if (!notifications?.enabled || !notifications.type) {
            return false;
        }

        const typeSetting = notifications.type.find(
            (typeSetting) =>
                typeSetting.name.toLowerCase() ===
                    notificationType.toLowerCase() && typeSetting.enabled
        );

        if (!typeSetting || !typeSetting.preferences) {
            return false;
        }

        return Boolean(typeSetting.preferences[preference]);
    }
}
