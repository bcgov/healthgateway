import { EventData, SnowplowWindow } from "@/plugins/extensions";
import { ILogger } from "@/services/interfaces";
import { WinstonLogger } from "@/services/winstonLogger";

declare let window: SnowplowWindow;
const logger: ILogger = new WinstonLogger(true); // TODO: Inject this
export default abstract class SnowPlow {
    public static trackEvent(data: EventData): void {
        logger.debug(`Tracking event: ${data.action} - ${data.text}`);
        window.snowplow("trackSelfDescribingEvent", {
            schema: "iglu:ca.bc.gov.gateway/action/jsonschema/1-0-0",
            data,
        });
    }
}
