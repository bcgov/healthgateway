export enum EntryType {
  Medication,
  Immunization,
  Laboratory,
  Note,
  NONE
}

// The base timeline entry model
export default abstract class TimelineEntry {
  public readonly id: string;
  public readonly type: EntryType;
  public readonly date?: Date;

  public constructor(id: string, type: EntryType, date?: Date) {
    this.id = id;
    this.type = type;
    this.date = date;
  }

  public abstract filterApplies(
    filterText: string,
    filterTypes: string[]
  ): boolean;
}
