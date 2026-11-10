import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { AccessibilityInfo, Platform } from "react-native";

import { resetAnnouncements } from "@/components/common/announce";
import { Announcer } from "@/components/common/Announcer";
import { IdleDialog } from "@/components/site/IdleDialog";
import { advanceTimers, inAct, render, screen, textOf } from "@/test/ui";

describe("IdleDialog", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });
    afterEach(async () => {
        // the Announcer is still mounted here: an un-acted update would warn
        await inAct(() => resetAnnouncements());
        jest.restoreAllMocks();
        jest.useRealTimers();
    });

    it("announces the countdown once as it opens; the ticking text itself is not live", async () => {
        const say = jest
            .spyOn(AccessibilityInfo, "announceForAccessibility")
            .mockImplementation(() => undefined);
        await render(
            <>
                <Announcer />
                <IdleDialog
                    countdown={60000}
                    onStillHere={() => undefined}
                    onExpired={() => undefined}
                />
            </>
        );
        const message = "You will be automatically logged out in 60 seconds.";
        expect(screen.getByText(message)).toBeTruthy();
        await advanceTimers(100);
        if (Platform.OS === "web")
            expect(textOf(screen.getByTestId("announcer-assertive"))).toBe(
                message
            );
        else expect(say).toHaveBeenCalledWith(message);
        await advanceTimers(2000);
        expect(
            screen.getByText(
                "You will be automatically logged out in 58 seconds."
            )
        ).toBeTruthy();
        if (Platform.OS === "web")
            expect(textOf(screen.getByTestId("announcer-assertive"))).toBe(
                message
            );
        else expect(say).toHaveBeenCalledTimes(1);
    });
});
