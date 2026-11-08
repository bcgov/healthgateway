import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { HttpError } from "@/models/errors";
import { services } from "@/services/container";
import { HttpClient } from "@/services/http/httpClient";
import { logger } from "@/services/logger";
import { savedConfigKey, useConfigStore } from "@/stores/config.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useErrorStore } from "@/stores/error.store";
import { configFixture, jsonResponse } from "@/test/fixtures";

const reset = () => {
    useConnectivityStore.setState({
        isOnline: true,
        serviceUnavailable: false,
        sessionStatus: "live",
    });
    logger.setFailuresExpected(false);
};

describe("offline behaviour", () => {
    beforeEach(async () => {
        reset();
        useErrorStore.setState({ errors: [] });
        await AsyncStorage.clear();
    });

    afterEach(() => {
        reset();
        jest.restoreAllMocks();
    });

    describe("HttpClient", () => {
        it("fails at once offline, without calling the network", async () => {
            const fetchSpy = jest.spyOn(global, "fetch");
            useConnectivityStore.setState({ isOnline: false });
            await expect(new HttpClient().get("https://x")).rejects.toThrow(
                HttpError
            );
            expect(fetchSpy).not.toHaveBeenCalled();
        });

        it("fails at once while the login is expired", async () => {
            const fetchSpy = jest.spyOn(global, "fetch");
            useConnectivityStore.setState({ sessionStatus: "expired" });
            await expect(new HttpClient().get("https://x")).rejects.toThrow(
                HttpError
            );
            expect(fetchSpy).not.toHaveBeenCalled();
        });

        it("calls the network otherwise", async () => {
            const fetchSpy = jest
                .spyOn(global, "fetch")
                .mockResolvedValue(jsonResponse({ ok: true }));
            await new HttpClient().get("https://x");
            expect(fetchSpy).toHaveBeenCalled();
        });
    });

    describe("Health Gateway unreachable, with a connection", () => {
        const state = () => useConnectivityStore.getState();

        it("a call that gets no answer marks it unavailable", async () => {
            jest.spyOn(global, "fetch").mockRejectedValue(
                new TypeError("Network request failed")
            );
            await expect(new HttpClient().get("https://x")).rejects.toThrow(
                HttpError
            );
            expect(state().serviceUnavailable).toBe(true);
        });

        it("still tries while unavailable, and any answer clears it", async () => {
            useConnectivityStore.setState({ serviceUnavailable: true });
            jest.spyOn(global, "fetch").mockResolvedValue(
                jsonResponse({ ok: true })
            );
            await new HttpClient().get("https://x");
            expect(state().serviceUnavailable).toBe(false);
        });

        it("an error response means the service is up: that failure gets its own error", async () => {
            jest.spyOn(global, "fetch").mockResolvedValue(
                jsonResponse({ detail: "boom" }, 503)
            );
            await expect(new HttpClient().get("https://x")).rejects.toThrow();
            expect(state().serviceUnavailable).toBe(false);
        });

        it("keeps error banners quiet", () => {
            useConnectivityStore.setState({ serviceUnavailable: true });
            useErrorStore
                .getState()
                .addError(ErrorType.Retrieve, ErrorSourceType.User);
            expect(useErrorStore.getState().errors).toHaveLength(0);
        });

        it("logs failures quietly while unreachable, not as console errors", () => {
            const error = jest
                .spyOn(console, "error")
                .mockImplementation(() => undefined);
            const warn = jest
                .spyOn(console, "warn")
                .mockImplementation(() => undefined);
            logger.setLevel("debug");
            jest.spyOn(console, "debug").mockImplementation(() => undefined);
            state().setServiceUnavailable(true);
            logger.error("expected");
            logger.warn("expected");
            expect(error).not.toHaveBeenCalled();
            expect(warn).not.toHaveBeenCalled();
            state().setServiceUnavailable(false);
            logger.error("real");
            expect(error).toHaveBeenCalledTimes(1);
            logger.setLevel("silent");
        });
    });

    describe("error banners", () => {
        const add = () =>
            useErrorStore
                .getState()
                .addError(ErrorType.Retrieve, ErrorSourceType.User);

        it("show normally when connected", () => {
            add();
            expect(useErrorStore.getState().errors).toHaveLength(1);
        });

        it("stay quiet offline: the session banner explains it", () => {
            useConnectivityStore.setState({ isOnline: false });
            add();
            useErrorStore.getState().addCustomError("x", ErrorSourceType.User);
            expect(useErrorStore.getState().errors).toHaveLength(0);
        });
    });

    describe("saved configuration", () => {
        const fail = () =>
            jest
                .spyOn(services.config, "getConfiguration")
                .mockRejectedValue(new Error("offline"));

        it("is saved on success and used offline with device security", async () => {
            jest.spyOn(services.config, "getConfiguration").mockResolvedValue(
                configFixture
            );
            await useConfigStore.getState().retrieve();
            jest.restoreAllMocks();
            fail();
            useConfigStore.setState({ config: undefined });
            await expect(
                useConfigStore.getState().retrieve(true)
            ).resolves.toEqual(configFixture);
            expect(useConfigStore.getState().status).toBe("loaded");
        });

        it.each([
            ["damaged", "{ not json"],
            ["incomplete", JSON.stringify({ webClient: {} })],
            [
                "no notification centre toggle",
                JSON.stringify({
                    ...configFixture,
                    webClient: {
                        ...configFixture.webClient,
                        featureToggleConfiguration: {
                            ...configFixture.webClient
                                .featureToggleConfiguration,
                            notificationCentre: undefined,
                        },
                    },
                }),
            ],
            [
                "maintenance window with no whitelist",
                JSON.stringify({
                    ...configFixture,
                    webClient: {
                        ...configFixture.webClient,
                        offlineMode: {
                            startDateTime: "2026-01-01T00:00:00Z",
                            whitelist: null,
                        },
                    },
                }),
            ],
            [
                "present but unusable",
                JSON.stringify({
                    openIdConnect: {},
                    webClient: {},
                    serviceEndpoints: {},
                    identityProviders: [],
                }),
            ],
        ])(
            "a %s saved copy counts as none, and is dropped",
            async (_n, saved) => {
                await AsyncStorage.setItem(savedConfigKey(), saved);
                fail();
                await expect(
                    useConfigStore.getState().retrieve(true)
                ).rejects.toThrow("offline");
                expect(useConfigStore.getState().status).toBe("error");
                await expect(
                    AsyncStorage.getItem(savedConfigKey())
                ).resolves.toBeNull();
            }
        );

        it("each environment reads only its own saved copy", () => {
            expect(savedConfigKey("https://dev/configuration")).not.toBe(
                savedConfigKey("https://test/configuration")
            );
        });

        it("an older request answering late does not replace a newer one", async () => {
            let answerOld: (c: typeof configFixture) => void = () => undefined;
            const newer = { ...configFixture, marker: "newer" };
            jest.spyOn(services.config, "getConfiguration")
                .mockReturnValueOnce(
                    new Promise((resolve) => {
                        answerOld = resolve;
                    })
                )
                .mockResolvedValueOnce(newer);
            const old = useConfigStore.getState().retrieve();
            await useConfigStore.getState().retrieve();
            answerOld(configFixture);
            await old;
            expect(useConfigStore.getState().config).toBe(newer);
        });

        it("is not used without device security: the error page stays", async () => {
            jest.spyOn(services.config, "getConfiguration").mockResolvedValue(
                configFixture
            );
            await useConfigStore.getState().retrieve();
            jest.restoreAllMocks();
            fail();
            await expect(
                useConfigStore.getState().retrieve()
            ).rejects.toThrow();
        });

        it("still fails with nothing saved", async () => {
            fail();
            await expect(
                useConfigStore.getState().retrieve(true)
            ).rejects.toThrow();
        });
    });
});
