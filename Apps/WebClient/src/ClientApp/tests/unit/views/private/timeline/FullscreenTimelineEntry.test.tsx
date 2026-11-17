import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { NoteTimelineEntry } from "@/models/timeline/noteTimelineEntry";
import { services } from "@/services/container";
import { useTimelineStore } from "@/stores/timeline.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter, router, routerState } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { render, screen, waitFor } from "@/test/ui";
import {
    entryPagePath,
    FullscreenTimelineEntry,
} from "@/views/private/timeline/FullscreenTimelineEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const entry = new NoteTimelineEntry({
    id: "n1",
    hdId: "HDID1",
    title: "Flu shot",
    text: "Left arm",
    journalDate: "2026-10-01",
    version: 1,
});

const openEntry = () =>
    updateStores(() => useTimelineStore.getState().openFullscreenEntry(entry));

describe("FullscreenTimelineEntry", () => {
    beforeEach(() => {
        setBreakpoint(390);
        resetRouter("/timeline");
        routerState.focused = true;
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        updateStores(() => useTimelineStore.getState().closeFullscreenEntry());
        routerState.focused = true;
        resetStores();
    });

    if (Platform.OS === "web") {
        it("web: opens the web client's full-screen dialog, with its back button", async () => {
            await render(<FullscreenTimelineEntry hdid="HDID1" />);
            openEntry();
            await waitFor(() =>
                expect(screen.getByTestId("entryDetailsCard")).toBeTruthy()
            );
            expect(screen.getByTestId("backBtn")).toBeTruthy();
            expect(router.push).not.toHaveBeenCalled();
        });
        return;
    }

    describe("native: the entry's own page", () => {
        it("is pushed beside the timeline that opened it, for whose records they are", async () => {
            await render(
                <FullscreenTimelineEntry hdid="HDID1" commentsAreEnabled />
            );
            openEntry();
            expect(router.push).toHaveBeenCalledWith("/timeline/entry");
            expect(useTimelineStore.getState().entryPageFor).toEqual({
                hdid: "HDID1",
                commentsAreEnabled: true,
            });
            expect(useTimelineStore.getState().entryPageInstant).toBe(false);
            expect(screen.queryByTestId("entryDetailsCard")).toBeNull();
        });

        it("a dependent's timeline pushes its own", async () => {
            resetRouter("/dependents/DEP1/timeline");
            await render(<FullscreenTimelineEntry hdid="DEP1" />);
            openEntry();
            expect(router.push).toHaveBeenCalledWith("/dependents/DEP1/entry");
        });

        it("not from a timeline that isn't on screen (the other tab's)", async () => {
            routerState.focused = false;
            await render(<FullscreenTimelineEntry hdid="HDID1" />);
            openEntry();
            expect(router.push).not.toHaveBeenCalled();
        });

        it("not on a wide window, where the entry opens in its card", async () => {
            setBreakpoint(1366, 1024);
            await render(<FullscreenTimelineEntry hdid="HDID1" />);
            openEntry();
            expect(router.push).not.toHaveBeenCalled();
        });

        it("not again when the timeline is back on screen with the entry still open (leaving the page)", async () => {
            await render(<FullscreenTimelineEntry hdid="HDID1" />);
            openEntry();
            expect(router.push).toHaveBeenCalledTimes(1);
            // redraws at the same layout, as the page covering the timeline and going again cause
            routerState.focused = false;
            updateStores(() => setBreakpoint(390, 800));
            routerState.focused = true;
            updateStores(() => setBreakpoint(390, 844));
            expect(router.push).toHaveBeenCalledTimes(1);
        });
    });

    it("the page's path beside each timeline", () => {
        expect(entryPagePath("/timeline")).toBe("/timeline/entry");
        expect(entryPagePath("/dependents/DEP1/timeline")).toBe(
            "/dependents/DEP1/entry"
        );
    });
});
