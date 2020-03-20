import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import UserNote from "@/models/userNote";

// The note timeline entry model
export default class NoteTimelineEntry extends TimelineEntry {
  public text: string;
  public textSummary: string;
  public title: string;
  public hdid?: string;
  public version?: number;

  public constructor(model?: UserNote) {
    super(model?.id ?? "", EntryType.Note, model?.journalDateTime);
    this.text = model?.text || "";
    this.title = model?.title || "No Title";
    this.textSummary = this.text.substring(0, 100);
    this.hdid = model?.hdId;
    this.version = model?.version;
  }

  public filterApplies(filterText: string): boolean {
    var text = (this.title! || "") + (this.text! || "");
    text = text.toUpperCase();
    return text.includes(filterText.toUpperCase());
  }
}
