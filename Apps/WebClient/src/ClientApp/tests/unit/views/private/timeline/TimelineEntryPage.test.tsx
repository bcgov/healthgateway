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
import { router } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { render, screen, unmount } from "@/test/ui";
import { TimelineEntryPage } from "@/views/private/timeline/TimelineEntryPage";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
const mockNavigation = {
    setOptions: jest.fn(),
    goBack: jest.fn(),
    canGoBack: jest.fn(() => true),
};
jest.mock("expo-router/react-navigation", () => ({
    ...jest.requireActual<object>("expo-router/react-navigation"),
    useNavigation: () => mockNavigation,
}));

const note = new NoteTimelineEntry({
    id: "n1",
    hdId: "HDID1",
    title: "Flu shot",
    text: "Left arm",
    journalDate: "2026-10-01",
    version: 1,
});

/** As the timeline leaves it when it pushes the page (`FullscreenTimelineEntry`). */
const opened = () =>
    updateStores(() => {
        useTimelineStore.getState().openFullscreenEntry(note);
        useTimelineStore.setState({
            entryPageFor: { hdid: "HDID1", commentsAreEnabled: false },
        });
    });

describe("TimelineEntryPage (native)", () => {
    beforeEach(() => {
        setBreakpoint(390);
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
        mockNavigation.setOptions.mockClear();
        mockNavigation.goBack.mockClear();
        mockNavigation.canGoBack.mockReturnValue(true);
    });
    afterEach(async () => {
        await unmount();
        updateStores(() => useTimelineStore.getState().closeFullscreenEntry());
        resetStores();
    });

    // web opens entries in a dialog and never comes here
    if (Platform.OS === "web") {
        it("is not used on web", () => undefined);
        return;
    }

    it("shows the entry, titled with its record type, with an unlabelled back button", async () => {
        opened();
        await render(<TimelineEntryPage timelinePath="/timeline" />);
        expect(screen.getByTestId("entryDetailsCard")).toBeTruthy();
        expect(mockNavigation.setOptions).toHaveBeenCalledWith({
            title: "Note",
            headerBackButtonDisplayMode: "minimal",
        });
        expect(mockNavigation.goBack).not.toHaveBeenCalled();
    });

    it("leaving it closes the entry", async () => {
        opened();
        await render(<TimelineEntryPage timelinePath="/timeline" />);
        await unmount();
        expect(useTimelineStore.getState().fullscreenEntry).toBeUndefined();
        expect(useTimelineStore.getState().entryPageFor).toBeUndefined();
    });

    it("widening goes back to the timeline without the animation, the entry still open for its card", async () => {
        opened();
        await render(<TimelineEntryPage timelinePath="/timeline" />);
        updateStores(() => setBreakpoint(1366, 1024));
        expect(useTimelineStore.getState().entryPageInstant).toBe(true);
        expect(mockNavigation.goBack).toHaveBeenCalledTimes(1);
        expect(useTimelineStore.getState().fullscreenEntry).toBe(note);
    });

    it("keeps its own entry when the other tab's page takes the selection, and leaving clears only its own", async () => {
        opened();
        await render(<TimelineEntryPage timelinePath="/timeline" />);
        const dependentNote = new NoteTimelineEntry({
            id: "n2",
            hdId: "DEP1",
            title: "Dependent",
            text: "Theirs",
            journalDate: "2026-09-01",
            version: 1,
        });
        // the Dependents tab opens one of a dependent's entries on its own page
        updateStores(() => {
            useTimelineStore.getState().openFullscreenEntry(dependentNote);
            useTimelineStore.setState({
                entryPageFor: { hdid: "DEP1", commentsAreEnabled: false },
            });
        });
        expect(screen.getByText("Flu shot")).toBeTruthy();
        expect(screen.queryByText("Dependent")).toBeNull();
        // and its page goes: this one stays as it is
        updateStores(() => useTimelineStore.getState().closeFullscreenEntry());
        expect(screen.getByText("Flu shot")).toBeTruthy();
        expect(mockNavigation.goBack).not.toHaveBeenCalled();
        // this one leaving doesn't clear a selection that isn't its own
        updateStores(() =>
            useTimelineStore.getState().openFullscreenEntry(dependentNote)
        );
        await unmount();
        expect(useTimelineStore.getState().fullscreenEntry).toBe(dependentNote);
    });

    it("goes back when its note is edited or deleted, as web's dialog closes", async () => {
        opened();
        await render(<TimelineEntryPage timelinePath="/timeline" />);
        updateStores(() =>
            useTimelineStore.setState({ updatedEntry: { id: "other" } })
        );
        expect(mockNavigation.goBack).not.toHaveBeenCalled();
        updateStores(() =>
            useTimelineStore.setState({ updatedEntry: { id: "n1" } })
        );
        expect(mockNavigation.goBack).toHaveBeenCalledTimes(1);
    });

    it("ignores a change to its note from before it opened", async () => {
        updateStores(() =>
            useTimelineStore.setState({ updatedEntry: { id: "n1" } })
        );
        opened();
        await render(<TimelineEntryPage timelinePath="/timeline" />);
        expect(mockNavigation.goBack).not.toHaveBeenCalled();
    });

    it("with nothing to show goes back to the timeline", async () => {
        await render(<TimelineEntryPage timelinePath="/timeline" />);
        expect(mockNavigation.goBack).toHaveBeenCalledTimes(1);
        expect(screen.queryByTestId("entryDetailsCard")).toBeNull();
    });

    it("with nothing to show and nothing beneath (opened by a link) goes to the timeline", async () => {
        mockNavigation.canGoBack.mockReturnValue(false);
        await render(
            <TimelineEntryPage timelinePath="/dependents/DEP1/timeline" />
        );
        expect(router.replace).toHaveBeenCalledWith(
            "/dependents/DEP1/timeline"
        );
    });
});
