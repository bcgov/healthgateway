import { afterEach, describe, expect, it, jest } from "@jest/globals";

import { HttpError, ResultError } from "@/models/errors";
import { ConfigService } from "@/services/config.service";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { useLayoutStore } from "@/stores/layout.store";
import { configFixture, FakeHttp, resetStores } from "@/test/fixtures";

describe("config store retrieve", () => {
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("loads the configuration and marks it loaded", async () => {
        jest.spyOn(services.config, "getConfiguration").mockResolvedValue(
            configFixture
        );
        await expect(useConfigStore.getState().retrieve()).resolves.toBe(
            configFixture
        );
        expect(useConfigStore.getState()).toMatchObject({
            status: "loaded",
            config: configFixture,
        });
    });

    it("records the error and rethrows when it cannot load", async () => {
        jest.spyOn(services.config, "getConfiguration").mockRejectedValue(
            new ResultError("ClientApp-CI-HGU", "offline", 503)
        );
        await expect(useConfigStore.getState().retrieve()).rejects.toThrow(
            "offline"
        );
        expect(useConfigStore.getState()).toMatchObject({
            status: "error",
            errorMessage: "offline",
            config: undefined,
        });
    });

    it("ConfigService translates transport errors", async () => {
        const svc = new ConfigService(
            new FakeHttp().fail(new HttpError("down", 502))
        );
        const e = (await svc
            .getConfiguration("https://cfg.test")
            .catch((x: unknown) => x)) as ResultError;
        expect(e.errorCode).toBe("ClientApp-CI-HGU");
        expect(e.statusCode).toBe(502);
    });
});

describe("layout store", () => {
    it("toggles the sidebar from its responsive default and tracks the idle flag", () => {
        useLayoutStore.setState({ sidebarOpen: undefined, isIdle: false });
        useLayoutStore.getState().toggleSidebar(false); // desktop default open → closed
        expect(useLayoutStore.getState().sidebarOpen).toBe(false);
        useLayoutStore.getState().toggleSidebar(false);
        expect(useLayoutStore.getState().sidebarOpen).toBe(true);
        useLayoutStore.getState().setSidebar(false);
        expect(useLayoutStore.getState().sidebarOpen).toBe(false);
        useLayoutStore.getState().setIsIdle(true);
        expect(useLayoutStore.getState().isIdle).toBe(true);
    });
});
