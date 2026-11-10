import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ResultError } from "@/models/errors";
import { NoteTimelineEntry } from "@/models/timeline/noteTimelineEntry";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import { useNoteDialogStore, useNoteStore } from "@/stores/note.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { changeText, press, render, screen, textOf, waitFor } from "@/test/ui";
import { NoteDialog } from "@/views/private/timeline/NoteDialog";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const existing = {
    id: "n1",
    hdId: "HDID1",
    title: "Old",
    text: "Body",
    journalDate: "2026-09-15",
    version: 2,
};

describe("NoteDialog (NoteDialogComponent.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        updateStores(() => useNoteDialogStore.getState().closeDialog());
        jest.restoreAllMocks();
    });

    const openNew = async () => {
        await render(<NoteDialog />);
        updateStores(() => useNoteDialogStore.getState().openDialog());
        await waitFor(() => expect(screen.getByText("Add Note")).toBeTruthy());
    };

    it("Add Note: the title is required; Save creates the note dated today and closes", async () => {
        const create = jest
            .spyOn(services.userNote, "createNote")
            .mockImplementation(async (_h, n) => ({ ...n, id: "new" }));
        await openNew();
        await press(screen.getByTestId("saveNoteBtn"));
        expect(screen.getByText("Title is required.")).toBeTruthy();
        expect(create).not.toHaveBeenCalled();
        await changeText(screen.getByTestId("noteTitleInput"), "Flu shot");
        await changeText(screen.getByTestId("noteTextInput"), "Left arm");
        await press(screen.getByTestId("saveNoteBtn"));
        await waitFor(() =>
            expect(useNoteDialogStore.getState().open).toBe(false)
        );
        expect(create).toHaveBeenCalledWith(
            "HDID1",
            expect.objectContaining({
                title: "Flu shot",
                text: "Left arm",
                hdId: "HDID1",
                version: 0,
            })
        );
        expect(useNoteStore.getState().notes.map((n) => n.id)).toEqual(["new"]);
    });

    it("Update Note: prefilled; an API error shows the edit error banner", async () => {
        updateStores(() => useNoteStore.setState({ notes: [existing] }));
        jest.spyOn(services.userNote, "updateNote").mockRejectedValue(
            new ResultError("E", "Version conflict", 409)
        );
        await render(<NoteDialog />);
        updateStores(() => useNoteDialogStore.getState().openDialog("n1"));
        await waitFor(() =>
            expect(screen.getByText("Update Note")).toBeTruthy()
        );
        await press(screen.getByTestId("saveNoteBtn"));
        await waitFor(() =>
            expect(textOf(screen.getByTestId("noteEditErrorText"))).toBe(
                "Version conflict"
            )
        );
        expect(services.userNote.updateNote).toHaveBeenCalledWith(
            "HDID1",
            expect.objectContaining({
                id: "n1",
                title: "Old",
                journalDate: "2026-09-15",
                version: 2,
            })
        );
    });

    it("a 429 shows the too-many message inside the dialog", async () => {
        jest.spyOn(services.userNote, "createNote").mockRejectedValue(
            new ResultError("E", "busy", 429)
        );
        await openNew();
        await changeText(screen.getByTestId("noteTitleInput"), "T");
        await press(screen.getByTestId("saveNoteBtn"));
        await waitFor(() =>
            expect(useErrorStore.getState().tooManyRequestsError).toBe(
                "noteDialog"
            )
        );
        expect(screen.getByTestId("too-many-requests-error")).toBeTruthy();
    });

    it("edits a note that came back without an id, rather than creating one", async () => {
        const noId = {
            hdId: "HDID1",
            title: "Old",
            text: "Text",
            journalDate: "2026-09-15",
            version: 2,
        };
        updateStores(() => useNoteStore.setState({ notes: [noId] }));
        await render(<NoteDialog />);
        updateStores(() =>
            useNoteDialogStore
                .getState()
                .openDialog(new NoteTimelineEntry(noId).id)
        );
        await waitFor(() =>
            expect(screen.getByText("Update Note")).toBeTruthy()
        );
    });
});
