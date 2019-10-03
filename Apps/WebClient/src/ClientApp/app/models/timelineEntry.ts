import MedicationStatement from "@/models/medicationStatement";

export enum EntryType {
  Medication,
  Laboratory,
  NONE
}

// Timeline entry model
export default class TimelineEntry {
  public readonly date: Date;
  public readonly id: string;
  public readonly title: string;
  public readonly description: string;
  public readonly type: EntryType;

  public constructor(other: MedicationStatement) {
    // The ID needs to come from the the server
    this.id = "id-" + Math.random();
    this.date = other.dispensedDate;
    this.title = "TODO: - Brand name - " + other.brandName;
    this.description = other.genericName;
    this.type = EntryType.Medication;
  }
}
