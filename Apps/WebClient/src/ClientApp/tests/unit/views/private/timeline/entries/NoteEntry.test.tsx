import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { NoteTimelineEntry } from "@/models/timeline/noteTimelineEntry";
import { services } from "@/services/container";
import { useNoteDialogStore, useNoteStore } from "@/stores/note.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { press, render, screen, textOf, waitFor } from "@/test/ui";
import { confirmAction } from "@/utils/confirm";
import { NoteEntry } from "@/views/private/timeline/entries/NoteEntry";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/confirm", () => ({
    confirmAction: jest.fn(async () => true),
}));

const note = {
    id: "n1",
    hdId: "HDID1",
    title: "Flu shot",
    text: "Left arm",
    journalDate: "2026-10-01",
    version: 1,
};

describe("NoteEntry (NoteTimelineComponent.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
        updateStores(() => useNoteStore.setState({ notes: [note] }));
    });
    afterEach(() => {
        resetStores();
        updateStores(() => useNoteDialogStore.getState().closeDialog());
        jest.restoreAllMocks();
        jest.clearAllMocks();
    });

    it("title and summary in the header; Edit opens the dialog on this note", async () => {
        await render(
            <NoteEntry entry={new NoteTimelineEntry(note)} hdid="HDID1" />
        );
        expect(textOf(screen.getByTestId("entryCardDetailsTitle"))).toBe(
            "Flu shot"
        );
        expect(textOf(screen.getByTestId("entryCardDetailsSubtitle"))).toBe(
            "Left arm"
        );
        await press(screen.getByTestId("noteMenuBtn"));
        await press(screen.getByTestId("editNoteMenuBtn"));
        expect(useNoteDialogStore.getState()).toMatchObject({
            open: true,
            entryId: "n1",
        });
    });

    it("Delete asks first, then deletes the note", async () => {
        const del = jest
            .spyOn(services.userNote, "deleteNote")
            .mockResolvedValue(undefined);
        await render(
            <NoteEntry entry={new NoteTimelineEntry(note)} hdid="HDID1" />
        );
        await press(screen.getByTestId("noteMenuBtn"));
        await press(screen.getByTestId("deleteNoteMenuBtn"));
        await waitFor(() =>
            expect(del).toHaveBeenCalledWith(
                "HDID1",
                expect.objectContaining({ id: "n1" })
            )
        );
        expect(confirmAction).toHaveBeenCalledWith(
            "Are you sure you want to delete this note?"
        );
        expect(useNoteStore.getState().notes).toEqual([]);
    });
});
