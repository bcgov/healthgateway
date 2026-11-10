import { afterEach, describe, expect, it, jest } from "@jest/globals";

import { services } from "@/services/container";
import {
    DEFAULT_MOBILE_IDLE_MS,
    selectIdleTimeout,
    selectUpdateRequired,
    useConfigStore,
} from "@/stores/config.store";
import { configFixture } from "@/test/fixtures";

jest.mock("expo-constants", () => ({
    __esModule: true,
    default: { expoConfig: { version: "1.2.3" } },
}));

const withMobile = (mobile: unknown) =>
    useConfigStore.setState({
        config: { ...configFixture, mobile } as never,
    });
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

        it.each([[undefined], [{}], [{ minimumVersion: "" }]])(
            "ignores the check when missing: %j",
            (mobile) => {
                withMobile(mobile);
                expect(selectUpdateRequired(state())).toBe(false);
            }
        );
    });

    describe("timeouts.idle", () => {
        it("uses the configured milliseconds", () => {
            withMobile({ timeouts: { idle: 60000 } });
            expect(selectIdleTimeout(state())).toBe(60000);
        });

        it.each([
            [undefined],
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

    describe("refresh", () => {
        it("picks up a new minimum version while running", async () => {
            withMobile(undefined);
            jest.spyOn(services.config, "getConfiguration").mockResolvedValue({
                ...configFixture,
                mobile: { minimumVersion: "9.0.0" },
            });
            await state().refresh();
            expect(selectUpdateRequired(state())).toBe(true);
        });

        it("keeps the current configuration when it cannot be read", async () => {
            withMobile({ minimumVersion: "1.0.0" });
            jest.spyOn(services.config, "getConfiguration").mockRejectedValue(
                new Error("offline")
            );
            await state().refresh();
            expect(state().config?.mobile?.minimumVersion).toBe("1.0.0");
        });
    });
});
