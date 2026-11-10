import { describe, expect, it } from "@jest/globals";

import { formatAppVersion, isUpdateRequired } from "@/utils/appVersion";

describe("formatAppVersion", () => {
    it.each([
        ["3.0.0", 57, "v3.0.0.0057"],
        ["2.1.4", 1234, "v2.1.4.1234"],
        ["2.1.4", 12345, "v2.1.4.12345"],
        ["0.0.0", 0, "v0.0.0.0000"],
        [" 1.0.7 ", 7, "v1.0.7.0007"],
    ])(
        "%s build %s shows as %s, the run's name",
        (version, build, expected) => {
            expect(formatAppVersion(version, build)).toBe(expected);
        }
    );

    it.each([
        ["3.0.0", undefined, "v3.0.0"],
        ["3.0.0", -1, "v3.0.0"],
        ["3.0.0", 1.5, "v3.0.0"],
        ["1.2", 7, "1.2"],
        ["dev", 7, "dev"],
    ])("%s build %s shows what there is: %s", (version, build, expected) => {
        expect(formatAppVersion(version, build)).toBe(expected);
    });

    it("puts a label after the version", () => {
        expect(formatAppVersion("3.0.0", 12, "Custom")).toBe(
            "v3.0.0.0012 - Custom"
        );
        expect(formatAppVersion("3.0.0", 12, "")).toBe("v3.0.0.0012");
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
