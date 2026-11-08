import { afterEach, describe, expect, it, jest } from "@jest/globals";
import Constants from "expo-constants";

import { AppVersion } from "@/components/ui/AppVersion";
import { render, screen, textOf } from "@/test/ui";

jest.mock("expo-constants", () => ({
    __esModule: true,
    default: { expoConfig: { version: "1.2.345" } },
}));

describe("AppVersion", () => {
    const config = Constants.expoConfig as { version?: string };

    afterEach(() => {
        config.version = "1.2.345";
    });

    it("shows major.minor (build)", async () => {
        await render(<AppVersion />);
        expect(textOf(screen.getByTestId("appVersion"))).toBe(
            "Version 1.2 (345)"
        );
    });

    it("shows a build not from dev as Custom", async () => {
        const constants = Constants as { expoConfig: { extra?: unknown } };
        constants.expoConfig.extra = { versionLabel: "Custom" };
        await render(<AppVersion />);
        constants.expoConfig.extra = undefined;
        expect(textOf(screen.getByTestId("appVersion"))).toBe(
            "Version 1.2 (345) - Custom"
        );
    });

    it("shows a local build's 0.0.0 as 0.0 (0)", async () => {
        config.version = "0.0.0";
        await render(<AppVersion />);
        expect(textOf(screen.getByTestId("appVersion"))).toBe(
            "Version 0.0 (0)"
        );
    });

    it("shows nothing without a version", async () => {
        config.version = undefined;
        await render(<AppVersion />);
        expect(screen.queryByTestId("appVersion")).toBeNull();
    });
});
