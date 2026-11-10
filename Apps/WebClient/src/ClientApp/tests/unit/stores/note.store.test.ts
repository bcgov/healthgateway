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
import { useNoteStore } from "@/stores/note.store";
import { useTimelineStore } from "@/stores/timeline.store";
import {
    configFixture,
    fakeAuth,
    requestResult,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

const note = (id: string, title = "T") => ({
    id,
    hdId: "H1",
    title,
    text: "x",
    journalDate: "2026-10-01",
    version: 1,
});

describe("note store (stores/note.ts port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        resetStores();
        useTimelineStore.setState({ fullscreenEntry: undefined });
        jest.restoreAllMocks();
    });

    it("retrieves once, then creates, updates and deletes in place", async () => {
        const get = jest
            .spyOn(services.userNote, "getNotes")
            .mockResolvedValue(requestResult([note("a")]));
        jest.spyOn(services.userNote, "createNote").mockResolvedValue(
            note("b")
        );
        jest.spyOn(services.userNote, "updateNote").mockResolvedValue(
            note("a", "Edited")
        );
        jest.spyOn(services.userNote, "deleteNote").mockResolvedValue(
            undefined
        );
        await useNoteStore.getState().retrieveNotes("H1");
        await useNoteStore.getState().retrieveNotes("H1");
        expect(get).toHaveBeenCalledTimes(1);
        await useNoteStore.getState().createNote("H1", note("b"));
        await useNoteStore.getState().updateNote("H1", note("a", "Edited"));
        expect(useNoteStore.getState().notes.map((n) => n.title)).toEqual([
            "Edited",
            "T",
        ]);
        await useNoteStore.getState().deleteNote("H1", note("a"));
        expect(useNoteStore.getState().notes.map((n) => n.id)).toEqual(["b"]);
    });

    it("a change to the note shown full screen closes it", async () => {
        jest.spyOn(services.userNote, "getNotes").mockResolvedValue(
            requestResult([note("a")])
        );
        jest.spyOn(services.userNote, "deleteNote").mockResolvedValue(
            undefined
        );
        await useNoteStore.getState().retrieveNotes("H1");
        useTimelineStore
            .getState()
            .openFullscreenEntry(new NoteTimelineEntry(note("a")));
        await useNoteStore.getState().deleteNote("H1", note("a"));
        expect(useTimelineStore.getState().fullscreenEntry).toBeUndefined();
    });

    it("429s: retrieve warns the page, delete errors the page, create / update error the note dialog", async () => {
        const busy = new ResultError("ClientApp-CI-HGU", "busy", 429);
        jest.spyOn(services.userNote, "getNotes").mockRejectedValue(busy);
        jest.spyOn(services.userNote, "deleteNote").mockRejectedValue(busy);
        jest.spyOn(services.userNote, "createNote").mockRejectedValue(busy);
        await expect(
            useNoteStore.getState().retrieveNotes("H1")
        ).rejects.toThrow();
        expect(useErrorStore.getState().tooManyRequestsWarning).toBe("page");
        await expect(
            useNoteStore.getState().deleteNote("H1", note("a"))
        ).rejects.toThrow();
        expect(useErrorStore.getState().tooManyRequestsError).toBe("page");
        await expect(
            useNoteStore.getState().createNote("H1", note("a"))
        ).rejects.toThrow();
        expect(useErrorStore.getState().tooManyRequestsError).toBe(
            "noteDialog"
        );
    });
});
