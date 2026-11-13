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
        [57, 300, true],
        [0, 1, true],
        [299, 300, true],
        [300, 300, false],
        [301, 300, false],
        [57, 0, false],
        [0, 0, false],
    ])("build %s against minimum %s: %s", (build, minimum, expected) => {
        expect(isUpdateRequired(build, minimum)).toBe(expected);
    });

    it.each([
        [57, undefined],
        [57, null],
        [57, "300"],
        [57, 300.5],
        [57, -1],
        [undefined, 300],
        ["57", 300],
        [-1, 300],
    ])("ignores build %j against %j", (build, minimum) => {
        expect(isUpdateRequired(build, minimum)).toBe(false);
    });
});
