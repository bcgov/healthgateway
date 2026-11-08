import { EntryType } from "@/constants/entryType";
import type { UserNote } from "@/models/userNote";
import { formatDate, fromIsoDate, today } from "@/utils/dateUtil";

import { TimelineEntry, type UserComment } from "./timelineEntry";

/** Port of models/timeline/noteTimelineEntry.ts */
export class NoteTimelineEntry extends TimelineEntry {
    private static readonly maxSumaryLength = 40;
    public text: string;
    public textSummary: string;
    public title: string;
    public hdid: string;
    public version?: number;

    public constructor(model: UserNote) {
        super(
            model.id ?? "TEMP_ID",
            EntryType.Note,
            model.journalDate ? fromIsoDate(model.journalDate) : today()
        );
        this.text = model.text ?? "";
        this.title = model.title || "No Title";
        this.textSummary = this.text.substring(
            0,
            NoteTimelineEntry.maxSumaryLength
        );
        if (this.text.length > NoteTimelineEntry.maxSumaryLength)
            this.textSummary += "…";
        this.hdid = model.hdId ?? "";
        this.version = model.version;
    }

    public get comments(): UserComment[] | null {
        return null;
    }

    public containsText(keyword: string): boolean {
        return ((this.title ?? "") + (this.text ?? ""))
            .toUpperCase()
            .includes(keyword.toUpperCase());
    }

    public toModel(): UserNote {
        return {
            id: this.id,
            hdId: this.hdid,
            title: this.title,
            text: this.text,
            journalDate: formatDate(this.date, "yyyy-MM-dd"),
            version: this.version ?? 0,
        };
    }
}
