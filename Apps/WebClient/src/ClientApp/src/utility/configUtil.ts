import { CardNames } from "@/constants/cardNames";
import { EntryType } from "@/constants/entryType";
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
}
