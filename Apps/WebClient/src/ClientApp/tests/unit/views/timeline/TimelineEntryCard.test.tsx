import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { act } from "@testing-library/react-native";
import { Platform, StyleSheet, View } from "react-native";

import { NoteTimelineEntry } from "@/models/timeline/noteTimelineEntry";
import { useTimelineStore } from "@/stores/timeline.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetStores, updateStores } from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";
import { TimelineEntryCard } from "@/views/timeline/TimelineEntryCard";

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

describe("TimelineEntryCard", () => {
    beforeEach(() => setBreakpoint(1366));
    afterEach(() => {
        updateStores(() => useTimelineStore.getState().closeFullscreenEntry());
        resetStores();
    });

    const renderCard = (isMobileDetails = false, testID = "card") =>
        render(
            <TimelineEntryCard
                entry={entry}
                title="Flu shot"
                testID={testID}
                isMobileDetails={isMobileDetails}
                headerMenu={<View testID="menu" accessibilityRole="button" />}
            >
                <View testID="details" />
            </TimelineEntryCard>
        );

    it("draws the entry icon like the web client's x-large v-icon: 30px tall in Font Awesome's 1.25em box", async () => {
        // the icon font is mocked as text; on native the fixed-width box around it is a real view
        if (Platform.OS === "web") return; // Icon.web's box is covered in IconFixedWidth.test.tsx
        await renderCard();
        const glyph = screen.getByText(/^icon:/) as unknown as {
            parent: { props: { style?: unknown } } | null;
        };
        let node = glyph.parent;
        while (node && !node.props.style)
            node = (node as unknown as typeof glyph).parent;
        expect(StyleSheet.flatten(node?.props.style as never)).toMatchObject({
            width: 37.5,
            height: 30,
        });
    });

    it("has one child, for `.children().within()`", async () => {
        if (Platform.OS !== "web") return; // Cypress runs on web only
        await renderCard(true);
        const card = screen.getByTestId("card") as unknown as HTMLElement;
        expect(card.children).toHaveLength(1);
    });

    describe("at the open iPhone Duo's 669 by 951", () => {
        beforeEach(() => setBreakpoint(669, 951));

        const pressHeader = async () =>
            press(screen.getByLabelText(/^2026-Oct-01/));

        if (Platform.OS === "web") {
            it("web: opens full screen, as the web client does under 960", async () => {
                await renderCard();
                await pressHeader();
                expect(useTimelineStore.getState().fullscreenEntry).toBe(entry);
                expect(screen.queryByTestId("details")).toBeNull();
            });
        } else {
            it("native: expands in place and collapses again", async () => {
                await renderCard();
                await pressHeader();
                expect(screen.getByTestId("details")).toBeTruthy();
                expect(
                    useTimelineStore.getState().fullscreenEntry
                ).toBeUndefined();
                await pressHeader();
                expect(screen.queryByTestId("details")).toBeNull();
            });

            it("native: collapses when the Duo folds, and stays collapsed when it opens", async () => {
                await render(
                    <TimelineEntryCard entry={entry} title="Flu shot">
                        <View testID="details" />
                    </TimelineEntryCard>
                );
                await pressHeader();
                expect(screen.getByTestId("details")).toBeTruthy();

                updateStores(() => setBreakpoint(466, 678));
                expect(screen.queryByTestId("details")).toBeNull();

                updateStores(() => setBreakpoint(669, 951));
                expect(screen.queryByTestId("details")).toBeNull();
            });
        }
    });

    it("opens full screen on a phone in landscape", async () => {
        setBreakpoint(874, 402);
        await renderCard();
        if (Platform.OS === "web") return; // covered by the web cases above
        await press(screen.getByLabelText(/^2026-Oct-01/));
        expect(useTimelineStore.getState().fullscreenEntry).toBe(entry);
        expect(screen.queryByTestId("details")).toBeNull();
    });

    if (Platform.OS !== "web") return;

    const click = (id: string) =>
        act(async () => {
            (screen.getByTestId(id) as unknown as HTMLElement).click();
        });

    it("web: a click sent straight to the title opens and closes the details", async () => {
        await renderCard();
        expect(screen.queryByTestId("details")).toBeNull();
        await click("entryCardDetailsTitle");
        expect(screen.getByTestId("details")).toBeTruthy();
        await click("entryCardDetailsTitle");
        expect(screen.queryByTestId("details")).toBeNull();
    });

    it("web: a click on a button in the header leaves the details alone", async () => {
        await renderCard();
        await click("menu");
        expect(screen.queryByTestId("details")).toBeNull();
    });
});
