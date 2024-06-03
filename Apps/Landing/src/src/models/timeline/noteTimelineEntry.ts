import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { UserComment } from "@/models/userComment";
import UserNote from "@/models/userNote";

// The note timeline entry model
export default class NoteTimelineEntry extends TimelineEntry {
    public text: string;
    public textSummary: string;
    public title: string;
    public hdid: string;
    public version?: number;
    private static maxSumaryLength = 40;

    public constructor(model: UserNote) {
        super(
            model.id ?? "TEMP_ID",
            EntryType.Note,
            model.journalDate
                ? DateWrapper.fromIsoDate(model.journalDate)
                : DateWrapper.today()
        );
        this.text = model.text ?? "";
        this.title = model.title || "No Title";
        this.textSummary = this.text.substring(
            0,
            NoteTimelineEntry.maxSumaryLength
        );
        if (this.text.length > NoteTimelineEntry.maxSumaryLength) {
            this.textSummary += "…";
        }

        this.hdid = model.hdId ?? "";
        this.version = model.version;
    }

    public get comments(): UserComment[] | null {
        return null;
    }

    public containsText(keyword: string): boolean {
        let text = (this.title ?? "") + (this.text ?? "");
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
            version: this.version ?? 0,
        };
    }
}
