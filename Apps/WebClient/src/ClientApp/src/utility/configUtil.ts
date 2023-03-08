import { EntryType } from "@/constants/entryType";
import { ServiceName } from "@/constants/serviceName";
import {
    FeatureToggleConfiguration,
    WebClientConfiguration,
} from "@/models/configData";
import container from "@/plugins/container";
import { STORE_IDENTIFIER } from "@/plugins/inversify";
import { IStoreProvider } from "@/services/interfaces";

export default abstract class ConfigUtil {
    public static getWebClientConfiguration(): WebClientConfiguration {
        const storeWrapper = container.get<IStoreProvider>(
            STORE_IDENTIFIER.StoreProvider
        );
        const store = storeWrapper.getStore();

        return store.getters["config/webClient"];
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

    public static isServiceEnabled(serviceName: ServiceName) {
        const config = ConfigUtil.getFeatureConfiguration();
        console.log("isServiceEnabled - config log:", config);
        if (config.services && config.services.enabled) {
            const result = config.services.services.some(
                (service) =>
                    service.name.toLowerCase() === serviceName.toLowerCase() &&
                    service.enabled
            );
            console.log("isServiceEnabled: " + serviceName, result);
            return result;
        }
        return false;
    }

    public static isDependentDatasetEnabled(datasetName: EntryType): boolean {
        const config = ConfigUtil.getFeatureConfiguration();

        const disabledByOverride =
            config.dependents.datasets.find(
                (ds) => ds.name.toLowerCase() === datasetName.toLowerCase()
            )?.enabled === false;

        return ConfigUtil.isDatasetEnabled(datasetName) && !disabledByOverride;
    }
}
