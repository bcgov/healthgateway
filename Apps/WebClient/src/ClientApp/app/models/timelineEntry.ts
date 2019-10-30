import MedicationStatement from "@/models/medicationStatement";

export enum EntryType {
  Medication,
  Laboratory,
  NONE
}

export class EntryDetail {
  public readonly name: string;
  public readonly value: string;

  public constructor(name: string, value: any) {
    this.name = name;
    this.value = value ? value.toString() : "N/A";
  }
}

// Timeline entry model
export default class TimelineEntry {
  public readonly date?: Date;
  public readonly id?: string;
  public readonly title?: string;
  public readonly description?: string;
  public readonly type: EntryType;
  public readonly details: EntryDetail[];

  public constructor(other: MedicationStatement) {
    // The ID needs to come from the the server
    this.type = EntryType.Medication;
    this.id = "id-" + Math.random();
    this.date = other.dispensedDate;
    this.title = other.medicationSumary.brandName;
    this.description = other.medicationSumary.genericName;

    this.details = [
      new EntryDetail("Practitioner", other.practitionerSurname),
      new EntryDetail("Prescription Number", other.prescriptionIdentifier),
      new EntryDetail("Quantity", other.medicationSumary.quantity),
      new EntryDetail("Strength", "TODO"),
      new EntryDetail("Form", other.medicationSumary.form),
      new EntryDetail("Manufacturer", other.medicationSumary.manufacturer),
      new EntryDetail("Filled At", "TODO")
    ];
  }
}
