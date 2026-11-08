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

    it("shows major.minor.build", async () => {
        await render(<AppVersion />);
        expect(textOf(screen.getByTestId("appVersion"))).toBe(
            "Version 1.2.345"
        );
    });

    it("shows nothing without a version", async () => {
        config.version = undefined;
        await render(<AppVersion />);
        expect(screen.queryByTestId("appVersion")).toBeNull();
    });
});
