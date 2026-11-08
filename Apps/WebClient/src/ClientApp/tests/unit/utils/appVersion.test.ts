import { describe, expect, it } from "@jest/globals";

import { isUpdateRequired } from "@/utils/appVersion";

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
