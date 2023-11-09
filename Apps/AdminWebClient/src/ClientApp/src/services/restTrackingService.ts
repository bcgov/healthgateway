import { injectable } from "inversify";

import { EventData, SnowplowWindow } from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";

declare let window: SnowplowWindow;

@injectable()
export class RestTrackingService implements ITrackingService {
    public trackEvent(data: EventData): void {
        console.log(`Tracking event: ${JSON.stringify(data)}`);
        window.snowplow("trackSelfDescribingEvent", {
            schema: "iglu:ca.bc.gov.gateway/action/jsonschema/1-0-0",
            data,
        });
    }
}
