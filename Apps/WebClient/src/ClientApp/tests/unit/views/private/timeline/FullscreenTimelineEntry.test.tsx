import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform, StyleSheet } from "react-native";
import { SafeAreaProvider } from "react-native-safe-area-context";

import { NoteTimelineEntry } from "@/models/timeline/noteTimelineEntry";
import { services } from "@/services/container";
import { useTimelineStore } from "@/stores/timeline.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { press, render, screen, waitFor } from "@/test/ui";
import { FullscreenTimelineEntry } from "@/views/private/timeline/FullscreenTimelineEntry";

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

describe("FullscreenTimelineEntry", () => {
    beforeEach(() => {
        setBreakpoint(390);
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        updateStores(() => useTimelineStore.getState().closeFullscreenEntry());
        resetStores();
    });

    const open = async () => {
        await render(<FullscreenTimelineEntry hdid="HDID1" />);
        updateStores(() =>
            useTimelineStore.getState().openFullscreenEntry(entry)
        );
        await waitFor(() =>
            expect(screen.getByTestId("entryDetailsCard")).toBeTruthy()
        );
    };

    it("has the swipe strip on iOS only", async () => {
        await open();
        if (Platform.OS === "ios")
            expect(screen.getByTestId("swipeBackStrip")).toBeTruthy();
        else expect(screen.queryByTestId("swipeBackStrip")).toBeNull();
    });

    it("iOS: the swipe strip still reaches the screen's edge in landscape", async () => {
        if (Platform.OS !== "ios") return;
        await render(
            <SafeAreaProvider
                initialMetrics={{
                    insets: { top: 0, right: 62, bottom: 21, left: 62 },
                    frame: { x: 0, y: 0, width: 956, height: 440 },
                }}
            >
                <FullscreenTimelineEntry hdid="HDID1" />
            </SafeAreaProvider>
        );
        updateStores(() =>
            useTimelineStore.getState().openFullscreenEntry(entry)
        );
        const strip = await waitFor(() => screen.getByTestId("swipeBackStrip"));
        const props = (strip as unknown as { props: { style: unknown } }).props;
        expect(StyleSheet.flatten(props.style as never)).toMatchObject({
            left: -62,
            width: 86,
        });
    });

    it("closes with the back button", async () => {
        if (Platform.OS === "web") return; // web closes through browser history
        await open();
        await press(screen.getByTestId("backBtn"));
        expect(useTimelineStore.getState().fullscreenEntry).toBeUndefined();
    });
});
