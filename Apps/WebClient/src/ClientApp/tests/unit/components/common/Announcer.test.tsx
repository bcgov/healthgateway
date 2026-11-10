import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { AccessibilityInfo, Platform } from "react-native";

import { announce, resetAnnouncements } from "@/components/common/announce";
import { Announcer } from "@/components/common/Announcer";
import {
    advanceTimers,
    attrOf,
    inAct,
    render,
    screen,
    textOf,
} from "@/test/ui";

describe("announce and Announcer", () => {
    afterEach(async () => {
        // the Announcer is still mounted here: an un-acted update would warn
        await inAct(() => resetAnnouncements());
        jest.restoreAllMocks();
        jest.useRealTimers();
    });

    it("native: speaks through the platform", () => {
        if (Platform.OS === "web") return;
        const say = jest
            .spyOn(AccessibilityInfo, "announceForAccessibility")
            .mockImplementation(() => undefined);
        announce("Page loaded");
        expect(say).toHaveBeenCalledWith("Page loaded");
    });

    it("web: writes into the hidden live region of the right politeness, and a repeat still gets through", async () => {
        if (Platform.OS !== "web") return;
        jest.useFakeTimers();
        await render(<Announcer />);
        const polite = () => screen.getByTestId("announcer-polite");
        const assertive = () => screen.getByTestId("announcer-assertive");
        expect(attrOf(polite(), "aria-live")).toBe("polite");
        expect(attrOf(polite(), "role")).toBe("status");
        expect(attrOf(assertive(), "aria-live")).toBe("assertive");
        expect(attrOf(assertive(), "role")).toBe("alert");
        await inAct(() => announce("Page loaded"));
        await advanceTimers(100);
        expect(textOf(polite())).toBe("Page loaded");
        // the same text again: emptied first, so the second one is a change the reader hears
        await inAct(() => announce("Page loaded"));
        expect(textOf(polite())).toBe("");
        await advanceTimers(100);
        expect(textOf(polite())).toBe("Page loaded");
        await inAct(() => announce("Your login has expired", "assertive"));
        await advanceTimers(100);
        expect(textOf(assertive())).toBe("Your login has expired");
        expect(textOf(polite())).toBe("");
    });
});
