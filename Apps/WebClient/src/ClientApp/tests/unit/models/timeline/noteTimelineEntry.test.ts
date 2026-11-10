import { describe, expect, it } from "@jest/globals";

import { NoteTimelineEntry } from "@/models/timeline/noteTimelineEntry";

describe("NoteTimelineEntry (noteTimelineEntry.ts port)", () => {
    it("summarises past 40 characters with an ellipsis, defaults the title and maps back to the model", () => {
        const text = "a".repeat(45);
        const e = new NoteTimelineEntry({
            id: "n1",
            hdId: "H1",
            title: "",
            text,
            journalDate: "2026-10-01",
            version: 3,
        });
        expect(e.title).toBe("No Title");
        expect(e.textSummary).toBe(`${"a".repeat(40)}…`);
        expect(e.toModel()).toEqual({
            id: "n1",
            hdId: "H1",
            title: "No Title",
            text,
            journalDate: "2026-10-01",
            version: 3,
        });
    });

    it("a note without an id is TEMP_ID", () => {
        expect(
            new NoteTimelineEntry({
                hdId: "H1",
                title: "T",
                text: "",
                journalDate: "2026-10-01",
                version: 0,
            }).id
        ).toBe("TEMP_ID");
    });
});
