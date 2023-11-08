import { EventData, SnowplowWindow } from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";

declare let window: SnowplowWindow;

export class RestTrackingService implements ITrackingService {
    private logger;

    constructor(logger: ILogger) {
        this.logger = logger;
    }

    public track(data: EventData): Promise<void> {
        this.logger.debug(`Tracking event: ${JSON.stringify(data)}`);

        window.snowplow("trackSelfDescribingEvent", {
            schema: "iglu:ca.bc.gov.gateway/action/jsonschema/1-0-0",
            data,
        });

        return Promise.resolve();
    }
}
