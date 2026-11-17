import { describe, expect, it } from "@jest/globals";

import { keepsScreenBeneath } from "@/navigation/RootNavigator";

const pages = { name: "(pages)" };
const tabs = { name: "(tabs)" };
const settings = { name: "(settings)" };
const sheet = { name: "loginProviders" };

describe("root stack: which screen stays beneath", () => {
    it("Security and Terms of Service keep the page that opened them beneath, in a tab or outside the tabs", () => {
        expect(keepsScreenBeneath([tabs, settings], 1)).toBe(true);
        expect(keepsScreenBeneath([pages, settings], 1)).toBe(true);
    });

    it("signing in or out keeps only where it leads: no tabs left mounted under the pages, nor the reverse", () => {
        expect(keepsScreenBeneath([pages, tabs], 1)).toBe(false);
        expect(keepsScreenBeneath([tabs, pages], 1)).toBe(false);
        expect(keepsScreenBeneath([settings, pages], 1)).toBe(false);
        expect(keepsScreenBeneath([settings, tabs], 1)).toBe(false);
    });

    it("a replace within a group or a deeper stack keeps only the top", () => {
        expect(keepsScreenBeneath([pages, pages], 1)).toBe(false);
        expect(keepsScreenBeneath([pages, tabs, pages], 2)).toBe(false);
        expect(keepsScreenBeneath([pages, tabs, tabs], 2)).toBe(false);
    });

    it("nothing to keep with the first screen on top", () => {
        expect(keepsScreenBeneath([settings], 0)).toBe(false);
        expect(keepsScreenBeneath([pages, settings], 0)).toBe(false);
    });

    it("the login sheet keeps the page that opened it beneath", () => {
        expect(keepsScreenBeneath([pages, sheet], 1)).toBe(true);
        expect(keepsScreenBeneath([sheet], 0)).toBe(false);
    });
});
