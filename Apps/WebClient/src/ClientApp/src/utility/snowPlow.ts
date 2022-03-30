import { EventData, SnowplowWindow } from "@/plugins/extensions";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

declare let window: SnowplowWindow;

export default abstract class SnowPlow {
    public static trackEvent(data: EventData): void {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.debug(`Tracking event: ${data.action} - ${data.text}`);
        window.snowplow("trackSelfDescribingEvent", {
            schema: "iglu:ca.bc.gov.gateway/action/jsonschema/1-0-0",
            data,
        });
    }
}
