import { afterEach, describe, expect, it, jest } from "@jest/globals";
import Constants from "expo-constants";

import { AppVersion } from "@/components/common/AppVersion";
import { render, screen, textOf } from "@/test/ui";

jest.mock("expo-constants", () => ({
    __esModule: true,
    default: { expoConfig: { version: "3.0.1", extra: { build: 57 } } },
}));

describe("AppVersion", () => {
    const config = Constants.expoConfig as {
        version?: string;
        extra?: Record<string, unknown>;
    };

    afterEach(() => {
        config.version = "3.0.1";
        config.extra = { build: 57 };
    });

    it("shows the run's name: v major.minor.fix and the build to 4 digits", async () => {
        await render(<AppVersion />);
        expect(textOf(screen.getByTestId("appVersion"))).toBe("v3.0.1.0057");
    });

    it("shows a build not from dev as Custom", async () => {
        config.extra = { build: 57, versionLabel: "Custom" };
        await render(<AppVersion />);
        expect(textOf(screen.getByTestId("appVersion"))).toBe(
            "v3.0.1.0057 - Custom"
        );
    });

    it("shows a local build as v0.0.0.0000", async () => {
        config.version = "0.0.0";
        config.extra = { build: 0 };
        await render(<AppVersion />);
        expect(textOf(screen.getByTestId("appVersion"))).toBe("v0.0.0.0000");
    });

    it("without a build number, shows the version alone", async () => {
        config.extra = undefined;
        await render(<AppVersion />);
        expect(textOf(screen.getByTestId("appVersion"))).toBe("v3.0.1");
    });

    it("shows nothing without a version", async () => {
        config.version = undefined;
        await render(<AppVersion />);
        expect(screen.queryByTestId("appVersion")).toBeNull();
    });
});
