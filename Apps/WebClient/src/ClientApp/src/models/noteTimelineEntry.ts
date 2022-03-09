import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";
import UserNote from "@/models/userNote";

// The note timeline entry model
export default class NoteTimelineEntry extends TimelineEntry {
    public text: string;
    public textSummary: string;
    public title: string;
    public hdid: string;
    public version?: number;
    private static maxSumaryLenght = 40;

    public constructor(model: UserNote) {
        super(
            model.id ?? "TEMP_ID",
            EntryType.Note,
            new DateWrapper(model.journalDate ?? "")
        );
        this.text = model.text || "";
        this.title = model.title || "No Title";
        this.textSummary = this.text.substring(
            0,
            NoteTimelineEntry.maxSumaryLenght
        );
        if (this.text.length > NoteTimelineEntry.maxSumaryLenght) {
            this.textSummary += "...";
        }

        this.hdid = model.hdId || "";
        this.version = model.version;
    }

    public get comments(): UserComment[] | null {
        return null;
    }

    public containsText(keyword: string): boolean {
        let text = (this.title || "") + (this.text || "");
        text = text.toUpperCase();
        return text.includes(keyword.toUpperCase());
    }

    public toModel(): UserNote {
        return {
            id: this.id,
            hdId: this.hdid,
            title: this.title,
            text: this.text,
            journalDate: this.date.toISODate(),
            version: this.version || 0,
        };
    }
}
