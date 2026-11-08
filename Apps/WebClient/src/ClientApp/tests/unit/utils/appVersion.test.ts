import { describe, expect, it } from "@jest/globals";

import { formatAppVersion, isUpdateRequired } from "@/utils/appVersion";

describe("formatAppVersion", () => {
    it.each([
        ["2.1.1234", "2.1 (1234)"],
        ["0.0.0", "0.0 (0)"],
        [" 1.0.7 ", "1.0 (7)"],
        ["1.2", "1.2"],
        ["dev", "dev"],
    ])("%s shows as %s", (version, expected) => {
        expect(formatAppVersion(version)).toBe(expected);
    });

    it("puts a label after the build number", () => {
        expect(formatAppVersion("0.0.12", "Custom")).toBe("0.0 (12) - Custom");
        expect(formatAppVersion("0.0.12", "")).toBe("0.0 (12)");
    });
});

describe("isUpdateRequired", () => {
    it.each([
        ["0.1.1", "0.1.2", true],
        ["0.1.9", "0.2.0", true],
        ["0.9.9", "1.0.0", true],
        ["1.2.3", "1.2.3", false],
        ["1.2.4", "1.2.3", false],
        ["2.0.0", "1.9.9", false],
        ["0.1.10", "0.1.9", false],
        ["1.2", "1.2.1", true],
        ["1.2.0", "1.2", false],
    ])("%s against minimum %s: %s", (current, minimum, expected) => {
        expect(isUpdateRequired(current, minimum)).toBe(expected);
    });

    it.each([
        ["1.0.0", undefined],
        ["1.0.0", ""],
        ["1.0.0", "latest"],
        [undefined, "1.0.0"],
    ])("ignores %s against %s", (current, minimum) => {
        expect(isUpdateRequired(current, minimum)).toBe(false);
    });
});
