import { describe, expect, it } from "@jest/globals";

import { trackingConfigFromEnv } from "@/services/tracking/trackingService";

describe("trackingConfigFromEnv", () => {
    it("is on when EXPO_PUBLIC_SNOWPLOW_ENABLED is unset or true (any case), off for anything else", () => {
        expect(trackingConfigFromEnv({}).enabled).toBe(true);
        expect(
            trackingConfigFromEnv({ EXPO_PUBLIC_SNOWPLOW_ENABLED: "TRUE" })
                .enabled
        ).toBe(true);
        expect(
            trackingConfigFromEnv({ EXPO_PUBLIC_SNOWPLOW_ENABLED: "false" })
                .enabled
        ).toBe(false);
        expect(
            trackingConfigFromEnv({ EXPO_PUBLIC_SNOWPLOW_ENABLED: "yes" })
                .enabled
        ).toBe(false);
    });

    it("defaults the collector and app id to the web client's values", () => {
        expect(trackingConfigFromEnv({})).toMatchObject({
            collector: "spt.apps.gov.bc.ca",
            appId: "Snowplow_standalone",
        });
        expect(
            trackingConfigFromEnv({
                EXPO_PUBLIC_SNOWPLOW_COLLECTOR: "spm.apps.gov.bc.ca",
                EXPO_PUBLIC_SNOWPLOW_APP_ID: "Snowplow_mobile",
            })
        ).toMatchObject({
            collector: "spm.apps.gov.bc.ca",
            appId: "Snowplow_mobile",
        });
    });

    it("reads the app's own environment by default", () => {
        const c = trackingConfigFromEnv();
        expect(typeof c.enabled).toBe("boolean");
        expect(c.collector.length).toBeGreaterThan(0);
        expect(c.appId.length).toBeGreaterThan(0);
    });
});
