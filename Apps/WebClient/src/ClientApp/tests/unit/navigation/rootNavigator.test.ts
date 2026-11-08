import { describe, expect, it } from "@jest/globals";

import { keepsScreenBeneath } from "@/navigation/RootNavigator";

const pages = { name: "(pages)" };
const tabs = { name: "(tabs)" };

describe("root stack: which screen stays beneath", () => {
    it("signed out, the tabs pushed over the signed-out pages keep them beneath for the back swipe", () => {
        expect(keepsScreenBeneath([pages, tabs], 1, true)).toBe(true);
    });

    it("signed in, nothing stays beneath (sign-in lands on the tabs alone)", () => {
        expect(keepsScreenBeneath([pages, tabs], 1, false)).toBe(false);
    });

    it("signing out does not leave the signed-in tabs mounted under the pages", () => {
        expect(keepsScreenBeneath([tabs, pages], 1, true)).toBe(false);
        expect(keepsScreenBeneath([tabs, pages], 1, false)).toBe(false);
    });

    it("a replace within a group or a deeper stack keeps only the top", () => {
        expect(keepsScreenBeneath([pages, pages], 1, true)).toBe(false);
        expect(keepsScreenBeneath([pages, tabs, pages], 2, true)).toBe(false);
        expect(keepsScreenBeneath([pages, tabs, tabs], 2, true)).toBe(false);
    });

    it("only while the tabs are on top", () => {
        expect(keepsScreenBeneath([pages, tabs], 0, true)).toBe(false);
    });
});
