import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { logLegacyCleanup, removeLegacyData } from "@/services/legacyCleanup";
import { logger } from "@/services/logger";

import NativeCleanup from "../../../modules/hg-legacy-cleanup";

// Jest has no native module, which reads as null: stand one in
jest.mock("../../../modules/hg-legacy-cleanup", () => ({
    __esModule: true,
    default: {
        legacyCleanupReport: jest.fn(),
        removeLegacyData: jest.fn(),
    },
}));

const native = jest.mocked(NativeCleanup!);
const group = (name: string, found: number, failed = 0) => ({
    group: name,
    found,
    removed: found - failed,
    failed,
});

describe("legacy cleanup", () => {
    const info = jest.fn();
    const warn = jest.fn();
    const debug = jest.fn();

    beforeEach(() => {
        jest.spyOn(logger, "info").mockImplementation(info);
        jest.spyOn(logger, "warn").mockImplementation(warn);
        jest.spyOn(logger, "debug").mockImplementation(debug);
    });
    afterEach(() => {
        jest.restoreAllMocks();
        jest.clearAllMocks();
    });

    it("logs what the launch removed, by group", async () => {
        native.legacyCleanupReport.mockResolvedValue([
            group("legacyKeychain", 9),
            group("database", 3),
            group("pdf", 0),
        ]);
        await logLegacyCleanup();
        expect(info).toHaveBeenCalledWith(
            "Legacy cleanup: legacyKeychain 9 removed; database 3 removed"
        );
        expect(warn).not.toHaveBeenCalled();
    });

    it("warns when something could not be deleted", async () => {
        native.legacyCleanupReport.mockResolvedValue([
            group("database", 3, 1),
            group("analytics", 2),
        ]);
        await logLegacyCleanup();
        expect(warn).toHaveBeenCalledWith(
            "Legacy cleanup: database 2 removed, 1 failed; analytics 2 removed"
        );
    });

    it("logs quietly on a device that never had the old app", async () => {
        native.legacyCleanupReport.mockResolvedValue([group("database", 0)]);
        await logLegacyCleanup();
        expect(debug).toHaveBeenCalledWith("Legacy cleanup: nothing found");
        expect(info).not.toHaveBeenCalled();
    });

    it("logs nothing once an earlier launch finished the cleanup", async () => {
        native.legacyCleanupReport.mockResolvedValue(null);
        await logLegacyCleanup();
        expect(info).not.toHaveBeenCalled();
        expect(warn).not.toHaveBeenCalled();
        expect(debug).not.toHaveBeenCalled();
    });

    it("does not stop start-up when the report fails", async () => {
        native.legacyCleanupReport.mockRejectedValue(new Error("gone"));
        await expect(logLegacyCleanup()).resolves.toBeUndefined();
        expect(warn).toHaveBeenCalledWith(
            "Legacy cleanup report failed: Error"
        );
    });

    it("removes the old data again at a reset, and logs it", async () => {
        native.removeLegacyData.mockResolvedValue([group("preferences", 4)]);
        await removeLegacyData();
        expect(info).toHaveBeenCalledWith(
            "Legacy cleanup at reset: preferences 4 removed"
        );
    });

    it("does not stop a reset when the deletes fail", async () => {
        native.removeLegacyData.mockRejectedValue(new Error("gone"));
        await expect(removeLegacyData()).resolves.toBeUndefined();
        expect(warn).toHaveBeenCalledWith(
            "Legacy cleanup at reset failed: Error"
        );
    });
});
