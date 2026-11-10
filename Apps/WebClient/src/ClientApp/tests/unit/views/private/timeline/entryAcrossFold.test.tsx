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
import { routerState } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";
import { TimelineEntryView } from "@/views/private/timeline/entries";
import { FullscreenTimelineEntry } from "@/views/private/timeline/FullscreenTimelineEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const note = (id: string, title: string, journalDate: string) =>
    new NoteTimelineEntry({
        id,
        hdId: "HDID1",
        title,
        // long enough to have details: the header shows only a summary of it
        text: `${title} ${"long text ".repeat(30)}`.trim(),
        journalDate,
        version: 1,
    });
const body = (title: string) => `${title} ${"long text ".repeat(30)}`.trim();
const first = note("n1", "First", "2026-10-01");
const second = note("n2", "Second", "2026-09-01");

const fullscreen = () => useTimelineStore.getState().fullscreenEntry;

describe("an open timeline entry across a fold", () => {
    const Timeline = () => (
        <>
            <TimelineEntryView entry={first} hdid="HDID1" />
            <TimelineEntryView entry={second} hdid="HDID1" />
            <FullscreenTimelineEntry hdid="HDID1" />
        </>
    );
    const resize = (width: number, height: number) =>
        updateStores(() => setBreakpoint(width, height));
    const fold = () => resize(466, 678);
    const unfold = () => resize(669, 951);
    const pressCard = (date: RegExp) => press(screen.getByLabelText(date));
    const firstCard = /^2026-Oct-01/;
    const secondCard = /^2026-Sep-01/;

    beforeEach(() => {
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
        it("web: narrowing closes the entry, as the web client does", async () => {
            setBreakpoint(1366, 768);
            await render(<Timeline />);
            await pressCard(firstCard);
            expect(screen.getByText(body("First"))).toBeTruthy();
            resize(800, 768);
            expect(fullscreen()).toBeUndefined();
            expect(screen.queryByText(body("First"))).toBeNull();
        });
        return;
    }

    it("folding moves an expanded entry to full screen", async () => {
        setBreakpoint(669, 951);
        await render(<Timeline />);
        await pressCard(firstCard);
        expect(fullscreen()).toBeUndefined();
        fold();
        expect(fullscreen()).toBe(first);
        expect(screen.getByTestId("entryDetailsCard")).toBeTruthy();
    });

    it("unfolding moves a full-screen entry back to its card, expanded", async () => {
        setBreakpoint(466, 678);
        await render(<Timeline />);
        await pressCard(firstCard);
        expect(fullscreen()).toBe(first);
        unfold();
        expect(fullscreen()).toBeUndefined();
        expect(screen.getByText(body("First"))).toBeTruthy();
        expect(screen.queryByText(body("Second"))).toBeNull();
    });

    it("survives folding and unfolding again and again", async () => {
        setBreakpoint(669, 951);
        await render(<Timeline />);
        await pressCard(firstCard);
        fold();
        unfold();
        fold();
        expect(fullscreen()).toBe(first);
        unfold();
        expect(screen.getByText(body("First"))).toBeTruthy();
    });

    it("carries the entry expanded last, and collapses the others", async () => {
        setBreakpoint(669, 951);
        await render(<Timeline />);
        await pressCard(firstCard);
        await pressCard(secondCard);
        fold();
        expect(fullscreen()).toBe(second);
        unfold();
        expect(screen.getByText(body("Second"))).toBeTruthy();
        expect(screen.queryByText(body("First"))).toBeNull();
    });

    it("does not bring back an entry closed with Back", async () => {
        setBreakpoint(669, 951);
        await render(<Timeline />);
        await pressCard(firstCard);
        fold();
        await press(screen.getByTestId("backBtn"));
        expect(fullscreen()).toBeUndefined();
        unfold();
        expect(screen.queryByText(body("First"))).toBeNull();
    });

    it("opens nothing when the entry was collapsed before folding", async () => {
        setBreakpoint(669, 951);
        await render(<Timeline />);
        await pressCard(firstCard);
        await pressCard(firstCard);
        fold();
        expect(fullscreen()).toBeUndefined();
    });

    it("does not rise over another tab", async () => {
        setBreakpoint(669, 951);
        await render(<Timeline />);
        await pressCard(firstCard);
        routerState.focused = false;
        // a redraw at the same layout, as leaving the tab causes
        resize(669, 950);
        fold();
        expect(fullscreen()).toBeUndefined();
    });
});
