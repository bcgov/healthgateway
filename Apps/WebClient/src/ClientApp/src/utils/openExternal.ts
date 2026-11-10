import { Linking } from "react-native";

import { describeError, logger } from "@/services/logger";

/**
 * Open an external URL (https, mailto, …). Never throws: on a device with no handler for the scheme
 * (e.g. no Mail app on the simulator) Linking.openURL rejects, which would otherwise surface as an
 * unhandled promise rejection.
 */
export async function openExternal(url: string): Promise<void> {
    try {
        await Linking.openURL(url);
    } catch (error) {
        logger.warn(`Unable to open a link: ${describeError(error)}`);
    }
}
