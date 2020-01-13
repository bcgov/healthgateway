export enum EntryType {
  Medication,
  Laboratory,
  NONE
}

// The base timeline entry model
export default abstract class TimelineEntry {
  public readonly id?: string;
  public readonly date?: Date;
  public readonly type: EntryType;

  public constructor(id: string, date?: Date, type?: EntryType) {
    this.id = id;
    this.date = date;
    this.type = type ? type : EntryType.NONE;
  }

  public abstract filterApplies(filterText: string): boolean;
}
