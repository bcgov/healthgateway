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
    this.id = "id-" + Math.random();
    this.date = other.dispensedDate;
    this.title = other.brandName;
    this.description = other.genericName;
    this.type = EntryType.Medication;

    var pharmacyDescription: string = "";

    if (other.pharmacy) {
      pharmacyDescription = other.pharmacy.name ? other.pharmacy.name : "";
      pharmacyDescription += other.pharmacy.addressLine1 ? other.pharmacy.addressLine1 : "";
      pharmacyDescription += other.pharmacy.addressLine2 ? other.pharmacy.addressLine2 : "";
      pharmacyDescription += other.pharmacy.phoneNumber ? other.pharmacy.phoneNumber : "";
    }

    this.details = [
      new EntryDetail("Practitioner", other.practitionerSurname),
      new EntryDetail("Quantity", other.quantity),
      new EntryDetail("Filled At", pharmacyDescription)
    ];
  }
}
