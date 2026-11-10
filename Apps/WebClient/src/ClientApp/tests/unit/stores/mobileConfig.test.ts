import { afterEach, describe, expect, it, jest } from "@jest/globals";

import type { MobileConfiguration } from "@/models/configData";
import { services } from "@/services/container";
import {
    DEFAULT_MOBILE_IDLE_MS,
    MobileConfigurationError,
    nativeOidcConfig,
    selectIdentityProviders,
    selectIdleTimeout,
    selectUpdateRequired,
    useConfigStore,
} from "@/stores/config.store";
import { configFixture } from "@/test/fixtures";

jest.mock("expo-constants", () => ({
    __esModule: true,
    default: { expoConfig: { version: "1.2.3" } },
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

    describe("minimumVersion", () => {
        it("requires an update when this app is older", () => {
            withMobile({ minimumVersion: "1.2.4" });
            expect(selectUpdateRequired(state())).toBe(true);
        });

        it("does not when this app is the same or newer", () => {
            withMobile({ minimumVersion: "1.2.3" });
            expect(selectUpdateRequired(state())).toBe(false);
        });

        it.each([[undefined], [null], [{}], [{ minimumVersion: "" }]])(
            "ignores the check when missing: %j",
            (mobile) => {
                withMobile(mobile);
                expect(selectUpdateRequired(state())).toBe(false);
            }
        );

        it("is not read from the top level", () => {
            useConfigStore.setState({
                config: {
                    ...configWithMobile(undefined),
                    mobile: { minimumVersion: "9.0.0" },
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
        const { identityProviders } = authentication;

        it.each([["ios"], ["android"]])(
            "signs in on %s with its own client at the mobile endpoint",
            (os) => {
                const oidc = nativeOidcConfig(
                    configWithMobile({ authentication }),
                    os
                );
                expect(oidc).toEqual({
                    authority: "https://sso.test/auth/realms/mobile",
                    clientId: `hg-mobile-${os}`,
                    scope: configFixture.openIdConnect.scope,
                    callbacks: null,
                });
            }
        );

        it.each([
            [
                "the whole block",
                undefined,
                [
                    "webClient.mobile.authentication.endpoint",
                    "webClient.mobile.authentication.clientIdPrefix",
                    "webClient.mobile.authentication.identityProviders",
                ],
            ],
            [
                "the endpoint",
                { clientIdPrefix: "hg-mobile", identityProviders },
                ["webClient.mobile.authentication.endpoint"],
            ],
            [
                "the client ID prefix",
                {
                    endpoint: "https://sso.test/auth/realms/mobile",
                    identityProviders,
                },
                ["webClient.mobile.authentication.clientIdPrefix"],
            ],
            [
                "the identity providers",
                {
                    endpoint: "https://sso.test/auth/realms/mobile",
                    clientIdPrefix: "hg-mobile",
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
                configWithMobile({ minimumVersion: "9.0.0" })
            );
            await state().refresh();
            expect(selectUpdateRequired(state())).toBe(true);
        });

        it("keeps the current configuration when it cannot be read", async () => {
            withMobile({ minimumVersion: "1.0.0" });
            jest.spyOn(services.config, "getConfiguration").mockRejectedValue(
                new Error("offline")
            );
            await state().refresh();
            expect(state().config?.webClient.mobile?.minimumVersion).toBe(
                "1.0.0"
            );
        });
    });
});
