import { useErrorStore } from "@/stores/error";

// declare let window: SnowplowWindow; // TODO: Implement snowplow

export function afterEachHook() {
    const errorStore = useErrorStore();

    errorStore.clearErrors();
    errorStore.clearTooManyRequestsWarning();
    errorStore.clearTooManyRequestsError();

    // window.snowplow("trackPageView"); // TODO: Implement snowplow
}
