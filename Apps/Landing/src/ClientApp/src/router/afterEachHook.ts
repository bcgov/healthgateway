import { SnowplowWindow } from "@/plugins/extensions";

declare let window: SnowplowWindow;

export function afterEachHook() {
    window.snowplow("trackPageView");
}
