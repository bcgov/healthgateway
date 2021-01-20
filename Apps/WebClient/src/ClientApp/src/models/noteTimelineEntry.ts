import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import TimelineFilter from "@/models/timelineFilter";
import UserNote from "@/models/userNote";

// The note timeline entry model
export default class NoteTimelineEntry extends TimelineEntry {
    public text: string;
    public textSummary: string;
    public title: string;
    public hdid: string;
    public version?: number;

    public constructor(model: UserNote) {
        super(
            model.id ?? "TEMP_ID",
            EntryType.Note,
            new DateWrapper(model.journalDateTime ?? "")
        );
        this.text = model.text || "";
        this.title = model.title || "No Title";
        this.textSummary = this.text.substring(0, 100);
        this.hdid = model.hdId || "";
        this.version = model.version;
    }

    public keywordApplies(filter: TimelineFilter): boolean {
        let text = (this.title || "") + (this.text || "");
        text = text.toUpperCase();
        return text.includes(filter.keyword.toUpperCase());
    }

    public toModel(): UserNote {
        return {
            id: this.id,
            hdId: this.hdid,
            title: this.title,
            text: this.text,
            journalDateTime: this.date.toISODate(),
            version: this.version || 0,
        };
    }
}
