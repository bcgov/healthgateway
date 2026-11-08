import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { useNoteDialogStore } from "@/stores/note.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    requestResult,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { press, render, screen, unmount } from "@/test/ui";
import { TimelineView } from "@/views/timeline/TimelineView";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("TimelineView (UserTimelineView.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
        updateStores(() =>
            useConfigStore.setState({
                config: {
                    ...configFixture,
                    webClient: {
                        ...configFixture.webClient,
                        featureToggleConfiguration: {
                            ...configFixture.webClient
                                .featureToggleConfiguration,
                            datasets: [{ name: "Note", enabled: true }],
                        },
                    },
                },
            })
        );
        jest.spyOn(services.userNote, "getNotes").mockResolvedValue(
            requestResult([])
        );
    });
    afterEach(async () => {
        await unmount();
        resetStores();
        updateStores(() => useNoteDialogStore.getState().closeDialog());
        jest.restoreAllMocks();
    });

    it("Health Records title (#subject) with Add a Note, which opens the note dialog", async () => {
        await render(<TimelineView />);
        expect(screen.getByTestId("addNoteBtn")).toBeTruthy();
        await press(screen.getByTestId("addNoteBtn"));
        expect(useNoteDialogStore.getState().open).toBe(true);
    });
});
