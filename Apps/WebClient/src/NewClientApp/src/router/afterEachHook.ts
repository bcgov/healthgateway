import { SnowplowWindow } from "@/plugins/extensions";
import { useErrorStore } from "@/stores/error";

declare let window: SnowplowWindow;

export function afterEachHook() {
    const errorStore = useErrorStore();

    errorStore.clearErrors();
    errorStore.clearTooManyRequestsWarning();
    errorStore.clearTooManyRequestsError();

    window.snowplow("trackPageView");
}
