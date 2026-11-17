import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import type {
    CacheConfiguration,
    MobileConfiguration,
} from "@/models/configData";
import { services } from "@/services/container";
import { logger } from "@/services/logger";
import {
    DEFAULT_CACHE_MS,
    DEFAULT_MOBILE_IDLE_MS,
    MIN_CACHE_MS,
    MobileConfigurationError,
    nativeOidcConfig,
    selectCacheMs,
    selectIdentityProviders,
    selectIdleTimeout,
    selectUpdateRequired,
    useConfigStore,
} from "@/stores/config.store";
import { configFixture } from "@/test/fixtures";

jest.mock("expo-constants", () => ({
    __esModule: true,
    default: { expoConfig: { version: "1.2.3", extra: { build: 57 } } },
}));

const configWithMobile = (mobile: unknown) => ({
    ...configFixture,
    webClient: {
        ...configFixture.webClient,
        mobile: mobile as MobileConfiguration,
    },
});
const withMobile = (mobile: unknown) =>
    useConfigStore.setState({ config: configWithMobile(mobile) });
const state = () => useConfigStore.getState();

describe("mobile configuration", () => {
    afterEach(() => {
        jest.restoreAllMocks();
        useConfigStore.setState({ config: undefined });
    });

    describe("minimumBuild", () => {
        it("requires an update when this app is older", () => {
            withMobile({ minimumBuild: 58 });
            expect(selectUpdateRequired(state())).toBe(true);
        });

        it("does not when this app is the same or newer", () => {
            withMobile({ minimumBuild: 57 });
            expect(selectUpdateRequired(state())).toBe(false);
        });

        it.each([
            [undefined],
            [null],
            [{}],
            [{ minimumBuild: null }],
            [{ minimumBuild: "300" }],
        ])("ignores the check when missing: %j", (mobile) => {
            withMobile(mobile);
            expect(selectUpdateRequired(state())).toBe(false);
        });

        it("is not read from the top level", () => {
            useConfigStore.setState({
                config: {
                    ...configWithMobile(undefined),
                    mobile: { minimumBuild: 9999 },
                } as never,
            });
            expect(selectUpdateRequired(state())).toBe(false);
        });
    });

    describe("timeouts.idle", () => {
        it("uses the configured milliseconds", () => {
            withMobile({ timeouts: { idle: 60000 } });
            expect(selectIdleTimeout(state())).toBe(60000);
        });

        it.each([
            [undefined],
            [null],
            [{}],
            [{ timeouts: {} }],
            [{ timeouts: { idle: 0 } }],
            [{ timeouts: { idle: "soon" } }],
        ])("defaults to 20 minutes: %j", (mobile) => {
            withMobile(mobile);
            expect(selectIdleTimeout(state())).toBe(DEFAULT_MOBILE_IDLE_MS);
            expect(DEFAULT_MOBILE_IDLE_MS).toBe(1200000);
        });

        it("does not use the web client's idle time on mobile", () => {
            withMobile(undefined);
            expect(selectIdleTimeout(state())).not.toBe(
                configFixture.webClient.timeouts.idle
            );
        });
    });

    describe("authentication", () => {
        const authentication = {
            endpoint: "https://sso.test/auth/realms/mobile",
            clientIdPrefix: "hg-mobile",
            scope: "openid profile offline_access",
            identityProviders: [
                {
                    id: "bcsc",
                    name: "BC Services Card",
                    icon: "address-card",
                    hint: "bcsc-mobile",
                    disabled: false,
                },
            ],
        };
        const { identityProviders, scope } = authentication;

        it.each([["ios"], ["android"]])(
            "signs in on %s with its own client and scopes at the mobile endpoint",
            (os) => {
                const oidc = nativeOidcConfig(
                    configWithMobile({ authentication }),
                    os
                );
                expect(oidc).toEqual({
                    authority: "https://sso.test/auth/realms/mobile",
                    clientId: `hg-mobile-${os}`,
                    scope: "openid profile offline_access",
                    callbacks: null,
                });
                expect(oidc.scope).not.toBe(configFixture.openIdConnect.scope);
            }
        );

        it.each([
            [
                "the whole block",
                undefined,
                [
                    "webClient.mobile.authentication.endpoint",
                    "webClient.mobile.authentication.clientIdPrefix",
                    "webClient.mobile.authentication.scope",
                    "webClient.mobile.authentication.identityProviders",
                ],
            ],
            [
                "the endpoint",
                { clientIdPrefix: "hg-mobile", scope, identityProviders },
                ["webClient.mobile.authentication.endpoint"],
            ],
            [
                "the client ID prefix",
                {
                    endpoint: "https://sso.test/auth/realms/mobile",
                    scope,
                    identityProviders,
                },
                ["webClient.mobile.authentication.clientIdPrefix"],
            ],
            [
                "the scope",
                {
                    endpoint: "https://sso.test/auth/realms/mobile",
                    clientIdPrefix: "hg-mobile",
                    identityProviders,
                },
                ["webClient.mobile.authentication.scope"],
            ],
            [
                "an empty scope",
                { ...authentication, scope: "" },
                ["webClient.mobile.authentication.scope"],
            ],
            [
                "the identity providers",
                {
                    endpoint: "https://sso.test/auth/realms/mobile",
                    clientIdPrefix: "hg-mobile",
                    scope,
                },
                ["webClient.mobile.authentication.identityProviders"],
            ],
            [
                "any identity providers",
                { ...authentication, identityProviders: [] },
                ["webClient.mobile.authentication.identityProviders"],
            ],
            [
                "an empty endpoint",
                { ...authentication, endpoint: "" },
                ["webClient.mobile.authentication.endpoint"],
            ],
        ])(
            "without %s, it is a configuration error, not the web client's settings",
            (_name, auth, missing) => {
                const config = configWithMobile({ authentication: auth });
                let thrown: unknown;
                try {
                    nativeOidcConfig(config, "ios");
                } catch (error) {
                    thrown = error;
                }
                expect(thrown).toBeInstanceOf(MobileConfigurationError);
                expect((thrown as MobileConfigurationError).missing).toEqual(
                    missing
                );
            }
        );

        it("offers the mobile identity providers on native, not the web client's", () => {
            withMobile({ authentication });
            expect(selectIdentityProviders(state())).toEqual(identityProviders);
            expect(selectIdentityProviders(state())).not.toEqual(
                configFixture.identityProviders
            );
        });

        it("offers none on native without the mobile list, never the web client's", () => {
            withMobile(undefined);
            expect(selectIdentityProviders(state())).toEqual([]);
        });

        it("is a configuration error when there is no mobile block", () => {
            expect(() =>
                nativeOidcConfig(configWithMobile(null), "android")
            ).toThrow(MobileConfigurationError);
        });
    });

    describe("refresh", () => {
        it("picks up a new minimum version while running", async () => {
            withMobile(undefined);
            jest.spyOn(services.config, "getConfiguration").mockResolvedValue(
                configWithMobile({ minimumBuild: 9999 })
            );
            await state().refresh();
            expect(selectUpdateRequired(state())).toBe(true);
        });

        it("keeps the current configuration when it cannot be read", async () => {
            withMobile({ minimumBuild: 1 });
            jest.spyOn(services.config, "getConfiguration").mockRejectedValue(
                new Error("offline")
            );
            await state().refresh();
            expect(state().config?.webClient.mobile?.minimumBuild).toBe(1);
        });
    });

    describe("cache", () => {
        const withCache = (
            web: CacheConfiguration | undefined,
            mobile: CacheConfiguration | undefined
        ) =>
            useConfigStore.setState({
                config: {
                    ...configFixture,
                    webClient: {
                        ...configFixture.webClient,
                        cache: web,
                        mobile: { cache: mobile },
                    },
                },
            });
        const cacheMs = (service = "Communications") =>
            selectCacheMs(state(), service);

        it("uses the embedded 30 minutes with no cache block", () => {
            withCache(undefined, undefined);
            expect(cacheMs()).toBe(DEFAULT_CACHE_MS);
            expect(DEFAULT_CACHE_MS).toBe(1800000);
        });

        it("stacks: web default, then mobile default, then web service, then mobile service", () => {
            withCache({ default: 3600 }, undefined);
            expect(cacheMs()).toBe(3600000);
            withCache({ default: 3600 }, { default: 600 });
            expect(cacheMs()).toBe(600000);
            withCache(
                { default: 3600, services: { Communications: 21600 } },
                { default: 600 }
            );
            expect(cacheMs()).toBe(21600000);
            withCache(
                { default: 3600, services: { Communications: 21600 } },
                { default: 600, services: { Communications: 360 } }
            );
            expect(cacheMs()).toBe(360000);
        });

        it("gives a service not in the list the default", () => {
            withCache(
                { default: 3600, services: { Communications: 21600 } },
                undefined
            );
            expect(cacheMs("Immunization")).toBe(3600000);
        });

        it("matches the service name without regard to case", () => {
            withCache({ services: { communications: 21600 } }, undefined);
            expect(cacheMs("Communications")).toBe(21600000);
        });

        it("raises anything under five minutes to five minutes, once warned", () => {
            const warn = jest
                .spyOn(logger, "warn")
                .mockImplementation(() => undefined);
            expect(MIN_CACHE_MS).toBe(300000);
            withCache({ default: 0 }, undefined);
            expect(cacheMs()).toBe(MIN_CACHE_MS);
            withCache({ services: { Communications: 299 } }, undefined);
            expect(cacheMs()).toBe(MIN_CACHE_MS);
            expect(cacheMs()).toBe(MIN_CACHE_MS);
            expect(warn).toHaveBeenCalledTimes(2);
            expect(warn).toHaveBeenCalledWith(
                "Cache time for Communications is 299s, under the 300s minimum: using 300s"
            );
            withCache({ services: { Communications: 300 } }, undefined);
            expect(cacheMs()).toBe(300000);
            expect(warn).toHaveBeenCalledTimes(2);
        });

        it("skips values it can't use", () => {
            withCache(
                {
                    default: 3600,
                    services: { Communications: -1 },
                },
                { default: "soon" } as never
            );
            expect(cacheMs()).toBe(3600000);
        });

        it("web reads only webClient.cache", () => {
            const os = Platform.OS;
            Object.defineProperty(Platform, "OS", {
                configurable: true,
                value: "web",
            });
            try {
                withCache(
                    { default: 3600, services: { Communications: 21600 } },
                    { default: 600, services: { Communications: 360 } }
                );
                expect(cacheMs()).toBe(21600000);
                expect(cacheMs("Immunization")).toBe(3600000);
            } finally {
                Object.defineProperty(Platform, "OS", {
                    configurable: true,
                    value: os,
                });
            }
        });
    });
});
