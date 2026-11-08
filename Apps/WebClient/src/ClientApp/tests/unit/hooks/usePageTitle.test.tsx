import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { resetAnnouncements } from "@/components/ui/announce";
import { Announcer } from "@/components/ui/Announcer";
import { resetPageTitles, usePageTitle } from "@/hooks/usePageTitle";
import { advanceTimers, inAct, render, screen, textOf } from "@/test/ui";

function Titled({ title }: { title: string }) {
    usePageTitle(title);
    return null;
}

describe("usePageTitle", () => {
    beforeEach(() => {
        jest.useFakeTimers();
        resetPageTitles();
    });
    afterEach(async () => {
        await inAct(() => resetAnnouncements());
        jest.useRealTimers();
    });

    it("web: the document title carries the site name, and a change of page is announced but the first load is not", async () => {
        if (Platform.OS !== "web") return;
        const polite = () => textOf(screen.getByTestId("announcer-polite"));
        await render(
            <>
                <Announcer />
                <Titled title="Home" />
            </>
        );
        expect(document.title).toBe("Home | Health Gateway");
        await advanceTimers(100);
        expect(polite()).toBe("");
        // the next page, after a client-side route change
        await render(
            <>
                <Announcer />
                <Titled title="Profile" />
            </>
        );
        expect(document.title).toBe("Profile | Health Gateway");
        await advanceTimers(100);
        expect(polite()).toBe("Profile");
        // the landing page is the site itself
        await render(<Titled title="Health Gateway" />);
        expect(document.title).toBe("Health Gateway");
    });

    it("native: nothing to do", async () => {
        if (Platform.OS === "web") return;
        await render(<Titled title="Home" />);
        expect(true).toBe(true);
    });
});
