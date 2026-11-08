import { afterEach, describe, expect, it, jest } from "@jest/globals";

import { logger, logLevelFromConfig } from "@/services/logger";

describe("logLevelFromConfig (server webClient.logLevel → client level)", () => {
    it.each([
        ["Verbose", "verbose"],
        ["Trace", "verbose"],
        ["Debug", "debug"],
        ["Information", "info"],
        ["Info", "info"],
        ["Warning", "warn"],
        ["Warn", "warn"],
        ["Error", "error"],
        ["Critical", "error"],
        ["None", "silent"],
    ])("%s → %s", (server, level) => {
        expect(logLevelFromConfig(server)).toBe(level);
    });

    it("falls back to debug in development builds for unknown or missing values", () => {
        expect(logLevelFromConfig(undefined)).toBe(__DEV__ ? "debug" : "warn");
        expect(logLevelFromConfig("Loud")).toBe(__DEV__ ? "debug" : "warn");
    });
});

describe("logger", () => {
    afterEach(() => {
        logger.setLevel("silent"); // tests run with the logger silenced (jest.setup.js)
        jest.restoreAllMocks();
    });

    const spies = () => ({
        debug: jest.spyOn(console, "debug").mockImplementation(() => undefined),
        info: jest.spyOn(console, "info").mockImplementation(() => undefined),
        warn: jest.spyOn(console, "warn").mockImplementation(() => undefined),
        error: jest.spyOn(console, "error").mockImplementation(() => undefined),
    });
    const logAll = () => {
        logger.verbose("v");
        logger.debug("d");
        logger.info("i");
        logger.warn("w");
        logger.error("e", new Error("x"));
    };

    it("writes every level with its prefix at verbose", () => {
        const c = spies();
        logger.setLevel("verbose");
        logAll();
        expect(c.debug).toHaveBeenCalledWith("[verbose] v");
        expect(c.debug).toHaveBeenCalledWith("[debug] d");
        expect(c.info).toHaveBeenCalledWith("[info] i");
        expect(c.warn).toHaveBeenCalledWith("[warn] w");
        expect(c.error).toHaveBeenCalledWith("[error] e", "Error");
    });

    it("drops everything below the level", () => {
        const c = spies();
        logger.setLevel("warn");
        logAll();
        expect(c.debug).not.toHaveBeenCalled();
        expect(c.info).not.toHaveBeenCalled();
        expect(c.warn).toHaveBeenCalledTimes(1);
        expect(c.error).toHaveBeenCalledTimes(1);
    });

    it("writes nothing when silent, and an error without a cause gets an empty second argument", () => {
        const c = spies();
        logger.setLevel("silent");
        logAll();
        expect(Object.values(c).every((s) => s.mock.calls.length === 0)).toBe(
            true
        );
        logger.setLevel("error");
        logger.error("only");
        expect(c.error).toHaveBeenCalledWith("[error] only", "");
    });
});
