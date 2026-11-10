import { afterEach, describe, expect, it } from "@jest/globals";
import { Platform } from "react-native";

import {
    getConfigUrl,
    selectFeatureToggles,
    selectIdentityProviders,
    selectIsOffline,
    useConfigStore,
} from "@/stores/config.store";
import { useEnvironmentStore } from "@/stores/environment.store";
import { configFixture, resetStores } from "@/test/fixtures";

const withOffline = (
    offline: NonNullable<typeof configFixture.webClient.offlineMode> | null,
    clientIP?: string
) =>
    useConfigStore.setState({
        config: {
            ...configFixture,
            webClient: {
                ...configFixture.webClient,
                offlineMode: offline,
                clientIP,
            },
        },
    });

describe("config store selectors", () => {
    afterEach(resetStores);

    it("native reads the configuration URL from the chosen environment", () => {
        // a development build starts on Local: the services on this computer
        expect(getConfigUrl()).toBe("http://localhost:3025/configuration");
        useEnvironmentStore.setState({ selected: "test" });
        expect(getConfigUrl()).toBe(
            "https://test.healthgateway.gov.bc.ca/configuration"
        );
        useEnvironmentStore.setState({ selected: undefined });
    });

    it("web appends to the CLI's base URI, or asks the server that hosted it", () => {
        const saved = process.env.EXPO_PUBLIC_WEB_CLIENT_BASE_URI;
        const os = Platform.OS;
        Object.defineProperty(Platform, "OS", {
            configurable: true,
            value: "web",
        });
        delete process.env.EXPO_PUBLIC_WEB_CLIENT_BASE_URI;
        expect(getConfigUrl()).toBe("/configuration");
        process.env.EXPO_PUBLIC_WEB_CLIENT_BASE_URI = "/";
        expect(getConfigUrl()).toBe("/configuration");
        process.env.EXPO_PUBLIC_WEB_CLIENT_BASE_URI = "https://cfg.test/";
        expect(getConfigUrl()).toBe("https://cfg.test/configuration");
        if (saved === undefined)
            delete process.env.EXPO_PUBLIC_WEB_CLIENT_BASE_URI;
        else process.env.EXPO_PUBLIC_WEB_CLIENT_BASE_URI = saved;
        Object.defineProperty(Platform, "OS", {
            configurable: true,
            value: os,
        });
    });

    it("is offline with no configuration and online with no offline mode", () => {
        expect(selectIsOffline(useConfigStore.getState())).toBe(true);
        withOffline(null);
        expect(selectIsOffline(useConfigStore.getState())).toBe(false);
    });

    it("is offline inside the window unless the client IP is whitelisted", () => {
        withOffline(
            {
                startDateTime: "2000-01-01T00:00:00Z",
                message: "Down",
                whitelist: [],
            },
            "1.1.1.1"
        );
        expect(selectIsOffline(useConfigStore.getState())).toBe(true);
        withOffline(
            {
                startDateTime: "2000-01-01T00:00:00Z",
                message: "Down",
                whitelist: ["1.1.1.1"],
            },
            "1.1.1.1"
        );
        expect(selectIsOffline(useConfigStore.getState())).toBe(false);
    });

    it("is online before the window starts or after it ends", () => {
        withOffline({
            startDateTime: "2099-01-01T00:00:00Z",
            message: "Soon",
            whitelist: [],
        });
        expect(selectIsOffline(useConfigStore.getState())).toBe(false);
        withOffline({
            startDateTime: "2000-01-01T00:00:00Z",
            endDateTime: "2000-01-02T00:00:00Z",
            message: "Past",
            whitelist: [],
        });
        expect(selectIsOffline(useConfigStore.getState())).toBe(false);
    });

    it("exposes identity providers and feature toggles", () => {
        expect(selectIdentityProviders(useConfigStore.getState())).toEqual([]);
        // stable reference without a configuration (a fresh [] would loop subscribed components)
        expect(selectIdentityProviders(useConfigStore.getState())).toBe(
            selectIdentityProviders(useConfigStore.getState())
        );
        useConfigStore.setState({ config: configFixture });
        expect(selectIdentityProviders(useConfigStore.getState())[0].id).toBe(
            "bcsc"
        );
        expect(
            selectFeatureToggles(useConfigStore.getState())?.dependents.enabled
        ).toBe(true);
    });
});
