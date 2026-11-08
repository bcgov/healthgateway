import type { EventData } from "@/constants/tracking";

export interface TrackingConfig {
    enabled: boolean;
    /** Collector host, e.g. spt.apps.gov.bc.ca */
    collector: string;
    appId: string;
    /** Native: stands in for the web client's page address, which is how analytics tell the environments apart */
    origin?: string;
}

export interface ITrackingService {
    init(config: TrackingConfig): void;
    /** Self-describing event with the Health Gateway action schema (same as the web client). */
    trackEvent(data: EventData): void;
    /** Page view (web) / screen view (native) on every route change. */
    trackPageView(path: string): void;
}

/**
 * Web: reads the switch from the app's own configuration (native: the selected environment, `config/environments.ts`); ON unless
 * EXPO_PUBLIC_SNOWPLOW_ENABLED says otherwise, locally too, as the old Vue client was (Jest sets it to false).
 * `env` is a parameter so tests can pass their own: babel-preset-expo inlines `process.env.EXPO_PUBLIC_*`
 * at build time, so the real values cannot be changed at runtime.
 */
export function trackingConfigFromEnv(
    env: Record<string, string | undefined> = {
        EXPO_PUBLIC_SNOWPLOW_ENABLED: process.env.EXPO_PUBLIC_SNOWPLOW_ENABLED,
        EXPO_PUBLIC_SNOWPLOW_COLLECTOR:
            process.env.EXPO_PUBLIC_SNOWPLOW_COLLECTOR,
        EXPO_PUBLIC_SNOWPLOW_APP_ID: process.env.EXPO_PUBLIC_SNOWPLOW_APP_ID,
    }
): TrackingConfig {
    return {
        enabled:
            (env.EXPO_PUBLIC_SNOWPLOW_ENABLED ?? "true").toLowerCase() ===
            "true",
        collector: env.EXPO_PUBLIC_SNOWPLOW_COLLECTOR || "spt.apps.gov.bc.ca",
        appId: env.EXPO_PUBLIC_SNOWPLOW_APP_ID || "Snowplow_standalone",
    };
}
