import { EventData, SnowplowWindow } from "@/plugins/extensions";

declare let window: SnowplowWindow;

export default abstract class SnowPlow {
    public static trackEvent(data: EventData): void {
        window.snowplow("trackSelfDescribingEvent", {
            schema: "iglu:ca.bc.gov.gateway/action/jsonschema/1-0-0",
            data,
        });
    }
}
